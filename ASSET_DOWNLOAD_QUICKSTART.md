# TARTARIA — Asset Download Quick Start Guide

**Priority: IMMEDIATE** — Get these 4 asset packs first to cover 80% of wishlist needs.

---

## 🔥 DOWNLOAD THESE FIRST (Top 4)

### 1. ⭐ Kenney UI Pack — **COVERS ALL UI NEEDS**
- **URL:** https://www.kenney.nl/assets/ui-pack
- **License:** CC0 (Public Domain) — No attribution needed!
- **Size:** ~15 MB
- **What you get:** 2000+ UI elements (icons, buttons, panels, banners, checkmarks)
- **Covers:** Quest markers, objective icons, notification banners, collectible icons
- **Import:** Download ZIP → Extract to `Assets/_Project/Art/UI/Kenney/`

### 2. ⭐ Unity Particle Pack — **COVERS ALL VFX NEEDS**
- **URL:** https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-particle-pack-127325
- **License:** Unity Companion (Free for commercial)
- **Size:** ~50 MB
- **What you get:** 100+ particle effects (beams, glows, sparkles, bursts)
- **Covers:** Quest markers, collectible sparkles, weather particles, cache bursts
- **Import:** Unity Asset Store → "Add to My Assets" → Package Manager → Import

### 3. ⭐ Hovl Studio Crystals Pack — **PERFECT FOR AETHER FRAGMENTS**
- **URL:** https://assetstore.unity.com/packages/vfx/particles/environment/stylized-crystal-pack-94937
- **License:** Unity Asset Store EULA (Free)
- **Size:** ~30 MB
- **What you get:** 5 crystal models + glow materials + particle effects
- **Covers:** Aether fragment glows, collectible idle VFX, magical effects
- **Import:** Unity Asset Store → "Add to My Assets" → Package Manager → Import

### 4. ⭐ Freesound Audio Bundle — **COVERS ALL AUDIO NEEDS**
Download these 10 essential sounds (all under 5 MB total):

#### Quest Audio (3 files)
1. **Quest Started:** https://freesound.org/people/Kenney/sounds/# (use Kenney completion sounds)
2. **Quest Complete:** https://freesound.org/people/fins/sounds/146723/ (CC0)
3. **Objective Progress:** https://freesound.org/people/plasterbrain/sounds/423169/ (CC BY 4.0)

#### Collectible Audio (3 files)
1. **Common Pickup:** https://freesound.org/people/Greencouch/sounds/427594/ (CC0)
2. **Rare Pickup:** https://freesound.org/people/Bertrof/sounds/351565/ (CC0)
3. **Epic Pickup:** https://freesound.org/people/plasterbrain/sounds/423169/ (CC BY 4.0)

#### Weather Audio (4 files)
1. **Wind Loop:** https://freesound.org/people/kangaroovindaloo/sounds/585177/ (CC0)
2. **Rain Loop:** https://freesound.org/people/FlatHill/sounds/237729/ (CC BY 3.0)
3. **Blizzard Loop:** https://freesound.org/people/klankbeeld/sounds/198296/ (CC BY 4.0)
4. **Underwater Loop:** https://freesound.org/people/CGEffex/sounds/98335/ (CC BY 3.0)

**Import:** Download → Move to `Assets/_Project/Audio/SFX/` (quest/collectible) or `Assets/_Project/Audio/Ambient/` (weather)

---

## 📋 Import Checklist

- [ ] Download Kenney UI Pack → Extract to `Assets/_Project/Art/UI/Kenney/`
- [ ] Import Unity Particle Pack via Package Manager
- [ ] Import Hovl Studio Crystals Pack via Package Manager
- [ ] Download 10 Freesound files → Organize into Audio folders
- [ ] Create attribution file: `Assets/_Project/ATTRIBUTIONS.txt`

---

## 📝 Attribution File Template

Create `Assets/_Project/ATTRIBUTIONS.txt`:

