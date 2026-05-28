using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Scene Master — The Frostbound Citadel coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon5SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon5LevelBuilder levelBuilder;
        [SerializeField] Moon5MaterialSetup materialSetup;
        [SerializeField] Moon5PlayerSetup playerSetup;
        [SerializeField] Moon5LightingSetup lightingSetup;
        [SerializeField] Moon5AmbientAudio ambientAudio;
        [SerializeField] Moon5NPCSpawner npcSpawner;
        [SerializeField] Moon5EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon5AmbientParticles ambientParticles;
        [SerializeField] Moon5InteractiveObjects interactiveObjects;
        [SerializeField] Moon5AmbientCreatures ambientCreatures;
        [SerializeField] Moon5DynamicHazards dynamicHazards;
        [SerializeField] Moon5VisualLandmarks visualLandmarks;
        [SerializeField] Moon5AudioZones audioZones;
        [SerializeField] Moon5WeatherSystem weatherSystem;
        [SerializeField] Moon5QuestNodes questNodes;
        [SerializeField] Moon5Collectibles collectibles;
        [SerializeField] Moon5PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 5: THE FROSTBOUND CITADEL — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon5LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon5MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon5PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon5LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon5AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon5NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon5EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon5AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon5InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon5AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon5DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon5VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon5AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon5WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon5QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon5Collectibles>();
                if (postProcessing == null) postProcessing = GetComponent<Moon5PostProcessing>();
            }
        }
    }
}
