using UnityEngine;

namespace Tartaria.Integration
#pragma warning disable CS0414 // Placeholder counts for planned features
{
    /// <summary>
    /// Moon 2 Scene Master — Root coordinator for The Resonant Caverns
    /// Ensures proper initialization order and validates all systems
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon2SceneMaster : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] Moon2LevelBuilder levelBuilder;
        [SerializeField] Moon2LightingSetup lightingSetup;
        [SerializeField] Moon2AmbientAudio ambientAudio;
        [SerializeField] Moon2NPCSpawner npcSpawner;
        [SerializeField] Moon2QuestTriggers questTriggers;
        [SerializeField] Moon2PlayerSetup playerSetup;

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
            Debug.Log("  🌙 MOON 2: THE RESONANT CAVERNS — INITIALIZING");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Auto-find components if not assigned
            if (levelBuilder == null) levelBuilder = GetComponent<Moon2LevelBuilder>();
            if (lightingSetup == null) lightingSetup = GetComponent<Moon2LightingSetup>();
            if (ambientAudio == null) ambientAudio = GetComponent<Moon2AmbientAudio>();
            if (npcSpawner == null) npcSpawner = GetComponent<Moon2NPCSpawner>();
            if (questTriggers == null) questTriggers = GetComponent<Moon2QuestTriggers>();
            if (playerSetup == null) playerSetup = GetComponent<Moon2PlayerSetup>();

            Debug.Log("[Moon2SceneMaster] ✅ Scene initialization complete!");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        public void ValidateSystems()
        {
            Debug.Log("[Moon2SceneMaster] Validating all systems...");

            int systemCount = 0;
            int activeCount = 0;

            if (ValidateSystem("LevelBuilder", levelBuilder)) { systemCount++; activeCount++; }
            if (ValidateSystem("LightingSetup", lightingSetup)) { systemCount++; activeCount++; }
            if (ValidateSystem("AmbientAudio", ambientAudio)) { systemCount++; activeCount++; }
            if (ValidateSystem("NPCSpawner", npcSpawner)) { systemCount++; activeCount++; }
            if (ValidateSystem("QuestTriggers", questTriggers)) { systemCount++; activeCount++; }
            if (ValidateSystem("PlayerSetup", playerSetup)) { systemCount++; activeCount++; }

            Debug.Log($"[Moon2SceneMaster] ✅ Validation complete: {activeCount}/{systemCount} systems active");

            if (activeCount < systemCount)
            {
                Debug.LogWarning($"[Moon2SceneMaster] ⚠ {systemCount - activeCount} systems missing or inactive!");
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
            Debug.Log("[Moon2SceneMaster] Forcing scene reinitialization...");
            InitializeScene();
            ValidateSystems();
        }
    }
}
