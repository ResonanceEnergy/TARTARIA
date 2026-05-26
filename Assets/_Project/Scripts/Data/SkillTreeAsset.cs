using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core.Enums;

namespace Tartaria.Data
{
    /// <summary>
    /// ScriptableObject asset defining a complete skill tree.
    /// Place in Resources/SkillTrees/ for runtime loading.
    /// Designer-friendly — drag SkillNodeData assets to populate the tree.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillTree_", menuName = "Tartaria/Skill Tree", order = 101)]
    public class SkillTreeAsset : ScriptableObject
    {
        [Header("Tree Identity")]
        [Tooltip("Which archetype tree this represents")]
        public SkillTreeType treeType = SkillTreeType.Resonator;

        [Header("Skill Nodes")]
        [Tooltip("All nodes in this tree — drag SkillNodeData assets here")]
        public List<SkillNodeData> nodes = new();

        [Header("Editor Validation")]
        [SerializeField, TextArea(3, 8)]
        private string _validationReport = "Click Validate to check tree integrity";

        [ContextMenu("Validate Tree")]
        private void ValidateTree()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine($"=== {treeType} Tree Validation ===\n");

            // Check for null nodes
            int nullCount = nodes.FindAll(n => n == null).Count;
            if (nullCount > 0)
                report.AppendLine($"⚠ WARNING: {nullCount} null node slots detected!\n");

            // Check for duplicate skill IDs
            var seenIds = new HashSet<SkillId>();
            var duplicates = new List<SkillId>();
            foreach (var node in nodes)
            {
                if (node == null) continue;
                if (!seenIds.Add(node.skillId))
                    duplicates.Add(node.skillId);
            }
            if (duplicates.Count > 0)
                report.AppendLine($"⚠ DUPLICATE IDs: {string.Join(", ", duplicates)}\n");

            // Check prerequisite validity
            var prereqErrors = new List<string>();
            foreach (var node in nodes)
            {
                if (node == null) continue;
                foreach (var prereqId in node.prerequisiteIds)
                {
                    if (prereqId == SkillId.None) continue;
                    bool prereqExists = nodes.Exists(n => n != null && n.skillId == prereqId);
                    if (!prereqExists)
                        prereqErrors.Add($"  {node.skillId} → {prereqId} (missing!)");
                }
            }
            if (prereqErrors.Count > 0)
            {
                report.AppendLine($"⚠ INVALID PREREQUISITES:\n{string.Join("\n", prereqErrors)}\n");
            }

            // Summary
            int validNodes = nodes.FindAll(n => n != null).Count;
            report.AppendLine($"Total Nodes: {validNodes}");
            report.AppendLine($"Tier Distribution:");
            for (int tier = 1; tier <= 5; tier++)
            {
                int count = nodes.FindAll(n => n != null && n.tier == tier).Count;
                if (count > 0)
                    report.AppendLine($"  Tier {tier}: {count} nodes");
            }

            int blessings = nodes.FindAll(n => n != null && n.rsCost == 0f).Count;
            if (blessings > 0)
                report.AppendLine($"Progression Blessings: {blessings} (0 RS cost)");

            if (nullCount == 0 && duplicates.Count == 0 && prereqErrors.Count == 0)
                report.AppendLine("\n✓ VALIDATION PASSED");

            _validationReport = report.ToString();
            Debug.Log($"[SkillTreeAsset] {name} validation:\n{_validationReport}");
        }
    }
}
