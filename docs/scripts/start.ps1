# PowerShell script to start the WowAchievementsApp
# This script stops any existing instance and then starts the application
# Usage: .\start.ps1

$ErrorActionPreference = "Stop"

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$appProjectPath = Join-Path $projectRoot "WowAchievementsApp"

Write-Host "Starting WowAchievementsApp..." -ForegroundColor Cyan
Write-Host ""

# First, stop any existing instance
Write-Host "Checking for existing instances..." -ForegroundColor Yellow
$stopScript = Join-Path $scriptDir "stop.ps1"

if (Test-Path $stopScript) {
    Write-Host "Stopping any existing instances..." -ForegroundColor Yellow
    & $stopScript
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        Write-Warning "Stop script returned exit code $LASTEXITCODE, but continuing anyway..."
    }
    Write-Host ""
}
else {
    Write-Warning "Stop script not found at $stopScript, skipping stop step..."
}

# Verify the project exists
if (-not (Test-Path $appProjectPath)) {
    Write-Error "Application project not found at: $appProjectPath"
    exit 1
}

# Start the application
Write-Host "Starting application..." -ForegroundColor Green
Write-Host "Project path: $appProjectPath" -ForegroundColor Gray
Write-Host ""

Set-Location $appProjectPath
dotnet run

