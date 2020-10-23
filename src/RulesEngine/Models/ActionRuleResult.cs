using System.Collections.Generic;
using RulesEngine.Models;


public class ActionRuleResult : ActionResult{
    public List<RuleResultTree> Results {get; set;} 
}