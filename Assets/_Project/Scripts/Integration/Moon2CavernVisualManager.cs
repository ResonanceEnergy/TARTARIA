using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Visual manager for the Moon 2 (Crystalline Caverns) zone.
    ///
    /// HISTORY: The original implementation was lost in a prior WIP commit. This
    /// rebuild restores the public surface that <see cref="Moon2ExplorationSecrets"/>
    /// and Moon2ZoneScaffold depend on, with real (but minimal) visible behaviour
    /// — discovery markers materialise, resonance pulses fire through
    /// <see cref="VFXController"/>, and the permanent epic upgrade re-tints
    /// any registered crystal/vein/foliage renderers.
    ///
    /// Heavy-asset upgrades (fractal vein shader passes, godrays, 9-probe interior
    /// lighting) are wired through <see cref="VFXController"/> hooks so artists can
    /// later attach VFX Graph prefabs without touching this manager.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2CavernVisualManager : MonoBehaviour
    {
        [Header("Discovered visuals (auto-populated)")]
        [SerializeField] Renderer[] _crystalRenderers;
        [SerializeField] Renderer[] _veinRenderers;
        [SerializeField] Renderer[] _foliageRenderers;
        [SerializeField] ReflectionProbe[] _interiorProbes;

        bool _epicPolishApplied;

        void Awake()
        {
            if (_crystalRenderers == null || _crystalRenderers.Length == 0) DiscoverAllVisualProps();
        }

        /// <summary>Caches all renderers/probes under this transform for later effect dispatch.</summary>
        public void DiscoverAllVisualProps()
        {
            var all = GetComponentsInChildren<Renderer>(true);
            var crystals = new System.Collections.Generic.List<Renderer>();
            var veins    = new System.Collections.Generic.List<Renderer>();
            var foliage  = new System.Collections.Generic.List<Renderer>();
            foreach (var r in all)
            {
                var n = r.name.ToLowerInvariant();
                if      (n.Contains("crystal")) crystals.Add(r);
                else if (n.Contains("vein") || n.Contains("ley")) veins.Add(r);
                else if (n.Contains("foliage") || n.Contains("grass") || n.Contains("moss")) foliage.Add(r);
            }
            _crystalRenderers = crystals.ToArray();
            _veinRenderers    = veins.ToArray();
            _foliageRenderers = foliage.ToArray();
            _interiorProbes   = GetComponentsInChildren<ReflectionProbe>(true);
        }

        /// <summary>Re-runs discovery and (optionally) resets material property blocks.</summary>
        public void ForceReDiscoverAndResetVisuals(bool resetTints)
        {
            DiscoverAllVisualProps();
            if (!resetTints) return;
            var block = new MaterialPropertyBlock();
            foreach (var r in _crystalRenderers) if (r != null) r.SetPropertyBlock(block);
            foreach (var r in _veinRenderers)    if (r != null) r.SetPropertyBlock(block);
            foreach (var r in _foliageRenderers) if (r != null) r.SetPropertyBlock(block);
        }

        public void SetupOptimizedInteriorReflectionProbes()
        {
            if (_interiorProbes == null) return;
            foreach (var probe in _interiorProbes)
            {
                if (probe == null) continue;
                probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
                probe.resolution = 128;
            }
        }

        public void SpawnCrystalResonancePulse()
        {
            ServiceLocator.VFX?.PlayResonancePulse(transform.position, 4f);
        }

        public void SpawnLeyLineSparksOnRestore(string locationId)
        {
            ServiceLocator.VFX?.PlayLeyLineRestore(transform.position, transform.position + Vector3.up * 4f);
        }

        public void RevealMoon2SecretVisual(string id, Vector3 pos, string type, string hint)
        {
            ServiceLocator.VFX?.PlayDiscoveryBurst(pos);
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = $"Moon2Secret_{id}";
            marker.transform.position = pos;
            marker.transform.localScale = Vector3.one * 0.35f;
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = renderer.material;
                mat.color = type == "epic" ? new Color(0.95f, 0.65f, 1f, 0.85f) : new Color(0.55f, 0.85f, 1f, 0.85f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", mat.color * 2.4f);
            }
            Object.Destroy(marker.GetComponent<Collider>());
            Object.Destroy(marker, 12f);
        }

        public void ApplyMoon2EpicSecretPermanentVisualUpgrade()
        {
            if (_epicPolishApplied) return;
            _epicPolishApplied = true;

            var block = new MaterialPropertyBlock();
            var emissive = new Color(0.55f, 0.85f, 1.0f) * 1.4f;
            foreach (var r in _crystalRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", emissive);
                r.SetPropertyBlock(block);
            }
            foreach (var r in _veinRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", emissive * 0.7f);
                r.SetPropertyBlock(block);
            }
            ServiceLocator.VFX?.PlayResonancePulse(transform.position, 9f);
        }
    }
}
