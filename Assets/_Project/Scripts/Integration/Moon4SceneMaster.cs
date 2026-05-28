using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Scene Master — The Sunscorched Oasis coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon4SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon4LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 4: THE SUNSCORCHED OASIS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon4LevelBuilder>();
            }
        }
    }
}
