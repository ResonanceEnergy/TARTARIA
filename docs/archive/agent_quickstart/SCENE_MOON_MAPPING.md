# 🗺️ TARTARIA SCENE-TO-MOON MAPPING

## CURRENT STATE (May 28, 2026)

### Main Scenes (3):
- **Boot.unity** (17 KB) → Bootstrap/launcher scene
- **Echohaven_VerticalSlice.unity** (287 KB) → **MOON 1: Magnetic Moon**
- **UI_Overlay.unity** (92 KB) → HUD/UI additive scene

### Moon Scenes (12 files):

Based on docs/03_CAMPAIGN_13_MOONS.md, here's the likely mapping:

| Moon # | Moon Name | Scene File (Likely) | Size | Status |
|---|---|---|---|---|
| 1 | **Magnetic Moon** (Echohaven) | Echohaven_VerticalSlice.unity | 287 KB | ✅ Primary Moon 1 |
| 2 | **Lunar Moon** (Crystalline Caverns) | CrystallineCaverns.unity | 283 KB | ✅ Exists |
| 3 | **Electric Moon** (Orphan Train) | ❓ TidalArchive.unity? | 233 KB | ⚠️ Name mismatch |
| 4 | **Self-Existing Moon** (Corrupted Golem) | DeepForge.unity | 328 KB | ⚠️ Thematic guess |
| 5 | **Overtone Moon** (White City) | CelestialObservatory.unity | 144 KB | ⚠️ Thematic guess |
| 6 | **Rhythmic Moon** (Timekeeper) | ClockworkCitadel.unity | 323 KB | ✅ Good match |
| 7 | **Resonant Moon** (Korath Sacrifice) | SunkenColosseum.unity | 286 KB | ⚠️ Thematic guess |
| 8 | **Galactic Moon** (Airship Armada) | AuroralSpire.unity | 296 KB | ⚠️ Sky-themed, maybe |
| 9 | **Solar Moon** (Prophecy Letters) | LivingLibrary.unity | 289 KB | ⚠️ Letters = library? |
| 10 | **Planetary Moon** (Rail Network) | PlanetaryNexus.unity | 271 KB | ✅ Good name match |
| 11 | **Spectral Moon** (Fleet Combat) | WindsweptHighlands.unity | 220 KB | ⚠️ Naval highlands? |
| 12 | **Crystal Moon** (Bell Convergence) | StarFortBastion.unity | 284 KB | ⚠️ Star fort theme |
| 13 | **Cosmic Moon** (Final Confrontation) | VerdantCanopy.unity | 453 KB | ⚠️ Biggest, endgame? |

### ⚠️ UNCERTAINTY:

**Problem:** Scene file names don't match Moon/doc names clearly.

**Examples:**
- Moon 3 "Orphan Train" → No matching scene name
- "TidalArchive" → Doesn't match any Moon explicitly
- "VerdantCanopy" → Could be Moon 3 (nature) or Moon 13 (grand finale)

### ✅ WHAT'S CLEAR:
- 1 Bootstrap (Boot.unity)
- 1 Moon 1 (Echohaven_VerticalSlice.unity)
- 1 UI (UI_Overlay.unity)
- 12 More Moon scenes (need verification of which is which)

**Total: 15 scenes = sufficient for 13 Moons + Boot + UI**

---

## RECOMMENDED ACTIONS:

### 1. Rename Scenes for Clarity
**Current:** `AuroralSpire.unity`  
**Should be:** `Moon08_AuroralSpire.unity` (Airship Armada)

This makes it OBVIOUS which Moon each scene represents.

### 2. Create Scene Registry ScriptableObject
```csharp
[CreateAssetMenu]
public class MoonSceneRegistry : ScriptableObject
{
    public MoonSceneData[] moons;
}

[System.Serializable]
public class MoonSceneData
{
    public int moonNumber;
    public string moonName;
    public string scenePath;
    public bool isComplete;
}
```

### 3. Verify Each Scene Has Required Components

For EACH scene, check:
- [ ] PlayerSpawner GameObject exists
- [ ] ContentSpawner (MoonXContentSpawner) attached
- [ ] SceneMaster (MoonXSceneMaster) attached
- [ ] LevelBuilder (MoonXLevelBuilder) attached
- [ ] NavMesh baked
- [ ] Post-processing volume configured
- [ ] Prefabs assigned in Inspector

---

## NEXT STEPS:

1. **Open Unity Editor**
2. **Verify Echohaven_VerticalSlice.unity** (Moon 1):
   - Check if PlayerSpawner exists in Hierarchy
   - Check if prefabs are assigned
   - Press Play → See if player spawns
3. **Document which scene = which Moon**
4. **Rename scenes** to Moon01_, Moon02_, etc.
5. **Create validation script** to check all scenes

**Want me to create the validation/checker script?**
