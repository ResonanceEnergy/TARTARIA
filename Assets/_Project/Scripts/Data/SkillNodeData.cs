using UnityEngine;
using System;
using System.Collections.Generic;
using Tartaria.Gameplay;

namespace Tartaria.Data
{
    /// <summary>
    /// ScriptableObject representing a single skill node in a skill tree.
    /// Designer-editable in Unity Inspector — no code changes needed for balance tweaks.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNode_", menuName = "Tartaria/Skill Node", order = 100)]
    public class SkillNodeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique skill identifier (must match SkillId enum)")]
        public SkillId skillId = SkillId.None;

        [Tooltip("Tier/level within tree (1-5 typically)")]
        [Range(1, 5)]
        public int tier = 1;

        [Header("Costs & Requirements")]
        [Tooltip("Resonance Score cost to unlock (0 = progression blessing)")]
        public float rsCost = 50f;

        [Tooltip("Skills that must be unlocked first")]
        public List<SkillId> prerequisiteIds = new();

        [Header("Display")]
        [Tooltip("Name shown in UI")]
        public string displayName = "New Skill";

        [Tooltip("Description shown in tooltip")]
        [TextArea(3, 6)]
        public string description = "Skill description here.";

        [Header("Mechanics")]
        [Tooltip("What type of modifier this skill grants")]
        public SkillModifierType modifierType = SkillModifierType.TuningPrecision;

        [Tooltip("Numeric value of the modifier (0.1 = +10%, etc)")]
        public float modifierValue = 0.1f;

        [Header("Editor Tools")]
        [Tooltip("Quick preview of full skill identity")]
        [SerializeField, TextArea(2, 4)]
        private string _editorPreview = "";

        private void OnValidate()
        {
            // Auto-generate preview for designer convenience
            _editorPreview = $"[{skillId}] Tier {tier} | {rsCost} RS\n" +
                             $"{displayName}: +{modifierValue:P0} {modifierType}\n" +
                             $"Prereqs: {prerequisiteIds.Count}";
        }
    }
}
