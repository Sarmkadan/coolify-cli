# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Windows PowerShell deployment script for Coolify CLI

param(
    [Parameter(Mandatory=$false)]
    [int]$AppId = 0,

    [Parameter(Mandatory=$false)]
    [string]$Strategy = "blue-green",

    [Parameter(Mandatory=$false)]
    [int]$Timeout = 600,

    [Parameter(Mandatory=$false)]
    [string]$LogDir = "$PSScriptRoot\logs"
)

# Configuration
$ErrorActionPreference = "Stop"
$WarningPreference = "Continue"
$LogFile = Join-Path $LogDir "deploy_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$CliPath = "coolify-cli"  # Assumes coolify-cli is in PATH

# Create log directory
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir | Out-Null
}

# Logging functions
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"

    Write-Host $logMessage
    Add-Content -Path $LogFile -Value $logMessage
}

function Write-LogSuccess {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
    Write-Log $Message "SUCCESS"
}

function Write-LogError {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
    Write-Log $Message "ERROR"
}

function Write-LogWarning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
    Write-Log $Message "WARNING"
}

# Check prerequisites
function Test-Prerequisites {
    Write-Log "Checking prerequisites..."

    # Check if coolify-cli is installed
    if (-not (Get-Command $CliPath -ErrorAction SilentlyContinue)) {
        Write-LogError "coolify-cli not found in PATH"
        exit 1
    }

    # Check environment variables
    if (-not $env:COOLIFY_API_KEY) {
        Write-LogError "COOLIFY_API_KEY environment variable not set"
        exit 1
    }

    if (-not $env:COOLIFY_API_URL) {
        Write-LogError "COOLIFY_API_URL environment variable not set"
        exit 1
    }

    Write-LogSuccess "Prerequisites verified"
}

# Verify API connectivity
function Test-ApiConnectivity {
    Write-Log "Verifying API connectivity..."

    try {
        & $CliPath health | Out-Null
        Write-LogSuccess "API connectivity verified"
        return $true
    } catch {
        Write-LogError "Failed to connect to API: $_"
        return $false
    }
}

# Get application details
function Get-ApplicationDetails {
    param([int]$AppId)

    Write-Log "Getting application details for ID: $AppId"

    try {
        $output = & $CliPath app get $AppId
        Write-LogSuccess "Retrieved application details"
        return $output
    } catch {
        Write-LogError "Failed to get application details: $_"
        return $null
    }
}

# Deploy application
function Deploy-Application {
    param(
        [int]$AppId,
        [string]$Strategy,
        [int]$Timeout
    )

    Write-Log "Starting deployment for application $AppId..."
    Write-Log "Strategy: $Strategy | Timeout: ${Timeout}s"

    try {
        $output = & $CliPath app deploy $AppId --strategy $Strategy --timeout $Timeout --wait true
        Write-LogSuccess "Deployment initiated for application $AppId"
        return $output
    } catch {
        Write-LogError "Deployment failed: $_"
        return $null
    }
}

# Check application status
function Get-ApplicationStatus {
    param([int]$AppId)

    try {
        $status = & $CliPath app status $AppId
        return $status
    } catch {
        Write-LogError "Failed to get application status: $_"
        return $null
    }
}

# Monitor deployment
function Monitor-Deployment {
    param([int]$AppId, [int]$MaxAttempts = 10)

    Write-Log "Monitoring deployment (max attempts: $MaxAttempts)..."

    $attempt = 0
    while ($attempt -lt $MaxAttempts) {
        Start-Sleep -Seconds 30
        $attempt++

        $status = Get-ApplicationStatus $AppId
        if ($status -match "running") {
            Write-LogSuccess "Application is running"
            return $true
        }

        Write-Log "Deployment in progress... (attempt $attempt/$MaxAttempts)"
    }

    Write-LogWarning "Deployment did not complete within expected time"
    return $false
}

