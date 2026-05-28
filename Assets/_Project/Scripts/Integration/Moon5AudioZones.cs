using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon5AudioZones : MonoBehaviour
    {
        [Header("Moon 5: Ice Audio Zones")]
        [SerializeField] int blizzardZoneCount = 6;
        [SerializeField] int caveEchoZoneCount = 5;
        [SerializeField] int iceCrackZoneCount = 4;
        [SerializeField] int windHowlZoneCount = 7;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Blizzard Zones - harsh wind and snow sounds
            for (int i = 0; i < blizzardZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    4f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"BlizzardZone_{i}", pos, new Vector3(22f, 12f, 22f), "Blizzard", 1f);
            }

            // Cave Echo Zones - deep reverb
            for (int i = 0; i < caveEchoZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"CaveEchoZone_{i}", pos, new Vector3(18f, 10f, 18f), "CaveEcho", 0.8f);
            }

            // Ice Crack Zones - creaking ice sounds
            for (int i = 0; i < iceCrackZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    1f,
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"IceCrackZone_{i}", pos, new Vector3(16f, 8f, 16f), "IceCrack", 0.7f);
            }

            // Wind Howl Zones - eerie wind sounds
            for (int i = 0; i < windHowlZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    5f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"WindHowlZone_{i}", pos, new Vector3(14f, 10f, 14f), "WindHowl", 0.6f);
            }

            Debug.Log($"🎵 Moon5AudioZones spawned {zones.Count} audio zones");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon5AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon5AudioZoneTrigger>();
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

    public class Moon5AudioZoneTrigger : MonoBehaviour
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
