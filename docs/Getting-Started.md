## Getting Started with Rules Engine
RulesEngine is a library/NuGet package for abstracting rules and running the Rules Engine.

### Publicly accessible interfaces, models, methods and schemas
As with any library/package there are public interfaces with which we interact with that library/packages. There are a few public interfaces in this package as well. The interface which will be used to access this package is [IRulesEngine](#irulesengine), with four overloaded methods for executing rules. To understand the methods, we need to go through some of the models/schemas first. 

#### Rules
The rules used in this system is mostly comprising of [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions). Anything that can be defined in a lambda expression can be used as a rule in this library.

#### Rules Schema
Rules schema is available in the [schema file](https://github.com/microsoft/RulesEngine/blob/main/schema/workflow-schema.json). The workflow rules are how we store the rules in the system. In our system, the name of the model typed in the library is [Workflow](https://github.com/microsoft/RulesEngine/blob/main/src/RulesEngine/Models/Workflow.cs). An example json would be – 

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
      },
      {
        "RuleName": "GiveDiscount25",
        "SuccessEvent": "25",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.country != \"india\" AND input1.loyalityFactor >= 2 AND input1.totalPurchasesToDate >= 10000 AND input2.totalOrders > 2 AND input3.noOfVisitsPerMonth > 5"
      },
      {
        "RuleName": "GiveDiscount30",
        "SuccessEvent": "30",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.loyalityFactor > 3 AND input1.totalPurchasesToDate >= 50000 AND input1.totalPurchasesToDate <= 100000 AND input2.totalOrders > 5 AND input3.noOfVisitsPerMonth > 15"
      },
      {
        "RuleName": "GiveDiscount35",
        "SuccessEvent": "35",
        "ErrorMessage": "One or more adjust rules failed.",
        "ErrorType": "Error",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "input1.loyalityFactor > 3 AND input1.totalPurchasesToDate >= 100000 AND input2.totalOrders > 15 AND input3.noOfVisitsPerMonth > 25"
      }
    ]
  }
]
```

This workflow rules showcased in the above json is of a sample [Use Case](https://github.com/microsoft/RulesEngine/wiki/Use-Case) which is going to be used to explain the library. 
Demo App for the given use case is available at [this location](https://github.com/microsoft/RulesEngine/tree/main/demo).
#### Logger
Another public interface for custom logging is ILogger. This interface is not implemented and looks for custom implementation of the user who wants to use it. The methods available for this interface are – 
```c# 
void LogTrace(string msg);
void LogError(Exception ex);
```
These methods can be implemented in any logging mechanism that is expected and its instance can be injected into the Rules Engine as shown in [Initiating the Rules Engine](#initiating-the-rules-engine).
#### ReSettings
This model is a list of custom types. 
While, lambda expressions are quite powerful, there is a limit to what they can do because of the fact that the methods that a lambda expression can do are limited to [System namespace](https://docs.microsoft.com/en-us/dotnet/api/system) of [.Net framework](https://docs.microsoft.com/en-us/dotnet/framework/). 

To use more complex and custom classes and have some logics which are way too complex to be written into a lambda expression, these settings come into picture. If the user wants to create a custom method to run for the rule to be used, these settings help them. 

Example – 

You can create a public class called Utils and include a method in it which check contains in a list.
```c#
using System;
using System.Linq;

