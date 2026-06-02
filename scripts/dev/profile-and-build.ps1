# profile-and-build.ps1
# Headless TARTARIA Moon 1 -> itch.io build with profiler gate.
# Owner: agent/tools (Moon1ItchBuild sprint-3).
#
# Invokes Tartaria.Editor.Moon1ItchBuild.BuildItchWithProfilerGate which:
#   1. Reflectively runs the sprint-2 Moon1ProfilerBaseline (if present)
#   2. Parses Builds/itch_moon1/profile_report.md
#   3. Aborts if avg frame time > 16.6 ms
#   4. Builds StandaloneWindows64 -> Builds/itch_moon1/TARTARIA_Moon1.exe
#   5. Zips the folder -> TARTARIA_Moon1_itch.zip
#   6. Logs SHA256 of the zip
#
# Exit codes from the Unity batchmode pass through:
#   0 = success
#   1 = build pipeline / exception
#   2 = perf gate refused

$ErrorActionPreference = "Stop"

$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$project  = "C:\dev\TARTARIA_new"
$outDir   = Join-Path $project "Builds\itch_moon1"
$logPath  = Join-Path $outDir "build.log"

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

if (-not (Test-Path $unityExe)) {
    Write-Error "Unity not found at $unityExe -- update the path in profile-and-build.ps1"
    exit 1
}

Write-Host "================================================================"
Write-Host "TARTARIA Moon 1 -> itch.io build (profiler-gated, headless)"
Write-Host "Project: $project"
Write-Host "Unity:   $unityExe"
Write-Host "Output:  $outDir"
Write-Host "Log:     $logPath"
Write-Host "================================================================"

& $unityExe `
    -batchmode `
    -quit `
    -nographics `
    -projectPath $project `
    -executeMethod Tartaria.Editor.Moon1ItchBuild.BuildItchWithProfilerGate `
    -logFile $logPath

$unityExit = $LASTEXITCODE
Write-Host "Unity exit code: $unityExit"
Write-Host "Build log:       $logPath"

if ($unityExit -eq 2) {
    Write-Error "Build aborted by profiler gate (avg frame time > 16.6 ms). See $logPath."
    exit 2
}
if ($unityExit -ne 0) {
    Write-Error "Unity batchmode exited non-zero ($unityExit). See $logPath."
    exit $unityExit
}

$zipMatches = @(Get-ChildItem -Path $outDir -Filter *.zip -ErrorAction SilentlyContinue)
if ($zipMatches.Count -gt 0) {
    foreach ($zip in $zipMatches) {
        $hash = Get-FileHash $zip.FullName -Algorithm SHA256
        Write-Host "Zip: $($zip.Name)"
        Write-Host "  Path:   $($zip.FullName)"
        Write-Host "  Size:   $([math]::Round($zip.Length / 1MB, 2)) MB"
        Write-Host "  SHA256: $($hash.Hash)"
    }
} else {
    Write-Error "No zip produced in $outDir -- check $logPath"
    exit 1
}

Write-Host "Done."
exit 0
