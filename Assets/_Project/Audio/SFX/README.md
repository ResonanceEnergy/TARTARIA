# SFX Drop-Zone

Drop `.wav` / `.ogg` SFX one-shots and short loops here. Subfolders called
`Resources/` are picked up by `Resources.Load<AudioClip>(...)`; everything
else is wired by inspector references or designer tools.

Per the 2026-06-02 no-debt mandate, every audio controller in this project
logs LOUD when an expected clip is missing — names below match what the code
asks for. Add the wav, restart Play, and the warning goes away.

---

## Expected clips (Moon 1)

### `pipe_organ_drone.wav` — Cathedral ambient drone

- **Consumer:** `Assets/_Project/Scripts/Integration/Moon1PipeOrganController.cs`
- **Resources fallback path:** `Resources/Audio/SFX/pipe_organ_drone`
  (i.e. `Assets/_Project/Audio/SFX/Resources/Audio/SFX/pipe_organ_drone.wav`,
  or any other `Resources` folder containing `Audio/SFX/pipe_organ_drone.wav`).
- **Preferred wiring:** inspector-assign the clip on the `Moon1PipeOrganController`
  component (drag into the `Drone Clip` slot). This bypasses the Resources
  fallback warning entirely.
- **Length:** seamless loop, 8-30 seconds. Pad chords drawn from a pipe-organ
  sample bank, sustained drone — no hard attacks, no melodic motion. Should
  loop without an audible seam (apply a short crossfade in the editor).
- **Frequency target:** Telluric/Harmonic band — root drone around 432 Hz
  (or its octave-multiple) per the Aether band map in `CLAUDE.md`. Avoid
  528 Hz (Celestial band) for this clip — that's reserved for restoration
  payoff cues.
- **Spatial properties at runtime (set by the controller, not the clip):**
  - `spatialBlend = 1.0` (fully 3D)
  - `rolloffMode = Linear`
  - `minDistance = 5m`, `maxDistance = 50m`
  - `volume = 0.4`
  - `pitch = 1.0` resting → `1.25` after `GameEvents.OnBuildingRestored`
    fires with a buildingId containing "cathedral" (2-second ramp).

### Other Moon 1 SFX consumers

Other controllers (combat hits, brazier crackle, lore-stone hums, mud-pool
gurgle, tuning-pedestal tones) document their expected clips in their own
files — grep for `Resources.Load<AudioClip>` and `Debug.LogWarning` under
`Assets/_Project/Scripts/Integration/Moon1*.cs` to find the full list.

---

## How to generate placeholders

If you don't yet have a real recording:

1. **Editor menu:** `Tartaria → 3 Tier → Tier 3 Procedural Audio` — runs the
   procedural-audio batch which lays down loopable drones, hits, and stings.
   The pipe-organ drone variant is in the "Cathedral / Sustain" bucket and
   exports straight to `Assets/_Project/Audio/SFX/`.
2. **Audacity:** Generate → Tone (sine, 432 Hz, 12s) layered with detuned
   octaves at 216 Hz and 864 Hz, then Effect → Fade In/Out at the seam.
3. **Free sources** (commercial-OK):
   - **Sonniss GDC bundles** — https://sonniss.com/gameaudiogdc
   - **Pixabay Music** — https://pixabay.com/music — search `pipe organ drone`
   - **Freesound** (CC0 filter) — https://freesound.org — search
     `pipe organ sustain` and filter by CC0.

After dropping the wav, Unity will import it. If you placed it under a
`Resources` subfolder matching the path above, the controller picks it up
automatically; otherwise drag it into the inspector slot on the cathedral
pipe-organ GameObject (tagged `PipeOrgan`).

---

## Naming convention

- snake_case, lowercase.
- Prefix by Moon when scope-specific: `moon1_pipe_organ_drone.wav` is
  acceptable as an alternative to `pipe_organ_drone.wav`, but update the
  serialized field on the controller in that case — the Resources fallback
  path is hard-coded to `pipe_organ_drone`.

---

## Routing

If `Assets/_Project/Audio/Mixers/MasterMixer.mixer` exists with an `SFX`
group, dropped clips are routed to it automatically by the import
postprocessor. Otherwise they play at unity gain through the default
AudioListener. Mixer creation is owned by the Audio infrastructure pass
(see `Assets/_Project/Audio/Mixers/`).
