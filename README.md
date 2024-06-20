## Overview

[forked from](https://github.com/microsoft/RulesEngine) which does not appear to be maintained anymore

Rules Engine is a library (not yet NuGet package) for abstracting business logic/rules/policies out of a system. It provides a simple way of giving you the ability to put your rules in a store outside the core logic of the system, thus ensuring that any change in rules don't affect the core system.

## How to use it

There are several ways to populate workflows for the Rules Engine as listed below.

You need to store the rules based on the [schema definition](https://github.com/asulwer/RulesEngine/blob/main/schema/workflow-schema.json) given and they can be stored in any store as deemed appropriate. For RuleExpressionType `LambdaExpression`, the rule is written as a [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions).

An example rule:

```json
[
  {
    "WorkflowName": "Discount",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "SuccessMessage": "10",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyaltyFactor <= 2 AND input1.totalPurchasesToDate >= 5000"
      },
      {
        "RuleName": "GiveDiscount20",
        "SuccessMessage": "20",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyaltyFactor >= 3 AND input1.totalPurchasesToDate >= 10000"
      }
    ]
  }
]
```

You can inject the rules into the Rules Engine by initiating an instance by using the following code - 

```c#
var rulesEngine = new RulesEngine(workflow);
```
Here, *workflow* is a list of deserialized objects based on the schema explained above
Once initialised, the Rules Engine needs to execute the rules for a given input. This can be done by calling the method `ExecuteAllRulesAsync`: 

```c#
List<RuleResultTree> response = await rulesEngine.ExecuteAllRulesAsync(workflowName, input);
```

Here, *workflowName* is the name of the workflow, which is *Discount* in the above mentioned example. And *input* is the object which needs to be checked against the rules,  which itself may consist of a list of class instances.

The *response* will contain a list of [*RuleResultTree*](https://github.com/asulwer/RulesEngine/blob/main/src/RulesEngine/Models/RuleResultTree.cs) which gives information if a particular rule passed or failed. 

_A demo app for the is available at [this location](https://github.com/asulwer/RulesEngine/tree/main/demo)._

<details>

<summary>Basic</summary>

A simple example via code only is as follows:

```c#
List<Rule> rules = new List<Rule>();

Rule rule = new Rule();
rule.RuleName = "Test Rule";
rule.SuccessEvent = "Count is within tolerance.";
rule.ErrorMessage = "Over expected.";
rule.Expression = "count < 3";
rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
rules.Add(rule);

var workflows = new List<Workflow>();

Workflow exampleWorkflow = new Workflow();
exampleWorkflow.WorkflowName = "Example Workflow";
exampleWorkflow.Rules = rules;

workflows.Add(exampleWorkflow);

var bre = new RulesEngine.RulesEngine(workflows.ToArray());
```

[Additional Examples](https://github.com/asulwer/RulesEngine/tree/main/demo/DemoApp/Demos)

</details>

<details>

<summary>Entity Framework</summary>

Consuming Entity Framework and populating the Rules Engine is shown in the [EFDemo class](https://github.com/asulwer/RulesEngine/blob/main/demo/DemoApp/Demos/EF.cs) with Workflow rules populating the array and passed to the Rules Engine, The Demo App includes an example [RulesEngineDemoContext](https://github.com/asulwer/RulesEngine/blob/main/demo/DemoApp/Demos/RulesEngineContext.cs) using SQLite and could be swapped out for another provider.

```c#
var wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();
var bre = new RulesEngine.RulesEngine(wfr, null);
```

*Note: For each level of nested rules expected, a ThenInclude query appended will be needed as shown above.*
</details>

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution.
