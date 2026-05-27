# TARTARIA FREE ASSET UPGRADE — DETAILED EXECUTION ROADMAP
**Zero-Budget Path to Production Quality**  
**Date:** May 26, 2026  
**Total Cost:** $0  
**Timeline:** 4-6 weeks (100-120 hours total)  
**Target Quality:** 72-75/100 (solid indie, Steam-ready)

---

## 🎯 YOU SELECTED: OPTION A (FREE) FOR ALL CATEGORIES

**What This Means:**
- ✅ **$0 financial investment** (only your time)
- ✅ **Learn professional tools** (Blender, AI voice tools, Unity advanced)
- ✅ **Full creative control** (iterate unlimited, no vendor dependencies)
- ✅ **Quality sufficient for:** Steam Early Access, Kickstarter pitch, portfolio showcase
- ⚠️ **Time investment:** 100-120 hours (4-6 weeks full-time, or 10-15 weeks part-time)
- ⚠️ **Learning curve:** Blender basics required (20-30 hours if starting from zero)

---

## 📋 QUICK START CHECKLIST

**TODAY (30 minutes):**
- [ ] Create Adobe ID for Mixamo: https://account.adobe.com
- [ ] Download Blender 4.1: https://www.blender.org/download
- [ ] Create Azure free account: https://azure.microsoft.com/en-us/free
- [ ] Bookmark YouTube: "Blender for Game Assets" playlists

**THIS WEEK (Week 1):**
- [ ] Download 6 Mixamo characters + 48 animations (~3 hours)
- [ ] Download itch.io/OpenGameArt packs (~2 hours)
- [ ] Complete Blender beginner tutorial (~4 hours)
- [ ] Start character modifications in Blender (~8 hours)

**WEEKS 2-3:**
- [ ] Build 4 hero buildings with Blender Geometry Nodes (~40 hours)
- [ ] Create SSML voice scripts from GDD dialogue (~8 hours)
- [ ] Generate 660 voice lines via Azure Speech (~4 hours)

**WEEK 4:**
- [ ] Import all assets to Unity (~8 hours)
- [ ] Replace placeholder references in code (~8 hours)
- [ ] Polish pass + build validation (~8 hours)

---

## 🎭 PART 1: CHARACTERS (Week 1 — 25 hours)

### DAY 1: Mixamo Downloads (4 hours)

**STEP 1: Setup (15 min)**
1. Go to https://mixamo.com
2. Click "Sign In" → Create Adobe ID (free)
3. Verify email

**STEP 2: Download Characters (90 min)**

Download these 6 models **WITH T-POSE**:

| Your Character | Mixamo Search | Download Settings |
|----------------|---------------|-------------------|
| **Elara (player)** | "adventurer woman" | FBX Unity, T-Pose, With Skin ✅ |
| **Korath (giant)** | "mutant" | FBX Unity, T-Pose, With Skin ✅ |
| **Cassian (spy)** | "business man" | FBX Unity, T-Pose, With Skin ✅ |
| **Thorne (pilot)** | "military officer" | FBX Unity, T-Pose, With Skin ✅ |
| **Lirael (architect)** | "queen elegant" | FBX Unity, T-Pose, With Skin ✅ |
| **Veritas (organist)** | "formal suit man" | FBX Unity, T-Pose, With Skin ✅ |

Save to: `C:\dev\TARTARIA_new\Downloads\Mixamo\Characters\`

**STEP 3: Download Animations (90 min)**

For EACH of the 6 characters, download these 8 animations:

| Animation | Mixamo Search | Settings |
|-----------|---------------|----------|
| Idle | "standing idle" | 30 FPS, In Place ✅, With Skin ✅ |
| Walk | "walking" | 30 FPS, In Place ✅, With Skin ✅ |
| Run | "running" | 30 FPS, In Place ✅, With Skin ✅ |
| Jump | "jumping" | 30 FPS, In Place ✅, With Skin ✅ |
| Attack1 | "sword slash" | 30 FPS, In Place ✅, With Skin ✅ |
| Attack2 | "kick" | 30 FPS, In Place ✅, With Skin ✅ |
| Die | "death" | 30 FPS, In Place ✅, With Skin ✅ |
| Interact | "picking up" | 30 FPS, In Place ✅, With Skin ✅ |

**6 characters × 8 animations = 48 files**

Save to: `C:\dev\TARTARIA_new\Downloads\Mixamo\Animations\{CharacterName}\`

**VALIDATION:**
```powershell
Get-ChildItem "Downloads\Mixamo\Characters" -Filter "*.fbx" | Measure-Object
# Should show: Count = 6

