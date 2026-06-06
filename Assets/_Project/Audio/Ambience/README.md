# Ambience Drop-Zone

Drop `.wav` / `.ogg` / `.mp3` ambient music tracks here. They get auto-wired
into the Echohaven scene as looping `AudioSource`s on every build (Phase 9k2)
or via menu **TARTARIA → Audio → Bind Ambience Tracks**.

## Recommended free sources (commercial-OK)

- **Sonniss GDC bundles** — https://sonniss.com/gameaudiogdc — 30-50 GB/year, royalty-free.
  Cherry-pick the long-form ambient/drone cuts.
- **Pixabay Music** — https://pixabay.com/music — search `ambient`, `drone`, `singing bowl`.
- **Freesound** (CC0 filter) — https://freesound.org — user `InspectorJ` for pristine recordings.
- **Audacity / Cardinal** — generate 432 Hz pads from scratch.

## Routing

If `Assets/_Project/Audio/Mixers/MasterMixer.mixer` exists with an `Ambience`
group, dropped clips are routed to it automatically. Otherwise they play at
default unity-gain. Mixer is created/populated by Phase 9k.

## Naming

Any filename works. Loops play at volume 0.22 by default. To rename or
re-volume, open the Echohaven scene and edit the `AudioAmbience/DesignerTracks/<clip>`
GameObject.
