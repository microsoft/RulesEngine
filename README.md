# Rules Engine
![build](https://github.com/microsoft/RulesEngine/workflows/build/badge.svg?branch=main)
[![Coverage Status](https://coveralls.io/repos/github/microsoft/RulesEngine/badge.svg?branch=main)](https://coveralls.io/github/microsoft/RulesEngine?branch=main)
[![Nuget download][download-image]][download-url]

[download-image]: https://img.shields.io/nuget/dt/RulesEngine
[download-url]: https://www.nuget.org/packages/RulesEngine/
## Overview
Rules Engine is a library/NuGet package for abstracting business logic/rules/policies out of the system. This works in a very simple way by giving you an ability to put your rules in a store outside the core logic of the system thus ensuring that any change in rules doesn't affect the core system.

## Installation
To install this library, please download the latest version of  [NuGet Package](https://www.nuget.org/packages/RulesEngine/) from [nuget.org](https://www.nuget.org/) and refer it into your project.  

## How to use it
There are several ways to populate workflows for the Rules Engine as listed below.

You need to store the rules based on the [schema definition](https://github.com/microsoft/RulesEngine/blob/main/schema/workflowRules-schema.json) given and they can be stored in any store as deemed appropriate like Azure Blob Storage, Cosmos DB, Azure App Configuration, [Entity Framework](https://github.com/microsoft/RulesEngine#entity-framework), SQL Servers, file systems etc. The expressions are supposed to be a [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions).

An example rule could be - 
```json
[
  {
    "WorkflowName": "Discount",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "SuccessEvent": "10",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000"
      },
      {
        "RuleName": "GiveDiscount20",
        "SuccessEvent": "20",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor >= 3 AND input1.totalPurchasesToDate >= 10000"
      }
    ]
  }
]
```

You can inject the rules into the Rules Engine by initiating an instance by using the following code - 
```c#
var rulesEngine = new RulesEngine(workflowRules, logger);
```
Here, *workflowRules* is a list of deserialized object based out of the schema explained above and *logger* is a custom logger instance made out of an [ILogger](https://github.com/microsoft/RulesEngine/wiki/Getting-Started#logger) instance.

Once done, the Rules Engine needs to execute the rules for a given input. It can be done by calling the method ExecuteAllRulesAsync as shown below - 
```c#
List<RuleResultTree> response = await rulesEngine.ExecuteAllRulesAsync(workflowName, input);
```
Here, *workflowName* is the name of the workflow, which is *Discount* in the above mentioned example. And *input* is the object which needs to be checked against the rules,  which itself may consist of a list of class instances.

The *response* will contain a list of [*RuleResultTree*](https://github.com/microsoft/RulesEngine/wiki/Getting-Started#ruleresulttree) which gives information if a particular rule passed or failed. 


_Note: A detailed example showcasing how to use Rules Engine is explained in [Getting Started page](https://github.com/microsoft/RulesEngine/wiki/Getting-Started) of [Rules Engine Wiki](https://github.com/microsoft/RulesEngine/wiki)._

_A demo app for the is available at [this location](https://github.com/microsoft/RulesEngine/tree/main/demo)._

### Basic
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

workflowRule.Rules = rules;

workFlowRules.Add(workflowRule);

var bre = new RulesEngine.RulesEngine(workFlowRules.ToArray(), null);
```


### Entity Framework
Consuming Entity Framework and populating the Rules Engine is shown in the [EFDemo class](https://github.com/microsoft/RulesEngine/blob/main/demo/DemoApp/EFDemo.cs) with Workflow rules populating the array and passed to the Rules Engine, The Demo App includes an example [RulesEngineDemoContext](https://github.com/microsoft/RulesEngine/blob/main/demo/DemoApp.EFDataExample/RulesEngineDemoContext.cs) using SQLite and could be swapped out for another provider.
```c#
var wfr = db.WorkflowRules.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();
var bre = new RulesEngine.RulesEngine(wfr, null);
```
*Note: For each level of nested rules expected, a ThenInclude query appended will be needed as shown above.*

## How it works

![](https://github.com/microsoft/RulesEngine/blob/main/assets/BlockDiagram.png)

The rules can be stored in any store and be fed to the system in a structure which follows a proper [schema](https://github.com/microsoft/RulesEngine/blob/main/schema/workflowRules-schema.json) of WorkFlow model.

The wrapper needs to be created over the Rules Engine package, which will get the rules and input message(s) from any store that your system dictates and put it into the Engine. Also, the wrapper then needs to handle the output using appropriate means.


_Note: To know in detail of the workings of Rules Engine, please visit [How it works section](https://github.com/microsoft/RulesEngine/wiki/Introduction#how-it-works) in [Rules Engine Wiki](https://github.com/microsoft/RulesEngine/wiki)._

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.




---

_For more details please check out [Rules Engine Wiki](https://github.com/microsoft/RulesEngine/wiki)._
