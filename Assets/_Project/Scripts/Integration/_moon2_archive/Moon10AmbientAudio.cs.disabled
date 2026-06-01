using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Ambient Audio — The Temporal Rift
    /// Time soundscape: temporal distortion, echoes across time
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon10AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon10_AudioZones");
            parent.transform.SetParent(transform);

            // Time vortex hum (center)
            CreateAudioZone(parent, new Vector3(0f, 15f, 0f), 90f, "Time_Vortex_Hum", 0.7f);

            // 3 Time layer ambiences
            CreateAudioZone(parent, Vector3.zero, 40f, "Past_Echoes", 0.3f);
            CreateAudioZone(parent, new Vector3(0f, 5f, 0f), 60f, "Present_Flow", 0.4f);
            CreateAudioZone(parent, new Vector3(0f, 10f, 0f), 80f, "Future_Resonance", 0.35f);

            // 8 Temporal anchor tones
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(55f, 8f, 0f);
                CreateAudioZone(parent, pos, 20f, $"Anchor_Tone_{i}", 0.25f);
            }

            CreateReverbZone(parent, new Vector3(0f, 5f, 0f), 100f, "Temporal_Reverb");

            Debug.Log("[Moon10AmbientAudio] ✅ Temporal soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Hallway; // Echoing time corridor
        }
    }
}
