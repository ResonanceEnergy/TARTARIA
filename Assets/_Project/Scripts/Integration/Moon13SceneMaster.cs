using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Scene Master — The Aether Convergence coordinator
    /// FINAL LEVEL - Culmination of all 12 moons
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon13SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon13LevelBuilder levelBuilder;
        [SerializeField] Moon13MaterialSetup materialSetup;
        [SerializeField] Moon13PlayerSetup playerSetup;
        [SerializeField] Moon13LightingSetup lightingSetup;
        [SerializeField] Moon13AmbientAudio ambientAudio;
        [SerializeField] Moon13NPCSpawner npcSpawner;
        [SerializeField] Moon13EnvironmentDecorator environmentDecorator;
        [SerializeField] Moon13AmbientParticles ambientParticles;
        [SerializeField] Moon13InteractiveObjects interactiveObjects;
        [SerializeField] Moon13AmbientCreatures ambientCreatures;
        [SerializeField] Moon13PostProcessing postProcessing;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("═══════════════════════════════════════════════════════════════");
                Debug.Log("  🌙 MOON 13: THE AETHER CONVERGENCE — INITIALIZING");
                Debug.Log("    ✨ FINAL LEVEL - THE CULMINATION OF THE 13 MOONS ✨");
                Debug.Log("═══════════════════════════════════════════════════════════════");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon13LevelBuilder>();
                if (materialSetup == null) materialSetup = GetComponent<Moon13MaterialSetup>();
                if (playerSetup == null) playerSetup = GetComponent<Moon13PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon13LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon13AmbientAudio>();
                if (npcSpawner == null) npcSpawner = GetComponent<Moon13NPCSpawner>();
                if (environmentDecorator == null) environmentDecorator = GetComponent<Moon13EnvironmentDecorator>();
                if (ambientParticles == null) ambientParticles = GetComponent<Moon13AmbientParticles>();
                if (interactiveObjects == null) interactiveObjects = GetComponent<Moon13InteractiveObjects>();
                if (ambientCreatures == null) ambientCreatures = GetComponent<Moon13AmbientCreatures>();
                if (postProcessing == null) postProcessing = GetComponent<Moon13PostProcessing>();
            }
        }
    }
}
