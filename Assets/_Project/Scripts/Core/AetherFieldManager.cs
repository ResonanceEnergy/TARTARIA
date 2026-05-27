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
        [SerializeField, Range(0f, 100f)] float startingRS = 0f;

        [Header("Aether Charge")]
        [SerializeField, Range(0f, 100f)] float maxAetherCharge = 100f;

        float _resonanceScore;
        float _aetherCharge;

        public float ResonanceScore => _resonanceScore;
        public float AetherCharge => _aetherCharge;
        public float MaxAetherCharge => maxAetherCharge;
        public float AetherChargeNormalized => maxAetherCharge > 0 ? _aetherCharge / maxAetherCharge : 0f;

        public event System.Action<float> OnResonanceScoreChanged;
        public event System.Action<float> OnAetherChargeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
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

        public void AddAetherCharge(float amount)
        {
            _aetherCharge = Mathf.Clamp(_aetherCharge + amount, 0f, maxAetherCharge);
            OnAetherChargeChanged?.Invoke(_aetherCharge);
        }

        public void DeductAetherCharge(float amount)
        {
            AddAetherCharge(-amount);
        }

        public bool CanSpendAetherCharge(float amount)
        {
            return _aetherCharge >= amount;
        }
    }
}
