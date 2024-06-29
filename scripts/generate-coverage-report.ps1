dotnet tool restore
dotnet reportgenerator "-reports:**/coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:"Html;lcov;Cobertura"