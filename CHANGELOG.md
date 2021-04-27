# CHANGELOG

All notable changes to this project will be documented in this file.

## [3.1.0]
- Added globalParams feature which can be applied to all rules
- Enabled localParams support for nested Rules
- Made certain fields in Rule model optional allowing users to define workflow with minimal fields
- Added option to disable Rule in workflow json
- Added `GetAllRegisteredWorkflow` to RulesEngine to return all registeredWorkflows
- Runtime errors for expressions will now be logged as errorMessage instead of throwing Exceptions by default
- Fixed RuleParameter passed as null

## [3.0.2]
- Fixed LocalParams cache not getting cleaned up when RemoveWorkflow and ClearWorkflows are called

## [3.0.1]
- Moved ActionResult and ActionRuleResult under RulesEngine.Models namespace


## [3.0.0]
### Major Enhancements
- Added support for Actions. More details on [actions wiki](https://github.com/microsoft/RulesEngine/wiki/Actions)
- Major performance improvement
	- 25% improvement from previous version
	- Upto 35% improvement by disabling optional features
- RulesEngine now virtually supports unlimited inputs (Previous limitation was 16 inputs)
- RuleExpressionParser is now available to use expression evaluation outside RulesEngine

### Breaking Changes
- `ExecuteRule` method has been renamed to `ExecuteAllRulesAsync`
- `Input` field in RuleResultTree has been changed to `Inputs` which returns all the the inputs as Dictionary of name and value pair

## [2.1.5] - 02-11-2020
- Added `Properties` field to Rule to allow custom fields to Rule

## [2.1.4] - 15-10-2020
- Added exception data properties to identify RuleName.

## [2.1.3] - 12-10-2020
- Optional parameter for rethrow exception on failure of expression compilation.

## [2.1.2] - 02-10-2020
- Fixed binary expression requirement. Now any expression will work as long as it evalutes to boolean.

## [2.1.1] - 01-09-2020
- Fixed exception thrown when errormessage field is null
- Added better messaging when identifier is not found in expression
- Fixed other minor bugs

## [2.1.0] - 18-05-2020
- Adding local param support to make expression authroing more intuitive.

## [2.0.0] - 18-05-2020
### Changed
- Interface simplified by removing redundant parameters in the IRulesEngine.
- Custom Logger replaced with Microsoft Logger.

## [1.0.2] - 16-01-2020
### Added
- Cache system added so that rules compilation is stored and thus made more efficient.

### Fix
- Concurrency issue which arose by dictionary was resolved.

## [1.0.1] - 24-09-2019
### Added
- Exceptions handling scenario in the case a rule execution throws an exception 

## [1.0.0] - 20-08-2019

### Added
- The first version of the NuGet
