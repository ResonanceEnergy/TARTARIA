using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Scene Master — The Verdant Labyrinth coordinator
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon3SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon3LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("🌙 MOON 3: THE VERDANT LABYRINTH — INITIALIZING");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon3LevelBuilder>();
            }
        }
    }
}