Get-ChildItem "Downloads\Mixamo\Animations" -Recurse -Filter "*.fbx" | Measure-Object
# Should show: Count = 48
```

---

### DAY 2: itch.io Supplemental Packs (3 hours)

**PACK 1: Quaternius Low Poly Pack (for Milo Fox + Anastasia Child)**

1. Visit: https://quaternius.itch.io/ultimate-low-poly-pack
2. Click "Download Now" → Enter $0 → "No thanks, just take me to downloads"
3. Download ZIP (~150 MB)
4. Extract to `Downloads\Quaternius\`
5. Find files:
   - `Characters\Fox.fbx` → copy to `Downloads\Mixamo\Characters\Milo_Fox_Base.fbx`
   - `Characters\Child_Female.fbx` → copy to `Downloads\Mixamo\Characters\Anastasia_Child_Base.fbx`

**PACK 2: KayKit Animations (backup animations)**

1. Visit: https://kaylousberg.itch.io/kaykit-character-animations
2. Download (CC0 license)
3. Extract → keep as backup if Mixamo animations don't fit

---

### DAY 3-5: Blender Modifications (18 hours)

**DAY 3 MORNING: Install Blender (2 hours)**

1. Download: https://www.blender.org/download/lts/4-1/
2. Install (default settings, ~500 MB)
3. Launch Blender → Help → Manual → Bookmark
4. Preferences → Add-ons → Enable "Import-Export: FBX format"

**DAY 3 AFTERNOON: Learn Blender Basics (4 hours)**

Watch YouTube tutorials:
- "Blender Beginner Tutorial 2024" by Blender Guru (2 hours)
- "Unity FBX Export from Blender" by Game Dev Tutorials (30 min)
- "Character Editing in Blender" (1 hour)

**DAY 3 EVENING: Milo Six Tails (3 hours)**

**Goal:** Add 6 tails to fox model

1. Blender → File → Import → FBX → Select `Milo_Fox_Base.fbx`
2. Select fox mesh → Tab (Edit Mode)
3. Select tail vertices
4. Shift+D (duplicate) → R (rotate) → 60 degrees → Enter
5. Repeat 5 more times (creates 6 tails in circle)
6. Tab (Object Mode) → File → Export → FBX
7. Settings: Apply Transform ✅, Armature ✅
8. Save as `Milo_SixTails.fbx`

---

**DAY 4 MORNING: Korath Stone Texture (3 hours)**

**Goal:** Procedural stone skin for giant

1. Import `Mutant_Mesh.fbx` (Korath base)
2. Shading workspace → Add Noise Texture + Voronoi (stone cracks)
3. Add Bump node (roughness)
4. Color: grey-brown
5. UV unwrap → Bake texture (2048×2048)
6. Save: `Korath_Stone_Diffuse.png`, `Korath_Stone_Normal.png`
7. Export FBX: `Korath_Stone.fbx`

**Tutorial:** Search YouTube "Blender procedural stone texture"

---

**DAY 4 AFTERNOON: Cassian Coat (3 hours)**

1. Import `BusinessMan_Mesh.fbx`
2. Duplicate torso → Extrude to create coat
3. Add cufflink geometry (small spheres on wrists)
4. Export: `Cassian_Coat.fbx`

---

**DAY 5 MORNING: Anastasia Scale + Mote Points (2 hours)**

1. Import `Anastasia_Child_Base.fbx`
2. Scale 0.8× (7-9 year old proportions)
3. Add Empty objects at hands/head/chest (for particle emitters)
4. Parent to armature bones
5. Export: `Anastasia_Scaled.fbx`

---

**DAY 5 AFTERNOON: Thorne Weathered Textures (2 hours)**

1. Import `MilitaryOfficer_Mesh.fbx`
2. Texture Paint mode → paint dirt/wear
3. Bake to 2K texture
4. Export: `Thorne_Weathered.fbx`

---

## 🏰 PART 2: BUILDINGS (Week 2-3 — 50 hours)

### OpenGameArt Downloads (Day 7 — 3 hours)

**Download 4 free packs:**

1. **Gothic Cathedral Kit:** https://opengameart.org/content/gothic-cathedral-kit
   - 50+ modular pieces (walls, arches, windows)
   - Extract to `Downloads\OpenGameArt\Cathedral\`

2. **Low Poly Arena:** https://opengameart.org/content/low-poly-arena
   - Stone floors, pillars, domes
   - Extract to `Downloads\OpenGameArt\Arena\`

3. **Fountain Pack:** https://opengameart.org/content/fountain-pack
   - Fountain bases, basins
   - Extract to `Downloads\OpenGameArt\Fountains\`

4. **Polyhaven Textures:** https://polyhaven.com/textures
   - Search: "cathedral stone", "marble", "copper", "crystal"
   - Download 2K PNG sets (Diffuse, Normal, Roughness, AO)
   - Extract to `Downloads\Polyhaven\`

---

### Blender Geometry Nodes Buildings (Day 8-14 — 40 hours)

**BUILDING 1: Star Dome (Day 9-10 — 12 hours)**

**Steps:**
1. UV Sphere → flatten to dome (40m diameter)
2. Add rose window (12-petal circle array)
3. Spire with fibonacci spiral taper (20m tall)
4. Flying buttresses (array × 12)
5. Materials: Polyhaven cathedral stone textures
6. Export: `StarDome_Complete.fbx`

**Tutorial:** Search YouTube "Blender procedural cathedral"

---

**BUILDING 2: Harmonic Fountain (Day 11 — 6 hours)**

1. Cylinder basin (8m diameter, copper material)
2. Central spout (3 tiers, golden ratio heights)
3. Crystal formations (array × 8 around rim)
4. Water plane (animated shader)
5. Export: `HarmonicFountain_Complete.fbx`

---

**BUILDING 3: Crystal Spire (Day 12-13 — 10 hours)**

1. Ico Sphere → stretch to 60m column
2. Fibonacci spiral staircase inside
3. Faceted crystal material (transmission 1.0, IOR 1.5)
4. Interior light points
5. Export: `CrystalSpire_Complete.fbx`

---

**BUILDING 4: Cathedral Interior (Day 14 — 8 hours)**

1. Floor plan (80m × 50m)
2. Vaulted ceiling (rib vault curves)
3. Pipe organ (61 pipes, 5 registers)
4. Stained glass windows (rose window array × 12)
5. Export modular pieces (floor, walls, ceiling, organ)

---

### Unity Import (Day 15-16 — 14 hours)

1. Import all FBX files to `Assets\_Project\Models\Buildings\FreeAssets\`
2. Apply Polyhaven materials
3. Create building prefabs
4. Add InteractableBuilding scripts
5. Replace placeholder references in ContentSpawner files
6. Test in-game

---

## 🎤 PART 3: VOICE (Week 3-4 — 25 hours)

### Azure Speech Setup (Day 17 — 2 hours)

**OPTION 1: Azure Speech (RECOMMENDED — 500K free chars/month)**

1. Visit: https://azure.microsoft.com/en-us/free
2. Sign up (credit card for verification, won't charge)
3. Create Speech resource (Free F0 tier)
4. Copy API key + region

**OPTION 2: Grok/xAI (if available)**
1. Visit: https://x.ai
2. Create account
3. Free tier: 10K chars/month

**OPTION 3: ElevenLabs (fallback)**
1. Visit: https://elevenlabs.io
2. Free tier: 10K chars/month

---

### Voice Selection (Day 17 — 2 hours)

**Azure Neural Voices:**

| Character | Voice | Pitch | Rate |
|-----------|-------|-------|------|
| Korath | en-GB-RyanNeural | -20% | 0.8 |
| Anastasia | en-US-AnaNeural | +10% | 0.9 |
| Cassian | en-US-GuyNeural | 0% | 1.1 |
| Thorne | en-GB-ThomasNeural | -5% | 1.0 |
| Lirael | en-US-AriaNeural | +5% | 0.95 |
| Veritas | en-GB-LibbyNeural | 0% | 1.0 |

---

### Script Preparation (Day 18 — 6 hours)

1. Extract 660 dialogue lines from `docs\05_CHARACTERS_DIALOGUE.md`
2. Create CSV: Line ID, Character, Text, Emotion
3. Generate SSML scripts per character
4. Add prosody tags (pitch, rate, pauses)

**Example SSML:**
```xml
<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
  <voice name="en-GB-RyanNeural">
    <prosody pitch="-20%" rate="0.8">
      <break time="1s"/>
      I built for them. I always built for them.
      <break time="3s"/>
    </prosody>
  </voice>
