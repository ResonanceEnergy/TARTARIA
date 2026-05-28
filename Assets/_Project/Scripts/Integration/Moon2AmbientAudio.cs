using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Ambient Audio — Cave acoustics with echo effects
    /// Water drips, crystal resonance, distant rumbles, wind through tunnels
    /// 432 Hz tuning, spatial 3D audio, reverb zones
    /// </summary>
    [DefaultExecutionOrder(-83)]
    public class Moon2AmbientAudio : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] AudioClip caveAmbience;
        [SerializeField] AudioClip waterDrips;
        [SerializeField] AudioClip crystalResonance;
        [SerializeField] AudioClip distantRumble;
        [SerializeField] AudioClip windTunnel;

        [Header("Audio Settings")]
        [SerializeField] float masterVolume = 0.5f;
        [SerializeField] float spatialBlend = 1f; // Full 3D
        const float PITCH_432HZ = 0.9818f;

        void Start()
        {
            CreateAmbientAudio();
        }

        void CreateAmbientAudio()
        {
            Debug.Log("[Moon2AmbientAudio] Setting up cavern audio zones...");

            // Global cave ambience
            CreateAudioZone("CaveAmbience_Global", Vector3.zero, caveAmbience, 150f, 0.2f);

            // Entrance Chamber — water drips + echo
            CreateAudioZone("EntranceChamber_Drips", new Vector3(0f, 10f, -80f), waterDrips, 40f, 0.4f);

            // Echo Hall — wind tunnel effect
            CreateAudioZone("EchoHall_Wind", new Vector3(-50f, 6f, 0f), windTunnel, 60f, 0.5f);

            // Resonance Chamber — crystal resonance (main)
            CreateAudioZone("ResonanceChamber_Crystal", new Vector3(0f, 12f, 50f), crystalResonance, 50f, 0.7f);

            // Crystal Grotto — intense crystal humming
            CreateAudioZone("CrystalGrotto_Hum", new Vector3(60f, 9f, 20f), crystalResonance, 35f, 0.6f);

            // Harmonic Sanctum — deep resonance + rumble
            CreateAudioZone("HarmonicSanctum_Resonance", new Vector3(0f, 20f, 0f), crystalResonance, 70f, 0.8f);
            CreateAudioZone("HarmonicSanctum_Rumble", new Vector3(0f, 15f, 0f), distantRumble, 80f, 0.3f);

            Debug.Log("[Moon2AmbientAudio] ✅ 7 ambient audio zones created!");
        }

        void CreateAudioZone(string name, Vector3 position, AudioClip clip, float radius, float volume)
        {
            if (clip == null)
            {
                Debug.LogWarning($"  ✗ {name}: No audio clip assigned");
                return;
            }

            var audioObj = new GameObject(name);
            audioObj.transform.position = position;

            var audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume * masterVolume;
            audioSource.pitch = PITCH_432HZ; // 432 Hz tuning
            audioSource.maxDistance = radius;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            audioSource.playOnAwake = true;
            audioSource.Play();

            // Add reverb for cave acoustics
            var reverb = audioObj.AddComponent<AudioReverbZone>();
            reverb.minDistance = radius * 0.5f;
            reverb.maxDistance = radius;
            reverb.reverbPreset = AudioReverbPreset.Cave;

            Debug.Log($"  ✓ {name}: {radius}m radius, {volume} volume");
        }
    }
}
