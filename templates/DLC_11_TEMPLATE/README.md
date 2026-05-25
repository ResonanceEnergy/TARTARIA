# DLC TEMPLATE: DLC_11_CELESTIAL (Moon 14)

This template provides the complete structure for creating a new TARTARIA DLC expansion.
Copy this folder to `StreamingAssets/DLC/DLC_XX_NAME/` and customize.

## ✅ DLC Production Checklist

### Phase 1: Pre-Production (Week 1-2)
- [ ] Define DLC scope (zones, quests, mechanics, NPCs)
- [ ] Create DLC manifest.json (set dlcId, moonNumber, dependencies)
- [ ] Design save compatibility (what save blocks needed?)
- [ ] Create content spec doc (story beats, mechanics, art list)
- [ ] Budget estimation (art, audio, code hours)

### Phase 2: Content Creation (Week 3-8)
- [ ] Build Moon 14 zone scenes (Unity Scenes/)
- [ ] Create DLC prefabs (enemies, NPCs, props)
- [ ] Implement Moon14ContentSpawner.cs
- [ ] Add Moon14SaveBlock to SaveData.cs
- [ ] Write quest chains + dialogue trees
- [ ] Create DLC items/equipment (ScriptableObjects)
- [ ] Design DLC boss encounters
- [ ] Build DLC tutorial/intro sequence

### Phase 3: Integration (Week 9-10)
- [ ] Hook DLC spawner to SaveManager events
- [ ] Implement DLC gate at Moon 14 portal
- [ ] Add DLC content to Addressables catalog
- [ ] Create DLC-specific audio assets
- [ ] Implement DLC achievement triggers
- [ ] Test save compatibility (v18 → v19 migration)
- [ ] Verify base game can load DLC saves (forward compat)

### Phase 4: Polish & Testing (Week 11-12)
- [ ] Full playtest (Moon 14 start → completion)
- [ ] Balance pass (combat, rewards, progression)
- [ ] Performance optimization (LOD, occlusion culling)
- [ ] Localization (translate DLC text)
- [ ] Bug fixing (critical + high priority)
- [ ] Create DLC trailer assets (screenshots, video)

### Phase 5: Launch Prep (Week 13-14)
- [ ] Steam DLC setup (App ID, pricing, store page)
- [ ] Epic Games DLC setup (Item ID, pricing)
- [ ] GOG DLC setup (Product ID)
- [ ] Build DLC as Addressables remote bundle
- [ ] Upload DLC to CDN (Steam, Epic, GOG)
- [ ] Test DLC ownership validation (all platforms)
- [ ] QA: Install → Purchase → Play flow
- [ ] Marketing assets (store page, social media)
- [ ] Press review copies

### Phase 6: Launch (Week 15)
- [ ] Release DLC on all platforms
- [ ] Monitor telemetry (gate hits, playtime, completion rate)
- [ ] Community support (Discord, forums, bug reports)
- [ ] Hotfix deployment (if critical issues found)
- [ ] Post-launch analytics report

## 📁 DLC Folder Structure

```
DLC_11_CELESTIAL/
├── manifest.json                  ← DLC metadata (REQUIRED)
├── README.md                      ← This file
├── Scenes/
│   ├── Moon14_CelestialHub.unity
│   ├── Moon14_CosmicAltars.unity
│   └── Moon14_BossFight.unity
├── Prefabs/
│   ├── DLC_11_CELESTIAL_Spawner.prefab  ← ContentSpawner instance
│   ├── Moon14Portal.prefab
│   ├── CosmicAltar.prefab
│   └── Enemies/
│       ├── CelestialGuardian.prefab
│       └── CosmicLeviathan.prefab
├── Scripts/
│   ├── Moon14ContentSpawner.cs   ← Core DLC logic
│   ├── CosmicAltarPuzzle.cs
│   └── CelestialBossFight.cs
├── ScriptableObjects/
│   ├── Moon14QuestDatabase.asset
│   ├── DLC11ItemsDatabase.asset
│   └── CelestialAbilities.asset
├── Audio/
│   ├── Music/
│   └── SFX/
├── Localization/
│   ├── EN_DLC11.csv
│   ├── ES_DLC11.csv
│   └── FR_DLC11.csv
└── Addressables/
    ├── DLC11_Assets.json          ← Remote asset catalog
    └── DLC11_Scenes.json

```

## 🔧 Quick Start