# View application logs
function Get-ApplicationLogs {
    param([int]$AppId, [int]$Lines = 100)

    Write-Log "Retrieving application logs (last $Lines lines)..."

    try {
        $logs = & $CliPath logs $AppId --lines $Lines
        return $logs
    } catch {
        Write-LogError "Failed to retrieve logs: $_"
        return $null
    }
}

# Rollback deployment
function Rollback-Deployment {
    param([int]$AppId)

    Write-LogWarning "Initiating rollback for application $AppId..."

    try {
        $output = & $CliPath app rollback $AppId
        Write-LogSuccess "Rollback initiated"
        return $output
    } catch {
        Write-LogError "Rollback failed: $_"
        return $null
    }
}

# Generate deployment report
function New-DeploymentReport {
    param(
        [int]$AppId,
        [string]$Status,
        [string]$StartTime,
        [string]$EndTime
    )

    $reportFile = Join-Path $LogDir "deployment_report_$(Get-Date -Format 'yyyyMMdd_HHmmss').html"

    $duration = (Get-Date $EndTime) - (Get-Date $StartTime)

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Deployment Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .container { max-width: 800px; margin: 0 auto; }
        .header { background: #f2f2f2; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
        .status { padding: 10px; margin: 10px 0; border-radius: 3px; }
        .success { background: #d4edda; color: #155724; }
        .failed { background: #f8d7da; color: #721c24; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #f2f2f2; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Deployment Report</h1>
            <p>Application ID: $AppId</p>
            <p>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
        </div>

        <div class="status $($Status -eq 'Success' ? 'success' : 'failed')">
            <strong>Status:</strong> $Status
        </div>

        <table>
            <tr>
                <th>Parameter</th>
                <th>Value</th>
            </tr>
            <tr>
                <td>Start Time</td>
                <td>$StartTime</td>
            </tr>
            <tr>
                <td>End Time</td>
                <td>$EndTime</td>
            </tr>
            <tr>
                <td>Duration</td>
                <td>$($duration.TotalSeconds) seconds</td>
            </tr>
            <tr>
                <td>Strategy</td>
                <td>$Strategy</td>
            </tr>
        </table>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $reportFile
    Write-LogSuccess "Report generated: $reportFile"
}

# Main deployment function
function Invoke-Deployment {
    Write-Log "======================================"
    Write-Log "Coolify Deployment - Starting"
    Write-Log "======================================"

    # Check prerequisites
    Test-Prerequisites

    # Verify connectivity
    if (-not (Test-ApiConnectivity)) {
        exit 1
    }

    # Get application ID
    if ($AppId -eq 0) {
        # List applications and prompt for selection
        Write-Log "Listing available applications..."
        $apps = & $CliPath app list
        Write-Host $apps

        $AppId = Read-Host "Enter Application ID to deploy"
    }

    # Get application details
    $appDetails = Get-ApplicationDetails $AppId
    if (-not $appDetails) {
        exit 1
    }

    Write-Host $appDetails

    # Confirm deployment
    $confirm = Read-Host "Continue with deployment? (y/n)"
    if ($confirm -ne "y") {
        Write-LogWarning "Deployment cancelled by user"
        exit 0
    }

    # Perform deployment
    $startTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    if (-not (Deploy-Application $AppId $Strategy $Timeout)) {
        Write-LogError "Deployment failed"
        Rollback-Deployment $AppId
        exit 1
    }

    # Monitor deployment
    if (-not (Monitor-Deployment $AppId)) {
        Write-LogWarning "Deployment monitoring timed out"
    }

    # Get logs
    Write-Log "Retrieving application logs..."
    $logs = Get-ApplicationLogs $AppId 50
    if ($logs) {
        Write-Host $logs
    }

    $endTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    # Generate report
    New-DeploymentReport $AppId "Success" $startTime $endTime

    Write-Log "======================================"
    Write-LogSuccess "Deployment completed successfully"
    Write-Log "======================================"
}

# Error handling
$ErrorActionPreference = "Stop"
trap {
    Write-LogError "Unexpected error: $_"
    exit 1
}

# Run deployment
Invoke-Deployment
