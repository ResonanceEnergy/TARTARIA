# scripts/build-itch.ps1
# Headless itch.io Windows build for TARTARIA.
# Wraps Unity CLI -> Tartaria.EditorTools.ItchBuildPipeline.BuildItchWindowsHeadless
# Output:
#   Builds/itch_moon1/TARTARIA.exe
#   Builds/itch_moon1.zip  (+ SHA256 printed in Unity log)
#
# Usage:  .\scripts\build-itch.ps1
# Env override:  $env:UNITY_EDITOR_PATH = "C:\Path\To\Unity.exe"

$ErrorActionPreference = 'Stop'

$candidates = @(
    $env:UNITY_EDITOR_PATH,
    'C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe'
) | Where-Object { $_ -and (Test-Path $_) }

if (-not $candidates -or $candidates.Count -eq 0) {
    Write-Error "Unity.exe not found. Set `$env:UNITY_EDITOR_PATH or install 6000.3.6f1."
    exit 127
}

$unity = $candidates[0]
$projectPath = (Resolve-Path "$PSScriptRoot\..").Path

Write-Host "[build-itch] Unity:       $unity"
Write-Host "[build-itch] ProjectPath: $projectPath"
Write-Host "[build-itch] Method:      Tartaria.EditorTools.ItchBuildPipeline.BuildItchWindowsHeadless"

& $unity `
    -batchmode `
    -nographics `
    -quit `
    -projectPath $projectPath `
    -executeMethod Tartaria.EditorTools.ItchBuildPipeline.BuildItchWindowsHeadless `
    -logFile -

exit $LASTEXITCODE
