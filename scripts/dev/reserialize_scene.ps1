cd C:\dev\TARTARIA_new

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.30f1\Editor\Unity.exe"

Write-Host "Forcing Unity to re-serialize scene as text..." -ForegroundColor Cyan

& "$unityPath" -quit -batchmode -projectPath "$PWD" -executeMethod UnityEditor.AssetDatabase.Refresh -logFile "$PWD\Logs\text_serialize.log"

Write-Host "Checking scene file format..." -ForegroundColor Yellow
$sceneBytes = Get-Content "Assets\_Project\Scenes\Echohaven_VerticalSlice.unity" -Raw -Encoding Byte -TotalCount 100
if ($sceneBytes[0] -eq 37 -and $sceneBytes[1] -eq 89) {
    Write-Host "SUCCESS: Scene is now in YAML text format" -ForegroundColor Green
} else {
    Write-Host "Scene is still binary" -ForegroundColor Red
}
