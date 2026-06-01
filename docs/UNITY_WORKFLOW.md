# 🚀 TARTARIA - UNITY WORKFLOW QUICK REFERENCE

## 🎯 CURRENT STATUS

✅ **Code Complete**: 182 Moon systems (45,000 lines)
✅ **Tools Ready**: PrefabGeneratorTool + AutomatedPrefabWiring
✅ **Assets Present**: 110+ models, 80+ VFX, 50 materials, 50 UI sounds
⏳ **Prefabs**: Need generation (5 min in Unity)
⏳ **Wiring**: Need execution (10 min in Unity)

## ⚡ QUICK START (Easiest)

**Double-click:** `QUICK-START.bat`

This will:
1. Run pre-flight checks
2. Launch Unity with step-by-step instructions

## 🛠️ MANUAL WORKFLOW

### Step 1: Pre-Flight Check
```powershell
.\Preflight-Check.ps1
```

Verifies all assets and scripts are present.

### Step 2: Launch Unity
```powershell
.\Launch-Unity.ps1
```

Or open Unity Hub manually and load: `C:\dev\TARTARIA_new`

### Step 3: Generate Prefabs (in Unity)
1. Wait for scripts to compile (~2 minutes)
2. Menu → **Tartaria** → **Prefab Generator**
3. Click **"Test: Find KayKit Models"** (should show ✅ for all)
4. Select mode: **"Moon 1 Only"**
5. Click **"▶ GENERATE PREFABS"**
6. Wait 5 minutes
7. Check `Assets/_Project/Prefabs/` for new files

### Step 4: Wire Prefabs to Systems (in Unity)
1. Menu → **Tartaria** → **Automated Prefab Wiring**
2. Select **"Wire Moon 1"**
3. Check **"Create Missing Prefabs"** (if needed)
4. Click **"▶ RUN AUTOMATED WIRING"**
5. Wait 10 minutes

### Step 5: Test Playable Moon 1 (in Unity)
1. Open scene: `Scenes/Echohaven_VerticalSlice.unity`
2. Press **Play** (▶)
3. Controls:
   - **WASD** - Move
   - **Mouse** - Look
   - **E** - Interact
   - **Space** - Jump
   - **Shift** - Sprint
4. Test:
   - Walk around Echohaven
   - Collect glowing cyan Aether Shards
   - Fight MudGolems
   - Activate Tuning Nodes
   - Enter buildings
5. Fix bugs as needed

## 📦 WHAT GETS CREATED

### Prefabs Generated (15 for Moon 1):
```
Characters/
  Player.prefab (KayKit Barbarian)
  Milo.prefab (KayKit Ranger)
  Lirael.prefab (KayKit Mage)
  Cassian.prefab (KayKit Knight)
  Anastasia.prefab (KayKit Rogue)

Enemies/
  Moon1_MudGolem/MudGolem.prefab (Skeleton + mud material)

Collectibles/
  AetherShard/AetherShard.prefab (glowing cyan sphere)
  LoreArtifact/LoreArtifact.prefab (glowing book)

Interactive/
  TuningNode/TuningNode.prefab (ruin pillar + purple glow)

PowerUps/
  RS_Boost.prefab (cyan sphere)
  Combat_Boost.prefab (red sphere)
  Healing_Orb.prefab (green sphere)

Props/
  Candle.prefab (torch + fire VFX)
  Barrel.prefab (KayKit barrel)
  Rock.prefab (KayKit rock)
```

## 🎨 ASSET SOURCES

- **Characters**: `Assets/KayKit_Adventurers_2.0_FREE/Characters/gltf/`
- **Enemies**: `Assets/KayKit_Skeletons_1.1_FREE/characters/gltf/`
- **Buildings**: `Assets/_Project/Resources/Models/Buildings/FantasyRuins/`
- **VFX**: `Assets/Hovl Studio/` + `Assets/EffectExamples/`
- **UI Audio**: `Assets/_Project/Audio/UI/` (50 Kenney sounds)
- **Materials**: `Assets/_Project/Resources/Textures/Polyhaven/` (33 PBR sets)

## 🐛 TROUBLESHOOTING

### "KayKit models not found"
- Check paths in Prefab Generator test button
- Verify `Assets/KayKit_*` folders exist
- If missing, reimport KayKit packs

### "VFX prefabs missing"
- Check `Assets/Hovl Studio/` exists
- Check `Assets/EffectExamples/` exists
- These provide 80+ ready-to-use VFX prefabs

### "Compilation errors"
- Check Console for red errors
- Fix any missing namespaces
- Verify all .cs files in `Assets/_Project/Scripts/`

### "Prefab Generator window empty"
- Wait for full compilation
- Close and reopen window
- Check Editor folder has PrefabGeneratorTool.cs

### "Wiring tool fails"
- Ensure prefabs are generated first
- Check prefab paths match system expectations
- Review Console for specific errors

## 📊 EXPECTED TIMELINE

| Phase | Time | What Happens |
|-------|------|--------------|
| Unity opens + compiles | 2 min | Scripts compile, tools appear in menu |
| Generate prefabs | 5 min | 15 prefabs created in Assets/_Project/Prefabs/ |
| Wire prefabs | 10 min | Prefabs assigned to Moon1 system components |
| Test Moon 1 | 1 hour | Playthrough, bug fixes, iteration |
| **TOTAL** | **~1.5-2 hours** | **Playable Moon 1** |

## 🎯 SUCCESS CRITERIA

You'll know it's working when:
- ✅ Prefabs exist in `Assets/_Project/Prefabs/`
- ✅ Moon1 systems have prefabs assigned (check Inspector)
- ✅ Scene loads without errors
- ✅ Player spawns and can move (WASD)
- ✅ Aether Shards are visible and collectible
- ✅ MudGolems spawn and patrol
- ✅ VFX play on collection/combat

## 🚀 AFTER MOON 1 WORKS

Generate all 13 Moons:
1. Prefab Generator → Select **"All Moons"**
2. Generate prefabs (10 min)
3. Automated Wiring → Select **"Wire All 13 Moons"**
4. Wire prefabs (20 min)
5. Test each Moon scene

## 📞 HELP

- Check `CONTEXT.md` for full session history
- Check `WHATS_LEFT_TO_BUILD.md` for status
- Check `COMPLETE_ASSET_DISCOVERY.md` for assets
- Check Console logs in Unity for errors

---

**Everything is ready. Just run the tools in Unity!** 🎮
