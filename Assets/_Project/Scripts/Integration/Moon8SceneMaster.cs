using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Scene Master — The Celestial Spires coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon8SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon8LevelBuilder levelBuilder;
        [SerializeField] Moon8MaterialSetup materialSetup;
        [SerializeField] Moon8PlayerSetup playerSetup;
        [SerializeField] Moon8LightingSetup lightingSetup;
        [SerializeField] Moon8AmbientAudio ambientAudio;
        [SerializeField] Moon8NPCSpawner npcSpawner;
        [SerializeField] Moon8EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon8AmbientParticles ambientParticles;
        [SerializeField] Moon8InteractiveObjects interactiveObjects;
        [SerializeField] Moon8AmbientCreatures ambientCreatures;
        [SerializeField] Moon8DynamicHazards dynamicHazards;
        [SerializeField] Moon8VisualLandmarks visualLandmarks;
        [SerializeField] Moon8AudioZones audioZones;
        [SerializeField] Moon8PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 8: THE CELESTIAL SPIRES — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon8LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon8MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon8PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon8LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon8AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon8NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon8EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon8AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon8InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon8AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon8DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon8VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon8AudioZones>();
                if (postProcessing == null) postProcessing = GetComponent<Moon8PostProcessing>();
            }
        }
    }
}
