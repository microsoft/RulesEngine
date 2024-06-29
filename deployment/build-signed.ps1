param(
    [Parameter(Mandatory)]
    [string] $csprojFilePath,
    [Parameter(Mandatory)]
    [string] $signingKey
)

# sign and build the project
$directory = Split-Path $csprojFilePath;
$signKeyFile = Join-Path $directory "signKey.snk";

$bytes = [Convert]::FromBase64String($signingKey)
[IO.File]::WriteAllBytes($signKeyFile, $bytes)

dotnet build $csprojFilePath -c Release -p:ContinuousIntegrationBuild=true -p:DelaySign=false -p:AssemblyOriginatorKeyFile=$signKeyFile 