# Create Missing Character Prefabs
# Generates prefab YAML files for characters that don't exist yet

$existingPrefabs = @("Anastasia", "Milo", "MudGolem", "Player")
$allCharacters = @(
    @{Name="Milo"; KayKit="Char_Ranger"; Scale=1.0; IsEnemy=$false},
    @{Name="Thorne"; KayKit="Char_Barbarian"; Scale=1.0; IsEnemy=$false},
    @{Name="Lirael"; KayKit="Char_Mage"; Scale=1.0; IsEnemy=$false},
    @{Name="Korath"; KayKit="Char_Barbarian"; Scale=2.0; IsEnemy=$false},
    @{Name="Cassian"; KayKit="Char_Rogue_Hooded"; Scale=1.0; IsEnemy=$false},
    @{Name="Anastasia"; KayKit="Char_Mage"; Scale=1.0; IsEnemy=$false},
    @{Name="MudGolem"; KayKit="Skeletons/Char_Skeleton_Warrior"; Scale=1.0; IsEnemy=$true},
    @{Name="ShadowStalker"; KayKit="Skeletons/Char_Skeleton_Rogue"; Scale=1.0; IsEnemy=$true},
    @{Name="CrystalSentry"; KayKit="Skeletons/Char_Skeleton_Minion"; Scale=1.0; IsEnemy=$true}
)

Write-Host "=== Creating Missing Character Prefabs ===" -ForegroundColor Cyan
Write-Host ""

$created = 0

foreach ($char in $allCharacters) {
    $prefabPath = "Assets\_Project\Prefabs\Characters\$($char.Name).prefab"
    
    if (Test-Path $prefabPath) {
        Write-Host "√ $($char.Name) already exists" -ForegroundColor Gray
        continue
    }
    
    # Generate random GUID for prefab
    $guid = [guid]::NewGuid().ToString("N")
    
    # Create minimal prefab YAML
    $prefabContent = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &$($guid.Substring(0,18))
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $($guid.Substring(18,18))}
  - component: {fileID: $($guid.Substring(0,10))$($guid.Substring(20,8))}
  m_Layer: 0
  m_Name: $($char.Name)
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$($guid.Substring(18,18))
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $($guid.Substring(0,18))}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!143 &$($guid.Substring(0,10))$($guid.Substring(20,8))
CharacterController:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $($guid.Substring(0,18))}
  serializedVersion: 3
  m_Height: 2
  m_Radius: 0.5
  m_SlopeLimit: 45
  m_StepOffset: 0.3
  m_SkinWidth: 0.08
  m_MinMoveDistance: 0.001
  m_Center: {x: 0, y: 1, z: 0}
"@

    [System.IO.File]::WriteAllText($prefabPath, $prefabContent, [System.Text.UTF8Encoding]::new($true))
    
    # Create .meta file
    $metaGuid = [guid]::NewGuid().ToString("N")
    $metaContent = @"
fileFormatVersion: 2
guid: $metaGuid
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@

    [System.IO.File]::WriteAllText("$prefabPath.meta", $metaContent, [System.Text.UTF8Encoding]::new($false))
    
    Write-Host "✓ Created $($char.Name)" -ForegroundColor Green
    $created++
}

Write-Host ""
Write-Host "Created $created new prefabs" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT: Run Unity to import these prefabs, then wire to KayKit models" -ForegroundColor Yellow
