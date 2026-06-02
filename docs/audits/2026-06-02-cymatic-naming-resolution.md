# Cymatic Engine & Aether Band Naming — Resolution

**Date:** 2026-06-02
**Sprint / Lane:** Sprint 9, Lane 9 (audit v2 item #9.4)
**Branch:** `agent/audio/cymatic-naming`
**Trunk:** 8ef925d6 (Sprint 8 L9 — HitFeedback)

---

## Audit item

Audit v2 had `#9.4 — "CymaticEngine naming decision"` marked pending. The
question: is the `CymaticMusicEngine` (introduced in the Sprint 5 creative
swarm, branch `agent/audio/mixer-snapshot-system` commit `24433c62`) using the
canonical Aether band names per CLAUDE.md, and do all peripheral systems agree?

CLAUDE.md (2026-05-29) is canonical:

> Aether band naming: **Telluric (7.83 Hz) / Harmonic (432 Hz) / Celestial (528 Hz)**.
> Resolves the doc 02 vs doc 15 contradiction. **Use 528 not 1296 for the top band.**

---

## Investigation

### What "Cymatic" actually means in this codebase

`CymaticMusicEngine` (Sprint 5, sibling branch) is a **generic procedural
three-band drone engine**. It uses the canonical names internally:

```csharp
public const float TELLURIC_HZ  = 7.83f;   // Earth — Schumann
public const float HARMONIC_HZ  = 432f;    // Water — Verdi
public const float CELESTIAL_HZ = 528f;    // Light — solfeggio
```

So the engine **name** "Cymatic" is a reference to the visible-vibration
phenomenon (Chladni patterns) that the in-fiction restoration buildings produce
— it's an aesthetic / domain label for the engine, not a 4th band. It coexists
correctly with Telluric / Harmonic / Celestial.

**Decision branch: B + C — `Cymatic` is fine as the engine name; the drift
is elsewhere.**

### Where the drift actually lived

Three files used the OLD `1296 Hz = Celestial` mapping that CLAUDE.md retired:

| File | Symbol | Before | After |
|---|---|---|---|
| `Scripts/Core/TartariaConstants.cs:22` | `celestialFrequencyHz` | `1296f` | `528f` |
| `Scripts/Core/TartariaConstants.cs:25` | `band3Frequency` | `129.6f` | `7.83f` |
| `Scripts/Core/TartariaConstants.cs:27` | `band9Frequency` | `1296f` | `528f` |
| `Scripts/Core/AetherComponents.cs:13`  | `HarmonicBand.Celestial` (comment) | `1296 Hz — 3x432` | `528 Hz — solfeggio` |
| `Scripts/Audio/ProceduralSFXLibrary.cs:23` | `F_CELESTIAL` | `1296f` | `528f` (and new `F_OVERTONE_HIGH = 1296f`) |
| `Scripts/Audio/AudioManager.cs:13` | header comment | `528 Hz (Healing), 1296 Hz (Celestial)` | canonical 3-band list |
| `Scripts/UI/WorldMapUI.cs:429` | `tech_frequency_healing` codex | "1296 Hz (celestial connection)" | canonical 3-band list |
| `Scripts/UI/WorldMapUI.cs:454` | `freq_528` codex | "Transformation" | "Celestial (Transformation)" |
| `Scripts/UI/WorldMapUI.cs:456` | `freq_1296` codex | "1296 Hz — Celestial Connection" | "1296 Hz — Harmonic Overtone (3x432)" |

### Sound design preserved

`ProceduralSFXLibrary.cs` used `F_CELESTIAL` as a literal **musical pitch** in
nine sound-design sites (chimes, sweeps, cascade chords, credit theme). Naively
changing the constant from 1296 → 528 would have altered the *sound* of those
clips. To preserve the audio output while fixing the naming:

1. `F_CELESTIAL` now correctly equals `528f` (canonical Celestial band).
2. A new constant `F_OVERTONE_HIGH = 1296f` was introduced.
3. All nine sites that were using `F_CELESTIAL` for its **old** numeric value
   (sound-design overtones, NOT band labels) were converted to
   `F_OVERTONE_HIGH`.

Net audio change: **zero**. Net naming change: aligned to canon.

### Back-compat

`F_HEALING = 528f` and `healingFrequencyHz = 528f` kept as aliases so any
existing call sites that referred to "healing" continue to compile and behave
identically. They're documented as back-compat aliases of Celestial.

### Out of scope

- `AdaptiveMusicController.cs` lines 145 (`BossDefeat` stinger = 1296 Hz) and 326
  (`_layer3Triumphant` chord with 1296 Hz overtone) — these are **pitch
  choices in sound design**, not band labels. Left untouched.
- Extension bands `Ethereal = 12 (3888 Hz)` and `Resonant = 15 (5832 Hz)` in
  `AetherComponents.cs` — not part of the canonical 3 bands; comment updated
  to flag them as post-canon extension bands but enum kept.
- Design docs (`docs/02_AETHER_ENERGY_SYSTEM.md`, others) — doc 02 already says
  "9-Band: 528 Hz" so it agrees with canon; older references to 1296 in lore
  docs are out of scope for this code-layer audit.

---

## Verification

- `grep "1296" Assets/_Project/Scripts/**/*.cs` — only sound-design sites remain
  (intentional musical pitch, no longer labeled "Celestial").
- `grep "celestialFrequencyHz|band9Frequency" Assets/_Project/Scripts` — both
  now resolve to `528f`.
- No consumers reference the renamed/added constant fields on
  `TartariaConstants` (verified via grep of the field names across `Scripts/`),
  so no downstream breakage from the SO defaults changing.

---

## Conclusion

**Decision: branch C** — scattered ad-hoc names ("Healing 528" + "Celestial
1296" + raw `1296f` literals labeled as bands) were unified under the canon
**Telluric 7.83 / Harmonic 432 / Celestial 528**, with `F_OVERTONE_HIGH = 1296`
introduced as the proper home for the musical overtone that the legacy code was
conflating with a band.

`CymaticMusicEngine` (the engine name) is kept — "cymatic" describes the visual
phenomenon (vibrations creating patterns) and is compatible with the engine
being a music driver for the three canonical bands.

Audit v2 #9.4 — **RESOLVED.**
