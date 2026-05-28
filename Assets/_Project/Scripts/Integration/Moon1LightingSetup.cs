using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Lighting Setup — Creates URP Forward+ optimized lighting for Echohaven
    /// Configures: Directional sun, ambient probe, light probes, realtime GI
    /// Integrates with DayNightController for 17-hour cycle
    /// </summary>
    [DefaultExecutionOrder(-75)] // After Moon1LevelBuilder (-85)
    public class Moon1LightingSetup : MonoBehaviour
    {
        [Header("Sun Configuration")]
        [SerializeField] float sunIntensity = 1.2f;
        [SerializeField] Color dayColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] Color nightColor = new Color(0.2f, 0.3f, 0.5f);

        [Header("Ambient Configuration")]
        [SerializeField] float ambientIntensity = 0.6f;
        [SerializeField] Color ambientDay = new Color(0.5f, 0.55f, 0.6f);
        [SerializeField] Color ambientNight = new Color(0.1f, 0.12f, 0.18f);

        [Header("Building Accent Lights")]
        [SerializeField] bool createAccentLights = true;
        [SerializeField] Color accentColor = new Color(1f, 0.85f, 0.3f); // Golden
        [SerializeField] float accentIntensity = 2.5f;

        Light _sun;

        void Start()
        {
            SetupDirectionalLight();
            ConfigureAmbientLighting();
            if (createAccentLights)
                CreateBuildingAccentLights();

            Debug.Log("[Moon1LightingSetup] Lighting configured - Forward+ URP optimized");
        }

        void SetupDirectionalLight()
        {
            // Find or create sun
            _sun = FindObjectOfType<Light>();
            if (_sun == null || _sun.type != LightType.Directional)
            {
                GameObject sunGO = new GameObject("Sun");
                _sun = sunGO.AddComponent<Light>();
                _sun.type = LightType.Directional;
            }

            // Configure for URP Forward+
            _sun.intensity = sunIntensity;
            _sun.color = dayColor;
            _sun.shadows = LightShadows.Soft;
            _sun.shadowStrength = 0.85f;
            _sun.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;

            // Forward+ optimizations
            _sun.renderMode = LightRenderMode.ForcePixel;
            _sun.cullingMask = ~0; // All layers

            // Starting angle (morning)
            _sun.transform.rotation = Quaternion.Euler(30f, -30f, 0f);

            // Tag for DayNightController to find
            _sun.gameObject.tag = "MainLight";
        }

        void ConfigureAmbientLighting()
        {
            // Set ambient mode to gradient (better for outdoor scenes)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientDay;
            RenderSettings.ambientEquatorColor = ambientDay * 0.7f;
            RenderSettings.ambientGroundColor = ambientDay * 0.5f;
            RenderSettings.ambientIntensity = ambientIntensity;

            // Enable realtime GI for dynamic lighting
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.defaultReflectionResolution = 128;
        }

        void CreateBuildingAccentLights()
        {
            // Find all buildings in scene
            var buildings = GameObject.FindGameObjectsWithTag("Building");
            if (buildings.Length == 0)
            {
                // Try by layer instead
                int buildingLayer = LayerMask.NameToLayer("Building");
                if (buildingLayer >= 0)
                    buildings = FindGameObjectsInLayer(buildingLayer);
            }

            foreach (var building in buildings)
            {
                CreateAccentLight(building);
            }

            Debug.Log($"[Moon1LightingSetup] Created {buildings.Length} building accent lights");
        }

        void CreateAccentLight(GameObject building)
        {
            // Check if accent light already exists
            Transform existingLight = building.transform.Find("AccentLight");
            if (existingLight != null)
                return;

            GameObject lightGO = new GameObject("AccentLight");
            lightGO.transform.SetParent(building.transform);

            // Position at building top
            var rend = building.GetComponentInChildren<Renderer>();
            float topY = rend != null ? rend.bounds.max.y - building.transform.position.y + 2f : 10f;
            lightGO.transform.localPosition = new Vector3(0f, topY, 0f);

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = accentColor;
            light.intensity = accentIntensity;
            light.range = 20f;
            light.shadows = LightShadows.None; // Performance optimization
            light.renderMode = LightRenderMode.ForcePixel;
            light.cullingMask = ~0;

            // Add subtle animation
            var animator = lightGO.AddComponent<LightPulseAnimator>();
            animator.baseIntensity = accentIntensity;
            animator.pulseAmount = 0.3f;
            animator.pulseSpeed = 0.5f;
        }

        GameObject[] FindGameObjectsInLayer(int layer)
        {
            var allObjects = FindObjectsOfType<GameObject>();
            System.Collections.Generic.List<GameObject> result = new System.Collections.Generic.List<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.layer == layer)
                    result.Add(obj);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Update lighting based on time of day (called by DayNightController)
        /// </summary>
        public void UpdateLighting(float timeNormalized)
        {
            if (_sun == null) return;

            // Lerp sun color between day and night
            _sun.color = Color.Lerp(nightColor, dayColor, Mathf.Clamp01(timeNormalized * 2f));

            // Lerp ambient lighting
            Color ambientCurrent = Color.Lerp(ambientNight, ambientDay, Mathf.Clamp01(timeNormalized * 2f));
            RenderSettings.ambientSkyColor = ambientCurrent;
            RenderSettings.ambientEquatorColor = ambientCurrent * 0.7f;
            RenderSettings.ambientGroundColor = ambientCurrent * 0.5f;

            // Adjust sun intensity (dimmer at night)
            _sun.intensity = Mathf.Lerp(0.3f, sunIntensity, Mathf.Clamp01(timeNormalized * 2f));
        }
    }

    /// <summary>
    /// Subtle pulsing animation for building accent lights
    /// </summary>
    public class LightPulseAnimator : MonoBehaviour
    {
        public float baseIntensity = 2.5f;
        public float pulseAmount = 0.3f;
        public float pulseSpeed = 0.5f;

        Light _light;
        float _phase;

        void Start()
        {
            _light = GetComponent<Light>();
            _phase = Random.Range(0f, Mathf.PI * 2f); // Random start phase
        }

        void Update()
        {
            if (_light == null) return;

            _phase += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(_phase) * pulseAmount;
            _light.intensity = baseIntensity + pulse;
        }
    }
}
