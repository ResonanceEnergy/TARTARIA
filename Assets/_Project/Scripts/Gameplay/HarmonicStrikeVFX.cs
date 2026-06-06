using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Harmonic Strike VFX — sphere shockwave effect for AoE ability.
    /// Spawns at player position, expands outward with particle ring and mesh scaling.
    /// Auto-destroys after animation completes.
    /// </summary>
    public class HarmonicStrikeVFX : MonoBehaviour
    {
        [Header("Shockwave Settings")]
        [SerializeField] private float expansionDuration = 0.6f;
        [SerializeField] private float maxRadius = 5f;
        [SerializeField] private AnimationCurve expansionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private Gradient colorGradient;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer shockwaveRenderer;
        [SerializeField] private ParticleSystem particleRing;
        [SerializeField] private Light pulseLight;

        private float _elapsedTime;
        private Material _shockwaveMaterial;
        private Color _startColor = new Color(0.9f, 0.3f, 0.3f, 0.8f);
        private Color _endColor = new Color(0.9f, 0.3f, 0.3f, 0f);

        void Awake()
        {
            // Create sphere mesh if not assigned
            if (shockwaveRenderer == null)
            {
                var sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphereGO.GetComponent<SphereCollider>()); // Remove collider
                sphereGO.transform.SetParent(transform, false);
                sphereGO.transform.localPosition = Vector3.zero;
                sphereGO.transform.localScale = Vector3.zero;
                shockwaveRenderer = sphereGO.GetComponent<MeshRenderer>();
            }

            // Create material with transparency
            _shockwaveMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _shockwaveMaterial.SetFloat("_Surface", 1); // Transparent
            _shockwaveMaterial.SetFloat("_Blend", 0); // Alpha blend
            _shockwaveMaterial.SetColor("_BaseColor", _startColor);
            _shockwaveMaterial.SetFloat("_Smoothness", 0.8f);
            _shockwaveMaterial.SetFloat("_Metallic", 0.2f);
            if (shockwaveRenderer != null)
                shockwaveRenderer.material = _shockwaveMaterial;

            // Initialize gradient if null
            if (colorGradient == null || colorGradient.colorKeys.Length == 0)
            {
                colorGradient = new Gradient();
                colorGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(_startColor, 0f), new GradientColorKey(_endColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
                );
            }

            // Spawn particles
            if (particleRing != null)
            {
                particleRing.Play();
            }

            // Pulse light
            if (pulseLight == null)
            {
                var lightGO = new GameObject("PulseLight");
                lightGO.transform.SetParent(transform, false);
                pulseLight = lightGO.AddComponent<Light>();
                pulseLight.type = LightType.Point;
                pulseLight.range = maxRadius * 2f;
                pulseLight.intensity = 2f;
                pulseLight.color = new Color(0.9f, 0.4f, 0.3f);
            }
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / expansionDuration);
            float curveValue = expansionCurve.Evaluate(t);

            // Scale sphere outward
            float currentRadius = maxRadius * curveValue;
            if (shockwaveRenderer != null)
                shockwaveRenderer.transform.localScale = Vector3.one * currentRadius * 2f; // Diameter

            // Fade color
            Color currentColor = colorGradient.Evaluate(t);
            if (_shockwaveMaterial != null)
                _shockwaveMaterial.SetColor("_BaseColor", currentColor);

            // Fade light
            if (pulseLight != null)
            {
                pulseLight.intensity = Mathf.Lerp(2f, 0f, t);
            }

            // Destroy when complete
            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_shockwaveMaterial != null)
                Destroy(_shockwaveMaterial);
        }

        /// <summary>
        /// Spawn a Harmonic Strike shockwave VFX at the specified position.
        /// </summary>
        public static void Spawn(Vector3 position, float radius = 5f)
        {
            var vfxGO = new GameObject("HarmonicStrikeVFX");
            vfxGO.transform.position = position;
            var vfx = vfxGO.AddComponent<HarmonicStrikeVFX>();
            vfx.maxRadius = radius;

            Debug.Log($"[HarmonicStrikeVFX] Spawned at {position}, radius {radius}");
        }
    }
}
