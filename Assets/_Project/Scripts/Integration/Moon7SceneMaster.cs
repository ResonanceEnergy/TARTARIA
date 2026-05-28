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
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 7: THE ABYSSAL DEPTHS — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon7LevelBuilder>();
            }
        }
    }
}
