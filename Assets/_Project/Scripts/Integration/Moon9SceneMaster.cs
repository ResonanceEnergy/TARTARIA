using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 Scene Master — The Blighted Wastes coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon9SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon9LevelBuilder levelBuilder;
        [SerializeField] Moon9MaterialSetup materialSetup;
        [SerializeField] Moon9PlayerSetup playerSetup;
        [SerializeField] Moon9LightingSetup lightingSetup;
        [SerializeField] Moon9AmbientAudio ambientAudio;
        [SerializeField] Moon9NPCSpawner npcSpawner;
        [SerializeField] Moon9EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon9AmbientParticles ambientParticles;
        [SerializeField] Moon9InteractiveObjects interactiveObjects;
        [SerializeField] Moon9AmbientCreatures ambientCreatures;
        [SerializeField] Moon9DynamicHazards dynamicHazards;
        [SerializeField] Moon9VisualLandmarks visualLandmarks;
        [SerializeField] Moon9AudioZones audioZones;
        [SerializeField] Moon9WeatherSystem weatherSystem;
        [SerializeField] Moon9QuestNodes questNodes;
        [SerializeField] Moon9Collectibles collectibles;
        [SerializeField] Moon9PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 9: THE BLIGHTED WASTES — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon9LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon9MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon9PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon9LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon9AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon9NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon9EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon9AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon9InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon9AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon9DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon9VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon9AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon9WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon9QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon9Collectibles>();
                if (postProcessing == null) postProcessing = GetComponent<Moon9PostProcessing>();
            }
        }
    }
}
