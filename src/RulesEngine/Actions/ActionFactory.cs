using System;
using System.Collections.Generic;

namespace RulesEngine.Actions
{
    internal class ActionFactory
    {
        private readonly IDictionary<string, Func<ActionBase>> _actionRegistry;

        internal ActionFactory()
        {
            _actionRegistry = new Dictionary<string, Func<ActionBase>>();

        }
        internal ActionFactory(IDictionary<string,Func<ActionBase>> actionRegistry)
        {
            _actionRegistry = actionRegistry;
        }

        internal ActionBase Get(string name)
        {
            if (_actionRegistry.ContainsKey(name))
            {
                return _actionRegistry[name]();
            }
            throw new KeyNotFoundException($"Action with name:{name} does not exist");
        }
    }
}
