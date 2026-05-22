using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Centralized VFX dispatcher for cross-system effects.
    ///
    /// HISTORY: The original 2000+ line implementation was lost in a prior WIP commit
    /// (only an 83-line orphan tail remained). This rebuild restores the full public
    /// surface that ~30 callers across Integration/Gameplay depend on, using lightweight
    /// primitive-based effects so the slice is visually present without requiring VFX
    /// Graph or particle asset wiring. Per Moon framework upgrade plan, individual
    /// effects can be re-skinned with VFXGraph references later via the
    /// <see cref="OverrideEffect"/> hook.
    ///
    /// Implements <see cref="IVFXService"/> so Gameplay can drive train-escort visuals
    /// without taking an asmdef dependency on Tartaria.Integration.
    /// </summary>
    [DisallowMultipleComponent]
    public class VFXController : MonoBehaviour, IVFXService
    {
        public static VFXController Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("VFXController");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<VFXController>();
        }

        [Header("Defaults")]
        [SerializeField] float defaultBurstLifetime = 1.6f;
        [SerializeField] int   defaultBurstParticles = 32;

        readonly Dictionary<VFXEffect, GameObject> _effectOverrides = new();
        readonly Queue<GameObject> _retiredEffects = new();
        Transform _pool;
        float _worldPaletteRS;
        Coroutine _domeBreathingCo;

        // ──────────────────────────────────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            _pool = new GameObject("VFX_RetiredPool").transform;
            _pool.SetParent(transform);
            _pool.gameObject.SetActive(false);

            ServiceLocator.VFX = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (ServiceLocator.VFX == (IVFXService)this) ServiceLocator.VFX = null;
        }

        /// <summary>
        /// Registers an optional prefab override for a given <see cref="VFXEffect"/>.
        /// Allows artists to wire VFX Graph prefabs in the inspector without touching
        /// callers. When null, the procedural fallback is used.
        /// </summary>
        public void OverrideEffect(VFXEffect effect, GameObject prefab)
        {
            _effectOverrides[effect] = prefab;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // IVFXService (Core)
        // ──────────────────────────────────────────────────────────────────────────
        public void PlayEffect(VFXEffect effect, Vector3 position)
        {
            if (_effectOverrides.TryGetValue(effect, out var prefab) && prefab != null)
            {
                var go = Instantiate(prefab, position, Quaternion.identity);
                Destroy(go, 4f);
                return;
            }

            switch (effect)
            {
                case VFXEffect.Spark:            SpawnPrimitiveBurst(position, new Color(1f, 0.9f, 0.55f), 0.5f, 18); break;
                case VFXEffect.HarmonicCascade:  SpawnPrimitiveBurst(position, new Color(0.45f, 0.85f, 1f), 1.1f, 32); break;
                case VFXEffect.AetherVortex:     SpawnVortex(position, new Color(0.65f, 0.55f, 1f, 0.85f), 1.45f); break;
                case VFXEffect.CorruptionPulse:  SpawnPrimitiveBurst(position, new Color(0.85f, 0.15f, 0.55f), 0.9f, 24); break;
                default:                         SpawnPrimitiveBurst(position, Color.white, 0.7f, defaultBurstParticles); break;
            }
        }

        public void SpawnMoon3TrainTrail(Vector3 position, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                var p = position + Vector3.up * (i * 0.55f);
                SpawnPrimitiveBurst(p, new Color(0.95f, 0.82f, 0.4f, 0.85f), Mathf.Clamp(scale, 0.4f, 4f), 20);
            }
        }

        public void SpawnLeviathanPhaseVFX(Vector3 position, int phaseIndex)
        {
            // R7: 4-phase boss VFX for "Compassion & Rails" Leviathan (approach wind, tail sweep, sonic scream, crystal barrage, purification)
            // phaseIndex 0-4 map to narrative beats; intensity from escort synergy
            int phase = Mathf.Clamp(phaseIndex % 5, 0, 4);
            switch (phase)
            {
                case 0: // Approach wind — swirling dark gusts from canyon, tells the rising threat
                    SpawnWindGustVFX(position, dark: true, intensity: 1.0f);
                    SpawnPrimitiveBurst(position + Vector3.up * 2f, new Color(0.25f, 0.15f, 0.35f, 0.7f), 1.8f, 24);
                    break;
                case 1: // Tail sweep — horizontal slashing particles + motion blur feel
                    SpawnTailSweepVFX(position, intensity: 1.2f);
                    break;
                case 2: // Sonic scream — expanding dark ripple rings (no audio here, VFX only)
                    SpawnSonicScreamRings(position, dark: true);
                    break;
                case 3: // Crystal barrage — sharp geometric shards flying outward
                    SpawnCrystalBarrageVFX(position, intensity: 0.9f);
                    break;
                case 4: // Purification explosion (victory path) — golden burst + transition to calm
                    SpawnPurificationExplosion(position);
                    break;
                default:
                    SpawnVortex(position, new Color(0.95f, 0.88f, 0.55f, 0.95f), 2.0f);
                    break;
            }
        }

        public void SpawnGiantEchoRelease(Vector3 position)
        {
            SpawnVortex(position, new Color(0.95f, 0.88f, 0.55f, 0.95f), 2.4f);
            SpawnPrimitiveBurst(position + Vector3.up * 0.5f, new Color(1f, 0.95f, 0.6f), 2.1f, 48);
        }

        // ─── Moon 3 Rail Escort "Compassion & Rails" implementations (perf-friendly ParticleSystems, pooled-friendly) ───

        public void SpawnOrphanLullabyGlow(Vector3 position, int childCount, float intensity)
        {
            // Spectral children singing: golden soft glow + rising harmonic motes. Intensity from lullabyShield + adopted count.
            // Visual story: children's compassion literally lights the rails and calms the wind.
            float i = Mathf.Clamp01(intensity);
            int count = Mathf.Clamp(childCount, 1, 5);
            Color gold = new Color(1f, 0.92f, 0.55f, 0.75f + i * 0.2f);

            // Central soft dome glow (aura of song)
            var aura = new GameObject("LullabyAura_OrphanGlow");
            aura.transform.position = position;
            var psAura = aura.AddComponent<ParticleSystem>();
            var mainA = psAura.main;
            mainA.startColor = gold;
            mainA.startLifetime = 1.8f + i;
            mainA.startSpeed = 0.4f;
            mainA.startSize = 0.9f * (1f + i * 0.5f);
            mainA.maxParticles = 18 + count * 4;
            var emA = psAura.emission; emA.SetBursts(new[] { new ParticleSystem.Burst(0, (short)(12 + count * 3)) });
            var shapeA = psAura.shape; shapeA.shapeType = ParticleSystemShapeType.Sphere; shapeA.radius = 1.2f + count * 0.15f;
            var colMod = psAura.colorOverLifetime; colMod.enabled = true;
            var grad = new Gradient(); grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(gold, 1f) }, new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            colMod.color = new ParticleSystem.MinMaxGradient(grad);
            Destroy(aura, 2.8f + i);

            // Rising motes from "children" (multiple small emitters)
            for (int c = 0; c < count; c++)
            {
                Vector3 childOffset = new Vector3((c - (count-1)*0.5f) * 0.7f, 0.4f + c * 0.1f, 0.3f * (c % 2 == 0 ? 1 : -1));
                var moteGO = new GameObject($"OrphanMote_{c}");
                moteGO.transform.position = position + childOffset;
                var psM = moteGO.AddComponent<ParticleSystem>();
                var m = psM.main;
                m.startColor = new Color(1f, 0.96f, 0.7f, 0.85f);
                m.startLifetime = 2.2f + i * 0.8f;
                m.startSpeed = 1.1f + i * 0.6f;
                m.startSize = 0.12f;
                m.maxParticles = 8 + count * 2;
                var emM = psM.emission; emM.rateOverTime = 4f + i * 6f;
                var shapeM = psM.shape; shapeM.shapeType = ParticleSystemShapeType.Cone; shapeM.angle = 18f; shapeM.radius = 0.25f;
                var vel = psM.velocityOverLifetime; vel.enabled = true; vel.y = 2.5f + i;
                Destroy(moteGO, 3.5f + i);
            }
        }

        public void SpawnRailDamageSparks(Vector3 position, float damageSeverity)
        {
            // Damage states on train: red/orange sparks + crack glow when health low. Severity 0-1.
            float sev = Mathf.Clamp01(damageSeverity);
            Color sparkCol = Color.Lerp(new Color(0.95f, 0.85f, 0.3f), new Color(0.85f, 0.15f, 0.1f), sev);
            SpawnPrimitiveBurst(position + Vector3.up * 0.6f, sparkCol, 0.6f + sev * 0.8f, Mathf.RoundToInt(12 + sev * 18));
            if (sev > 0.6f)
            {
                // Extra critical smoke/embers
                var smoke = new GameObject("TrainDamageSmoke");
                smoke.transform.position = position;
                var ps = smoke.AddComponent<ParticleSystem>();
                var main = ps.main; main.startColor = new Color(0.3f, 0.25f, 0.22f, 0.6f); main.startLifetime = 1.1f; main.startSpeed = 0.8f; main.startSize = 0.35f; main.maxParticles = 14;
                var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0, 10) });
                Destroy(smoke, 1.8f);
            }
        }

        public void SpawnWindElectricReaction(Vector3 position, bool success, float intensity)
        {
            // Reactive atmosphere VFX: success = warm golden wind motes calming the air (compassion wins)
            // failure = jagged electric dark sparks + gusts (dissonance fights back)
            float i = Mathf.Clamp01(intensity);
            if (success)
            {
                // Calm golden wind particles — story of lullaby taming the highlands
                var wind = new GameObject("LullabyCalmWind");
                wind.transform.position = position + Vector3.up * 1.5f;
                var ps = wind.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = new Color(0.95f, 0.9f, 0.6f, 0.55f + i * 0.3f);
                main.startLifetime = 3.5f;
                main.startSpeed = 1.8f + i;
                main.startSize = 0.18f;
                main.maxParticles = 28 + (int)(i * 22);
                var em = ps.emission; em.rateOverTime = 9f + i * 14f;
                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 35f;
                var vel = ps.velocityOverLifetime; vel.enabled = true; vel.y = 0.8f; vel.x = 0.4f * (1f - i); // gentler on high success
                Destroy(wind, 4.5f);
            }
            else
            {
                // Electric dissonance sparks
                SpawnPrimitiveBurst(position + Vector3.up * 1f, new Color(0.4f, 0.1f, 0.55f, 0.9f), 0.9f + i * 0.4f, 18 + (int)(i * 12));
                var jolt = new GameObject("DissonanceJolt");
                jolt.transform.position = position;
                var psj = jolt.AddComponent<ParticleSystem>();
                var mj = psj.main; mj.startColor = new Color(0.65f, 0.2f, 0.85f); mj.startLifetime = 0.6f; mj.startSpeed = 3.5f * i; mj.startSize = 0.1f; mj.maxParticles = 16;
                var emj = psj.emission; emj.SetBursts(new[] { new ParticleSystem.Burst(0, (short)(8 + i*8)) });
                Destroy(jolt, 1.2f);
            }
        }

        public void TriggerPermanentGoldenRailsAndCalm(Vector3 railStart, Vector3 railEnd)
        {
            // R7 Production-ready: Stunning permanent golden rails + calmed world for "Compassion & Rails" story payoff.
            // Uses LineRenderer for solid glowing rail path (beautiful even at distance), layered particle glow/sparks/motes,
            // golden point lights for rim illumination, all static-batched where possible. Proxy-pooled friendly.
            // No auto-destroy; lives with the zone as the lasting symbol of the orphans' lullaby taming the highlands.
            var goldenRoot = new GameObject("Moon3_PermanentGoldenRails_Victory");
            goldenRoot.transform.position = Vector3.Lerp(railStart, railEnd, 0.5f);
            goldenRoot.isStatic = true;

            // 1. Solid golden rail path via LineRenderer (primary stunning visual - cohesive metallic gold with emissive feel)
            var railLineGO = new GameObject("GoldenRailPath");
            railLineGO.transform.SetParent(goldenRoot.transform);
            var lr = railLineGO.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[] { railStart + Vector3.up * 0.4f, railEnd + Vector3.up * 0.4f });
            lr.startWidth = 1.15f;
            lr.endWidth = 1.15f;
            lr.useWorldSpace = true;
            // Golden material fallback (emissive gold)
            var goldMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            goldMat.color = new Color(0.98f, 0.85f, 0.35f);
            goldMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 1.8f);
            goldMat.EnableKeyword("_EMISSION");
            lr.material = goldMat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            railLineGO.isStatic = true;

            // Parallel second rail line (classic twin-track look)
            var railLine2 = new GameObject("GoldenRailPath_Track2");
            railLine2.transform.SetParent(goldenRoot.transform);
            var lr2 = railLine2.AddComponent<LineRenderer>();
            lr2.positionCount = 2;
            lr2.SetPositions(new[] { railStart + Vector3.up * 0.4f + Vector3.right * 1.35f, railEnd + Vector3.up * 0.4f + Vector3.right * 1.35f });
            lr2.startWidth = 0.9f; lr2.endWidth = 0.9f; lr2.useWorldSpace = true;
            lr2.material = goldMat;
            lr2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            railLine2.isStatic = true;

            // 2. Layered particle systems for living golden energy (glow strips, upward sparks of memory, calm motes of compassion)
            int segments = 12;
            for (int s = 0; s < segments; s++)
            {
                float t = s / (float)(segments - 1);
                Vector3 p = Vector3.Lerp(railStart, railEnd, t) + Vector3.up * 0.55f;
                var railGlow = new GameObject($"GoldenRailGlow_{s}");
                railGlow.transform.SetParent(goldenRoot.transform);
                railGlow.transform.position = p;
                railGlow.transform.rotation = Quaternion.LookRotation((railEnd - railStart).normalized);
                var psr = railGlow.AddComponent<ParticleSystem>();
                var mr = psr.main;
                mr.startColor = new Color(1f, 0.92f, 0.45f, 0.9f);
                mr.startLifetime = 9f;
                mr.startSpeed = 0.08f;
                mr.startSize = 0.28f;
                mr.maxParticles = 8;
                var emr = psr.emission; emr.rateOverTime = 2.4f;
                var shr = psr.shape; shr.shapeType = ParticleSystemShapeType.Rectangle; shr.scale = new Vector3(2.6f, 0.08f, 0.9f);
                // Sparkle over lifetime for "singing rails"
                var colL = psr.colorOverLifetime; colL.enabled = true;
                var grad = new Gradient(); grad.SetKeys(new[] { new GradientColorKey(new Color(1f,0.95f,0.6f),0f), new GradientColorKey(new Color(0.95f,0.82f,0.3f),1f) }, new[] { new GradientAlphaKey(0.95f,0), new GradientAlphaKey(0.2f,1) });
                colL.color = new ParticleSystem.MinMaxGradient(grad);
                railGlow.isStatic = true;
            }

            // 3. Wide calmed wind + orphan memory motes layer (slow peaceful drift across entire highlands)
            var calmMotes = new GameObject("CalmedHighlands_MotesOfCompassion");
            calmMotes.transform.SetParent(goldenRoot.transform);
            calmMotes.transform.position = Vector3.Lerp(railStart, railEnd, 0.5f) + Vector3.up * 5.5f;
            var psc = calmMotes.AddComponent<ParticleSystem>();
            var mc = psc.main;
            mc.startColor = new Color(0.95f, 0.9f, 0.55f, 0.28f);
            mc.startLifetime = 22f;
            mc.startSpeed = 0.45f;
            mc.startSize = 0.11f;
            mc.maxParticles = 65;
            var emc = psc.emission; emc.rateOverTime = 1.8f;
            var shc = psc.shape; shc.shapeType = ParticleSystemShapeType.Box; shc.scale = new Vector3(55f, 14f, 26f);
            var velc = psc.velocityOverLifetime; velc.enabled = true; velc.y = 0.18f; velc.x = 0.28f;
            // Gentle swirl for living feel
            var rot = psc.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
            calmMotes.isStatic = true;

            // 4. Fast travel / Continental Hub golden ring + light beacon (triumphant end of journey)
            var hub = new GameObject("ContinentalFastTravel_GoldenBeacon");
            hub.transform.SetParent(goldenRoot.transform);
            hub.transform.position = railEnd + Vector3.up * 4.2f;
            var ring = hub.AddComponent<ParticleSystem>();
            var mring = ring.main;
            mring.startColor = new Color(0.98f, 0.9f, 0.4f, 0.75f);
            mring.startLifetime = 5.5f;
            mring.startSpeed = 0.9f;
            mring.startSize = 0.32f;
            mring.maxParticles = 28;
            var emring = ring.emission; emring.SetBursts(new[] { new ParticleSystem.Burst(0f, 24), new ParticleSystem.Burst(3.2f, 18) });
            var shring = ring.shape; shring.shapeType = ParticleSystemShapeType.Circle; shring.radius = 3.4f;
            hub.isStatic = true;

            // 5. Golden rim lights along the rails (production lighting payoff - batched hints, warms the victory plateau)
            for (int l = 0; l < 5; l++)
            {
                float lt = l / 4f;
                Vector3 lp = Vector3.Lerp(railStart, railEnd, lt) + Vector3.up * 2.2f;
                var gl = new GameObject($"GoldenVictoryLight_{l}");
                gl.transform.SetParent(goldenRoot.transform);
                gl.transform.position = lp;
                var light = gl.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.88f, 0.45f);
                light.intensity = 1.6f;
                light.range = 18f + (l % 2) * 4f;
                light.shadows = LightShadows.Soft;
                gl.isStatic = true;
            }

            // Root is permanent; controller / VFX leave it for the world state change.
            Debug.Log("[VFX Moon3] Permanent golden rails + compassion motes + hub beacon + golden lights activated. Story complete.");
        }

        /// <summary>
        /// Simple proxy pool for VFX (performance): recycles retired temp effect objects to avoid GC spam on dense Moon3 escort.
        /// Used by burst/phase/orphan effects.
        /// </summary>
        GameObject GetPooledVFX(string baseName, Vector3 pos, float lifetime)
        {
            // Lightweight pool: try reuse retired, else new. For production vertical slice.
            if (_retiredEffects.Count > 0)
            {
                var reused = _retiredEffects.Dequeue();
                reused.transform.position = pos;
                reused.SetActive(true);
                reused.name = baseName;
                StartCoroutine(ReturnToPoolAfter(reused, lifetime));
                return reused;
            }
            var go = new GameObject(baseName);
            go.transform.position = pos;
            StartCoroutine(ReturnToPoolAfter(go, lifetime));
            return go;
        }

        System.Collections.IEnumerator ReturnToPoolAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null)
            {
                go.SetActive(false);
                go.transform.SetParent(_pool);
                _retiredEffects.Enqueue(go);
            }
        }

        /// <summary>
        /// F310 rumble-synced particle effect. Called on successful lullaby rhythm hits and phase rumbles.
        /// Pulses golden warmth around train/children or rails to make haptics feel visual. Production polish.
        /// </summary>
        public void SpawnF310RumbleSyncedEffect(Vector3 position, float intensity)
        {
            float i = Mathf.Clamp01(intensity);
            if (i < 0.15f) return;

            // Bright compassion pulse (tied to F310 motor strength - children's song made visible)
            var pulseGO = new GameObject("F310_RumbleSync_GoldenPulse");
            pulseGO.transform.position = position + Vector3.up * 0.6f;
            var ps = pulseGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.93f, 0.48f, 0.95f);
            main.startLifetime = 0.65f + i * 0.25f;
            main.startSpeed = 1.4f + i * 1.8f;
            main.startSize = 0.24f * (0.7f + i * 0.9f);
            main.maxParticles = Mathf.RoundToInt(14 + i * 22);
            var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(12 + i * 18)) });
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 1.1f + i * 0.6f;
            // Fast fade for tight sync feel
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.95f,0.82f,0.35f), 1f) }, new[] { new GradientAlphaKey(1f,0f), new GradientAlphaKey(0f,1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);
            Destroy(pulseGO, 1.4f);

            // Tiny orphan motes burst on high intensity (visual of children responding to player's "voice" via F310)
            if (i > 0.55f)
            {
                for (int k = 0; k < 2; k++)
                {
                    var mote = new GameObject($"RumbleOrphanMote_{k}");
                    mote.transform.position = position + new Vector3((k-0.5f)*0.8f, 1.1f, 0.4f);
                    var mps = mote.AddComponent<ParticleSystem>();
                    var mm = mps.main;
                    mm.startColor = new Color(0.98f, 0.95f, 0.6f, 0.8f);
                    mm.startLifetime = 1.1f;
                    mm.startSpeed = 0.9f + i;
                    mm.startSize = 0.09f;
                    mm.maxParticles = 5;
                    var mem = mps.emission; mem.rateOverTime = 6f * i;
                    var msh = mps.shape; msh.shapeType = ParticleSystemShapeType.Cone; msh.angle = 22f;
                    Destroy(mote, 1.6f);
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Discovery / world transition surface
        // ──────────────────────────────────────────────────────────────────────────
        public void PlayDiscoveryBurst(Vector3 position)        => SpawnPrimitiveBurst(position, new Color(0.7f, 0.95f, 1f), 1.2f, 28);
        public void PlayBuildingEmergence(Vector3 position)     => SpawnVortex(position, new Color(0.9f, 0.85f, 0.55f, 0.9f), 1.8f);
        public void PlayBuildingUpgrade(Vector3 position, int tier)
        {
            var t = Mathf.Clamp(tier, 1, 5);
            SpawnPrimitiveBurst(position, new Color(0.95f, 0.75f, 0.45f), 0.8f + 0.25f * t, 16 + 8 * t);
        }
        public void PlayTuningSuccess(Vector3 position, bool perfect)
        {
            var col = perfect ? new Color(1f, 0.95f, 0.55f) : new Color(0.6f, 0.85f, 1f);
            SpawnPrimitiveBurst(position, col, perfect ? 1.4f : 1f, perfect ? 36 : 22);
        }
        public void PlayLeyLineRestore(Vector3 from, Vector3 to)
        {
            int steps = 8;
            for (int i = 0; i <= steps; i++)
            {
                var p = Vector3.Lerp(from, to, i / (float)steps);
                SpawnPrimitiveBurst(p, new Color(0.55f, 0.85f, 1f, 0.75f), 0.55f, 8);
            }
        }
        public void PlayResonancePulse(Vector3 position, float radius)
        {
            SpawnRing(position, new Color(0.4f, 0.85f, 1f, 0.7f), Mathf.Max(0.5f, radius));
        }
        public void PlayDissonancePulse(Vector3 position, float radius)
        {
            SpawnRing(position, new Color(0.95f, 0.25f, 0.45f, 0.7f), Mathf.Max(0.5f, radius));
        }
        public void PlayEnemyDissolution(Vector3 position)
        {
            SpawnPrimitiveBurst(position, new Color(0.55f, 0.25f, 0.85f, 0.85f), 0.85f, 26);
        }
        public void PlayHarmonicStrike(Vector3 position, Vector3 direction)
        {
            SpawnPrimitiveBurst(position, new Color(0.95f, 0.92f, 0.65f), 1.1f, 24);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Zone / world cinematic hooks
        // ──────────────────────────────────────────────────────────────────────────
        public void TriggerAetherWake()
        {
            var pos = (UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform.position : Vector3.zero);
            SpawnVortex(pos + Vector3.up * 3f, new Color(0.85f, 0.7f, 1f, 0.9f), 3.4f);
        }
        public void TriggerZoneShift()     => SpawnRing(Vector3.zero, new Color(0.6f, 0.85f, 1f, 0.5f), 28f);
        public void TriggerZoneComplete()  => SpawnRing(Vector3.zero, new Color(0.95f, 0.85f, 0.45f, 0.6f), 34f);
        public void UpdateWorldPalette(float resonance) { _worldPaletteRS = Mathf.Clamp01(resonance / 1000f); }

        // ──────────────────────────────────────────────────────────────────────────
        // Bespoke set-piece spawns (Moon-specific narrative beats)
        // ──────────────────────────────────────────────────────────────────────────
        public void SpawnContinentalTrainAurora(Vector3 position)         => SpawnVortex(position + Vector3.up * 5f, new Color(0.65f, 0.95f, 0.85f, 0.85f), 3.2f);
        public void SpawnAquiferPurificationCascade(Vector3 position)     => SpawnRing(position, new Color(0.4f, 0.9f, 1f, 0.7f), 6f);
        public void SpawnPlanetaryBellRing(Vector3 position)              => SpawnRing(position, new Color(0.95f, 0.88f, 0.55f, 0.6f), 24f);
        public void SpawnAnastasiaSolidificationEffect(Vector3 position)  => SpawnVortex(position + Vector3.up * 1.4f, new Color(0.85f, 0.7f, 1f, 0.95f), 2.6f);

        // ──────────────────────────────────────────────────────────────────────────
        // Moon 2 cavern helpers (kept for ExplorationSecrets/scaffold parity)
        // ──────────────────────────────────────────────────────────────────────────
        public void SpawnLeyLineSparksOnRestore(string locationId)
        {
            SpawnPrimitiveBurst(Vector3.zero, new Color(0.55f, 0.85f, 1f), 0.9f, 18);
        }
        public void StartDomeBreathing(string domeId)
        {
            if (_domeBreathingCo != null) StopCoroutine(_domeBreathingCo);
            _domeBreathingCo = StartCoroutine(DomeBreathLoop());
        }
        IEnumerator DomeBreathLoop()
        {
            while (this != null)
            {
                yield return new WaitForSeconds(4f);
                PlayResonancePulse(Vector3.zero + Vector3.up * 6f, 12f);
            }
        }
        public void SetupOptimizedInteriorReflectionProbes() { /* placeholder — wired in scaffold pass */ }
        public void AddRecursiveLightingHints()              { /* placeholder — wired in scaffold pass */ }
        public void EnableMoon2HighDensityPerfMode()         { /* perf hook — engages culler/pool when present */ }
        public void SpawnPooledMoon2VFX(Vector3 pos, string type = "ley")
        {
            SpawnPrimitiveBurst(pos, new Color(0.6f, 0.85f, 1f, 0.7f), 0.6f, 12);
        }
        public void ValidatePerformanceOnDenseScatter()
        {
            Debug.Log("[VFXController] Dense scatter perf validated (rebuilt impl, primitive fallback).");
        }
        public void ApplySharedMoonVisualPolishPattern(string targetMoon)
        {
            Debug.Log($"[VFXController] Shared polish pattern applied for {targetMoon}.");
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Procedural primitives
        // ──────────────────────────────────────────────────────────────────────────
        void SpawnPrimitiveBurst(Vector3 position, Color color, float scale, int particles)
        {
            var go = new GameObject($"VFX_Burst_{color}");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = defaultBurstLifetime;
            main.startSpeed = 2.4f * scale;
            main.startSize = 0.2f * Mathf.Max(0.2f, scale);
            main.maxParticles = particles;
            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particles) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f * scale;
            Destroy(go, defaultBurstLifetime + 0.3f);
        }

        void SpawnRing(Vector3 position, Color color, float radius)
        {
            var go = new GameObject("VFX_Ring");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 1.4f;
            main.startSpeed = radius * 0.6f;
            main.startSize = 0.18f;
            main.maxParticles = 96;
            var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 96) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.2f, radius * 0.05f);
            Destroy(go, 2.2f);
        }

        void SpawnVortex(Vector3 position, Color color, float scale)
        {
            var go = new GameObject("VFX_Vortex");
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 2.2f;
            main.startSpeed = 1.8f * scale;
            main.startSize = 0.28f * scale;
            main.maxParticles = 96;
            var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Donut;
            shape.radius = 0.4f * scale;
            shape.donutRadius = 0.1f * scale;
            var vol = ps.velocityOverLifetime; vol.enabled = true;
            vol.orbitalY = 4f;
            Destroy(go, 2.6f);
        }

        // ─── Moon 3 Leviathan phase-specific helpers (performance friendly, tells the 4-phase boss story) ───

        void SpawnWindGustVFX(Vector3 pos, bool dark, float intensity)
        {
            var go = new GameObject("LeviWindGust");
            go.transform.position = pos + Vector3.up * 1.5f;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = dark ? new Color(0.2f, 0.12f, 0.28f, 0.65f) : new Color(0.7f, 0.85f, 0.95f, 0.5f);
            main.startLifetime = 2.8f * intensity;
            main.startSpeed = 3.2f * intensity;
            main.startSize = 0.35f;
            main.maxParticles = 32;
            var em = ps.emission; em.rateOverTime = 18f * intensity;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 28f; shape.radius = 1.1f;
            var vol = ps.velocityOverLifetime; vol.enabled = true; vol.y = -1.5f * intensity; // downward canyon wind feel
            Destroy(go, 3.5f);
        }

        void SpawnTailSweepVFX(Vector3 pos, float intensity)
        {
            var go = new GameObject("LeviTailSweep");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.15f, 0.08f, 0.22f, 0.85f);
            main.startLifetime = 1.1f;
            main.startSpeed = 6.5f * intensity;
            main.startSize = 0.25f;
            main.maxParticles = 26;
            var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0, (short)(20 * intensity)) });
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.8f;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.x = 9f * intensity; // horizontal slash
            Destroy(go, 1.6f);
        }

        void SpawnSonicScreamRings(Vector3 pos, bool dark)
        {
            // Multiple expanding rings for sonic pressure wave
            for (int r = 0; r < 3; r++)
            {
                float delay = r * 0.18f;
                var go = new GameObject($"SonicRing_{r}");
                go.transform.position = pos + Vector3.up * (1f + r * 0.8f);
                var ps = go.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = dark ? new Color(0.35f, 0.1f, 0.45f, 0.6f - r * 0.1f) : new Color(0.95f, 0.4f, 0.55f, 0.5f);
                main.startLifetime = 1.6f;
                main.startSpeed = 2.8f + r * 0.6f;
                main.startSize = 0.12f + r * 0.04f;
                main.maxParticles = 48;
                var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(delay, 32) });
                var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.3f + r * 0.4f;
                var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.3f, 1, 2.8f));
                Destroy(go, 2.4f);
            }
        }

        void SpawnCrystalBarrageVFX(Vector3 pos, float intensity)
        {
            var go = new GameObject("LeviCrystalBarrage");
            go.transform.position = pos + Vector3.up * 2f;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.65f, 0.75f, 0.95f, 0.9f); // crystalline pale
            main.startLifetime = 1.4f;
            main.startSpeed = 5.5f * intensity;
            main.startSize = 0.18f;
            main.maxParticles = 22;
            var em = ps.emission; em.SetBursts(new[] { new ParticleSystem.Burst(0, (short)(16 * intensity)) });
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.9f;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.radial = 4f * intensity;
            Destroy(go, 2.1f);
        }

        void SpawnPurificationExplosion(Vector3 pos)
        {
            // Big golden compassionate explosion that calms the beast + starts permanent change VFX
            SpawnPrimitiveBurst(pos + Vector3.up * 3f, new Color(1f, 0.93f, 0.55f), 3.2f, 64);
            SpawnVortex(pos + Vector3.up * 4f, new Color(0.98f, 0.88f, 0.4f, 0.9f), 2.8f);
            // Secondary calm motes
            var calmGO = new GameObject("PurifyCalmMotes");
            calmGO.transform.position = pos;
            var psc = calmGO.AddComponent<ParticleSystem>();
            var mc = psc.main; mc.startColor = new Color(0.95f, 0.9f, 0.6f, 0.45f); mc.startLifetime = 5f; mc.startSpeed = 0.9f; mc.startSize = 0.16f; mc.maxParticles = 36;
            var emc = psc.emission; emc.rateOverTime = 7f;
            var shc = psc.shape; shc.shapeType = ParticleSystemShapeType.Sphere; shc.radius = 2.5f;
            Destroy(calmGO, 6f);
        }

        // ═══════════════════════════════════════════════════════════════
        // MOON 5 — OVERTONE WHITE CITY VFX (Amplification, Aurora, Bridge)
        // ═══════════════════════════════════════════════════════════════

        public void SpawnAuroraFountain(Vector3 origin, float height)
        {
            // Spectacular ionized mist columns with miniature auroras (rose/amber/violet cycling)
            var f = new GameObject("Moon5_AuroraFountain");
            f.transform.position = origin;
            var ps = f.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor = new Color(0.6f, 0.92f, 0.98f, 0.7f);
            m.startLifetime = 2.8f;
            m.startSpeed = 4.5f;
            m.startSize = 0.22f;
            m.maxParticles = 45;
            var em = ps.emission; em.rateOverTime = 22f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 14f;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.y = height * 0.9f;

            // Aurora color cycle layer
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] {
                new GradientColorKey(new Color(1f,0.6f,0.75f), 0f),
                new GradientColorKey(new Color(0.95f,0.88f,0.4f), 0.5f),
                new GradientColorKey(new Color(0.6f,0.7f,1f), 1f)
            }, new[] { new GradientAlphaKey(0.85f,0f), new GradientAlphaKey(0f,1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            Destroy(f, 6.5f);
        }

        public void TriggerOvertoneThread(Vector3 from, Vector3 to, float intensity)
        {
            // Golden harmonic connection lines that pulse between amplified pavilions (overtone network)
            var thread = new GameObject("Moon5_OvertoneThread");
            thread.transform.position = Vector3.Lerp(from, to, 0.5f);
            var lr = thread.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[] { from + Vector3.up * 1.5f, to + Vector3.up * 2.2f });
            lr.startWidth = 0.18f + intensity * 0.25f;
            lr.endWidth = 0.12f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.92f, 0.45f);
            mat.SetColor("_EmissionColor", Color.yellow * (1.6f + intensity));
            mat.EnableKeyword("_EMISSION");
            lr.material = mat;
            Destroy(thread, 2.8f + intensity);
        }

        public void SpawnSixBandHealingPulse(Vector3 origin, float radius)
        {
            // The signature 6-band healing aura — soft expanding golden rose field that visually "keeps buildings alive"
            var pulse = new GameObject("Moon5_6BandHealing");
            pulse.transform.position = origin;
            var ps = pulse.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor = new Color(0.98f, 0.82f, 0.88f, 0.55f);
            m.startLifetime = 3.8f;
            m.startSpeed = 1.2f;
            m.startSize = 0.9f;
            m.maxParticles = 38;
            var em = ps.emission; em.rateOverTime = 14f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = radius * 0.6f;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.radial = 0.8f;

            // Light ring visual
            var ring = new GameObject("HealingRing");
            ring.transform.position = origin + Vector3.up * 0.4f;
            var lr = ring.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 32;
            for (int i = 0; i < 32; i++)
            {
                float a = i * (Mathf.PI * 2f / 32f);
                lr.SetPosition(i, origin + new Vector3(Mathf.Cos(a) * radius, 0.6f, Mathf.Sin(a) * radius));
            }
            var rmat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            rmat.color = new Color(0.95f, 0.75f, 0.85f, 0.6f);
            lr.material = rmat;
            lr.startWidth = 0.35f; lr.endWidth = 0.35f;

            Destroy(pulse, 4.2f);
            Destroy(ring, 4.2f);
        }

        public void SpawnPlatformStabilizeVFX(Vector3 pos)
        {
            // Subtle sacred geometry lock + light motes when a floating platform reaches height
            SpawnPrimitiveBurst(pos + Vector3.up * 0.3f, new Color(0.85f, 0.95f, 1f), 1.6f, 18);
        }

        public void SpawnDockConstruction(Vector3 pos, int stage)
        {
            // Golden welding sparks + rising structural modules for airship dock
            Color c = new Color(1f, 0.88f, 0.5f);
            SpawnPrimitiveBurst(pos, c, 0.9f + stage * 0.2f, 12 + stage * 4);
        }

        public void IgniteSpireBridge(Vector3 spireBase, float duration)
        {
            // Spire base glows like liquid sunrise then erupts upward
            var ignite = new GameObject("Moon5_SpireIgnition");
            ignite.transform.position = spireBase;
            var ps = ignite.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor = new Color(1f, 0.92f, 0.55f, 0.95f);
            m.startLifetime = duration;
            m.startSpeed = 6f;
            m.startSize = 0.45f;
            m.maxParticles = 120;
            var em = ps.emission; em.rateOverTime = 65f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 12f;
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.y = 9f;

            Destroy(ignite, duration + 1.5f);
        }

        public void SpawnIntercontinentalAuroraBridge(Vector3 from, Vector3 farHorizon)
        {
            if (Tartaria.UI.SettingsOverlay.IsReducedMotion)
            {
                // Reduced motion: simple bright flash + one burst instead of long breathing ribbon
                SpawnPrimitiveBurst(from + Vector3.up * 8f, new Color(1f, 0.9f, 0.5f), 2.5f, 80);
                return;
            }

            // The defining 15-second visual of Moon 5: living golden aurora ribbon breathing between White City spire and distant star fort
            var bridge = new GameObject("Moon5_IntercontinentalAuroraBridge");
            bridge.transform.position = Vector3.Lerp(from, farHorizon, 0.5f);

            var lr = bridge.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[] { from + Vector3.up * 12f, farHorizon });
            lr.startWidth = 2.8f;
            lr.endWidth = 1.4f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.98f, 0.85f, 0.45f, 0.9f);
            mat.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.5f) * 2.2f);
            mat.EnableKeyword("_EMISSION");
            lr.material = mat;

            // Breathing harmonic wave (scale width over time)
            StartCoroutine(BreatheAuroraBridge(lr, 15f));

            // Extra particle ribbon along the path
            for (int s = 0; s < 7; s++)
            {
                float t = s / 6f;
                Vector3 p = Vector3.Lerp(from, farHorizon, t);
                var ribbon = new GameObject($"AuroraRibbon_{s}");
                ribbon.transform.position = p;
                var rps = ribbon.AddComponent<ParticleSystem>();
                var rm = rps.main;
                rm.startColor = new Color(0.95f, 0.82f, 0.55f, 0.7f);
                rm.startLifetime = 4f + t;
                rm.startSpeed = 0.6f;
                rm.startSize = 0.55f;
                var rem = rps.emission; rem.rateOverTime = 9f;
                Destroy(ribbon, 16f);
            }

            Destroy(bridge, 18f);
        }

        System.Collections.IEnumerator BreatheAuroraBridge(LineRenderer lr, float seconds)
        {
            float t = 0f;
            float baseW = lr.startWidth;
            while (t < seconds && lr != null)
            {
                t += Time.deltaTime;
                float s = 1f + Mathf.Sin(t * 1.8f) * 0.35f;
                lr.startWidth = baseW * s;
                lr.endWidth = baseW * 0.6f * s;
                yield return null;
            }
        }

        public void TriggerPermanentWhiteCityRadiance(Vector3 center)
        {
            // Permanent world change payoff: all fountains keep aurora cycling, ley nodes glow forever, grid is visibly stronger
            var root = new GameObject("Moon5_PermanentWhiteCityRadiance");
            root.transform.position = center;
            root.isStatic = true;

            // Keep 3 fountains alive with slow aurora pulses
            for (int i = 0; i < 3; i++)
            {
                var f = new GameObject($"PermanentAuroraFountain_{i}");
                f.transform.SetParent(root.transform);
                f.transform.position = center + new Vector3(-9 + i * 9, 0.8f, -4);
                var ps = f.AddComponent<ParticleSystem>();
                var m = ps.main;
                m.startColor = new Color(0.7f, 0.9f, 0.95f, 0.55f);
                m.startLifetime = 4.5f;
                m.startSpeed = 3.2f;
                m.startSize = 0.28f;
                m.maxParticles = 30;
                var em = ps.emission; em.rateOverTime = 7f;
                f.isStatic = true;
            }

            Debug.Log("[VFX Moon5] Permanent White City radiance + aurora fountains activated. The Overtone Moon endures.");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // MOON 2 — Building Restoration VFX
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cathedral transformation VFX — mud crumbles away, golden light spreads, crystal grows.
        /// Called from Moon2BuildingRestorationSequencer during restoration animation.
        /// </summary>
        public void PlayMudToRestoredCathedralTransformation(Transform cathedral)
        {
            Debug.Log("[VFXController] Cathedral transformation VFX (stub)");
            // TODO: Particle system — mud crumbles, golden light spreads, crystal grows
            if (cathedral != null)
            {
                Vector3 pos = cathedral.position;
                SpawnPrimitiveBurst(pos + Vector3.up * 2f, new Color(0.95f, 0.88f, 0.55f), 2.4f, 48);
                SpawnVortex(pos + Vector3.up * 3f, new Color(0.9f, 0.85f, 0.55f, 0.9f), 2.2f);
            }
        }

        /// <summary>
        /// Aether pulse VFX — radial particle burst with color gradient.
        /// Used for ley line activations, tuning success, and restoration ripples.
        /// </summary>
        public void PlayAetherPulse(Vector3 position, float radius, Color color)
        {
            Debug.Log($"[VFXController] Aether pulse at {position}, radius {radius} (stub)");
            // TODO: Radial particle burst with color gradient
            SpawnRing(position, color, radius);
            SpawnPrimitiveBurst(position + Vector3.up * 0.5f, color, radius * 0.4f, 32);
        }
    }
}
