using UnityEngine;
using System;
using System.Collections.Generic;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Skill Tree Framework -- 4 trees matching the player archetypes:
    ///
    ///   Resonator  -- Frequency mastery, Aether channeling, tuning precision
    ///   Architect  -- Building enhancement, repair speed, structural bonuses
    ///   Guardian   -- Combat skills, shield strength, damage output
    ///   Historian  -- Lore discovery, hidden area reveals, RS bonuses
    ///
    /// Skills are unlocked by spending Resonance Score (RS).
    /// Each tree has 5 tiers with prerequisites.
    ///
    /// Moon 2 Extension (Progression Agent): 6 Lunar Purge Blessings/Mutations (500+) auto-granted
    /// by Moon2ProgressionSystem upon restoring/purging the 5 key Crystalline Caverns sites
    /// (cathedral_dome, bell_tower, fountain, crystal_hall, ley_chamber). These represent permanent
    /// "echoes of the purge" — the corruption you cleanse leaves lasting power and visual mutations in the player.
    /// Tied directly to "purge the corruption" fantasy. Nodes integrate seamlessly with existing save/restore/UI/modifier system.
    /// </summary>
    public class SkillTreeSystem : MonoBehaviour
    {
        public static SkillTreeSystem Instance { get; private set; }

        readonly Dictionary<SkillTreeType, SkillTree> _trees = new();
        readonly Dictionary<SkillId, SkillNode> _nodeLookup = new();
        readonly Dictionary<SkillModifierType, float> _modifierCache = new();
        bool _modifierCacheDirty = true;

        public event Action<SkillId> OnSkillUnlocked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("SkillTreeSystem");
            DontDestroyOnLoad(go);
            go.AddComponent<SkillTreeSystem>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            BuildTrees();
            BuildNodeLookup();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void BuildNodeLookup()
        {
            _nodeLookup.Clear();
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    _nodeLookup[node.id] = node;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Attempt to unlock a skill. Returns true if successful.
        /// Deducts RS cost from AetherFieldManager.
        /// </summary>
        public bool TryUnlockSkill(SkillId id)
        {
            var node = FindNode(id);
            if (node == null || node.isUnlocked) return false;
            if (!ArePrereqsMet(node)) return false;

            float currentRS = AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            if (currentRS < node.rsCost) return false;

            AetherFieldManager.Instance?.AddResonanceScore(-node.rsCost);
            node.isUnlocked = true;
            _modifierCacheDirty = true;
            ApplySkillEffect(node);
            OnSkillUnlocked?.Invoke(id);
            AudioManager.Instance?.PlaySFX2D("SkillUnlocked");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            return true;
        }

        /// <summary>
        /// Check if a specific skill is unlocked.
        /// </summary>
        public bool IsSkillUnlocked(SkillId id)
        {
            var node = FindNode(id);
            return node?.isUnlocked ?? false;
        }

        /// <summary>
        /// Moon 2 Progression Hook: Force-unlock a cavern purge blessing (0 RS cost, no prereq gate for blessings).
        /// Called exclusively by Moon2ProgressionSystem when a key site (cathedral/bell/fountain/crystal/ley) is restored + purged.
        /// The blessing persists via normal GetSaveData / skill save. Feels like a true earned permanent mutation.
        /// </summary>
        public void ForceUnlockMoon2Blessing(SkillId id)
        {
            var node = FindNode(id);
            if (node == null || node.isUnlocked) return;

            node.isUnlocked = true;
            _modifierCacheDirty = true;
            ApplySkillEffect(node);
            OnSkillUnlocked?.Invoke(id);
            AudioManager.Instance?.PlaySFX2D("SkillUnlocked");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[SkillTree] Moon 2 Purge Blessing granted (permanent): {node.displayName}");
        }

        /// <summary>
        /// Get all nodes in a given tree.
        /// </summary>
        static readonly List<SkillNode> EmptyNodes = new();

        public IReadOnlyList<SkillNode> GetTree(SkillTreeType tree)
        {
            return _trees.TryGetValue(tree, out var t) ? t.nodes.AsReadOnly() : EmptyNodes.AsReadOnly();
        }

        /// <summary>
        /// Get the cumulative modifier for a given stat from all unlocked skills.
        /// </summary>
        public float GetModifier(SkillModifierType mod)
        {
            if (_modifierCacheDirty)
                RebuildModifierCache();
            return _modifierCache.TryGetValue(mod, out float val) ? val : 0f;
        }

        void RebuildModifierCache()
        {
            _modifierCache.Clear();
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    if (node.isUnlocked)
                    {
                        if (!_modifierCache.ContainsKey(node.modifierType))
                            _modifierCache[node.modifierType] = 0f;
                        _modifierCache[node.modifierType] += node.modifierValue;
                    }
            _modifierCacheDirty = false;
        }

        // ─── Save / Restore ─────────────────────────

        public SkillTreeSaveData GetSaveData()
        {
            var data = new SkillTreeSaveData { unlockedSkills = new List<int>() };
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    if (node.isUnlocked)
                        data.unlockedSkills.Add((int)node.id);
            return data;
        }

        public void RestoreFromSave(SkillTreeSaveData data)
        {
            // Reset all first
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    node.isUnlocked = false;

            if (data?.unlockedSkills == null) return;
            foreach (int id in data.unlockedSkills)
            {
                var node = FindNode((SkillId)id);
                if (node != null)
                {
                    node.isUnlocked = true;
                    ApplySkillEffect(node);
                }
            }
            _modifierCacheDirty = true;
        }

        // ─── Tree Construction ───────────────────────

        void BuildTrees()
        {
            _trees[SkillTreeType.Resonator] = BuildResonatorTree();
            _trees[SkillTreeType.Architect] = BuildArchitectTree();
            _trees[SkillTreeType.Guardian] = BuildGuardianTree();
            _trees[SkillTreeType.Historian] = BuildHistorianTree();
        }

        SkillTree BuildResonatorTree()
        {
            var tree = new SkillTree { type = SkillTreeType.Resonator };
            tree.nodes.Add(new SkillNode(SkillId.Res_FreqSense, 1, 50f,
                "Frequency Sense", "See Aether frequency values on buildings.",
                SkillModifierType.TuningPrecision, 0.1f));
            tree.nodes.Add(new SkillNode(SkillId.Res_TuneSpeed, 2, 120f,
                "Rapid Tuning", "+20% tuning mini-game time limit.",
                SkillModifierType.TuningSpeed, 0.2f,
                SkillId.Res_FreqSense));
            tree.nodes.Add(new SkillNode(SkillId.Res_AetherPool, 2, 150f,
                "Aether Reservoir", "+25% max Aether capacity.",
                SkillModifierType.AetherCapacity, 0.25f,
                SkillId.Res_FreqSense));
            tree.nodes.Add(new SkillNode(SkillId.Res_Cascade, 3, 250f,
                "Golden Cascade", "Golden Cascade combo extends to 15 hits.",
                SkillModifierType.ComboDuration, 0.25f,
                SkillId.Res_TuneSpeed));
            tree.nodes.Add(new SkillNode(SkillId.Res_MasterFreq, 4, 500f,
                "Master Frequency", "Tuning success rate +40%. Unlock harmonic chaining.",
                SkillModifierType.TuningPrecision, 0.4f,
                SkillId.Res_Cascade));

            // Moon 2 Cavern Purge Blessings (Resonator lunar theme) — granted by Moon2ProgressionSystem on key site purge/restore.
            // These are the permanent mutations: player literally carries the purified power of the caverns forever.
            tree.nodes.Add(new SkillNode(SkillId.M2_CathedralBreath, 3, 0f,
                "Cathedral's Eternal Breath", "Moon 2 Purge Blessing (cathedral_dome): The Grand Cathedral's living dome now breathes within you. +15% Resonance Score from all cavern restorations and purges. The corruption you burned away empowers your future.",
                SkillModifierType.LunarRSBonus, 0.15f));
            tree.nodes.Add(new SkillNode(SkillId.M2_BellCleansing, 3, 0f,
                "Bell of Cleansing Chime", "Moon 2 Purge Blessing (bell_tower): The Bell Tower's pure tone resonates in your staff. Perfect frequency matches now emit a cleansing chime that weakens nearby corruption nodes (especially powerful in micro-giant). Permanent echo of the purge.",
                SkillModifierType.PulseDamage, 0.12f));
            tree.nodes.Add(new SkillNode(SkillId.M2_FountainSpring, 4, 0f,
                "Aetheric Spring's Grace", "Moon 2 Purge Blessing (fountain): Living water from the fountain flows through your blood. -25% corruption spread rate globally + passive Aether regeneration near restored Moon 2 fountains. You have become a living counter-current to the Mud.",
                SkillModifierType.CorruptionResistance, 0.25f));

            return tree;
        }

        SkillTree BuildArchitectTree()
        {
            var tree = new SkillTree { type = SkillTreeType.Architect };
            tree.nodes.Add(new SkillNode(SkillId.Arc_BlueprintScan, 1, 50f,
                "Blueprint Scanner", "See building blueprints at 50m range.",
                SkillModifierType.RepairSpeed, 0.1f));
            tree.nodes.Add(new SkillNode(SkillId.Arc_QuickRepair, 2, 120f,
                "Rapid Repair", "+30% building repair speed.",
                SkillModifierType.RepairSpeed, 0.3f,
                SkillId.Arc_BlueprintScan));
            tree.nodes.Add(new SkillNode(SkillId.Arc_Fortify, 2, 140f,
                "Structural Fortify", "Repaired buildings +20% corruption resistance.",
                SkillModifierType.BuildingResistance, 0.2f,
                SkillId.Arc_BlueprintScan));
            tree.nodes.Add(new SkillNode(SkillId.Arc_MassRestore, 3, 300f,
                "Mass Restoration", "Repair 3 buildings simultaneously.",
                SkillModifierType.RepairSpeed, 0.5f,
                SkillId.Arc_QuickRepair));
            tree.nodes.Add(new SkillNode(SkillId.Arc_GoldenRatio, 4, 500f,
                "Golden Ratio Mastery", "Buildings auto-align to phi proportions. +50% RS from restored buildings.",
                SkillModifierType.RSMultiplier, 0.5f,
                SkillId.Arc_MassRestore));
            return tree;
        }

        SkillTree BuildGuardianTree()
        {
            var tree = new SkillTree { type = SkillTreeType.Guardian };
            tree.nodes.Add(new SkillNode(SkillId.Grd_StrongPulse, 1, 50f,
                "Potent Pulse", "Resonance Pulse damage +15%.",
                SkillModifierType.PulseDamage, 0.15f));
            tree.nodes.Add(new SkillNode(SkillId.Grd_ShieldDuration, 2, 110f,
                "Extended Shield", "Frequency Shield lasts 5s instead of 3s.",
                SkillModifierType.ShieldDuration, 2f,
                SkillId.Grd_StrongPulse));
            tree.nodes.Add(new SkillNode(SkillId.Grd_StrikeRange, 2, 130f,
                "Harmonic Reach", "Harmonic Strike range +30%.",
                SkillModifierType.StrikeRange, 0.3f,
                SkillId.Grd_StrongPulse));
            tree.nodes.Add(new SkillNode(SkillId.Grd_AOEPurge, 3, 280f,
                "Purification Wave", "Resonance Pulse also purges corruption in AOE.",
                SkillModifierType.PulseDamage, 0.25f,
                SkillId.Grd_ShieldDuration));
            tree.nodes.Add(new SkillNode(SkillId.Grd_Invulnerable, 4, 500f,
                "Harmonic Immunity", "3s invulnerability after perfect combo. 30s cooldown.",
                SkillModifierType.ShieldDuration, 3f,
                SkillId.Grd_AOEPurge));

            // Round 4 Giant production nodes (synergies, flight, terrain, forms, 180s Titan, harmony)
            tree.nodes.Add(new SkillNode(SkillId.Grd_TitanFlight, 3, 320f, "Titan Soar", "Unlock Titan flight (physics+input+camera) in giant.", SkillModifierType.StrikeRange, 0.2f, SkillId.Grd_StrikeRange));
            tree.nodes.Add(new SkillNode(SkillId.Grd_EarthShaper, 3, 340f, "Earth Shaper", "Real terrain deformation via giant.", SkillModifierType.PulseDamage, 0.1f, SkillId.Grd_AOEPurge));
            tree.nodes.Add(new SkillNode(SkillId.Grd_WorldMover, 4, 480f, "World Mover", "Large scale terrain + object shift.", SkillModifierType.ShieldDuration, 1.5f, SkillId.Grd_EarthShaper));
            tree.nodes.Add(new SkillNode(SkillId.Grd_AncestralTitan, 4, 410f, "Ancestral Titan", "Historical giant form visuals + buffs.", SkillModifierType.PulseDamage, 0.15f, SkillId.Grd_Invulnerable));
            tree.nodes.Add(new SkillNode(SkillId.Grd_ColossusForm, 4, 550f, "Living Colossus", "Triple synergy giant cathedral form.", SkillModifierType.PulseDamage, 0.35f, SkillId.Grd_WorldMover));
            tree.nodes.Add(new SkillNode(SkillId.Grd_AvatarForm, 5, 720f, "Avatar of the First", "Ultimate 180s+ avatar form.", SkillModifierType.ShieldDuration, 4f, SkillId.Grd_ColossusForm));
            tree.nodes.Add(new SkillNode(SkillId.Grd_GiantResonanceHarmony, 3, 380f, "Cassian/Anastasia Resonance", "Narrative giant harmony synergy.", SkillModifierType.ComboDuration, 2f, SkillId.Grd_TitanFlight));
            tree.nodes.Add(new SkillNode(SkillId.Grd_TitanStability, 4, 450f, "Titan Endurance", "180s Titan stability + flight efficiency.", SkillModifierType.ShieldDuration, 2f, SkillId.Grd_Invulnerable));
            tree.nodes.Add(new SkillNode(SkillId.Grd_AbilityCooldownMastery, 3, 290f, "Giant's Reflex", "40% faster giant ability cooldowns.", SkillModifierType.StrikeRange, 0.1f, SkillId.Grd_StrongPulse));

            // Moon 2 Cavern Purge Blessings (Guardian micro-giant + combat theme) — permanent mutations from deep cavern purges
            tree.nodes.Add(new SkillNode(SkillId.M2_CrystalLens, 3, 0f,
                "Fractal Crystal Lens", "Moon 2 Purge Blessing (crystal_hall): The Crystal Hall's fractal geometry now lives in your eyes. Corruption nodes, veins and hidden fractal structures glow visibly without needing the Dissonance Lens inside the caverns. Permanent mutation — you see the world's wounds clearly.",
                SkillModifierType.StrikeRange, 0.15f, SkillId.Grd_AOEPurge));
            tree.nodes.Add(new SkillNode(SkillId.M2_LeyBond, 4, 0f,
                "Ley Heart Bond", "Moon 2 Purge Blessing (ley_chamber): Your spirit is now bound to the ancient ley grid of the caverns. +20% micro-giant duration while inside Moon 2 + living ley sparks orbit your form as a visible sigil of the purge. You walk the veins of the world.",
                SkillModifierType.MicroGiantExtend, 0.20f, SkillId.Grd_TitanFlight));
            tree.nodes.Add(new SkillNode(SkillId.M2_TrueLunarPurifier, 5, 0f,
                "True Lunar Purifier", "Moon 2 Capstone Blessing: All five key sites of the Crystalline Caverns purged. You have become the living antithesis to corruption. Minor corruption auto-purges on any restore, +50% RS from Moon 2 activities, and every cavern purge triggers a golden cascade visual. The Mud itself recoils from your presence. Permanent ultimate mutation.",
                SkillModifierType.RSMultiplier, 0.5f, SkillId.M2_LeyBond));

            return tree;
        }

        SkillTree BuildHistorianTree()
        {
            var tree = new SkillTree { type = SkillTreeType.Historian };
            tree.nodes.Add(new SkillNode(SkillId.His_LoreReveal, 1, 40f,
                "Lore Sight", "Hidden inscriptions glow within 30m.",
                SkillModifierType.RSMultiplier, 0.1f));
            tree.nodes.Add(new SkillNode(SkillId.His_SecretPaths, 2, 100f,
                "Secret Paths", "Reveal hidden passages in buildings.",
                SkillModifierType.RSMultiplier, 0.15f,
                SkillId.His_LoreReveal));
            tree.nodes.Add(new SkillNode(SkillId.His_MemoryEcho, 2, 110f,
                "Memory Echo", "Hear echoes of building history when nearby.",
                SkillModifierType.RSMultiplier, 0.1f,
                SkillId.His_LoreReveal));
            tree.nodes.Add(new SkillNode(SkillId.His_AncientMap, 3, 250f,
                "Ancient Cartography", "Full zone map revealed including buried structures.",
                SkillModifierType.RSMultiplier, 0.2f,
                SkillId.His_SecretPaths));
            tree.nodes.Add(new SkillNode(SkillId.His_TrueHistory, 4, 500f,
                "True History", "All lore auto-collected. +100% RS from discoveries.",
                SkillModifierType.RSMultiplier, 1.0f,
                SkillId.His_AncientMap));
            return tree;
        }

        // ─── Helpers ─────────────────────────────────

        SkillNode FindNode(SkillId id)
        {
            return _nodeLookup.TryGetValue(id, out var node) ? node : null;
        }

        bool ArePrereqsMet(SkillNode node)
        {
            if (node.prerequisite == SkillId.None) return true;
            var prereq = FindNode(node.prerequisite);
            return prereq?.isUnlocked ?? false;
        }

        void ApplySkillEffect(SkillNode node)
        {
            // Effects are queried via GetModifier() -- no immediate side effect needed.
            Debug.Log($"[SkillTree] Unlocked: {node.displayName} (+{node.modifierValue} {node.modifierType})");
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum SkillTreeType : byte
    {
        Resonator = 0,   // Frequency mastery
        Architect = 1,   // Building enhancement
        Guardian  = 2,   // Combat skills
        Historian = 3    // Lore and discovery
    }

    public enum SkillId : int
    {
        None = 0,

        // Resonator tree (100+)
        Res_FreqSense   = 100,
        Res_TuneSpeed   = 101,
        Res_AetherPool  = 102,
        Res_Cascade     = 103,
        Res_MasterFreq  = 104,

        // Architect tree (200+)
        Arc_BlueprintScan = 200,
        Arc_QuickRepair   = 201,
        Arc_Fortify       = 202,
        Arc_MassRestore   = 203,
        Arc_GoldenRatio   = 204,

        // Guardian tree (300+)
        Grd_StrongPulse    = 300,
        Grd_ShieldDuration = 301,
        Grd_StrikeRange    = 302,
        Grd_AOEPurge       = 303,
        Grd_Invulnerable   = 304,
        // Round 4: Giant advanced (flight, EarthShaper, WorldMover, Ancestral/Colossus/Avatar, Cassian/Anastasia harmony, Titan 180s, cooldowns)
        Grd_TitanFlight = 305,
        Grd_EarthShaper = 306,
        Grd_WorldMover = 307,
        Grd_AncestralTitan = 308,
        Grd_ColossusForm = 309,
        Grd_AvatarForm = 310,
        Grd_GiantResonanceHarmony = 311,
        Grd_TitanStability = 312,
        Grd_AbilityCooldownMastery = 313,

        // Historian tree (400+)
        His_LoreReveal   = 400,
        His_SecretPaths  = 401,
        His_MemoryEcho   = 402,
        His_AncientMap   = 403,
        His_TrueHistory  = 404,

        // Moon 2 (Lunar Moon / Crystalline Caverns) Permanent Purge Blessings & Mutations (500+)
        // These are the core progression hooks. Auto-granted (no RS spend) by Moon2ProgressionSystem when player purges/restores the five key sites.
        // They make progression feel deeply tied to the "purge the corruption" fantasy: each restored cathedral, bell, fountain, hall, and ley chamber leaves an indelible, powerful, visual change in the player that carries forward.
        M2_CathedralBreath   = 500,
        M2_BellCleansing     = 501,
        M2_FountainSpring    = 502,
        M2_CrystalLens       = 503,
        M2_LeyBond           = 504,
        M2_TrueLunarPurifier = 505
    }

    public enum SkillModifierType : byte
    {
        TuningPrecision    = 0,
        TuningSpeed        = 1,
        AetherCapacity     = 2,
        ComboDuration      = 3,
        RepairSpeed        = 4,
        BuildingResistance = 5,
        RSMultiplier       = 6,
        PulseDamage        = 7,
        ShieldDuration     = 8,
        StrikeRange        = 9,
        // Moon 2 progression extensions (used by lunar purge blessings)
        CorruptionResistance = 10,
        LunarRSBonus         = 11,
        MicroGiantExtend     = 12
    }

    [Serializable]
    public class SkillNode
    {
        public SkillId id;
        public int tier;
        public float rsCost;
        public string displayName;
        public string description;
        public SkillModifierType modifierType;
        public float modifierValue;
        public SkillId prerequisite;
        public bool isUnlocked;

        public SkillNode(SkillId id, int tier, float rsCost,
            string displayName, string description,
            SkillModifierType modifierType, float modifierValue,
            SkillId prerequisite = SkillId.None)
        {
            this.id = id;
            this.tier = tier;
            this.rsCost = rsCost;
            this.displayName = displayName;
            this.description = description;
            this.modifierType = modifierType;
            this.modifierValue = modifierValue;
            this.prerequisite = prerequisite;
        }
    }

    [Serializable]
    public class SkillTree
    {
        public SkillTreeType type;
        public List<SkillNode> nodes = new();
    }

    [Serializable]
    public class SkillTreeSaveData
    {
        public List<int> unlockedSkills;
    }
}