```
TARTARIA — Third-Party Asset Attributions
==========================================

UI ASSETS
---------
- Kenney UI Pack (CC0) - https://www.kenney.nl/assets/ui-pack
  No attribution required but credit given: Kenney.nl

AUDIO ASSETS
------------
- Rain Loop by FlatHill (CC BY 3.0) - https://freesound.org/people/FlatHill/sounds/237729/
- Blizzard Wind by klankbeeld (CC BY 4.0) - https://freesound.org/people/klankbeeld/sounds/198296/
- Underwater Ambience by CGEffex (CC BY 3.0) - https://freesound.org/people/CGEffex/sounds/98335/
- Magic Success by plasterbrain (CC BY 4.0) - https://freesound.org/people/plasterbrain/sounds/423169/

VFX ASSETS
----------
- Unity Particle Pack (Unity Companion License) - Unity Technologies
- Hovl Studio Crystals Pack (Unity Asset Store EULA) - Hovl Studio

All other assets created in-house or are CC0/public domain.
```

---

## 🚀 Quick Integration Guide

### Quest Markers (5 minutes)
1. Import Unity Particle Pack
2. Find `Particle Pack/Prefabs/Shaft Light` prefab
3. Create 3 variants:
   - `QuestMarker_Start` (gold color, scale 1.0)
   - `QuestMarker_Complete` (green color, scale 1.2)
   - `QuestMarker_Objective` (blue color, scale 0.8)
4. Assign to `Moon3QuestNodes.cs` CreateQuestNode() visual instantiation

### Collectible Pickups (10 minutes)
1. Import Freesound audio files to `Assets/_Project/Audio/SFX/Collectibles/`
2. Create AudioClip references in AudioManager
3. Update `Moon3Collectibles.cs` CollectibleItem.Interact():
   ```csharp
   Audio.AudioManager.Instance?.PlaySFX2D(rsReward switch {
       5f => "Collectible_Common",
       15f => "Collectible_Rare",
       30f => "Collectible_Epic",
       _ => "Collectible_Common"
   });
   ```

### Aether Fragment Glow (3 minutes)
1. Import Hovl Studio Crystals Pack
2. Find `Crystals/Prefabs/Crystal_Glow` prefab
3. Scale to 0.4 units
4. Attach to collectible visual in `Moon3Collectibles.cs` CreateCollectible()

### UI Integration (15 minutes)
1. Import Kenney UI Pack
2. Create UI sprites:
   - Quest marker icons: Use `icon_square` variants
   - Notification banners: Use `panel` variants with rounded corners
   - Checkmarks: Use `checkmark` icons (green tint)
3. Create Unity UI prefabs for notifications
4. Hook into `Core.GameEvents.RaiseHUDShowObjective()` calls

---

## 📦 Estimated Download Time

- **Total size:** ~100 MB (excluding optional Sonniss GDC bundle)
- **Fast connection (50 Mbps):** 20 minutes
- **Slow connection (5 Mbps):** 3 hours
- **Import time in Unity:** 10-15 minutes

---

## ✅ Post-Import Validation

Run this checklist after importing:

1. **VFX Test:** Drag Unity Particle Pack prefabs into scene → verify they render
2. **Audio Test:** Play Freesound files in Unity Inspector → verify waveform
3. **UI Test:** Create test UI panel with Kenney sprites → verify resolution
4. **Crystal Test:** Instantiate Hovl crystal prefab → verify glow shader works in URP

---

## 🔧 Troubleshooting

### Issue: Unity Particle Pack particles don't render
- **Fix:** Ensure URP asset is assigned in Project Settings → Graphics
- **Fix:** Check if particles use Built-in shaders → convert to URP shaders

### Issue: Freesound files won't import
- **Fix:** Ensure files are .wav or .mp3 format
- **Fix:** Check Unity import settings: Load Type = Decompress On Load (for short SFX)

### Issue: Hovl Studio crystals appear black
- **Fix:** Shaders may need URP conversion → right-click material → Edit URP Shader
- **Fix:** Assign default URP/Lit shader temporarily

### Issue: Kenney UI sprites appear blurry
- **Fix:** Set texture import settings: Texture Type = Sprite, Filter Mode = Point (for pixel art) or Bilinear

---

## 🎯 Next Steps After Quick Start

Once you have these 4 core asset packs:
1. Create prefab variants for each moon theme (recolor particles/materials)
2. Build AudioManager preset with all SFX references
3. Create UI prefab library for HUD notifications
4. Test integration in Moon3 scene first, then replicate to Moons 4-13

**See:** `FREE_ASSETS_RESEARCH_REPORT.md` for full list of 40+ assets including MEDIUM and LOW priority items.
