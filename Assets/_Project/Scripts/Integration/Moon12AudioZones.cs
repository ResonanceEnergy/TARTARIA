using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon12AudioZones : MonoBehaviour
    {
        [Header("Moon 12: Shadow Audio Zones")]
        [SerializeField] int silenceZoneCount = 5;
        [SerializeField] int umbralWhisperZoneCount = 6;
        [SerializeField] int voidResonanceZoneCount = 4;
        [SerializeField] int darknessHumZoneCount = 7;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Silence Zones - near-total audio suppression
            for (int i = 0; i < silenceZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"SilenceZone_{i}", pos, new Vector3(22f, 12f, 22f), "Silence", 1f);
            }

            // Umbral Whisper Zones - barely audible whispers
            for (int i = 0; i < umbralWhisperZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"UmbralWhisperZone_{i}", pos, new Vector3(18f, 10f, 18f), "UmbralWhisper", 0.3f);
            }

            // Void Resonance Zones - deep subsonic rumbles
            for (int i = 0; i < voidResonanceZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    2f,
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"VoidResonanceZone_{i}", pos, new Vector3(24f, 14f, 24f), "VoidResonance", 0.9f);
            }

            // Darkness Hum Zones - low frequency hum
            for (int i = 0; i < darknessHumZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"DarknessHumZone_{i}", pos, new Vector3(14f, 8f, 14f), "DarknessHum", 0.6f);
            }

            Debug.Log($"🎵 Moon12AudioZones spawned {zones.Count} audio zones");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon12AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon12AudioZoneTrigger>();
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

    public class Moon12AudioZoneTrigger : MonoBehaviour
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
