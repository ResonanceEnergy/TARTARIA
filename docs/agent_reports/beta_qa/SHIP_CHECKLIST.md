# TARTARIA Beta v0.9 — Ship Checklist

**Package:** TARTARIA_Beta_v0.9_Win64.zip  
**Build Date:** May 22, 2026  
**Unity Version:** 6000.3.6f1  
**Platform:** Windows x64  
**Compilation Status:** CS:0 ✓  

---

## 📦 Package Contents

### Build Artifacts
- [ ] Tartaria.exe (standalone executable)
- [ ] Tartaria_Data/ (game data folder)
- [ ] UnityCrashHandler64.exe (crash reporter)
- [ ] UnityPlayer.dll (engine runtime)
- [ ] MonoBleedingEdge/ (Mono runtime)

### Documentation Files
- [ ] README.md (player-facing quick start)
- [ ] BETA_RELEASE_NOTES.md (features + known issues)
- [ ] LICENSE.txt (if applicable)

### Build Stats
- **Total Size:** TBD MB (will populate after build completes)
- **File Count:** TBD files
- **Compression:** ZIP (standard compression)

---

## 🎯 Content Verification

### Moon Coverage
- [x] **Moon 1:** Echohaven (production-ready — full restoration loop, Milo companion, tutorial)
- [x] **Moon 2:** Crystal Cathedral (environmental restoration puzzles)
- [x] **Moon 3:** Hollow Earth Station (underground train network)
- [x] **Moon 4-7:** Mid-game expansion (water temples, White City, Sunken Cathedral, Giant awakening)
- [x] **Moon 8-10:** Continental scale (airship travel, rail networks, boss encounters)
- [x] **Moon 11-13:** Endgame climax (Aquifer restoration, bell tower harmonies, final convergence)

### Core Systems
- [x] **4 Companions:** Milo, Lirael, Anastasia, Korath (unique abilities)
- [x] **3 Endings:** Light / Synthesis / Dark paths
- [x] **Resonance Scanning:** 432Hz frequency discovery mechanic
- [x] **Save/Load:** Full persistence (13 Moons + player state)
- [x] **Inventory:** 10-slot system with persistence
- [x] **Quest System:** 50+ quests across 13 Moons
- [x] **Combat:** Ranged + melee, enemy AI, boss encounters
- [x] **Progression:** XP/leveling, stat bonuses, skill unlocks

### Audio
- [x] **Procedural SFX:** 80+ golden ratio harmonics
- [x] **432Hz Tuning:** All audio frequency-aligned
- [x] **Ambient Zones:** Environmental audio transitions
- [x] **Voice Cues:** Companion dialogue + NPC interactions

---

## ⚠️ Known Issues (P1/P2/P3)

### **Priority 2 (Polish)**
- Giant Mode visual scaling: Player scale transitions may jitter on low-end hardware
- Companion pathfinding: Occasional stuck behavior in dense forests
- Save file migration: Beta saves may not be compatible with v1.0 final release

### **Priority 3 (Minor)**
- Moon 4-13 custom SFX: Some late-game audio still using placeholder tones (functional but not final mix)
- Cutscene camera: Cinematic sequences may not respect player-configured FOV
- Quest log sorting: Completed quests not visually separated from active quests
- Performance spikes: First load of each Moon may cause 1-2 second hitches
- Tutorial tooltips: Some interactions lack hover hints
- Localization: English only in this beta
- Achievements: System implemented but no Steam integration yet

### **Blockers (P0)** 
✅ **NONE** — All P0 blockers resolved in Session 5+6

---

## 💻 System Requirements

### **Minimum**
- **OS:** Windows 10 64-bit (version 1909 or newer)
- **CPU:** Intel Core i5-8400 / AMD Ryzen 5 2600
- **GPU:** NVIDIA GTX 1060 6GB / AMD RX 580 8GB (DX12 required)
- **RAM:** 12 GB
- **Storage:** 5 GB available space
- **DirectX:** Version 12

### **Recommended**
- **OS:** Windows 11 64-bit
- **CPU:** Intel Core i7-10700 / AMD Ryzen 7 3700X
- **GPU:** NVIDIA RTX 3060 / AMD RX 6700 XT
- **RAM:** 16 GB
- **Storage:** 8 GB available space (SSD recommended)

---

## 🚀 Upload Instructions

### **Option 1: itch.io Distribution**

