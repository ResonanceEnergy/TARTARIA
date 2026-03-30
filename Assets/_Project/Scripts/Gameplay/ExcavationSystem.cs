using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Excavation System — core Moon 1 dig mechanic for unearthing buried Tartarian structures.
    ///
    /// Design per GDD §00, §15 (MVP), §26 (Level Design):
    ///   - Player uses Resonance Scanner to locate buried sites
    ///   - Dig action penetrates layers of sediment (mud → clay → rubble → foundation)
    ///   - Each layer yields RS proportional to depth and scan accuracy
    ///   - Giant-mode structures require mandatory deep excavation (5+ layers)
    ///   - VFX and haptic hooks fire per-layer for satisfying feedback
    ///   - Performance budget: 1ms (within Gameplay 2ms budget)
    /// </summary>
    [DisallowMultipleComponent]
    public class ExcavationSystem : MonoBehaviour
    {
        public static ExcavationSystem Instance { get; private set; }

        // ─── Events ───
        public event Action<ExcavationSite> OnSiteDiscovered;
        public event Action<ExcavationSite, int> OnLayerCleared;     // site, layerIndex
        public event Action<ExcavationSite> OnExcavationComplete;
        public event Action<ExcavationSite, float> OnRSYielded;      // site, rsAmount

        [Header("Excavation Settings")]
        [SerializeField] float baseDigTime = 2f;
        [SerializeField] float layerDigTimeScale = 0.3f;  // each deeper layer adds this
        [SerializeField] float baseRSPerLayer = 3f;
        [SerializeField] float depthRSMultiplier = 0.5f;
        [SerializeField] float scanAccuracyBonus = 1.5f;  // multiplier at 100% scan accuracy
[SerializeField] float interactionRadius = 3f;

        // ─── Active Sites ───
        readonly Dictionary<string, ExcavationSite> _sites = new();
        ExcavationSite _activeSite;
        float _digProgress;
        bool _isDigging;

        public bool IsDigging => _isDigging;
        public ExcavationSite ActiveSite => _activeSite;
        public float DigProgress => _digProgress;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Site Registration ───

        public void RegisterSite(string siteId, Vector3 position, int totalLayers,
            bool isGiantMode = false, string buildingId = null)
        {
            if (_sites.ContainsKey(siteId)) return;

            var site = new ExcavationSite
            {
                siteId = siteId,
                position = position,
                totalLayers = Mathf.Max(1, totalLayers),
                layersCleared = 0,
                isGiantMode = isGiantMode,
                buildingId = buildingId,
                scanAccuracy = 0f,
                isComplete = false
            };

            _sites[siteId] = site;
        }

        public ExcavationSite? GetSite(string siteId)
        {
            return _sites.TryGetValue(siteId, out var site) ? site : null;
        }

        public List<ExcavationSite> GetAllSites() => new(_sites.Values);

        // ─── Scan Integration ───

        public void SetScanAccuracy(string siteId, float accuracy)
        {
            if (!_sites.TryGetValue(siteId, out var site)) return;
            site.scanAccuracy = Mathf.Clamp01(accuracy);
            _sites[siteId] = site;
        }

        public void DiscoverSite(string siteId)
        {
            if (!_sites.TryGetValue(siteId, out var site)) return;
            if (site.isDiscovered) return;

            site.isDiscovered = true;
            _sites[siteId] = site;
            OnSiteDiscovered?.Invoke(site);

            // HUD marker
            ServiceLocator.HUD?.ShowInteractionPrompt(
                $"Excavation site discovered: {siteId}");

            Debug.Log($"[Excavation] Site discovered: {siteId} ({site.totalLayers} layers)");
        }

        // ─── Dig Mechanic ───

        public void BeginDig(string siteId)
        {
            if (_isDigging) return;
            if (!_sites.TryGetValue(siteId, out var site)) return;
            if (site.isComplete) return;

            // Range check — player must be within interactionRadius
            var player = GameObject.FindWithTag("Player");
            if (player != null && Vector3.Distance(player.transform.position, site.position) > interactionRadius)
            {
                ServiceLocator.HUD?.ShowInteractionPrompt("Too far to dig here.");
                return;
            }

            _activeSite = site;
            _digProgress = 0f;
            _isDigging = true;

            Debug.Log($"[Excavation] Digging layer {site.layersCleared + 1}/{site.totalLayers} at {siteId}");
        }

        public void CancelDig()
        {
            _isDigging = false;
            _activeSite = default;
            _digProgress = 0f;
        }

        void Update()
        {
            if (!_isDigging) return;

            float layerTime = baseDigTime + _activeSite.layersCleared * layerDigTimeScale;
            _digProgress += Time.deltaTime / layerTime;

            if (_digProgress >= 1f)
            {
                ClearCurrentLayer();
            }
        }

        void ClearCurrentLayer()
        {
            var site = _activeSite;
            site.layersCleared++;

            // Calculate RS yield
            float depthBonus = 1f + site.layersCleared * depthRSMultiplier;
            float accuracyMult = Mathf.Lerp(1f, scanAccuracyBonus, site.scanAccuracy);
            float rsYield = baseRSPerLayer * depthBonus * accuracyMult;

            // Award RS
            AetherFieldManager.Instance?.AddResonanceScore(rsYield);
            OnRSYielded?.Invoke(site, rsYield);
            OnLayerCleared?.Invoke(site, site.layersCleared - 1);

            // VFX + Haptics per layer
            var layerType = GetLayerType(site.layersCleared - 1, site.totalLayers);
            ServiceLocator.VFX?.PlayEffect(VFXEffect.Spark, site.position);
            HapticFeedbackManager.Instance?.PlayDiscovery();

            Debug.Log($"[Excavation] Layer {site.layersCleared}/{site.totalLayers} cleared — " +
                      $"RS +{rsYield:F1} ({layerType})");

            // Check completion
            if (site.layersCleared >= site.totalLayers)
            {
                site.isComplete = true;
                _sites[site.siteId] = site;
                _isDigging = false;
                _digProgress = 0f;

                OnExcavationComplete?.Invoke(site);

                // Trigger building discovery if linked
                if (!string.IsNullOrEmpty(site.buildingId))
                {
                    ServiceLocator.GameLoop?.OnBuildingDiscovered(
                        site.buildingId, site.position);
                }

                Debug.Log($"[Excavation] Site {site.siteId} fully excavated!");
            }
            else
            {
                // Continue to next layer
                _activeSite = site;
                _sites[site.siteId] = site;
                _digProgress = 0f;
            }
        }

        static ExcavationLayerType GetLayerType(int layerIndex, int totalLayers)
        {
            float ratio = (float)layerIndex / totalLayers;
            if (ratio < 0.25f) return ExcavationLayerType.Mud;
            if (ratio < 0.50f) return ExcavationLayerType.Clay;
            if (ratio < 0.75f) return ExcavationLayerType.Rubble;
            return ExcavationLayerType.Foundation;
        }

        // ─── Save / Load ───

        public ExcavationSaveData GetSaveData()
        {
            var entries = new List<ExcavationSiteEntry>();
            foreach (var kvp in _sites)
            {
                var s = kvp.Value;
                entries.Add(new ExcavationSiteEntry
                {
                    siteId = s.siteId,
                    px = s.position.x, py = s.position.y, pz = s.position.z,
                    totalLayers = s.totalLayers,
                    layersCleared = s.layersCleared,
                    isGiantMode = s.isGiantMode,
                    buildingId = s.buildingId,
                    scanAccuracy = s.scanAccuracy,
                    isDiscovered = s.isDiscovered,
                    isComplete = s.isComplete
                });
            }
            return new ExcavationSaveData { sites = entries };
        }

        public void LoadSaveData(ExcavationSaveData data)
        {
            _sites.Clear();
            if (data?.sites == null) return;

            foreach (var e in data.sites)
            {
                _sites[e.siteId] = new ExcavationSite
                {
                    siteId = e.siteId,
                    position = new Vector3(e.px, e.py, e.pz),
                    totalLayers = e.totalLayers,
                    layersCleared = e.layersCleared,
                    isGiantMode = e.isGiantMode,
                    buildingId = e.buildingId,
                    scanAccuracy = e.scanAccuracy,
                    isDiscovered = e.isDiscovered,
                    isComplete = e.isComplete
                };
            }
        }
    }

    // ─── Data Types ──────────────────────────────

    public enum ExcavationLayerType : byte
    {
        Mud = 0,
        Clay = 1,
        Rubble = 2,
        Foundation = 3
    }

    [Serializable]
    public struct ExcavationSite
    {
        public string siteId;
        public Vector3 position;
        public int totalLayers;
        public int layersCleared;
        public bool isGiantMode;
        public string buildingId;
        public float scanAccuracy;
        public bool isDiscovered;
        public bool isComplete;
    }

    [Serializable]
    public class ExcavationSaveData
    {
        public List<ExcavationSiteEntry> sites;
    }

    [Serializable]
    public class ExcavationSiteEntry
    {
        public string siteId;
        public float px, py, pz;
        public int totalLayers;
        public int layersCleared;
        public bool isGiantMode;
        public string buildingId;
        public float scanAccuracy;
        public bool isDiscovered;
        public bool isComplete;
    }
}
