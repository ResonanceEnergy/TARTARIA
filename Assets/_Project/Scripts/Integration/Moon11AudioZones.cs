using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon11AudioZones : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Audio Zones")]
        [SerializeField] int spectrumResonanceZoneCount = 7;
        [SerializeField] int crystalChimeZoneCount = 6;
        [SerializeField] int colorShiftZoneCount = 5;
        [SerializeField] int harmonyZoneCount = 8;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Spectrum Resonance Zones - shifting tones across frequencies
            for (int i = 0; i < spectrumResonanceZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    3f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"SpectrumResonanceZone_{i}", pos, new Vector3(20f, 12f, 20f), "SpectrumResonance", 1f);
            }

            // Crystal Chime Zones - pure crystalline tones
            for (int i = 0; i < crystalChimeZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    4f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"CrystalChimeZone_{i}", pos, new Vector3(16f, 10f, 16f), "CrystalChime", 0.8f);
            }

            // Color Shift Zones - audio that shifts pitch like color
            for (int i = 0; i < colorShiftZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    2f,
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"ColorShiftZone_{i}", pos, new Vector3(18f, 10f, 18f), "ColorShift", 0.9f);
            }

            // Harmony Zones - peaceful multi-tone sounds
            for (int i = 0; i < harmonyZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"HarmonyZone_{i}", pos, new Vector3(14f, 8f, 14f), "Harmony", 0.7f);
            }

            Debug.Log($"🎵 Moon11AudioZones spawned {zones.Count} audio zones");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon11AudioZones.Moon11AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon11AudioZones.Moon11AudioZoneTrigger>();
            zoneTrigger.zoneType = zoneType;
            zoneTrigger.intensity = intensity;

            zones.Add(zone);
            return zone;
        }

        void OnDestroy()
        {
            foreach (GameObject zone in zones)
            {
                if (zone != null) Destroy(zone);
            }
            zones.Clear();
        }
    }

    public class Moon11AudioZoneTrigger : MonoBehaviour
    {
        public string zoneType;
        public float intensity;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"🎵 Player entered {zoneType} audio zone (intensity: {intensity})");
                // TODO: Wire to actual audio system - adjust AudioSource parameters, trigger zone-specific audio
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"🎵 Player exited {zoneType} audio zone");
                // TODO: Restore default audio parameters
            }
        }
    }
}
