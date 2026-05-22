# TARTARIA Beta v0.9 — Release Notes

**Build Date:** May 22, 2026  
**Platform:** Windows x64  
**Status:** Beta Release Candidate  

---

## 🎮 What's in This Beta

### **13 Moons of Content**
Complete playable campaign spanning 13 interconnected regions:
- **Moon 1:** Echohaven (The Awakening) — Tutorial + first discoveries
- **Moon 2:** Crystal Cathedral — Environmental restoration puzzles  
- **Moon 3:** Hollow Earth Station — Underground train network
- **Moon 4-7:** Mid-game expansion — Water temples, White City, Sunken Cathedral, Giant awakening
- **Moon 8-10:** Continental scale — Airship travel, rail networks, oceanic threats
- **Moon 11-13:** Endgame climax — Aquifer restoration, bell tower harmonies, final convergence

### **Core Features**
- **4 Companion Characters:** Milo, Lirael, Anastasia, Korath (with unique abilities)
- **3 Distinct Endings:** Light/Synthesis/Dark paths based on player choices
- **Resonance Scanning System:** 432Hz frequency-based discovery mechanic
- **Save/Load System:** Full persistence across all Moons and player state
- **Procedural Audio:** 80+ SFX generated using golden ratio harmonics
- **Quest System:** 50+ quests with branching objectives
- **Inventory & Progression:** 10-slot inventory, XP/leveling, stat bonuses

### **Technical Highlights**
- Built in Unity 6000.3.6f1 with Universal Render Pipeline (URP)
- ECS (Entity Component System) for performance-critical systems
- Golden Ratio (φ = 1.618033988749) integrated into gameplay, visuals, and audio
- 432Hz tuning for all procedural audio
- Adaptive LOD and occlusion culling for open-world performance

---

## ⚠️ Known Issues (P2/P3)

### **High Priority (P2)**
- **Giant Mode visual scaling:** Player scale transitions may jitter on low-end hardware
- **Companion pathfinding:** Occasional stuck behavior in dense forests  
- **Save file migration:** Beta saves may not be compatible with v1.0 final release

### **Medium Priority (P3)**
- **Moon 4-13 custom SFX:** Some late-game audio still using placeholder tones (functional but not final mix)
- **Cutscene camera:** Cinematic sequences may not respect player-configured FOV
- **Quest log sorting:** Completed quests not visually separated from active quests
- **Performance spikes:** First load of each Moon may cause 1-2 second hitches

### **Low Priority (Polish)**
- **Tutorial tooltips:** Some interactions lack hover hints  
- **Localization:** English only in this beta
- **Achievements:** System implemented but no Steam integration yet

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

## 🐛 Reporting Bugs & Feedback

### **Before Reporting**
1. Check the [Known Issues](#️-known-issues-p2p3) section above
2. Verify you meet minimum system requirements
3. Update your GPU drivers to the latest version

### **Bug Report Template**
When reporting issues, please include:
- **Moon/Location:** Where did the issue occur?
- **Steps to Reproduce:** What were you doing when it happened?
- **Expected vs Actual:** What should have happened vs what actually happened?
- **Save File:** Attach your save file if possible (found in `%APPDATA%\Tartaria\Saves\`)
- **System Specs:** GPU model, RAM, OS version
- **Screenshot/Video:** Visual proof helps immensely

### **Feedback Channels**
- **GitHub Issues:** [github.com/ResonanceEnergy/TARTARIA/issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
- **Discord:** `#beta-testing` channel (invite link on itch.io page)
- **Email:** tartaria-beta@resonanceenergy.dev

---

## 🎯 What to Test

### **Critical Paths**
- [ ] Complete Moon 1 tutorial sequence (15-20 minutes)
- [ ] Trigger at least one companion unlock (Milo in Moon 1)
- [ ] Save and reload in the middle of a Moon
- [ ] Reach Moon 3 and use the train system
- [ ] Test all 3 ending paths (requires ~10-15 hours per path)

### **Edge Cases**
- [ ] Die during a boss fight — does respawn work correctly?
- [ ] Fill inventory to 10/10 slots — what happens when picking up items?
- [ ] Fast travel between Moons — any crashes or loading issues?
- [ ] Switch resolution/graphics settings mid-game
- [ ] Alt+Tab during cutscenes

---

## 🗺️ Estimated Playtime
- **First-time playthrough:** 12-18 hours (single ending)
- **100% completion (all 3 endings):** 35-45 hours
- **Moon 1 (tutorial):** 20-30 minutes
- **Speed run (any% single ending):** ~6-8 hours (estimated, not tested)

---

## 📝 Controls

### **Keyboard & Mouse (Default)**
- **WASD:** Movement
- **Space:** Jump
- **E:** Interact / Scan
- **Q:** Open Resonance Scanner
- **Tab:** Inventory
- **J:** Quest Log
- **M:** Map
- **Esc:** Pause Menu
- **Mouse:** Camera control
- **Left Click:** Attack (when combat enabled)
- **Right Click:** Block / Aim

### **Gamepad (Xbox)**
- **Left Stick:** Movement
- **Right Stick:** Camera
- **A:** Jump / Confirm
- **B:** Cancel / Dodge
- **X:** Interact / Scan
- **Y:** Open Scanner
- **LB:** Prev Companion
- **RB:** Next Companion
- **View:** Map
- **Menu:** Pause
- **D-Pad Up:** Quest Log
- **D-Pad Down:** Inventory

**Note:** Gamepad controls are fully functional but may lack some visual button prompts.

---

## 🔄 Changelog (Beta v0.9 → v1.0 Roadmap)

### **Planned for v1.0**
- [ ] Final audio pass (complete Moon 4-13 custom SFX)
- [ ] Cutscene polish (camera interpolation, lip sync)
- [ ] Performance optimization (target 60fps on GTX 1070)
- [ ] Tutorial refinement based on beta feedback
- [ ] Companion AI improvements (pathfinding, combat behavior)
- [ ] Achievement integration (Steam)
- [ ] Localization (French, German, Spanish, Japanese)
- [ ] New Game+ mode (carry over companion unlocks + cosmetics)

---

## ❤️ Credits & Thanks

**Core Team:**
- **Design & Systems:** [Your Name / Studio]
- **Procedural Audio:** Golden Ratio Harmonics Engine
- **Asset Sources:** KayKit (CC0), AmbientCG textures, Mixamo animations

**Beta Testers:**
- Special thanks to early playtesters who helped shape this build!

---

**Thank you for playing TARTARIA Beta v0.9!**  
Your feedback will directly influence the final release.

— The Resonance Energy Team
