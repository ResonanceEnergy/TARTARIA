# TARTARIA DLC — Build & Deployment Pipeline
## Automated DLC Distribution for Steam, Epic, Console

**Version:** 1.0.0  
**Updated:** 2026-05-24  
**Owner:** Live Ops Agent 10

---

## Table of Contents

1. [Overview](#overview)
2. [Build Stages](#build-stages)
3. [Platform-Specific Pipelines](#platform-specific-pipelines)
4. [CI/CD Automation](#cicd-automation)
5. [Testing & Validation](#testing--validation)
6. [Rollback Procedures](#rollback-procedures)

---

## Overview

TARTARIA DLC uses a **multi-stage build pipeline** to:

- Build base game + DLC independently
- Package DLC for each platform (Steam, Epic, Xbox, PlayStation, Switch)
- Deploy DLC without patching base game (hot updates)
- Run automated tests (integration, save compat, performance)
- Rollback on failure (blue-green deployment)

### Key Features

- **Parallel Builds:** Base game + all DLC build simultaneously (CI agents)
- **Incremental Builds:** Only rebuild changed DLC (detect via git diff)
- **Platform Isolation:** Steam depot != Epic bundle != Console package
- **Beta Channels:** Test DLC on beta branch before public release

---

## Build Stages

### Stage 1: Pre-Build Validation

**Duration:** ~2 minutes  
**Goal:** Catch errors before expensive builds

```bash
#!/bin/bash
# validate_dlc.sh

# 1. Check Unity project compiles
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod EditorScripts.CompileCheck \
  -logFile Logs/pre_build_compile.log

# 2. Validate DLC manifests
python Scripts/validate_manifests.py --all

# 3. Check Addressables integrity
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod AddressablesBuildScript.ValidateGroups \
  -logFile Logs/addressables_validation.log

# 4. Verify asset references (no missing prefabs)
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod AssetValidator.CheckReferences \
  -logFile Logs/asset_references.log
```

**Failure Conditions:**
- Compilation errors → abort build
- Invalid manifest.json → abort build
- Missing Addressables groups → abort build
- Broken asset references → abort build

---

### Stage 2: Base Game Build

**Duration:** ~15 minutes (Windows), ~20 minutes (Linux)  
**Goal:** Build base game executable (Moons 1-13)

```bash
#!/bin/bash
# build_base_game.sh

Unity.exe -batchmode -quit \
  -projectPath . \
  -buildTarget StandaloneWindows64 \
  -executeMethod BuildScript.BuildBaseGame \
  -logFile Logs/build_base_win64.log
```

**Output:**
- `Build/Windows/TARTARIA.exe` (base game, no DLC)
- `Build/Windows/TARTARIA_Data/` (core assets)
- `Build/Windows/StreamingAssets/` (Addressables catalogs)

**Build Settings:**
- Compression: LZ4HC (faster decompression)
- Scripting Backend: IL2CPP (better performance, smaller build)
- .NET: .NET Standard 2.1 (C# 9.0 features)

---

### Stage 3: DLC Addressables Build

**Duration:** ~3 minutes per DLC  
**Goal:** Build Addressables bundles for each DLC

```bash
#!/bin/bash
# build_dlc_addressables.sh

# Build DLC 11 (Moon 14)
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod AddressablesBuildScript.BuildDLC \
  -dlcId DLC_11_CELESTIAL \
  -buildTarget StandaloneWindows64 \
  -logFile Logs/build_dlc11_addressables.log

# Build DLC 12 (Moon 15)
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod AddressablesBuildScript.BuildDLC \
  -dlcId DLC_12_ARCANE \
  -buildTarget StandaloneWindows64 \
  -logFile Logs/build_dlc12_addressables.log

# Continue for DLC 13-20...
```

**Output (per DLC):**
- `Build/DLC_11/bundles/*.bundle` (asset bundles)
- `Build/DLC_11/DLC_11_Catalog.json` (Addressables catalog)
- `Build/DLC_11/manifest.json` (DLC metadata)

---

### Stage 4: Platform Packaging

#### 4A. Steam (Windows, Linux, macOS)

**Duration:** ~5 minutes  
**Tool:** `steamcmd` (Steam Pipe)

```bash
#!/bin/bash
# package_steam_dlc.sh

# Upload DLC 11 to Steam depot
steamcmd +login $STEAM_USER $STEAM_PASS \
  +build_set_steam_depot \
    app_id 2100000 \
    depot_id 2100011 \
    content_path Build/DLC_11/ \
  +quit

# Set DLC 11 live on Steam (beta branch first)
steamcmd +login $STEAM_USER $STEAM_PASS \
  +set_steam_branch beta DLC_11 \
  +quit
```

**Steam Depot Structure:**
```
TARTARIA (AppID 2100000)
  └─ Base Game Depot (2100001)
  └─ DLC 11 Depot (2100011) ← separate download
  └─ DLC 12 Depot (2100012)
  ...
```

#### 4B. Epic Games Store

**Duration:** ~8 minutes  
**Tool:** Epic Online Services (EOS) CLI

```bash
#!/bin/bash
# package_epic_dlc.sh

# Upload DLC 11 to Epic CDN
eos-cli upload \
  --app-id tartaria \
  --item-id DLC_11_CELESTIAL \
  --path Build/DLC_11/ \
  --build-version 1.0.0

# Set DLC 11 live on Epic (beta channel first)
eos-cli publish \
  --item-id DLC_11_CELESTIAL \
  --channel beta
```

#### 4C. Xbox (GDK)

**Duration:** ~10 minutes  
**Tool:** Xbox GDK (xbapp.exe)

```bash
#!/bin/bash
# package_xbox_dlc.sh

# Package DLC 11 as Xbox .xvc container
xbapp package \
  --input Build/DLC_11/ \
  --output Build/Xbox/DLC_11.xvc \
  --dlc \
  --parent-app-id 9NBLGGH2JHXJ

# Upload to Xbox Partner Center
xbapp upload \
  --sandbox RETAIL \
  --xvc Build/Xbox/DLC_11.xvc
```

#### 4D. PlayStation (PS5 SDK)

**Duration:** ~12 minutes  
**Tool:** PlayStation SDK (orbis-pub-cmd)

```bash
#!/bin/bash
# package_ps5_dlc.sh

# Package DLC 11 as PS5 .pkg
orbis-pub-cmd img_create \
  --input Build/DLC_11/ \
  --output Build/PS5/DLC_11.pkg \
  --passcode $PS5_PASSCODE \
  --content-id UP1234-CUSA12345_00-DLC11CELESTIAL00

# Upload to PlayStation DevNet
orbis-pub-cmd img_upload \
  --pkg Build/PS5/DLC_11.pkg \
  --devnet-account $DEVNET_USER
```

#### 4E. Nintendo Switch

**Duration:** ~15 minutes  
**Tool:** Nintendo SDK (AuthoringTool)

```bash
#!/bin/bash
# package_switch_dlc.sh

# Package DLC 11 as Switch .nsp
AuthoringTool.exe create_aoc \
  --input Build/DLC_11/ \
  --output Build/Switch/DLC_11.nsp \
  --aoc-index 1 \
  --parent-app-id 01007EF00XXXX000

# Upload to Nintendo Developer Portal
NintendoSDK upload \
  --nsp Build/Switch/DLC_11.nsp \
  --environment PROD
```

---

### Stage 5: CDN Deployment

**Duration:** ~5 minutes  
**Goal:** Upload Addressables bundles to CDN (for Steam/Epic remote loading)

```bash
#!/bin/bash
# deploy_to_cdn.sh

# Upload DLC 11 bundles to AWS CloudFront
aws s3 sync Build/DLC_11/bundles/ \
  s3://tartaria-cdn/dlc/v1.0.0/DLC_11/bundles/ \
  --acl public-read \
  --cache-control "max-age=31536000"

# Upload catalog (no cache, always fetch latest)
aws s3 cp Build/DLC_11/DLC_11_Catalog.json \
  s3://tartaria-cdn/dlc/v1.0.0/DLC_11_Catalog.json \
  --acl public-read \
  --cache-control "max-age=60"

# Invalidate CloudFront cache (force refresh)
aws cloudfront create-invalidation \
  --distribution-id E1234567890ABC \
  --paths "/dlc/v1.0.0/DLC_11_Catalog.json"
```

---

### Stage 6: Post-Build Testing

**Duration:** ~20 minutes  
**Goal:** Automated smoke tests before release

```bash
#!/bin/bash
# test_dlc_build.sh

# 1. Integration test: base game + DLC
python Tests/integration_test_dlc11.py --headless

# 2. Save compatibility test
python Tests/save_compat_test.py --dlc DLC_11

# 3. Performance test (load times, memory)
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod TestRunner.RunPerformanceTests \
  -dlcId DLC_11_CELESTIAL \
  -logFile Logs/perf_test_dlc11.log

# 4. Asset integrity check (no missing refs)
Unity.exe -batchmode -quit \
  -projectPath . \
  -executeMethod TestRunner.ValidateLoadedDLC \
  -dlcId DLC_11_CELESTIAL \
  -logFile Logs/integrity_test_dlc11.log
```

**Pass Criteria:**
- All integration tests green (100%)
- Save migration works (v18 → v19)
- Load time < 5s (Moon 14 scene)
- Memory usage < 250 MB (DLC 11 zone)
- Zero missing asset references

---

## CI/CD Automation

### GitHub Actions Workflow

**Trigger:** Push to `dlc/moon14` branch or tag `v1.1.0-dlc11`

```yaml
name: Build & Deploy DLC 11

on:
  push:
    branches:
      - dlc/moon14
    tags:
      - v1.1.0-dlc11

env:
  UNITY_VERSION: 6000.3.6f1
  DLC_ID: DLC_11_CELESTIAL

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Pre-Build Validation
        run: ./Scripts/validate_dlc.sh

  build-base:
    needs: validate
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Base Game
        run: ./Scripts/build_base_game.sh

  build-dlc:
    needs: validate
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dlc: [DLC_11_CELESTIAL, DLC_12_ARCANE, DLC_13_PRIMAL]
    steps:
      - uses: actions/checkout@v3
      - name: Build DLC Addressables
        run: ./Scripts/build_dlc_addressables.sh ${{ matrix.dlc }}

  package-steam:
    needs: [build-base, build-dlc]
    runs-on: ubuntu-latest
    steps:
      - name: Upload to Steam
        run: ./Scripts/package_steam_dlc.sh
        env:
          STEAM_USER: ${{ secrets.STEAM_USER }}
          STEAM_PASS: ${{ secrets.STEAM_PASS }}

  package-epic:
    needs: [build-base, build-dlc]
    runs-on: ubuntu-latest
    steps:
      - name: Upload to Epic
        run: ./Scripts/package_epic_dlc.sh
        env:
          EPIC_API_KEY: ${{ secrets.EPIC_API_KEY }}

  deploy-cdn:
    needs: build-dlc
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to CDN
        run: ./Scripts/deploy_to_cdn.sh
        env:
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_KEY }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET }}

  test:
    needs: deploy-cdn
    runs-on: ubuntu-latest
    steps:
      - name: Run Integration Tests
        run: ./Scripts/test_dlc_build.sh

  release:
    needs: test
    runs-on: ubuntu-latest
    if: startsWith(github.ref, 'refs/tags/')
    steps:
      - name: Promote to Public
        run: |
          steamcmd +set_steam_branch default DLC_11
          eos-cli publish --channel live
```

---

## Testing & Validation

### Test Matrix (Before Release)

| Test Case | Platform | Expected Result |
|-----------|----------|-----------------|
| Base game only (no DLC) | Steam, Epic | Moon 14 portal gated, upsell shown |
| Base + DLC 11 | Steam, Epic | Moon 14 unlocked, loads in < 5s |
| DLC 11 on fresh save | All | Moon 14 available after Act 1 completion |
| DLC 11 on existing save (v18) | All | Save migrates to v19, Moon 14 unlocked |
| Uninstall DLC 11 | Steam | Moon 14 portal shows "Download DLC" |
| Offline mode | All | DLC 11 assets cached, no network calls |
| Poor network (3G sim) | All | Streaming pauses, resumes when signal improves |
| DLC 11 without base v1.0+ | All | Error message: "Update base game to v1.0.0" |

### Automated Tests (pytest)

```python
# Tests/integration_test_dlc11.py

import pytest
import unity_test_runner

def test_dlc11_ownership_gate():
    """Verify DLC gate blocks non-owners."""
    game = unity_test_runner.start_game(dlc_owned=False)
    game.player.move_to("Moon14Portal")
    assert game.ui.is_shown("DLCUpsellPanel")
    assert game.player.current_zone != "Moon14_Celestial"

def test_dlc11_load_time():
    """Verify Moon 14 loads in < 5 seconds."""
    game = unity_test_runner.start_game(dlc_owned=True)
    start_time = time.time()
    game.load_zone("Moon14_Celestial")
    load_time = time.time() - start_time
    assert load_time < 5.0, f"Moon 14 took {load_time:.2f}s to load"

def test_save_migration_v18_to_v19():
    """Verify save migrates when loading DLC 11."""
    save = unity_test_runner.load_save("test_save_v18.json")
    assert save.version == 18
    game = unity_test_runner.start_game(save=save, dlc_owned=True)
    assert game.save.version == 19
    assert "Moon14SaveBlock" in game.save.blocks
```

---

## Rollback Procedures

### Scenario: DLC 11 Breaks Base Game

**Symptoms:**
- Players report crashes when entering Moon 14
- Save files corrupted (Moon14SaveBlock invalid)
- Memory leak in DLC asset loading

**Rollback Steps (< 15 minutes):**

```bash
#!/bin/bash
# rollback_dlc11.sh

# 1. Revert CDN to previous version
aws s3 sync s3://tartaria-cdn/dlc/v1.0.0-backup/DLC_11/ \
  s3://tartaria-cdn/dlc/v1.0.0/DLC_11/ \
  --acl public-read

# 2. Invalidate CloudFront cache (force old bundles)
aws cloudfront create-invalidation \
  --distribution-id E1234567890ABC \
  --paths "/dlc/v1.0.0/DLC_11/*"

# 3. Rollback Steam depot
steamcmd +login $STEAM_USER $STEAM_PASS \
  +set_steam_branch default DLC_11_v1.0.0-backup \
  +quit

# 4. Rollback Epic
eos-cli rollback \
  --item-id DLC_11_CELESTIAL \
  --build-version 1.0.0-backup

# 5. Notify players (Steam announcement)
steamcmd +post_announcement \
  "DLC 11 temporarily rolled back due to technical issues. Fix incoming."
```

**Blue-Green Deployment (Prevention):**

- Always deploy to **beta channel** first
- Run 24-hour soak test with beta testers
- Monitor crash reports, performance metrics
- If green → promote to public
- If red → fix in dev, redeploy to beta

---

## Platform-Specific Notes

### Steam

- **Depot Isolation:** Each DLC has separate depot (incremental downloads)
- **Beta Branches:** `beta`, `experimental`, `default` (public)
- **Automatic Updates:** Steam auto-downloads DLC when owned
- **Rollback:** Steam keeps 2 previous depots (instant rollback)

### Epic Games Store

- **Item-Based DLC:** Each DLC is an EOS item (separate entitlement)
- **CDN Hosting:** Bundles hosted on Epic's CloudFront (fast)
- **Manual Download:** Players must click "Install DLC" button
- **Rollback:** Manual via EOS CLI (slower than Steam)

### Console (Xbox, PlayStation, Switch)

- **Certification:** 7-10 day approval process (plan ahead!)
- **Package Size Limits:** Xbox = 50 GB, PS5 = 100 GB, Switch = 32 GB
- **Patching:** DLC cannot patch base game (strict isolation)
- **Rollback:** Submit emergency patch (7-day turnaround)

---

## Deployment Schedule (v1.1.0 - v2.0.0)

| DLC # | Moon | Release Date | Build Deadline | Cert Deadline (Console) |
|-------|------|--------------|----------------|-------------------------|
| 11 | 14 | 2026-09-15 | 2026-09-01 | 2026-08-15 |
| 12 | 15 | 2026-11-01 | 2026-10-15 | 2026-10-01 |
| 13 | 16 | 2027-01-15 | 2027-01-01 | 2026-12-15 |
| 14 | 17 | 2027-03-01 | 2027-02-15 | 2027-02-01 |
| 15 | 18 | 2027-05-01 | 2027-04-15 | 2027-04-01 |
| 16 | 19 | 2027-07-01 | 2027-06-15 | 2027-06-01 |
| 17 | 20 | 2027-09-01 | 2027-08-15 | 2027-08-01 |
| 18 | 21 | 2027-11-01 | 2027-10-15 | 2027-10-01 |
| 19 | 22 | 2028-01-01 | 2027-12-15 | 2027-12-01 |
| 20 | 23 | 2028-03-01 | 2028-02-15 | 2028-02-01 |

**Cadence:** Bi-monthly releases (2-month dev cycles)

---

## References

- [Steam Partner Documentation](https://partner.steamgames.com/doc/store/application/dlc)
- [Epic Online Services SDK](https://dev.epicgames.com/docs/services/en-US/index.html)
- [Xbox GDK Documentation](https://learn.microsoft.com/en-us/gaming/gdk/)
- [PlayStation 5 SDK](https://www.playstation.com/en-us/dev/)
- [Nintendo Switch SDK](https://developer.nintendo.com/)
- [DLC_ADDRESSABLES_ARCHITECTURE.md](DLC_ADDRESSABLES_ARCHITECTURE.md) — Asset bundling strategy

---

**Status:** ✅ **Pipeline Ready** — Automated CI/CD for DLC 11-20  
**Next Steps:**  
1. Configure GitHub Actions secrets (Steam, Epic, AWS)  
2. Test beta deployment (DLC 11 → Steam beta branch)  
3. Run 24-hour soak test with beta testers  
4. Promote to public after validation
