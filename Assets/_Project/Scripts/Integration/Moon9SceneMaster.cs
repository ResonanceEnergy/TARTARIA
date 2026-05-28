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
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 9: THE BLIGHTED WASTES — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon9LevelBuilder>();
            }
        }
    }
}
