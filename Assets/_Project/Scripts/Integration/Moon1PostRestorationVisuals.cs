using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1PostRestorationVisuals - Sprint 6 Lane 9.
    ///
    /// Subscribes to GameEvents.OnMoonCompleted (canonical
    /// Action<MoonCompletedEventArgs>, GameEvents.cs:192; moonIndex at
    /// GameEvents.cs:799). On args.moonIndex == 1, plays a single 30-second
    /// cinematic transformation of Echohaven from muddy-dusk to golden-hour:
    /// fountain water + audio, star-dome projection, spire Telluric emission
    /// pulse, golden drift particles at each hero building, and a
    /// directional-light color/intensity ramp.
    ///
    /// Per CLAUDE.md no-debt rule 4 (no silent fallback): every transform.Find
    /// that returns null logs an error citing expected child path AND parent
    /// hierarchy path. Per rule 3 (no silent fails): every error is loud.
    /// One-shot via static _alreadyPlayed guard.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1PostRestorationVisuals : MonoBehaviour
    {
        static bool _alreadyPlayed;
        static Moon1PostRestorationVisuals _instance;

        const float CinematicDuration = 30f;
        const string FountainWaterChild = "FountainWater";
        const string FountainAudioChild = "FountainAudio";
        const string StarProjectionChild = "StarProjection";

        static readonly string[] DomeNames =
            { "Building_dome", "EchohavenStarDome", "StarDome", "Echohaven_StarDome" };
        static readonly string[] FountainNames =
            { "Building_fountain", "EchohavenHarmonicFountain", "HarmonicFountain",
              "Fountain", "Echohaven_HarmonicFountain" };
        static readonly string[] SpireNames =
            { "Building_spire", "EchohavenCrystalSpire", "CrystalSpire",
              "Spire", "Echohaven_CrystalSpire", "Cathedral" };
        static readonly global::UnityEngine.Color SunStartColor =
            new global::UnityEngine.Color(160f / 255f, 120f / 255f, 90f / 255f);
        static readonly global::UnityEngine.Color SunEndColor =
            new global::UnityEngine.Color(255f / 255f, 210f / 255f, 140f / 255f);
        const float SunStartIntensity = 0.8f;
        const float SunEndIntensity = 1.4f;

        static readonly global::UnityEngine.Color TelluricEmissionBase =
            new global::UnityEngine.Color(0.65f, 0.45f, 0.2f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject(nameof(Moon1PostRestorationVisuals));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1PostRestorationVisuals>();
        }

        void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
        }

        void OnDestroy()
        {
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
            if (_instance == this) _instance = null;
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            if (args == null)
            {
                Debug.LogError("[Moon1PostRestorationVisuals] OnMoonCompleted fired with null args - " +
                               "publisher should pass populated MoonCompletedEventArgs (GameEvents.cs:797).");
                return;
            }
            if (args.moonIndex != 1) return;
            if (_alreadyPlayed)
            {
                Debug.Log("[Moon1PostRestorationVisuals] Skipped - cinematic already played this session.");
                return;
            }
            _alreadyPlayed = true;

            Debug.Log($"[Moon1PostRestorationVisuals] OnMoonCompleted (moon={args.moonIndex} " +
                      $"name='{args.moonName}' rs={args.rsReward} t={args.completionTime:F1}s) - " +
                      "starting 30s post-restoration transformation.");

            StartCoroutine(PlayCinematic());
        }
        IEnumerator PlayCinematic()
        {
            var fountain = FindHeroBuilding(FountainNames, "Fountain");
            var dome = FindHeroBuilding(DomeNames, "Star Dome");
            var spire = FindHeroBuilding(SpireNames, "Cathedral Spire");

            ParticleSystem fountainWater = null;
            AudioSource fountainAudio = null;
            if (fountain != null)
            {
                fountainWater = EnableChildParticleSystem(fountain, FountainWaterChild);
                fountainAudio = EnableChildAudioSource(fountain, FountainAudioChild);
            }

            GameObject starProjection = null;
            if (dome != null)
                starProjection = EnableChildGameObject(dome, StarProjectionChild);

            MeshRenderer spireRenderer = null;
            Material spireMaterial = null;
            if (spire != null)
            {
                spireRenderer = spire.GetComponentInChildren<MeshRenderer>();
                if (spireRenderer == null)
                {
                    Debug.LogError($"[Moon1PostRestorationVisuals] Spire '{spire.name}' " +
                                   $"(hierarchy '{GetHierarchyPath(spire)}') has no MeshRenderer in " +
                                   "any child - emission pulse cannot run.");
                }
                else
                {
                    spireMaterial = spireRenderer.material;
                    if (spireMaterial != null)
                    {
                        spireMaterial.EnableKeyword("_EMISSION");
                        spireRenderer.material = spireMaterial;
                    }
                    else
                    {
                        Debug.LogError($"[Moon1PostRestorationVisuals] Spire '{spire.name}' " +
                                       "MeshRenderer has no material - cannot pulse emission.");
                    }
                }
            }

            var goldenSystems = new List<ParticleSystem>();
            if (fountain != null) goldenSystems.Add(SpawnGoldenDrift(fountain.position, "Golden_Fountain"));
            if (dome != null) goldenSystems.Add(SpawnGoldenDrift(dome.position, "Golden_StarDome"));
            if (spire != null) goldenSystems.Add(SpawnGoldenDrift(spire.position, "Golden_Spire"));

            var sun = ResolveSunLight();
            if (sun != null)
            {
                sun.color = SunStartColor;
                sun.intensity = SunStartIntensity;
            }

            // Touch the cached refs so the compiler doesn't warn on unused locals.
            if (fountainWater != null || fountainAudio != null || starProjection != null) { }
            float t = 0f;
            while (t < CinematicDuration)
            {
                float dt = UnityEngine.Time.deltaTime;
                t += dt;
                float u = Mathf.Clamp01(t / CinematicDuration);

                if (spireRenderer != null && spireMaterial != null)
                {
                    float pulse = 1f + 0.3f * Mathf.Sin(t * 2f);
                    var emission = TelluricEmissionBase * pulse;
                    spireMaterial.SetColor("_EmissionColor", emission);
                }

                if (sun != null)
                {
                    sun.color = global::UnityEngine.Color.Lerp(SunStartColor, SunEndColor, u);
                    sun.intensity = Mathf.Lerp(SunStartIntensity, SunEndIntensity, u);
                }

                yield return null;
            }

            if (sun != null)
            {
                sun.color = SunEndColor;
                sun.intensity = SunEndIntensity;
            }
            if (spireRenderer != null && spireMaterial != null)
            {
                spireMaterial.SetColor("_EmissionColor", TelluricEmissionBase);
            }

            foreach (var ps in goldenSystems)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            Debug.Log($"[Moon1PostRestorationVisuals] Post-restoration cinematic complete (duration={CinematicDuration:F0}s).");
        }
        Transform FindHeroBuilding(string[] candidateNames, string humanLabel)
        {
            foreach (var n in candidateNames)
            {
                var go = GameObject.Find(n);
                if (go != null) return go.transform;
            }
            Debug.LogError($"[Moon1PostRestorationVisuals] {humanLabel} hero building not found in " +
                           "scene - tried names [" + string.Join(", ", candidateNames) + "]. " +
                           "Verify BuildingSpawner has run (BuildingSpawner.cs:90 spawns " +
                           "'Building_dome', 'Building_fountain', 'Building_spire').");
            return null;
        }

        ParticleSystem EnableChildParticleSystem(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                Debug.LogError($"[Moon1PostRestorationVisuals] Missing child '{childName}' under " +
                               $"'{parent.name}' (hierarchy '{GetHierarchyPath(parent)}'). " +
                               "Cannot enable fountain water ParticleSystem.");
                return null;
            }
            child.gameObject.SetActive(true);
            var ps = child.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogError($"[Moon1PostRestorationVisuals] Child '{childName}' under " +
                               $"'{GetHierarchyPath(parent)}' has no ParticleSystem component.");
                return null;
            }
            ps.Play(true);
            return ps;
        }

        AudioSource EnableChildAudioSource(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                Debug.LogError($"[Moon1PostRestorationVisuals] Missing child '{childName}' under " +
                               $"'{parent.name}' (hierarchy '{GetHierarchyPath(parent)}'). " +
                               "Cannot enable fountain audio.");
                return null;
            }
            child.gameObject.SetActive(true);
            var src = child.GetComponent<AudioSource>();
            if (src == null)
            {
                Debug.LogError($"[Moon1PostRestorationVisuals] Child '{childName}' under " +
                               $"'{GetHierarchyPath(parent)}' has no AudioSource component.");
                return null;
            }
            if (!src.isPlaying) src.Play();
            return src;
        }

        GameObject EnableChildGameObject(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                Debug.LogError($"[Moon1PostRestorationVisuals] Missing child '{childName}' under " +
                               $"'{parent.name}' (hierarchy '{GetHierarchyPath(parent)}'). " +
                               "Cannot enable Star Dome projection.");
                return null;
            }
            child.gameObject.SetActive(true);
            return child.gameObject;
        }
        ParticleSystem SpawnGoldenDrift(Vector3 worldPos, string label)
        {
            var go = new GameObject(label);
            go.transform.position = worldPos + new Vector3(0f, 0.5f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = CinematicDuration;
            main.loop = false;
            main.startLifetime = 8f;
            main.startSpeed = 0.8f;
            main.startSize = 0.35f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f;
            main.startColor = new global::UnityEngine.Color(1f, 0.85f, 0.35f, 1f);

            var em = ps.emission;
            em.rateOverTime = 14f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 1.2f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new global::UnityEngine.Color(1f, 0.95f, 0.55f), 0f),
                    new GradientColorKey(new global::UnityEngine.Color(1f, 0.78f, 0.30f), 0.55f),
                    new GradientColorKey(new global::UnityEngine.Color(0.9f, 0.55f, 0.15f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.15f),
                    new GradientAlphaKey(0.8f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0.4f));
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var velOverLife = ps.velocityOverLifetime;
            velOverLife.enabled = true;
            velOverLife.space = ParticleSystemSimulationSpace.World;
            velOverLife.y = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);

            ps.Play(true);
            return ps;
        }
        Light ResolveSunLight()
        {
            var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Light sun = null;
            float brightest = -1f;
            foreach (var l in lights)
            {
                if (l == null || l.type != LightType.Directional) continue;
                if (l.intensity > brightest)
                {
                    brightest = l.intensity;
                    sun = l;
                }
            }
            if (sun == null)
            {
                Debug.LogError("[Moon1PostRestorationVisuals] No directional Light found in scene - " +
                               "sun color/intensity ramp will be skipped. Moon1LightingSetup.cs should " +
                               "have placed one.");
            }
            return sun;
        }

        static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var sb = new System.Text.StringBuilder(t.name);
            var cur = t.parent;
            while (cur != null)
            {
                sb.Insert(0, cur.name + "/");
                cur = cur.parent;
            }
            return sb.ToString();
        }
    }
}