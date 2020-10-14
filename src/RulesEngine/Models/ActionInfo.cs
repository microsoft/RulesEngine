using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Models
{
    public class ActionInfo
    {
        public string Name { get; set; }
        public Dictionary<string, object> Context { get; set; }
    }
}
