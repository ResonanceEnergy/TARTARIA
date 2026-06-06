# TARTARIA — Apply Test Integration
# Wires TestOrchestrator into Echohaven_VerticalSlice.unity via batchmode Editor script.
#
# MISSION: Automate scene integration without manual Unity Editor actions
#
# USAGE:
#   .\apply-test-integration.ps1
#
# OUTPUT:
#   - Unity Console logs with [SceneIntegration] prefix
#   - Exit code 0 (success) or 1 (failure)
#   - Scene saved with TestOrchestrator GameObject + component configured
#
# REQUIREMENTS:
#   - Unity 6000.3.6f1 (or compatible)
#   - SceneIntegrationPatch.cs in Assets/_Project/Editor/QA/
#   - Echohaven_VerticalSlice.unity scene exists
#
# IDEMPOTENT: Safe to run multiple times (will update config if already exists)

param(
    [switch]$Verbose
)

cd C:\dev\TARTARIA_new

# ══════════════════════════════════════════════════════════════════════════════
# CONFIGURATION
# ══════════════════════════════════════════════════════════════════════════════

$ProjectPath = "C:\dev\TARTARIA_new"
$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$LogFile = "Logs\scene-integration.log"
$ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity"

# ══════════════════════════════════════════════════════════════════════════════
# VALIDATION
# ══════════════════════════════════════════════════════════════════════════════

Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TARTARIA — Apply Test Integration" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Check Unity exists
if (-not (Test-Path $UnityExe))
{
    Write-Host "✗ Unity not found at: $UnityExe" -ForegroundColor Red
    Write-Host "Update `$UnityExe path in script header" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Unity found: $UnityExe" -ForegroundColor Green

# Check project path exists
if (-not (Test-Path $ProjectPath))
{
    Write-Host "✗ Project not found at: $ProjectPath" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Project path: $ProjectPath" -ForegroundColor Green

# Check scene exists
if (-not (Test-Path "$ProjectPath\$ScenePath"))
{
    Write-Host "✗ Scene not found: $ScenePath" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Scene found: $ScenePath" -ForegroundColor Green

# Check SceneIntegrationPatch.cs exists
$PatchScript = "$ProjectPath\Assets\_Project\Editor\QA\SceneIntegrationPatch.cs"
if (-not (Test-Path $PatchScript))
{
    Write-Host "✗ SceneIntegrationPatch.cs not found at: $PatchScript" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Editor script found: SceneIntegrationPatch.cs" -ForegroundColor Green

# ══════════════════════════════════════════════════════════════════════════════
# PREPARE LOG DIRECTORY
# ══════════════════════════════════════════════════════════════════════════════

$LogDir = Split-Path $LogFile -Parent
if (-not (Test-Path $LogDir))
{
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
    Write-Host "✓ Created log directory: $LogDir" -ForegroundColor Green
}

# ══════════════════════════════════════════════════════════════════════════════
# RUN UNITY BATCHMODE
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "Running Unity batchmode: SceneIntegrationPatch.WireTestOrchestrator..." -ForegroundColor Cyan
Write-Host ""

$BatchArgs = @(
    "-batchmode",
    "-nographics",
    "-projectPath", "`"$ProjectPath`"",
    "-executeMethod", "Tartaria.Editor.SceneIntegrationPatch.WireTestOrchestrator",
    "-logFile", "`"$LogFile`"",
    "-quit"
)

$ProcessArgs = $BatchArgs -join " "

if ($Verbose)
{
    Write-Host "Command: $UnityExe $ProcessArgs" -ForegroundColor DarkGray
    Write-Host ""
}

$Process = Start-Process -FilePath $UnityExe -ArgumentList $BatchArgs -NoNewWindow -PassThru -Wait

$ExitCode = $Process.ExitCode

# ══════════════════════════════════════════════════════════════════════════════
# PARSE RESULTS
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "RESULTS" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if (Test-Path $LogFile)
{
    $LogContent = Get-Content $LogFile -Raw
    
    # Extract [SceneIntegration] lines
    $IntegrationLines = $LogContent -split "`n" | Where-Object { $_ -match '\[SceneIntegration\]' }
    
    if ($IntegrationLines.Count -gt 0)
    {
        Write-Host "Unity Log (filtered):" -ForegroundColor Yellow
        foreach ($Line in $IntegrationLines)
        {
            # Color code output
            if ($Line -match '✓')
            {
                Write-Host $Line -ForegroundColor Green
            }
            elseif ($Line -match '✗')
            {
                Write-Host $Line -ForegroundColor Red
            }
            elseif ($Line -match '⚠')
            {
                Write-Host $Line -ForegroundColor Yellow
            }
            else
            {
                Write-Host $Line
            }
        }
    }
    else
    {
        Write-Host "⚠ No [SceneIntegration] log entries found" -ForegroundColor Yellow
        
        if ($Verbose)
        {
            Write-Host ""
            Write-Host "Full log:" -ForegroundColor DarkGray
            Write-Host $LogContent -ForegroundColor DarkGray
        }
    }
}
else
{
    Write-Host "⚠ Log file not found: $LogFile" -ForegroundColor Yellow
}

# ══════════════════════════════════════════════════════════════════════════════
# EXIT CODE HANDLING
# ══════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($ExitCode -eq 0)
{
    Write-Host "✓ SCENE INTEGRATION COMPLETE" -ForegroundColor Green
    Write-Host "TestOrchestrator successfully wired into Echohaven_VerticalSlice.unity" -ForegroundColor Green
    Write-Host ""
    Write-Host "Configuration:" -ForegroundColor Cyan
    Write-Host "  • autoStartOnPlay = true" -ForegroundColor White
    Write-Host "  • phaseDelay = 1.5s" -ForegroundColor White
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Open Echohaven_VerticalSlice.unity in Unity Editor" -ForegroundColor White
    Write-Host "  2. Press Play to run automated tests" -ForegroundColor White
    Write-Host "  3. Or run: .\tartaria-play.ps1 -Scene Echohaven_VerticalSlice" -ForegroundColor White
}
else
{
    Write-Host "✗ SCENE INTEGRATION FAILED" -ForegroundColor Red
    Write-Host "Unity exit code: $ExitCode" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check log for details: $LogFile" -ForegroundColor Yellow
}

Write-Host "══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

exit $ExitCode
