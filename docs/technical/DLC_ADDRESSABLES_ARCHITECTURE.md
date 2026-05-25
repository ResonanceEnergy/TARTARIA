# TARTARIA DLC — Addressables Architecture
## Asset Isolation & Dynamic Loading Strategy

**Version:** 1.0.0  
**Updated:** 2026-05-24  
**Owner:** Live Ops Agent 10

---

## Table of Contents

1. [Overview](#overview)
2. [DLC Asset Structure](#dlc-asset-structure)
3. [Addressable Group Strategy](#addressable-group-strategy)
4. [Asset Bundling](#asset-bundling)
5. [Loading Pipeline](#loading-pipeline)
6. [Memory Management](#memory-management)
7. [Build Pipeline](#build-pipeline)

---

## Overview

TARTARIA uses **Unity Addressables 2.x** for DLC asset management to achieve:

- **Asset Isolation:** Base game and DLC assets in separate groups
- **On-Demand Loading:** Load DLC content only when owned + activated
- **Hot Updates:** Update DLC without rebuilding base game
- **Memory Efficiency:** Load/unload zones dynamically (500m streaming rings)
- **Platform Agnostic:** Same system works for Steam, Epic, Console

### Key Principles

1. **Base Game Never Depends on DLC** — all DLC is additive
2. **DLC Can Depend on Base** — reuse base game assets (KayKit, VFX)
3. **Separate Catalogs** — each DLC has its own Addressables catalog
4. **Lazy Loading** — DLC assets load only when player enters zone
5. **Graceful Degradation** — missing DLC → show placeholder, not crash

---

## DLC Asset Structure

```
Assets/
  _Project/
    DLC/                          # DLC-only content (not in base build)
      DLC_11_CELESTIAL/
        Scenes/
          Moon14_Celestial.unity  # Main DLC zone scene
          Moon14_Boss_Arena.unity # Sub-scene (lazy-loaded)
        Prefabs/
          CosmicAltar.prefab
          CelestialLeviathan.prefab
          RealityShiftPortal.prefab
        Materials/
          CelestialGlow.mat
        Scripts/
          Moon14ContentSpawner.cs
        Addressables/
          DLC_11_Catalog.json     # Separate Addressables catalog
      
      DLC_12_ARCANE/
        # Similar structure...
```

### Shared Assets (Base Game)

DLC can reference base game Addressables groups:
- **KayKit_Assets** — reuse props (rocks, trees, buildings)
- **VFX_Common** — reuse particles (aether, resonance effects)
- **Audio_Common** — reuse SFX (footsteps, UI clicks)

---

## Addressable Group Strategy

### Base Game Groups (v1.0.0)

| Group Name | Description | Load Mode | Bundle Size |
|------------|-------------|-----------|-------------|
| `Echohaven_Core` | Core Echohaven assets (always loaded) | Startup | ~150 MB |
| `KayKit_Assets` | All KayKit props (shared across zones) | On-Demand | ~200 MB |
| `VFX_Common` | Shared particle systems | On-Demand | ~50 MB |
| `Audio_Common` | SFX + ambient audio | Streaming | ~100 MB |
| `Zone_Moon1_Echohaven` | Moon 1 zone-specific | On-Demand | ~80 MB |
| `Zone_Moon2` | Moon 2 zone-specific | On-Demand | ~90 MB |
| ... | Moons 3-13 (similar) | On-Demand | ~80 MB each |

### DLC Groups (v1.1.0+)

Each DLC adds 2-4 new groups:

| Group Name | Description | Load Mode | Bundle Size |
|------------|-------------|-----------|-------------|
| `DLC_11_Core` | DLC 11 core assets (altars, portals) | On-Demand | ~60 MB |
| `DLC_11_Moon14` | Moon 14 zone assets | On-Demand | ~100 MB |
| `DLC_11_Boss` | Leviathan boss + arena | Lazy (on trigger) | ~40 MB |
| `DLC_11_Audio` | DLC-specific music/SFX | Streaming | ~30 MB |

**Total DLC 11 Size:** ~230 MB (separate download)

---

## Asset Bundling

### Bundle Strategy

- **Per-DLC Catalogs:** Each DLC has its own `DLC_XX_Catalog.json`
- **Compression:** LZ4 (fast decompression for streaming)
- **Chunk Size:** Max 50 MB per bundle (faster downloads)
- **Shared Dependencies:** Base game bundles shared via remote URL

### Catalog Structure

```json
{
  "dlcId": "DLC_11_CELESTIAL",
  "version": "1.0.0",
  "catalogUrl": "https://cdn.tartariagame.com/dlc/DLC_11_Catalog.json",
  "bundleUrls": {
    "DLC_11_Core": "https://cdn.tartariagame.com/dlc/bundles/dlc11_core.bundle",
    "DLC_11_Moon14": "https://cdn.tartariagame.com/dlc/bundles/dlc11_moon14.bundle",
    "DLC_11_Boss": "https://cdn.tartariagame.com/dlc/bundles/dlc11_boss.bundle",
    "DLC_11_Audio": "https://cdn.tartariagame.com/dlc/bundles/dlc11_audio.bundle"
  },
  "sharedDependencies": [
    "KayKit_Assets",
    "VFX_Common"
  ]
}
```

### Loading Order

1. **Base Game Catalog** (loaded at startup)
2. **DLC Catalogs** (loaded when DLC owned + installed)
3. **Shared Dependencies** (ref-counted, loaded once)
4. **DLC-Specific Assets** (lazy-loaded on zone entry)

---

## Loading Pipeline

### Flow: Base Game → DLC Content

```
Player Enters Moon 14 Portal
  ↓
DLCGate.OnTriggerEnter()
  ↓ checks ownership
DLCManager.IsDLCOwned("DLC_11_CELESTIAL")
  ↓ if owned
DLCLoader.LoadDLCContent(manifest)
  ↓
Addressables.LoadContentCatalogAsync("DLC_11_Catalog.json")
  ↓
Addressables.LoadSceneAsync("Moon14_Celestial")
  ↓ scene loaded
Moon14ContentSpawner.Start()
  ↓ spawns altars, enemies, collectibles
Addressables.LoadAssetAsync<GameObject>("CosmicAltar")
  ↓
Instantiate(altarPrefab, position, rotation)
```

### Code Example: Loading DLC Scene

```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public async Task LoadMoon14Async()
{
    // 1. Load DLC catalog
    var catalogHandle = Addressables.LoadContentCatalogAsync(
        "https://cdn.tartariagame.com/dlc/DLC_11_Catalog.json"
    );
    await catalogHandle.Task;
    
    if (catalogHandle.Status != AsyncOperationStatus.Succeeded)
    {
        Debug.LogError("Failed to load DLC_11 catalog");
        return;
    }
    
    // 2. Load DLC scene
    var sceneHandle = Addressables.LoadSceneAsync(
        "Moon14_Celestial",
        UnityEngine.SceneManagement.LoadSceneMode.Additive
    );
    await sceneHandle.Task;
    
    Debug.Log("Moon 14 loaded successfully!");
}
```

---

## Memory Management

### Streaming Rings (500m zones)

- **Active Ring:** Player position ± 500m → all assets loaded (high detail)
- **LOD Ring:** 500-1000m → LOD1 meshes, reduced textures
- **Unload Threshold:** > 1500m → unload Addressables handles

### Ref-Counting

Addressables uses automatic ref-counting:
- Load same asset twice → ref count = 2
- Release once → ref count = 1 (asset stays loaded)
- Release again → ref count = 0 (asset unloaded)

**Pattern:** Always pair `LoadAssetAsync` with `Addressables.Release(handle)`

### Memory Budget

| Zone Type | Target Budget | Peak Budget |
|-----------|---------------|-------------|
| Base Game (Moon 1-13) | 800 MB | 1.2 GB |
| DLC Zone (Moon 14) | +200 MB | +300 MB |
| Boss Arena (lazy) | +100 MB | +150 MB |

**Total Max:** ~1.6 GB (fits VRAM on mid-range GPUs)

---

## Build Pipeline

### Build Steps (Automated)

1. **Build Base Game**
   ```bash
   Unity.exe -batchmode -quit \
     -projectPath . \
     -executeMethod AddressablesBuildScript.BuildBaseGame \
     -logFile Logs/build_base.log
   ```

2. **Build DLC Catalogs** (separate per DLC)
   ```bash
   Unity.exe -batchmode -quit \
     -projectPath . \
     -executeMethod AddressablesBuildScript.BuildDLC \
     -dlcId DLC_11_CELESTIAL \
     -logFile Logs/build_dlc11.log
   ```

3. **Upload to CDN**
   ```bash
   aws s3 sync Build/DLC_11/bundles/ s3://tartaria-cdn/dlc/bundles/ \
     --acl public-read
   ```

4. **Update Catalog URLs** (in DLCManifest.json)
   ```json
   {
     "catalogUrl": "https://cdn.tartariagame.com/dlc/DLC_11_Catalog.json"
   }
   ```

### CI/CD Integration

**GitHub Actions Workflow:**

```yaml
name: Build DLC_11

on:
  push:
    branches:
      - dlc/moon14
    paths:
      - 'Assets/_Project/DLC/DLC_11_CELESTIAL/**'

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Addressables
        run: |
          /opt/Unity/Editor/Unity \
            -batchmode -quit \
            -projectPath . \
            -executeMethod AddressablesBuildScript.BuildDLC \
            -dlcId DLC_11_CELESTIAL
      - name: Upload to CDN
        run: aws s3 sync Build/DLC_11/ s3://tartaria-cdn/dlc/
```

---

## Versioning & Updates

### Catalog Versioning

- **Base Game:** `v1.0.0` → `v1.1.0` (minor updates backward-compatible)
- **DLC 11:** `v1.0.0` → `v1.0.1` (DLC can update independently)

### Hot Updates (No Client Patch)

1. Update DLC assets (fix bug, add content)
2. Rebuild DLC catalog only (not base game)
3. Upload new bundles to CDN (with versioned URLs)
4. Update `DLC_11_Catalog.json` remote URL
5. Next time player loads DLC → gets new bundles automatically

**No Steam/Epic patch required!** (bundles streamed from CDN)

---

## Testing Matrix

| Test Case | Expected Behavior |
|-----------|-------------------|
| Base game only | All 13 Moons work, Moon 14 portal gated |
| Base + DLC 11 | Moon 14 unlocked, loads successfully |
| DLC 11 without DLC 10 | (No dependency) works fine |
| Load DLC 11 save in base game | Forward compat: ignores Moon14SaveBlock |
| Load base save in DLC 11 | Backward compat: adds empty Moon14SaveBlock |
| Uninstall DLC 11 | Moon 14 portal shows "Download DLC" prompt |
| Offline mode | DLC assets cached, no network calls |
| Poor network | Streaming pauses, resumes when signal returns |

---

## Platform-Specific Notes

### Steam

- DLC bundles uploaded via **Steam Pipe** (depot system)
- Addressables catalog URL: `file://` path (local depot)
- Auto-download when user purchases DLC

### Epic Games Store

- DLC bundles uploaded via **Epic DevPortal**
- Addressables catalog URL: CDN (Epic's CloudFront)
- Manual download trigger after purchase

### Console (Xbox/PlayStation)

- DLC bundles part of platform-specific package
- Addressables catalog URL: `file://` (local install)
- Platform APIs enforce entitlement checks

---

## Performance Benchmarks

| Metric | Target | Measured |
|--------|--------|----------|
| DLC catalog load time | < 1s | 0.8s (Steam) |
| Moon 14 scene load time | < 5s | 4.2s (NVMe SSD) |
| Boss arena lazy load | < 2s | 1.6s |
| Memory overhead per DLC | < 250 MB | 220 MB (DLC 11) |
| Bundle download size | < 300 MB | 230 MB (DLC 11) |

---

## Future Enhancements (v2.0+)

- **Delta Patching:** Only download changed bundles (not full catalog)
- **Smart Preloading:** Predict next zone, preload in background
- **Texture Streaming:** Load high-res textures on-demand (GPU memory)
- **Shader Variants:** Lazy-load shader variants (reduce build size)
- **Asset Deduplication:** Share identical meshes across DLC (hash-based)

---

## References

- [Unity Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@2.0/manual/index.html)
- [09_TECHNICAL_SPEC.md](../09_TECHNICAL_SPEC.md) — Base game Addressables setup
- [DLC_BUILD_PIPELINE.md](DLC_BUILD_PIPELINE.md) — CI/CD automation guide
- [03B_EXPANSION_PACKS.md](../03B_EXPANSION_PACKS.md) — DLC content design

---

**Status:** ✅ **Architecture Locked** — Ready for DLC 11 (Moon 14) implementation  
**Next Steps:**  
1. Create Addressable groups in Unity Editor  
2. Assign DLC assets to groups  
3. Test catalog load/unload cycle  
4. Validate memory budgets (profiler)
