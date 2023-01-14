RulesEngine is a highly extensible library to build rule based system using C# expressions


**Features**
- Json based rules definition
- Multiple input support
- Dynamic object input support
- C# Expression support
- Extending expression via custom class/type injection
- Scoped parameters
- Post rule execution actions

**Table Of Content**
- [Installation](#installation)
- [Basic Usage](#basic-usage)
  - [Create a workflow file with rules](#create-a-workflow-file-with-rules)
  - [Initialise RulesEngine with the workflow:](#initialise-rulesengine-with-the-workflow)
  - [Execute the workflow rules with input:](#execute-the-workflow-rules-with-input)
  - [Using custom names for inputs](#using-custom-names-for-inputs)
- [C# Expression support](#c-expression-support)
- [ScopedParams](#scopedparams)
  - [GlobalParams](#globalparams)
    - [Example](#example)
  - [LocalParams](#localparams)
    - [Example](#example-1)
  - [Referencing ScopedParams in other ScopedParams](#referencing-scopedparams-in-other-scopedparams)
- [Post rule execution actions](#post-rule-execution-actions)
  - [Inbuilt Actions](#inbuilt-actions)
    - [OutputExpression](#outputexpression)
      - [Usage](#usage)
    - [EvaluateRule](#evaluaterule)
      - [Usage](#usage-1)
  - [Custom Actions](#custom-actions)
    - [Steps to use a custom Action](#steps-to-use-a-custom-action)



## Installation
Nuget package: [![nuget](https://img.shields.io/nuget/dt/RulesEngine)](https://www.nuget.org/packages/RulesEngine/)

## Basic Usage
### Create a workflow file with rules
```json
[
  {
    "WorkflowName": "Discount",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000 AND input2.totalOrders > 2 AND input3.noOfVisitsPerMonth > 2"
      },
      {
        "RuleName": "GiveDiscount20",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor == 3 AND input1.totalPurchasesToDate >= 10000 AND input2.totalOrders > 2 AND input3.noOfVisitsPerMonth > 2"
      }
    ]
  }
] 
```

### Initialise RulesEngine with the workflow:
```c#
var workflowRules = //Get list of workflow rules declared in the json
var re = new RulesEngine.RulesEngine(workflowRules);
```

### Execute the workflow rules with input:
```c#
// Declare input1,input2,input3 
var resultList  = await re.ExecuteAllRulesAsync("Discount", input1,input2,input3);

//Check success for rule
foreach(var result in resultList){
  Console.WriteLine($"Rule - {result.Rule.RuleName}, IsSuccess - {result.IsSuccess}");
}
```
This will execute all the rules under `Discount` workflow and return ruleResultTree for all rules

**Note: input passed to rulesEngine can be of a concrete type, an anonymous type or dynamic(Expandobject). In case of dynamic object, RulesEngine will internally convert to an anonymous type**

### Using custom names for inputs
By Default, RulesEngine will name the inputs as input1, input2, input3... respectively.
It is possible to use a custom name in rules by passing input as `RuleParameter`
```json
[
  {
    "WorkflowName": "DiscountWithCustomInputNames",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "Expression": "basicInfo.country == \"india\" AND basicInfo.loyalityFactor <= 2 AND basicInfo.totalPurchasesToDate >= 5000 AND orderInfo.totalOrders > 2 AND telemetryInfo.noOfVisitsPerMonth > 2"
      },
      {
        "RuleName": "GiveDiscount20",
        "Expression": "basicInfo.country == \"india\" AND basicInfo.loyalityFactor == 3 AND basicInfo.totalPurchasesToDate >= 10000 AND orderInfo.totalOrders > 2 AND telemetryInfo.noOfVisitsPerMonth > 2"
      }
    ]
  }
] 

```
Now we can call rulesEngine with the custom names:
```c#
var workflowRules = //Get list of workflow rules declared in the json
var re = new RulesEngine.RulesEngine(workflowRules);


// Declare input1,input2,input3 

var rp1 = new RuleParameter("basicInfo",input1);
var rp2 = new RuleParameter("orderInfo", input2);
var rp3 = new RuleParameter("telemetryInfo",input3);

var resultList  = await re.ExecuteAllRulesAsync("DiscountWithCustomInputNames",rp1,rp2,rp3);

```

## C# Expression support
The lambda expression allows you to use most of C# constructs and along with some of linq features.

For more details on supported expression language refer - [expression language](https://dynamic-linq.net/expression-language)

For supported linq operations refer - [sequence operators](https://dynamic-linq.net/expression-language#sequence-operators)


## ScopedParams
Sometimes Rules can get very long and complex, scopedParams allow users to replace an expression in rule with an alias making it easier to maintain rule.

RulesEngine supports two type of ScopedParams:
- GlobalParams
- LocalParams


### GlobalParams
GlobalParams are defined at workflow level and can be used in any rule.

#### Example

```jsonc
//Rule.json
{
  "WorkflowName": "workflowWithGlobalParam",
  "GlobalParams":[
    {
      "Name":"myglobal1",
      "Expression":"myInput.hello.ToLower()"
    }
  ],
  "Rules":[
    {
      "RuleName": "checkGlobalEqualsHello",
      "Expression":"myglobal1 == \"hello\""
    },
    {
      "RuleName": "checkGlobalEqualsInputHello",
      "Expression":"myInput.hello.ToLower() == myglobal1"
    }
  ]
}
```

These rules when executed with the below input will return success
```c#
  var input = new RuleParameter("myInput",new {
    hello = "HELLO"
  });

  var resultList  = await re.ExecuteAllRulesAsync("workflowWithGlobalParam",rp);


```


### LocalParams
LocalParams are defined at rule level and can be used by the rule and its child rules

#### Example

```jsonc
//Rule.json
{
  "WorkflowName": "workflowWithLocalParam",
  
  "Rules":[
    {
      "RuleName": "checkLocalEqualsHello",
      "LocalParams":[
        {
          "Name":"mylocal1",
          "Expression":"myInput.hello.ToLower()"
        }
      ],
      "Expression":"mylocal1 == \"hello\""
    },
    {
      "RuleName": "checkLocalEqualsInputHelloInNested",
      "LocalParams":[
        {
          "Name":"mylocal1", //redefined here as it is scoped at rule level
          "Expression":"myInput.hello.ToLower()"
        }
      ],
      "Operator": "And",
      "Rules":[
        {
          "RuleName": "nestedRule",
          "Expression":"myInput.hello.ToLower() == mylocal1" //mylocal1 can be used here since it is nested to Rule where mylocal1 is defined
        }
      ]
      
    }
  ]
}
```

These rules when executed with the below input will return success
```c#
  var input = new RuleParameter("myInput",new {
    hello = "HELLO"
  });

  var resultList  = await re.ExecuteAllRulesAsync("workflowWithLocalParam",rp);
```

### Referencing ScopedParams in other ScopedParams

Similar to how ScopedParams can be used in expressions, they can also be used in other scoped params that come after them.
This allows us to create multi-step rule which is easier to read and maintain


```jsonc
//Rule.json
{
  "WorkflowName": "workflowWithReferencedRule",
  "GlobalParams":[
    {
      "Name":"myglobal1",
      "Expression":"myInput.hello"
    }
  ],
  "Rules":[
    {
      "RuleName": "checkGlobalAndLocalEqualsHello",
      "LocalParams":[
        {
          "Name": "mylocal1",
          "Expression": "myglobal1.ToLower()"
        }
      ],
      "Expression":"mylocal1 == \"hello\""
    },
    {
      "RuleName": "checklocalEqualsInputHello",
       "LocalParams":[
        {
          "Name": "mylocal1",
          "Expression": "myglobal1.ToLower()"
        },
        {
          "Name": "mylocal2",
          "Expression": "myInput.hello.ToLower() == mylocal1"
        }
      ],
      "Expression":"mylocal2 == true"
    }
  ]
}
```

These rules when executed with the below input will return success
```c#
  var input = new RuleParameter("myInput",new {
    hello = "HELLO"
  });

  var resultList  = await re.ExecuteAllRulesAsync("workflowWithReferencedRule",rp);


```

## Post rule execution actions
As a part of v3, Actions have been introduced to allow custom code execution on rule result. This can be achieved by calling `ExecuteAllRulesAsync` method of RulesEngine

### Inbuilt Actions
RulesEngine provides inbuilt action which cover major scenarios related to rule execution

#### OutputExpression
This action evaluates an expression based on the RuleParameters and returns its value as Output
##### Usage
Define OnSuccess or OnFailure Action for your Rule:
```jsonc
{
  "WorkflowName": "inputWorkflow",
  "Rules": [
    {
      "RuleName": "GiveDiscount10Percent",
      "SuccessEvent": "10",
      "ErrorMessage": "One or more adjust rules failed.",
      "ErrorType": "Error",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "input1.couy == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000 AND input2.totalOrders > 2 AND input2.noOfVisitsPerMonth > 2",
      "Actions": {
         "OnSuccess": {
            "Name": "OutputExpression",  //Name of action you want to call
            "Context": {  //This is passed to the action as action context
               "Expression": "input1.TotalBilled * 0.9"
            }
         }
      }
    }
  ]
}
```
Call `ExecuteAllRulesAsync` with the workflowName, ruleName and ruleParameters
```c#
   var ruleResultList = await rulesEngine.ExecuteAllRulesAsync("inputWorkflow",ruleParameters);
   foreach(var ruleResult in ruleResultList){
      if(ruleResult.ActionResult != null){
          Console.WriteLine(ruleResult.ActionResult.Output); //ActionResult.Output contains the evaluated value of the action
      }
   }
   
```

#### EvaluateRule
This action allows chaining of rules along with their actions. It also supports filtering inputs provided to chained rule as well as providing custom inputs

##### Usage
Define OnSuccess or OnFailure Action for your Rule:
```jsonc
{
  "WorkflowName": "inputWorkflow",
  "Rules": [
    {
        "RuleName": "GiveDiscount20Percent",
        "Expression": "input1.couy == \"india\" AND input1.loyalityFactor <= 5 AND input1.totalPurchasesToDate >= 20000",
        "Actions": {
           "OnSuccess": {
              "Name": "OutputExpression",  //Name of action you want to call
              "Context": {  //This is passed to the action as action context
                 "Expression": "input1.TotalBilled * 0.8"
              }
           },
           "OnFailure": { // This will execute if the Rule evaluates to failure
               "Name": "EvaluateRule",
               "Context": {
                   "WorkflowName": "inputWorkflow",
                   "ruleName": "GiveDiscount10Percent"
               }
           }
        }
    },
    {
      "RuleName": "GiveDiscount10Percent",
      "SuccessEvent": "10",
      "ErrorMessage": "One or more adjust rules failed.",
      "ErrorType": "Error",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "input1.couy == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000 AND input2.totalOrders > 2 AND input2.noOfVisitsPerMonth > 2",
      "Actions": {
         "OnSuccess": {
            "Name": "OutputExpression",  //Name of action you want to call
            "Context": {  //This is passed to the action as action context
               "Expression": "input1.TotalBilled * 0.9"
            }
         }
      }
    }
  ]
}
```
Call `ExecuteActionWorkflowAsync` with the workflowName, ruleName and ruleParameters
```c#
   var result = await rulesEngine.ExecuteActionWorkflowAsync("inputWorkflow","GiveDiscount20Percent",ruleParameters);
   Console.WriteLine(result.Output); //result.Output contains the evaluated value of the action
```

In the above scenario if `GiveDiscount20Percent` succeeds, it will return 20 percent discount in output. If it fails, `EvaluateRule` action will call `GiveDiscount10Percent` internally and if it succeeds, it will return 10 percent discount in output.

EvaluateRule also supports passing filtered inputs and computed inputs to chained rule
```jsonc
 "Actions": {
         "OnSuccess": {
            "Name": "EvaluateRule",
               "Context": {
                   "WorkflowName": "inputWorkflow",
                   "ruleName": "GiveDiscount10Percent",
                   "inputFilter": ["input2"], //will only pass input2 from existing inputs,scopedparams to the chained rule
                   "additionalInputs":[ // will pass a new input named currentDiscount with the result of the expression to the chained rule
                     {
                       "Name": "currentDiscount",
                       "Expression": "input1.TotalBilled * 0.9"
                     }
                   ]
               }
         }
      }

```


### Custom Actions
RulesEngine allows registering custom actions which can be used in the rules workflow.

#### Steps to use a custom Action
1. Create a class which extends `ActionBase` class and implement the run method
```c#
 public class MyCustomAction: ActionBase
    {
     
        public MyCustomAction(SomeInput someInput)
        {
            ....
        }

        public override ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var customInput = context.GetContext<string>("customContextInput");
            //Add your custom logic here and return a ValueTask
        }
```
Actions can have async code as well
```c#
 public class MyCustomAction: ActionBase
    {
     
        public MyCustomAction(SomeInput someInput)
        {
            ....
        }

        public override async ValueTask<object> Run(ActionContext context, RuleParameter[] ruleParameters)
        {
            var customInput = context.GetContext<string>("customContextInput");
            //Add your custom logic here
            return await MyCustomLogicAsync();
        }
```
2. Register them in ReSettings and pass it to RulesEngine
```c#
   var reSettings = new ReSettings{
                        CustomActions = new Dictionary<string, Func<ActionBase>>{
                                             {"MyCustomAction", () => new MyCustomAction(someInput) }
                                         }
                     };

   var re = new RulesEngine(workflowRules,reSettings);
```
3. You can now use the name you registered in the Rules json in success or failure actions
```jsonc
{
  "WorkflowName": "inputWorkflow",
  "Rules": [
    {
      "RuleName": "GiveDiscount10Percent",
      "SuccessEvent": "10",
      "ErrorMessage": "One or more adjust rules failed.",
      "ErrorType": "Error",
      "RuleExpressionType": "LambdaExpression",
      "Expression": "input1.couy == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000 AND input2.totalOrders > 2 AND input2.noOfVisitsPerMonth > 2",
      "Actions": {
         "OnSuccess": {
            "Name": "MyCustomAction",  //Name context
            "Context": {  //This is passed to the action as action context
               "customContextInput": "input1.TotalBilled * 0.9"
            }
         }
      }
    }
  ]
}
```


_For more details please check out [Rules Engine Wiki](https://github.com/microsoft/RulesEngine/wiki)._
