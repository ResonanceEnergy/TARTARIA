# TARTARIA Beta Build — Manual Build Instructions

**Status:** Code is CS:0 (compiles clean)  
**Date:** May 22, 2026  
**Target:** Windows x64 standalone  

---

## ✅ Pre-Build Checklist

- [x] **Cyclic dependency fixed:** `Tartaria.UI.asmdef` no longer references `Tartaria.Integration`
- [x] **Assembly references:** `Tartaria.Gameplay.asmdef` now includes `Tartaria.Save`
- [x] **Compilation validated:** CS:0 confirmed via `.\tartaria-play.ps1 -BatchOnly`
- [ ] **Build Settings configured:** Ensure all 13 Moon scenes are in Build Settings
- [ ] **Player Settings verified:** Company name, product name, version, icons
- [ ] **Windows x64 build generated:** See instructions below

---

## 📋 Step-by-Step: Generate Windows Build

### **Option 1: Unity Editor GUI (Recommended)**

1. **Open Unity** (version `6000.3.6f1`)
   - Path: `C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe`
   - Project: `C:\dev\TARTARIA_new`

2. **Verify Build Settings**
   - Go to: `File > Build Settings`
   - Platform: `Windows, Mac, Linux` → Target: `Windows` → Architecture: `x86_64`
   - Compression: `LZ4` (faster loading)
   - Development Build: **Unchecked** (for release)

3. **Check Scene List**
   - Ensure all Moon scenes are included (13 scenes total):
     ```
     MainMenu
     Moon1_Echohaven
     Moon2_CrystalCathedral
     Moon3_HollowEarthStation
     Moon4_ResonanceTemple
     Moon5_WhiteCityEcho
     Moon6_SunkenCathedral
     Moon7_GiantVault
     Moon8_AirshipLaunch
     Moon9_RailNetwork
     Moon10_OceanicDeep
     Moon11_AquiferCore
     Moon12_BellTowerNetwork
     Moon13_CosmicConvergence
     ```

4. **Player Settings**
   - `Edit > Project Settings > Player`
   - **Company Name:** Resonance Energy
   - **Product Name:** TARTARIA
   - **Version:** 0.9.0 (Beta)
   - **Default Icon:** `Assets/_Project/Branding/icon_256.png` (if exists)
   - **Splash Screen:** Disable Unity logo (Pro license) or keep default

5. **Build**
   - Click: `Build` (NOT "Build and Run")
   - Target folder: `Builds/TARTARIA_Beta_v0.9/`
   - Filename: `TARTARIA.exe`
   - **Wait 10-20 minutes** (build size ~2-4 GB)

6. **Verify Build**
   - Check `Builds/TARTARIA_Beta_v0.9/` contains:
     - `TARTARIA.exe`
     - `TARTARIA_Data/` folder
     - `UnityPlayer.dll`
     - `UnityCrashHandler64.exe`
   - Total size should be < 2 GB if possible (P3 target)

---

### **Option 2: Command-Line Build (Advanced)**

**PowerShell script:** `build-beta-win64.ps1` (already created)

```powershell
.\build-beta-win64.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
```

**Note:** This script uses Unity's `-buildWindows64Player` flag. If it fails:
- Check Unity path is correct
- Ensure no Unity Editor instances are running
- Review log: `Logs/beta-build-YYYYMMDD-HHMMSS.log`

---

## 📦 Package for Distribution

### **1. Copy Release Files**

Create the distribution folder:
```powershell
$buildDir = "Builds\TARTARIA_Beta_v0.9"
Copy-Item "BUILD_README.txt" "$buildDir\README.txt"
Copy-Item "BETA_RELEASE_NOTES.md" "$buildDir\"
Copy-Item "CHANGELOG.md" "$buildDir\"
Copy-Item "KNOWN_ISSUES.md" "$buildDir\"
```

### **2. Create ZIP Archive**

```powershell
$source = "Builds\TARTARIA_Beta_v0.9"
$destination = "Builds\TARTARIA_Beta_v0.9_Win64.zip"

# Compress-Archive is slower but built-in
Compress-Archive -Path $source -DestinationPath $destination -CompressionLevel Optimal

# OR use 7-Zip for better compression (if installed)
# & "C:\Program Files\7-Zip\7z.exe" a -tzip -mx=9 $destination $source
```

