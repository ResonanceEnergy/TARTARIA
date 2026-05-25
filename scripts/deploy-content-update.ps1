# ============================================================================
# TARTARIA Content Update Deployment Script
# ============================================================================
# Purpose: Package and deploy new quests, items, events, and dialogue
#          without requiring full game rebuild or Unity recompile.
#
# Usage:
#   .\deploy-content-update.ps1 -ContentType DailyQuest -Version "1.1.0"
#   .\deploy-content-update.ps1 -ContentType Event -Version "winter-2026" -Validate
#   .\deploy-content-update.ps1 -ContentType All -Version "1.1.1" -Deploy
#
# Deployment Targets:
#   - Local: Build\Windows\TARTARIA_Data\StreamingAssets\
#   - Remote: CDN or file server (configure $RemoteServer below)
#
# SLA Target: <1 hour from content creation to live deployment
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("DailyQuest", "Event", "Dialogue", "All")]
    [string]$ContentType,

    [Parameter(Mandatory=$true)]
    [string]$Version,

    [switch]$Validate,      # Run validation only (no deployment)
    [switch]$Deploy,        # Deploy to production
    [switch]$SkipBackup,    # Skip backup creation (not recommended)
    [string]$RemoteServer = ""  # CDN/file server URL (empty = local only)
)

cd C:\dev\TARTARIA_new

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "Logs\content-deploy-$timestamp.log"

# Ensure log directory exists
if (!(Test-Path "Logs")) { New-Item -ItemType Directory -Path "Logs" | Out-Null }

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logEntry = "[$(Get-Date -Format 'HH:mm:ss')] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $logFile -Value $logEntry
}

Write-Log "========================================" "START"
Write-Log "TARTARIA Content Update Deployment"
Write-Log "Content Type: $ContentType | Version: $Version"
Write-Log "========================================"

# ─── VALIDATION PHASE ────────────────────────────────────────────────────────

Write-Log "PHASE 1: Content Validation" "INFO"

$validationErrors = 0
$validationWarnings = 0

function Validate-JsonFile {
    param([string]$FilePath)
    
    try {
        $content = Get-Content $FilePath -Raw | ConvertFrom-Json
        
        # Check for required fields based on content type
        if ($ContentType -eq "DailyQuest" -or $ContentType -eq "All") {
            if (!$content.questId) { throw "Missing questId" }
            if (!$content.displayName) { throw "Missing displayName" }
            if (!$content.objectives) { throw "Missing objectives" }
            if (!$content.rewards) { throw "Missing rewards" }
        }
        
        if ($ContentType -eq "Event" -or $ContentType -eq "All") {
            if (!$content.eventId) { throw "Missing eventId" }
            if (!$content.startTimeISO) { throw "Missing startTimeISO" }
            if (!$content.endTimeISO) { throw "Missing endTimeISO" }
        }
        
        Write-Log "  ✓ Valid: $(Split-Path $FilePath -Leaf)" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "  ✗ Invalid JSON: $(Split-Path $FilePath -Leaf) - $($_.Exception.Message)" "ERROR"
        $script:validationErrors++
        return $false
    }
}

# Validate daily quests
if ($ContentType -eq "DailyQuest" -or $ContentType -eq "All") {
    Write-Log "Validating daily quests..."
    $questFiles = Get-ChildItem "Assets\StreamingAssets\LiveOps\DailyQuests\*.json" -ErrorAction SilentlyContinue
    
    if ($questFiles) {
        foreach ($file in $questFiles) {
            Validate-JsonFile -FilePath $file.FullName
        }
        Write-Log "Validated $($questFiles.Count) daily quest(s)"
    }
    else {
        Write-Log "  ⚠ No daily quest files found" "WARN"
        $validationWarnings++
    }
}

