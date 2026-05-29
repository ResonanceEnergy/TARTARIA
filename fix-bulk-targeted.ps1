# Targeted bulk fix for TARTARIA Integration layer
cd C:\dev\TARTARIA_new

Write-Host "`n=== TARTARIA Targeted Fixes ===" -ForegroundColor Cyan

# 1. ArchiveManager delegate signatures (CS0123)
Write-Host "`n[1/10] Fixing ArchiveManager delegates..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\ArchiveManager.cs"
$content = Get-Content $file -Raw
$content = $content -replace 'void HandleBeforeSave\(\)', 'void HandleBeforeSave(SaveData data)'
$content = $content -replace 'void HandleAfterLoad\(\)', 'void HandleAfterLoad(SaveData data)'
$content = $content -replace 'void OnBeforeSave\(\)', 'void OnBeforeSave(SaveData data)'
$content = $content -replace 'void OnAfterLoad\(\)', 'void OnAfterLoad(SaveData data)'
[System.IO.File]::WriteAllText($file, $content)
Write-Host "  ✅ Fixed ArchiveManager" -ForegroundColor Green

# 2. Moon6-9 ContentSpawner OnSave/OnLoad signatures  
$moonFiles = @(6, 7, 8, 9)
foreach ($num in $moonFiles) {
    Write-Host "[$(2+$moonFiles.IndexOf($num))/10] Fixing Moon${num}ContentSpawner..." -ForegroundColor Yellow
    $file = "Assets\_Project\Scripts\Integration\Moon${num}ContentSpawner.cs"
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        $content = $content -replace 'void OnSave\(\)', 'void OnSave(SaveData saveData)'
        $content = $content -replace 'void OnLoad\(\)', 'void OnLoad(SaveData saveData)'
        [System.IO.File]::WriteAllText($file, $content)
        Write-Host "  ✅ Fixed Moon${num}ContentSpawner" -ForegroundColor Green
    }
}

# 3. AudioFeedbackController Camera.main fix
Write-Host "[6/10] Fixing AudioFeedbackController.Camera.main..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\AudioFeedbackController.cs"
$content = Get-Content $file -Raw
$content = $content -replace 'Tartaria\.Camera\.main', 'UnityEngine.Camera.main'
[System.IO.File]::WriteAllText($file, $content)
Write-Host "  ✅ Fixed Camera.main" -ForegroundColor Green

# 4. DialogueManager 3-arg overload
Write-Host "[7/10] Adding DialogueManager 3-arg overload..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\DialogueManager.cs"
$content = Get-Content $file -Raw
if ($content -notmatch 'ShowDialogue\(string speaker, string text, System\.Action onComplete\)') {
    # Find the 2-arg method and add overload after it
    $pattern = '(public void ShowDialogue\(string speaker, string text\)[^\}]+\})'
    $replacement = '$1' + "`n`n        public void ShowDialogue(string speaker, string text, System.Action onComplete)`n        {`n            ShowDialogue(speaker, text);`n            onComplete?.Invoke();`n        }"
    $content = $content -replace $pattern, $replacement
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "  ✅ Added 3-arg ShowDialogue" -ForegroundColor Green
}

# 5. PostProcessingSetup Instance singleton
Write-Host "[8/10] Adding PostProcessingSetup.Instance..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\PostProcessingSetup.cs"
$content = Get-Content $file -Raw
if ($content -notmatch 'public static PostProcessingSetup Instance') {
    # Add Instance property and Awake method
    $pattern = '(public class PostProcessingSetup : MonoBehaviour\s*\{)'
    $replacement = '$1' + "`n        public static PostProcessingSetup Instance { get; private set; }`n`n        void Awake()`n        {`n            if (Instance == null) Instance = this;`n            else if (Instance != this) Destroy(gameObject);`n        }"
    $content = $content -replace $pattern, $replacement
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "  ✅ Added PostProcessingSetup.Instance" -ForegroundColor Green
}

# 6. CassianController PlayerAbilities → PlayerAbilitiesComplete.Instance
Write-Host "[9/10] Fixing CassianController PlayerAbilities..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\CassianController.cs"
$content = Get-Content $file -Raw
$content = $content -replace 'PlayerAbilities\.UnlockAbility', 'PlayerAbilitiesComplete.Instance?.UnlockAbility'
[System.IO.File]::WriteAllText($file, $content)
Write-Host "  ✅ Fixed PlayerAbilities refs" -ForegroundColor Green

# 7. SpireRestorationSystem AwardRS(amount) only
Write-Host "[10/10] Fixing SpireRestorationSystem AwardRS..." -ForegroundColor Yellow
$file = "Assets\_Project\Scripts\Integration\SpireRestorationSystem.cs"
$content = Get-Content $file -Raw
$content = $content -replace 'AwardRS\s*\(\s*([^,)]+)\s*,\s*"[^"]+"\s*\)', 'AwardRS($1)'
[System.IO.File]::WriteAllText($file, $content)
Write-Host "  ✅ Fixed AwardRS calls" -ForegroundColor Green

Write-Host "`n=== All fixes complete ===" -ForegroundColor Green