</speak>
```

---

### Voice Generation (Day 19-20 — 8 hours)

**Batch Generation via Python:**

```python
import azure.cognitiveservices.speech as speechsdk

speech_key = "YOUR_KEY"
region = "YOUR_REGION"
speech_config = speechsdk.SpeechConfig(subscription=speech_key, region=region)

# For each SSML file:
for ssml_file in ssml_files:
    synthesizer = speechsdk.SpeechSynthesizer(speech_config, audio_config)
    result = synthesizer.speak_ssml_async(ssml_text).get()
    # Saves 660 WAV files
```

**Post-Processing in Audacity:**
1. Normalize to -3dB
2. Noise reduction
3. Trim silence (50ms head/tail)
4. Export as OGG (Vorbis quality 5)

---

### Unity Integration (Day 21 — 2 hours)

1. Update `VOPlaceholderLibrary.cs` line 43:
   ```csharp
   private const string VOICE_FOLDER = "VO/Production/";
   ```
2. Test dialogue playback in-game
3. Adjust volume balance (Dialogue -6dB, Music -12dB)

---

## ✅ WEEK 5: VALIDATION & POLISH (15 hours)

### Day 22: Build Test (4 hours)

```powershell
.\tartaria-play.ps1 -BatchOnly
```

**Test all systems:**
- [ ] Characters appear with new meshes (not KayKit)
- [ ] Buildings appear (not primitive cubes)
- [ ] Voice lines play (not 432Hz tones)
- [ ] Animations work
- [ ] Colliders function
- [ ] Performance stable (60+ FPS)

---

### Day 23: Polish (5 hours)

1. Lighting adjustments (sun, ambient)
2. Post-processing (bloom, AO, color grading)
3. Audio reverb zones
4. Material tweaks (emission, normal strength)

---

### Day 24: Documentation (3 hours)

1. Update `KNOWN_PLACEHOLDERS.md` → mark all as UPGRADED
2. Create `ASSET_CREDITS.md` with attribution
3. Update README.md with new screenshots

---

### Day 25: Final Commit (3 hours)

```powershell
git add .
git commit -m "Asset upgrade complete — FREE path

