using System;
using System.Collections.Generic;
using System.Text;

namespace RulesEngine.Models
{
    public delegate T RuleFunc<T>(params object[] param);
   
}
