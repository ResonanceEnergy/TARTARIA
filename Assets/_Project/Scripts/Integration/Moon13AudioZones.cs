using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-54)]
    public class Moon13AudioZones : MonoBehaviour
    {
        [Header("Moon 13: Aether Convergence Audio Zones")]
        [SerializeField] int convergenceZoneCount = 4; // Primary convergence areas
        [SerializeField] int moonTributeZoneCount = 12; // One per moon in circle
        [SerializeField] int aetherHarmonyZoneCount = 5;
        [SerializeField] int realityBreathZoneCount = 6;

        List<GameObject> zones = new List<GameObject>();

        void Start()
        {
            SpawnAudioZones();
        }

        void SpawnAudioZones()
        {
            // Central Convergence Zone - all sounds merge here
            CreateAudioZone("CentralConvergenceZone", Vector3.zero, new Vector3(30f, 20f, 30f), "CentralConvergence", 1.2f);

            // Cardinal Convergence Zones
            for (int i = 0; i < convergenceZoneCount - 1; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * 50f,
                    3f,
                    Mathf.Sin(angle) * 50f
                );
                CreateAudioZone($"CardinalConvergenceZone_{i}", pos, new Vector3(22f, 14f, 22f), "CardinalConvergence", 1f);
            }

            // Moon Tribute Zones - 12 zones in circle (one per moon)
            string[] moonAudioTypes = GetMoonTributeAudioTypes();
            float radius = 80f;
            for (int i = 0; i < moonTributeZoneCount; i++)
            {
                float angle = (i * 360f / moonTributeZoneCount) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    2f,
                    Mathf.Sin(angle) * radius
                );
                CreateAudioZone($"MoonTributeZone_{i + 1}_{moonAudioTypes[i]}", pos, new Vector3(18f, 12f, 18f), moonAudioTypes[i], 0.9f);
            }

            // Aether Harmony Zones - peaceful energy zones
            for (int i = 0; i < aetherHarmonyZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    3f,
                    Random.Range(-60f, 60f)
                );
                CreateAudioZone($"AetherHarmonyZone_{i}", pos, new Vector3(20f, 12f, 20f), "AetherHarmony", 0.8f);
            }

            // Reality Breath Zones - subtle ambient zones
            for (int i = 0; i < realityBreathZoneCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    2f,
                    Random.Range(-70f, 70f)
                );
                CreateAudioZone($"RealityBreathZone_{i}", pos, new Vector3(16f, 10f, 16f), "RealityBreath", 0.7f);
            }

            Debug.Log($"✨ Moon13AudioZones spawned {zones.Count} audio zones (including 12-moon tribute circle at radius 80f and FINAL convergence zone)");
        }

        GameObject CreateAudioZone(string name, Vector3 position, Vector3 scale, string zoneType, float intensity)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;

            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = scale;
            trigger.isTrigger = true;

            Moon13AudioZones.AudioZoneTrigger zoneTrigger = zone.AddComponent<Moon13AudioZones.AudioZoneTrigger>();
            zoneTrigger.zoneType = zoneType;
            zoneTrigger.intensity = intensity;

            zones.Add(zone);
            return zone;
        }

        string[] GetMoonTributeAudioTypes()
        {
            return new string[]
            {
                "MemoryEchoes",    // Moon1: Memory
                "DreamWhispers",   // Moon2: Dream
                "JungleAmbience",  // Moon3: Jungle
                "DesertWinds",     // Moon4: Desert
                "IceCreaking",     // Moon5: Ice
                "LavaRumble",      // Moon6: Lava
                "UnderwaterFlow",  // Moon7: Underwater
                "SkyWind",         // Moon8: Sky
                "CorruptionPulse", // Moon9: Corruption
                "TemporalDistort", // Moon10: Time
                "PrismaticHarmony",// Moon11: Prismatic
                "ShadowSilence"    // Moon12: Shadow
            };
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
