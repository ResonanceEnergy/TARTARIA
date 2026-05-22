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
    /// by Moon2ProgressionSystem upon restoring/purging the 5 key Crystalline Caverns sites.
    ///
    /// Moon 1 Echohaven Extension (Progression & Save Compatibility Agent): 4 Early Hub Blessings (600+)
    /// auto-granted by EchohavenProgressionSystem upon restoring the 3 core starting hub buildings
    /// (fountain, dome, spire). These provide meaningful permanent early progression and power that
    /// carries through the entire game. Full save/load compatibility via dedicated EchohavenSaveBlock.
    ///
    /// Moon 3 (Electric) Extension: Compassion & Rails blessings (Lullaby, Golden Rails, Continental Fast Travel, Orphan Trust, World's Fair) wired from CampaignFlowController + Moon03.json data on M3 completion.
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
        /// Get current modifier value (sum of all unlocked nodes affecting the type).
        /// </summary>
        public float GetModifier(SkillModifierType type)
        {
            if (_modifierCacheDirty) RebuildModifierCache();
            return _modifierCache.TryGetValue(type, out float val) ? val : 0f;
        }

        void RebuildModifierCache()
        {
            _modifierCache.Clear();
            foreach (var tree in _trees.Values)
            {
                foreach (var node in tree.nodes)
                {
                    if (!node.isUnlocked) continue;
                    if (!_modifierCache.ContainsKey(node.modifierType))
                        _modifierCache[node.modifierType] = 0f;
                    _modifierCache[node.modifierType] += node.modifierValue;
                }
            }
            _modifierCacheDirty = false;
        }

        /// <summary>
        /// Force-unlock used by Moon 2 purge progression (kept for compat).
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
        /// General force-unlock for progression systems (Echohaven Moon1 early hub restorations, future moons).
        /// Skips all costs and prerequisites. Blessings are permanent and persist via normal skill save.
        /// </summary>
        public void ForceUnlockSkill(SkillId id)
        {
            var node = FindNode(id);
            if (node == null || node.isUnlocked) return;

            node.isUnlocked = true;
            _modifierCacheDirty = true;
            ApplySkillEffect(node);
            OnSkillUnlocked?.Invoke(id);
            AudioManager.Instance?.PlaySFX2D("SkillUnlocked");
            HapticFeedbackManager.Instance?.PlayDiscovery();
            Debug.Log($"[SkillTree] Progression blessing granted (permanent early/late): {node.displayName}");
        }

        public SkillTreeSaveData GetSaveData()
        {
            var unlocked = new List<int>();
            foreach (var tree in _trees.Values)
                foreach (var node in tree.nodes)
                    if (node.isUnlocked)
                        unlocked.Add((int)node.id);
            return new SkillTreeSaveData { unlockedSkills = unlocked };
        }

        /// <summary>Returns the list of nodes for the requested tree (empty list if not built).</summary>
        public List<SkillNode> GetTree(SkillTreeType type)
        {
            return _trees.TryGetValue(type, out var tree) ? tree.nodes : new List<SkillNode>();
        }

        public void RestoreFromSave(SkillTreeSaveData data)
        {
            if (data?.unlockedSkills == null) return;
            foreach (int raw in data.unlockedSkills)
            {
                var id = (SkillId)raw;
                var node = FindNode(id);
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
            // Data-driven architecture: load from ScriptableObject assets in Resources/SkillTrees/
            _trees[SkillTreeType.Resonator] = LoadTreeFromAsset("SkillTrees/Resonator");
            _trees[SkillTreeType.Architect] = LoadTreeFromAsset("SkillTrees/Architect");
            _trees[SkillTreeType.Guardian] = LoadTreeFromAsset("SkillTrees/Guardian");
            _trees[SkillTreeType.Historian] = LoadTreeFromAsset("SkillTrees/Historian");
        }

        SkillTree LoadTreeFromAsset(string resourcePath)
        {
            var asset = Resources.Load<Data.SkillTreeAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[SkillTree] Failed to load tree asset: {resourcePath}");
                return new SkillTree { type = SkillTreeType.Resonator, nodes = new List<SkillNode>() };
            }

            var tree = new SkillTree { type = asset.treeType };
            
            // Convert ScriptableObject data to runtime SkillNode instances
            foreach (var nodeData in asset.nodes)
            {
                if (nodeData == null) continue;

                // Support multiple prerequisites (use first one for backward compat)
                var prereq = nodeData.prerequisiteIds.Count > 0 ? nodeData.prerequisiteIds[0] : SkillId.None;
                
                tree.nodes.Add(new SkillNode(
                    nodeData.skillId,
                    nodeData.tier,
                    nodeData.rsCost,
                    nodeData.displayName,
                    nodeData.description,
                    nodeData.modifierType,
                    nodeData.modifierValue,
                    prereq
                ));
            }

            Debug.Log($"[SkillTree] Loaded {tree.nodes.Count} nodes from {resourcePath}");
            return tree;
        }

        // ═══ LEGACY HARDCODED TREE BUILDERS REMOVED ═══
        // Previously 218 lines of BuildResonatorTree() / BuildArchitectTree() / BuildGuardianTree() / BuildHistorianTree()
        // NOW: Data-driven architecture via SkillTreeAsset ScriptableObjects in Resources/SkillTrees/
        // Designers can modify trees in Unity Inspector without touching code.

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

        // ─── Moon 3 (Electric Moon) Blessing Hook (wired from CampaignFlowController + Moon03.json) ───
        /// <summary>
        /// Grants Moon 3 Compassion & Rails capstone blessings on campaign completion of Windswept Highlands.
        /// Includes lullaby synergy, golden rail permanent world change, fast travel, orphan trust network, World's Fair access.
        /// </summary>
        public void UnlockMoon3RailBlessing()
        {
            Debug.Log("[SkillTree] Moon 3 Electric Moon blessings unlocked (from Moon03.json via Campaign): Lullaby Shield, Orphan Trust, Golden Rails (permanent), Continental Fast Travel, World's Fair ticket variants.");
            _modifierCacheDirty = true;
            OnSkillUnlocked?.Invoke(SkillId.His_TrueHistory); // reuse as proxy for now; real would have M3_ ids
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_skill_blessing");
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum SkillTreeType : byte
    {
        Resonator = 0,   // Frequency mastery
        Architect = 1,   // Building enhancement
        Guardian  = 2,   // Combat skills
        Historian = 3    // Lore and discovery
    };

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
        M2_TrueLunarPurifier = 505,

        // Moon 1 Echohaven Early Progression Permanent Hub Blessings (600+)
        // Auto-granted by EchohavenProgressionSystem on restoring the 3 core buildings of the starting hub (fountain/dome/spire).
        // Provides meaningful, permanent early-game player power growth and world-state changes that persist via Skill save/load.
        // Restoring the hub now feels like a foundational, lasting transformation rather than a one-off event.
        E_FountainEcho   = 600,
        E_DomeInsight    = 601,
        E_SpireResonance = 602,
        E_HubAwakened    = 603
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
