using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Data.Query
{
    /// <summary>
    /// Initializes all data registries at game startup.
    /// Builds indexes for O(1) data queries (<100ms startup time).
    /// Attach to GameManager or create a dedicated GameObject in the first scene.
    /// </summary>
    public class DataRegistryInitializer : MonoBehaviour
    {
        [Header("Database Assets (Resources Path)")]
        [SerializeField] string itemDatabasePath = "ItemDatabase";
        [SerializeField] string questDatabasePath = "QuestDatabase";
        [SerializeField] string craftingDatabasePath = "CraftingRecipeDatabase";
        [SerializeField] string skillTreePath = "SkillTrees/MainSkillTree";

        [Header("Initialization")]
        [SerializeField] bool initializeOnAwake = true;
        [SerializeField] bool logPerformance = true;

        void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeAllRegistries();
            }
        }

        /// <summary>
        /// Initializes all data registries.
        /// Call this once at game startup before any data queries.
        /// </summary>
        public void InitializeAllRegistries()
        {
            var totalTime = Time.realtimeSinceStartup;

            Debug.Log("[DataRegistry] Initializing all data registries...");

            // Initialize ItemRegistry
            InitializeItemRegistry();

            // Initialize QuestRegistry
            InitializeQuestRegistry();

            // Initialize CraftingRecipeRegistry
            InitializeCraftingRegistry();

            // Initialize SkillRegistry
            InitializeSkillRegistry();

            var elapsed = (Time.realtimeSinceStartup - totalTime) * 1000f;
            
            if (logPerformance)
            {
                Debug.Log($"[DataRegistry] All registries initialized in {elapsed:F2}ms");
                LogRegistryStats();
            }

            // Verify startup time constraint
            if (elapsed > 100f)
            {
                Debug.LogWarning($"[DataRegistry] Initialization took {elapsed:F2}ms (target: <100ms). Consider optimizing database loading.");
            }
        }

        void InitializeItemRegistry()
        {
            var startTime = Time.realtimeSinceStartup;
            
            var itemDatabase = Resources.Load<ItemDatabase>(itemDatabasePath);
            if (itemDatabase == null)
            {
                Debug.LogError($"[DataRegistry] Failed to load ItemDatabase from Resources/{itemDatabasePath}");
                return;
            }

            ItemRegistry.Initialize(itemDatabase);

            if (logPerformance)
            {
                var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"[DataRegistry] ItemRegistry initialized in {elapsed:F2}ms ({ItemRegistry.Count} items)");
            }
        }

        void InitializeQuestRegistry()
        {
            var startTime = Time.realtimeSinceStartup;
            
            var questDatabase = Resources.Load<QuestDatabase>(questDatabasePath);
            if (questDatabase == null)
            {
                Debug.LogError($"[DataRegistry] Failed to load QuestDatabase from Resources/{questDatabasePath}");
                return;
            }

            QuestRegistry.Initialize(questDatabase);

            if (logPerformance)
            {
                var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"[DataRegistry] QuestRegistry initialized in {elapsed:F2}ms ({QuestRegistry.Count} quests)");
            }
        }

        void InitializeCraftingRegistry()
        {
            var startTime = Time.realtimeSinceStartup;
            
            var craftingDatabase = Resources.Load<CraftingRecipeDatabase>(craftingDatabasePath);
            if (craftingDatabase == null)
            {
                Debug.LogWarning($"[DataRegistry] Failed to load CraftingRecipeDatabase from Resources/{craftingDatabasePath} (optional)");
                return;
            }

            CraftingRecipeRegistry.Initialize(craftingDatabase);

            if (logPerformance)
            {
                var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"[DataRegistry] CraftingRecipeRegistry initialized in {elapsed:F2}ms ({CraftingRecipeRegistry.Count} recipes)");
            }
        }

        void InitializeSkillRegistry()
        {
            var startTime = Time.realtimeSinceStartup;
            
            var skillTree = Resources.Load<SkillTreeAsset>(skillTreePath);
            if (skillTree == null)
            {
                Debug.LogWarning($"[DataRegistry] Failed to load SkillTreeAsset from Resources/{skillTreePath} (optional)");
                return;
            }

            SkillRegistry.Initialize(skillTree);

            if (logPerformance)
            {
                var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log($"[DataRegistry] SkillRegistry initialized in {elapsed:F2}ms ({SkillRegistry.Count} skills)");
            }
        }

        void LogRegistryStats()
        {
            Debug.Log($"[DataRegistry] Registry Statistics:\n" +
                     $"  Items: {ItemRegistry.Count}\n" +
                     $"  Quests: {QuestRegistry.Count}\n" +
                     $"  Crafting Recipes: {CraftingRecipeRegistry.Count}\n" +
                     $"  Skills: {SkillRegistry.Count}");
        }

        /// <summary>
        /// Clears all registries (for hot-reload/testing).
        /// </summary>
        [ContextMenu("Clear All Registries")]
        public void ClearAllRegistries()
        {
            ItemRegistry.Clear();
            QuestRegistry.Clear();
            CraftingRecipeRegistry.Clear();
            SkillRegistry.Clear();
            
            Debug.Log("[DataRegistry] All registries cleared");
        }

        /// <summary>
        /// Rebuilds all registries (for hot-reload after data changes).
        /// </summary>
        [ContextMenu("Rebuild All Registries")]
        public void RebuildAllRegistries()
        {
            ClearAllRegistries();
            InitializeAllRegistries();
        }
    }
}
