dotnet tool install --tool-path tools dotnet-reportgenerator-globaltool
./tools/reportgenerator "-reports:**/coverage.cobertura.xml" "-targetdir:coveragereport" -reporttypes:"Html;lcov;Cobertura"