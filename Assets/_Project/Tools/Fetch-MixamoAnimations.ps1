# Fetch-MixamoAnimations.ps1
# Downloads a curated set of Mixamo animations as FBX-for-Unity.
# REQUIRES: Adobe SSO bearer token (Mixamo has no public API).
#
# How to get the token:
#   1. Open https://www.mixamo.com in Chrome/Edge → log in.
#   2. F12 → Application tab → Cookies → mixamo.com
#   3. Find "ADOBE_DEVICE_TOKEN" or open any animation, click Download
#      and copy the "Authorization: Bearer xxxx" header from the
#      Network tab (filter: api.mixamo.com).
#   4. Pass it: pwsh .\Fetch-MixamoAnimations.ps1 -BearerToken "eyJxxx..."
#
# The Adobe Mixamo API is unofficial; endpoints can change without notice.
#
param(
    [Parameter(Mandatory=$true)]
    [string]$BearerToken,
    [string]$OutDir = "Assets\_Project\Models\Animations",
    [string]$Character = "Y Bot"   # default Mixamo character for in-place anims
)
$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }

# Curated Tartaria starter set: locomotion, dig, combat, idle, jump, fall.
# IDs from Mixamo's animation catalogue (stable since 2018).
$animations = @(
    @{ Id = "8d22bfde-d695-11e1-b2e0-22000a48b6f3"; Name = "Idle";          InPlace = $true  }
    @{ Id = "63ed5cb8-c81c-11e2-820e-22000aae0e85"; Name = "Walk";          InPlace = $true  }
    @{ Id = "bb24da30-d692-11e1-91d3-1231392238dd"; Name = "Run";           InPlace = $true  }
    @{ Id = "37d61ddc-d97f-11e1-bdf5-22000aa01b6f"; Name = "Jump";          InPlace = $true  }
    @{ Id = "f7d6cfb6-d96c-11e1-bdf5-22000aa01b6f"; Name = "Fall";          InPlace = $true  }
    @{ Id = "26ea49c5-d8c1-11e1-91d3-1231392238dd"; Name = "Dig";           InPlace = $true  }
    @{ Id = "d8c0ca1f-d8c0-11e1-91d3-1231392238dd"; Name = "SwordSlash";    InPlace = $true  }
    @{ Id = "20c6c08c-d97e-11e1-bdf5-22000aa01b6f"; Name = "TakeDamage";    InPlace = $true  }
)

$headers = @{
    "Authorization" = "Bearer $BearerToken"
    "X-Api-Key"     = "mixamo2"
    "Accept"        = "application/json"
}

foreach ($anim in $animations) {
    $outFile = Join-Path $OutDir "$($anim.Name).fbx"
    if (Test-Path $outFile) { Write-Host "  SKIP $($anim.Name) (exists)" -ForegroundColor DarkGray; continue }

    Write-Host "  EXPORT $($anim.Name)..." -ForegroundColor Cyan
    $body = @{
        character_id    = $anim.Id
        type            = "Motion"
        product_name    = $anim.Name
        preferences     = @{
            format       = "fbx7_unity"
            skin         = "false"      # animation only, no mesh
            fps          = "30"
            reducekf     = "0"
            mesh_motionpack = "fbx7"
            mesh_pose    = "0"
        }
    } | ConvertTo-Json -Depth 5

    try {
        # Step 1: queue export
        $export = Invoke-RestMethod -Uri "https://www.mixamo.com/api/v1/animation/retarget" `
            -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 60
        $jobUuid = $export.uuid

        # Step 2: poll until ready
        $ready = $false
        for ($i = 0; $i -lt 60; $i++) {
            Start-Sleep -Seconds 2
            $status = Invoke-RestMethod -Uri "https://www.mixamo.com/api/v1/animation/retarget/$jobUuid/monitor" `
                -Headers $headers -TimeoutSec 30
            if ($status.status -eq "completed") { $ready = $true; break }
            if ($status.status -eq "failed")    { throw "Mixamo export failed" }
        }
        if (-not $ready) { throw "Export poll timeout for $($anim.Name)" }

        # Step 3: download
        Invoke-WebRequest -Uri $status.job_result -OutFile $outFile `
            -Headers @{ "Authorization" = "Bearer $BearerToken" } -TimeoutSec 300
        $size = [math]::Round((Get-Item $outFile).Length / 1KB, 1)
        Write-Host "    OK   $($anim.Name).fbx (${size} KB)" -ForegroundColor Green
    } catch {
        Write-Host "    FAIL $($anim.Name) : $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "         (Token may be expired — refresh from mixamo.com Network tab)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Mixamo fetch complete. Files in: $OutDir" -ForegroundColor Green
Write-Host "Unity will auto-import on next focus." -ForegroundColor DarkGray
