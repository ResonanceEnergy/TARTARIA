using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Resonance Scanner System — the player's first and primary exploration tool.
    ///
    /// Design per GDD §00 (Master GDD), §15 (MVP), §02 (Aether Energy):
    ///   - Ping: sends out a resonance pulse that reveals nearby POIs
    ///   - Echo-locate: returns strongest signal direction for buried structures
    ///   - Reveal: marks discovered POIs on HUD with distance/direction
    ///   - Accuracy improves with RS level (higher RS = wider scan, more detail)
    ///   - Aether cost per scan; cooldown between pings
    ///   - Feeds scan accuracy into ExcavationSystem for RS yield bonus
    ///
    /// Performance budget: 0.5ms per scan (burst, not per-frame).
    /// </summary>
    [DisallowMultipleComponent]
    public class ResonanceScannerSystem : MonoBehaviour
    {
        public static ResonanceScannerSystem Instance { get; private set; }

        // ─── Events ───
        public event Action OnScanStarted;
        public event Action<List<ScanResult>> OnScanComplete;
        public event Action<ScanResult> OnPOIRevealed;

        [Header("Scanner Settings")]
        [SerializeField, Min(1f)] float baseScanRadius = 30f;
        [SerializeField, Min(1f)] float maxScanRadius = 80f;
        [SerializeField, Min(0f)] float scanCooldown = 5f;
        [SerializeField, Min(0f)] float aetherCostPerScan = 5f;
        [SerializeField, Min(1)] int maxPingsPerScan = 10;

        float _cooldownTimer;
        Transform _cachedPlayerTransform;
        bool _scannerUnlocked = true; // Available from game start
        readonly List<ScanResult> _lastResults = new();
        readonly List<ScanPOI> _registeredPOIs = new();

        public bool IsReady => _cooldownTimer <= 0f;
        public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);
        public IReadOnlyList<ScanResult> LastResults => _lastResults;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;
        }

        // ─── POI Registration ───

        public void RegisterPOI(ScanPOI poi)
        {
            _registeredPOIs.Add(poi);
        }

        public void UnregisterPOI(string poiId)
        {
            _registeredPOIs.RemoveAll(p => p.poiId == poiId);
        }

        // ─── Scan ───

        /// <summary>
        /// Perform a resonance scan centered on the player's position.
        /// Returns true if scan was executed, false if on cooldown or no aether.
        /// </summary>
        public bool PerformScan(Vector3 playerPosition, float currentRS)
        {
            if (!_scannerUnlocked) return false;
            if (_cooldownTimer > 0f) return false;

            // Check aether
            if (!EconomySystem.Instance?.CanAfford(CurrencyType.AetherShards, (int)aetherCostPerScan) ?? true)
            {
                Debug.Log("[Scanner] Insufficient Aether for scan.");
                return false;
            }

            EconomySystem.Instance?.SpendCurrency(CurrencyType.AetherShards, (int)aetherCostPerScan);
            _cooldownTimer = scanCooldown;

            // Calculate effective radius based on RS
            float rsNorm = Mathf.Clamp01(currentRS / 100f);
            float effectiveRadius = Mathf.Lerp(baseScanRadius, maxScanRadius, rsNorm);

            OnScanStarted?.Invoke();

            // Scan for POIs
            _lastResults.Clear();
            int found = 0;

            foreach (var poi in _registeredPOIs)
            {
                if (poi.isRevealed) continue;
                if (found >= maxPingsPerScan) break;

                float distance = Vector3.Distance(playerPosition, poi.position);
                if (distance > effectiveRadius) continue;

                // Calculate scan accuracy (closer = more accurate)
                float accuracy = 1f - (distance / effectiveRadius);
                accuracy = Mathf.Clamp01(accuracy * (1f + rsNorm * 0.5f));

                Vector3 direction = (poi.position - playerPosition).normalized;
                float signalStrength = Mathf.Lerp(0.2f, 1f, accuracy);

                var result = new ScanResult
                {
                    poiId = poi.poiId,
                    poiType = poi.poiType,
                    direction = direction,
                    distance = distance,
                    accuracy = accuracy,
                    signalStrength = signalStrength,
                    worldPosition = poi.position
                };

                _lastResults.Add(result);
                found++;

                // Mark as revealed for HUD tracking
                poi.isRevealed = true;
                OnPOIRevealed?.Invoke(result);

                // Feed accuracy to ExcavationSystem if this is a dig site
                if (poi.poiType == ScanPOIType.BuriedStructure || poi.poiType == ScanPOIType.ExcavationSite)
                {
                    ExcavationSystem.Instance?.SetScanAccuracy(poi.poiId, accuracy);
                    ExcavationSystem.Instance?.DiscoverSite(poi.poiId);
                }

                Debug.Log($"[Scanner] Found: {poi.poiId} ({poi.poiType}) at {distance:F0}m, accuracy {accuracy:P0}");
            }

            // VFX + Haptics
            ServiceLocator.VFX?.PlayEffect(VFXEffect.AetherVortex, playerPosition);
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Audio ping
            AudioManager.Instance?.PlayTone(432f, 0.3f);

            OnScanComplete?.Invoke(_lastResults);

            if (_lastResults.Count == 0)
            {
                ServiceLocator.HUD?.ShowInteractionPrompt("No resonance signals detected nearby.");
            }
            else
            {
                ServiceLocator.HUD?.ShowInteractionPrompt(
                    $"Scan complete: {_lastResults.Count} signal{(_lastResults.Count > 1 ? "s" : "")} detected.");
            }

            Debug.Log($"[Scanner] Scan complete: {found} POIs found within {effectiveRadius:F0}m radius.");
            return true;
        }

        /// <summary>
        /// Get the strongest unvisited signal direction (compass needle feature).
        /// </summary>
        public bool TryGetStrongestSignal(out Vector3 direction, out float strength)
        {
            direction = Vector3.zero;
            strength = 0f;

            var player = _cachedPlayerTransform != null ? _cachedPlayerTransform.gameObject : GameObject.FindWithTag("Player");
            if (player == null) return false;
            _cachedPlayerTransform = player.transform;

            Vector3 playerPos = player.transform.position;

            foreach (var poi in _registeredPOIs)
            {
                if (poi.isRevealed) continue;

                float dist = Vector3.Distance(playerPos, poi.position);
                float sig = 1f / Mathf.Max(dist, 1f);

                if (sig > strength)
                {
                    strength = sig;
                    direction = (poi.position - playerPos).normalized;
                }
            }

            return strength > 0f;
        }

        // ─── Save / Load ───

        public ScannerSaveData GetSaveData()
        {
            var revealed = new List<string>();
            foreach (var poi in _registeredPOIs)
            {
                if (poi.isRevealed)
                    revealed.Add(poi.poiId);
            }
            return new ScannerSaveData { revealedPOIs = revealed };
        }

        public void LoadSaveData(ScannerSaveData data)
        {
            if (data?.revealedPOIs == null) return;
            var set = new HashSet<string>(data.revealedPOIs);
            foreach (var poi in _registeredPOIs)
            {
                if (set.Contains(poi.poiId))
                    poi.isRevealed = true;
            }
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum ScanPOIType : byte
    {
        BuriedStructure = 0,
        ExcavationSite = 1,
        LeyLineNode = 2,
        CorruptionSource = 3,
        HiddenChest = 4,
        GoldenMote = 5,
        QuestObjective = 6
    }

    [Serializable]
    public class ScanPOI
    {
        public string poiId;
        public ScanPOIType poiType;
        public Vector3 position;
        public bool isRevealed;
    }

    [Serializable]
    public struct ScanResult
    {
        public string poiId;
        public ScanPOIType poiType;
        public Vector3 direction;
        public float distance;
        public float accuracy;
        public float signalStrength;
        public Vector3 worldPosition;
    }

    [Serializable]
    public class ScannerSaveData
    {
        public List<string> revealedPOIs;
    }
}
