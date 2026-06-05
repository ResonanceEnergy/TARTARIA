using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Ambient Audio — The Umbral Sanctum
    /// Shadow soundscape: void whispers, dark resonance, silence
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon12AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon12_AudioZones");
            parent.transform.SetParent(transform);

            // Void whispers (center)
            CreateAudioZone(parent, new Vector3(0f, 15f, 0f), 100f, "Void_Whispers", 0.4f);

            // 6 Spire shadow tones
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 12f, 0f);
                CreateAudioZone(parent, pos, 25f, $"Spire_Shadow_{i}", 0.25f);
            }

            // 6 Void bridge resonances
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(60f, 20f, 0f);
                CreateAudioZone(parent, pos, 20f, $"Bridge_Resonance_{i}", 0.2f);
            }

            // 12 Obelisk void hums
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(30f, 50f), 10f, 0f);
                CreateAudioZone(parent, pos, 15f, $"Obelisk_Hum_{i}", 0.15f);
            }

            CreateReverbZone(parent, Vector3.zero, 90f, "Void_Reverb");

            Debug.Log("[Moon12AmbientAudio] ✅ Shadow soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Cave; // Deep void echo
        }
    }
}
