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
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 5: THE FROSTBOUND CITADEL — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon5LevelBuilder>();
            }
        }
    }
}
