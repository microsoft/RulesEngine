using RulesEngine.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RulesEngine
{
    /// <summary>Maintains the cache of evaludated param.</summary>
    internal class ParamCache<T> where T : class
    {
        /// <summary>
        /// The compile rules
        /// </summary>
        private readonly ConcurrentDictionary<string, T> _evaluatedParams = new ConcurrentDictionary<string, T>();

        /// <summary>
        ///   <para></para>
        ///   <para>Determines whether the specified parameter key name contains parameters.
        /// </para>
        /// </summary>
        /// <param name="paramKeyName">Name of the parameter key.</param>
        /// <returns>
        ///   <c>true</c> if the specified parameter key name contains parameters; otherwise, <c>false</c>.</returns>
        public bool ContainsParams(string paramKeyName)
        {
            return _evaluatedParams.ContainsKey(paramKeyName);
        }

        /// <summary>Adds the or update evaluated parameter.</summary>
        /// <param name="paramKeyName">Name of the parameter key.</param>
        /// <param name="ruleParameters">The rule parameters.</param>
        public void AddOrUpdateParams(string paramKeyName, T ruleParameters)
        {
            _evaluatedParams.AddOrUpdate(paramKeyName, ruleParameters, (k, v) => v);
        }

        /// <summary>Clears this instance.</summary>
        public void Clear()
        {
            _evaluatedParams.Clear();
        }

        /// <summary>Gets the evaluated parameters.</summary>
        /// <param name="paramKeyName">Name of the parameter key.</param>
        /// <returns>Delegate[].</returns>
        public T GetParams(string paramKeyName)
        {
            return _evaluatedParams[paramKeyName];
        }

        /// <summary>Removes the specified workflow name.</summary>
        /// <param name="workflowName">Name of the workflow.</param>
        public void RemoveCompiledParams(string paramKeyName)
        {
            if (_evaluatedParams.TryRemove(paramKeyName, out T ruleParameters))
            {
                var compiledKeysToRemove = _evaluatedParams.Keys.Where(key => key.StartsWith(paramKeyName));
                foreach (var key in compiledKeysToRemove)
                {
                    _evaluatedParams.TryRemove(key, out T val);
                }
            }
        }
    }
}
