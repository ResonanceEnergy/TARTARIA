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
        [SerializeField] Moon8PlayerSetup playerSetup;
        [SerializeField] Moon8LightingSetup lightingSetup;
        [SerializeField] Moon8AmbientAudio ambientAudio;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 8: THE CELESTIAL SPIRES — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon8LevelBuilder>();
                if (playerSetup == null) playerSetup = GetComponent<Moon8PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon8LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon8AmbientAudio>();
            }
        }
    }
}
