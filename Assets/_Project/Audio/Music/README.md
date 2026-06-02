# Moon 1 Zone Music — `Assets/_Project/Audio/Music/`

This folder holds the three authored AudioClips that drive `AdaptiveMusicController`'s
Moon 1 zone-music layer. They are referenced by Inspector field on the scene-owned
`AdaptiveMusicController` GameObject (auto-bootstraps if missing — see
`Scripts/Audio/AdaptiveMusicController.cs`).

The procedural 4-layer RS-reactive bed continues to run alongside this — the zone
music is an additional *authored* layer for Moon 1 narrative beats.

## Expected files

| File | Purpose | When it plays |
|---|---|---|
| `village_ambient.wav` (or `.mp3`/`.ogg`) | Soft Echohaven village bed — drone-y, low harmonics, sparse percussion OK | Auto-started in `AdaptiveMusicController.Start()` if assigned. Crossfades in over 2.0s. |
| `restoration_swell.wav` | Bright golden swell — 1.5-3s, builds and releases | Layered (one-shot) over the live ambient every time `GameEvents.OnBuildingRestored` fires. Does NOT replace the ambient bed. |
| `win_capstone.wav` | Triumphant capstone motif — 4-8s, full resolution | One-shot when `GameEvents.OnMoonCompleted` fires. Ambient then crossfades to silence over 2.0s after an 8-second delay. |

Drop the assets directly into this folder. Then on the
`AdaptiveMusicController` GameObject, drag each clip onto:

- **Village Ambient** → `village_ambient`
- **Restoration Swell** → `restoration_swell`
- **Win Capstone** → `win_capstone`

There is also an optional **Music Group** field — wire this to the `Music`
`AudioMixerGroup` of `Resources/Audio/Mixers/EchohavenMaster` so the zone music
flows through the pause snapshot ducking system. If unset, the sources run on
the master bus (still audible, just not snapshot-controlled).

## How to generate placeholder clips (Cowork)

The art / audio pipeline already ships procedural placeholder generators. If you
don't have authored clips yet, run from the Unity Editor menu bar:

> `Tartaria → 3 Tier → Tier 3 Procedural Audio`

That tool writes placeholder `.wav` files for music + SFX into the appropriate
asset folders so the project compiles and plays with audible feedback before
final audio assets arrive. Re-running it will not overwrite real authored files
unless you explicitly tell it to.

Alternatively, the existing `AdaptiveMusicController.GenRestorationSwell()` and
`GenDiscoveryArpeggio()` helpers already produce a passable swell in-engine for
testing — assign nothing here and the procedural Layer-2 reactive system still
fires its own swell on `OnBuildingRestored`. The zone-music path simply adds a
*second*, louder, authored layer on top once these files exist.

## Wiring summary (current as of 2026-06-01)

```
Inspector clip       AdaptiveMusicController field          Source @ runtime
─────────────────────────────────────────────────────────────────────────────
village_ambient   →  village_ambient                    →  ZoneAmbient_A/B (loop, crossfade)
restoration_swell →  restoration_swell                  →  ZoneStinger (PlayOneShot, layered)
win_capstone      →  win_capstone                       →  ZoneStinger (PlayOneShot, then ambient fade)
```

Public API exposed on `AdaptiveMusicController` for QA / Editor hooks:

```csharp
public AudioClip VillageAmbient { get; set; }
public AudioClip RestorationSwell { get; set; }
public AudioClip WinCapstone { get; set; }
public AudioMixerGroup MusicGroup { get; set; }

public void SetZoneAmbient(AudioClip clip);   // 2.0s crossfade, null = fade to silence
public void PlayStinger(AudioClip clip);      // layered one-shot over ambient
```

— Audio Engineer agent, 2026-06-01

---

## Cymatic Music Engine — three-band Aether drone bed

> Added 2026-06-02. Lives at `Scripts/Audio/CymaticMusicEngine.cs`. Runs in
> parallel with `AdaptiveMusicController` — this is the *resonance physics*
> layer, not the *narrative authored* layer.

`CymaticMusicEngine` generates three sine-wave drones procedurally at runtime
(no .wav files required, no Resources lookups). Each drone is one Aether
band per the canonical mapping in `CLAUDE.md`:

| Band | Frequency | Element | Restoration trigger (buildingId substring, case-insensitive) |
|---|---|---|---|
| Telluric  | 7.83 Hz | Earth | `spire`, `cathedral` |
| Harmonic  | 432 Hz  | Water | `fountain` |
| Celestial | 528 Hz  | Light | `dome`, `stardome` |

### What happens at runtime

1. `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` auto-creates a
   `CymaticMusicEngine (auto)` GameObject in `DontDestroyOnLoad` if no
   authored instance exists.
2. `Awake()` builds three `Band` records, each with two `AudioSource` children
   (carrier + 2nd-order overtone). All sources start playing immediately at
   `volume = 0` so phase is continuous when they fade in.
3. Each `AudioSource.clip` is an `AudioClip.Create(stream:true)` with a
   `PCMReaderCallback` that synthesizes `Math.Sin(phase)` into the buffer.
   Phase is captured via closure refs so loop-boundary samples are
   click-free.
4. `GameEvents.OnBuildingRestored` (canonical `Action<string>` per
   `docs/agents/API_CONTRACT.md`) routes through `MatchBand()` and fades the
   matched carrier from 0 → 0.4 over 3 seconds.
5. `GameEvents.OnMoonCompleted` with `args.moonIndex == 1` is the **capstone**:
   all three carriers ramp to 0.55 and all three 2nd-order overtones (carrier
   * 2 Hz) ramp to 0.22 over 5.5 seconds — full Moon 1 mix.

### Logging contract (per CLAUDE.md no-debt mandate rules 3 + 4)

Every layer activation logs the building id, target volume, and full mix
state in this format (so a future bug "the music didn't change" can be
diagnosed by reading console output alone):

```
[CymaticMusicEngine] LAYER-ACTIVATE id='Harmonic' carrierHz=432 targetVol=0.40 trigger='echohaven_fountain' mixBefore=[T=0.00/o0.00|H=0.00/o0.00|C=0.40/o0.00]
[CymaticMusicEngine] LAYER-ACTIVE-COMPLETE id='Harmonic' atVol=0.40 mixAfter=[T=0.00/o0.00|H=0.40/o0.00|C=0.40/o0.00]
```

Unmatched buildingIds also log (not a silent fallback):

```
[CymaticMusicEngine] OnBuildingRestored('echohaven_well') — no band matches substring rules (fountain|dome|stardome|spire|cathedral). Mix unchanged: [...].
```

### Debug hooks for QA

```csharp
CymaticMusicEngine.Instance.DebugActivateTelluric();
CymaticMusicEngine.Instance.DebugActivateHarmonic();
CymaticMusicEngine.Instance.DebugActivateCelestial();
CymaticMusicEngine.Instance.DebugCapstone();
string mix = CymaticMusicEngine.Instance.PublicMixSnapshot();
```

### Relationship to `AdaptiveMusicController`

`AdaptiveMusicController` plays *authored* clips (village ambient, restoration
swell, win capstone). `CymaticMusicEngine` plays *procedural* drones tied to
the Aether physics model. They are independent — both subscribe to
`OnBuildingRestored` and `OnMoonCompleted`, neither blocks the other. Mix
both through the `Music` AudioMixerGroup if you need snapshot-controlled
ducking; currently `CymaticMusicEngine` runs on the master bus.

— Audio Engineer agent, 2026-06-02
