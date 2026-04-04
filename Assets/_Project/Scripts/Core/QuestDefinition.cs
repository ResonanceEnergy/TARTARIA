using UnityEngine;

namespace Tartaria.Core
{
    [CreateAssetMenu(menuName = "Tartaria/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string questId;
        public string displayName;
        [TextArea(2, 5)]
        public string description;

        [Header("Type")]
        public bool isMainQuest;
        public bool autoActivate;
        public float rsRequirement;

        [Header("Objectives")]
        public QuestObjective[] objectives;

        [Header("Rewards")]
        public float rsReward;

        [Header("Chain")]
        public string[] followUpQuestIds = System.Array.Empty<string>();
    }
}
