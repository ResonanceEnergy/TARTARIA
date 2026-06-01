using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 Ambient Audio — The Blighted Wastes
    /// Corrupted soundscape: whispers, dark energy hum, distortion
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon9AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon9_AudioZones");
            parent.transform.SetParent(transform);

            // Corruption hum (center)
            CreateAudioZone(parent, Vector3.zero, 100f, "Corruption_Hum", 0.6f);

            // 5 Spire whispers
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(50f, 10f, 0f);
                CreateAudioZone(parent, pos, 25f, $"Spire_Whispers_{i}", 0.35f);
            }

            // Dark energy zones (12 monoliths)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(30f, 70f), 8f, 0f);
                CreateAudioZone(parent, pos, 15f, $"Dark_Energy_{i}", 0.25f);
            }

            CreateReverbZone(parent, Vector3.zero, 80f, "Wasteland_Reverb");

            Debug.Log("[Moon9AmbientAudio] ✅ Corrupted soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Cave; // Hollow corrupted sound
        }
    }
}
