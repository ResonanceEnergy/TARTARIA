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
            
            // VFX replacement for secret marker
            GameObject markerVFX = new GameObject($"Moon2Secret_{id}_VFX");
            markerVFX.transform.position = pos;
            
            Color markerColor = type == "epic" ? new Color(0.95f, 0.65f, 1f, 0.9f) : new Color(0.55f, 0.85f, 1f, 0.9f);
            
            ParticleSystem psMarker = markerVFX.AddComponent<ParticleSystem>();
            var mainMarker = psMarker.main;
            mainMarker.startLifetime = 1.5f;
            mainMarker.startSpeed = 0.2f;
            mainMarker.startSize = 0.35f;
            mainMarker.startColor = markerColor;
            mainMarker.maxParticles = 50;
            mainMarker.loop = true;
            
            var emissionMarker = psMarker.emission;
            emissionMarker.rateOverTime = 20f;
            
            var shapeMarker = psMarker.shape;
            shapeMarker.shapeType = ParticleSystemShapeType.Sphere;
            shapeMarker.radius = 0.175f;
            
            var rendererMarker = markerVFX.GetComponent<ParticleSystemRenderer>();
            rendererMarker.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            rendererMarker.material.SetColor("_BaseColor", markerColor);
            rendererMarker.material.EnableKeyword("_EMISSION");
            rendererMarker.material.SetColor("_EmissionColor", markerColor * 2.4f);
            
            psMarker.Play();
            
            // Cleanup
            Destroy(markerVFX, 12f);
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