# Validate events
if ($ContentType -eq "Event" -or $ContentType -eq "All") {
    Write-Log "Validating seasonal events..."
    $eventFiles = Get-ChildItem "Assets\StreamingAssets\LiveOps\Events\*.json" -ErrorAction SilentlyContinue
    
    if ($eventFiles) {
        foreach ($file in $eventFiles) {
            Validate-JsonFile -FilePath $file.FullName
        }
        Write-Log "Validated $($eventFiles.Count) event(s)"
    }
    else {
        Write-Log "  ⚠ No event files found" "WARN"
        $validationWarnings++
    }
}

# Validate dialogue
if ($ContentType -eq "Dialogue" -or $ContentType -eq "All") {
    Write-Log "Validating dialogue files..."
    $dialogueFiles = Get-ChildItem "Assets\StreamingAssets\Dialogue\*.json" -ErrorAction SilentlyContinue
    
    if ($dialogueFiles) {
        foreach ($file in $dialogueFiles) {
            try {
                $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
                if (!$content.lines) { throw "Missing lines array" }
                Write-Log "  ✓ Valid: $(Split-Path $file.FullName -Leaf)" "SUCCESS"
            }
            catch {
                Write-Log "  ✗ Invalid: $(Split-Path $file.FullName -Leaf) - $($_.Exception.Message)" "ERROR"
                $validationErrors++
            }
        }
        Write-Log "Validated $($dialogueFiles.Count) dialogue file(s)"
    }
    else {
        Write-Log "  ⚠ No dialogue files found" "WARN"
        $validationWarnings++
    }
}

Write-Log "Validation complete: $validationErrors error(s), $validationWarnings warning(s)"

if ($validationErrors -gt 0) {
    Write-Log "Validation failed. Aborting deployment." "ERROR"
    exit 1
}

if ($Validate) {
    Write-Log "Validation-only mode. Exiting." "INFO"
    exit 0
}

# ─── BACKUP PHASE ────────────────────────────────────────────────────────────

if (!$SkipBackup) {
    Write-Log "PHASE 2: Creating backup" "INFO"
    
    $backupDir = "Backups\ContentUpdates\$Version-$timestamp"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    
    # Backup current StreamingAssets content
    if (Test-Path "Assets\StreamingAssets\LiveOps") {
        Copy-Item -Path "Assets\StreamingAssets\LiveOps" -Destination "$backupDir\LiveOps" -Recurse -Force
        Write-Log "  ✓ Backed up LiveOps content to $backupDir"
    }
    
    if (Test-Path "Assets\StreamingAssets\Dialogue") {
        Copy-Item -Path "Assets\StreamingAssets\Dialogue" -Destination "$backupDir\Dialogue" -Recurse -Force
        Write-Log "  ✓ Backed up Dialogue content to $backupDir"
    }
    
    # Create manifest
    $manifest = @{
        Version = $Version
        Timestamp = $timestamp
        ContentType = $ContentType
        FileCount = (Get-ChildItem $backupDir -Recurse -File | Measure-Object).Count
    } | ConvertTo-Json
    
    $manifest | Out-File "$backupDir\manifest.json"
    Write-Log "Backup complete: $backupDir"
}

# ─── PACKAGE PHASE ───────────────────────────────────────────────────────────

Write-Log "PHASE 3: Creating deployment package" "INFO"

