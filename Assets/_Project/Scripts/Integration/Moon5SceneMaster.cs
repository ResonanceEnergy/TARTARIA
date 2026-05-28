using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Scene Master — The Frostbound Citadel coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon5SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon5LevelBuilder levelBuilder;
        [SerializeField] Moon5PlayerSetup playerSetup;
        [SerializeField] Moon5LightingSetup lightingSetup;
        [SerializeField] Moon5AmbientAudio ambientAudio;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 5: THE FROSTBOUND CITADEL — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon5LevelBuilder>();
                if (playerSetup == null) playerSetup = GetComponent<Moon5PlayerSetup>();
                if (lightingSetup == null) lightingSetup = GetComponent<Moon5LightingSetup>();
                if (ambientAudio == null) ambientAudio = GetComponent<Moon5AmbientAudio>();
            }
        }
    }
}
