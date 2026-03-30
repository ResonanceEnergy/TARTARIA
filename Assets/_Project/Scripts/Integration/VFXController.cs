using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// VFX Controller — manages all particle effects and visual feedback.
    /// Uses Unity's built-in particle system with runtime-generated configurations.
    ///
    /// Effects are created at startup and pooled. Each effect type has a
    /// ParticleSystem prefab that is repositioned and played on demand.
    ///
    /// Phase 1 effects:
    ///   - Discovery burst (golden sparkles)
    ///   - Tuning success (frequency rings)
    ///   - Building emergence (mud dissolution particles)
    ///   - Resonance pulse (expanding ring)
    ///   - Harmonic strike (directional wave)
    ///   - Shield activation (sphere shell)
    ///   - Enemy dissolution (corruption evaporating)
    ///   - Aether collection (blue wisps)
    ///   - World palette shift (ambient particles reflecting RS)
    /// </summary>
    [DisallowMultipleComponent]
    public class VFXController : MonoBehaviour, IVFXService
    {
        public static VFXController Instance { get; private set; }

        // Pooled particle systems
        ParticleSystem _discoveryBurst;
        ParticleSystem _tuningRings;
        ParticleSystem _emergenceParticles;
        ParticleSystem _resonancePulseVFX;
        ParticleSystem _harmonicStrikeVFX;
        ParticleSystem _shieldVFX;
        ParticleSystem _dissolutionVFX;
        ParticleSystem _ambientParticles;

        // World palette
        float _currentRS;
        static readonly Color GoldenGlow = new(0.95f, 0.82f, 0.35f);
        static readonly Color AetherBlue = new(0.2f, 0.6f, 0.95f);
        static readonly Color MudBrown = new(0.4f, 0.3f, 0.2f);
        static readonly Color CelestialWhite = new(1f, 0.95f, 0.85f);

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.VFX = this;
            CreateParticleSystems();
        }

        // ─── Initialization ──────────────────────────

        void CreateParticleSystems()
        {
            _discoveryBurst = CreateSystem("VFX_Discovery", GoldenGlow, 50, 0.3f, 1.5f, burst: true);
            _tuningRings = CreateSystem("VFX_Tuning", AetherBlue, 30, 0.2f, 1f, burst: true);
            _emergenceParticles = CreateSystem("VFX_Emergence", GoldenGlow, 100, 0.5f, 5f, burst: false);
            _resonancePulseVFX = CreateSystem("VFX_ResonancePulse", AetherBlue, 40, 0.1f, 0.8f, burst: true);
            _harmonicStrikeVFX = CreateSystem("VFX_HarmonicStrike", GoldenGlow, 25, 0.15f, 0.6f, burst: true);
            _shieldVFX = CreateSystem("VFX_Shield", CelestialWhite, 60, 0.05f, 2f, burst: false);
            _dissolutionVFX = CreateSystem("VFX_Dissolution", MudBrown, 80, 0.3f, 2f, burst: true);
            _ambientParticles = CreateAmbientSystem();
        }

        ParticleSystem CreateSystem(string name, Color color, int maxParticles,
            float size, float lifetime, bool burst)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startLifetime = lifetime;
            main.maxParticles = maxParticles;
            main.playOnAwake = false;
            main.loop = !burst;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = true;
            if (burst)
            {
                emission.rateOverTime = 0;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)maxParticles) });
            }
            else
            {
                emission.rateOverTime = maxParticles / lifetime;
            }

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1f;

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 0.7f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.1f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLife.color = gradient;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            // Renderer setup
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = color;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        ParticleSystem CreateAmbientSystem()
        {
            var go = new GameObject("VFX_Ambient");
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = new Color(0.8f, 0.7f, 0.4f, 0.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.maxParticles = 200;
            main.playOnAwake = true;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);

            var emission = ps.emission;
            emission.rateOverTime = 5; // Sparse at start, denser at high RS

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(80f, 20f, 80f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = new Color(0.8f, 0.7f, 0.4f, 0.3f);

            return ps;
        }

        // ─── IVFXService ─────────────────────────────

        public void PlayEffect(VFXEffect effect, Vector3 position)
        {
            switch (effect)
            {
                case VFXEffect.Spark:           PlayAt(_discoveryBurst, position, 1f); break;
                case VFXEffect.AetherVortex:    PlayAt(_resonancePulseVFX, position, 3f); break;
                case VFXEffect.DiscoveryBurst:  PlayDiscoveryBurst(position); break;
                case VFXEffect.BuildingEmergence: PlayBuildingEmergence(position); break;
                case VFXEffect.HarmonicStrike:  PlayHarmonicStrike(position, Vector3.forward); break;
                case VFXEffect.ShieldActivation: PlayShieldActivation(position); break;
                case VFXEffect.EnemyDissolution: PlayEnemyDissolution(position); break;
                case VFXEffect.ResonancePulse:  PlayAt(_resonancePulseVFX, position, 5f); break;
                default: PlayAt(_discoveryBurst, position, 1f); break;
            }
        }

        // ─── Public Effect Triggers ──────────────────

        public void PlayDiscoveryBurst(Vector3 position)
        {
            PlayAt(_discoveryBurst, position, 3f);
        }

        public void PlayTuningSuccess(Vector3 position, bool isPerfect)
        {
            if (isPerfect)
            {
                // Extra golden burst for perfect tuning
                _tuningRings.transform.position = position;
                var main = _tuningRings.main;
                main.startColor = GoldenGlow;
                main.startSize = 0.4f;
                _tuningRings.Play();
            }
            PlayAt(_tuningRings, position, 1.5f);
        }

        public void PlayBuildingEmergence(Vector3 position)
        {
            _emergenceParticles.transform.position = position;
            var shape = _emergenceParticles.shape;
            shape.radius = 10f; // Large area for building
            _emergenceParticles.Play();
        }

        public void PlayBuildingUpgrade(Vector3 position, int tier)
        {
            _emergenceParticles.transform.position = position;
            var main = _emergenceParticles.main;
            main.startSize = 2f + tier * 0.5f;
            _emergenceParticles.Play();
        }

        public void PlayResonancePulse(Vector3 position, float range)
        {
            _resonancePulseVFX.transform.position = position;
            var shape = _resonancePulseVFX.shape;
            shape.radius = range;
            _resonancePulseVFX.Play();
        }

        public void PlayHarmonicStrike(Vector3 position, Vector3 direction)
        {
            _harmonicStrikeVFX.transform.position = position;
            _harmonicStrikeVFX.transform.forward = direction;
            var shape = _harmonicStrikeVFX.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            _harmonicStrikeVFX.Play();
        }

        public void PlayShieldActivation(Vector3 position)
        {
            _shieldVFX.transform.position = position;
            _shieldVFX.Play();
        }

        public void PlayEnemyDissolution(Vector3 position)
        {
            PlayAt(_dissolutionVFX, position, 5f);
        }

        /// <summary>
        /// Plays a dissonance corruption pulse at the given world position.
        /// Used by ZerethController during dissonance events.
        /// </summary>
        public void PlayDissonancePulse(Vector3 position, float radius)
        {
            // Reuse the resonance pulse system with inverted aesthetics
            _resonancePulseVFX.transform.position = position;
            var shape = _resonancePulseVFX.shape;
            shape.radius = radius;
            var main = _resonancePulseVFX.main;
            main.startColor = new Color(0.6f, 0.1f, 0.3f); // dark dissonance red-violet
            _resonancePulseVFX.Play();
        }

        /// <summary>
        /// Plays a ley line restoration visual at the midpoint between restored nodes.
        /// </summary>
        public void PlayLeyLineRestore(Vector3 midpoint)
        {
            _resonancePulseVFX.transform.position = midpoint;
            var main = _resonancePulseVFX.main;
            main.startColor = new Color(0.2f, 0.9f, 0.4f); // restoration green
            _resonancePulseVFX.Play();
        }

        // ─── World Palette ───────────────────────────

        /// <summary>
        /// Updates ambient particle density and color based on RS.
        /// Called by GameLoopController each RS poll.
        /// </summary>
        public void UpdateWorldPalette(float rs)
        {
            _currentRS = rs;

            if (_ambientParticles == null) return;

            // Ambient particle density scales with RS
            var emission = _ambientParticles.emission;
            emission.rateOverTime = Mathf.Lerp(2f, 40f, rs / 100f);

            // Color shifts from brown dust → golden mist → Aether wisps
            var main = _ambientParticles.main;
            Color ambientColor;
            if (rs < 50f)
                ambientColor = Color.Lerp(MudBrown, GoldenGlow, rs / 50f);
            else
                ambientColor = Color.Lerp(GoldenGlow, AetherBlue, (rs - 50f) / 50f);

            ambientColor.a = Mathf.Lerp(0.1f, 0.4f, rs / 100f);
            main.startColor = ambientColor;
        }

        // ─── Threshold Events ────────────────────────

        public void TriggerAetherWake()
        {
            // Golden mist eruption at RS 50
            if (_ambientParticles != null)
            {
                _ambientParticles.Emit(100); // Burst of particles
            }
        }

        public void TriggerZoneShift()
        {
            // Full color transformation burst
            if (_ambientParticles != null)
            {
                _ambientParticles.Emit(200);
            }
        }

        public void TriggerZoneComplete()
        {
            // Grand aurora effect
            if (_ambientParticles != null)
            {
                var main = _ambientParticles.main;
                main.startColor = CelestialWhite;
                _ambientParticles.Emit(300);
            }
        }

        // ─── Helpers ─────────────────────────────────

        void PlayAt(ParticleSystem ps, Vector3 position, float shapeRadius = 1f)
        {
            if (ps == null) return;
            ps.transform.position = position;
            var shape = ps.shape;
            shape.radius = shapeRadius;
            ps.Play();
        }

        // ─── Roadmap v8 VFX ─────────────────────────

        /// <summary>Anastasia solidification — golden crystal pillar burst (DotT finale).</summary>
        public void SpawnAnastasiaSolidificationEffect(Vector3 position)
        {
            if (_ambientParticles != null)
            {
                _ambientParticles.transform.position = position;
                var main = _ambientParticles.main;
                main.startColor = new Color(0.95f, 0.82f, 0.35f, 1f); // gold
                var shape = _ambientParticles.shape;
                shape.radius = 8f;
                _ambientParticles.Emit(500);
            }
            Debug.Log("[VFX] Anastasia Solidification Effect spawned.");
        }

        /// <summary>Planetary bell ring — expanding golden rings (Veritas organ concert).</summary>
        public void SpawnPlanetaryBellRing(Vector3 position)
        {
            if (_tuningRings != null)
            {
                PlayAt(_tuningRings, position, 15f);
                var main = _tuningRings.main;
                main.startColor = new Color(0.85f, 0.75f, 0.3f, 1f);
                main.startLifetime = 6f;
                _tuningRings.Emit(200);
            }
            Debug.Log("[VFX] Planetary Bell Ring spawned.");
        }

        /// <summary>Continental train aurora — trailing ribbon of light along rail network.</summary>
        public void SpawnContinentalTrainAurora(Vector3 position)
        {
            if (_ambientParticles != null)
            {
                _ambientParticles.transform.position = position;
                var main = _ambientParticles.main;
                main.startColor = new Color(0.4f, 0.9f, 0.7f, 0.8f); // aurora green
                var shape = _ambientParticles.shape;
                shape.radius = 20f;
                _ambientParticles.Emit(400);
            }
            Debug.Log("[VFX] Continental Train Aurora spawned.");
        }

        /// <summary>Aquifer purification cascade — blue-white water cleansing burst.</summary>
        public void SpawnAquiferPurificationCascade(Vector3 position)
        {
            if (_ambientParticles != null)
            {
                _ambientParticles.transform.position = position;
                var main = _ambientParticles.main;
                main.startColor = new Color(0.6f, 0.85f, 1f, 1f); // purified blue
                var shape = _ambientParticles.shape;
                shape.radius = 12f;
                _ambientParticles.Emit(350);
            }
            Debug.Log("[VFX] Aquifer Purification Cascade spawned.");
        }

        /// <summary>Prophecy stone alignment — ley line energy converging on activated stones.</summary>
        public void SpawnProphecyStoneAlignment(Vector3 position)
        {
            if (_tuningRings != null)
            {
                PlayAt(_tuningRings, position, 5f);
                var main = _tuningRings.main;
                main.startColor = new Color(0.7f, 0.5f, 1f, 1f); // prophecy violet
                main.startLifetime = 4f;
                _tuningRings.Emit(150);
            }
            Debug.Log("[VFX] Prophecy Stone Alignment spawned.");
        }
    }
}
