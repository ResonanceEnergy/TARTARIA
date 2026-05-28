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
        [SerializeField] Moon9PlayerSetup playerSetup;
        [SerializeField] Moon9LightingSetup lightingSetup;
        [SerializeField] Moon9AmbientAudio ambientAudio;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 9: THE BLIGHTED WASTES — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon9LevelBuilder>();
                if (playerSetup == null) playerSetup = GetComponent<Moon9PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon9LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon9AmbientAudio>();
            }
        }
    }
}
