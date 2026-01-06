# PowerShell script to stop the WowAchievementsApp by finding and killing processes on its port
# Usage: .\stop.ps1 [port]
# If no port is provided, it will be read from launchSettings.json

param(
    [int]$Port = 0
)

$ErrorActionPreference = "Stop"

# Function to extract port from launchSettings.json
function Get-PortFromLaunchSettings {
    $launchSettingsPath = Join-Path $PSScriptRoot "..\..\WowAchievementsApp\Properties\launchSettings.json"
    
    if (-not (Test-Path $launchSettingsPath)) {
        Write-Error "Could not find launchSettings.json at: $launchSettingsPath"
        return $null
    }
    
    try {
        $launchSettings = Get-Content $launchSettingsPath | ConvertFrom-Json
        $applicationUrl = $launchSettings.profiles.https.applicationUrl
        
        if ($applicationUrl -match ':(\d+)') {
            $port = [int]$matches[1]
            Write-Host "Found port $port from launchSettings.json" -ForegroundColor Green
            return $port
        }
    }
    catch {
        Write-Error "Failed to parse launchSettings.json: $($_.Exception.Message)"
        return $null
    }
    
    return $null
}

# Determine the port to use
if ($Port -eq 0) {
    $Port = Get-PortFromLaunchSettings
    if ($null -eq $Port) {
        Write-Error "Could not determine port. Please specify it as a parameter: .\stop.ps1 -Port 5089"
        exit 1
    }
}

Write-Host "Checking for processes on port $Port..." -ForegroundColor Cyan

# Find processes using the port
try {
    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    
    if ($null -eq $connections -or $connections.Count -eq 0) {
        Write-Host "No processes found running on port $Port" -ForegroundColor Yellow
        exit 0
    }
    
    # Get unique process IDs
    $processIds = $connections | Select-Object -ExpandProperty OwningProcess -Unique
    
    Write-Host "Found $($processIds.Count) process(es) using port ${Port}:" -ForegroundColor Yellow
    
    foreach ($processId in $processIds) {
        try {
            $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  - PID: $processId, Name: $($process.ProcessName), Path: $($process.Path)" -ForegroundColor White
            }
        }
        catch {
            Write-Host "  - PID: $processId (process information unavailable)" -ForegroundColor Gray
        }
    }
    
    # Kill the processes
    Write-Host "`nStopping processes..." -ForegroundColor Cyan
    $killedCount = 0
    
    foreach ($processId in $processIds) {
        try {
            $process = Get-Process -Id $processId -ErrorAction Stop
            Stop-Process -Id $processId -Force -ErrorAction Stop
            Write-Host "  [OK] Stopped process $processId ($($process.ProcessName))" -ForegroundColor Green
            $killedCount++
        }
        catch {
            Write-Host "  [FAIL] Failed to stop process $processId : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    if ($killedCount -gt 0) {
        Write-Host "`nSuccessfully stopped $killedCount process(es) on port $Port" -ForegroundColor Green
    }
    else {
        Write-Host "`nNo processes were stopped" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    $errorMsg = $_.Exception.Message
    Write-Error "An error occurred while checking for processes: $errorMsg"
    exit 1
}

