using System;
using System.Collections.Generic;

namespace RulesEngine.Actions
{
    public class ActionFactory
    {
        private readonly IDictionary<string, Func<ActionBase>> _actionRegistry;

        public ActionFactory()
        {
            _actionRegistry = new Dictionary<string, Func<ActionBase>>();

        }
        public ActionFactory(IDictionary<string,Func<ActionBase>> actionRegistry)
        {
            _actionRegistry = actionRegistry;
        }

        public ActionBase Get(string name)
        {
            if (_actionRegistry.ContainsKey(name))
            {
                return _actionRegistry[name]();
            }
            throw new KeyNotFoundException($"Action with name:{name} does not exist");
        }
    }
}
