# CHANGELOG

All notable changes to this project will be documented in this file.

## [3.0.0-preview.2]
- Made LocalParams and ErrorMessage formatting optional via ReSettings
- Major performance improvement
	- 25% improvement from previous version
	- Upto 35% improvement by disabling optional features

## [3.0.0-preview.1] - 23-10-2020
- Renamed `ExecuteRule` to `ExecuteAllRulesAsync`
- Added Actions support. More details on [actions wiki](https://github.com/microsoft/RulesEngine/wiki/Actions)

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
