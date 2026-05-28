using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Lighting Setup — Dark cavern atmosphere with crystal light sources
    /// Contrast to Moon 1: No sun, bioluminescent + crystal lighting, deep shadows
    /// Dynamic lighting: pulsing crystals, flickering torches, resonance effects
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon2LightingSetup : MonoBehaviour
    {
        [Header("Ambient Lighting")]
        [SerializeField] Color ambientColor = new Color(0.05f, 0.08f, 0.12f); // Very dark blue
        [SerializeField] float ambientIntensity = 0.3f;

        [Header("Crystal Lighting")]
        [SerializeField] int crystalLightCount = 50;
        [SerializeField] float crystalPulseSpeed = 0.5f;
        [SerializeField] float crystalPulseAmount = 0.3f;

        [Header("Chamber Accent Lights")]
        [SerializeField] Color entranceAccentColor = new Color(0.8f, 0.9f, 1f); // Cool blue
        [SerializeField] Color resonanceAccentColor = new Color(0.6f, 0.8f, 1f); // Bright cyan
        [SerializeField] Color harmonicAccentColor = new Color(0.9f, 0.6f, 1f); // Purple

        [Header("Fog Settings")]
        [SerializeField] bool enableFog = true;
        [SerializeField] Color fogColor = new Color(0.08f, 0.12f, 0.18f);
        [SerializeField] float fogDensity = 0.015f;

        private Light[] crystalLights;

        void Start()
        {
            SetupLighting();
        }

        void SetupLighting()
        {
            Debug.Log("[Moon2LightingSetup] Configuring cavern lighting...");

            ConfigureAmbient();
            DisableSun();
            CreateCrystalLights();
            CreateChamberLights();
            ConfigureFog();

            Debug.Log("[Moon2LightingSetup] ✅ Cavern lighting complete!");
        }

        void ConfigureAmbient()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;

            Debug.Log($"  ✓ Ambient: {ambientColor} @ {ambientIntensity}");
        }

        void DisableSun()
        {
            // Disable sun (underground level)
            var sun = GameObject.Find("Directional Light");
            if (sun != null)
            {
                sun.SetActive(false);
                Debug.Log("  ✓ Sun disabled (underground)");
            }
        }

        void CreateCrystalLights()
        {
            var lightParent = new GameObject("Moon2_CrystalLights");
            var crystalLightsList = new System.Collections.Generic.List<Light>();

            // Scatter crystal lights throughout cavern
            for (int i = 0; i < crystalLightCount; i++)
            {
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(20f, 100f);
                float height = Random.Range(2f, 40f);

                Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, height, 0f);

                var lightObj = new GameObject($"CrystalLight_{i:D2}");
                lightObj.transform.SetParent(lightParent.transform);
                lightObj.transform.position = position;

                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(
                    Random.Range(0.5f, 0.8f),
                    Random.Range(0.7f, 1f),
                    Random.Range(0.8f, 1f)
                ); // Cool blue-cyan spectrum
                light.intensity = Random.Range(1.5f, 3f);
                light.range = Random.Range(15f, 30f);
                light.shadows = LightShadows.Soft;

                // Add pulsing component
                var pulser = lightObj.AddComponent<CrystalLightPulse>();
                pulser.baseIntensity = light.intensity;
                pulser.pulseSpeed = crystalPulseSpeed + Random.Range(-0.2f, 0.2f);
                pulser.pulseAmount = crystalPulseAmount;

                crystalLightsList.Add(light);
            }

            crystalLights = crystalLightsList.ToArray();
            Debug.Log($"  ✓ {crystalLightCount} pulsing crystal lights");
        }

        void CreateChamberLights()
        {
            // Entrance Chamber
            CreateAccentLight("EntranceChamber_Light", new Vector3(0f, 15f, -80f), entranceAccentColor, 8f, 50f);

            // Echo Hall
            CreateAccentLight("EchoHall_Light", new Vector3(-50f, 8f, 0f), entranceAccentColor, 6f, 40f);

            // Resonance Chamber (brightest)
            CreateAccentLight("ResonanceChamber_Light", new Vector3(0f, 20f, 50f), resonanceAccentColor, 12f, 60f);

            // Crystal Grotto
            CreateAccentLight("CrystalGrotto_Light", new Vector3(60f, 12f, 20f), resonanceAccentColor, 10f, 45f);

            // Harmonic Sanctum (mystical purple)
            CreateAccentLight("HarmonicSanctum_Light", new Vector3(0f, 15f, 0f), harmonicAccentColor, 15f, 70f);

            Debug.Log("  ✓ 5 chamber accent lights");
        }

        void CreateAccentLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObj = new GameObject(name);
            lightObj.transform.position = position;

            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;

            // Add very slow pulse
            var pulser = lightObj.AddComponent<CrystalLightPulse>();
            pulser.baseIntensity = intensity;
            pulser.pulseSpeed = 0.2f;
            pulser.pulseAmount = 0.15f;
        }

        void ConfigureFog()
        {
            if (!enableFog) return;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;

            Debug.Log($"  ✓ Fog: {fogColor}, density={fogDensity}");
        }
    }

    /// <summary>
    /// Crystal Light Pulse — Animates crystal light intensity
    /// </summary>
    public class CrystalLightPulse : MonoBehaviour
    {
        public float baseIntensity = 2f;
        public float pulseSpeed = 0.5f;
        public float pulseAmount = 0.3f;

        private Light lightComponent;
        private float pulseOffset;

        void Start()
        {
            lightComponent = GetComponent<Light>();
            pulseOffset = Random.Range(0f, Mathf.PI * 2f); // Randomize phase
        }

        void Update()
        {
            if (lightComponent == null) return;

            float pulse = Mathf.Sin((Time.time * pulseSpeed) + pulseOffset) * pulseAmount;
            lightComponent.intensity = baseIntensity + (baseIntensity * pulse);
        }
    }
}
