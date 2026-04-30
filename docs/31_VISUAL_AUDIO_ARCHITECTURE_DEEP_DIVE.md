# 31 — Visual & Audio Architecture Deep Dive

**Audience**: Solo / small-team devs shipping their first 3D Unity title.
**Project context**: TARTARIA (Unity 6000.3.6f1, URP 17.3, single-dev procedural pipeline).
**Goal**: Industry-honest map of where we sit, what juniors actually do, and how to build a swap-out / upgrade-out asset framework.

---

## 1. Where TARTARIA Sits Today (Honest Audit)

| Axis | Current | AAA Floor | Indie Floor | Steam Wishlist Floor |
|---|---|---|---|---|
| Geometry | Procedural primitives + Mixamo capsule | Sculpted, retopo'd, baked | Quaternius/Synty kits | Synty kits, modular |
| Materials | URP/Lit + 4 custom shaders + AmbientCG PBR | Substance + decals + tessellation | URP/Lit + tiled PBR | URP/Lit + 1 trim sheet |
| Lighting | Realtime directional + APV + Forward+ | Lumen-equivalent (Unity 6 APV scenarios) | Baked + 1 realtime | Mixed: bake + light probes |
| Post-FX | Volume w/ bloom, vignette, tonemap | LUT grading per scene + DOF + CA | Bloom + vignette | Bloom + vignette + tonemap |
| VFX | 5 particle prefabs + 1 VFX Graph | Niagara/VFX Graph w/ 50+ effects | Shuriken libraries | Asset store packs |
| Animation | Capoeira clips on capsule (no humanoid mesh) | Mocap + IK + blendtrees + Animation Rigging | Mixamo + manual blends | Mixamo single locomotion |
| Audio | 4 procedural SFX + 1 432Hz loop, no mixer | Wwise/FMOD + 200+ assets + mixing | 50+ SFX + Audio Mixer + ducking | 20 SFX, 3 music loops |
| Pipeline | OneClickBuild (26 phases, headless) | Perforce + Jenkins + Houdini | git + Unity build profiles | Manual + Build Settings |
| **Composite** | **~45/100** | 90+ | 60-70 | 50-55 |

**Verdict**: You are **above the median junior solo dev** on pipeline/architecture, **below on art/audio content**. The framework is overbuilt for the amount of art in it — which is the *correct* direction for a 13-moon scope.

---

## 2. What Other Junior Devs Actually Do (2025-2026 reality check)

### The 3 Common Junior Trajectories

**A. The "Buy a Kit" path (most common, fastest to playable):**
- Synty POLYGON kit ($30-80) → drop into URP project
- Mixamo characters + 1 locomotion clip
- Default URP settings, post-processing volume
- 1-2 free music tracks from Pixabay
- Ships in 6 months, looks generic, finds an audience because *gameplay matters more than art*
- Examples: Lethal Company (free PS1 assets), Content Warning, Schedule I

**B. The "Greybox forever" path (most common failure):**
- Build everything procedurally for 18 months
- Never commit to art direction
- Engine looks like a tech demo
- Loses momentum, project dies
- **TARTARIA was on this path until you started integrating real assets.**

**C. The "Stylized commitment" path (best ROI for solo):**
- Pick ONE shader style (toon, painterly, ps1, low-poly flat)
- Buy/find 1 character base + 1 environment kit matching style
- Write 3-4 shaders, reuse everywhere
- Looks intentional even with $0 of assets
- Examples: A Short Hike, Chants of Sennaar, Tunic

**Recommendation for TARTARIA**: You have C's bones (4 custom shaders + procedural arch). Commit harder to the *Aether-stylized realism* lane. Don't try to compete with AAA fidelity — compete with **iconic silhouette + readable VFX language**.

### What junior devs get wrong

1. **Texturing too early** — they paint specific 4K textures before geometry is locked. Use trim sheets + tiled PBR until layout is final.
2. **No naming convention** — files become unfindable at ~200 assets. We already enforce `M_`, `T_`, `SM_`, `Echohaven_*` prefixes.
3. **Coupling content to scripts** — hardcoded `Resources.Load("Player_Mesh_v2")` everywhere. We use prefabs + ScriptableObject configs (good).
4. **No Build settings discipline** — manual scene drag-drop, breaks CI. We have `Phase 10` automating this.
5. **Audio as afterthought** — 80% ship without an Audio Mixer. We have `AudioManager` but no Mixer asset yet — fix in §5.
6. **One giant scene** — we already split Boot / Gameplay / UI_Overlay (additive load). Above industry average.

---

