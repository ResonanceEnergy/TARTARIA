using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Scene Master — The Abyssal Depths coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon7SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon7LevelBuilder levelBuilder;
        [SerializeField] Moon7MaterialSetup materialSetup;
        [SerializeField] Moon7PlayerSetup playerSetup;
        [SerializeField] Moon7LightingSetup lightingSetup;
        [SerializeField] Moon7AmbientAudio ambientAudio;
        [SerializeField] Moon7NPCSpawner npcSpawner;
        [SerializeField] Moon7EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon7AmbientParticles ambientParticles;
        [SerializeField] Moon7InteractiveObjects interactiveObjects;
        [SerializeField] Moon7AmbientCreatures ambientCreatures;
        [SerializeField] Moon7DynamicHazards dynamicHazards;
        [SerializeField] Moon7VisualLandmarks visualLandmarks;
        [SerializeField] Moon7AudioZones audioZones;
        [SerializeField] Moon7WeatherSystem weatherSystem;
        [SerializeField] Moon7QuestNodes questNodes;
        [SerializeField] Moon7Collectibles collectibles;
        [SerializeField] Moon7NPCDialogues npcDialogues;
        [SerializeField] Moon7PowerUps powerUps;
        [SerializeField] Moon7EnemySpawners enemySpawners;
        [SerializeField] Moon7Secrets secrets;
        [SerializeField] Moon7PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 7: THE ABYSSAL DEPTHS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon7LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon7MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon7PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon7LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon7AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon7NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon7EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon7AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon7InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon7AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon7DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon7VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon7AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon7WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon7QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon7Collectibles>();
                if (npcDialogues == null) npcDialogues = GetComponent<Moon7NPCDialogues>();
                if (powerUps == null) powerUps = GetComponent<Moon7PowerUps>();
                if (enemySpawners == null) enemySpawners = GetComponent<Moon7EnemySpawners>();
                if (secrets == null) secrets = GetComponent<Moon7Secrets>();
                if (postProcessing == null) postProcessing = GetComponent<Moon7PostProcessing>();
            }
        }
    }
}
