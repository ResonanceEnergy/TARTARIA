using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Scene Master — The Molten Forge coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon6SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon6LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 6: THE MOLTEN FORGE — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon6LevelBuilder>();
            }
        }
    }
}
