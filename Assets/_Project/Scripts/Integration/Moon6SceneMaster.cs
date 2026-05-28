using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Scene Master — The Molten Forge coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon6SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon6LevelBuilder levelBuilder;
        [SerializeField] Moon6MaterialSetup materialSetup;
        [SerializeField] Moon6PlayerSetup playerSetup;
        [SerializeField] Moon6LightingSetup lightingSetup;
        [SerializeField] Moon6AmbientAudio ambientAudio;
        [SerializeField] Moon6NPCSpawner npcSpawner;
        [SerializeField] Moon6EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon6AmbientParticles ambientParticles;
        [SerializeField] Moon6InteractiveObjects interactiveObjects;
        [SerializeField] Moon6AmbientCreatures ambientCreatures;
        [SerializeField] Moon6DynamicHazards dynamicHazards;
        [SerializeField] Moon6VisualLandmarks visualLandmarks;
        [SerializeField] Moon6AudioZones audioZones;
        [SerializeField] Moon6WeatherSystem weatherSystem;
        [SerializeField] Moon6QuestNodes questNodes;
        [SerializeField] Moon6Collectibles collectibles;
        [SerializeField] Moon6PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 6: THE MOLTEN FORGE — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon6LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon6MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon6PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon6LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon6AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon6NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon6EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon6AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon6InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon6AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon6DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon6VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon6AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon6WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon6QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon6Collectibles>();
                if (postProcessing == null) postProcessing = GetComponent<Moon6PostProcessing>();
            }
        }
    }
}
