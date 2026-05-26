using UnityEngine;
using Tartaria.Data;
using Tartaria.Core;

namespace Tartaria.Save
{
    /// <summary>
    /// Quest Data Migrator: V1 → V2 (Future Example)
    /// 
    /// EXAMPLE migration for future objective system refactor.
    /// NOT ACTIVE YET (CURRENT_QUEST = V1).
    /// 
    /// Hypothetical changes:
    ///   - Changed: objectives[] → objectiveData[] (more structured)
    ///   - Added: questTags[] for filtering
    ///   - Added: estimatedDurationMinutes
    ///   - Renamed: itemRewards → rewardItems (consistency)
    /// 
    /// When this goes live:
    ///   1. Update SchemaVersion.CURRENT_QUEST = 2
    ///   2. Add [SerializeField] int schemaVersion = 1 to QuestData
    ///   3. Register this migrator in MigrationPipeline
    ///   4. Run batch migration tool on all quest assets
    /// </summary>
    public class QuestDataMigrator_V1_to_V2 : IDataMigrator<QuestData, QuestData>
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public QuestData Migrate(QuestData input)
        {
            if (input == null)
            {
                Debug.LogError("[QuestDataMigrator_V1_to_V2] Input is null!");
                return null;
            }

            // Clone via ScriptableObject.Instantiate
            var output = Object.Instantiate(input);

            // V2 CHANGES (example):
            // MigrateObjectives(input, output);
            // output.questTags = InferQuestTags(input);
            // output.estimatedDurationMinutes = EstimateDuration(input);

            Debug.Log($"[QuestDataMigrator_V1_to_V2] Migrated quest: {input.questId}");
            return output;
        }

        public bool Validate(QuestData input)
        {
            if (input == null) return false;
            if (string.IsNullOrEmpty(input.questId))
            {
                Debug.LogWarning("[QuestDataMigrator_V1_to_V2] Quest has empty questId!");
                return false;
            }
            return true;
        }

        public string GetChangeDescription()
        {
            return "V1→V2: Refactored objectives to ObjectiveData, added quest tags (FUTURE)";
        }

        // Example: convert old objectives array to new ObjectiveData[]
        void MigrateObjectives(QuestData input, QuestData output)
        {
            var baseObjectives = input.GetRuntimeObjectives();
            if (baseObjectives == null || baseObjectives.Length == 0)
            {
                // output.objectiveData = System.Array.Empty<ObjectiveData>();
                return;
            }

            // Convert each QuestObjective to ObjectiveData
            // var newObjectives = new ObjectiveData[baseObjectives.Length];
            // for (int i = 0; i < baseObjectives.Length; i++)
            // {
            //     newObjectives[i] = ObjectiveData.FromQuestObjective(baseObjectives[i]);
            // }
            // output.objectiveData = newObjectives;
        }

        // Example: infer tags from quest properties
        string[] InferQuestTags(QuestData input)
        {
            var tags = new System.Collections.Generic.List<string>();

            if (input.category == QuestCategory.Main) tags.Add("main");
            if (input.category == QuestCategory.Side) tags.Add("side");
            if (input.xpReward > 1000) tags.Add("high_reward");
            if (input.moonId > 0) tags.Add($"moon{input.moonId}");
            if (input.isRepeatable) tags.Add("repeatable");

            return tags.ToArray();
        }

        // Example: estimate duration from objectives
        int EstimateDuration(QuestData input)
        {
            var objectives = input.GetRuntimeObjectives();
            if (objectives == null) return 15; // Default 15 min

            // Simple heuristic: 5 min per objective, +10 min for travel
            return (objectives.Length * 5) + 10;
        }
    }
}
