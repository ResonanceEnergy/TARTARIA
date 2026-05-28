using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Scene Master — The Umbral Sanctum coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon12SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon12LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 12: THE UMBRAL SANCTUM — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon12LevelBuilder>();
            }
        }
    }
}
