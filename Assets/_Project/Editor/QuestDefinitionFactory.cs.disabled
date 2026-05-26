using UnityEditor;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Factory that generates QuestDefinition ScriptableObject assets on disk
    /// from the canonical runtime QuestDatabaseBuilder.
    /// Menu: Tartaria > Build Assets > Quest Definitions
    /// </summary>
    public static class QuestDefinitionFactory
    {
        const string BasePath = "Assets/_Project/Config/Quests";

        [MenuItem("Tartaria/Build Assets/Quest Definitions")]
        public static void BuildAllQuests()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Config"))
                AssetDatabase.CreateFolder("Assets/_Project", "Config");
            if (!AssetDatabase.IsValidFolder(BasePath))
                AssetDatabase.CreateFolder("Assets/_Project/Config", "Quests");

            var quests = QuestDatabaseBuilder.BuildAll();
            int created = 0;
            int updated = 0;

            foreach (var quest in quests)
            {
                string path = $"{BasePath}/Quest_{quest.questId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);

                if (existing != null)
                {
                    EditorUtility.SetDirty(existing);
                    existing.questId = quest.questId;
                    existing.displayName = quest.displayName;
                    existing.description = quest.description;
                    existing.isMainQuest = quest.isMainQuest;
                    existing.autoActivate = quest.autoActivate;
                    existing.rsRequirement = quest.rsRequirement;
                    existing.rsReward = quest.rsReward;
                    existing.objectives = quest.objectives;
                    existing.followUpQuestIds = quest.followUpQuestIds;
                    Object.DestroyImmediate(quest);
                    updated++;
                }
                else
                {
                    quest.name = quest.questId;
                    AssetDatabase.CreateAsset(quest, path);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[QuestDefinitionFactory] {quests.Length} quests processed ({created} created, {updated} updated).");
        }
    }
}