- Characters: Mixamo + Blender (72/100 quality)
- Buildings: OpenGameArt + Blender (75/100 quality)
- Voice: Azure Speech (78/100 quality)
- Overall: 75/100 — Steam-ready indie grade
"
git push origin main
```

---

## 🎯 EXPECTED RESULTS

**Before:**
- Characters: 60/100 (KayKit generic)
- Buildings: 50/100 (primitive cubes)
- Voice: 40/100 (432Hz tones)
- **OVERALL: 50/100**

**After (Free Path):**
- Characters: 72/100 (Mixamo + custom Blender)
- Buildings: 75/100 (OpenGameArt + Geometry Nodes)
- Voice: 78/100 (Azure Speech neural voices)
- **OVERALL: 75/100 ✅**

**Quality Sufficient For:**
- ✅ Steam Early Access launch
- ✅ Kickstarter pitch video
- ✅ Portfolio showcase
- ✅ Publisher pitch deck
- ✅ IndieDB / itch.io listing

---

## 💰 TOTAL COST: $0

**Time Investment:**
- Week 1 (Characters): 25 hours
- Week 2-3 (Buildings): 50 hours
- Week 3-4 (Voice): 25 hours
- Week 5 (Polish): 15 hours
- **TOTAL: 115 hours (~3 weeks full-time or 12 weeks part-time)**

**Skills Learned:**
- Blender 3D modeling & Geometry Nodes
- Azure Speech SSML scripting
- Unity advanced asset pipeline
- PBR material creation
- Audio post-production

**Transferable Value:** These skills apply to ANY future game project!

---

## 🚀 FUTURE UPGRADE PATH (If Funded Later)

When you have $2000-3000 budget, highest-impact upgrades:

1. **Commission Elara character** ($600) → unique player design
2. **Commission Star Dome** ($1200) → iconic first building
3. **Hire Korath voice actor** ($400) → replace AI with human bass performance
4. **Hire Anastasia child actor** ($800) → authentic child voice

**Result:** 75/100 → 88/100 (AAA indie competitive)

---

**READY TO START?**

Your first task: Create Adobe ID and download first Mixamo character (15 minutes)

Go to: https://mixamo.com

After signup, search "adventurer woman" → download Elara mesh!

**Document Status:** READY TO EXECUTE  
**Created:** May 26, 2026  
**Estimated Completion:** June 20, 2026 (4 weeks from now)
