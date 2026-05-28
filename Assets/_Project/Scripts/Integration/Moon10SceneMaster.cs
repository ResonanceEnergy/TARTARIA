using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Scene Master — The Temporal Rift coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon10SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon10LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 10: THE TEMPORAL RIFT — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon10LevelBuilder>();
            }
        }
    }
}
