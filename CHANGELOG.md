# CHANGELOG

All notable changes to this project will be documented in this file.

## [Unreleased]

## [6.0.1]

Stable release rolling up everything from `6.0.1-preview.1` through `6.0.1-preview.3` — no additional code changes beyond preview.3. See per-preview sections below for the full delta from `6.0.0`.

### Headline changes since 6.0.0
- **Perf:** Restored the compiled-expression cache in `RuleExpressionParser` — 100×–1900× speedups on paths not served by `RulesCache` (#673, #727).
- **Perf:** Fixed the warmup regression from per-expression AppDomain assembly scans in `CustomTypeProvider` — ~6.6× faster registration on realistic large workflows (#739, #740).
- **Feature:** Opt-in `ReSettings.EnableParallelRuleCompilation` for further warmup gains on very large (10k+ rule) workflows (#741, #744).
- **Feature:** `IRulesEngine.ExecuteAllRulesAsync` overload accepting a `CancellationToken` with cooperative cancellation between rules and before each action (#609).
- **Feature:** `ReSettings.AutoExecuteActions` (default `true`) — set to `false` to evaluate rules without automatically running their OnSuccess/OnFailure actions (#596).
- **Fixes:** Cleaner errors for exception propagation (#624), list schema union (#704), OutputExpression syntax hints (#711), global-param dedup (#714), object-return diagnostics (#717), `ExecuteActionWorkflowAsync` error message population (#519), `ActionContext` null guard (#576), deep dotted `ErrorMessage` interpolation (#696), STJ `JsonElement` unwrap for `ExpandoObject` inputs (#668), and clear errors for duplicate param names or input/global collisions (#743).

## [6.0.1-preview.3]

### Performance
- Fixed the warmup regression introduced by #675: `CustomTypeProvider.GetCustomTypes()` scanned every AppDomain assembly on every expression parse. Now memoized on the provider, and `RuleExpressionParser` reuses a single `ParsingConfig` until `ReSettings.CustomTypes` swaps. Restores 5.0.3 warmup on the reporter's 20,000-rule benchmark: 113.8s → 16.4s (#739, #740).

### Features
- New `ReSettings.EnableParallelRuleCompilation` (default `false`). When `true`, workflow rules are compiled in parallel during registration, materially reducing warmup time for workflows with many thousands of rules. Silently falls back to serial compilation when combined with `UseFastExpressionCompiler = true` (which regresses ~3× under contention) or for workflows below an internal scheduling-cost threshold (~32). Builds on the warmup work in #740 (#741, #744).

### Fixes
- `WorkflowsValidator` and `RuleValidator` now surface clear `RuleValidationException` messages when two `GlobalParams` (or two `LocalParams`) share a name, instead of failing later with the cryptic "An item with the same key has already been added" from result-tree construction. `RulesEngine.AppendGlobals` similarly detects when a caller-supplied `RuleParameter.Name` collides with a workflow `GlobalParam.Name` and surfaces the collision via the same per-rule error-surfacing path used elsewhere (#743).

## [6.0.1-preview.2]

### Features
- `IRulesEngine.ExecuteAllRulesAsync` gains an overload accepting a `CancellationToken`, observed cooperatively between rules and before each action. The existing `params object[]` and `params RuleParameter[]` overloads are unchanged; call-site overload resolution continues to pick them when no token is supplied (#609).
- New `ReSettings.AutoExecuteActions` (default `true`). Set to `false` to evaluate rules without automatically running their OnSuccess/OnFailure actions, so callers can run actions selectively via `ExecuteActionWorkflowAsync` (#596).
- Documented and tested passing computed `additionalInputs` into the `EvaluateRule` action — the additionalInput `Name` is referenced directly in the target rule's expression (#573).

### Fixes
- `Utils.CreateAbstractClassType` / `CreateObject` now unwrap `System.Text.Json.JsonElement` scalar values to their native CLR types (string / int / long / double / bool / null) when building typed objects from `ExpandoObject` inputs. This restores the pre-System.Text.Json behavior for rule expressions like `input1.country == "india"` that previously failed with "binary operator Equal is not defined for the types 'JsonElement' and 'String'" (#668).

### Docs
- Removed the obsolete `ErrorType` field from JSON examples in `README.md`, `docs/Getting-Started.md`, and `docs/index.md`. `ErrorType` was removed from the `Rule` model in 4.0.0 (#676).

### Regression guards added (already correct on master, now covered by tests)
- #692 — Nullable `DateTime` comparisons against `null` (`null < someDate` / `null > someDate`) return `false`, matching standard C# `Nullable<T>` semantics. Test documents the recommended `HasValue` workaround for users who want null-aware ordering.

## [6.0.1-preview.1]

### Performance
- Restored the compiled-expression cache in `RuleExpressionParser` that was removed in 5.0.1, fixing a 10×–1900× regression on the direct parser, `ExecuteActionWorkflowAsync`, and fresh-engine paths. The cache is now instance-scoped with a settings fingerprint in the key, avoiding the latent cross-settings bug the original static cache had (#673, #727).

### Fixes
- `EnableExceptionAsErrorMessage = false` now correctly propagates exceptions thrown from custom actions' `Run()` — they were previously swallowed into `ActionRuleResult.Exception` (#624, #728).
- `Utils.CreateAbstractClassType` now unions schemas across every element of an `ExpandoObject`/`Dictionary` list, so properties that appear only in later elements are no longer dropped (#704, #728).
- `OutputExpressionAction` detects C#-style anonymous-object syntax (`new { X = ... }`) and replaces the cryptic "missing 'as' clause" Dynamic.Core error with a clear hint pointing at the correct `new (value as X, ...)` form (#711, #728).
- Workflow `GlobalParams` are now evaluated **once per `ExecuteAllRulesAsync` call** instead of once per rule. A delegate is compiled at registration time and results are appended to each rule's inputs (#714, #728).
- `LambdaExpressionBuilder` detects "exists in type 'Object'" / "'System.Object'" parse errors and appends a helpful hint when a custom method's `object` return type is the cause (#717, #728).
- `ExecuteActionWorkflowAsync` now runs `FormatErrorMessages` like `ExecuteAllRulesAsync` does, so `Rule.ErrorMessage` templates are interpolated into `ExceptionMessage` consistently across both APIs (#519, #729).
- `ActionContext` no longer NREs when a rule action's `Context` (or a nested value) is null — common for custom actions that don't need configuration (#576, #729).
- `ErrorMessage` interpolation now walks arbitrary-depth dotted paths via `JsonNode`: `$(input.Inner.Name)` resolves to the leaf scalar instead of the raw JSON of the intermediate object (#696, #729).

### Regression guards added (already fixed upstream, now covered by tests)
- #581 — custom `RuleParameter` names not being honored (resolved by `AutoRegisterInputType` in 5.0.1).
- #590 — exception state from one execution leaking into the next (resolved by #592 in 5.0.3).
- #606 — lambda parameter on the left side of `=>` reported as unknown identifier (resolved by `System.Linq.Dynamic.Core` 1.4.3 → 1.6.7 bumps).
- #608 — `UseFastExpressionCompiler = true` NRE on chained scoped-param `Sum()` (resolved by recent `FastExpressionCompiler` upgrades).

## [5.0.3]
- Updated dependencies to latest
- Fixed RulesEngine throwing exception when type name is same as input name
- Added config to disable FastCompile for expressions
- Added RuleParameter.Create method for better handling on types when value is null

## [5.0.2]
- Fixed Scoped Params returning incorrect results in some corner case scenarios

## [5.0.1]
- Added option to disable automatic type registry for input parameters in reSettings
- Added option to make expression case sensitive in reSettings

## [5.0.0]
- Fixed security bug related to System.Dynamic.Linq.Core

### Breaking Changes
- As a part of security bug fix, method call for only registered types via reSettings will be allowed. This only impacts strongly typed inputs and nested types


## [4.0.0]
- RulesEngine is now available in both dotnet 6 and netstandard 2.0
- Dependency on ILogger, MemoryCache have been removed 
- Obsolete Properties and Methods have been removed
- Fixed name of RuleParameter is ignored if the type is recognized (by @peeveen)
### Breaking Changes
- ILogger has been removed from RulesEngine and all its constructors
```diff
- RulesEngine(string[] jsonConfig, ILogger logger = null, ReSettings reSettings = null)
+ RulesEngine(string[] jsonConfig, ReSettings reSettings = null)

- RulesEngine(Workflow[] Workflows, ILogger logger = null, ReSettings reSettings = null)
+ RulesEngine(Workflow[] Workflows, ReSettings reSettings = null)

- RulesEngine(ILogger logger = null, ReSettings reSettings = null)
+ RulesEngine(ReSettings reSettings = null)
```
- Obsolete methods and properties have been removed, from the follow models:-
	- RuleResultTree
		- `ToResultTreeMessages()` has been removed from `RuleResultTree` model
		- `GetMessages()` has been removed from `RuleResultTree` model
		- `RuleEvaluatedParams` has been removed from `RuleResultTree` model, Please use `Inputs` instead

	- Workflow
		- `WorkflowRulesToInject` has been removed, Please use `WorkflowsToInject` instead
		- `ErrorType` has been removed from `Rule`

	- Resettings
		- `EnableLocalParams` has been removed from `ReSettings`, Please use `EnableScopedParams` instead
	

## [3.5.0]
- `EvaluateRule` action now support custom inputs and filtered inputs
- Added `ContainsWorkflow` method in RulesEngine (by @okolobaxa)
- Fixed minor bugs (#258 & #259)

## [3.4.0]
- Made RulesEngine Strong Name and Authenticode signed
- Renamed few models to streamline names (by @alexrich)
	- `WorkflowRules` is renamed to `Workflow`
	- `WorkflowRulesToInject` is renamed to `WorkflowsToInject`
	- `RuleAction` is renamed to `RuleActions`
	
	**Note**: The old models are still supported but will be removed with version 4.0.0


## [3.3.0]
- Added support for actions in nested rules
- Improved serialization support for System.Text.Json for workflow model
  
Breaking Change:
  - Type of Action has been changed from `Dictionary<ActionTriggerType, ActionInfo>` to `RuleActions`
    - No impact if you are serializing workflow from json
    - For workflow objects created in code, refer - [link](https://github.com/microsoft/RulesEngine/pull/182/files#diff-a5093dda2dcc1e4958ce3533edb607bb61406e1f0a9071eca4e317bdd987c0d3)

## [3.2.0]
- Added AddOrUpdateWorkflow method to update workflows atomically (by @AshishPrasad)
- Updated dependencies to latest

Breaking Change:
  - `AddWorkflow` now throws exception if you try to add a workflow which already exists.
  Use `AddOrUpdateWorkflow` to update existing workflow

## [3.1.0]
- Added globalParams feature which can be applied to all rules
- Enabled localParams support for nested Rules
- Made certain fields in Rule model optional allowing users to define workflow with minimal fields
- Added option to disable Rule in workflow json
- Added `GetAllRegisteredWorkflow` to RulesEngine to return all registered workflows
- Runtime errors for expressions will now be logged as errorMessage instead of throwing Exceptions by default
- Fixed RuleParameter passed as null

## [3.0.2]
- Fixed LocalParams cache not getting cleaned up when RemoveWorkflows and ClearWorkflows are called

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
