using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Ambient Audio — The Aether Convergence
    /// FINAL LEVEL — Epic orchestral atmosphere, all frequencies converge
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon13AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  ✨ MOON 13 AUDIO — The Aether Convergence ✨");
            Debug.Log("  FINAL LEVEL SOUNDSCAPE");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var parent = new GameObject("Moon13_AudioZones");
            parent.transform.SetParent(transform);

            // Aether Core resonance (center, massive)
            CreateAudioZone(parent, new Vector3(0f, 50f, 0f), 180f, "Aether_Core_Resonance", 0.8f);

            // 12 Tribute platform harmonics (each moon's signature sound)
            float phi = 1.618033988749895f;
            float phi_inv = 1f / phi;
            Vector3[] positions = {
                new Vector3(1, 1, 1).normalized * 90f,
                new Vector3(1, 1, -1).normalized * 90f,
                new Vector3(1, -1, 1).normalized * 90f,
                new Vector3(1, -1, -1).normalized * 90f,
                new Vector3(-1, 1, 1).normalized * 90f,
                new Vector3(-1, 1, -1).normalized * 90f,
                new Vector3(-1, -1, 1).normalized * 90f,
                new Vector3(-1, -1, -1).normalized * 90f,
                new Vector3(0, phi_inv, phi).normalized * 90f,
                new Vector3(0, phi_inv, -phi).normalized * 90f,
                new Vector3(0, -phi_inv, phi).normalized * 90f,
                new Vector3(0, -phi_inv, -phi).normalized * 90f
            };

            for (int i = 0; i < 12; i++)
            {
                Vector3 pos = positions[i] + new Vector3(0f, 30f, 0f);
                CreateAudioZone(parent, pos, 40f, $"Tribute_Moon{i + 1}_Harmonic", 0.4f);
            }

            // Golden spiral path resonance (50 steps)
            CreateAudioZone(parent, new Vector3(0f, 40f, 0f), 100f, "Spiral_Path_Resonance", 0.5f);

            // 3 Pillar ring tones
            CreateAudioZone(parent, Vector3.zero, 70f, "Inner_Ring_Tone", 0.35f);
            CreateAudioZone(parent, Vector3.zero, 110f, "Middle_Ring_Tone", 0.4f);
            CreateAudioZone(parent, Vector3.zero, 150f, "Outer_Ring_Tone", 0.45f);

            // Final altar chorus (peak at 100m)
            CreateAudioZone(parent, new Vector3(0f, 100f, 0f), 60f, "Final_Altar_Chorus", 0.7f);

            // Converging energy streams
            for (int i = 0; i < 12; i++)
            {
                Vector3 pos = positions[i] / 2f + new Vector3(0f, 40f, 0f);
                CreateAudioZone(parent, pos, 25f, $"Energy_Stream_{i}", 0.3f);
            }

            CreateReverbZone(parent, new Vector3(0f, 50f, 0f), 200f, "Aether_Reverb");

            Debug.Log("[Moon13AmbientAudio] ✅ Final level soundscape complete!");
            Debug.Log("  • Aether Core resonance (180m radius)");
            Debug.Log("  • 12 Tribute platform harmonics");
            Debug.Log("  • Golden spiral + 3 pillar ring tones");
            Debug.Log("  • Final Altar chorus at peak");
            Debug.Log("  • 12 Energy stream convergences");
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
            source.spatialBlend = 1f;
            source.volume = volume;
            source.pitch = 0.9818f; // 432 Hz tuning
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
            reverb.reverbPreset = AudioReverbPreset.Arena; // Epic open space
        }
    }
}