1. **Create itch.io Project**
   - Navigate to: https://itch.io/game/new
   - Title: `TARTARIA — World of Wonder (Beta v0.9)`
   - Classification: `Game`
   - Kind of project: `Downloadable`
   - Pricing: `Free` or `Pay What You Want` (for beta testing)

2. **Upload Build**
   - Upload file: `TARTARIA_Beta_v0.9_Win64.zip`
   - Platform: `Windows`
   - Check: `This file will be played in the browser` → **NO** (standalone)
   - Set as primary download: **YES**

3. **Configure Page**
   - Short description: Use first 3 paragraphs from BETA_RELEASE_NOTES.md
   - Cover image: 630x500px (create from in-game screenshot)
   - Screenshots: 5-8 images showing Moon 1-13 highlights
   - Genre tags: `Adventure`, `Action`, `Puzzle`, `Open World`, `Restoration`, `Fantasy`
   - Release status: `In development` (Beta)
   - Access: `Restricted` (beta key required) or `Public` (open beta)

4. **Beta Keys** (if restricted)
   - Generate 50-100 beta keys for testers
   - Distribute via: Discord, email list, GitHub sponsors

5. **Set Visibility**
   - Draft → **Public** or **Restricted**
   - Copy shareable link for testers

---

### **Option 2: Steam Playtest**

1. **Prerequisites**
   - Steamworks account with app ID registered
   - App configured in Steamworks partner portal
   - Store page in "Coming Soon" status

2. **Upload to Steam**
   - Install **Steamworks SDK** + **SteamPipe** tools
   - Configure `app_build_XXXXXX.vdf` (replace XXXXXX with your app ID):
     ```vdf
     "AppBuild"
     {
         "AppID" "XXXXXX"
         "Desc" "Beta v0.9 Build - May 22 2026"
         "BuildOutput" "C:\\dev\\TARTARIA_new\\SteamBuilds\\output"
         "ContentRoot" "C:\\dev\\TARTARIA_new\\Build\\Windows"
         "SetLive" "beta"
         "Depots"
         {
             "XXXXXX" // Depot ID for Windows
             {
                 "LocalPath" ".\\"
                 "DepotPath" "."
                 "Recursive" "1"
             }
         }
     }
     ```
   - Run: `steamcmd.exe +login <username> +run_app_build C:\path\to\app_build_XXXXXX.vdf +quit`
   - Wait for upload completion (progress shown in console)

3. **Set Beta Branch**
   - In Steamworks: **App Admin → Builds**
   - Find uploaded build, click **Set build live on branch**
   - Branch name: `beta` (password: optional, e.g., `tartaria2026`)
   - Save changes

4. **Configure Playtest**
   - Navigate to: **App Admin → Playtests**
   - Create new playtest event
   - Start date: Immediate or scheduled
   - Playtest access: Open signup or invite-only
   - Publish playtest announcement

5. **Share with Testers**
   - Testers opt-in via Steam Store page → **Request Access**
   - Or send direct Steam keys via Steamworks key generation

---

### **Option 3: Manual Distribution (Google Drive / Dropbox)**

1. **Upload ZIP**
   - Upload `TARTARIA_Beta_v0.9_Win64.zip` to cloud storage
   - Google Drive: Right-click → **Get link** → Set to **Anyone with the link**
   - Dropbox: Right-click → **Share** → Copy link

2. **Share Link**
   - Distribute via: Discord, email, GitHub release page
   - Include: Link + SHA256 checksum (for integrity verification)

3. **Generate Checksum** (optional but recommended)
   ```powershell
   Get-FileHash TARTARIA_Beta_v0.9_Win64.zip -Algorithm SHA256 | Select-Object -ExpandProperty Hash
   ```
   - Include checksum in announcement so testers can verify download integrity

---

## 🧪 Beta Tester Onboarding

### **Welcome Message Template**

