using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Ambient Audio — The Abyssal Depths
    /// Underwater soundscape: muffled ambience, whale songs, pressure
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon7AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon7_AudioZones");
            parent.transform.SetParent(transform);

            // Underwater ambience (wide area)
            CreateAudioZone(parent, Vector3.zero, 120f, "Underwater_Ambience", 0.5f);

            // 5 Pressure chamber sounds
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(0f, -40f - (i * 15f), 0f);
                CreateAudioZone(parent, pos, 30f, $"Pressure_Chamber_{i + 1}", 0.4f);
            }

            // Distant whale songs (3)
            CreateAudioZone(parent, new Vector3(60f, -30f, 60f), 80f, "Whale_Song_1", 0.2f);
            CreateAudioZone(parent, new Vector3(-60f, -40f, -60f), 80f, "Whale_Song_2", 0.2f);
            CreateAudioZone(parent, new Vector3(0f, -60f, 0f), 80f, "Whale_Song_3", 0.2f);

            CreateReverbZone(parent, new Vector3(0f, -20f, 0f), 100f, "Underwater_Reverb");

            Debug.Log("[Moon7AmbientAudio] ✅ Underwater soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Underwater; // Muffled reverb
        }
    }
}
