using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Scene Master — The Umbral Sanctum coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon12SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon12LevelBuilder levelBuilder;
        [SerializeField] Moon12MaterialSetup materialSetup;
        [SerializeField] Moon12PlayerSetup playerSetup;
        [SerializeField] Moon12LightingSetup lightingSetup;
        [SerializeField] Moon12AmbientAudio ambientAudio;
        [SerializeField] Moon12NPCSpawner npcSpawner;
        [SerializeField] Moon12EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon12AmbientParticles ambientParticles;
        [SerializeField] Moon12InteractiveObjects interactiveObjects;
        [SerializeField] Moon12AmbientCreatures ambientCreatures;
        [SerializeField] Moon12DynamicHazards dynamicHazards;
        [SerializeField] Moon12VisualLandmarks visualLandmarks;
        [SerializeField] Moon12AudioZones audioZones;
        [SerializeField] Moon12WeatherSystem weatherSystem;
        [SerializeField] Moon12PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 12: THE UMBRAL SANCTUM — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon12LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon12MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon12PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon12LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon12AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon12NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon12EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon12AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon12InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon12AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon12DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon12VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon12AudioZones>();
                if (weatherSystem == null) weatherSystem = GetComponent<Moon12WeatherSystem>();
                if (postProcessing == null) postProcessing = GetComponent<Moon12PostProcessing>();
            }
        }
    }
}
