using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon6AudioZones : MonoBehaviour
    {
        [Header("Moon 6: Lava Audio Zones")]
        [SerializeField] int moltenRumbleZoneCount = 6;
        [SerializeField] int forgeZoneCount = 4;
        [SerializeField] int lavaFlowZoneCount = 5;
        [SerializeField] int emberCrackleZoneCount = 8;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Molten Rumble Zones - deep volcanic rumbling
            for (int i = 0; i < moltenRumbleZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"MoltenRumbleZone_{i}", pos, new Vector3(24f, 14f, 24f), "MoltenRumble", 1f);
            }

            // Forge Zones - metallic hammering and flames
            for (int i = 0; i < forgeZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    3f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"ForgeZone_{i}", pos, new Vector3(16f, 10f, 16f), "Forge", 0.8f);
            }

            // Lava Flow Zones - flowing lava sounds
            for (int i = 0; i < lavaFlowZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    1f,
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"LavaFlowZone_{i}", pos, new Vector3(20f, 8f, 20f), "LavaFlow", 0.9f);
            }

            // Ember Crackle Zones - crackling fire sounds
            for (int i = 0; i < emberCrackleZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"EmberCrackleZone_{i}", pos, new Vector3(12f, 6f, 12f), "EmberCrackle", 0.6f);
            }

            Debug.Log($"🎵 Moon6AudioZones spawned {zones.Count} audio zones");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon6AudioZones.AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon6AudioZones.AudioZoneTrigger>();
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
