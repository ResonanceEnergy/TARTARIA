# TARTARIA — Visual Upgrade Solutions (Greybox → Production)
**Date:** 2026-04-29  
**Current State:** Greybox prototype with AAA systems + rendering but primitive geometry  
**Goal:** Choose your path to visual completion

---

## 🎯 CHOOSE YOUR PATH

| Path | Time | Cost | Result |
|------|------|------|--------|
| **Solution 1: MVP** | 4-8 hours | Free | Indie game quality, playable |
| **Solution 2: Asset Store** | 2-4 hours | $200-500 | AA quality, fast |
| **Solution 3: Full Custom** | 6-8 weeks | $2000-5000 | AAA quality, unique |

---

## ✅ SOLUTION 1: MINIMUM VIABLE (4-8 hours, FREE)

**Goal:** Replace primitives with Mixamo characters, make VFX visible, add skybox

### **Step 1: Character Models (4 hours)**

```powershell
# Navigate to project
cd C:\dev\TARTARIA_new

# Create characters directory
New-Item -ItemType Directory -Force Assets\_Project\Models\Characters
```

**Manual download from Mixamo.com:**
1. Visit https://mixamo.com (requires free Adobe account)
2. For each character, select **T-POSE** (not animation), then download:
   - **"Adventurer"** male → Save as `Player_Mesh.fbx`
   - **"Queen"** female → Save as `Anastasia_Mesh.fbx`
   - **"Worker"** male → Save as `Milo_Mesh.fbx`
   - **"Mutant"** creature → Save as `MudGolem_Mesh.fbx`
3. Move all 4 FBX files to: `Assets\_Project\Models\Characters\`
4. Run auto-import:
   ```powershell
   .\apply-visual-upgrades.ps1 -P0Only
   ```

**Expected result:** Characters will be humanoid meshes instead of capsules/cubes

### **Step 2: Make VFX Visible (1 hour)**

Add Aurora VFX to sky + periodic scan pulses:

```csharp
// Edit: Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs
// In SpawnParticleEffects() method, add at the end:

void SpawnParticleEffects()
{
    // ... existing wisps and dust motes ...

    // NEW: Spawn Aurora in sky (permanent ambient VFX)
    var auroraPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
        "Assets/_Project/Prefabs/VFX/Aurora.prefab");
    if (auroraPrefab != null)
    {
        var aurora = Instantiate(auroraPrefab, new Vector3(0, 50, 0), Quaternion.identity);
        aurora.name = "Sky_Aurora";
        Debug.Log("[EchohavenContentSpawner] Aurora VFX spawned in sky.");
    }
    
    // NEW: Periodic scan pulse VFX (every 10 seconds)
    InvokeRepeating(nameof(TriggerAmbientScanPulse), 5f, 10f);

    Debug.Log("[EchohavenContentSpawner] Particle effects spawned.");
}