$packageDir = "Build\ContentUpdates\$Version"
if (Test-Path $packageDir) {
    Remove-Item -Path $packageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# Copy content files
if ($ContentType -eq "DailyQuest" -or $ContentType -eq "All") {
    $destDir = "$packageDir\LiveOps\DailyQuests"
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Copy-Item -Path "Assets\StreamingAssets\LiveOps\DailyQuests\*.json" -Destination $destDir -ErrorAction SilentlyContinue
    $questCount = (Get-ChildItem $destDir -File -ErrorAction SilentlyContinue | Measure-Object).Count
    Write-Log "  ✓ Packaged $questCount daily quest(s)"
}

if ($ContentType -eq "Event" -or $ContentType -eq "All") {
    $destDir = "$packageDir\LiveOps\Events"
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Copy-Item -Path "Assets\StreamingAssets\LiveOps\Events\*.json" -Destination $destDir -ErrorAction SilentlyContinue
    $eventCount = (Get-ChildItem $destDir -File -ErrorAction SilentlyContinue | Measure-Object).Count
    Write-Log "  ✓ Packaged $eventCount event(s)"
}

if ($ContentType -eq "Dialogue" -or $ContentType -eq "All") {
    $destDir = "$packageDir\Dialogue"
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Copy-Item -Path "Assets\StreamingAssets\Dialogue\*.json" -Destination $destDir -ErrorAction SilentlyContinue
    $dialogueCount = (Get-ChildItem $destDir -File -ErrorAction SilentlyContinue | Measure-Object).Count
    Write-Log "  ✓ Packaged $dialogueCount dialogue file(s)"
}

# Create version manifest
$versionManifest = @{
    Version = $Version
    ContentType = $ContentType
    BuildDate = $timestamp
    GameVersion = "0.9.0"
    MinCompatibleVersion = "0.9.0"
    Files = @()
}

Get-ChildItem $packageDir -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($packageDir.Length + 1)
    $versionManifest.Files += @{
        Path = $relativePath
        Size = $_.Length
        Hash = (Get-FileHash $_.FullName -Algorithm MD5).Hash
    }
}

$versionManifest | ConvertTo-Json -Depth 10 | Out-File "$packageDir\version-manifest.json"
Write-Log "Package created: $packageDir ($(($versionManifest.Files).Count) files)"

# ─── DEPLOY PHASE ────────────────────────────────────────────────────────────

if ($Deploy) {
    Write-Log "PHASE 4: Deploying to production" "INFO"
    
    # Deploy to local build
    $localDest = "Build\Windows\TARTARIA_Data\StreamingAssets"
    if (Test-Path $localDest) {
        Copy-Item -Path "$packageDir\*" -Destination $localDest -Recurse -Force
        Write-Log "  ✓ Deployed to local build: $localDest"
    }
    else {
        Write-Log "  ⚠ Local build not found: $localDest" "WARN"
    }
    
    # Deploy to remote server (if configured)
    if ($RemoteServer) {
        Write-Log "Deploying to remote server: $RemoteServer"
        
        # Compress package
        $zipPath = "Build\ContentUpdates\$Version.zip"
        Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force
        Write-Log "  ✓ Created deployment package: $zipPath"
        
        # TODO: Upload to CDN/file server
        # Example: Invoke-RestMethod -Uri "$RemoteServer/upload" -Method Post -InFile $zipPath
        Write-Log "  ⚠ Remote upload not yet implemented - manual upload required" "WARN"
    }
    
    Write-Log "Deployment complete!"
}
else {
    Write-Log "Deployment package ready (use -Deploy flag to deploy)" "INFO"
}

# ─── SUMMARY ─────────────────────────────────────────────────────────────────

Write-Log "========================================" "END"
Write-Log "Content Update Summary:"
Write-Log "  Version: $Version"
Write-Log "  Content Type: $ContentType"
Write-Log "  Validation: $validationErrors error(s), $validationWarnings warning(s)"
Write-Log "  Package: $packageDir"
Write-Log "  Log: $logFile"
Write-Log "========================================"

# ─── ROLLBACK INSTRUCTIONS ───────────────────────────────────────────────────

Write-Log ""
Write-Log "ROLLBACK INSTRUCTIONS (if needed):"
if (!$SkipBackup) {
    Write-Log "  1. Stop the game/server"
    Write-Log "  2. Restore from backup:"
    Write-Log "     Copy-Item '$backupDir\*' -Destination 'Assets\StreamingAssets' -Recurse -Force"
    Write-Log "  3. Re-deploy previous version"
    Write-Log "  4. Restart game/server"
}
else {
    Write-Log "  WARNING: No backup was created (-SkipBackup was used)"
}

exit 0
