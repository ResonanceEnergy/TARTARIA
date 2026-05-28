using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Scene Master — The Abyssal Depths coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon7SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon7LevelBuilder levelBuilder;
        [SerializeField] Moon7PlayerSetup playerSetup;
        [SerializeField] Moon7LightingSetup lightingSetup;
        [SerializeField] Moon7AmbientAudio ambientAudio;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 7: THE ABYSSAL DEPTHS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon7LevelBuilder>();
                if (playerSetup == null) playerSetup = GetComponent<Moon7PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon7LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon7AmbientAudio>();
            }
        }
    }
}
