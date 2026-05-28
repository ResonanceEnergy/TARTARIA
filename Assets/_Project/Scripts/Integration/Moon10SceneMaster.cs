using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Scene Master — The Temporal Rift coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon10SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon10LevelBuilder levelBuilder;
        [SerializeField] Moon10MaterialSetup materialSetup;
        [SerializeField] Moon10PlayerSetup playerSetup;
        [SerializeField] Moon10LightingSetup lightingSetup;
        [SerializeField] Moon10AmbientAudio ambientAudio;
        [SerializeField] Moon10NPCSpawner npcSpawner;
        [SerializeField] Moon10EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon10AmbientParticles ambientParticles;
        [SerializeField] Moon10InteractiveObjects interactiveObjects;
        [SerializeField] Moon10AmbientCreatures ambientCreatures;
        [SerializeField] Moon10DynamicHazards dynamicHazards;
        [SerializeField] Moon10VisualLandmarks visualLandmarks;
        [SerializeField] Moon10AudioZones audioZones;
        [SerializeField] Moon10WeatherSystem weatherSystem;
        [SerializeField] Moon10QuestNodes questNodes;
        [SerializeField] Moon10Collectibles collectibles;
        [SerializeField] Moon10PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 10: THE TEMPORAL RIFT — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon10LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon10MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon10PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon10LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon10AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon10NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon10EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon10AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon10InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon10AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon10DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon10VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon10AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon10WeatherSystem>();
                if (questNodes == null) questNodes = GetComponent<Moon10QuestNodes>();
                if (collectibles == null) collectibles = GetComponent<Moon10Collectibles>();
                if (postProcessing == null) postProcessing = GetComponent<Moon10PostProcessing>();
            }
        }
    }
}
