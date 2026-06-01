using UnityEngine;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 1 Scene Master — Root coordinator for all Echohaven systems
    /// Ensures proper initialization order and validates all systems are active
    /// One component to rule them all
    /// </summary>
    [DefaultExecutionOrder(-100)] // First to run
    public class Moon1SceneMaster : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] Moon1HeroBuildingSpawner heroBuildings;
        [SerializeField] Moon1LevelBuilder levelBuilder;
        [SerializeField] Moon1EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon1PathGenerator pathGenerator;
        [SerializeField] Moon1NPCSpawner npcSpawner;
        [SerializeField] Moon1QuestTriggers questTriggers;
        [SerializeField] Moon1AmbientAudio ambientAudio;
        [SerializeField] Moon1ExcavationSites excavationSites;
        [SerializeField] Moon1LightingSetup lightingSetup;
        [SerializeField] Moon1PlayerSetup playerSetup;
        [SerializeField] Moon1PostProcessing postProcessing;

        [Header("Scene State")]
        [SerializeField] bool autoInitialize = true;
        [SerializeField] bool validateOnStart = true;

        void Awake()
        {
            if (autoInitialize)
            {
                InitializeScene();
            }
        }

        void Start()
        {
            if (validateOnStart)
            {
                ValidateSystems();
            }
        }

        public void InitializeScene()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 1: ECHOHAVEN — INITIALIZING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Auto-find components if not assigned
            if (heroBuildings == null) heroBuildings = GetComponent<Moon1HeroBuildingSpawner>();
            if (levelBuilder == null) levelBuilder = GetComponent<Moon1LevelBuilder>();
            if (environmentDecorator == null) environmentDecorator = GetComponent<Moon1EnvironmentDecorator>();
            if (pathGenerator == null) pathGenerator = GetComponent<Moon1PathGenerator>();
            if (npcSpawner == null) npcSpawner = GetComponent<Moon1NPCSpawner>();
            if (questTriggers == null) questTriggers = GetComponent<Moon1QuestTriggers>();
            if (ambientAudio == null) ambientAudio = GetComponent<Moon1AmbientAudio>();
            if (excavationSites == null) excavationSites = GetComponent<Moon1ExcavationSites>();
            if (lightingSetup == null) lightingSetup = GetComponent<Moon1LightingSetup>();
            if (playerSetup == null) playerSetup = GetComponent<Moon1PlayerSetup>();
            if (postProcessing == null) postProcessing = GetComponent<Moon1PostProcessing>();

            Debug.Log("[Moon1SceneMaster] ✅ Scene initialization complete!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        public void ValidateSystems()
        {
            Debug.Log("[Moon1SceneMaster] Validating all systems...");

            int systemCount = 0;
            int activeCount = 0;

            if (ValidateSystem("HeroBuildingSpawner", heroBuildings)) { systemCount++; activeCount++; }
            if (ValidateSystem("LevelBuilder", levelBuilder)) { systemCount++; activeCount++; }
            if (ValidateSystem("EnvironmentDecorator", environmentDecorator)) { systemCount++; activeCount++; }
            if (ValidateSystem("PathGenerator", pathGenerator)) { systemCount++; activeCount++; }
            if (ValidateSystem("NPCSpawner", npcSpawner)) { systemCount++; activeCount++; }
            if (ValidateSystem("QuestTriggers", questTriggers)) { systemCount++; activeCount++; }
            if (ValidateSystem("AmbientAudio", ambientAudio)) { systemCount++; activeCount++; }
            if (ValidateSystem("ExcavationSites", excavationSites)) { systemCount++; activeCount++; }
            if (ValidateSystem("LightingSetup", lightingSetup)) { systemCount++; activeCount++; }
            if (ValidateSystem("PlayerSetup", playerSetup)) { systemCount++; activeCount++; }
            if (ValidateSystem("PostProcessing", postProcessing)) { systemCount++; activeCount++; }

            Debug.Log($"[Moon1SceneMaster] ✅ Validation complete: {activeCount}/{systemCount} systems active");

            if (activeCount < systemCount)
            {
                Debug.LogWarning($"[Moon1SceneMaster] ⚠ {systemCount - activeCount} systems missing or inactive!");
            }
        }

        bool ValidateSystem(string name, MonoBehaviour system)
        {
            if (system == null)
            {
                Debug.LogWarning($"  ✗ {name}: NOT FOUND");
                return false;
            }

            if (!system.enabled)
            {
                Debug.LogWarning($"  ⚠ {name}: DISABLED");
                return false;
            }

            Debug.Log($"  ✓ {name}: ACTIVE");
            return true;
        }

        [ContextMenu("Force Reinitialize Scene")]
        public void ForceReinitialize()
        {
            Debug.Log("[Moon1SceneMaster] Forcing scene reinitialization...");
            InitializeScene();
            ValidateSystems();
        }
    }
}