1. **Copy Template:**
   ```bash
   cp -r DLC_11_TEMPLATE/ StreamingAssets/DLC/DLC_14_NEWNAME/
   ```

2. **Edit manifest.json:**
   - Set `dlcId`, `displayName`, `moonNumber`
   - Update `requiredGameVersion`, `requiredSaveVersion`
   - Add Steam/Epic/GOG platform IDs

3. **Create Moon14ContentSpawner.cs:**
   - Copy from Moon10ContentSpawner.cs as reference
   - Update moon number, objectives, rewards
   - Hook into SaveManager events

4. **Add Save Block to SaveData.cs:**
   ```csharp
   // In SaveData.cs
   public Moon14SaveBlock moon14 = new();
   ```

5. **Test in Editor:**
   - Set DLCLoader.skipOwnershipValidation = true (dev mode)
   - Play base game → reach Moon 14 portal
   - Verify DLC content loads

6. **Build Addressables:**
   - Window → Asset Management → Addressables → Groups
   - Create "DLC_11_CELESTIAL" group
   - Mark DLC assets as Addressable
   - Build → New Build → Default Build Script

7. **Test Ownership Gating:**
   - Set skipOwnershipValidation = false
   - Verify DLC gate blocks access
   - Simulate ownership: `PlayerPrefs.SetInt("DLC_OWNED_DLC_11_CELESTIAL", 1)`
   - Verify gate opens

## 📊 DLC Metrics to Track

- **Gate Hits**: How many non-owners try to access DLC?
- **Conversion Rate**: Gate hits → DLC purchases
- **Completion Rate**: % of DLC owners who finish Moon 14
- **Playtime**: Average hours spent in DLC content
- **Retention**: Day 7/Day 30 return rate after DLC purchase
- **Ratings**: Store reviews (Steam, Epic, GOG)
- **Bug Reports**: Critical/High/Medium/Low severity
- **Revenue**: DLC sales by platform + region

## 🐛 Common Issues & Solutions

### Issue: DLC not appearing in game
- **Fix**: Check manifest.json exists in StreamingAssets/DLC/
- **Fix**: Verify dlcId matches DLCGate configuration
- **Fix**: Enable verbose logging in DLCLoader

### Issue: Save compatibility errors
- **Fix**: Increment requiredSaveVersion in manifest
- **Fix**: Add migration logic to DLCSaveCompatibility
- **Fix**: Test v18 → v19 save migration

### Issue: DLC gate not blocking
- **Fix**: Set skipOwnershipValidation = false
- **Fix**: Verify CheckDLCOwnership() logic
- **Fix**: Clear PlayerPrefs: `PlayerPrefs.DeleteKey("DLC_OWNED_...")`

### Issue: Addressables not loading
- **Fix**: Build Addressables before DLC build
- **Fix**: Upload catalog.json to CDN
- **Fix**: Check Addressables settings (remote load paths)

### Issue: Performance regression
- **Fix**: Profile DLC scenes (Profiler → CPU/GPU/Memory)
- **Fix**: Add LOD groups to DLC meshes
- **Fix**: Enable occlusion culling in DLC zones

## 📚 Resources

- **DLC Architecture**: See `Assets/_Project/Scripts/DLC/DLCLoader.cs`
- **Save System**: See `Assets/_Project/Scripts/Save/SaveManager.cs`
- **ContentSpawner Pattern**: See `Moon10ContentSpawner.cs`
- **Addressables Docs**: https://docs.unity3d.com/Packages/com.unity.addressables@latest
- **Steam DLC Setup**: https://partner.steamgames.com/doc/store/application/dlc

## 🎯 Success Criteria

### Launch Readiness (/100):
- [ ] +10: DLC manifest.json valid
- [ ] +10: All scenes/prefabs/scripts built
- [ ] +10: Save compatibility tested (v18 → v19)
- [ ] +10: DLC gate working on all platforms
- [ ] +10: Addressables catalog uploaded to CDN
- [ ] +10: Ownership validation working (Steam/Epic/GOG)
- [ ] +10: Full playthrough completed (QA)
- [ ] +10: Performance targets met (60 FPS min)
- [ ] +10: No critical bugs remaining
- [ ] +10: Store pages live (all platforms)

**Target Score: 85+** before launch.

---

*Template Version: 1.0 (Agent 10, Build 1)*
*Last Updated: 2026-05-24*
