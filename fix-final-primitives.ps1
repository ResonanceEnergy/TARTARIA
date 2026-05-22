cd C:\dev\TARTARIA_new

$file = "Assets\_Project\Scripts\Integration\EchohavenContentSpawner.cs"
$content = Get-Content $file -Raw

# Head skeleton primitives (4 fixes)
$content = $content -replace `
    'var headSkull = GameObject\.CreatePrimitive\(PrimitiveType\.Sphere\);\s+headSkull\.name = "Skull";', `
    'var headSkull = new GameObject("Skull");'

$content = $content -replace `
    'var headJaw = GameObject\.CreatePrimitive\(PrimitiveType\.Cube\);\s+headJaw\.name = "Jaw";', `
    'var headJaw = new GameObject("Jaw");'

$content = $content -replace `
    'var headEyeL = GameObject\.CreatePrimitive\(PrimitiveType\.Sphere\);\s+headEyeL\.name = "EyeL";', `
    'var headEyeL = new GameObject("EyeL");'

$content = $content -replace `
    'var headEyeR = GameObject\.CreatePrimitive\(PrimitiveType\.Sphere\);\s+headEyeR\.name = "EyeR";', `
    'var headEyeR = new GameObject("EyeR");'

# Marker/nameplate primitives (4 fixes)
$content = $content -replace `
    'var markerBg = GameObject\.CreatePrimitive\(PrimitiveType\.Quad\);\s+markerBg\.name = "Background";', `
    'var markerBg = new GameObject("Background");'

$content = $content -replace `
    'var markerFrame = GameObject\.CreatePrimitive\(PrimitiveType\.Quad\);\s+markerFrame\.name = "Frame";', `
    'var markerFrame = new GameObject("Frame");'

$content = $content -replace `
    'var markerGlow = GameObject\.CreatePrimitive\(PrimitiveType\.Quad\);\s+markerGlow\.name = "Glow";', `
    'var markerGlow = new GameObject("Glow");'

$content = $content -replace `
    'var markerText = GameObject\.CreatePrimitive\(PrimitiveType\.Quad\);\s+markerText\.name = "TextHolder";', `
    'var markerText = new GameObject("TextHolder");'

# Add mesh components for head skull (after SetParent line)
$content = $content -replace `
    '(var headSkull = new GameObject\("Skull"\);\s+headSkull\.transform\.SetParent\(headRoot\.transform\);\s+headSkull\.transform\.localScale = new Vector3\(0\.7f, 0\.7f, 0\.7f\);)', `
    "`$1`n            var skullMF = headSkull.AddComponent<MeshFilter>();`n            skullMF.mesh = Resources.GetBuiltinResource<Mesh>(""Sphere.fbx"");`n            headSkull.AddComponent<MeshRenderer>();`n            headSkull.AddComponent<SphereCollider>();"

# Add mesh components for head jaw
$content = $content -replace `
    '(var headJaw = new GameObject\("Jaw"\);\s+headJaw\.transform\.SetParent\(headRoot\.transform\);\s+headJaw\.transform\.localPosition = new Vector3\(0f, -0\.25f, 0\.1f\);\s+headJaw\.transform\.localScale = new Vector3\(0\.5f, 0\.2f, 0\.4f\);)', `
    "`$1`n            var jawMF = headJaw.AddComponent<MeshFilter>();`n            jawMF.mesh = Resources.GetBuiltinResource<Mesh>(""Cube.fbx"");`n            headJaw.AddComponent<MeshRenderer>();`n            headJaw.AddComponent<BoxCollider>();"

# Add mesh components for head eyeL
$content = $content -replace `
    '(var headEyeL = new GameObject\("EyeL"\);\s+headEyeL\.transform\.SetParent\(headRoot\.transform\);\s+headEyeL\.transform\.localPosition = new Vector3\(-0\.2f, 0\.1f, 0\.3f\);\s+headEyeL\.transform\.localScale = new Vector3\(0\.15f, 0\.15f, 0\.15f\);)', `
    "`$1`n            var eyeLMF = headEyeL.AddComponent<MeshFilter>();`n            eyeLMF.mesh = Resources.GetBuiltinResource<Mesh>(""Sphere.fbx"");`n            headEyeL.AddComponent<MeshRenderer>();`n            headEyeL.AddComponent<SphereCollider>();"