```
🎮 Welcome to TARTARIA Beta v0.9!

Thank you for joining the closed beta test. You're about to experience the first 13 Moons 
of the TARTARIA campaign — from the Echohaven awakening through the final convergence.

WHAT TO EXPECT:
- 12-18 hours of gameplay (single ending path)
- 4 companion characters with unique abilities
- 13 interconnected Moons (regions) with distinct mechanics
- Resonance scanning, combat, restoration puzzles, and exploration
- 3 distinct endings (Light, Synthesis, Dark)

WHAT TO TEST:
1. Tutorial clarity — Does Moon 1 teach the core loop effectively?
2. Companion unlocks — Do Milo, Lirael, Anastasia, and Korath feel rewarding?
3. Save/Load reliability — Can you save mid-Moon and resume seamlessly?
4. Performance — What's your average FPS on Moon 1? (Press F3 for stats)
5. Bugs & crashes — Report anything that breaks immersion or progress

HOW TO REPORT BUGS:
- GitHub Issues: [link to repo/issues]
- Discord: #beta-testing channel [invite link]
- Email: tartaria-beta@resonanceenergy.dev

Include: Moon/location, steps to reproduce, screenshot/video, system specs.

KNOWN ISSUES (before reporting):
- See BETA_RELEASE_NOTES.md in game folder
- P2: Giant Mode scaling jitter, companion pathfinding hiccups
- P3: Some late-game SFX are placeholders, quest log sorting

YOUR FEEDBACK MATTERS:
This beta determines what gets polished for v1.0. Tell us what works, 
what doesn't, and what makes you want to keep playing.

Ready to tune the world? 🎵
— The TARTARIA Team
```

---

## 📊 Build Metadata

### **Compilation Report**
- **C# Errors:** 0 (CS:0 ✓)
- **Warnings:** 0 (Unity 6 deprecated API fully migrated)
- **Total Scripts:** ~150 .cs files
- **LOC:** ~60,000 lines (Core + AI + Gameplay + Integration + UI + Tests)

### **Session History**
- **Session 5:** M1/M2 foundation, MainMenuOverlay, haptics/VFX, Unity 6 API migration
- **Session 6:** Moon 2-13 content expansion, quest system, save/load, ECS combat, boss encounters

### **Git Commit Reference**
- Last commit before packaging: `[INSERT COMMIT HASH]`
- Branch: `main`
- Total commits: ~60+ (sessions 1-6)

---

## ✅ Pre-Upload Checklist

- [ ] Build completed successfully (Tartaria.exe exists)
- [ ] CS:0 maintained (zero compilation errors)
- [ ] README.md + BETA_RELEASE_NOTES.md included in ZIP
- [ ] ZIP file created and tested (can extract without errors)
- [ ] File size verified (should be ~1-3 GB compressed)
- [ ] SHA256 checksum generated (for manual distribution)
- [ ] Known issues documented in BETA_RELEASE_NOTES.md
- [ ] System requirements validated against build specs
- [ ] Upload destination chosen (itch.io / Steam / manual)
- [ ] Beta tester onboarding message prepared
- [ ] Feedback channels ready (Discord, GitHub Issues, email)

---

## 📝 Post-Upload Tasks

- [ ] Announce beta launch on social media / Discord / mailing list
- [ ] Monitor initial feedback for P0 blockers (first 48 hours critical)
- [ ] Triage bug reports into P0/P1/P2/P3 buckets
- [ ] Plan hotfix patch v0.9.1 if P0 blockers emerge
- [ ] Collect performance metrics (FPS, RAM usage, crash reports)
- [ ] Schedule playtest debrief session with core testers (1 week post-launch)
- [ ] Update ROADMAP.md with beta feedback integration plan

---

## 🎯 Success Criteria

### **Beta Launch = Success If:**
- [ ] 80%+ testers complete Moon 1 (Echohaven tutorial)
- [ ] Average FPS ≥52 on GTX 1070 / RX 580 (Medium settings)
- [ ] Save/Load round-trip functional (no progress loss)
- [ ] Zero P0 blockers reported in first 72 hours
- [ ] At least 30% of testers reach Moon 5+ (mid-game engagement)
- [ ] Positive sentiment in qualitative feedback (fun > frustration)

### **Red Flags (Require Hotfix):**
- 3+ reports of save file corruption
- Consistent crashes on Moon 1 load (>20% of testers)
- Average FPS <30 on recommended spec hardware
- Companion unlock broken (Milo doesn't spawn after tutorial)
- Main quest progression blocked (can't advance from Moon 1 to Moon 2)

---

## 🚢 Ready to Ship?

**Final Approval:** Package & Ship Agent sign-off  
**Date:** May 22, 2026  
**Status:** ⏳ Awaiting build completion...

Once `Tartaria.exe` is verified and ZIP is created:
✅ **APPROVED FOR DISTRIBUTION**

---

*"The empire never fell — it was only buried. And you are the one conducting its resurrection."*
