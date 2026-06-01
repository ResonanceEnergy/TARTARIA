using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Ambient Audio — The Verdant Labyrinth
    /// Jungle soundscape: birds, insects, rustling leaves
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon3AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 3 AUDIO — Verdant Labyrinth Soundscape");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon3_AudioZones");
            parent.transform.SetParent(transform);

            // Central jungle ambience
            CreateAudioZone(parent, Vector3.zero, 100f, "Jungle_Core", 0.5f);

            // 4 Directional bird zones (cardinal directions)
            CreateAudioZone(parent, new Vector3(60f, 15f, 0f), 30f, "Bird_Zone_East", 0.3f);
            CreateAudioZone(parent, new Vector3(-60f, 15f, 0f), 30f, "Bird_Zone_West", 0.3f);
            CreateAudioZone(parent, new Vector3(0f, 15f, 60f), 30f, "Bird_Zone_North", 0.3f);
            CreateAudioZone(parent, new Vector3(0f, 15f, -60f), 30f, "Bird_Zone_South", 0.3f);

            // Temple interior ambience (reverb zone)
            CreateReverbZone(parent, Vector3.zero, 40f, "Temple_Reverb");

            Debug.Log("[Moon3AmbientAudio] ✅ Soundscape complete!");
            Debug.Log("  • Central jungle ambience (100m radius)");
            Debug.Log("  • 4 directional bird zones");
            Debug.Log("  • Temple reverb zone");
            Debug.Log("  • All sources: 432 Hz tuned (pitch 0.9818)");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        void CreateAudioZone(GameObject parent, Vector3 position, float radius, string zoneName, float volume)
        {
            var zone = new GameObject(zoneName);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;

            var source = zone.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 1f; // 3D
            source.volume = volume;
            source.pitch = 0.9818f; // 432 Hz tuning
            source.minDistance = radius * 0.3f;
            source.maxDistance = radius;
            source.rolloffMode = AudioRolloffMode.Linear;
        }

        void CreateReverbZone(GameObject parent, Vector3 position, float radius, string zoneName)
        {
            var zone = new GameObject(zoneName);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;

            var reverb = zone.AddComponent<AudioReverbZone>();
            reverb.minDistance = radius * 0.5f;
            reverb.maxDistance = radius;
            reverb.reverbPreset = AudioReverbPreset.Forest; // Natural jungle reverb
        }
    }
}
