using Newtonsoft.Json;
using System.Collections.Generic;

namespace RulesEngine.Actions
{
    public class ActionContext
    {
        private readonly IDictionary<string, string> _context;
        public ActionContext(IDictionary<string, object> context)
        {
            _context = new Dictionary<string, string>();
            foreach (var kv in context)
            {
                string key = kv.Key.ToLower();
                string value = kv.Value is string ? kv.Value.ToString() : JsonConvert.SerializeObject(kv.Value);
                _context.Add(key, value);
            }

        }
        public T GetContext<T>(string name) where T : class
        {
            name = name.ToLower();
            if (typeof(T) == typeof(string))
            {
                return _context[name] as T;
            }
            return JsonConvert.DeserializeObject<T>(_context[name]);
        }
    }
}
