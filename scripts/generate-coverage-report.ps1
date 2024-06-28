dotnet tool restore
reportgenerator "-reports:**/coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:"Html;lcov;Cobertura"