using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// AetherFieldManager — MonoBehaviour singleton that tracks the player's Resonance Score (RS).
    /// Lives in Core so both Gameplay and Integration can reference it without circular dependencies.
    ///
    /// The ECS-based AetherFieldSystem handles per-node field simulation;
    /// this manager tracks the global RS economy visible to all assemblies.
    /// </summary>
    [DisallowMultipleComponent]
    public class AetherFieldManager : MonoBehaviour
    {
        public static AetherFieldManager Instance { get; private set; }

        [Header("Resonance Score")]
        [SerializeField] float startingRS = 0f;

        float _resonanceScore;

        public float ResonanceScore => _resonanceScore;

        public event System.Action<float> OnResonanceScoreChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _resonanceScore = startingRS;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddResonanceScore(float amount)
        {
            _resonanceScore = Mathf.Clamp(_resonanceScore + amount, 0f, 100f);
            OnResonanceScoreChanged?.Invoke(_resonanceScore);
        }

        public void AddFieldEnergy(float amount)
        {
            AddResonanceScore(amount);
        }

        public void DeductRS(float amount)
        {
            AddResonanceScore(-amount);
        }
    }
}