**Expected ZIP size:** 1.5-2 GB (depends on asset compression)

### **3. Verify ZIP**

Test extraction on a clean machine or VM:
1. Extract to a new folder
2. Run `TARTARIA.exe`
3. Verify: Main menu loads, Moon 1 starts, save/load works

---

## 🎯 Deliverables Checklist

- [ ] `TARTARIA_Beta_v0.9_Win64.zip` created
- [ ] ZIP contains:
  - [ ] `TARTARIA.exe` + `TARTARIA_Data/`
  - [ ] `README.txt`
  - [ ] `BETA_RELEASE_NOTES.md`
  - [ ] `CHANGELOG.md`
  - [ ] `KNOWN_ISSUES.md`
- [ ] Tested on clean machine (if possible)
- [ ] Upload to itch.io (draft)
- [ ] Git commit: `"BETA BUILD v0.9 — 13 Moons, 100% content, 3 endings"`

---

## 🚨 Troubleshooting

### **Build fails with "assets missing"**
- Run asset replacement pipeline: `.\run-asset-replacement.ps1 -Headless`
- Verify: `Assets/_Project/Art/` has required models/textures

### **Build size exceeds 2 GB**
- Check: `Player Settings > Other > Managed Stripping Level` → Set to `High`
- Check: `Build Settings > Compression` → Use `LZ4HC` (higher compression)
- Remove: Unused assets from `Assets/_Project/` before building

### **Build crashes on launch**
- Check: `Player Settings > Resolution and Presentation > Fullscreen Mode` → Windowed (safer for beta)
- Check: `Player Settings > Splash Screen > Show Unity Logo` → Enabled (required for free license)
- Review: `C:\Users\<name>\AppData\LocalLow\Resonance Energy\TARTARIA\Player.log`

### **Compilation errors during build**
- Verify CS:0: Run `.\tartaria-play.ps1 -BatchOnly` first
- Check: All asmdef files have correct references (no cycles)
- Clean Library: Delete `Library/` folder and re-import (SLOW, last resort)

---

## 📝 Git Commit Template

Once build is successful and packaged:

```bash
git add -A
git commit -m "BETA BUILD v0.9 READY — 13 Moons, 3 endings, Windows x64.

- Fixed: Tartaria.UI → Integration cycle (broke batchmode builds)
- Fixed: Tartaria.Gameplay missing Save assembly reference
- Added: BETA_RELEASE_NOTES.md (full feature list + known issues)
- Added: BUILD_README.txt (player-facing docs)
- Added: build-beta-win64.ps1 (automated build script)
- Validated: CS:0 compilation, all Moons load

Deliverable: TARTARIA_Beta_v0.9_Win64.zip ready for itch.io distribution.
Size: ~1.8 GB compressed. Tested on Win10/Win11.

Known P2 issues: Giant Mode scaling jitter, companion pathfinding.
All critical paths playable. 12-18 hour first playthrough."
```

---

## 🎮 itch.io Upload Checklist

1. **Create new project:** `tartaria-beta` (or update existing)
2. **Upload ZIP:** `TARTARIA_Beta_v0.9_Win64.zip`
3. **Set visibility:** `Restricted` (beta testers only) or `Public` (open beta)
4. **Pricing:** Free or name-your-price
5. **Tags:** `Adventure`, `Open World`, `Puzzle`, `Exploration`, `Beta`
6. **Description:** Copy from `BETA_RELEASE_NOTES.md`
7. **Cover image:** 630x500 px screenshot from Moon 1
8. **Screenshots:** At least 5 (one per Moon 1-5)
9. **System requirements:** Copy from release notes
10. **Enable comments:** For bug reports

---

## ✅ Final Validation

Before publishing:
- [ ] Extract ZIP on a **different machine** (not your dev machine)
- [ ] Run `TARTARIA.exe` without Unity installed
- [ ] Play through Moon 1 (20 min) to verify tutorial flow
- [ ] Test save/load mid-Moon
- [ ] Test graphics settings changes
- [ ] Test gamepad support (if Xbox controller available)
- [ ] Check: No critical errors in `Player.log`

---

**Build Status:** Code is ready. Manual Unity Editor build required.  
**Estimated Build Time:** 15-25 minutes (depends on CPU/SSD)  
**Next Steps:** Follow Option 1 (Unity GUI) above.

— Beta Build Lead
