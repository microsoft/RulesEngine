// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RulesEngine.Actions
{
    public class ActionContext
    {
        private readonly IDictionary<string, string> _context;
        private readonly CancellationToken _token = CancellationToken.None;
        private readonly RuleResultTree _parentResult;

        public ActionContext(IDictionary<string, object> context, RuleResultTree parentResult, CancellationToken cancellationToken) : this(context, parentResult)
        {
           _token = cancellationToken;
        }

        public ActionContext(IDictionary<string, object> context, RuleResultTree parentResult)
        {
            _context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in context)
            {
                string key = kv.Key;
                string value;
                switch (kv.Value.GetType().Name)
                {
                    case "String":
                    case "JsonElement":
                        value = kv.Value.ToString();
                        break;
                    default:
                        value = JsonConvert.SerializeObject(kv.Value);
                        break;

                }
                _context.Add(key, value);
            }
            _parentResult = parentResult;
        }


        public RuleResultTree GetParentRuleResult()
        {
            return _parentResult;
        }

        public bool TryGetContext<T>(string name,out T output)
        {
            try
            {
                output =  GetContext<T>(name);
                return true;
            }
            catch(ArgumentException)
            {
                output = default(T);
                return false;
            }
        }

        public T GetContext<T>(string name)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(_context[name], typeof(T));
                }
                return JsonConvert.DeserializeObject<T>(_context[name]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"Argument `{name}` was not found in the action context");
            }
            catch (JsonException)
            {
                throw new ArgumentException($"Failed to convert argument `{name}` to type `{typeof(T).Name}` in the action context");
            }
        }

        public CancellationToken GetCancellationToken()
        {
            return _token;
        }
    }
}