namespace RE.HelperFunctions
{
    public static class Utils
    {
        public static bool CheckContains(string check, string valList)
        {
            if (String.IsNullOrEmpty(check) || String.IsNullOrEmpty(valList))
                return false;

            var list = valList.Split(',').ToList();
            return list.Contains(check);
        }
    }
}
```
And this can be then used in lambda expression in a very simple manner like this – 
```json
"Expression": "Utils.CheckContains(input1.country, \"india,usa,canada,France\") == true"
```

To use the custom class when evaluating the rules:

1. Register the class
2. Then pass settings through rules engine
```csharp
var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(Utils) } };
new RulesEngine.RulesEngine(workflowRules.ToArray(), null, reSettingsWithCustomTypes);
```

#### RuleParameter
This is a model class for custom inputs which can be seen in the [RuleParameter Class](https://github.com/microsoft/RulesEngine/blob/main/src/RulesEngine/Models/RuleParameter.cs). This type is present to add another layer of customization to the rules. 

For example, the rules present in the example mentioned in the [Rules Schema](#rules-schema) section are using 3 different inputs for each run. The inputs are of different types as mentioned in the [Use Case]((https://github.com/microsoft/RulesEngine/wiki/Use-Case)) and is coming from different sources. Now, in rules we had to use input1, input2 and input3 to target data coming from the basic info, order info and telemetry info, respectively. 


With RuleParameter class, we can give context specific names to the rules in the list of rules instead of input1, input2 and input3. 

#### LocalParams

Rules Engine has a param (like ‘var’ in c#) feature support now, it makes authoring and troubleshooting of issues very easy. Now you can breakdown your bigger statements into smaller logical expressions as parameters within a rule definition.

Below is an example of a complex rule which can be authored easily using logical intermediate parameters and can be used to write the final rule expression to return a binary value. Sample rule requirement here is to provide access to a user only when user has completed some mandatory trainings or the user is accessing the site it from a secure domain. 

```
{
        "name": "allow_access_if_all_mandatory_trainings_are_done_or_access_isSecure",
        "errorMessage": "Please complete all your training(s) to get access to this content or access it from a secure domain/location.",
        "errorType": "Error",
        "localParams": [
          {
            "name": "completedSecurityTrainings",
            "expression": "MasterSecurityComplainceTrainings.Where(Status.Equals(\"Completed\", StringComparison.InvariantCultureIgnoreCase))"
          },
          {
            "name": "completedProjectTrainings",
            "expression": "MasterProjectComplainceTrainings.Where(Status.Equals(\"Completed\", StringComparison.InvariantCultureIgnoreCase))"
          },
          {
            "name": "isRequestAccessSecured",
            "expression": "UserRequestDetails.Location.Country == \"India\" ? ((UserRequestDetails.Location.City == \"Bangalore\" && UserRequestDetails.Domain=\"xxxx\")? true : false):false"
          }
        ],
        "expression": "(completedSecurityTrainings.Any() && completedProjectTrainings.Any()) || isRequestAccessSecured "
      }
 ```


#### RuleResultTree
[This model](https://github.com/microsoft/RulesEngine/blob/main/src/RulesEngine/Models/RuleResultTree.cs) is the output of the Rules Engine. Once the execution of the Rules Engine is completed and the Engine has gone through all the rules, a list of this type is returned. What this model include is – 
##### Rule
This is the rule that is currently being referred. It is of a custom model type and has information of that rule which ran on the input. 
##### IsSuccess
This is a Boolean value showcasing whether the given rule passed or not.
##### ChildResults
In the case, the rule has child rules, this variable gets initialized else it is null. This is a nested list of RuleResultTree type to showcase the response of the children rules.
##### Input
This is the input that was being checked upon while the rules were being verified on this object. In case of multiple inputs, it takes up the first input.


#### IRulesEngine
IRulesEngine is the main interface which is used to handle all the executions. This interface has four overloaded methods to execute rules – 
```c#
List<RuleResultTree> ExecuteRule(string workflowName, IEnumerable<dynamic> input, object[] otherInputs);
List<RuleResultTree> ExecuteRule(string workflowName, object[] inputs);
List<RuleResultTree> ExecuteRule(string workflowName, object input);
List<RuleResultTree> ExecuteRule(string workflowName, RuleParameter[] ruleParams);
```
One Rules Engine can take in multiple workflows and the workflows can be distributed based on the logic dictated by the system you are building.


In the first definition – 
* workflowName is the name of the workflow you want to take up.
* input is a list of dynamic inputs which is being entered. 
* otherInputs is an array of other auxiliary inputs which complement the inputs based on the rules present.


In the second definition – 
* workflowName is the name of the workflow you want to take up.
* input is an array of dynamic inputs which is being entered. 


In the third definition – 
* workflowName is the name of the workflow you want to take up.
* input is a single dynamic input which is being entered.


In the fourth definition – 
* workflowName is the name of the workflow you want to take up.
* ruleParams is an array of RuleParameters as explained in [RuleParameter](#ruleparameter). 

#### Initiating the Rules Engine
To initiate the Rules Engine instance to be used for executing rules, the workflow rules need to be injected into the library. The two different definitions of constructors are – 
```c#
public RulesEngine(string[] jsonConfig, ILogger logger, ReSettings reSettings = null) 
public RulesEngine(WorkflowRules[] workflowRules, ILogger logger, ReSettings reSettings = null)
```
Here, 
* jsonConfig is the list of serialized json strings following the schema mentioned in [Rules Schema](#rules-schema).
* logger is an instance of the logger created by you, following the information given in [Logger](#logger).
* reSettings is list of custom types as mention in [ReSettings](#resettings).
* workflowRules is a list of objects of type WorkflowRules which is mentioned in the [Rules Schema](#rules-schema).

#### Success/Failure
For the rules to make sense, there are always success and failure scenarios. This library gives the user an inbuilt scenario where in success and failure scenario an event can be created.
##### Success
In case of success, there could be one or more than one rules which passed based on the given input(s). The success event will be triggered and will be run based on the first rule which was true and give you the success event which was initialized as SuccessEvent in the RulesSchema section.


Example – 
```c#
List<RuleResultTree> resultList = bre.ExecuteRule("Discount", inputs);

resultList.OnSuccess((eventName) =>
{
discountOffered = $"Discount offered is {eventName} % over MRP.";
});
```
##### Failure
In case, none of the rules succeeded the failure event gets triggered. 

Example – 
```c#
List<RuleResultTree> resultList = bre.ExecuteRule("Discount", inputs);
resultList.OnFail(() =>
{
discountOffered = "The user is not eligible for any discount.";
});
```

### How to use Rules Engine
1.	To install this library, please download the latest version of  [NuGet Package](https://www.nuget.org/packages/RulesEngine/) from [nuget.org](https://www.nuget.org/) and refer it into your project. 
2.	Initiate the instance of Rules Engine as mentioned in [Initiating the Rules Engine](#initiating-the-rules-engine).
3.	Once done, the rules can be executed using any of the overloaded methods as explained in [IRulesEngine](#irulesengine) section. It returns the list of [RuleResultTree](#ruleresulttree) which can be used in any way the user wants to. 
4.	The success or failure events can be defined as explained in the [Success/Failure](#successfailure) section. 
    * Based on the rules and input the success or failure event can be triggered. 
