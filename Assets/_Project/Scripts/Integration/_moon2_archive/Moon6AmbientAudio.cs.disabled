using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Ambient Audio — The Molten Forge
    /// Volcanic soundscape: lava bubbling, forge hammers, rumbling
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon6AmbientAudio : MonoBehaviour
    {
        void Start()
        {
            SetupAudio();
        }

        void SetupAudio()
        {
            var parent = new GameObject("Moon6_AudioZones");
            parent.transform.SetParent(transform);

            // Forge ambience (center)
            CreateAudioZone(parent, Vector3.zero, 80f, "Forge_Ambience", 0.6f);

            // 8 Lava pool sounds
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(35f, 0.5f, 0f);
                CreateAudioZone(parent, pos, 15f, $"Lava_Pool_{i}", 0.4f);
            }

            // Hammer strikes (4 anvils)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(20f, 2f, 0f);
                CreateAudioZone(parent, pos, 12f, $"Anvil_Hammer_{i}", 0.35f);
            }

            CreateReverbZone(parent, Vector3.zero, 60f, "Forge_Reverb");

            Debug.Log("[Moon6AmbientAudio] ✅ Volcanic soundscape complete!");
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
            reverb.reverbPreset = AudioReverbPreset.Hangar; // Large metallic space
        }
    }
}