# Add mesh components for head eyeR
$content = $content -replace `
    '(var headEyeR = new GameObject\("EyeR"\);\s+headEyeR\.transform\.SetParent\(headRoot\.transform\);\s+headEyeR\.transform\.localPosition = new Vector3\(0\.2f, 0\.1f, 0\.3f\);\s+headEyeR\.transform\.localScale = new Vector3\(0\.15f, 0\.15f, 0\.15f\);)', `
    "`$1`n            var eyeRMF = headEyeR.AddComponent<MeshFilter>();`n            eyeRMF.mesh = Resources.GetBuiltinResource<Mesh>(""Sphere.fbx"");`n            headEyeR.AddComponent<MeshRenderer>();`n            headEyeR.AddComponent<SphereCollider>();"

# Add mesh components for markers (Quad meshes, no colliders)
$content = $content -replace `
    '(var markerBg = new GameObject\("Background"\);\s+markerBg\.transform\.SetParent\(nameplateRoot\.transform, false\);\s+markerBg\.transform\.localScale = new Vector3\(2f, 0\.4f, 1f\);)', `
    "`$1`n            var bgMF = markerBg.AddComponent<MeshFilter>();`n            bgMF.mesh = Resources.GetBuiltinResource<Mesh>(""Quad.fbx"");`n            markerBg.AddComponent<MeshRenderer>();"

$content = $content -replace `
    '(var markerFrame = new GameObject\("Frame"\);\s+markerFrame\.transform\.SetParent\(nameplateRoot\.transform, false\);\s+markerFrame\.transform\.localPosition = new Vector3\(0f, 0f, -0\.01f\);\s+markerFrame\.transform\.localScale = new Vector3\(2\.1f, 0\.45f, 1f\);)', `
    "`$1`n            var frameMF = markerFrame.AddComponent<MeshFilter>();`n            frameMF.mesh = Resources.GetBuiltinResource<Mesh>(""Quad.fbx"");`n            markerFrame.AddComponent<MeshRenderer>();"

$content = $content -replace `
    '(var markerGlow = new GameObject\("Glow"\);\s+markerGlow\.transform\.SetParent\(nameplateRoot\.transform, false\);\s+markerGlow\.transform\.localPosition = new Vector3\(0f, 0f, 0\.01f\);\s+markerGlow\.transform\.localScale = new Vector3\(1\.8f, 0\.35f, 1f\);)', `
    "`$1`n            var glowMF = markerGlow.AddComponent<MeshFilter>();`n            glowMF.mesh = Resources.GetBuiltinResource<Mesh>(""Quad.fbx"");`n            markerGlow.AddComponent<MeshRenderer>();"

$content = $content -replace `
    '(var markerText = new GameObject\("TextHolder"\);\s+markerText\.transform\.SetParent\(nameplateRoot\.transform, false\);\s+markerText\.transform\.localPosition = new Vector3\(0f, 0f, 0\.02f\);\s+markerText\.transform\.localScale = new Vector3\(1\.9f, 0\.38f, 1f\);)', `
    "`$1`n            var textMF = markerText.AddComponent<MeshFilter>();`n            textMF.mesh = Resources.GetBuiltinResource<Mesh>(""Quad.fbx"");`n            markerText.AddComponent<MeshRenderer>();"

# Save with UTF-8 BOM
[System.IO.File]::WriteAllText($file, $content, (New-Object System.Text.UTF8Encoding($true)))

Write-Host "√ Final 8 primitives fixed in Echohaven!" -ForegroundColor Green
Write-Host "`nVerifying..."
$remaining = (Select-String -Path $file -Pattern "CreatePrimitive" | Measure-Object).Count
Write-Host "Remaining primitives in file: $remaining" -ForegroundColor $(if($remaining -eq 0){'Green'}else{'Red'})
