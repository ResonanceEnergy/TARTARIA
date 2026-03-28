using UnityEngine;
using System;
using System.Collections.Generic;

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
    /// </summary>
    public class SkillTreeSystem : MonoBehaviour
    {
        public static SkillTreeSystem Instance { get; private set; }

        readonly Dictionary<SkillTreeType, SkillTree> _trees = new();

        public event Action<SkillId> OnSkillUnlocked;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildTrees();
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

            float currentRS = Core.AetherFieldManager.Instance?.ResonanceScore ?? 0f;
            if (currentRS < node.rsCost) return false;

            Core.AetherFieldManager.Instance?.AddResonanceScore(-node.rsCost);
            node.isUnlocked = true;
            ApplySkillEffect(node);
            OnSkillUnlocked?.Invoke(id);
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
            float total = 0f;
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    if (node.isUnlocked && node.modifierType == mod)
                        total += node.modifierValue;
            return total;
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
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    if (node.id == id) return node;
            return null;
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

        // Historian tree (400+)
        His_LoreReveal   = 400,
        His_SecretPaths  = 401,
        His_MemoryEcho   = 402,
        His_AncientMap   = 403,
        His_TrueHistory  = 404
    }

    public enum SkillModifierType : byte
    {
        TuningPrecision   = 0,
        TuningSpeed       = 1,
        AetherCapacity    = 2,
        ComboDuration     = 3,
        RepairSpeed       = 4,
        BuildingResistance= 5,
        RSMultiplier      = 6,
        PulseDamage       = 7,
        ShieldDuration    = 8,
        StrikeRange       = 9
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
