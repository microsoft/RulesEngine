## Overview

[forked from](https://github.com/microsoft/RulesEngine) which does not appear to be maintained anymore

Rules Engine is a library (not yet NuGet package) for abstracting business logic/rules/policies out of a system. It provides a simple way of giving you the ability to put your rules in a store outside the core logic of the system, thus ensuring that any change in rules don't affect the core system.

## How to use it

### [Basic Example](https://github.com/asulwer/RulesEngine/blob/v6.0.2/DemoApp/Demos/Basic.cs)

### 1. Create the Workflow and add a Rule to it

```
var workflows = new Workflow[] {
    new Workflow {
        WorkflowName = "Test Workflow Rule 1",
        Rules = [
            new Rule {
                RuleName = "Test Rule",
                SuccessMessage = "Count is within tolerance",
                ErrorMessage = "Over expected",
                Expression = "count < 3"
            }
        ]
    }
};
```

### 2. Create instance of RulesEngine, parameter Workflows (optional ReSettings)

```
var rulesEngine = new RulesEngine.RulesEngine(workflows);
```

### 3. Create RuleParameters to pass as parameter to instance of RulesEngine

```
var ruleParameters = new RuleParameter[] {
    new RuleParameter("input1", new { count = 1 })
};
```

### 4. Excute all Workflows and associated Rules (parameters RuleParameters and CancellationToken)

```
await foreach (var async_rrt in rulesEngine.ExecuteAllWorkflows(ruleParameters, ct))
{
    async_rrt.OnSuccess((eventName) => {
        
    });

    async_rrt.OnFail(() => {
        
    });
}
```

[Additional Examples](https://github.com/asulwer/RulesEngine/tree/main/DemoApp)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution.
