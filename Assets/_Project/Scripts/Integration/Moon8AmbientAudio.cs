using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Ambient Audio — The Celestial Spires
    /// Sky soundscape: wind currents, distant bells, bird calls
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon8AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon8_AudioZones");
            parent.transform.SetParent(transform);

            // High altitude wind (wide area)
            CreateAudioZone(parent, new Vector3(0f, 100f, 0f), 180f, "Sky_Wind", 0.5f);

            // 6 Temple bell zones
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(120f, 105f, 0f);
                CreateAudioZone(parent, offset, 30f, $"Temple_Bells_{i}", 0.3f);
            }

            // Wind bridge sounds (6)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 start = Quaternion.Euler(0f, angle, 0f) * new Vector3(20f, 100f, 0f);
                CreateAudioZone(parent, start, 20f, $"Wind_Bridge_{i}", 0.25f);
            }

            CreateReverbZone(parent, new Vector3(0f, 100f, 0f), 150f, "Sky_Reverb");

            Debug.Log("[Moon8AmbientAudio] ✅ Sky soundscape complete!");
        }

        void CreateAudioZone(GameObject parent, Vector3 position, float radius, string zoneName, float volume)
        {
            var zone = new GameObject(zoneName);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;

            var source = zone.AddComponent<AudioSource>();
            source.loop = true;
            source.spatialBlend = 1f;
            source.volume = volume;
            source.pitch = 0.9818f; // 432 Hz
            source.minDistance = radius * 0.3f;
            source.maxDistance = radius;
        }

        void CreateReverbZone(GameObject parent, Vector3 position, float radius, string zoneName)
        {
            var zone = new GameObject(zoneName);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;
            var reverb = zone.AddComponent<AudioReverbZone>();
            reverb.minDistance = radius * 0.5f;
            reverb.maxDistance = radius;
            reverb.reverbPreset = AudioReverbPreset.Mountains; // Open sky
        }
    }
}