// NEW: Trigger scan pulse at player position for ambient visual interest
void TriggerAmbientScanPulse()
{
    var player = GameObject.FindWithTag("Player");
    if (player != null && ResonanceScannerSystem.Instance != null)
    {
        // Trigger lightweight scan pulse VFX (won't reveal POIs, just visual)
        var scanPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Project/Prefabs/VFX/ScanPulse.prefab");
        if (scanPrefab != null)
        {
            Instantiate(scanPrefab, player.transform.position, Quaternion.identity);
        }
    }
}
```

### **Step 3: Add Skybox (30 min)**

Download free skybox from Unity Asset Store:
- "Free HDR Sky" by ProAssets
- Import to project
- Set in Lighting Settings

### **Step 4: Verify (30 min)**

```powershell
.\tartaria-play.ps1
```

**Expected improvements:**
- ✅ Characters are humanoid meshes (not capsules)
- ✅ Buildings have restoration shader (mud effect)
- ✅ Aurora VFX in sky
- ✅ Periodic scan pulses at player position
- ✅ Press Q → 500 particle scan burst
- ✅ Press E on building → 2000 particle restore sparkle

**Visual quality:** **Indie game** (60-70% better than current)

---

## 🏪 SOLUTION 2: ASSET STORE (2-4 hours, $200-500)

**Goal:** Buy pre-made assets, skip manual labor

### **Character Packs ($50-150):**
- **Polygon Fantasy Character Pack** — $50, 30+ characters
- **Modular RPG Heroes** — $100, 15 characters with armor variants
- **Medieval Fantasy Character Pack** — $75, 20 characters

### **Environment Packs ($100-300):**
- **Modular Fantasy Kingdom** — $150, cathedral/fountain/towers
- **Low Poly Dungeon Pack** — $100, ruins + architecture
- **Fantasy Environment Pack** — $200, buildings + props + vegetation

### **VFX Packs ($50-100):**
- **Epic Toon FX** — $75, 400+ particle effects
- **Hovl Studio VFX** — $50, magic/energy effects

### **Total Cost:** ~$400, saves 40+ hours of manual work

**Import process:**
1. Buy assets from Unity Asset Store
2. Import to project
3. Assign to prefabs (Player, Anastasia, Milo, MudGolem)
4. Replace building geometry
5. Test

**Visual quality:** **AA game** (80-90% better than current)

---

## 🎨 SOLUTION 3: FULL CUSTOM ART (6-8 weeks, $2000-5000)

**Goal:** Commission unique art assets, reach Genshin Impact quality

### **Hire 3D Artists (ArtStation.com):**

**Character Artist ($1000-2000, 2-4 weeks):**
- 4 custom characters (Player, Anastasia, Milo, MudGolem)
- Full rigging, facial bones, custom textures
- Armor/clothing variations

**Environment Artist ($1000-2000, 2-3 weeks):**
- 3 hero buildings (Star Dome, Harmonic Fountain, Crystal Spire)
- Modular architecture pieces (columns, arches, walls)
- Props (benches, fountains, statues)

**VFX Artist ($500-1000, 1-2 weeks):**
- Custom Aether VFX (volumetric glow, tendrils)
- Corruption fractal shaders
- Restoration wave effects
- Combat hit effects

**Visual quality:** **AAA game** (100% improvement, unique style)

---

## 📊 COMPARISON MATRIX

| Feature | Current (Greybox) | MVP (Free) | Asset Store ($400) | Custom ($4000) |
|---------|-------------------|------------|-------------------|----------------|
| **Characters** | Primitives | Mixamo humanoids | Modular heroes | Unique custom |
| **Buildings** | Procedural | Procedural + shaders | Modular architecture | Hand-crafted |
| **VFX** | 500-2000 particles | Same + Aurora | Epic VFX pack | Custom shaders |
| **Textures** | 12 PBR generic | Same | Pack textures | Custom PBR |
| **Time to complete** | N/A | 4-8 hours | 2-4 hours | 6-8 weeks |
| **Visual quality** | 20/100 | 65/100 | 85/100 | 95/100 |
| **Playable demo?** | ❌ | ✅ | ✅ | ✅ |
| **Shippable?** | ❌ | ⚠️ (indie) | ✅ (AA) | ✅ (AAA) |

---

## 🚀 RECOMMENDED: START WITH SOLUTION 1

**Why:**
1. **Free** — Zero cost, just time investment
2. **Fast** — 4-8 hours total
3. **Validating** — Proves the pipeline works before spending money
4. **Iterative** — Can upgrade to Solution 2 or 3 later

**After Solution 1, reassess:**
- If visual quality is good enough → Ship as indie game
- If you need better → Buy Asset Store packs (Solution 2)
- If you have budget → Commission custom art (Solution 3)

---

## 📋 CURRENT STATUS (ALREADY COMPLETED)

✅ **Rendering pipeline:** URP 17 Forward+, GPU Resident Drawer, APV — **AAA-tier**  
✅ **Custom shaders:** 4 shaders created, **NOW APPLIED TO BUILDINGS** (fixed name matching)  
✅ **VFX upgrade:** 500-2000 particles, Aurora created  
✅ **Build automation:** 25 phases, 108s execution, zero manual steps  
✅ **Game systems:** Quest/dialogue/combat/inventory/save all functional  
❌ **Character models:** Primitives (capsules, cubes, spheres) — **BLOCKING VISUAL QUALITY**  
❌ **VFX visibility:** Not spawned in scene (Aurora, periodic pulses)  

**Next bottleneck:** Character meshes (Solution 1 Step 1)

---

## 🎯 IMMEDIATE ACTION

```powershell
# 1. Download 4 Mixamo characters (see Solution 1 Step 1)
# 2. Place in: Assets\_Project\Models\Characters\
# 3. Run import:
.\apply-visual-upgrades.ps1 -P0Only

# 4. Verify:
.\tartaria-play.ps1
```

**Expected result:** Game will look **60-70% better** in 4-8 hours.

**After that:** Decide if you need Solution 2 (Asset Store) or Solution 3 (Custom Art).
