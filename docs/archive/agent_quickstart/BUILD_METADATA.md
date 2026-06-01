# TARTARIA Beta v0.9 — Build Metadata

**Build Date:** May 22, 2026  
**Unity Version:** 6000.3.6f1  
**Platform:** Windows x64 Standalone  
**Scripting Backend:** Mono (JIT)  
**API Compatibility Level:** .NET Standard 2.1  
**Compilation Status:** CS:0 ✓ (Zero C# errors, zero warnings)  

---

## 📋 Build Configuration

### Unity Project Settings
- **Product Name:** Tartaria
- **Company Name:** Resonance Energy
- **Version:** 0.9.0 (Beta)
- **Bundle Version:** 0.9.0
- **Target Platform:** Windows 64-bit
- **Graphics API:** Direct3D12 (DX12), Vulkan fallback
- **Rendering Pipeline:** Universal Render Pipeline (URP)
- **Color Space:** Linear
- **Scripting Define Symbols:** `UNITY_ENTITIES`, `ENABLE_BURST`

### Performance Targets
- **Target FPS:** 60 FPS (Medium tier: GTX 1070 / RX 580)
- **Min 1% Low FPS:** ≥28 FPS
- **Max RAM Budget:** ≤3.6 GB (gameplay)
- **Max VRAM:** ≤4 GB (textures + meshes)

### Build Options
- **Development Build:** No
- **Script Debugging:** Disabled
- **Compression:** LZ4 (fast decompression)
- **Managed Stripping Level:** Medium

---

## 🗂️ Scene Build List

15 scenes included in this build (in load order):

1. **MainMenu** — Main menu, settings, new game flow
2. **Echohaven** (Moon 1) — Tutorial + first restoration loop
3. **CrystalCathedral** (Moon 2) — Environmental restoration puzzles
4. **HollowEarthStation** (Moon 3) — Underground train network
5. **WhiteCityFountain** (Moon 4) — Water temple amplification
6. **SunkenCathedral** (Moon 5) — Underwater organ restoration
7. **GiantAwakening** (Moon 6) — Player scale transition mechanics
8. **AirshipIntro** (Moon 7) — Continental travel unlocked
9. **AirshipHub** (Moon 8) — Repair sequence + boss prep
10. **TemporalGuardian** (Moon 9) — Prophecy stones + time-shift boss
11. **RailLeviathan** (Moon 10) — Rail network + oceanic boss
12. **AquiferRestoration** (Moon 11) — Planetary water grid
13. **BellTowerHarmonies** (Moon 12) — Frequency harmonization climax
14. **FinalConvergence** (Moon 13) — 3-ending choice sequence
15. **Credits** — Post-game credits roll

---

## 🧩 Code Architecture

### Assembly Structure
- **Tartaria.Core** — Bootstrap, singletons, core systems
- **Tartaria.Gameplay** — Player, inventory, quests, progression
- **Tartaria.AI** — Enemy AI, companion behavior, ECS systems
- **Tartaria.Combat** — Ranged/melee combat, damage calculation
- **Tartaria.Audio** — Procedural SFX, 432Hz generation, AudioManager
- **Tartaria.Camera** — Player camera, cinematic camera, transitions
- **Tartaria.Input** — Input remapping, controller support
- **Tartaria.Save** — Save/load persistence system
- **Tartaria.UI** — Menus, HUD, inventory UI, quest log
- **Tartaria.Integration** — Moon content spawners, quest orchestration
- **Tartaria.Editor** — Build tools, test runners, asset pipelines

### Key Dependencies
- **Unity.Entities** (ECS/DOTS) — Enemy AI, companion behavior, performance-critical systems
- **Unity.Burst** — SIMD-optimized math for ECS systems (partially used)
- **Unity.InputSystem** — Cross-platform input with rebinding
- **Unity.TextMeshPro** — High-quality UI text rendering
- **Unity.Cinemachine** — Camera blending and tracking
- **Unity.Timeline** — Cutscene sequencing

---

## 🔧 Technical Highlights

### Golden Ratio Integration
- **φ = 1.618033988749** — Used in:
  - Procedural audio frequency generation (432Hz × φⁿ)
  - UI layout proportions (16:10 aspect ratio = φ × 10)
  - Enemy spawn patterns (Fibonacci spiral distributions)
  - Terrain generation (fractal noise octaves scaled by φ)

### 432Hz Audio Tuning
- All procedural tones tuned to A4 = 432Hz (vs standard 440Hz)
- Harmonic series: 432Hz, 648Hz, 972Hz, 1458Hz, 2187Hz
- Companion dialogue cues use φ-spaced intervals

### ECS Performance
- Enemy AI runs at 60Hz in ECS `ISystem` jobs
- Burst compilation on critical systems (except `Moon2CrystalEnemyAISystem` — calls managed code)
- Component data stored in archetypes for cache efficiency

### Save System
- **Location:** `%APPDATA%\Tartaria\Saves\`
- **Format:** JSON with gzip compression
- **Coverage:** Player state, inventory, quests, Moon progress, companion unlocks
- **Auto-save:** On Moon transitions, every 5 minutes during gameplay
- **Save slots:** 3 manual slots + 1 auto-save slot

---

## ⚙️ Known Optimizations

### Applied
- [x] **Occlusion culling:** Enabled on Moon 1-4 (large open areas)
- [x] **LOD groups:** Enemy models (3 LOD levels: 100%/50%/25% tris)
- [x] **Texture streaming:** Virtual texturing for terrain (reduces VRAM by ~40%)
- [x] **Particle pooling:** VFX spawned from object pool (ParticleEffectPool.Instance)
- [x] **Audio pooling:** SFX reuse AudioSource components (max 32 concurrent)
- [x] **Addressables:** Late-game assets loaded on-demand (Moon 8-13)

### Planned (Post-Beta)
- [ ] **GPU instancing:** Vegetation rendering (Moon 4-7 forests)
- [ ] **Async scene loading:** Background Moon pre-loads during travel
- [ ] **Shader stripping:** Remove unused URP shader variants (-50MB build size)
- [ ] **Mesh compression:** Reduce vertex data precision (normals, UVs)

---

## 🐛 Known Issues (At Build Time)

### **Resolved in This Build**
- ✅ **Burst error BC1016:** Removed `[BurstCompile]` from `Moon2CrystalEnemyAISystem` (calls managed `PlayerStatusEffects.Instance`)
- ✅ **IL2CPP not installed:** Switched scripting backend from IL2CPP → Mono for this beta build
- ✅ **Unity 6 deprecated API:** Migrated all `FindObjectOfType` → `FindFirstObjectByType`

### **Remaining (Non-Blocking)**
See `BETA_RELEASE_NOTES.md` for full P1/P2/P3 issue list:
- Giant Mode visual scaling jitter (P2)
- Companion pathfinding in dense areas (P2)
- Moon 4-13 placeholder SFX (P3)
- Quest log sorting (P3)

---

## 📊 Build Statistics

*(To be populated after build completes)*

- **Executable Size:** ___ MB
- **Data Folder Size:** ___ MB
- **Total Build Size:** ___ MB
- **File Count:** ___ files
- **Compressed ZIP Size:** ___ MB
- **Build Duration:** ___ minutes
- **Asset Import Time:** ___ minutes
- **Script Compilation Time:** ___ seconds

---

## 🔒 Integrity Verification

### SHA256 Checksums
*(Generate after packaging with `Get-FileHash -Algorithm SHA256`)*

- **Tartaria.exe:** `[CHECKSUM]`
- **UnityPlayer.dll:** `[CHECKSUM]`
- **TARTARIA_Beta_v0.9_Win64.zip:** `[CHECKSUM]`

### Verification Instructions for Testers
1. Download `TARTARIA_Beta_v0.9_Win64.zip`
2. Run PowerShell command:
   ```powershell
   Get-FileHash TARTARIA_Beta_v0.9_Win64.zip -Algorithm SHA256
   ```
3. Compare output hash with official checksum above
4. If hashes match → download is authentic and uncorrupted

---

## 🚀 Deployment Notes

### First-Time Launch
- Unity splash screen: 3 seconds (cannot be disabled in Free tier)
- Asset decompression on first boot: ~30 seconds
- Shader compilation on first scene load: ~10 seconds (per GPU)
- User preferences written to: `%APPDATA%\Tartaria\Preferences\`

### System Compatibility
- **Windows 10/11 64-bit** — Fully supported
- **DirectX 12** — Required (fallback: Vulkan)
- **Visual C++ Runtime** — Bundled in MonoBleedingEdge/
- **No admin rights required** — Portable installation

### Firewall & Antivirus
- Game does NOT require internet connection (fully offline)
- No telemetry, no cloud sync, no DRM
- Some antivirus software may flag Mono runtime (false positive)
- Whitelist directory if necessary: `Build\Windows\`

---

## 📞 Support Channels

### For Beta Testers
- **Bug Reports:** GitHub Issues @ `https://github.com/ResonanceEnergy/TARTARIA/issues`
- **Discord:** `#beta-testing` channel (invite link on itch.io)
- **Email:** `tartaria-beta@resonanceenergy.dev`

### For Developers
- **Build Pipeline:** See `BUILD_GUIDE.md`
- **Troubleshooting:** See `TROUBLESHOOTING.md`
- **Contributing:** See `CONTRIBUTING.md`

---

*Build generated by automated pipeline. See `build-beta.ps1` for full build script.*
