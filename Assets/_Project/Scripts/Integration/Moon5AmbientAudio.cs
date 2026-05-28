using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Ambient Audio — The Frostbound Citadel
    /// Frozen soundscape: howling wind, ice cracking, distant bells
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon5AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 5 AUDIO — Frostbound Citadel Soundscape");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon5_AudioZones");
            parent.transform.SetParent(transform);

            // Howling wind (perimeter)
            CreateAudioZone(parent, Vector3.zero, 120f, "Howling_Wind", 0.6f);

            // Ice cracking ambience (courtyard)
            CreateAudioZone(parent, Vector3.zero, 50f, "Ice_Cracking", 0.3f);

            // 4 Tower bell zones
            CreateAudioZone(parent, new Vector3(60f, 20f, 0f), 30f, "Tower_Bell_East", 0.25f);
            CreateAudioZone(parent, new Vector3(-60f, 20f, 0f), 30f, "Tower_Bell_West", 0.25f);
            CreateAudioZone(parent, new Vector3(0f, 20f, 60f), 30f, "Tower_Bell_North", 0.25f);
            CreateAudioZone(parent, new Vector3(0f, 20f, -60f), 30f, "Tower_Bell_South", 0.25f);

            // Interior keep reverb
            CreateReverbZone(parent, new Vector3(0f, 15f, 0f), 40f, "Keep_Reverb");

            Debug.Log("[Moon5AmbientAudio] ✅ Soundscape complete!");
            Debug.Log("  • Howling wind (120m radius)");
            Debug.Log("  • Ice cracking (50m radius)");
            Debug.Log("  • 4 tower bell zones");
            Debug.Log("  • Keep interior reverb");
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
            reverb.reverbPreset = AudioReverbPreset.Hallway; // Cold stone halls
        }
    }
}
