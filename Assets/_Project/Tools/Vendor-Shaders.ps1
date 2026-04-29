"""
Vendor-Shaders.ps1 — clone third-party shader/render packs that have no UPM
manifest and copy the relevant Editor/Runtime assets into Assets/_Project/Vendor/.

Each entry:
  - clones to %TEMP%
  - sparse-copies a subset of files into Assets/_Project/Vendor/{name}/

Idempotent: skips already-vendored repos unless -Force is passed.
"""
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Set-Location -Path "C:\dev\TARTARIA_new"

$VendorRoot = "Assets/_Project/Vendor"
New-Item -ItemType Directory -Force -Path $VendorRoot | Out-Null

# name, repo URL, [subpaths to copy] (relative to repo root).
$Repos = @(
    @{ Name = "Cyanilux_ShaderGraphCustomLighting"; Url = "https://github.com/Cyanilux/URP_ShaderGraphCustomLighting.git"; Paths = @("Assets") },
    @{ Name = "Josephy5_VolumetricLightScattering";  Url = "https://github.com/Josephy5/Unity-VolumetricLightScattering-URP.git"; Paths = @("Assets") },
    @{ Name = "Josephy5_StylizedFog";                Url = "https://github.com/Josephy5/Unity-StylizedGradientFog-URP.git"; Paths = @("Assets") },
    @{ Name = "JoshuaLim007_SSR";                    Url = "https://github.com/JoshuaLim007/Unity-ScreenSpaceReflections-URP.git"; Paths = @("Assets") },
    @{ Name = "MasonX_PCSS";                         Url = "https://github.com/TheMasonX/UnityPCSS.git"; Paths = @("Assets") }
)

$tmp = Join-Path $env:TEMP "tartaria_vendor"
New-Item -ItemType Directory -Force -Path $tmp | Out-Null

$ok = 0; $fail = 0; $skip = 0
foreach ($r in $Repos) {
    $dest = Join-Path $VendorRoot $r.Name
    if ((Test-Path $dest) -and -not $Force) {
        Write-Host "  SKIP $($r.Name) (exists)"
        $skip++
        continue
    }
    Write-Host "  CLONE $($r.Name)"
    $clone = Join-Path $tmp $r.Name
    if (Test-Path $clone) { Remove-Item -Recurse -Force $clone }
    try {
        git clone --depth 1 --quiet $r.Url $clone 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "git clone failed" }
        New-Item -ItemType Directory -Force -Path $dest | Out-Null
        foreach ($p in $r.Paths) {
            $src = Join-Path $clone $p
            if (Test-Path $src) {
                Copy-Item -Recurse -Force -Path (Join-Path $src "*") -Destination $dest
            }
        }
        # Strip .git, .github, README, LICENSE leftovers Unity doesn't need.
        Get-ChildItem -Path $dest -Recurse -Force -Include @(".git",".github","node_modules") -Directory -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "    OK"
        $ok++
    } catch {
        Write-Host "    FAIL $($r.Name): $_"
        $fail++
    }
}

Write-Host ""
Write-Host "[Vendor] OK=$ok  Skip=$skip  Fail=$fail"
exit ($(if ($fail -gt 0) {1} else {0}))