## 3. Swap-Out / Upgrade-Out Framework (Architecture)

The principle: **content references are data, not code**. Code asks for "the player mesh", data decides which one.

### 3.1 Visual Swap Layer

```
┌─────────────────────────────────────────────────────────┐
│  ScriptableObject: CharacterVisualProfile               │
│   ├─ Mesh prefab                                        │
│   ├─ Material overrides (per-slot)                      │
│   ├─ Animator override controller                       │
│   ├─ VFX prefab set (footstep, jump, hit)               │
│   └─ Audio cue set (footstep clips, vocals)             │
└─────────────────────────────────────────────────────────┘
        ▲                              ▲
        │                              │
   PlayerSpawner.Apply(profile)   NPCSpawner.Apply(profile)
```

**To upgrade from capsule → low-poly → AAA**:
1. Create `Profile_Elara_Capsule.asset` (current).
2. Create `Profile_Elara_Mixamo.asset` (downloaded mesh).
3. Create `Profile_Elara_Custom.asset` (commissioned model later).
4. Spawner reads which profile from a single `GameSettings.activeCharacterProfile` field.
5. **Zero code changes** when art swaps.

**Implementation sketch** (we should build this):

```csharp
// Assets/_Project/Scripts/Core/Data/CharacterVisualProfile.cs
[CreateAssetMenu(menuName = "Tartaria/Character Visual Profile")]
public class CharacterVisualProfile : ScriptableObject
{
    public GameObject meshPrefab;
    public RuntimeAnimatorController animator;
    public Material[] materialOverrides;
    public AudioClip[] footstepClips;
    public GameObject footstepVFX;
}

// PlayerSpawner.cs
public CharacterVisualProfile activeProfile;
void Spawn() {
    var visual = Instantiate(activeProfile.meshPrefab, root);
    visual.GetComponent<Animator>().runtimeAnimatorController = activeProfile.animator;
    // ...
}
```

### 3.2 Material Swap Layer (already partly built)

We already have:
- `Tartaria/AetherVein`, `Tartaria/Corruption`, `Tartaria/Restoration`, `Tartaria/SpectralGhost` shaders
- Per-building material assignment via `BuildingSpawner`

**Gap**: No `MaterialVariant` ScriptableObject. Add:

```csharp
[CreateAssetMenu(menuName = "Tartaria/Material Variant Set")]
public class MaterialVariantSet : ScriptableObject
{
    public Material lowQuality;   // URP/Simple Lit
    public Material standard;     // URP/Lit
    public Material highQuality;  // URP/Lit + tessellation + detail map
}
```

Then `CustomShaderApplicator` picks based on `QualitySettings.GetQualityLevel()`.

### 3.3 Audio Swap Layer (needs building)

```
┌─────────────────────────────────────────────────────────┐
│  ScriptableObject: AudioCueLibrary                      │
│   ├─ Dictionary<string, AudioCue>                       │
│   └─ AudioCue: { clips[], mixerGroup, volume, pitch }   │
└─────────────────────────────────────────────────────────┘
                       ▲
                       │
              AudioManager.Play("ui_click")
                       │
                  AudioMixer (Master → Music/SFX/UI/Ambience)
```

**Why a Mixer is non-negotiable**:
- Player wants a "music volume" slider → impossible without Mixer groups.
- Combat ducking music → snapshot transitions on Mixer.
- Localization may want different vocal mixes.
- All AAA + most successful indies use Mixer + Cue libraries.

We have **none of this yet** — `AudioManager` plays clips directly. **Action item**.

---

## 4. Asset Sources for Junior Devs (2026, Verified)

### 4.1 3D Models (free, commercial-safe)

