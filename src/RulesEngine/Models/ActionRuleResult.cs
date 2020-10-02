using System;
using System.Collections.Generic;
using RulesEngine.Models;

public class ActionRuleResult{
    public object Output {get; set;}

    public Exception Exception { get; set; }
    public List<RuleResultTree> Results {get; set;} 
}