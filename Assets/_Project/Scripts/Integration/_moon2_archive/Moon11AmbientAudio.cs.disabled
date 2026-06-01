using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Ambient Audio — The Prismatic Nexus
    /// Crystal soundscape: chimes, harmonic resonance, light tones
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon11AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon11_AudioZones");
            parent.transform.SetParent(transform);

            // Central crystal resonance
            CreateAudioZone(parent, new Vector3(0f, 20f, 0f), 100f, "Crystal_Resonance", 0.6f);

            // 7 Color chamber harmonics (each a different note)
            for (int i = 0; i < 7; i++)
            {
                float angle = i * 51.43f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(45f, 10f, 0f);
                CreateAudioZone(parent, pos, 25f, $"Chamber_Harmonic_{i}", 0.35f);
            }

            // 12 Refractor chimes
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(30f, 15f, 0f);
                CreateAudioZone(parent, pos, 12f, $"Refractor_Chime_{i}", 0.25f);
            }

            CreateReverbZone(parent, Vector3.zero, 90f, "Crystal_Reverb");

            Debug.Log("[Moon11AmbientAudio] ✅ Prismatic soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Arena; // Open crystalline space
        }
    }
}
