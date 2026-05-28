using UnityEngine;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Ambient Audio — Creates spatial audio zones for Echohaven
    /// Zones: Wind/nature (forest), crystal hum (spire), water (fountain), echoes (cathedral)
    /// Uses 432 Hz tuning for all ambient sounds
    /// </summary>
    [DefaultExecutionOrder(-80)] // After quest triggers (-81)
    public class Moon1AmbientAudio : MonoBehaviour
    {
        [Header("Audio Zones")]
        [SerializeField] Vector3 cathedralCenter = new Vector3(0f, 0f, 80f);
        [SerializeField] Vector3 fountainCenter = new Vector3(-60f, 0f, 40f);
        [SerializeField] Vector3 spireCenter = new Vector3(60f, 0f, 40f);

        [Header("Audio Settings")]
        [SerializeField] float zoneRadius = 25f;
        [SerializeField] float maxVolume = 0.6f;
        [SerializeField] float fadeDistance = 10f;

        [Header("Audio Clips (432 Hz)")]
        [SerializeField] AudioClip windAmbience;
        [SerializeField] AudioClip crystalHum;
        [SerializeField] AudioClip waterFlow;
        [SerializeField] AudioClip cathedralReverb;

        void Start()
        {
            CreateAmbientAudioZones();
        }

        void CreateAmbientAudioZones()
        {
            Debug.Log("[Moon1AmbientAudio] Creating ambient audio zones...");

            var audioParent = new GameObject("Ambient_Audio");
            audioParent.transform.position = Vector3.zero;

            // Village wind ambience (general background)
            CreateAudioZone(audioParent, "Village_Wind", Vector3.zero, 100f, windAmbience, 0.3f);

            // Cathedral reverb zone
            CreateAudioZone(audioParent, "Cathedral_Ambience", cathedralCenter, zoneRadius, cathedralReverb, maxVolume);

            // Fountain water zone
            CreateAudioZone(audioParent, "Fountain_Water", fountainCenter, zoneRadius, waterFlow, maxVolume);

            // Spire crystal hum zone
            CreateAudioZone(audioParent, "Spire_Crystal_Hum", spireCenter, zoneRadius, crystalHum, maxVolume);

            Debug.Log("[Moon1AmbientAudio] ✅ 4 ambient audio zones created");
        }

        void CreateAudioZone(GameObject parent, string name, Vector3 position, float radius, AudioClip clip, float volume)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;

            // Add AudioSource
            var audioSource = zone.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.spatialBlend = 1f; // Full 3D
            audioSource.volume = volume;
            audioSource.minDistance = radius * 0.5f;
            audioSource.maxDistance = radius * 2f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            // 432 Hz tuning (adjust pitch if needed)
            // Standard pitch is 440 Hz, so: 432/440 = 0.9818
            audioSource.pitch = 0.9818f;

            // Add trigger zone for debugging
            var collider = zone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = radius;

            // Start playing
            if (clip != null)
            {
                audioSource.Play();
                Debug.Log($"  ✓ {name} at {position} (radius: {radius}m, volume: {volume})");
            }
            else
            {
                Debug.LogWarning($"  ✗ {name} missing audio clip!");
            }
        }
    }
}
