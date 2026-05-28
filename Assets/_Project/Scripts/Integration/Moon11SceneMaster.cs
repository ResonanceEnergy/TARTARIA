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
                if (postProcessing == null) postProcessing = GetComponent<Moon11PostProcessing>();
            }
        }
    }
}
