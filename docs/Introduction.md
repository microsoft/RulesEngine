## What is the Rules Engine
While building any application, the crux or the core part of it is always business logic or business rules. And as with any application, there always comes a time when some or a lot of the rules or policies change in the system. But with that change, comes a lot of rework like changing design or creating a new module altogether to code in the changes in the rules, regression testing, performance testing etc. The rework along with debugging if required amounts to a lot of unnecessary work which can otherwise be utilized for other work, thus reducing the engineering cycle by drastic amounts.  

In this library, we have abstracted the rules so that the core logic is always maintained while the rules change can happen in an easy way without changing the code base. Also, the input to the system is dynamic in nature so the model need not be defined in the system. It can be sent as an expando object or any other typed object and the system will be able to handle it. 

These all features make this library highly configurable and extensible as shown in [Getting Started with Rules Engine](https://github.com/microsoft/RulesEngine/wiki/Getting-Started).


### How it works

[[https://github.com/microsoft/RulesEngine/blob/master/assets/BlockDiagram.png|alt=octocat]]

Here. there are multiple actors/component involved.
##### Rules Engine
This component is the Rules Engine library/NuGet package being referenced by the developer.
##### Rules Store
As shown in [Rules Schema](https://github.com/microsoft/RulesEngine/wiki/Getting-Started#rules-schema), we need rules in a particular format for the library to work. While the rules structure is rigid, the data itself can be stored in any component and can be accessed in any form the developer chooses. It can be stored in the form of json files in file structure or blob, or as documents in cosmos db, or any database, azure app configuration or any other place the developer thinks is going to be appropriate based on the project requirements. 
##### Input
The input(s) for the system can be taken from any place as well like user input, blobs, databases, service bus or any other system. 
##### Wrapper
This library sits as a black box outside the project as a referenced project or NuGet package. Then the user can create a wrapper around the library, which will get the rules from the rules store and convert it into the WorkFlowRules structure and send it to the RulesEngine along with the input(s). The RulesEngine then computes and give the information to the wrapper and the wrapper can then do whatever the logic demands with the output information.

