# TARTARIA — Deployment Guide

**Version:** 1.0.0-beta  
**Unity Version:** 6000.3.6f1  
**Target Platform:** Windows PC (Steam, itch.io)  
**Last Updated:** May 24, 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Pre-Deployment Checklist](#pre-deployment-checklist)
3. [Build Configuration](#build-configuration)
4. [Asset Bundle Optimization](#asset-bundle-optimization)
5. [Localization Export](#localization-export)
6. [Version Numbering](#version-numbering)
7. [Release Checklist](#release-checklist)
8. [Steam Deployment](#steam-deployment)
9. [itch.io Deployment](#itchio-deployment)
10. [Post-Deployment Validation](#post-deployment-validation)

---

## Overview

### Deployment Pipeline

```
Pre-Flight Checks
    ↓
Build Configuration
    ↓
Asset Optimization
    ↓
Localization Export
    ↓
Unity Build (.exe + data)
    ↓
Package Creation (.zip)
    ↓
Platform Upload (Steam/itch.io)
    ↓
Post-Deployment Validation
```

### Build Targets

| Platform | Architecture | Build Type | Compression | Target Size |
|----------|--------------|------------|-------------|-------------|
| **Windows (Primary)** | x64 | IL2CPP | LZ4HC | 1.2 GB |
| **Windows (Debug)** | x64 | Mono | LZ4 | 2.5 GB |

**Future targets:**
- Linux (planned for Q3 2026)
- macOS (planned for Q4 2026)

---

## Pre-Deployment Checklist

### Code Quality Gates

Run automated validation before building:

```powershell
cd C:\dev\TARTARIA_new

# 1. Full compilation check (must be CS:0)
Unity.exe -batchmode -quit -projectPath . -buildTarget Win64 -logFile Logs\compile-check.log
```

**Expected output:**
```
Compilation Result: 0 errors, 0 warnings
EXIT CODE: 0
```

**If errors found:**
- Review `Logs\compile-check.log`
- Fix all C# errors
- Re-run until CS:0 achieved

### Test Suite Execution

```powershell
# 2. Run all automated tests
.\run-automated-tests.ps1
```

**Expected output:**
```
PlayMode Tests: 19/19 PASSED
EditMode Tests: 42/42 PASSED
Total: 61/61 GREEN
```

**If tests fail:**
- Review test output for failure details
- Fix broken tests or update expectations
- Re-run until all tests pass

### Performance Validation

```powershell
# 3. Run performance gates
.\run-moon-tests.ps1 -PerformanceGates
```

**Validates:**
- Avg FPS ≥ 52 (Medium tier, GTX 1070)
- 1% Low FPS ≥ 28
- Peak RAM ≤ 3.6 GB
- Scene load time ≤ 8 seconds

**If gates fail:**
- Profile scene with Unity Profiler
- Optimize hotspots (see [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) Performance section)
- Re-test until all gates pass

### Asset Integrity Check

```powershell
# 4. Validate all ScriptableObject databases
# Menu → Tartaria → Validate All Databases
```

**Checks:**
- QuestDatabase: No missing prerequisites, no duplicate IDs
- ItemDatabase: No missing icons, valid stats
- CraftingRecipes: Valid ingredient references
- DialogueDatabase: No missing character names, valid contexts

**Auto-fix minor issues:**
- Null references → replaced with defaults
- Missing IDs → auto-generated
- Duplicate entries → merged or renamed

### Version Control Status

```bash
# 5. Ensure clean working directory
git status

# Expected: "working tree clean"
# If dirty: commit or stash changes before building
```

---

## Build Configuration

### Player Settings

**Edit → Project Settings → Player:**

#### Company & Product

```
Company Name: Resonance Energy
Product Name: TARTARIA World of Wonder
Default Icon: Assets/_Project/Art/UI/TartariaIcon.png
Default Cursor: Standard
```

#### Resolution & Presentation

```
Fullscreen Mode: Fullscreen Window
Default Width: 1920
Default Height: 1080
Run in Background: ON (for alt-tab resilience)
Display Resolution Dialog: Hidden (use in-game settings)
Resizable Window: ON
Visible in Background: ON
Allow Fullscreen Switch: ON (Alt+Enter support)
Use Flip Model Swap Chain: ON (lower latency)
```

#### Splash Image

```
Show Unity Logo: OFF (licensed Pro removes requirement)
Splash Screen: Custom
  - Logo: Assets/_Project/Art/UI/SplashScreen.png
  - Duration: 2 seconds
  - Background Color: #1a1a1a (dark gray)
```

#### Icon

```
Default Icon: 1024x1024 PNG (high-res for Steam)
Override for Windows: Yes
  - 256x256: TartariaIcon_256.png
  - 128x128: TartariaIcon_128.png
  - 48x48: TartariaIcon_48.png
  - 32x32: TartariaIcon_32.png
  - 16x16: TartariaIcon_16.png
```

#### Other Settings

**Rendering:**
```
Color Space: Linear (HDR support)
Auto Graphics API: ON
Graphics API: DirectX 11 (primary), Vulkan (fallback)
Lightmap Encoding: High Quality
HDR Cubemap Encoding: High Quality
Lightmap Streaming: ON
Streaming Priority: 512
```

**Scripting Backend:**
```
Scripting Backend: IL2CPP (release), Mono (debug)
API Compatibility Level: .NET Standard 2.1
Active Input Handling: Input System Package (new)
Suppress Common Warnings: OFF (enforce zero warnings)
Allow Unsafe Code: OFF
.NET Profile: .NET Standard
Use Incremental GC: ON (reduces GC spikes)
```

**Optimization:**
```
Prebake Collision Meshes: ON
Preload Audio: OFF (use async audio loading)
Optimize Mesh Data: ON (strips unused vertex attributes)
Managed Stripping Level: Medium (balance size vs reflection)
Strip Engine Code: ON (IL2CPP only)
Script Call Optimization: Fast but no exceptions (IL2CPP only)
```

#### Configuration

```
Scripting Define Symbols: UNITY_POST_PROCESSING_STACK_V2, TARTARIA_RELEASE
```

**Debug symbols:**
```
Development Build: OFF (release), ON (debug)
Script Debugging: OFF (release), ON (debug)
Deep Profiling: OFF (performance cost)
```

### Quality Settings

**Edit → Project Settings → Quality:**

#### Levels

**Low (Minimum spec: GTX 1050):**
```
Anti Aliasing: 2x MSAA
Texture Quality: Half Res
Anisotropic Textures: Per Texture
Shadow Resolution: Low (512)
Shadow Distance: 50
Shadow Cascades: No Cascades
LOD Bias: 0.7
VSync: Disabled
Particle Raycast Budget: 256
```

**Medium (Recommended: GTX 1070):**
```
Anti Aliasing: 4x MSAA
Texture Quality: Full Res
Anisotropic Textures: Forced On
Shadow Resolution: Medium (1024)
Shadow Distance: 100
Shadow Cascades: Two Cascades
LOD Bias: 1.0
VSync: Disabled
Particle Raycast Budget: 512
```

**High (GTX 3060):**
```
Anti Aliasing: 8x MSAA
Texture Quality: Full Res
Anisotropic Textures: Forced On
Shadow Resolution: High (2048)
Shadow Distance: 150
Shadow Cascades: Four Cascades
LOD Bias: 1.5
VSync: Disabled
Particle Raycast Budget: 1024
```

**Ultra (RTX 4070+):**
```
Anti Aliasing: 8x MSAA + TAA
Texture Quality: Full Res
Anisotropic Textures: Forced On
Shadow Resolution: Very High (4096)
Shadow Distance: 250
Shadow Cascades: Four Cascades
LOD Bias: 2.0
VSync: Disabled (use frame limiter)
Particle Raycast Budget: 2048
Realtime Reflection Probes: ON
```

### URP Asset Configuration

**Assets/Settings/UniversalRenderPipelineAsset_Medium.asset:**

```
Rendering:
  - Depth Texture: ON
  - Opaque Texture: OFF (performance)
  - Opaque Downsampling: None
  - Terrain Holes: ON

Lighting:
  - Main Light: Per Pixel
  - Main Light Shadows: ON
  - Shadow Resolution: 1024 (Medium), 2048 (High)
  - Shadow Distance: 100m (Medium), 150m (High)
  - Shadow Cascades: 2 (Medium), 4 (High)
  - Soft Shadows: ON
  - Additional Lights: Per Pixel
  - Additional Lights Per Object: 4
  - Additional Light Shadows: OFF (performance)

Post-Processing:
  - Enabled: ON
  - Grading Mode: HDR
  - LUT Size: 32
  - Fast sRGB/Linear Conversions: ON
  - Volume Update Mode: Every Frame
  - Volume Framework Update Mode: Use Pipeline Settings

Adaptive Performance:
  - Enabled: OFF (manual quality management)
```

### Build Settings

**File → Build Settings:**

```
Platform: Windows, Mac, Linux
  - Target Platform: Windows
  - Architecture: x64

Compression Method: LZ4HC (balance)
  - LZ4: Faster build, larger size (~2.5 GB)
  - LZ4HC: Slower build, smaller size (~1.2 GB) ← RELEASE
  - LZMA: Slowest build, smallest size (~800 MB, but slow load)

Development Build: OFF (release build)
Autoconnect Profiler: OFF
Deep Profiling: OFF
Script Debugging: OFF
Scripts Only Build: OFF
```

#### Scene Order

**Scenes in Build:**

1. `Boot` (index 0)
2. `Echohaven_VerticalSlice` (index 1)
3. `Moon2_CrystallineCaverns` (index 2)
4. `Moon3_OrphanTrain` (index 3)
5. `Moon4_StarFort` (index 4)
6. `Moon5_WhiteCity` (index 5)
7. `Moon6_Acoustic` (index 6)
8. `Moon7_Desert` (index 7)
9. `Moon8_Ocean` (index 8)
10. `Moon9_Arctic` (index 9)
11. `Moon10_Volcanic` (index 10)
12. `Moon11_Spectral` (index 11)
13. `Moon12_Crystalline` (index 12)
14. `Moon13_Cosmic` (index 13)

**Boot scene responsibility:**
- Initialize ServiceLocator
- Load player settings
- Check save data
- Transition to main menu or Echohaven (based on save)

---

## Asset Bundle Optimization

### Texture Optimization

**Automated tool:**

```csharp
// Menu → Tartaria → Optimize Textures
// Located in: Assets/_Project/Scripts/Editor/TextureOptimizer.cs
```

**Settings applied:**

| Texture Type | Max Size | Compression | Mipmaps | Quality |
|--------------|----------|-------------|---------|---------|
| **UI Icons** | 512x512 | BC7 (RGBA) | No | High |
| **UI Backgrounds** | 2048x2048 | BC7 (RGBA) | No | High |
| **Character Textures** | 2048x2048 | BC7 (RGBA) | Yes | High |
| **Environment Textures** | 2048x2048 | BC7 (RGB) | Yes | Normal |
| **Normal Maps** | 2048x2048 | BC5 (RG) | Yes | Normal |
| **Props** | 1024x1024 | BC7 (RGB) | Yes | Normal |
| **VFX Textures** | 512x512 | BC7 (RGBA) | Yes | Normal |

**Manual override:**
- Key character faces: 4096x4096 (Lirael, Cassian, Princess Anastasia)
- Cinematic backdrops: 4096x4096
- Low-detail background props: 256x256

### Mesh Optimization

**Guidelines:**

| Mesh Type | Tri Count | Vertex Attributes | LOD Levels |
|-----------|-----------|-------------------|------------|
| **Main Characters** | 30-50K | Position, Normal, Tangent, UV0, UV1, Color | 3 (LOD0, LOD1, LOD2) |
| **Companions (Milo)** | 20-30K | Position, Normal, Tangent, UV0, Color | 3 |
| **NPCs** | 15-25K | Position, Normal, UV0, Color | 2 (LOD0, LOD1) |
| **Buildings (Large)** | 20-40K | Position, Normal, Tangent, UV0, UV1 | 3 |
| **Buildings (Small)** | 5-15K | Position, Normal, UV0 | 2 |
| **Props** | 500-5K | Position, Normal, UV0 | 1 (no LOD) |
| **Collectibles** | 100-1K | Position, Normal, UV0 | 1 |

**Optimization tools:**

```csharp
// Menu → Tartaria → Optimize Meshes
// Removes unused vertex attributes, generates LODs
```

### Audio Optimization

**Compression settings:**

| Audio Type | Format | Sample Rate | Quality | Load Type |
|------------|--------|-------------|---------|-----------|
| **Music** | Ogg Vorbis | 44.1 kHz | 128 kbps | Streaming |
| **Ambient Loops** | Ogg Vorbis | 44.1 kHz | 96 kbps | Streaming |
| **SFX (Long)** | Ogg Vorbis | 44.1 kHz | 64 kbps | Compressed in Memory |
| **SFX (Short)** | PCM | 44.1 kHz | N/A | Decompress on Load |
| **Voice Lines** | Ogg Vorbis | 44.1 kHz | 96 kbps | Compressed in Memory |
| **UI SFX** | PCM | 22.05 kHz | N/A | Decompress on Load |

**Automated tool:**

```csharp
// Menu → Tartaria → Optimize Audio
// Applies compression settings to all audio files
```

### Shader Optimization

**Shader variants:**

```
Current Variants: ~12,000 (after optimization)
  - Main Character Shader: 450 variants
  - Environment Shader: 3,200 variants
  - VFX Shader: 2,100 variants
  - UI Shader: 120 variants
```

**Variant stripping:**

```csharp
// Edit → Project Settings → Graphics → Shader Stripping
// Custom shader variant stripping via ShaderVariantCollection

// Strip unused variants:
// - Directional lightmaps (not used)
// - Lightmap shadows (not used)
// - Dynamic lightmaps (not used)
```

**Preload shader variants:**

```
Assets/_Project/Settings/ShaderVariantCollection.shadervariants
  - Preload at boot (Boot scene)
  - Prevents runtime shader compilation stutters
```

---

## Localization Export

### Supported Languages

**Launch (v1.0):**
- English (en-US) — Primary
- Spanish (es-ES) — Secondary
- French (fr-FR) — Secondary

**Future (v1.1+):**
- German (de-DE)
- Portuguese (pt-BR)
- Russian (ru-RU)
- Japanese (ja-JP)
- Chinese Simplified (zh-CN)

### Localization Workflow

**Step 1: Export source strings**

```csharp
// Menu → Tartaria → Localization → Export Source Strings
// Exports to: Assets/StreamingAssets/Localization/en-US/strings.json
```

**Output format:**

```json
{
  "ui.main_menu.new_game": "New Game",
  "ui.main_menu.continue": "Continue",
  "ui.main_menu.settings": "Settings",
  "quest.moon1_main.title": "Echoes of the Buried City",
  "quest.moon1_main.description": "Discover and restore three buildings in Echohaven",
  "dialogue.milo.moon1.excited": "I SMELL something incredible beneath us!"
}
```

**Step 2: Translate strings**

Send `strings.json` to translators → receive translated files:
- `es-ES/strings.json`
- `fr-FR/strings.json`

**Step 3: Import translated strings**

```csharp
// Menu → Tartaria → Localization → Import Translations
// Place translated files in: Assets/StreamingAssets/Localization/{locale}/
```

**Step 4: Validate translations**

```csharp
// Menu → Tartaria → Localization → Validate All
```

**Checks:**
- All source keys present in translations
- No untranslated strings (empty values)
- UI string length limits (max 50 chars for buttons, 200 for tooltips)
- Encoding (UTF-8)

**Step 5: Test in-game**

```csharp
// In-game: Settings → Language → Select "Español"
// Verify:
// - UI text switches correctly
// - Quest log displays translated titles/descriptions
// - Dialogue subtitles use translated lines
// - No text overflow in UI panels
```

### Localization Build

**Include localization in build:**

```
Build Settings → Player Settings → Other Settings
  - Scripting Define Symbols: LOCALIZATION_ENABLED
```

**Localization files copied to build:**

```
Build/TARTARIA_Data/StreamingAssets/Localization/
  ├── en-US/
  │   └── strings.json
  ├── es-ES/
  │   └── strings.json
  └── fr-FR/
      └── strings.json
```

**Runtime loading:**

```csharp
// LocalizationManager.cs loads appropriate file based on:
// 1. Player settings (if saved)
// 2. System locale (Windows language)
// 3. Fallback to en-US
```

---

## Version Numbering

### Semantic Versioning

**Format:** `MAJOR.MINOR.PATCH-TAG`

**Examples:**
- `1.0.0` — Initial release
- `1.0.1` — Hotfix (bug fixes only)
- `1.1.0` — Minor update (new features, backward compatible)
- `2.0.0` — Major update (breaking changes, new campaign)
- `1.0.0-beta` — Beta build (pre-release testing)
- `1.0.0-alpha` — Alpha build (internal testing)

### Version Update Procedure

**Step 1: Update version in Unity**

```csharp
// Edit → Project Settings → Player → Version
Version: 1.0.0
Bundle Version Code: 10000 (MAJOR*10000 + MINOR*100 + PATCH)
```

**Step 2: Update version in code**

```csharp
// Assets/_Project/Scripts/Core/GameVersion.cs
public static class GameVersion
{
    public const string Version = "1.0.0";
    public const string BuildDate = "2026-05-24";
    public const int BuildNumber = 10000;
}
```

**Step 3: Update CHANGELOG.md**

```markdown
## [1.0.0] - 2026-05-24

### Added
- Moon 1 Echohaven vertical slice
- Core gameplay loop (tune → restore → save)
- Save/load system with corruption recovery
- Gamepad support (DualSense haptics)

### Fixed
- Save data corruption on Alt-F4
- Memory leaks in DialogueManager
- Compilation errors (Unity 6 API migration)

### Changed
- Quest system refactored to use QuestDatabase
- Performance optimization (60 FPS on GTX 1070)
```

**Step 4: Git tag release**

```bash
git tag -a v1.0.0 -m "Release v1.0.0 — Moon 1 Beta"
git push origin v1.0.0
```

### Build Numbering

**Automated build number:**

```powershell
# Build script increments build number in GameVersion.cs
.\build-release.ps1 -IncrementBuildNumber
```

**Build number displayed:**
- Main menu bottom-right: "v1.0.0 (Build 10042)"
- About screen: "TARTARIA v1.0.0 — Build 10042 — 2026-05-24"

---

## Release Checklist

### Pre-Build Validation

- [ ] **Code Quality:** CS:0 (zero compilation errors/warnings)
- [ ] **Tests:** All PlayMode + EditMode tests passing (61/61 GREEN)
- [ ] **Performance:** All perf gates passing (≥52 avg FPS, ≥28 1% low, ≤3.6 GB RAM)
- [ ] **Version:** Version number updated in Unity + GameVersion.cs + CHANGELOG.md
- [ ] **Git:** Working directory clean, version tagged (`git tag v1.0.0`)

### Build Execution

- [ ] **Player Settings:** Configured for release (IL2CPP, LZ4HC, no debug symbols)
- [ ] **Scene List:** All scenes included in correct order (Boot → Echohaven → Moon 2-13)
- [ ] **Asset Optimization:** Textures/meshes/audio compressed
- [ ] **Localization:** All languages exported and validated
- [ ] **Build:** Execute automated build: `.\build-release.ps1`

### Post-Build Validation

- [ ] **Size Check:** Build directory ≤ 2.5 GB (uncompressed), ≤ 1.2 GB (compressed)
- [ ] **Launch Test:** Double-click `TARTARIA.exe` → boots to main menu (< 10s)
- [ ] **Save/Load:** New Game → Save → Quit → Continue (resume from save)
- [ ] **Performance:** Run built .exe → verify 60 FPS in Echohaven
- [ ] **Localization:** Test all languages (Settings → Language)
- [ ] **Controller:** Test gamepad input (Xbox + PlayStation)

### Package Creation

- [ ] **Compression:** Create .zip package: `TARTARIA_v1.0.0_Windows_x64.zip`
- [ ] **README:** Include README.txt with system requirements, controls, credits
- [ ] **EULA:** Include LICENSE.txt (proprietary or MIT, as appropriate)
- [ ] **Virus Scan:** Scan .zip with antivirus (Windows Defender, VirusTotal)

### Distribution

- [ ] **Steam:** Upload to Steamworks (see [Steam Deployment](#steam-deployment))
- [ ] **itch.io:** Upload to itch.io (see [itch.io Deployment](#itchio-deployment))
- [ ] **Backup:** Archive build + source to external storage (NAS, cloud)

---

## Steam Deployment

### Prerequisites

- Steam Partner account (Steamworks access)
- App ID assigned (e.g., `123456`)
- Depots configured (Base Game, DLC placeholders)
- Store page created (capsule art, screenshots, description)

### Steamworks SDK Integration

**Step 1: Install Steamworks.NET**

```
Unity Package Manager → Add package from git URL:
https://github.com/rlabrecque/Steamworks.NET.git#upm
```

**Step 2: Configure Steam App ID**

```
Assets/Plugins/Steamworks.NET/steam_appid.txt
  → Replace "480" with your App ID (e.g., "123456")
```

**Step 3: Add Steamworks initialization**

```csharp
// Assets/_Project/Scripts/Core/SteamManager.cs
using Steamworks;

public class SteamManager : MonoBehaviour
{
    void Awake()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("Steam API initialization failed!");
            return;
        }
        Debug.Log("Steam API initialized successfully");
    }
    
    void OnDestroy()
    {
        SteamAPI.Shutdown();
    }
}
```

**Step 4: Test locally**

- Launch Steam client
- Run `TARTARIA.exe` from build folder
- Verify Steam overlay works (Shift+Tab)

### Upload to Steamworks

**Step 1: Prepare depot files**

```
depot_content/
├── TARTARIA.exe
├── TARTARIA_Data/
├── UnityCrashHandler64.exe
├── UnityPlayer.dll
└── MonoBleedingEdge/ (if Mono build)
```

**Step 2: Create depot manifest (depot_build_123456.vdf)**

```vdf
"DepotBuildConfig"
{
  "DepotID" "123457"
  "ContentRoot" "C:\\Builds\\TARTARIA_v1.0.0\\"
  "FileMapping"
  {
    "LocalPath" "*"
    "DepotPath" "."
    "recursive" "1"
  }
  "FileExclusion" "*.pdb"
  "FileExclusion" "*.log"
}
```

**Step 3: Create app build manifest (app_build_123456.vdf)**

```vdf
"AppBuild"
{
  "AppID" "123456"
  "Desc" "TARTARIA v1.0.0 Beta"
  "BuildOutput" "C:\\BuildOutput\\"
  "ContentRoot" ""
  "SetLive" "default"
  "Preview" "0"
  "Local" ""
  
  "Depots"
  {
    "123457" "depot_build_123457.vdf"
  }
}
```

**Step 4: Upload via SteamCMD**

```powershell
# Download SteamCMD: https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
cd C:\SteamCMD
steamcmd.exe +login <username> +run_app_build ..\app_build_123456.vdf +quit
```

**Step 5: Set build live**

- Open Steamworks → App Admin
- Select "SteamPipe" → "Builds"
- Find uploaded build → Set as "default" branch
- (Optional) Create "beta" branch for testing

### Steam Achievements

**Configure in Steamworks:**

1. Steamworks → App Admin → Stats & Achievements
2. Add achievements (API Name, Display Name, Description, Icon)
3. Export `achievements.json`

**Integrate in Unity:**

```csharp
// Unlock achievement
SteamUserStats.SetAchievement("ACH_MOON1_COMPLETE");
SteamUserStats.StoreStats();
```

### Steam Cloud Saves

**Enable in Steamworks:**

1. Steamworks → App Admin → Cloud
2. Enable Steam Cloud
3. Set quota (e.g., 100 MB per user)

**Integrate in Unity:**

```csharp
// Save to Steam Cloud
byte[] data = File.ReadAllBytes(savePath);
SteamRemoteStorage.FileWrite("save_slot_0.dat", data, data.Length);

// Load from Steam Cloud
int fileSize = SteamRemoteStorage.GetFileSize("save_slot_0.dat");
byte[] data = new byte[fileSize];
SteamRemoteStorage.FileRead("save_slot_0.dat", data, fileSize);
File.WriteAllBytes(savePath, data);
```

---

## itch.io Deployment

### Prerequisites

- itch.io account
- Game page created (https://resonanceenergy.itch.io/tartaria)
- Butler CLI tool installed

### Butler Setup

**Step 1: Install Butler**

```powershell
# Download butler: https://itch.io/docs/butler/installing.html
choco install butler
```

**Step 2: Authenticate**

```powershell
butler login
# Follow browser authentication flow
```

### Upload Build

**Step 1: Package build**

```
TARTARIA_v1.0.0_Windows/
├── TARTARIA.exe
├── TARTARIA_Data/
├── UnityCrashHandler64.exe
├── UnityPlayer.dll
└── README.txt
```

**Step 2: Push to itch.io**

```powershell
butler push TARTARIA_v1.0.0_Windows resonanceenergy/tartaria:windows-x64 --userversion 1.0.0
```

**Arguments:**
- `TARTARIA_v1.0.0_Windows` — Local directory
- `resonanceenergy/tartaria` — itch.io user/game
- `windows-x64` — Channel name
- `--userversion 1.0.0` — Display version

**Step 3: Verify upload**

- Open https://resonanceenergy.itch.io/tartaria/edit
- Check "Upload" tab → "windows-x64" channel present
- Download test build → verify integrity

### itch.io Page Configuration

**Pricing:**
- Free (beta builds)
- $19.99 (release)
- Pay What You Want ($5 minimum)

**Classification:**
- Genre: Adventure, RPG, Puzzle
- Release Status: In development (beta), Released (1.0)
- Accessibility: Colorblind-friendly, Subtitles, Gamepad support

**System Requirements (Windows):**

**Minimum:**
- OS: Windows 10/11 64-bit
- CPU: 4-core 2.5 GHz
- GPU: GTX 1050 / 4 GB VRAM
- RAM: 8 GB
- Storage: 4 GB

**Recommended:**
- OS: Windows 10/11 64-bit
- CPU: 6-core 3.0 GHz
- GPU: GTX 1070 / 8 GB VRAM
- RAM: 16 GB
- Storage: 4 GB SSD

---

## Post-Deployment Validation

### Smoke Tests

**After deployment, run these checks:**

#### 1. Download & Install

- [ ] Download build from Steam/itch.io
- [ ] Extract/install to test directory
- [ ] Verify all files present (exe, data folder, DLLs)

#### 2. Launch & Boot

- [ ] Double-click executable
- [ ] Boot screen appears (< 10s)
- [ ] Main menu loads
- [ ] Version number displayed correctly

#### 3. New Game Flow

- [ ] Click "New Game"
- [ ] Intro cutscene plays (if implemented)
- [ ] Echohaven loads (< 8s)
- [ ] Player spawns correctly
- [ ] HUD visible, controls responsive

#### 4. Save & Load

- [ ] Play for 5 minutes (explore, interact)
- [ ] Verify auto-save triggered (every 10s)
- [ ] Manual save (F5)
- [ ] Quit to main menu
- [ ] Click "Continue" → resumes from save

#### 5. Performance

- [ ] Run for 30 minutes
- [ ] Monitor FPS (should be ≥ 60 on Medium tier)
- [ ] Check RAM usage (≤ 3.6 GB)
- [ ] No memory leaks (RAM stable over time)
- [ ] No crashes or freezes

#### 6. Controller Support

- [ ] Connect Xbox controller
- [ ] Navigate main menu (works)
- [ ] In-game movement + actions (works)
- [ ] Verify button prompts update (keyboard → gamepad)

#### 7. Localization

- [ ] Settings → Language → Español
- [ ] UI switches to Spanish
- [ ] Restart game → Spanish retained
- [ ] Switch back to English (works)

### Monitoring & Analytics

**Track via Steam/itch.io dashboards:**

- **Downloads:** Daily download count
- **Players:** Concurrent players, total installs
- **Playtime:** Avg session length, total hours played
- **Crashes:** Crash reports (Unity Cloud Diagnostics)
- **Refunds:** Refund rate (Steam)

**Unity Analytics integration:**

```csharp
// Track key events
Analytics.CustomEvent("moon_completed", new Dictionary<string, object>
{
    { "moon_id", "moon1" },
    { "playtime_seconds", 1800 },
    { "buildings_restored", 3 }
});
```

### Hotfix Procedure

**If critical bug found post-launch:**

1. **Reproduce** — Verify bug in production build
2. **Fix** — Create hotfix branch (`hotfix/save-corruption-fix`)
3. **Test** — Run full test suite + manual validation
4. **Version** — Increment PATCH version (1.0.0 → 1.0.1)
5. **Build** — Create new release build
6. **Upload** — Push to Steam/itch.io
7. **Notify** — Announce hotfix in community channels

**Hotfix timeline:**
- **P0 (Critical: crashes, save corruption):** 24-48 hours
- **P1 (High: gameplay blockers):** 3-5 days
- **P2 (Medium: balance issues):** 1-2 weeks

---

## Additional Resources

**Documentation:**
- [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) — Development workflows
- [API_REFERENCE.md](API_REFERENCE.md) — Complete API documentation
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — Common issues and solutions

**External:**
- Unity Build Documentation: https://docs.unity3d.com/Manual/BuildSettings.html
- Steamworks Documentation: https://partner.steamgames.com/doc/home
- itch.io Butler: https://itch.io/docs/butler/
- Semantic Versioning: https://semver.org/

---

**Version History:**

- **1.0.0** (May 24, 2026) — Initial deployment guide for beta release