| Source | License | Style | Notes |
|---|---|---|---|
| [Quaternius](https://quaternius.com) | CC0 | Low-poly | 1000+ models, ultimate kits, weekly updates |
| [Kenney](https://kenney.nl) | CC0 | Low-poly + voxel | Industry standard for prototyping |
| [KayKit](https://kaylousberg.itch.io) | CC0 | Stylized low-poly | Excellent character + dungeon kits |
| [Poly Pizza](https://poly.pizza) | CC-BY/CC0 mix | Mixed | Aggregator, filter by license |
| [Sketchfab](https://sketchfab.com) | Mixed | Mixed | Filter by "Downloadable + CC license" |
| [Mixamo](https://mixamo.com) | Adobe free | Realistic humanoid | Auto-rig + 2500+ animations |
| [OpenGameArt](https://opengameart.org) | CC/GPL mix | Variable quality | Old but huge library |
| [Itch.io free assets](https://itch.io/game-assets/free) | Per-creator | Indie/stylized | KayKit, Kenney mirror here |

**For TARTARIA specifically** (Gothic / Sacred Geometry / Aether):
- Quaternius "Modular Ruins" + "Castle Kit" — Echohaven foundation
- KayKit "Medieval Builder" — village props
- Sketchfab CC0 search "cathedral" "obelisk" "sacred geometry"
- Poly Haven HDRIs (we use this) — sky atmosphere

### 4.2 Textures (PBR, free, commercial)

| Source | License | Quality | Notes |
|---|---|---|---|
| [Poly Haven](https://polyhaven.com) | CC0 | 1K-8K PBR | We already use this. Best in class. |
| [AmbientCG](https://ambientcg.com) | CC0 | 1K-8K PBR | We already use this. 2000+ materials. |
| [FreePBR](https://freepbr.com) | CC0 | 2K-4K | Smaller library, high quality |
| [textures.com](https://textures.com) | Free tier 15/day | Photo-source | Good for decals + reference |

**Pipeline**: Use AmbientCG for tileable surfaces (stone, mud, grass), Poly Haven for HDRIs. Avoid 4K textures until art is locked — bake to 1K + add detail map.

### 4.3 Audio (free, commercial)

| Source | License | Type | Notes |
|---|---|---|---|
| [Freesound](https://freesound.org) | CC0/CC-BY | SFX | Filter by CC0 only for safety |
| [ZapSplat](https://zapsplat.com) | Free w/ attribution | SFX | 100k+ assets, gold standard |
| [Pixabay Music](https://pixabay.com/music) | CC0 | Music | Royalty-free, commercial OK |
| [Free Music Archive](https://freemusicarchive.org) | CC mix | Music | Filter license carefully |
| [Incompetech](https://incompetech.com) | CC-BY | Music | Kevin MacLeod, attribution required |
| [Bensound](https://bensound.com) | Free tier w/ attr | Music | Good ambient/orchestral |
| [Sonniss GDC bundles](https://sonniss.com/gameaudiogdc) | Royalty-free | SFX libraries | 30-50GB packs released yearly, free |

**For TARTARIA** (432 Hz / sacred / resonance):
- Sonniss GDC 2024 bundle (30GB free SFX)
- Pixabay search: "ambient", "drone", "singing bowl"
- Freesound user "InspectorJ" — pristine recordings, CC-BY
- Generate 432 Hz pads in [Cardinal](https://cardinal.kx.studio) (free VCV Rack) or Audacity Tone Generator

### 4.4 VFX

- Unity VFX Graph templates (built-in package) — free
- [Unity VFX Samples](https://github.com/Unity-Technologies/VFXGraph-Samples) — official
- [Realtime VFX](https://realtimevfx.com) community shares
- [Asset Store free VFX](https://assetstore.unity.com/3d/vfx?free=true) — search filter

### 4.5 Shaders / Code

- [Shader Graph examples](https://github.com/Unity-Technologies/ShaderGraph) — official
- [Catlike Coding tutorials](https://catlikecoding.com) — gold standard URP/HDRP
- [Ronja Tutorials](https://ronja-tutorials.com) — beginner shader writeups
- [Daniel Ilett](https://danielilett.com) — URP-specific

### 4.6 Fonts

- [Google Fonts](https://fonts.google.com) — SIL OFL, commercial OK
- [Font Squirrel](https://fontsquirrel.com) — commercial-licensed only
- For TARTARIA: "Cinzel" (sacred/imperial), "Orbitron" (futuristic), "Crimson Pro" (body) — all SIL OFL

---

## 5. Beginner → Advanced Techniques

### Beginner (Months 0-3)

1. **Use prefabs for everything** — never instance scene objects directly.
2. **Tag + Layer discipline** — set up before writing colliders.
3. **One scene per concern** (Boot / Gameplay / UI). Additive loading.
4. **ScriptableObject for data** — never hardcode stats/values.
5. **Input System (new)** — never `Input.GetKey()`. Action assets bind across keyboard/gamepad.
6. **Cinemachine for cameras** — never `transform.position = player.position + offset`.
7. **TextMeshPro for text** — never legacy `Text`.

### Intermediate (Months 3-12)

1. **Object pooling** for bullets/VFX/enemies.
2. **Coroutines vs UniTask vs Awaitable** — Unity 6 Awaitable is the new default.
3. **Addressables** for streaming content (replaces Resources folder).
4. **Animation Rigging package** for IK + procedural pose adjust.
5. **URP Renderer Features** for outlines, scan effects, custom passes.
6. **Job System + Burst** for crowd/foliage/AI.
7. **Light baking + Light Probes + APV** (we use APV).
8. **Audio Mixer with snapshots** for combat/exploration transitions.

### Advanced (Year 1+)

1. **GPU instancing + GPU Resident Drawer** (we have this enabled).
2. **Custom SRP passes** — full screen blits, custom shadow algorithms.
3. **Compute shaders** for foliage, fluids, GPU particles.
4. **DOTS/ECS** for 1000+ entity simulations (only if needed — has cost).
5. **Visual Effect Graph (VFX Graph)** for million-particle systems.
6. **Custom IL post-processors** for serialization, codegen.
7. **Asset bundle / Addressables remote delivery** for live content updates.
8. **Wwise/FMOD integration** for AAA audio (free for indies under revenue cap).

---

## 6. File Structure (Proven Pattern)

```
Assets/
├── _Project/                      ← All your stuff (underscore = sorts first)
│   ├── Scripts/
│   │   ├── Core/                  ← GameManager, Events, Utilities
│   │   ├── Gameplay/              ← Player, Enemy, Items
│   │   ├── UI/                    ← HUD, Menus
│   │   ├── Audio/                 ← AudioManager, Cue libraries
│   │   ├── AI/                    ← Behavior trees, perception
│   │   ├── Save/                  ← Persistence
│   │   ├── Input/                 ← Input handlers
│   │   ├── Camera/                ← Cinemachine controllers
│   │   ├── Integration/           ← External systems glue
│   │   └── Editor/                ← Editor tools (must be in Editor folder OR have asmdef)
│   ├── Art/
│   │   ├── Models/                ← FBX (split: Characters/, Props/, Buildings/)
│   │   ├── Textures/              ← Albedo/Normal/MaskMap (split by usage)
│   │   ├── Materials/             ← .mat files
│   │   ├── Shaders/               ← .shader / .shadergraph
│   │   └── VFX/                   ← VFX Graph + particle prefabs
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   ├── Ambience/
│   │   └── Mixers/                ← AudioMixer assets
│   ├── Animations/                ← .anim, AnimatorController
│   ├── Prefabs/                   ← Split by domain
│   │   ├── Characters/
│   │   ├── Buildings/
│   │   ├── VFX/
│   │   └── UI/
│   ├── Scenes/
│   ├── Config/                    ← ScriptableObject databases
│   ├── Resources/                 ← USE SPARINGLY (loads at startup)
│   ├── Settings/                  ← URP assets, Quality, Render Pipeline
│   └── Tools/                     ← Editor scripts, Python helpers
├── Plugins/                       ← Third-party native
├── ThirdParty/                    ← Imported packages we modify
└── StreamingAssets/               ← Files copied as-is to build
```

**Naming convention** (we already enforce most):

| Prefix | Type | Example |
|---|---|---|
| `SM_` | Static mesh | `SM_Pillar_01` |
| `SK_` | Skeletal mesh | `SK_Elara_Body` |
| `M_` | Material | `M_AetherVein` |
| `T_` | Texture | `T_Stone_Albedo` |
| `MI_` | Material instance | `MI_Stone_Mossy` |
| `S_` | Shader | `S_Aether_Lit` |
| `VFX_` | VFX prefab | `VFX_DomeAwakening` |
| `SFX_` | Sound effect | `SFX_Footstep_Stone_01` |
| `BGM_` | Background music | `BGM_Echohaven_Day` |
| `P_` | Prefab | `P_Player`, `P_Building_StarDome` |
| `UI_` | UI prefab/asset | `UI_HUD`, `UI_DialogueBox` |
| `Anim_` | Animation clip | `Anim_Idle`, `Anim_Walk` |
| `AC_` | AnimatorController | `AC_Player` |
| `SO_` | ScriptableObject | `SO_Quest_AwakenDome` |

**Assembly Definitions** (we have 10):
- One per domain, prevents circular refs, **massively** speeds compile.
- Editor scripts in their own asmdef with `Editor` platform only.
- ✅ We do this correctly.

---

## 7. Where TARTARIA Sits on Complexity (Honest Map)

### Complexity Axes

| Axis | Score 1-10 | Reasoning |
|---|---|---|
| **Architecture sophistication** | 8 | 10 asmdefs, event-driven, ScriptableObject configs, automated pipeline. Above most juniors. |
| **Content volume** | 2 | 1 character (capsule), 3 buildings, 4 SFX, 1 music. Below MVP. |
| **Visual polish** | 4 | URP+Forward+ + 4 custom shaders + APV. Above default but no committed art direction. |
| **Audio depth** | 1 | No mixer, no cue library, 1 music track. Bottom-tier. |
| **Gameplay loop** | 3 | Movement + dialogue + state machine wired. Combat/quests/inventory stubs only. |
| **Pipeline maturity** | 9 | OneClickBuild, BatchReadinessValidator, headless mode, 25-check validation. AAA-tier for solo. |
| **Documentation** | 9 | 31 design docs, lore bible, character bios, quest DB. Pre-production excellence. |
| **Test/CI infra** | 2 | Build validator yes, no PlayMode/EditMode unit tests. |

### Composite

- **As a tech demo / framework**: 7/10 — better than most junior projects.
- **As a playable game**: 2/10 — there is no game yet, just systems.
- **As a shippable product**: 1/10 — months of content work remaining.

### What this means

You have **the chassis of a Honda Civic with the dashboard of a Bentley**. Time to:
1. **Stop adding pipeline phases** until content fills them.
2. **Lock art direction** with one decision: stylized or realistic?
3. **Build the swap-out framework** in §3 so future asset upgrades are zero-code.
4. **Ship a 5-minute playable** of Moon 1 before any new system.

---

## 8. Concrete Next-Action Backlog (TARTARIA-specific)

Ordered by *unblocks-the-most-shipping*:

1. **Build `CharacterVisualProfile` SO** (§3.1). Wire to `PlayerSpawner`.
2. **Build `AudioMixer` + `AudioCueLibrary`** (§3.3). Refactor `AudioManager` to use it.
3. **Add `Settings/` folder** with quality presets (Low/Med/High) tied to URP asset variants.
4. **Lock art direction** — write a 1-page `docs/32_ART_BIBLE.md` with: palette hex codes, silhouette rules, shader list, reference images.
5. **Source female humanoid** (Mixamo "Eve" or "Kachujin Re" or "Liam") to replace capsule.
6. **Build Audio Mixer asset** with groups: Master / Music / SFX / UI / Ambience / Voice. Add 2 snapshots: Exploration / Combat.
7. **Add 5 Sonniss GDC ambient tracks** to `Music/Echohaven/`. Wire to `AudioManager.SetAmbience()`.
8. **Replace 6 capsule body parts** with 1 humanoid SkinnedMeshRenderer when female mesh imported.
9. **Decal Renderer Feature** for footprint trails, blood splats, magic circles (we have the feature enabled).
10. **Light bake Echohaven** — currently fully realtime. Bake static lighting + add reflection probes.

---

## 9. Reference: What "AAA Quality" Actually Means (Demystified)

It's not better hardware — it's **labor density**:

- **AAA**: ~300 artists, 50 engineers, 18-month polish phase, $80M budget.
- **AA**: 30-60 people, 12-month dev, $5-15M.
- **Indie successful**: 1-10 people, 18-36 months, $0-500K.
- **Solo successful**: 1 person, 24-60 months, $0-50K.

**You are 1 person + AI agents.** Your edge is:
- Pipeline automation (you have it).
- Procedural content generation (you have it).
- Documentation depth (you have it).
- AI-assisted art generation (Suno for music, Midjourney for concept, Mixamo for rigs).

Your weakness:
- Final polish art passes (commission $200-2000 of art when ready).
- Audio mixing (use FMOD free tier when ready).
- QA (recruit 5 friends to playtest each milestone).

**Realistic visual ceiling for solo + AI in 2026**: 65/100 (think *Tunic*, *A Short Hike*, *Lethal Company*). Aim there, not at *Cyberpunk*.

---

## 10. TL;DR Action Plan

1. **Keep**: Pipeline, asmdefs, ScriptableObject pattern, OneClickBuild, design docs.
2. **Add**: `CharacterVisualProfile` SO, `AudioMixer`, `AudioCueLibrary`, `MaterialVariantSet` SO.
3. **Lock**: Art direction in 1-page bible (32_ART_BIBLE.md).
4. **Source**: Quaternius modular ruins, KayKit medieval, Mixamo female base, Sonniss GDC SFX, Pixabay 432Hz tracks.
5. **Stop**: Adding new build phases until existing ones have content. Stop optimizing what isn't shipping.
6. **Ship**: 5-minute Moon 1 vertical slice with humanoid Elara, 3 Echohaven buildings, 1 quest, 1 music track, 10 SFX.

This puts you at honest **55-60/100** quality — *above* "Steam wishlist floor" and ready to start collecting wishlists while you build out the remaining 12 moons.
