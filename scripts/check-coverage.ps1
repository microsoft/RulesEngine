param(
    [Parameter(Mandatory=$true)][string] $reportPath,
    [Parameter(Mandatory=$true)][decimal] $threshold
)


[XML]$report = Get-Content $reportPath;
[decimal]$coverage = [decimal]$report.coverage.'line-rate' * 100;

if ($coverage -lt $threshold) {
  Write-Error "Coverage($coverage) is less than $threshold percent"
  exit 1
}
else{
    Write-Host "Coverage($coverage) is more than $threshold percent"
}
