using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Scene Master — The Prismatic Nexus coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon11SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon11LevelBuilder levelBuilder;
        [SerializeField] Moon11MaterialSetup materialSetup;
        [SerializeField] Moon11PlayerSetup playerSetup;
        [SerializeField] Moon11LightingSetup lightingSetup;
        [SerializeField] Moon11AmbientAudio ambientAudio;
        [SerializeField] Moon11NPCSpawner npcSpawner;
        [SerializeField] Moon11EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon11AmbientParticles ambientParticles;
        [SerializeField] Moon11InteractiveObjects interactiveObjects;
        [SerializeField] Moon11AmbientCreatures ambientCreatures;
        [SerializeField] Moon11DynamicHazards dynamicHazards;
        [SerializeField] Moon11VisualLandmarks visualLandmarks;
        [SerializeField] Moon11AudioZones audioZones;
        [SerializeField] Moon11WeatherSystem weatherSystem;
        [SerializeField] Moon11QuestNodes questNodes;
        [SerializeField] Moon11Collectibles collectibles;
        [SerializeField] Moon11NPCDialogues npcDialogues;
        [SerializeField] Moon11PowerUps powerUps;
        [SerializeField] Moon11EnemySpawners enemySpawners;
        [SerializeField] Moon11Secrets secrets;
        [SerializeField] Moon11PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 11: THE PRISMATIC NEXUS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon11LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon11MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon11PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon11LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon11AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon11NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon11EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon11AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon11InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon11AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon11DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon11VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon11AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon11WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon11QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon11Collectibles>();
                if (npcDialogues == null) npcDialogues = GetComponent<Moon11NPCDialogues>();
                if (powerUps == null) powerUps = GetComponent<Moon11PowerUps>();
                if (enemySpawners == null) enemySpawners = GetComponent<Moon11EnemySpawners>();
                if (secrets == null) secrets = GetComponent<Moon11Secrets>();
                if (postProcessing == null) postProcessing = GetComponent<Moon11PostProcessing>();
            }
        }
    }
}
