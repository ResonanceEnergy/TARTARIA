using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Scene Master — The Verdant Labyrinth coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon3SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon3LevelBuilder levelBuilder;
        [SerializeField] Moon3MaterialSetup materialSetup;
        [SerializeField] Moon3PlayerSetup playerSetup;
        [SerializeField] Moon3LightingSetup lightingSetup;
        [SerializeField] Moon3AmbientAudio ambientAudio;
        [SerializeField] Moon3NPCSpawner npcSpawner;
        [SerializeField] Moon3EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon3AmbientParticles ambientParticles;
        [SerializeField] Moon3InteractiveObjects interactiveObjects;
        [SerializeField] Moon3AmbientCreatures ambientCreatures;
        [SerializeField] Moon3DynamicHazards dynamicHazards;
        [SerializeField] Moon3VisualLandmarks visualLandmarks;
        [SerializeField] Moon3AudioZones audioZones;
        [SerializeField] Moon3PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 3: THE VERDANT LABYRINTH — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon3LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon3MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon3PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon3LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon3AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon3NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon3EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon3AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon3InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon3AmbientCreatures>();
                if (dynamicHazards == null) dynamicHazards = GetComponent<Moon3DynamicHazards>();
                if (visualLandmarks == null) visualLandmarks = GetComponent<Moon3VisualLandmarks>();
                if (audioZones == null) audioZones = GetComponent<Moon3AudioZones>();
                if (postProcessing == null) postProcessing = GetComponent<Moon3PostProcessing>();
            }
        }
    }
}
