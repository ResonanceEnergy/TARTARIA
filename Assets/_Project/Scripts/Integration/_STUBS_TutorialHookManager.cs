using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// STUB: Placeholder for TutorialHookManager until real implementation.
    /// TODO: This type was mentioned in priority list but has ZERO code references.
    /// May be a planned feature. Replace with actual implementation when ready.
    /// </summary>
    public class TutorialHookManager : MonoBehaviour
    {
        public static TutorialHookManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Add hook registration methods as needed
        public void RegisterHook(string hookId, System.Action callback) { }
        public void TriggerHook(string hookId) { }
    }
}
