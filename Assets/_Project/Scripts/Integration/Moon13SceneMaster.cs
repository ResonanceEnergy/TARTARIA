using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Scene Master — The Aether Convergence coordinator
    /// FINAL LEVEL - Culmination of all 12 moons
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Moon13SceneMaster : MonoBehaviour
    {
        [SerializeField] Moon13LevelBuilder levelBuilder;
        [SerializeField] bool autoInitialize = true;

        void Awake()
        {
            if (autoInitialize)
            {
                Debug.Log("═══════════════════════════════════════════════════════════════");
                Debug.Log("  🌙 MOON 13: THE AETHER CONVERGENCE — INITIALIZING");
                Debug.Log("    ✨ FINAL LEVEL - THE CULMINATION OF THE 13 MOONS ✨");
                Debug.Log("═══════════════════════════════════════════════════════════════");
                if (levelBuilder == null) levelBuilder = GetComponent<Moon13LevelBuilder>();
            }
        }
    }
}
