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
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 11: THE PRISMATIC NEXUS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon11LevelBuilder>();
            }
        }
    }
}
