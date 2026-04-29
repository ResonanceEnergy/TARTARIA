$f = "Assets\_Project\Scenes\Echohaven_VerticalSlice.unity"
$c = Get-Content $f -Raw

$ids = [ordered]@{
  "StarDome_AetherGlow" = 941783983
  "StarDome_GoldRing" = 1056185459
  "WorldBoundary" = $null
  "PostProcessVolume" = $null
  "DistantPyramid" = 312331357
  "CameraRig" = 786933859
  "GroundPlane" = 408817732
}

$nameToId = @{}
$gos = [regex]::Matches($c, "--- !u!1 &(\d+)\s*\nGameObject:[\s\S]*?(?=\n--- !u!|\z)")
foreach ($g in $gos) {
  $name = [regex]::Match($g.Value, "m_Name:\s*(\S+)").Groups[1].Value
  $id = $g.Groups[1].Value
  $nameToId[$name] = $id
}
$ids["WorldBoundary"] = $nameToId["WorldBoundary"]
$ids["PostProcessVolume"] = $nameToId["PostProcessVolume"]
$ids["EchohavenTerrain"] = $nameToId["EchohavenTerrain"]

foreach ($kv in $ids.GetEnumerator()) {
  $name = $kv.Key; $id = $kv.Value
  if (-not $id) { Write-Host "$name : NOT FOUND"; continue }
  $tf = [regex]::Match($c, "--- !u!4 &\d+\s*\nTransform:[\s\S]*?m_GameObject:\s*\{fileID:\s*$id[\s\S]*?(?=\n--- !u!|\z)")
  if ($tf.Success) {
    $pos = [regex]::Match($tf.Value, "m_LocalPosition:\s*\{([^}]+)\}").Groups[1].Value
    $sca = [regex]::Match($tf.Value, "m_LocalScale:\s*\{([^}]+)\}").Groups[1].Value
    $rot = [regex]::Match($tf.Value, "m_LocalRotation:\s*\{([^}]+)\}").Groups[1].Value
    Write-Host "$name (GO=$id):"
    Write-Host "  pos: $pos"
    Write-Host "  scale: $sca"
    Write-Host "  rot: $rot"
  } else {
    Write-Host "$name (GO=$id): no Transform found"
  }
}

Write-Host "`n=== Inactive GameObjects (m_IsActive: 0) ==="
foreach ($g in $gos) {
  if ($g.Value -match "m_IsActive:\s*0") {
    $name = [regex]::Match($g.Value, "m_Name:\s*(\S+)").Groups[1].Value
    Write-Host "  $name (GO=$($g.Groups[1].Value))"
  }
}

Write-Host "`n=== GroundPlane material ==="
Get-ChildItem Assets\_Project\Materials\Generated -Filter *.mat -ErrorAction SilentlyContinue | ForEach-Object {
  $meta = "$($_.FullName).meta"
  if ((Test-Path $meta) -and (Get-Content $meta -Raw) -match "ed8120753e321b54da615ed0a85070c5") {
    Write-Host "GroundPlane material: $($_.Name)"
    Get-Content $_.FullName | Select-Object -First 60
  }
}
