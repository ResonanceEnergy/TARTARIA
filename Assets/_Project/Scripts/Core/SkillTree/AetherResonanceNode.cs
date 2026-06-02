// AetherResonanceNode.cs
// Owned by: Tartaria.Core.SkillTree (Systems Architect, 2026-06-02 sprint)
//
// Plain serializable node payload used by AetherResonanceTree (ScriptableObject).
// Lives in Tartaria.Core.SkillTree per API_CONTRACT.md — "SkillTree" is not on the
// banned namespace list (it does not shadow any UnityEngine type).

using System;

namespace Tartaria.Core.SkillTree
{
    /// <summary>
    /// Aether band this node belongs to. Matches the canonical Aether band naming
    /// resolved in CLAUDE.md (Telluric 7.83 Hz / Harmonic 432 Hz / Celestial 528 Hz)
    /// plus a Capstone slot that requires all three hero buildings restored.
    /// </summary>
    [Serializable]
    public enum AetherResonanceBand
    {
        Telluric  = 0,
        Harmonic  = 1,
        Celestial = 2,
        Capstone  = 3
    }

    /// <summary>
    /// One node in the Aether Resonance skill tree. Authored as a list entry inside
    /// an <see cref="AetherResonanceTree"/> ScriptableObject asset.
    ///
    /// Authoring contract (Cowork drives this from the Editor):
    ///   - <c>id</c> must be unique within the tree. Validated at unlock time.
    ///   - <c>displayName</c> + <c>description</c> are player-facing strings.
    ///   - <c>band</c> ties this node to a hero building (the band auto-unlock path).
    ///   - <c>requiredRestoredBuildings</c> is the integer floor of restored hero
    ///     buildings required before this node may unlock. Capstone nodes use 3.
    ///   - <c>dependsOnNodeIds</c> lists prerequisite node ids (all must be unlocked).
    ///   - <c>effectMagnitude</c> is the numeric payload consumers read (e.g. 0.20 =
    ///     +20% mini-game accuracy for "Resonant Bell").
    ///
    /// Per the 2026-06-02 NO-DEBT mandate (rule 4), <see cref="AetherResonanceTree"/>
    /// logs the exact prerequisite id that blocked any failed unlock attempt — this
    /// type stays a passive data carrier and does not silently mutate state.
    /// </summary>
    [Serializable]
    public class AetherResonanceNode
    {
        public string id;
        public string displayName;
        public string description;
        public AetherResonanceBand band;
        public int requiredRestoredBuildings;
        public string[] dependsOnNodeIds;
        public float effectMagnitude;

        /// <summary>
        /// Runtime-only unlocked state. Not serialized into the asset on disk; the
        /// tree's authoring asset is the spec, the runtime instance tracks progress.
        /// Cowork's save system mirrors this into PlayerProfile when wired.
        /// </summary>
        [NonSerialized] private bool _isUnlocked;
        public bool IsUnlocked
        {
            get => _isUnlocked;
            set => _isUnlocked = value;
        }
    }
}
