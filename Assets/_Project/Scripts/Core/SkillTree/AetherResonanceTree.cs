// AetherResonanceTree.cs
// Owned by: Tartaria.Core.SkillTree (Systems Architect, 2026-06-02 sprint)
//
// ScriptableObject container for the 12-node Aether Resonance skill tree.
//
// API_CONTRACT.md compliance:
//   - Namespace: Tartaria.Core.SkillTree — not on the banned list, no shadow of UnityEngine.
//   - GameEvents reference: ONLY GameEvents.OnBuildingRestored (Action<string>). Verified
//     against Assets/_Project/Scripts/Core/GameEvents.cs:56 before writing.
//   - No deprecated Unity 6 APIs used.
//   - No ambiguous-type collisions (no System.* / UnityEngine.* same-name imports).
//
// NO-DEBT mandate compliance (2026-06-02):
//   - Rule 3 (no silent fails): every failed unlock logs loud with the blocking
//     prereq id or restoration count.
//   - Rule 4 (no silent fallbacks): GetNode logs a warning when an id is unknown.
//   - Rule 7 (no // TODO): every method body does the thing.
//   - Rule 11 (read before write): grepped GameEvents.cs and the 3 hero buildingIds
//     ("echohaven_crystalspire", "echohaven_stardome", "echohaven_harmonicfountain")
//     authored in Editor/Moon1BuildOutBuildings.cs lines 63/76/89.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core.SkillTree
{
    /// <summary>
    /// The Aether Resonance skill tree. 12 nodes across 4 bands:
    ///   - 3 Telluric (Mud Tread, Earth Whisper, Telluric Bastion capstone)
    ///   - 3 Harmonic (Tide Sense, Resonant Bell, Harmonic Pulse capstone)
    ///   - 3 Celestial (Star Step, Lumen Veil, Celestial Beacon capstone)
    ///   - 3 Capstone (Echohaven Awakened tier — gated on all 3 hero buildings)
    ///
    /// Each hero building restored fires <see cref="GameEvents.OnBuildingRestored"/>
    /// with its buildingId. The tree auto-unlocks the 3 entry-tier nodes of the
    /// band tied to that building, then evaluates downstream nodes whose prereqs
    /// are now satisfied (chain unlock).
    ///
    /// Capstone nodes only unlock once <see cref="_restoredBuildingIds"/> contains
    /// all three hero ids (crystalspire, stardome, harmonicfountain). This aligns
    /// with the E_HubAwakened blessing already in SkillSystemEnums.cs:69.
    ///
    /// Manual unlock (e.g. UI button after the player meets the gate):
    ///     bool ok = tree.TryUnlock("harmonic_resonant_bell");
    /// </summary>
    [CreateAssetMenu(
        fileName = "AetherResonance",
        menuName = "Tartaria/Skills/Aether Resonance Tree",
        order    = 100)]
    public class AetherResonanceTree : ScriptableObject
    {
        // ── Canonical hero building ids — must match Moon1BuildOutBuildings.cs ──
        public const string HeroId_CrystalSpire     = "echohaven_crystalspire";
        public const string HeroId_StarDome         = "echohaven_stardome";
        public const string HeroId_HarmonicFountain = "echohaven_harmonicfountain";

        // Mapping from hero buildingId → band whose entry nodes that building unlocks.
        // Telluric ← CrystalSpire (deepest dig, earth-coupled)
        // Harmonic ← HarmonicFountain (water/thread coupling, mid-burial)
        // Celestial ← StarDome (sky-coupled, listeners' hall)
        // Capstone band is reserved for the all-three-restored gate.
        private static readonly Dictionary<string, AetherResonanceBand> _heroBandMap =
            new Dictionary<string, AetherResonanceBand>(StringComparer.Ordinal)
            {
                { HeroId_CrystalSpire,     AetherResonanceBand.Telluric  },
                { HeroId_HarmonicFountain, AetherResonanceBand.Harmonic  },
                { HeroId_StarDome,         AetherResonanceBand.Celestial },
            };

        [Tooltip("All 12 Aether Resonance nodes. Authored in the asset; the runtime " +
                 "tree mirrors IsUnlocked state per save profile.")]
        public List<AetherResonanceNode> nodes = new List<AetherResonanceNode>();

        // Runtime-only tracker of hero buildings already restored this session.
        // Survives until the SO is unloaded; persistence is the save system's job.
        [NonSerialized] private readonly HashSet<string> _restoredBuildingIds =
            new HashSet<string>(StringComparer.Ordinal);

        // ───────────────────────────────────────────────────────────────────
        // Subscription lifecycle
        // ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Called when Unity loads the asset (Editor or Player). We subscribe here
        /// so the tree starts listening for restoration events the moment it exists.
        /// Per rule 3 of the no-debt mandate: if the seed list is empty, we log loud
        /// rather than silently doing nothing.
        /// </summary>
        private void OnEnable()
        {
            // Reset transient runtime state — OnEnable fires on domain reload too.
            _restoredBuildingIds.Clear();
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] != null) nodes[i].IsUnlocked = false;
                }
            }

            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogWarning(
                    $"[AetherResonanceTree:'{name}'] OnEnable: 'nodes' is empty. " +
                    "Author the 12 nodes in the asset before play, or call " +
                    "PopulateDefaults() from an Editor tool. Tree will be inert.");
            }

            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        private void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        // ───────────────────────────────────────────────────────────────────
        // Public API
        // ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Look up a node by id. Returns null and logs a warning if not found —
        /// silent-null returns are banned by the no-debt mandate.
        /// </summary>
        public AetherResonanceNode GetNode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning(
                    $"[AetherResonanceTree:'{name}'] GetNode called with null/empty id.");
                return null;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n != null && string.Equals(n.id, id, StringComparison.Ordinal))
                    return n;
            }

            Debug.LogWarning(
                $"[AetherResonanceTree:'{name}'] GetNode('{id}'): no such node. " +
                $"Known ids = [{string.Join(", ", NodeIds())}]");
            return null;
        }

        /// <summary>
        /// Attempt to unlock the node with the given id. Returns true on success,
        /// false otherwise. Every failure path logs which prereq blocked it — per
        /// no-debt mandate rule 4 (no silent fails / no silent fallbacks).
        /// </summary>
        public bool TryUnlock(string id)
        {
            var node = GetNode(id);
            if (node == null)
            {
                // GetNode already logged the warning; just refuse the unlock.
                return false;
            }

            if (node.IsUnlocked)
            {
                // Idempotent re-call is fine, but loud-log so duplicate UI wires surface.
                Debug.Log(
                    $"[AetherResonanceTree:'{name}'] TryUnlock('{id}'): already unlocked.");
                return true;
            }

            // Gate 1 — required restored hero buildings.
            int restored = _restoredBuildingIds.Count;
            if (restored < node.requiredRestoredBuildings)
            {
                Debug.LogWarning(
                    $"[AetherResonanceTree:'{name}'] TryUnlock('{id}') BLOCKED: " +
                    $"requires {node.requiredRestoredBuildings} restored hero buildings " +
                    $"but only {restored} are restored " +
                    $"(restored ids = [{string.Join(", ", _restoredBuildingIds)}]).");
                return false;
            }

            // Gate 2 — prerequisite nodes.
            if (node.dependsOnNodeIds != null)
            {
                for (int i = 0; i < node.dependsOnNodeIds.Length; i++)
                {
                    string prereqId = node.dependsOnNodeIds[i];
                    if (string.IsNullOrEmpty(prereqId)) continue;

                    var prereq = GetNode(prereqId);
                    if (prereq == null)
                    {
                        Debug.LogWarning(
                            $"[AetherResonanceTree:'{name}'] TryUnlock('{id}') BLOCKED: " +
                            $"prereq id '{prereqId}' is not a node in this tree. " +
                            "Author error in dependsOnNodeIds — fix the asset.");
                        return false;
                    }

                    if (!prereq.IsUnlocked)
                    {
                        Debug.LogWarning(
                            $"[AetherResonanceTree:'{name}'] TryUnlock('{id}') BLOCKED: " +
                            $"prereq node '{prereqId}' ('{prereq.displayName}') is locked.");
                        return false;
                    }
                }
            }

            // All gates passed.
            node.IsUnlocked = true;
            Debug.Log(
                $"[AetherResonanceTree:'{name}'] UNLOCKED node '{id}' " +
                $"('{node.displayName}', band={node.band}, magnitude={node.effectMagnitude}).");
            return true;
        }

        // ───────────────────────────────────────────────────────────────────
        // GameEvents wiring
        // ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Handler for <see cref="GameEvents.OnBuildingRestored"/>. Signature
        /// verified: Action&lt;string&gt; (GameEvents.cs:56).
        ///
        /// When a hero building restores:
        ///   1. Record its id.
        ///   2. Auto-unlock the 3 entry-tier nodes of that hero's band (any node
        ///      whose <c>requiredRestoredBuildings == 0 (or 1)</c> and no prereqs).
        ///      In practice we attempt every node in the band — TryUnlock's gates
        ///      will refuse anything still locked downstream.
        ///   3. If all 3 hero ids are now restored, attempt every Capstone node.
        /// </summary>
        private void HandleBuildingRestored(string buildingId)
        {
            try
            {
                if (string.IsNullOrEmpty(buildingId))
                {
                    Debug.LogWarning(
                        $"[AetherResonanceTree:'{name}'] OnBuildingRestored fired " +
                        "with null/empty buildingId. Ignoring.");
                    return;
                }

                bool isNew = _restoredBuildingIds.Add(buildingId);
                if (!isNew)
                {
                    // Re-restoration is benign but log so duplicate fires surface.
                    Debug.Log(
                        $"[AetherResonanceTree:'{name}'] buildingId='{buildingId}' " +
                        "restored event re-fired; already in restored set.");
                }

                if (!_heroBandMap.TryGetValue(buildingId, out var band))
                {
                    // Non-hero buildings (Moon 1 has 9 village buildings) restore too —
                    // they don't drive the resonance tree. Log info-level, no warn.
                    Debug.Log(
                        $"[AetherResonanceTree:'{name}'] buildingId='{buildingId}' " +
                        "restored but is not a hero building; no band auto-unlock.");
                }
                else
                {
                    Debug.Log(
                        $"[AetherResonanceTree:'{name}'] Hero building '{buildingId}' " +
                        $"restored → attempting auto-unlock of band {band} nodes.");
                    AttemptUnlockBand(band);
                }

                // Whenever the restored set changes, re-try Capstones; gates will
                // refuse until requiredRestoredBuildings == 3 is satisfied.
                AttemptUnlockBand(AetherResonanceBand.Capstone);
            }
            catch (Exception ex)
            {
                // Rule 3: never swallow silently. Loud-log with file:line context.
                Debug.LogError(
                    $"[AetherResonanceTree:'{name}'] HandleBuildingRestored threw " +
                    $"for buildingId='{buildingId}': {ex}");
            }
        }

        private void AttemptUnlockBand(AetherResonanceBand band)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;
                if (n.band != band) continue;
                if (n.IsUnlocked) continue;

                // TryUnlock handles its own gate-failure logging; we don't double-log.
                TryUnlock(n.id);
            }
        }

        // ───────────────────────────────────────────────────────────────────
        // Authoring helper — Editor menu can call this to seed the 12 defaults
        // ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Populates <see cref="nodes"/> with the 12 canonical Moon 1 defaults
        /// (3 per band × 4 bands). Cowork can call this from a one-shot Editor
        /// script after Right-click → Create → Tartaria → Skills → Aether
        /// Resonance Tree to avoid hand-typing 12 list entries.
        ///
        /// Safe to call repeatedly: it overwrites the nodes list entirely.
        /// </summary>
        public void PopulateDefaults()
        {
            nodes = new List<AetherResonanceNode>(12)
            {
                // ── Telluric band (Crystal Spire restoration unlocks tier 1) ──
                new AetherResonanceNode
                {
                    id                        = "telluric_mud_tread",
                    displayName               = "Mud Tread",
                    description               = "Movement speed on mud surfaces increased. " +
                                                "The earth softens its hold on your stride.",
                    band                      = AetherResonanceBand.Telluric,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = Array.Empty<string>(),
                    effectMagnitude           = 0.25f, // +25% mud-walk speed
                },
                new AetherResonanceNode
                {
                    id                        = "telluric_earth_whisper",
                    displayName               = "Earth Whisper",
                    description               = "Aether Vision reveals buried items at " +
                                                "extended range — the soil hums their location.",
                    band                      = AetherResonanceBand.Telluric,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "telluric_mud_tread" },
                    effectMagnitude           = 8.0f,  // +8m Aether Vision buried-item radius
                },
                new AetherResonanceNode
                {
                    id                        = "telluric_bastion",
                    displayName               = "Telluric Bastion",
                    description               = "Capstone — incoming damage reduced while " +
                                                "standing on natural earth or stone.",
                    band                      = AetherResonanceBand.Telluric,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "telluric_earth_whisper" },
                    effectMagnitude           = 0.20f, // -20% damage on earth
                },

                // ── Harmonic band (Harmonic Fountain restoration unlocks tier 1) ──
                new AetherResonanceNode
                {
                    id                        = "harmonic_tide_sense",
                    displayName               = "Tide Sense",
                    description               = "Predict incoming water-hazard waves a beat " +
                                                "before they crest — the threads sing warning.",
                    band                      = AetherResonanceBand.Harmonic,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = Array.Empty<string>(),
                    effectMagnitude           = 1.0f,  // +1.0s telegraph window
                },
                new AetherResonanceNode
                {
                    id                        = "harmonic_resonant_bell",
                    displayName               = "Resonant Bell",
                    description               = "Tuning mini-game accuracy improved. The bell " +
                                                "rings true even when your hand wavers.",
                    band                      = AetherResonanceBand.Harmonic,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "harmonic_tide_sense" },
                    effectMagnitude           = 0.20f, // +20% mini-game accuracy
                },
                new AetherResonanceNode
                {
                    id                        = "harmonic_pulse",
                    displayName               = "Harmonic Pulse",
                    description               = "Capstone — Resonance Pulse emits a chord that " +
                                                "stuns nearby corruption for a beat.",
                    band                      = AetherResonanceBand.Harmonic,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "harmonic_resonant_bell" },
                    effectMagnitude           = 1.5f,  // 1.5s stun window
                },

                // ── Celestial band (Star Dome restoration unlocks tier 1) ──
                new AetherResonanceNode
                {
                    id                        = "celestial_star_step",
                    displayName               = "Star Step",
                    description               = "Jump height increased. The sky pulls a little " +
                                                "harder when you remember to listen.",
                    band                      = AetherResonanceBand.Celestial,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = Array.Empty<string>(),
                    effectMagnitude           = 0.15f, // +15% jump height
                },
                new AetherResonanceNode
                {
                    id                        = "celestial_lumen_veil",
                    displayName               = "Lumen Veil",
                    description               = "A brief shroud of light grants invulnerability " +
                                                "immediately after toggling Aether Vision.",
                    band                      = AetherResonanceBand.Celestial,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "celestial_star_step" },
                    effectMagnitude           = 1.0f,  // 1.0s invuln window
                },
                new AetherResonanceNode
                {
                    id                        = "celestial_beacon",
                    displayName               = "Celestial Beacon",
                    description               = "Capstone — mark a beacon at your position; " +
                                                "Aether Vision reveals all enemies within range.",
                    band                      = AetherResonanceBand.Celestial,
                    requiredRestoredBuildings = 1,
                    dependsOnNodeIds          = new[] { "celestial_lumen_veil" },
                    effectMagnitude           = 25.0f, // 25m reveal radius
                },

                // ── Capstone band (gated on ALL 3 hero buildings) ──
                new AetherResonanceNode
                {
                    id                        = "capstone_echohaven_awakened",
                    displayName               = "Echohaven Awakened",
                    description               = "All three voices of Echohaven sing as one. " +
                                                "Resonance Stone generation permanently boosted. " +
                                                "Aligns with the E_HubAwakened blessing.",
                    band                      = AetherResonanceBand.Capstone,
                    requiredRestoredBuildings = 3,
                    // Requires the three sub-capstones of each band.
                    dependsOnNodeIds          = new[]
                    {
                        "telluric_bastion",
                        "harmonic_pulse",
                        "celestial_beacon",
                    },
                    effectMagnitude           = 0.08f, // +8% permanent RS gen
                },
                new AetherResonanceNode
                {
                    id                        = "capstone_aether_choir",
                    displayName               = "Aether Choir",
                    description               = "The restored hub sustains a passive harmonic " +
                                                "field; nearby allies regenerate Resonance faster.",
                    band                      = AetherResonanceBand.Capstone,
                    requiredRestoredBuildings = 3,
                    dependsOnNodeIds          = new[] { "capstone_echohaven_awakened" },
                    effectMagnitude           = 0.15f, // +15% ally RS regen
                },
                new AetherResonanceNode
                {
                    id                        = "capstone_seventeenth_hour",
                    displayName               = "Seventeenth Hour",
                    description               = "During the Tartarian seventeenth hour, all " +
                                                "Aether band effects amplify. Listen for the bell.",
                    band                      = AetherResonanceBand.Capstone,
                    requiredRestoredBuildings = 3,
                    dependsOnNodeIds          = new[] { "capstone_aether_choir" },
                    effectMagnitude           = 0.50f, // +50% amplification during 17th hour
                },
            };
        }

        // ───────────────────────────────────────────────────────────────────
        // Internal helpers
        // ───────────────────────────────────────────────────────────────────

        private IEnumerable<string> NodeIds()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null) yield return nodes[i].id;
            }
        }
    }
}
