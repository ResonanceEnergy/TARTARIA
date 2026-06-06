using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Unity Editor utility to automatically generate example SkillTreeAsset + SkillNodeData assets.
    /// Menu: Tools/Tartaria/Generate Example Skill Trees
    /// Creates data-driven skill tree assets from hardcoded definitions for quick migration.
    /// </summary>
    public class SkillTreeAssetGenerator
    {
        const string NodePath = "Assets/_Project/Resources/SkillNodes/";
        const string TreePath = "Assets/_Project/Resources/SkillTrees/";

        [MenuItem("Tools/Tartaria/Generate Example Skill Trees")]
        public static void GenerateExampleTrees()
        {
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/SkillNodes"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "SkillNodes");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/SkillTrees"))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "SkillTrees");

            Debug.Log("[SkillTreeGen] Generating Resonator example tree with 9 nodes...");
            GenerateResonatorTree();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SkillTreeGen] ✓ Generation complete! Check Resources/SkillTrees/");
        }

        static void GenerateResonatorTree()
        {
            var nodes = new List<Tartaria.Data.SkillNodeData>();

            // Tier 1
            nodes.Add(CreateNode("Res_FreqSense", Tartaria.Gameplay.SkillId.Res_FreqSense, 1, 50f,
                "Frequency Sense", "See Aether frequency values on buildings.",
                Tartaria.Gameplay.SkillModifierType.TuningPrecision, 0.1f,
                new List<Tartaria.Gameplay.SkillId>()));

            // Tier 2
            nodes.Add(CreateNode("Res_TuneSpeed", Tartaria.Gameplay.SkillId.Res_TuneSpeed, 2, 120f,
                "Rapid Tuning", "+20% tuning mini-game time limit.",
                Tartaria.Gameplay.SkillModifierType.TuningSpeed, 0.2f,
                new List<Tartaria.Gameplay.SkillId> { Tartaria.Gameplay.SkillId.Res_FreqSense }));

            nodes.Add(CreateNode("Res_AetherPool", Tartaria.Gameplay.SkillId.Res_AetherPool, 2, 150f,
                "Aether Reservoir", "+25% max Aether capacity.",
                Tartaria.Gameplay.SkillModifierType.AetherCapacity, 0.25f,
                new List<Tartaria.Gameplay.SkillId> { Tartaria.Gameplay.SkillId.Res_FreqSense }));

            // Tier 3
            nodes.Add(CreateNode("Res_Cascade", Tartaria.Gameplay.SkillId.Res_Cascade, 3, 250f,
                "Golden Cascade", "Golden Cascade combo extends to 15 hits.",
                Tartaria.Gameplay.SkillModifierType.ComboDuration, 0.25f,
                new List<Tartaria.Gameplay.SkillId> { Tartaria.Gameplay.SkillId.Res_TuneSpeed }));

            // Tier 4
            nodes.Add(CreateNode("Res_MasterFreq", Tartaria.Gameplay.SkillId.Res_MasterFreq, 4, 500f,
                "Master Frequency", "Tuning success rate +40%. Unlock harmonic chaining.",
                Tartaria.Gameplay.SkillModifierType.TuningPrecision, 0.4f,
                new List<Tartaria.Gameplay.SkillId> { Tartaria.Gameplay.SkillId.Res_Cascade }));

            // Moon 2 Purge Blessings (Tier 3-4, 0 RS cost)
            nodes.Add(CreateNode("M2_CathedralBreath", Tartaria.Gameplay.SkillId.M2_CathedralBreath, 3, 0f,
                "Cathedral's Eternal Breath", "Moon 2 Purge Blessing (cathedral_dome): The Grand Cathedral's living dome now breathes within you. +15% Resonance Score from all cavern restorations and purges. The corruption you burned away empowers your future.",
                Tartaria.Gameplay.SkillModifierType.LunarRSBonus, 0.15f,
                new List<Tartaria.Gameplay.SkillId>()));

            nodes.Add(CreateNode("M2_BellCleansing", Tartaria.Gameplay.SkillId.M2_BellCleansing, 3, 0f,
                "Bell of Cleansing Chime", "Moon 2 Purge Blessing (bell_tower): The Bell Tower's pure tone resonates in your staff. Perfect frequency matches now emit a cleansing chime that weakens nearby corruption nodes (especially powerful in micro-giant). Permanent echo of the purge.",
                Tartaria.Gameplay.SkillModifierType.PulseDamage, 0.12f,
                new List<Tartaria.Gameplay.SkillId>()));

            nodes.Add(CreateNode("M2_FountainSpring", Tartaria.Gameplay.SkillId.M2_FountainSpring, 4, 0f,
                "Aetheric Spring's Grace", "Moon 2 Purge Blessing (fountain): Living water from the fountain flows through your blood. -25% corruption spread rate globally + passive Aether regeneration near restored Moon 2 fountains. You have become a living counter-current to the Mud.",
                Tartaria.Gameplay.SkillModifierType.CorruptionResistance, 0.25f,
                new List<Tartaria.Gameplay.SkillId>()));

            // Moon 1 Echohaven Early Blessing
            nodes.Add(CreateNode("E_FountainEcho", Tartaria.Gameplay.SkillId.E_FountainEcho, 1, 0f,
                "Fountain's Harmonic Echo", "Echohaven Hub Blessing (fountain): The Harmonic Fountain's first restored song echoes forever in your core. +15% tuning precision on every mini-game from the start. Restoring the heart of the hub made frequency mastery second nature.",
                Tartaria.Gameplay.SkillModifierType.TuningPrecision, 0.15f,
                new List<Tartaria.Gameplay.SkillId>()));

            // Create tree asset
            var treeAsset = ScriptableObject.CreateInstance<Tartaria.Data.SkillTreeAsset>();
            treeAsset.treeType = Tartaria.Gameplay.SkillTreeType.Resonator;
            treeAsset.nodes = nodes;

            AssetDatabase.CreateAsset(treeAsset, TreePath + "Resonator.asset");
            Debug.Log($"[SkillTreeGen] Created Resonator.asset with {nodes.Count} nodes");
        }

        static Tartaria.Data.SkillNodeData CreateNode(
            string assetName,
            Tartaria.Gameplay.SkillId skillId,
            int tier,
            float rsCost,
            string displayName,
            string description,
            Tartaria.Gameplay.SkillModifierType modifierType,
            float modifierValue,
            List<Tartaria.Gameplay.SkillId> prerequisites)
        {
            var node = ScriptableObject.CreateInstance<Tartaria.Data.SkillNodeData>();
            node.skillId = skillId;
            node.tier = tier;
            node.rsCost = rsCost;
            node.displayName = displayName;
            node.description = description;
            node.modifierType = modifierType;
            node.modifierValue = modifierValue;
            node.prerequisiteIds = prerequisites;

            string path = NodePath + assetName + ".asset";
            AssetDatabase.CreateAsset(node, path);
            return AssetDatabase.LoadAssetAtPath<Tartaria.Data.SkillNodeData>(path);
        }
    }
}
