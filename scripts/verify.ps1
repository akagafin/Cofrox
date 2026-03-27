# Run from repo root: pwsh -File .\scripts\verify.ps1
$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)

$repoRoot = Get-Location
$dotnetState = Join-Path $repoRoot ".dotnet"
$appDataRoot = Join-Path $repoRoot ".appdata"
$nugetPackages = Join-Path $repoRoot ".nuget\\packages"
New-Item -ItemType Directory -Force -Path $dotnetState, $appDataRoot, $nugetPackages | Out-Null
$env:DOTNET_CLI_HOME = $dotnetState
$env:APPDATA = $appDataRoot
$env:NUGET_PACKAGES = $nugetPackages

Write-Host "dotnet SDK:" (dotnet --version)
dotnet test .\tests\Cofrox.Core.Tests\Cofrox.Core.Tests.csproj -c Release -p:RestoreConfigFile=NuGet.Config
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet test .\tests\Cofrox.Application.Tests\Cofrox.Application.Tests.csproj -c Release -p:RestoreConfigFile=NuGet.Config
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Building solution (x64 Release)..."
dotnet build .\Cofrox.sln -c Release -p:Platform=x64 -p:RestoreConfigFile=NuGet.Config
exit $LASTEXITCODE
