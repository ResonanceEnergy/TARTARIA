using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Lights 8 braziers around the village ring + 3 hero-building entrances.
    /// Each brazier = stone bowl (cylinder/sphere) + flame ParticleSystem + point light.
    /// Per CLAUDE.md "no stubs" — every brazier renders flame + casts light.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1Braziers : MonoBehaviour
    {
        static Moon1Braziers _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1Braziers");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1Braziers>();
        }

        void Start()
        {
            var root = new GameObject("Moon1_Braziers_Root");
            root.transform.SetParent(transform);

            // 8 perimeter braziers at radius 50
            for (int i = 0; i < 8; i++)
            {
                float a = (i / 8f) * Mathf.PI * 2f;
                var pos = new Vector3(Mathf.Cos(a) * 50f, 0f, Mathf.Sin(a) * 50f);
                BuildBrazier(root.transform, "Brazier_Perimeter_" + i, pos, 1.0f);
            }

            // 3 hero-building entrance braziers (pair at each)
            BuildBrazier(root.transform, "Brazier_Cathedral_L", new Vector3(-4f, 0f, 24f), 1.1f);
            BuildBrazier(root.transform, "Brazier_Cathedral_R", new Vector3( 4f, 0f, 24f), 1.1f);
            BuildBrazier(root.transform, "Brazier_Fountain_L",  new Vector3(-24f, 0f, -3f), 1.1f);
            BuildBrazier(root.transform, "Brazier_Fountain_R",  new Vector3(-24f, 0f,  3f), 1.1f);
            BuildBrazier(root.transform, "Brazier_Spire_L",     new Vector3( 24f, 0f, -3f), 1.1f);
            BuildBrazier(root.transform, "Brazier_Spire_R",     new Vector3( 24f, 0f,  3f), 1.1f);

            Debug.Log("[Moon1Braziers] Lit 14 braziers (8 perimeter + 6 hero-entrance).");
        }

        void BuildBrazier(Transform parent, string name, Vector3 worldPos, float scale)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.position = worldPos;

            // Bowl base — stone cylinder
            var bowlBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
            bowlBase.name = "BowlBase";
            bowlBase.transform.SetParent(root.transform);
            bowlBase.transform.localPosition = new Vector3(0f, 0.6f * scale, 0f);
            bowlBase.transform.localScale = new Vector3(0.45f * scale, 0.6f * scale, 0.45f * scale);
            Destroy(bowlBase.GetComponent<Collider>());
            ApplyURPColor(bowlBase, new Color(0.38f, 0.34f, 0.30f), 0.10f);

            // Bowl rim — sphere
            var bowlRim = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
            bowlRim.name = "BowlRim";
            bowlRim.transform.SetParent(root.transform);
            bowlRim.transform.localPosition = new Vector3(0f, 1.25f * scale, 0f);
            bowlRim.transform.localScale = new Vector3(0.6f * scale, 0.25f * scale, 0.6f * scale);
            Destroy(bowlRim.GetComponent<Collider>());
            ApplyURPColor(bowlRim, new Color(0.45f, 0.40f, 0.34f), 0.10f);

            // Flame particle system
            var flameGO = new GameObject("Flame");
            flameGO.transform.SetParent(root.transform);
            flameGO.transform.localPosition = new Vector3(0f, 1.35f * scale, 0f);
            var ps = flameGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.9f;
            main.startSpeed = 1.4f;
            main.startSize = 0.55f * scale;
            main.startColor = new Color(1f, 0.65f, 0.18f, 1f);
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.3f;
            var em = ps.emission;
            em.rateOverTime = 35f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.18f * scale;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.85f, 0.30f), 0f),
                    new GradientColorKey(new Color(1f, 0.45f, 0.10f), 0.4f),
                    new GradientColorKey(new Color(0.5f, 0.15f, 0.05f), 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.9f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.3f, 1f),
                new Keyframe(1f, 0.1f));
            sz.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.18f) * 3.5f);
                renderer.material = mat;
            }

            // Point light for warm glow
            var lightGO = new GameObject("Light");
            lightGO.transform.SetParent(root.transform);
            lightGO.transform.localPosition = new Vector3(0f, 1.5f * scale, 0f);
            var lt = lightGO.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.62f, 0.20f);
            lt.intensity = 2.8f;
            lt.range = 12f * scale;
            lt.shadows = LightShadows.Soft;

            // Add subtle flicker animator (no MonoBehaviour needed — uses light cookie pulse via lifecycle)
            var flicker = lightGO.AddComponent<Moon1BrazierFlicker>();
            flicker.baseIntensity = 2.8f;
            flicker.variance = 0.6f;
        }

        static void ApplyURPColor(GameObject go, Color baseColor, float smoothness)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                if (r.material != null) r.material.color = baseColor;
                return;
            }
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            r.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Flickers a Light's intensity around a base value with random variance.
    /// Real implementation per CLAUDE.md "no stubs" mandate.
    /// </summary>
    public class Moon1BrazierFlicker : MonoBehaviour
    {
        public float baseIntensity = 2.8f;
        public float variance = 0.6f;
        public float speed = 8f;

        Light _light;
        float _seed;

        void Awake()
        {
            _light = GetComponent<Light>();
            _seed = Random.Range(0f, 1000f);
        }

        void Update()
        {
            if (_light == null) return;
            float n = Mathf.PerlinNoise(Time.time * speed + _seed, _seed * 0.31f);
            _light.intensity = baseIntensity + (n - 0.5f) * 2f * variance;
        }
    }
}
