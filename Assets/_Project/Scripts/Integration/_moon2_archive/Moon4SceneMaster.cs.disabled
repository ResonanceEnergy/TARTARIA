using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Scene Master — The Sunscorched Oasis coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon4SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon4LevelBuilder levelBuilder;
        [SerializeField] Moon4MaterialSetup materialSetup;
        [SerializeField] Moon4PlayerSetup playerSetup;
        [SerializeField] Moon4LightingSetup lightingSetup;
        [SerializeField] Moon4AmbientAudio ambientAudio;
        [SerializeField] Moon4NPCSpawner npcSpawner;
        [SerializeField] Moon4EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon4AmbientParticles ambientParticles;
        [SerializeField] Moon4InteractiveObjects interactiveObjects;
        [SerializeField] Moon4AmbientCreatures ambientCreatures;
        [SerializeField] Moon4DynamicHazards dynamicHazards;
        [SerializeField] Moon4VisualLandmarks visualLandmarks;
        [SerializeField] Moon4AudioZones audioZones;
        [SerializeField] Moon4WeatherSystem weatherSystem;
        [SerializeField] Moon4QuestNodes questNodes;
        [SerializeField] Moon4Collectibles collectibles;
        [SerializeField] Moon4NPCDialogues npcDialogues;
        [SerializeField] Moon4PowerUps powerUps;
        [SerializeField] Moon4EnemySpawners enemySpawners;
        [SerializeField] Moon4Secrets secrets;
        [SerializeField] Moon4PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 4: THE SUNSCORCHED OASIS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon4LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon4MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon4PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon4LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon4AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon4NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon4EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon4AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon4InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon4AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon4DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon4VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon4AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon4WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon4QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon4Collectibles>();
                if (npcDialogues == null) npcDialogues = GetComponent<Moon4NPCDialogues>();
                if (powerUps == null) powerUps = GetComponent<Moon4PowerUps>();
                if (enemySpawners == null) enemySpawners = GetComponent<Moon4EnemySpawners>();
                if (secrets == null) secrets = GetComponent<Moon4Secrets>();
                if (postProcessing == null) postProcessing = GetComponent<Moon4PostProcessing>();
            }
        }
    }
}
