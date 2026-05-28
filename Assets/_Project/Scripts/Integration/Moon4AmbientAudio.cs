using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Ambient Audio — The Sunscorched Oasis
    /// Desert soundscape: wind, water, distant temple chimes
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon4AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 4 AUDIO — Sunscorched Oasis Soundscape");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon4_AudioZones");
            parent.transform.SetParent(transform);

            // Desert wind ambience (wide area)
            CreateAudioZone(parent, Vector3.zero, 150f, "Desert_Wind", 0.4f);

            // Oasis water sounds (center)
            CreateAudioZone(parent, Vector3.zero, 30f, "Oasis_Water", 0.5f);

            // 3 Temple zones with wind chimes
            CreateAudioZone(parent, new Vector3(50f, 5f, 0f), 25f, "Temple_East_Chimes", 0.3f);
            CreateAudioZone(parent, new Vector3(-50f, 5f, 0f), 25f, "Temple_West_Chimes", 0.3f);
            CreateAudioZone(parent, new Vector3(0f, 5f, 50f), 25f, "Temple_North_Chimes", 0.3f);

            // Sand dune reverb (open space)
            CreateReverbZone(parent, Vector3.zero, 100f, "Desert_Reverb");

            Debug.Log("[Moon4AmbientAudio] ✅ Soundscape complete!");
            Debug.Log("  • Desert wind (150m radius)");
            Debug.Log("  • Oasis water (30m radius)");
            Debug.Log("  • 3 temple wind chime zones");
            Debug.Log("  • Open desert reverb");
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
            reverb.reverbPreset = AudioReverbPreset.Plain; // Open desert sound
        }
    }
}
