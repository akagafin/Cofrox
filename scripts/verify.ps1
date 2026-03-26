# Run from repo root: pwsh -File .\scripts\verify.ps1
$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)

Write-Host "dotnet SDK:" (dotnet --version)
dotnet test .\tests\Cofrox.Core.Tests\Cofrox.Core.Tests.csproj -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Building solution (x64 Release)..."
dotnet build .\Cofrox.sln -c Release -p:Platform=x64
exit $LASTEXITCODE
