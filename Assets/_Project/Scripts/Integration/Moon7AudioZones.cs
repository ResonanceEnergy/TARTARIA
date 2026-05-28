using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon7AudioZones : MonoBehaviour
    {
        [Header("Moon 7: Underwater Audio Zones")]
        [SerializeField] int deepWaterZoneCount = 6;
        [SerializeField] int whirlpoolZoneCount = 4;
        [SerializeField] int coralZoneCount = 5;
        [SerializeField] int currentZoneCount = 7;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Deep Water Zones - muffled underwater sounds
            for (int i = 0; i < deepWaterZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(0f, 8f),
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"DeepWaterZone_{i}", pos, new Vector3(26f, 16f, 26f), "DeepWater", 1f);
            }

            // Whirlpool Zones - swirling water sounds
            for (int i = 0; i < whirlpoolZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(2f, 10f),
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"WhirlpoolZone_{i}", pos, new Vector3(18f, 12f, 18f), "Whirlpool", 0.9f);
            }

            // Coral Zones - subtle aquatic life sounds
            for (int i = 0; i < coralZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    Random.Range(0f, 6f),
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"CoralZone_{i}", pos, new Vector3(16f, 10f, 16f), "Coral", 0.6f);
            }

            // Current Zones - flowing water sounds
            for (int i = 0; i < currentZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(1f, 9f),
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"CurrentZone_{i}", pos, new Vector3(14f, 8f, 14f), "Current", 0.7f);
            }

            Debug.Log($"🎵 Moon7AudioZones spawned {zones.Count} audio zones");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon7AudioZones.AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon7AudioZones.AudioZoneTrigger>();
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

    public class AudioZoneTrigger : MonoBehaviour
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
