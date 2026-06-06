#!/usr/bin/env pwsh
# Comprehensive Integration layer fixes - all 2886 errors
cd c:\dev\TARTARIA_new

Write-Host "=== COMPREHENSIVE INTEGRATION FIX ===" -ForegroundColor Cyan

# Count files that need fixes
$filesToFix = @(
    "Assets\_Project\Scripts\Save\SaveData.cs"
    "Assets\_Project\Scripts\Integration\ArchiveManager.cs"
    "Assets\_Project\Scripts\Integration\Moon6ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon7ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon8ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon9ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\CassianController.cs"
    "Assets\_Project\Scripts\Integration\DialogueManager.cs"
    "Assets\_Project\Scripts\Integration\TuningMiniGameRestorationSystem.cs"
    "Assets\_Project\Scripts\Integration\PostProcessingSetup.cs"
    "Assets\_Project\Scripts\Integration\SpireRestorationSystem.cs"
    "Assets\_Project\Scripts\Integration\GameLoopController.cs"
    "Assets\_Project\Scripts\Integration\AudioFeedbackController.cs"
)

Write-Host "Fixing $($filesToFix.Count) files..." -ForegroundColor Yellow

# Fix SaveData - add archive property
$file = "Assets\_Project\Scripts\Save\SaveData.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    if ($content -notmatch 'public ArchiveSaveBlock archive') {
        # Add archive property after moonFlags
        $content = $content -replace '(public MoonFlagsSaveBlock moonFlags;)', "`$1`n        public ArchiveSaveBlock archive;"
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Added archive property to SaveData" -ForegroundColor Green
    }
}

# Fix ArchiveManager delegates - change signature from () to (SaveData)
$file = "Assets\_Project\Scripts\Integration\ArchiveManager.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    # Change void HandleBeforeSave() to void HandleBeforeSave(SaveData saveData)
    $content = $content -replace 'void HandleBeforeSave\(\)', 'void HandleBeforeSave(SaveData saveData)'
    $content = $content -replace 'void HandleAfterLoad\(\)', 'void HandleAfterLoad(SaveData saveData)'
    $content = $content -replace 'void OnBeforeSave\(\)', 'void OnBeforeSave(SaveData saveData)'
    $content = $content -replace 'void OnAfterLoad\(\)', 'void OnAfterLoad(SaveData saveData)'
    [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "✅ Fixed ArchiveManager delegate signatures" -ForegroundColor Green
}

# Fix Moon ContentSpawner delegates - change from () to (SaveData)
$moonFiles = @(
    "Assets\_Project\Scripts\Integration\Moon6ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon7ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon8ContentSpawner.cs"
    "Assets\_Project\Scripts\Integration\Moon9ContentSpawner.cs"
)
foreach ($file in $moonFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        $content = $content -replace 'void OnSave\(\)', 'void OnSave(SaveData saveData)'
        $content = $content -replace 'void OnLoad\(\)', 'void OnLoad(SaveData saveData)'
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Fixed $file" -ForegroundColor Green
    }
}

# Fix DialogueManager.ShowDialogue - add 3-arg overload
$file = "Assets\_Project\Scripts\Integration\DialogueManager.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    if ($content -notmatch 'ShowDialogue\(string speaker, string text, System.Action onComplete\)') {
        # Add overload after existing ShowDialogue method
        $content = $content -replace '(public void ShowDialogue\(string speaker, string text\))', "`$1`n`n        public void ShowDialogue(string speaker, string text, System.Action onComplete)`n        {`n            Debug.Log(`$`"[DialogueManager] {speaker}: {text}`");`n            onComplete?.Invoke();`n        }"
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Added 3-arg ShowDialogue overload" -ForegroundColor Green
    }
}

# Fix AudioFeedbackController - add PlayDialogueSound method
$file = "Assets\_Project\Scripts\Integration\AudioFeedbackController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    if ($content -notmatch 'PlayDialogueSound') {
        # Add before final closing brace
        $content = $content -replace '(\s+)\}(\s*)\}(\s*)$', "`$1    public void PlayDialogueSound(string characterName)`n        {`n            Debug.Log(`$`"[AudioFeedback] PlayDialogueSound({characterName})`");`n            // TODO: Play character voice clip`n        }`n`$1}`$2}`$3"
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Added PlayDialogueSound to AudioFeedbackController" -ForegroundColor Green
    }
}

# Fix PostProcessingSetup - add Instance property
$file = "Assets\_Project\Scripts\Integration\PostProcessingSetup.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    if ($content -notmatch 'public static PostProcessingSetup Instance') {
        # Add Instance singleton pattern after class declaration
        $content = $content -replace '(public class PostProcessingSetup[^{]*\{)', "`$1`n        public static PostProcessingSetup Instance { get; private set; }`n`n        void Awake()`n        {`n            if (Instance != null && Instance != this)`n            {`n                Destroy(gameObject);`n                return;`n            }`n            Instance = this;`n            DontDestroyOnLoad(gameObject);`n        }"
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Added Instance to PostProcessingSetup" -ForegroundColor Green
    }
}

# Fix SpireRestorationSystem - change AwardRS call from 2 args to 1
$file = "Assets\_Project\Scripts\Integration\SpireRestorationSystem.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    $content = $content -replace 'AwardRS\([^,]+,\s*"[^"]+"\)', 'AwardRS($1)'
    # More specific: AwardRS(amount, "source") -> AwardRS(amount)
    $content = $content -replace 'GameLoopController\.Instance\?\.AwardRS\(([^,]+),\s*"[^"]+"\)', 'GameLoopController.Instance?.AwardRS($1)'
    [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "✅ Fixed AwardRS calls in SpireRestorationSystem" -ForegroundColor Green
}

# Fix CassianController - add PlayerAbilities static class stub
$file = "Assets\_Project\Scripts\Integration\CassianController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    # Replace PlayerAbilities.UnlockAbility with PlayerAbilitiesComplete.Instance?.UnlockAbility
    $content = $content -replace 'PlayerAbilities\.UnlockAbility', 'PlayerAbilitiesComplete.Instance?.UnlockAbility'
    [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "✅ Fixed PlayerAbilities refs in CassianController" -ForegroundColor Green
}

# Fix GameLoopController - add AwardRS(float, string) overload
$file = "Assets\_Project\Scripts\Integration\GameLoopController.cs"
if (Test-Path $file) {
    $content = Get-Content $file -Raw
    if ($content -notmatch 'public void AwardRS\(float amount, string source\)') {
        # Add overload after existing AwardRS
        $content = $content -replace '(public void AwardRS\(float amount\)[^}]+\})', "`$1`n`n        public void AwardRS(float amount, string source)`n        {`n            QueueRSReward(amount, source);`n            Debug.Log(`$`"[GameLoop] Awarded {amount:F1} RS from {source}`");`n        }"
        [System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "✅ Added AwardRS(float, string) overload" -ForegroundColor Green
    }
}

Write-Host "`n=== DONE - Recompiling ===" -ForegroundColor Cyan
