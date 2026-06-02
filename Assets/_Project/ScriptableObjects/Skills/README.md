# Assets/_Project/ScriptableObjects/Skills/

Authoring location for **skill-tree ScriptableObject assets** consumed by `Tartaria.Core.SkillTree`.

The folder is intentionally code-free. Code lives in
`Assets/_Project/Scripts/Core/SkillTree/`; this folder holds the `.asset` files Unity
serializes from those types. Cowork drives all authoring in the Unity Editor — agents
must NOT generate `.asset` files from outside the Editor (the GUIDs / fileIDs would
collide with future Editor regenerations and corrupt scene references).

---

## File: `AetherResonance.asset` (canonical)

A single `AetherResonanceTree` instance representing the 12-node Aether Resonance tree
for the player character. There is exactly **one** authored tree per save profile in
Phase 1 (Moon 1 ship). Future Moons may add per-character trees — those will live in
sibling folders (`Skills/Anastasia/`, `Skills/Lirael/`, etc).

### How to author it (Cowork in Unity Editor)

1. In the Project window, navigate to `Assets/_Project/ScriptableObjects/Skills/`.
2. Right-click the empty folder area → **Create → Tartaria → Skills → Aether Resonance Tree**.
3. Unity creates `AetherResonance.asset`. Name it exactly `AetherResonance` (the
   `[CreateAssetMenu]` default filename — keep it stable for save-system lookups).
4. Select the asset. The Inspector shows the empty `nodes` list.
5. (Recommended) From a one-shot Editor script, call
   `AetherResonance.PopulateDefaults()` once to seed the 12 canonical Moon 1 nodes.
   Or expand the list to 12 entries and fill them by hand — the canonical defaults
   are documented inline in `AetherResonanceTree.cs` (`PopulateDefaults()` method).
6. Save the project. Commit the `.asset` + its `.meta` file.

The runtime tree subscribes to `GameEvents.OnBuildingRestored` in its `OnEnable`,
so as soon as any system holds a reference to this `.asset` (a MonoBehaviour
`[SerializeField]`, an Addressables load, a `Resources.Load`), the wiring is live.

---

## Node layout — 12 nodes across 4 bands

| Band | Nodes (3 each) | Auto-unlocks when… |
|---|---|---|
| **Telluric** | Mud Tread → Earth Whisper → Telluric Bastion | `echohaven_crystalspire` restored |
| **Harmonic** | Tide Sense → Resonant Bell → Harmonic Pulse | `echohaven_harmonicfountain` restored |
| **Celestial** | Star Step → Lumen Veil → Celestial Beacon | `echohaven_stardome` restored |
| **Capstone** | Echohaven Awakened → Aether Choir → Seventeenth Hour | ALL 3 hero buildings restored |

Arrows above represent the `dependsOnNodeIds` chain inside each band. Capstones in
bands 1–3 depend on the prior node in the same band. The Capstone band's first
node (`capstone_echohaven_awakened`) requires all 3 band capstones to be unlocked.

---

## Auto-unlock vs manual unlock

- **Auto-unlock** happens inside `HandleBuildingRestored(buildingId)` when
  `GameEvents.OnBuildingRestored` fires. The tree maps the hero buildingId to
  its band via `_heroBandMap` and calls `TryUnlock` on every node in that band.
  Downstream nodes whose prereqs aren't yet satisfied stay locked and log loud.

- **Manual unlock** is for UI flows (the player clicks "Spend resonance" on a
  locked node). Call `AetherResonanceTree.TryUnlock(nodeId)` from your UI handler.
  The method returns `false` and logs **which specific prereq blocked it** when
  gates fail, per the 2026-06-02 NO-DEBT mandate (no silent fails).

---

## Save-system wiring (future work, tracked here for context)

The runtime `IsUnlocked` state on each node is `[NonSerialized]` — the asset on
disk is the *spec*, not the *progress*. When the save system lands, it should
mirror `node.IsUnlocked` to/from `PlayerProfile.unlockedSkillNodeIds` on save/load.
The tree itself does not write player progress to disk; that boundary belongs to
the save pipeline.

---

## Why this folder has a README and not a `.asset`

Cowork authors `.asset` files in the Editor to keep GUIDs and `fileID` references
stable. An agent writing a raw YAML asset from outside Unity will produce a file
Unity then re-stamps with new GUIDs on first import, breaking any scene reference
to the original. The README is the contract; Cowork is the authoring driver.
