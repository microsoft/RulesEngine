param(
    [Parameter(Mandatory)]
    [string] $csprojFilePath,
    [Parameter(Mandatory)]
    [string] $signingKey,
    [string] $nugetSrc = "https://api.nuget.org/v3/index.json",
    [Parameter(Mandatory)]
    [string] $nugetKey,
    [string] $versionPrefix = "v"
)

$version = $versionPrefix + ([xml](Get-Content $csprojFilePath)).Project.PropertyGroup.Version

# add git tags
git tag $version
git push origin $version

# sign and pack the project
$directory = Split-Path $csprojFilePath;
$signKeyFile = Join-Path $directory "signKey.snk";

$bytes = [Convert]::FromBase64String($signingKey)
[IO.File]::WriteAllBytes($signKeyFile, $bytes)

dotnet pack $csprojFilePath -c Release --output dist -p:ContinuousIntegrationBuild=true -p:DelaySign=false -p:AssemblyOriginatorKeyFile=$signKeyFile 

# publish nuget
dotnet nuget push "dist/*.nupkg" -s $nugetSrc -k $nugetKey



