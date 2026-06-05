using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Frequency Shield VFX — defensive overlay effect for player shield ability.
    /// Renders a semi-transparent sphere around the player with pulsing energy effect.
    /// Attaches to player transform and auto-destroys when shield expires.
    /// </summary>
    public class FrequencyShieldVFX : MonoBehaviour
    {
        [Header("Shield Settings")]
        [SerializeField] private float shieldRadius = 1.5f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        [SerializeField] private Color shieldColor = new Color(0.3f, 0.6f, 0.9f, 0.4f);

        [Header("Visuals")]
        [SerializeField] private MeshRenderer shieldRenderer;
        [SerializeField] private ParticleSystem energyParticles;
        [SerializeField] private Light shieldLight;

        private Material _shieldMaterial;
        private float _pulseTimer;
        private Transform _playerTransform;
        private float _lifetime;
        private float _maxLifetime = 5f; // Default shield duration

        void Awake()
        {
            // Create shield sphere mesh
            if (shieldRenderer == null)
            {
                var sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphereGO.GetComponent<SphereCollider>()); // Remove collider
                sphereGO.transform.SetParent(transform, false);
                sphereGO.transform.localPosition = Vector3.zero;
                sphereGO.transform.localScale = Vector3.one * shieldRadius * 2f;
                shieldRenderer = sphereGO.GetComponent<MeshRenderer>();
            }

            // Create translucent material
            _shieldMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _shieldMaterial.SetFloat("_Surface", 1); // Transparent
            _shieldMaterial.SetFloat("_Blend", 0); // Alpha blend
            _shieldMaterial.SetColor("_BaseColor", shieldColor);
            _shieldMaterial.SetFloat("_Smoothness", 0.9f);
            _shieldMaterial.SetFloat("_Metallic", 0.5f);
            _shieldMaterial.EnableKeyword("_EMISSION");
            _shieldMaterial.SetColor("_EmissionColor", shieldColor * 0.5f);
            if (shieldRenderer != null)
                shieldRenderer.material = _shieldMaterial;

            // Create energy particles
            if (energyParticles == null)
            {
                var particlesGO = new GameObject("EnergyParticles");
                particlesGO.transform.SetParent(transform, false);
                energyParticles = particlesGO.AddComponent<ParticleSystem>();
                var main = energyParticles.main;
                main.startLifetime = 1.5f;
                main.startSpeed = 0.5f;
                main.startSize = 0.1f;
                main.startColor = shieldColor;
                main.maxParticles = 50;
                var emission = energyParticles.emission;
                emission.rateOverTime = 30f;
                var shape = energyParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = shieldRadius;
                energyParticles.Play();
            }

            // Create point light
            if (shieldLight == null)
            {
                var lightGO = new GameObject("ShieldLight");
                lightGO.transform.SetParent(transform, false);
                shieldLight = lightGO.AddComponent<Light>();
                shieldLight.type = LightType.Point;
                shieldLight.range = shieldRadius * 3f;
                shieldLight.intensity = 1f;
                shieldLight.color = new Color(0.3f, 0.6f, 0.9f);
            }
        }

        void Update()
        {
            // Pulse effect
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(_pulseTimer) * pulseIntensity + 1f;

            // Scale pulsing
            if (shieldRenderer != null)
            {
                shieldRenderer.transform.localScale = Vector3.one * shieldRadius * 2f * pulse;
            }

            // Emission pulsing
            if (_shieldMaterial != null)
            {
                Color emissionColor = shieldColor * pulse * 0.5f;
                _shieldMaterial.SetColor("_EmissionColor", emissionColor);
            }

            // Light pulsing
            if (shieldLight != null)
            {
                shieldLight.intensity = pulse * 1.2f;
            }

            // Follow player if attached
            if (_playerTransform != null)
            {
                transform.position = _playerTransform.position;
            }

            // Lifetime tracking
            _lifetime += Time.deltaTime;
            if (_lifetime >= _maxLifetime)
            {
                FadeOut();
            }
        }

        void FadeOut()
        {
            // Quick fade and destroy
            if (_shieldMaterial != null)
            {
                Color fadedColor = shieldColor;
                fadedColor.a = 0f;
                _shieldMaterial.SetColor("_BaseColor", fadedColor);
            }
            Destroy(gameObject, 0.2f);
        }

        void OnDestroy()
        {
            if (_shieldMaterial != null)
                Destroy(_shieldMaterial);
        }

        /// <summary>
        /// Attach this shield VFX to a player transform.
        /// </summary>
        public void AttachToPlayer(Transform playerTransform, float duration = 5f)
        {
            _playerTransform = playerTransform;
            _maxLifetime = duration;
            transform.position = playerTransform.position;
        }

        /// <summary>
        /// Spawn a Frequency Shield VFX attached to the specified transform.
        /// </summary>
        public static FrequencyShieldVFX Spawn(Transform playerTransform, float radius = 1.5f, float duration = 5f)
        {
            var vfxGO = new GameObject("FrequencyShieldVFX");
            var vfx = vfxGO.AddComponent<FrequencyShieldVFX>();
            vfx.shieldRadius = radius;
            vfx.AttachToPlayer(playerTransform, duration);

            Debug.Log($"[FrequencyShieldVFX] Spawned shield for {playerTransform.name}, duration {duration}s");
            return vfx;
        }
    }
}
