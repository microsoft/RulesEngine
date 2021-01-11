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
        "SuccessEvent": "10",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor <= 2 AND input1.totalPurchasesToDate >= 5000 AND input2.totalOrders > 2 AND input3.noOfVisitsPerMonth > 2"
      },
      {
        "RuleName": "GiveDiscount20",
        "SuccessEvent": "20",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country == \"india\" AND input1.loyalityFactor == 3 AND input1.totalPurchasesToDate >= 10000 AND input2.totalOrders > 2 AND input3.noOfVisitsPerMonth > 2"
      }
    ]
  }
] 
```

### Initialise RulesEngine with the workflow:
```c#
var workflowRules = //Get list of workflow rules declared in the json
var re = new RulesEngine.RulesEngine(workflowRules, null);
```

### Execute the workflow rules with input:
```c#
// Declare input1,input2,input3 
var resultList  = await re.ExecuteAllRulesAsync("Discount", input1,input2,input3);
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
        "SuccessEvent": "10",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "basicInfo.country == \"india\" AND basicInfo.loyalityFactor <= 2 AND basicInfo.totalPurchasesToDate >= 5000 AND orderInfo.totalOrders > 2 AND telemetryInfo.noOfVisitsPerMonth > 2"
      },
      {
        "RuleName": "GiveDiscount20",
        "SuccessEvent": "20",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "basicInfo.country == \"india\" AND basicInfo.loyalityFactor == 3 AND basicInfo.totalPurchasesToDate >= 10000 AND orderInfo.totalOrders > 2 AND telemetryInfo.noOfVisitsPerMonth > 2"
      }
    ]
  }
] 

```
Now we can call rulesEngine with the custom names:
```c#
var workflowRules = //Get list of workflow rules declared in the json
var re = new RulesEngine.RulesEngine(workflowRules, null);


// Declare input1,input2,input3 

var rp1 = new RuleParameter("basicInfo",input1);
var rp2 = new RuleParameter("orderInfo", input2);
var rp3 = new RuleParameter("telemetryInfo",input3);

var resultList  = await re.ExecuteAllRulesAsync("DiscountWithCustomInputNames",rp1,rp2,rp3);

```
