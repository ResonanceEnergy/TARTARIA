# DLC QUICK REFERENCE — TARTARIA RPG
**Agent 10: Expansion & DLC Readiness**  
**Version:** 1.0 | **Date:** 2026-05-24

---

## 🚀 QUICK START: Adding New DLC

### 1. Copy Template
```bash
cp -r templates/DLC_11_TEMPLATE/ StreamingAssets/DLC/DLC_XX_NAME/
```

### 2. Edit manifest.json
```json
{
  "dlcId": "DLC_14_ABYSSAL",
  "moonNumber": 15,
  "requiredSaveVersion": 18,
  "steamAppId": 2100002,
  "epicItemId": "tartaria_dlc14_abyssal"
}
```

### 3. Create ContentSpawner
```csharp
// Copy from Moon10ContentSpawner.cs
public class Moon15ContentSpawner : DLCContentSpawner
{
    public override void Initialize(DLCManifest manifest, string contentPath)
    {
        base.Initialize(manifest, contentPath);
        // Your DLC logic here
    }
}
```

### 4. Add Save Block
```csharp
// In SaveData.cs
public Moon15SaveBlock moon15 = new();
```

### 5. Test
```csharp
// Dev mode: bypass ownership
DLCLoader.Instance.skipOwnershipValidation = true;

// QA mode: simulate ownership
PlayerPrefs.SetInt("DLC_OWNED_DLC_14_ABYSSAL", 1);
```

---

## 📁 DLC File Structure

```
StreamingAssets/DLC/
└── DLC_14_ABYSSAL/
    ├── manifest.json               ← REQUIRED
    ├── Scenes/
    │   └── Moon15_AbyssalDepths.unity
    ├── Prefabs/
    │   └── Moon15Portal.prefab
    ├── Scripts/
    │   └── Moon15ContentSpawner.cs
    └── Addressables/
        └── catalog.json
```

---

## 🔧 Core APIs

### DLCLoader
```csharp
// Check if DLC installed
bool isInstalled = DLCLoader.Instance.IsDLCInstalled("DLC_14_ABYSSAL");

// Check if DLC owned
bool isOwned = DLCLoader.Instance.IsDLCOwned("DLC_14_ABYSSAL");

// Check if DLC active (installed + owned)
bool isActive = DLCLoader.Instance.IsDLCActive("DLC_14_ABYSSAL");

// Get DLC manifest
DLCManifest manifest = DLCLoader.Instance.GetDLCManifest("DLC_14_ABYSSAL");

// Trigger gate event (for analytics)
DLCLoader.Instance.TriggerDLCGate("DLC_14_ABYSSAL", "zone_portal");
```

### DLCGate (Component)
```csharp
// Add to zone portal GameObject
public class Moon15Portal : MonoBehaviour
{
    DLCGate gate;
    
    void Start()
    {
        gate = GetComponent<DLCGate>();
        gate.dlcId = "DLC_14_ABYSSAL";
        gate.gateContext = "moon15_entrance";
    }
}
```

### Save Compatibility
```csharp
// Migrate save for new DLC
DLCSaveCompatibility.MigrateSaveForDLC(saveData, "DLC_14_ABYSSAL");

// Check if save can be loaded
bool canLoad = DLCSaveCompatibility.CanLoadSave(saveData.version);

// Get required DLCs for a save
List<string> required = DLCSaveCompatibility.GetRequiredDLCs(saveData);
```

### GameEvents
```csharp
// Subscribe to DLC events
GameEvents.OnDLCLoaded += HandleDLCLoaded;
GameEvents.OnDLCGateHit += HandleDLCGateHit;
GameEvents.OnDLCContentSpawned += HandleDLCContentSpawned;

// Fire DLC events
GameEvents.FireDLCLoaded("DLC_14_ABYSSAL");
GameEvents.FireDLCGateHit("DLC_14_ABYSSAL", "zone_portal");
GameEvents.FireDLCContentSpawned("DLC_14_ABYSSAL");
```

---

## 📊 Save Version Matrix

| Save Version | Moons | DLC Content |
|--------------|-------|-------------|
| v18 | 1-13 | Base game |
| v19 | 1-14 | DLC 11 (Moon 14) |
| v20 | 1-15 | DLC 12 (Moon 15) |
| v21 | 1-16 | DLC 13 (Moon 16) |
| v22-28 | 1-23 | DLC 14-20 (Moons 17-23) |

**Compatibility:** Base game (v18) can load any DLC save (v19-v28) by ignoring unknown blocks.

---

## 🎯 DLC Launch Checklist

### Pre-Production ✅
- [ ] Define scope (zones, quests, mechanics)
- [ ] Create manifest.json
- [ ] Design save blocks

### Content Creation 🎨
- [ ] Build Unity scenes
- [ ] Create prefabs (enemies, NPCs, props)
- [ ] Write quest chains
- [ ] Implement ContentSpawner

### Integration 🔌
- [ ] Hook to SaveManager
- [ ] Add DLC gate at portal
- [ ] Build Addressables catalog
- [ ] Test save migration

### Testing 🧪
- [ ] Full playthrough (start → completion)
- [ ] Save compatibility (v18 → v19)
- [ ] Ownership gating (Steam/Epic/GOG)
- [ ] Performance (60 FPS min)

### Launch 🚀
- [ ] Upload to CDN
- [ ] Steam/Epic/GOG store setup
- [ ] Release DLC
- [ ] Monitor telemetry

---

## 🐛 Common Issues

### DLC Not Loading?
1. Check manifest.json exists in `StreamingAssets/DLC/`
2. Verify dlcId matches your code
3. Enable verbose logging: `DLCLoader.verboseLogging = true`

### Gate Not Blocking?
1. Set `skipOwnershipValidation = false`
2. Clear PlayerPrefs: `PlayerPrefs.DeleteKey("DLC_OWNED_...")`
3. Test on actual platform (Steam/Epic/GOG)

### Save Errors?
1. Check save version: `SaveData.version >= requiredSaveVersion`
2. Add migration logic in DLCSaveCompatibility
3. Test v18 → v19 migration path

### Performance Issues?
1. Add LOD groups to DLC meshes
2. Enable occlusion culling in DLC scenes
3. Profile with Unity Profiler (CPU/GPU/Memory)

---

## 📈 DLC Metrics to Track

- **Gate Hits**: Non-owners trying to access DLC
- **Conversion Rate**: Gate hits → DLC purchases
- **Completion Rate**: % of owners who finish DLC
- **Playtime**: Average hours spent in DLC
- **Revenue**: Sales by platform + region
- **Ratings**: Store reviews (Steam/Epic/GOG)

---

## 🔗 Resources

- **Full Report**: `LIVEOPS_AGENT10_DLC_READINESS_REPORT.md`
- **DLC Template**: `templates/DLC_11_TEMPLATE/`
- **Save System**: `Assets/_Project/Scripts/Save/SaveManager.cs`
- **ContentSpawner Pattern**: `Moon10ContentSpawner.cs`

---

## 📞 Support

**Questions?** Contact Agent 10 team:
- Email: dlc-support@tartaria.dev
- Discord: #dlc-development
- Docs: https://docs.tartaria.dev/dlc

---

**DLC Readiness Score: 88/100** ✅ **PRODUCTION READY**  
**First DLC ETA:** Q3 2026 (Sep 15)  
**10 DLC Roadmap:** Q3 2026 → Q2 2028

*"Build once, expand forever."* — Agent 10
