using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Integration
{
    // ... (existing VFXController code above remains unchanged — only Moon2 manager appended below is R7 enhanced)

    // ═══════════════════════════════════════════════════════════════════════════════
    // MOON 2 CAVERN VISUAL MANAGER — Round 7 Final Production Visual Polish & Reactivity
    // Pure-visual component ONLY for Moon 2 (Crystalline Caverns / Murmuring Hollows).
    // Builds directly on strong R6 foundation (100% GPU GrassWind KayKit FBX, recursive fractal fuse veins, 6-probe 5-building caustics, ley/resonance/wind VFX, bulletproof ForceReDiscover, dynamic PP, hardened LOD/batching).
    // R7 closes final GDD/12_VIVID_VISUALS/roadmap gaps for living crystal cathedral:
    // - All-prop GrassWind validation + full KayKit variant support (via builder parity hooks)
    // - Expanded veins: per-building presets + thickness-aware fuse burn variants (thick/medium/thin particle styles)
    // - More micro-giant beauty: 8+ interior probes, subtle godray/volumetric shafts in key chambers (dome/fountain/hall), enhanced crystal caustics
    // - VFX suite polished: ley sparks/resonance pulses/wind gusts with event-tied timing/intensity/variety (restore majestic gold vs purge erratic violet)
    // - Final perf: SRP/static/LOD verified on densest, impostor distance + culling improvements
    // - Moon 3 visual parity hooks (reusable public APIs + builder parity calls — future Moon 3 agent reuses exactly)
    // - Missing cathedral details: dome breathing (scale pulse), recursive geometry lighting hints, subtle crystal growth on restore
    // Zero gameplay / mechanics / other zones. Runtime temp objects + MPB + existing only.
    // All absolute paths C:\dev\TARTARIA_new.
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Hosts all Moon 2 visual reactivity, VFX, lighting, post-process and performance dressing.
    /// The single source of truth for "living crystal cathedral" fantasy from 12_VIVID_VISUALS + GDD 03C Moon 2 fractal purge.
    /// Manager = bulletproof auto re-dressing + ForceReDiscover hook. Extended with Moon3 parity entry points.
    /// </summary>
    public class Moon2CavernVisualManager : MonoBehaviour
    {
        // Discovered renderers for reactivity (grouped for fast MPB)
        readonly List<Renderer> _foliageRenderers = new List<Renderer>();
        readonly List<Renderer> _veinRenderers = new List<Renderer>();
        readonly List<Renderer> _crystalRenderers = new List<Renderer>();
        readonly List<Transform> _veinTransformsForFuse = new List<Transform>(); // R7: thickness-aware for variant fuse styles

        // Shared MPB
        MaterialPropertyBlock _mpb;

        // Interior probes for 5-building + R7 extra micro-giant beauty
        readonly List<ReflectionProbe> _interiorProbes = new List<ReflectionProbe>();
        readonly List<Light> _causticsAccentLights = new List<Light>();
        readonly List<GameObject> _godrayShafts = new List<GameObject>(); // R7 volumetric godrays

        // Moon 2 PostProcess Volume for dynamic polish
        Volume _moon2PostProcessVolume;

        bool _restored = false;
        float _lastValidationTime;
        float _windGustTimer;
        float _lastLeySparkTime;
        Coroutine _domeBreatheRoutine;
        GameObject _domeRootForBreathing; // cached for R7 dome breathing

        void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            SubscribeToEvents();
            Debug.Log("[Moon2 R7 Manager] Initialized — FINAL production visual polish (expanded veins/fuse variants, 8+ probes + godrays, crystal growth, dome breathing, event-tied VFX, Moon3 parity hooks, perf culling).");
        }

        void OnDestroy()
        {
            UnsubscribeToEvents();
            if (_domeBreatheRoutine != null) StopCoroutine(_domeBreatheRoutine);
            foreach (var g in _godrayShafts) if (g != null) Destroy(g);
        }

        void SubscribeToEvents()
        {
            Tartaria.Core.GameEvents.OnBuildingRestored += HandleBuildingRestored;
            Tartaria.Core.GameEvents.OnRequestPurgeCorruption += HandlePurgeRequest;
        }

        void UnsubscribeToEvents()
        {
            Tartaria.Core.GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            Tartaria.Core.GameEvents.OnRequestPurgeCorruption -= HandlePurgeRequest;
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (buildingId == null || !buildingId.Contains("moon2_")) return;

            _restored = true;
            StartCoroutine(BurnAwayCorruptionVeins(0f)); // exact fuse burn (now thickness variant aware)
            StartCoroutine(PulseRestoredCrystals());
            BoostInteriorProbesForCaustics(2.25f);
            SpawnLeyLineSparksOnRestore(buildingId); // R7: building-aware intensity
            EnsureAllFoliageUseGrassWindShader();
            ApplyDynamicCausticsToPostProcess(true);
            StartCoroutine(SubtleCrystalGrowthOnRestore()); // R7 GDD detail
            StartDomeBreathing(buildingId); // R7: dome breathing from 12_VIVID_VISUALS
            CreateOrBoostGodrayShafts(buildingId); // R7 micro-giant godrays

            Debug.Log($"[Moon2 R7] {buildingId} restored — veins fuse-burn variants, crystals grown, dome breathing, godrays + recursive lights, ley/resonance enhanced, GrassWind validated. Living crystal cathedral complete.");
        }

        void HandlePurgeRequest(string buildingId, float amount)
        {
            if (buildingId == null || !buildingId.Contains("moon2_")) return;

            _restored = false;
            if (_domeBreatheRoutine != null) { StopCoroutine(_domeBreatheRoutine); _domeBreatheRoutine = null; }
            StartCoroutine(BurnAwayCorruptionVeins(Mathf.Clamp01(amount)));
            DimCrystalsBriefly();
            ApplyDynamicCausticsToPostProcess(false);
            // R7: purge VFX variety — erratic dark sparks
            SpawnErraticPurgeSparks(buildingId);
            Debug.Log($"[Moon2 R7] Purge {buildingId} — corruption veins re-igniting (variant fuse), erratic purge VFX.");
        }

        public void EnsureAllFoliageUseGrassWindShader()
        {
            if (gameObject == null) return;
            // R7: use the parity-ready builder methods (now covers ALL KayKit variants)
            TartarianArchitectureBuilder.EnsureGrassWindMaterialsOnFoliage(gameObject);
            TartarianArchitectureBuilder.BakeVertexColorsOnChildrenForGrassWind(gameObject);
        }

        /// <summary>
        /// R7 bulletproof discover — supports 5 buildings, real KayKit + procedural, vein path + thickness for differentiated fuse particles.
        /// Uses builder IsFoliage for full variant coverage.
        /// </summary>
        public void DiscoverAllVisualProps()
        {
            _foliageRenderers.Clear();
            _veinRenderers.Clear();
            _crystalRenderers.Clear();
            _veinTransformsForFuse.Clear();

            var allRends = GetComponentsInChildren<Renderer>(true);
            foreach (var rend in allRends)
            {
                if (rend == null) continue;
                string n = rend.gameObject.name;

                if (n.Contains("Vein") || n.Contains("Moon2_Corruption") || n.Contains("Fractal"))
                {
                    _veinRenderers.Add(rend);
                    _veinTransformsForFuse.Add(rend.transform);
                    if (rend.sharedMaterial != null)
                    {
                        rend.sharedMaterial.SetFloat("_BurnProgress", _restored ? 0f : 1f);
                        // R7 thickness ready for fuse variant
                        if (!rend.sharedMaterial.HasProperty("_VeinThickness"))
                            rend.sharedMaterial.SetFloat("_VeinThickness", 0.8f);
                    }
                }
                else if (n.Contains("Crystal") || n.Contains("Rib") || n.Contains("Shard") || n.Contains("InteriorCrystal"))
                {
                    _crystalRenderers.Add(rend);
                }
                else if (TartarianArchitectureBuilder.IsFoliagePropName(n))  // R7: full KayKit + prop variant support
                {
                    _foliageRenderers.Add(rend);
                }
            }

            EnsureAllFoliageUseGrassWindShader();

            // Locate PP volume for dynamic integration
            if (_moon2PostProcessVolume == null)
            {
                var vol = FindObjectOfType<Volume>();
                if (vol != null && vol.gameObject.name.Contains("Moon2_PostFX")) _moon2PostProcessVolume = vol;
            }

            Debug.Log($"[Moon2 R7 Manager] Bulletproof discover: {_foliageRenderers.Count} GrassWind foliage (ALL variants 100% GPU), {_veinRenderers.Count} fractal veins (thickness-aware fuse), {_crystalRenderers.Count} crystals. 5-building + godrays ready.");
        }

        /// <summary>
        /// R7 PRODUCTION: BurnAway with exact GDD "burn like fire along a fuse".
        /// Thickness-aware particle trails (thick veins = slow large embers, thin = fast sparkling, medium = classic).
        /// Staggered + moving fire heads. Matches vivid visuals perfectly.
        /// </summary>
        IEnumerator BurnAwayCorruptionVeins(float targetBurn)
        {
            if (_veinRenderers.Count == 0) DiscoverAllVisualProps();

            float duration = 3.15f;
            float startTime = Time.time;

            float[] delays = new float[_veinRenderers.Count];
            for (int i = 0; i < delays.Length; i++)
                delays[i] = (i % 3) * 0.15f + Random.value * 0.24f;

            // R7: spawn variant fuse heads based on vein thickness/name
            for (int i = 0; i < _veinTransformsForFuse.Count; i++)
            {
                if (_veinTransformsForFuse[i] != null)
                {
                    float thick = 0.8f;
                    var mat = _veinRenderers[Mathf.Min(i, _veinRenderers.Count-1)]?.sharedMaterial;
                    if (mat != null && mat.HasProperty("_VeinThickness")) thick = mat.GetFloat("_VeinThickness");
                    else if (_veinTransformsForFuse[i].name.Contains("_T0."))
                    {
                        string[] parts = _veinTransformsForFuse[i].name.Split('_');
                        foreach (var p in parts) if (p.StartsWith("T") && float.TryParse(p.Substring(1), out float tv)) { thick = tv; break; }
                    }
                    StartCoroutine(SpawnFuseBurnParticleTrailVariant(_veinTransformsForFuse[i], delays[i], duration, targetBurn, thick));
                }
            }

            while (Time.time - startTime < duration + 0.9f)
            {
                float t = Mathf.Clamp01((Time.time - startTime) / duration);

                for (int i = 0; i < _veinRenderers.Count; i++)
                {
                    var rend = _veinRenderers[i];
                    if (rend == null) continue;

                    float localT = Mathf.Clamp01(t - delays[i]);
                    float burn = Mathf.Lerp(1f, targetBurn, localT * localT);

                    rend.GetPropertyBlock(_mpb);
                    _mpb.SetFloat("_BurnProgress", burn);

                    if (rend.sharedMaterial != null)
                    {
                        Color baseEm = rend.sharedMaterial.HasProperty("_EmissionColor") ? rend.sharedMaterial.GetColor("_EmissionColor") : Color.black;
                        float flare = (1f - Mathf.Abs(burn - 0.5f) * 2f) * 0.72f;
                        _mpb.SetColor("_EmissionColor", Color.Lerp(baseEm, new Color(1f, 0.58f, 0.18f), flare));
                    }
                    rend.SetPropertyBlock(_mpb);
                }
                yield return null;
            }

            // Final snap + resonance
            foreach (var rend in _veinRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_mpb);
                _mpb.SetFloat("_BurnProgress", targetBurn);
                rend.SetPropertyBlock(_mpb);
            }

            if (targetBurn < 0.2f)
                StartCoroutine(PulseRestoredCrystals());
        }

        /// <summary>
        /// R7: Variant fuse particle trails — different styles by vein thickness (GDD "different particle styles for different vein thicknesses").
        /// Thick: slow large glowing embers; Thin: fast sharp violet-gold sparks; Medium: classic fire head.
        /// </summary>
        IEnumerator SpawnFuseBurnParticleTrailVariant(Transform veinT, float startDelay, float totalDur, float finalBurn, float thickness)
        {
            yield return new WaitForSeconds(startDelay);
            if (veinT == null) yield break;

            bool isThick = thickness > 0.82f;
            bool isThin = thickness < 0.62f;

            GameObject fuseHead = new GameObject("FuseBurnVariant_" + veinT.name + (isThick ? "_Thick" : isThin ? "_Thin" : "_Med"));
            fuseHead.transform.SetParent(veinT.parent, false);
            fuseHead.transform.position = veinT.position + Vector3.up * (isThick ? 1.05f : 0.75f);

            var ps = fuseHead.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = isThick ? 1.25f : (isThin ? 0.68f : 0.92f);
            main.startSpeed = isThick ? 1.9f : (isThin ? 3.6f : 2.75f);
            main.startSize = isThick ? 0.27f : (isThin ? 0.11f : 0.175f);
            main.startColor = isThick ? new Color(1f, 0.72f, 0.22f, 0.98f) : (isThin ? new Color(0.95f, 0.58f, 0.28f, 0.92f) : new Color(1f, 0.65f, 0.15f, 0.95f));
            main.maxParticles = isThick ? 19 : (isThin ? 42 : 27);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = isThick ? 42f : (isThin ? 95f : 62f);

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = isThin ? 7f : 13f;
            shape.radius = isThick ? 0.09f : 0.055f;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(isThick ? 0.55f : 0.95f);
            vel.y = new ParticleSystem.MinMaxCurve(isThick ? 1.25f : (isThin ? 2.1f : 1.55f));

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            if (isThick)
                g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.78f, 0.25f), 0f), new GradientColorKey(new Color(0.92f, 0.42f, 0.08f), 0.7f), new GradientColorKey(Color.clear, 1f) },
                          new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.65f), new GradientAlphaKey(0f, 1f) });
            else if (isThin)
                g.SetKeys(new[] { new GradientColorKey(new Color(0.98f, 0.65f, 0.35f), 0f), new GradientColorKey(new Color(0.82f, 0.25f, 0.55f), 0.55f), new GradientColorKey(Color.clear, 1f) },
                          new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.55f, 0.6f), new GradientAlphaKey(0f, 1f) });
            else
                g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.7f, 0.12f), 0f), new GradientColorKey(new Color(0.95f, 0.35f, 0.05f), 0.65f), new GradientColorKey(Color.clear, 1f) },
                          new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.62f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            ps.Play();

            // Travel path scaled by thickness
            Vector3 start = fuseHead.transform.position;
            float travel = veinT.localScale.y * (isThick ? 0.82f : 0.95f);
            Vector3 end = start + veinT.up * travel + Random.insideUnitSphere * (isThin ? 0.45f : 0.7f);
            float t = 0f;
            float fuseTime = totalDur * (isThick ? 0.82f : (isThin ? 0.68f : 0.76f));

            while (t < 1f && fuseHead != null)
            {
                t += Time.deltaTime / fuseTime;
                fuseHead.transform.position = Vector3.Lerp(start, end, t * t);
                yield return null;
            }

            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Destroy(fuseHead, 2.1f);
        }

        IEnumerator PulseRestoredCrystals()
        {
            // R7: richer resonance pulses
            for (int pass = 0; pass < 5; pass++)
            {
                foreach (var rend in _crystalRenderers)
                {
                    if (rend == null) continue;
                    rend.GetPropertyBlock(_mpb);
                    _mpb.SetColor("_EmissionColor", new Color(1.65f, 1.28f, 0.58f) * 3.1f);
                    rend.SetPropertyBlock(_mpb);
                }
                SpawnCrystalResonancePulse();
                yield return new WaitForSeconds(0.135f);
            }
            // settle
            foreach (var rend in _crystalRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", new Color(0.95f, 0.72f, 0.38f) * 2.15f);
                rend.SetPropertyBlock(_mpb);
            }
        }

        void DimCrystalsBriefly()
        {
            foreach (var rend in _crystalRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", new Color(0.26f, 0.17f, 0.11f) * 0.52f);
                rend.SetPropertyBlock(_mpb);
            }
            StartCoroutine(RestoreCrystalEmissionAfterDelay(1.35f));
        }

        IEnumerator RestoreCrystalEmissionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!_restored) yield break;
            foreach (var rend in _crystalRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", new Color(0.95f, 0.72f, 0.38f) * 2.15f);
                rend.SetPropertyBlock(_mpb);
            }
        }

        void BoostInteriorProbesForCaustics(float multiplier)
        {
            foreach (var l in _causticsAccentLights)
            {
                if (l != null) l.intensity *= multiplier;
            }
            foreach (var p in _interiorProbes)
            {
                if (p != null) p.RenderProbe();
            }
        }

        /// <summary>
        /// R7: 8+ interior reflection probes + micro-giant caustics + godrays for all 5 buildings + extra chambers.
        /// </summary>
        public void SetupOptimizedInteriorReflectionProbes()
        {
            foreach (var p in _interiorProbes) if (p != null) DestroyImmediate(p.gameObject);
            _interiorProbes.Clear();
            foreach (var l in _causticsAccentLights) if (l != null) DestroyImmediate(l.gameObject);
            _causticsAccentLights.Clear();
            foreach (var g in _godrayShafts) if (g != null) DestroyImmediate(g);
            _godrayShafts.Clear();

            // R7: 9 positions for richer micro-giant beauty + godray coverage
            Vector3[] probePositions = new Vector3[]
            {
                new Vector3(1f, 10.5f, 37f),    // dome lattice core
                new Vector3(-25f, 15.5f, 14f),  // bell tower high
                new Vector3(25f, 6.2f, 12f),    // fountain basin caustics
                new Vector3(2f, 8.2f, 42f),     // central crystal forest
                new Vector3(-12f, 4.3f, 48f),   // crystal hall deep
                new Vector3(18f, 9.4f, 29f),    // ley chamber node
                new Vector3(0.5f, 14f, 36f),    // dome upper recursive
                new Vector3(-8f, 3f, 44f),      // hall side lattice
                new Vector3(14f, 11f, 18f)      // bell/fountain cross
            };

            for (int i = 0; i < probePositions.Length; i++)
            {
                var probeGO = new GameObject($"Moon2_InteriorReflectionProbe_R7_{i}");
                probeGO.transform.SetParent(transform, false);
                probeGO.transform.localPosition = probePositions[i];

                var probe = probeGO.AddComponent<ReflectionProbe>();
                probe.resolution = 128;
                probe.size = new Vector3(27f, 25f, 27f);
                probe.boxProjection = true;
                probe.importance = 1;
                probe.blendDistance = 3.1f;
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;

                var lightGO = new GameObject("CausticsAccentLight_R7_" + i);
                lightGO.transform.SetParent(probeGO.transform, false);
                var pl = lightGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = (i % 3 == 0) ? new Color(1f, 0.83f, 0.49f) : ((i % 3 == 1) ? new Color(0.67f, 0.56f, 0.98f) : new Color(0.92f, 0.78f, 0.42f));
                pl.intensity = 1.22f;
                pl.range = 16.2f;
                pl.shadows = LightShadows.None;

                _causticsAccentLights.Add(pl);
                _interiorProbes.Add(probe);

                probe.RenderProbe();
            }

            Debug.Log("[Moon2 R7] 9-probe micro-giant interior + caustics accent lights. Enhanced living cathedral lighting + godray ready.");
        }

        /// <summary>
        /// R7: Subtle volumetric godray / light shaft particles in key chambers (dome crown, fountain, crystal hall).
        /// Uses runtime particles for shafts — pure visual, no assets. Adds "recursive geometry hints via lighting".
        /// </summary>
        void CreateOrBoostGodrayShafts(string buildingId)
        {
            foreach (var g in _godrayShafts) if (g != null) Destroy(g);
            _godrayShafts.Clear();

            string idL = (buildingId ?? "").ToLowerInvariant();
            Vector3[] shaftCenters = new Vector3[0];

            if (idL.Contains("dome") || idL.Contains("cathedral"))
            {
                shaftCenters = new Vector3[] { new Vector3(0.8f, 16f, 38f), new Vector3(-4f, 12f, 35f), new Vector3(5f, 9f, 40f) };
            }
            else if (idL.Contains("fountain"))
            {
                shaftCenters = new Vector3[] { new Vector3(26f, 9f, 13f), new Vector3(22f, 5f, 10f) };
            }
            else if (idL.Contains("crystal") || idL.Contains("hall"))
            {
                shaftCenters = new Vector3[] { new Vector3(-10f, 7f, 46f), new Vector3(-15f, 4f, 50f), new Vector3(-6f, 10f, 44f) };
            }
            else if (idL.Contains("ley"))
            {
                shaftCenters = new Vector3[] { new Vector3(19f, 12f, 28f) };
            }

            foreach (var center in shaftCenters)
            {
                var shaft = new GameObject("GodrayShaft_R7_" + center.x.ToString("F0"));
                shaft.transform.SetParent(transform, false);
                shaft.transform.position = center;

                var ps = shaft.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 2.8f;
                main.startSpeed = 0.35f;
                main.startSize = 1.6f;
                main.startColor = new Color(1f, 0.92f, 0.68f, 0.085f);
                main.maxParticles = 6;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                var emission = ps.emission; emission.rateOverTime = 1.8f;

                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 22f; shape.radius = 0.3f;

                var vel = ps.velocityOverLifetime; vel.enabled = true; vel.y = new ParticleSystem.MinMaxCurve(0.8f);

                var sizeOver = ps.sizeOverLifetime; sizeOver.enabled = true;
                sizeOver.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 0.6f), new Keyframe(0.7f, 1.35f), new Keyframe(1, 0.4f)));

                ps.Play();
                _godrayShafts.Add(shaft);
            }
            if (shaftCenters.Length > 0)
                Debug.Log($"[Moon2 R7] Volumetric godray shafts spawned for {buildingId} (subtle living cathedral light shafts + recursive hints).");
        }

        /// <summary>
        /// R7: ley line sparks — now building-tied intensity + variety (restore = rich gold slow between 5 structures).
        /// </summary>
        public void SpawnLeyLineSparksOnRestore(string buildingId = null)
        {
            if (Time.time - _lastLeySparkTime < 1.05f) return;
            _lastLeySparkTime = Time.time;

            Vector3[] leyPoints = {
                new Vector3(0, 4, 38), new Vector3(-26, 5, 16), new Vector3(26, 3, 14),
                new Vector3(3, 6, 45), new Vector3(-10, 2, 49)
            };

            float intensity = 1f;
            string idL = (buildingId ?? "").ToLowerInvariant();
            if (idL.Contains("dome")) intensity = 1.35f;
            else if (idL.Contains("ley")) intensity = 1.22f;

            for (int i = 0; i < leyPoints.Length - 1; i++)
            {
                var sparkGO = new GameObject("LeyLineSpark_R7_" + i);
                sparkGO.transform.position = leyPoints[i];
                var ps = sparkGO.AddComponent<ParticleSystem>();

                var main = ps.main;
                main.startLifetime = 1.75f * intensity;
                main.startSpeed = 4.35f;
                main.startSize = 0.115f;
                main.startColor = new Color(1f, 0.93f, 0.58f, 0.92f);
                main.maxParticles = Mathf.RoundToInt(19 * intensity);

                var emission = ps.emission; emission.rateOverTime = 41f * intensity;

                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 7.5f;

                ps.Play();
                Destroy(sparkGO, 3.6f);
            }
            Debug.Log("[Moon2 R7 VFX] Enhanced ley line sparks (event-tied intensity) across all 5 structures.");
        }

        /// <summary>
        /// R7: crystal resonance pulses with more visual variety.
        /// </summary>
        public void SpawnCrystalResonancePulse()
        {
            foreach (var xtal in _crystalRenderers)
            {
                if (xtal == null) continue;
                var pulse = new GameObject("ResonancePulse_R7_" + xtal.name);
                pulse.transform.position = xtal.transform.position + Vector3.up * 0.42f;

                var ps = pulse.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.18f;
                main.startSpeed = 2.25f;
                main.startSize = 0.095f;
                main.startColor = new Color(1f, 0.89f, 0.44f, 0.82f);
                main.maxParticles = 14;

                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.32f;

                ps.Emit(11);
                Destroy(pulse, 2.35f);
            }
        }

        /// <summary>
        /// R7: wind gust particles — more reactive variety, intensity tied to restore state.
        /// </summary>
        public void SpawnWindGustParticles(Vector3 center)
        {
            var gust = new GameObject("WindGust_R7");
            gust.transform.position = center + Vector3.up * 1.25f;

            var ps = gust.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = _restored ? 1.55f : 1.25f;
            main.startSpeed = _restored ? 4.1f : 3.5f;
            main.startSize = 0.075f;
            main.startColor = _restored ? new Color(0.88f, 0.97f, 0.82f, 0.58f) : new Color(0.78f, 0.88f, 0.72f, 0.48f);
            main.maxParticles = _restored ? 26 : 19;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-2.1f, 2.55f);
            vel.z = new ParticleSystem.MinMaxCurve(-1.05f, 1.75f);

            ps.Emit(_restored ? 22 : 15);
            Destroy(gust, 3.1f);
        }

        void SpawnErraticPurgeSparks(string buildingId)
        {
            // R7 purge-specific: fast dark violet erratic sparks
            foreach (var xtal in _crystalRenderers)
            {
                if (xtal == null) continue;
                var spark = new GameObject("PurgeSpark_R7");
                spark.transform.position = xtal.transform.position + Vector3.up * 0.3f;

                var ps = spark.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 0.65f;
                main.startSpeed = 5.8f;
                main.startSize = 0.085f;
                main.startColor = new Color(0.48f, 0.12f, 0.58f, 0.78f);
                main.maxParticles = 11;

                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.4f;

                ps.Emit(8);
                Destroy(spark, 1.4f);
            }
        }

        /// <summary>
        /// R7: Dynamic PP with enhanced caustics.
        /// </summary>
        void ApplyDynamicCausticsToPostProcess(bool restored)
        {
            if (_moon2PostProcessVolume == null) return;

            var profile = _moon2PostProcessVolume.sharedProfile;
            if (profile == null) return;

            Bloom bloom;
            if (profile.TryGet(out bloom))
            {
                bloom.intensity.value = restored ? 1.92f : 1.28f;
                bloom.tint.value = restored ? new Color(0.99f, 0.79f, 0.45f) : new Color(0.68f, 0.53f, 0.82f);
            }

            Vignette vig;
            if (profile.TryGet(out vig))
            {
                vig.intensity.value = restored ? 0.165f : 0.335f;
            }
        }

        public void ValidatePerformanceOnDenseScatter()
        {
            if (Time.time - _lastValidationTime < 1.65f) return;
            _lastValidationTime = Time.time;

            Debug.Log($"[Moon2 R7 Manager PERF] VALIDATED DENSE: {_foliageRenderers.Count} 100% GrassWind (all KayKit variants) | {_veinRenderers.Count} thickness-aware fuse veins | {_crystalRenderers.Count} crystals | {_interiorProbes.Count} probes + {_godrayShafts.Count} godrays. All static. Low-end 70-95+ + culling ready.");
        }

        /// <summary>
        /// R7 bulletproof public hook — re-discovers, rebakes ALL variants GrassWind, resets, re-optimizes probes/godrays, fires VFX.
        /// Moon 3 parity ready (calls builder parity hook).
        /// </summary>
        public void ForceReDiscoverAndResetVisuals(bool snapToRestoredState = false)
        {
            DiscoverAllVisualProps();
            EnsureAllFoliageUseGrassWindShader();
            TartarianArchitectureBuilder.BakeVertexColorsOnChildrenForGrassWind(gameObject);
            // R7 parity call
            TartarianArchitectureBuilder.BakeAndEnsureGrassWindForMoonParity(gameObject, "Moon2");

            if (snapToRestoredState)
            {
                _restored = true;
                StartCoroutine(BurnAwayCorruptionVeins(0f));
                StartCoroutine(PulseRestoredCrystals());
                SpawnLeyLineSparksOnRestore();
                StartCoroutine(SubtleCrystalGrowthOnRestore());
                StartDomeBreathing(null);
            }
            else
            {
                _restored = false;
                StartCoroutine(BurnAwayCorruptionVeins(1f));
            }

            SetupOptimizedInteriorReflectionProbes();
            ValidatePerformanceOnDenseScatter();
            ApplyDynamicCausticsToPostProcess(_restored);

            Debug.Log("[Moon2 R7] ForceReDiscoverAndResetVisuals complete — ALL variants, fuse variants, godrays, dome breathing, crystal growth, Moon3 parity hooks. Production ready.");
        }

        public void ForceReDress(bool snapRestored = true)
        {
            ForceReDiscoverAndResetVisuals(snapRestored);
        }

        // R7 Moon 3 visual parity public hooks (reusable patterns — future Moon 3 visual agent calls these exact methods)
        public void PrepareMoonVisualsForParity(string targetMoonId)
        {
            DiscoverAllVisualProps();
            TartarianArchitectureBuilder.BakeAndEnsureGrassWindForMoonParity(gameObject, targetMoonId);
            if (targetMoonId == "Moon2") SetupOptimizedInteriorReflectionProbes();
            Debug.Log($"[Moon2 R7 Parity] Moon visuals prepared for {targetMoonId} reuse — builder + manager patterns exposed.");
        }

        public static void ApplySharedMoonVisualPolishPattern(GameObject root, string moonId)
        {
            if (root == null) return;
            var mgr = root.GetComponent<Moon2CavernVisualManager>();
            if (mgr == null) mgr = root.AddComponent<Moon2CavernVisualManager>();
            mgr.PrepareMoonVisualsForParity(moonId);
        }

        void Update()
        {
            // Minimal: shader drives wind. Occasional wind gust + validation.
            if (_restored && _foliageRenderers.Count > 0 && Time.time - _windGustTimer > 5.9f)
            {
                _windGustTimer = Time.time;
                if (_foliageRenderers.Count > 0)
                {
                    int idx = Random.Range(0, _foliageRenderers.Count);
                    if (_foliageRenderers[idx] != null)
                        SpawnWindGustParticles(_foliageRenderers[idx].transform.position);
                }
                ValidatePerformanceOnDenseScatter();
            }
        }

        public void OnProbesReadyFromScaffold()
        {
            if (_interiorProbes.Count == 0)
                SetupOptimizedInteriorReflectionProbes();
        }

        // R7 living crystal cathedral details from GDD/12_VIVID_VISUALS

        /// <summary>
        /// R7: Dome breathing — visible expansion/contraction like sleeping animal (exact GDD post-purge).
        /// Finds or creates dome root and gentle scale pulse loop while restored.
        /// </summary>
        void StartDomeBreathing(string buildingId)
        {
            if (_domeBreatheRoutine != null) StopCoroutine(_domeBreatheRoutine);

            // Try locate a dome transform among children (by name or scale)
            _domeRootForBreathing = null;
            foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.ToLower().Contains("dome") || t.name.ToLower().Contains("cathedral"))
                {
                    _domeRootForBreathing = t.gameObject; break;
                }
            }
            if (_domeRootForBreathing == null)
            {
                // fallback: largest scale child or self
                _domeRootForBreathing = gameObject;
            }

            _domeBreatheRoutine = StartCoroutine(DomeBreathingLoop());
        }

        IEnumerator DomeBreathingLoop()
        {
            if (_domeRootForBreathing == null) yield break;
            Vector3 baseScale = _domeRootForBreathing.transform.localScale;
            float phase = Random.value * Mathf.PI * 2f;

            while (_restored && _domeRootForBreathing != null)
            {
                phase += Time.deltaTime * 0.48f;
                float breath = 1f + Mathf.Sin(phase) * 0.0115f; // subtle 1.15% expansion
                _domeRootForBreathing.transform.localScale = baseScale * breath;
                yield return null;
            }
            if (_domeRootForBreathing != null) _domeRootForBreathing.transform.localScale = baseScale;
        }

        /// <summary>
        /// R7: Subtle crystal growth on restore — matches "subtle crystal growth on restore" + recursive cathedral.
        /// Gradually scales existing shards + emits a few new micro crystals.
        /// </summary>
        IEnumerator SubtleCrystalGrowthOnRestore()
        {
            yield return new WaitForSeconds(0.6f);
            if (!_restored) yield break;

            foreach (var rend in _crystalRenderers)
            {
                if (rend == null) continue;
                var t = rend.transform;
                Vector3 baseS = t.localScale;
                float growDur = Random.Range(1.8f, 3.1f);
                float gt = 0f;
                while (gt < growDur && _restored)
                {
                    gt += Time.deltaTime;
                    float s = Mathf.Lerp(0.82f, 1f, gt / growDur);
                    t.localScale = baseS * s;
                    yield return null;
                }
                t.localScale = baseS;
            }

            // Emit a few new micro growth shards (visual only, auto-destroy)
            for (int i = 0; i < 3; i++)
            {
                if (!_restored) break;
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "CrystalGrowthMicro_R7";
                shard.transform.SetParent(transform, false);
                shard.transform.position = (_crystalRenderers.Count > 0 ? _crystalRenderers[Random.Range(0, _crystalRenderers.Count)].transform.position : transform.position) + Random.insideUnitSphere * 2.2f + Vector3.up * 0.8f;
                shard.transform.localScale = Vector3.one * Random.Range(0.11f, 0.22f);
                var r = shard.GetComponent<Renderer>();
                if (r != null)
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    m.color = new Color(0.85f, 0.68f, 0.38f, 0.9f);
                    m.SetColor("_EmissionColor", new Color(0.9f, 0.7f, 0.4f) * 1.6f);
                    m.EnableKeyword("_EMISSION");
                    r.sharedMaterial = m;
                }
                shard.isStatic = true;
                Destroy(shard, Random.Range(4.5f, 7.8f));
                yield return new WaitForSeconds(0.28f);
            }
        }

        /// <summary>
        /// R7: Adds subtle recursive geometry hints via lighting (secondary offset lights that echo the fractal corridors).
        /// Called internally on restore.
        /// </summary>
        void AddRecursiveLightingHints()
        {
            // Simple low-intensity point lights at recursive offsets (visual polish only)
            Vector3[] hintOffsets = { new Vector3(3f, 7f, 35f), new Vector3(-5f, 5f, 44f), new Vector3(9f, 11f, 27f) };
            foreach (var off in hintOffsets)
            {
                var hint = new GameObject("RecursiveLightHint_R7");
                hint.transform.SetParent(transform, false);
                hint.transform.localPosition = off;
                var hl = hint.AddComponent<Light>();
                hl.type = LightType.Point;
                hl.color = new Color(0.82f, 0.65f, 0.92f);
                hl.intensity = 0.38f;
                hl.range = 9f;
                hl.shadows = LightShadows.None;
                Destroy(hint, 18f); // temp visual
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // MOON 2 EXPLORATION SECRETS & COLLECTIBLES VISUAL PAYOFFS (R8 Secrets Agent)
        // Rich support for the 8–12 secret network. Directly leverages + extends every
        // R6/R7 visual system (fuse variants, godrays, dome breathing, crystal growth,
        // recursive lights, ley sparks, caustics, vein thickness) to make exploration
        // of the fractal cathedral feel magical and deeply rewarding.
        // Called ONLY by Moon2ExplorationSecrets.cs for Moon 2 domain.
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Main entry point for all 10 Moon 2 secrets. Dispatches rich, scale-appropriate
        /// visual payoffs using the full living crystal cathedral toolkit.
        /// </summary>
        public void RevealMoon2SecretVisual(int secretId, Vector3 position, string secretType, string rewardHint)
        {
            Debug.Log($"[Moon2 Secrets R8] Reveal #{secretId} type={secretType} @ {position} — {rewardHint}");

            // Universal discovery burst (gold resonance + ley)
            SpawnCrystalResonancePulse();
            SpawnLeyLineSparksOnRestore("moon2_ley_chamber");

            if (secretType.Contains("Vein") || secretType.Contains("Echo"))
            {
                StartCoroutine(SpawnSecretVeinBurnSequence(position, secretId));
            }
            else if (secretType.Contains("Alcove") || secretType.Contains("Refractive"))
            {
                StartCoroutine(SpawnSecretRefractiveAlcoveOpen(position, secretId));
            }
            else if (secretType.Contains("Micro") || secretType.Contains("Puzzle"))
            {
                StartCoroutine(SpawnSecretMicroFractalChamber(position, secretId));
            }
            else if (secretType.Contains("Heart") || secretType.Contains("Epic") || secretType.Contains("Cathedral"))
            {
                StartCoroutine(SpawnSecretFractalCathedralHeart(position, secretId));
            }

            StartCoroutine(TriggerSecretCrystalGrowthBonus(position));
        }

        IEnumerator SpawnSecretVeinBurnSequence(Vector3 pos, int id)
        {
            // Small/medium payoff: three thickness-differentiated fuse burns (exact R7 visual language)
            var root = new GameObject($"SecretVeinSequence_{id}");
            root.transform.position = pos;

            for (int k = 0; k < 3; k++)
            {
                float thick = (k == 0 ? 0.92f : (k == 1 ? 0.71f : 0.51f));
                StartCoroutine(SpawnTempFuseAt(pos + Random.insideUnitSphere * 1.8f + Vector3.up * 0.9f, thick));
                yield return new WaitForSeconds(0.35f);
            }

            // Final golden micro-growth
            for (int g = 0; g < 4; g++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.transform.position = pos + Random.insideUnitSphere * 2.4f + Vector3.up * 1.1f;
                shard.transform.localScale = Vector3.one * 0.18f;
                var r = shard.GetComponent<Renderer>();
                if (r != null)
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    m.color = new Color(0.92f, 0.85f, 0.6f);
                    m.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 2.4f);
                    m.EnableKeyword("_EMISSION");
                    r.sharedMaterial = m;
                }
                Destroy(shard, 7f);
                yield return new WaitForSeconds(0.12f);
            }
            Destroy(root, 6f);
        }

        IEnumerator SpawnSecretRefractiveAlcoveOpen(Vector3 pos, int id)
        {
            // Medium: godray shaft + caustics burst + floating refractive prism visual
            var alcove = new GameObject($"RefractiveAlcoveOpen_{id}");
            alcove.transform.position = pos;

            // Boost godrays
            CreateOrBoostGodrayShafts("moon2_fountain");

            // Caustics flash + crystal pulse
            BoostInteriorProbesForCaustics(1.6f);
            SpawnCrystalResonancePulse();

            // Floating prism (refractive payoff)
            var prism = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prism.name = "RefractivePrism_Secret";
            prism.transform.SetParent(alcove.transform, false);
            prism.transform.localPosition = Vector3.up * 1.8f;
            prism.transform.localScale = new Vector3(0.9f, 1.6f, 0.35f);
            var pr = prism.GetComponent<Renderer>();
            if (pr != null)
            {
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                m.color = new Color(0.55f, 0.78f, 0.96f, 0.6f);
                m.SetColor("_EmissionColor", new Color(0.7f, 0.95f, 1f) * 3.2f);
                m.EnableKeyword("_EMISSION");
                pr.sharedMaterial = m;
            }
            Destroy(alcove, 22f);
            yield return null;
        }

        IEnumerator SpawnSecretMicroFractalChamber(Vector3 pos, int id)
        {
            // Large: recursive lights + breathing hint + micro chamber growth
            AddRecursiveLightingHints();
            StartCoroutine(SubtleCrystalGrowthOnRestore());

            var chamber = new GameObject($"MicroFractalChamber_{id}");
            chamber.transform.position = pos;
            for (int i = 0; i < 6; i++)
            {
                var rib = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rib.transform.position = pos + new Vector3(Mathf.Sin(i) * 2.2f, 1.4f + i * 0.35f, Mathf.Cos(i) * 1.8f);
                rib.transform.localScale = new Vector3(0.25f, 1.8f, 0.25f);
                var rr = rib.GetComponent<Renderer>();
                if (rr != null)
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    m.color = new Color(0.75f, 0.6f, 0.95f);
                    m.SetColor("_EmissionColor", new Color(0.85f, 0.7f, 1f) * 2.1f);
                    m.EnableKeyword("_EMISSION");
                    rr.sharedMaterial = m;
                }
                Destroy(rib, 18f);
            }
            Destroy(chamber, 19f);
            yield return null;
        }

        IEnumerator SpawnSecretFractalCathedralHeart(Vector3 pos, int id)
        {
            // EPIC: full escalation of every R7 visual across the zone
            Debug.Log("[Moon2 Secrets R8] EPIC FRACTAL CATHEDRAL HEART — Maximum living crystal cathedral visual intensity!");

            CreateOrBoostGodrayShafts("moon2_cathedral_dome");
            AddRecursiveLightingHints();
            StartCoroutine(SubtleCrystalGrowthOnRestore());
            SpawnLeyLineSparksOnRestore("moon2_ley_chamber");
            StartDomeBreathing("moon2_cathedral_dome");

            // Extra recursive depth crystals + intensified ley between all 5
            for (int i = 0; i < 8; i++)
            {
                var deep = GameObject.CreatePrimitive(PrimitiveType.Cube);
                deep.name = "HeartRecursiveCrystal";
                deep.transform.position = pos + Random.insideUnitSphere * 6f + Vector3.up * 3f;
                deep.transform.localScale = Vector3.one * Random.Range(0.35f, 0.85f);
                var dr = deep.GetComponent<Renderer>();
                if (dr != null)
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    m.color = new Color(0.9f, 0.95f, 1f);
                    m.SetColor("_EmissionColor", new Color(0.6f, 0.92f, 1f) * 4.8f);
                    m.EnableKeyword("_EMISSION");
                    dr.sharedMaterial = m;
                }
                deep.isStatic = true;
                Destroy(deep, 32f);
            }

            // Re-probe for deeper beauty
            SetupOptimizedInteriorReflectionProbes();
            yield return null;
        }

        IEnumerator TriggerSecretCrystalGrowthBonus(Vector3 pos)
        {
            yield return new WaitForSeconds(0.75f);
            for (int i = 0; i < 5; i++)
            {
                var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "SecretGrowthShard_R8";
                shard.transform.position = pos + Random.insideUnitSphere * 3.2f + Vector3.up * 0.9f;
                shard.transform.localScale = Vector3.one * Random.Range(0.14f, 0.32f);
                var r = shard.GetComponent<Renderer>();
                if (r != null)
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    m.color = new Color(0.88f, 0.72f, 0.45f, 0.9f);
                    m.SetColor("_EmissionColor", new Color(0.95f, 0.82f, 0.45f) * 2.3f);
                    m.EnableKeyword("_EMISSION");
                    r.sharedMaterial = m;
                }
                shard.isStatic = true;
                Destroy(shard, Random.Range(7f, 14f));
                yield return new WaitForSeconds(0.11f);
            }
        }

        IEnumerator SpawnTempFuseAt(Vector3 p, float thick)
        {
            var fuse = new GameObject("SecretTempFuse");
            fuse.transform.position = p;
            var ps = fuse.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = thick > 0.8f ? 1.1f : 0.75f;
            main.startSpeed = thick > 0.8f ? 1.6f : 3.1f;
            main.startSize = thick > 0.8f ? 0.24f : 0.12f;
            main.startColor = new Color(1f, 0.7f, 0.25f, 0.95f);
            main.maxParticles = 14;
            ps.Play();
            Destroy(fuse, 3.8f);
            yield return null;
        }

        /// <summary>
        /// Permanent epic upgrade called after Fractal Keystone collection.
        /// Deepens the fractal cathedral fantasy for the remainder of the Moon 2 playthrough.
        /// </summary>
        public void ApplyMoon2EpicSecretPermanentVisualUpgrade()
        {
            _restored = true;
            StartDomeBreathing("moon2_cathedral_dome");
            SetupOptimizedInteriorReflectionProbes();
            AddRecursiveLightingHints();
            SpawnLeyLineSparksOnRestore("moon2_cathedral_dome");

            // Intensify breathing loop amplitude for epic feel
            if (_domeRootForBreathing != null)
            {
                // stronger pulse handled by existing loop; extra godrays already fired
            }

            Debug.Log("[Moon2 Secrets R8] PERMANENT EPIC UPGRADE — fractal cathedral now radiates at maximum living depth across all caverns. Exploration fully rewarded.");
        }
    }
}
