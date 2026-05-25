using UnityEngine;
using UnityEditor;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates all 13 ZoneDefinition ScriptableObject assets (one per Moon).
    /// Menu: Tartaria > Build Assets > Zone Definitions
    /// </summary>
    public static class ZoneDefinitionFactory
    {
        const string ZonePath = "Assets/_Project/Config/Zones";

        [MenuItem("Tartaria/Build Assets/Zone Definitions", false, 21)]
        public static void BuildZoneDefinitions()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Config/Zones"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/Config"))
                    AssetDatabase.CreateFolder("Assets/_Project", "Config");
                AssetDatabase.CreateFolder("Assets/_Project/Config", "Zones");
            }

            var zones = GetZoneData();
            int created = 0;

            for (int i = 0; i < zones.Length; i++)
            {
                var z = zones[i];
                string assetPath = $"{ZonePath}/Zone_{i + 1:D2}_{z.assetName}.asset";

                if (AssetDatabase.LoadAssetAtPath<ZoneDefinition>(assetPath) != null)
                    continue;

                var so = ScriptableObject.CreateInstance<ZoneDefinition>();
                so.zoneName = z.zoneName;
                so.subtitle = z.subtitle;
                so.loreIntro = z.loreIntro;
                so.zoneIndex = i;
                so.rsRequirementToUnlock = z.rsUnlock;
                so.buildingCount = z.buildingCount;
                so.playerSpawnPosition = z.spawnPos;
                so.fogColorLow = z.fogLow;
                so.fogColorHigh = z.fogHigh;
                so.startingFogDensity = z.fogDensity;
                so.ambientLow = z.ambientLow;
                so.ambientHigh = z.ambientHigh;
                so.loadingTip = z.loadingTip;
                so.sceneName = z.sceneName;
                so.npcSpawnPoints = z.npcSpawns ?? System.Array.Empty<NPCSpawnPoint>();

                AssetDatabase.CreateAsset(so, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Tartaria] Zone Definitions: {created} created ({zones.Length} total).");
        }

        struct ZoneData
        {
            public string assetName, zoneName, subtitle, loreIntro, loadingTip, sceneName;
            public float rsUnlock, fogDensity;
            public int buildingCount;
            public Vector3 spawnPos;
            public Color fogLow, fogHigh, ambientLow, ambientHigh;
            public NPCSpawnPoint[] npcSpawns;
        }

        static ZoneData[] GetZoneData()
        {
            return new[]
            {
                // Moon 1
                new ZoneData
                {
                    assetName = "Echohaven",
                    zoneName = "Echohaven",
                    subtitle = "New Chicago",
                    sceneName = "Echohaven_VerticalSlice",
                    loreIntro = "A once-great city buried under centuries of mud. The domes still hum beneath the earth, waiting to be remembered.",
                    rsUnlock = 0f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.35f, 0.28f, 0.2f),
                    fogHigh = new Color(0.8f, 0.75f, 0.5f),
                    fogDensity = 0.035f,
                    ambientLow = new Color(0.15f, 0.12f, 0.1f),
                    ambientHigh = new Color(0.6f, 0.55f, 0.4f),
                    loadingTip = "The stones remember their shape. You just need to remind them.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "milo", position = new Vector3(5, 1, 3), yRotation = 180f, requiresIntroduction = true },
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-4, 1, 6), yRotation = 90f, requiresIntroduction = true },
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(12, 1, -2), yRotation = 270f, requiresIntroduction = true },
                    }
                },
                // Moon 2
                new ZoneData
                {
                    assetName = "CrystallineCaverns",
                    zoneName = "Crystalline Caverns",
                    subtitle = "The First Antenna",
                    sceneName = "CrystallineCaverns",
                    loreIntro = "Subterranean crystal formations that once amplified the planet's resonance network. The deepest chamber holds the first antenna.",
                    rsUnlock = 100f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.15f, 0.18f, 0.25f),
                    fogHigh = new Color(0.4f, 0.55f, 0.7f),
                    fogDensity = 0.02f,
                    ambientLow = new Color(0.1f, 0.12f, 0.18f),
                    ambientHigh = new Color(0.4f, 0.5f, 0.65f),
                    loadingTip = "Crystals amplify Aether the way a bell amplifies silence.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "cassian", position = new Vector3(8, 1, -5), yRotation = 0f, requiresIntroduction = true },
                    }
                },
                // Moon 3
                new ZoneData
                {
                    assetName = "WindsweptHighlands",
                    zoneName = "Windswept Highlands",
                    subtitle = "The Orphan Route",
                    sceneName = "WindsweptHighlands",
                    loreIntro = "Highland plateaus carved by wind and time. The orphan train tracks still lead somewhere -- if you know where to look.",
                    rsUnlock = 200f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.4f, 0.42f, 0.45f),
                    fogHigh = new Color(0.7f, 0.75f, 0.8f),
                    fogDensity = 0.015f,
                    ambientLow = new Color(0.2f, 0.2f, 0.22f),
                    ambientHigh = new Color(0.65f, 0.65f, 0.7f),
                    loadingTip = "Highland architecture bends but never breaks. Like reeds.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-3, 1, 8), yRotation = 120f, requiresIntroduction = false },
                    }
                },
                // Moon 4
                new ZoneData
                {
                    assetName = "StarFortBastion",
                    zoneName = "Star Fort Bastion",
                    subtitle = "The Royal Amplifier",
                    sceneName = "StarFortBastion",
                    loreIntro = "A pentagonal star fort -- not military, but a five-point frequency amplifier. The Star Chamber within once accessed the Archive itself.",
                    rsUnlock = 300f,
                    buildingCount = 4,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.25f, 0.22f, 0.18f),
                    fogHigh = new Color(0.65f, 0.6f, 0.45f),
                    fogDensity = 0.025f,
                    ambientLow = new Color(0.15f, 0.13f, 0.1f),
                    ambientHigh = new Color(0.55f, 0.5f, 0.38f),
                    loadingTip = "Star forts were amplifiers. Five points, each at a different frequency.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(10, 1, -8), yRotation = 315f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "cassian", position = new Vector3(-6, 1, 12), yRotation = 180f, requiresIntroduction = false },
                    }
                },
                // Moon 5
                new ZoneData
                {
                    assetName = "SunkenColosseum",
                    zoneName = "Sunken Colosseum",
                    subtitle = "The Resonance Stage",
                    sceneName = "SunkenColosseum",
                    loreIntro = "A partially submerged colosseum built for resonance performances, not combat. Five thousand echo imprints linger in the seats.",
                    rsUnlock = 400f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.2f, 0.25f, 0.3f),
                    fogHigh = new Color(0.5f, 0.6f, 0.7f),
                    fogDensity = 0.03f,
                    ambientLow = new Color(0.12f, 0.15f, 0.2f),
                    ambientHigh = new Color(0.45f, 0.55f, 0.65f),
                    loadingTip = "Water doesn't corrupt -- it preserves.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "milo", position = new Vector3(6, 1, 4), yRotation = 225f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-8, 1, 10), yRotation = 90f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(14, 1, -4), yRotation = 270f, requiresIntroduction = false },
                    }
                },
                // Moon 6
                new ZoneData
                {
                    assetName = "LivingLibrary",
                    zoneName = "Living Library",
                    subtitle = "The Archive Interface",
                    sceneName = "LivingLibrary",
                    loreIntro = "The Living Library was not mere storage -- it was a direct interface to the planetary Archive. Rhythmic knowledge encoded in every shelf.",
                    rsUnlock = 500f,
                    buildingCount = 4,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.18f, 0.15f, 0.12f),
                    fogHigh = new Color(0.55f, 0.48f, 0.35f),
                    fogDensity = 0.02f,
                    ambientLow = new Color(0.12f, 0.1f, 0.08f),
                    ambientHigh = new Color(0.5f, 0.42f, 0.3f),
                    loadingTip = "Read with your ears, not your eyes.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "veritas", position = new Vector3(0, 1, 15), yRotation = 0f, requiresIntroduction = true },
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-5, 1, 12), yRotation = 45f, requiresIntroduction = false },
                    }
                },
                // Moon 7
                new ZoneData
                {
                    assetName = "ClockworkCitadel",
                    zoneName = "Clockwork Citadel",
                    subtitle = "Mechanical Prophecy",
                    sceneName = "ClockworkCitadel",
                    loreIntro = "A citadel of gears and orreries. The grand clockwork predicted the Mud Flood four hours before it came. The builders had three hours to archive what mattered.",
                    rsUnlock = 600f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.22f, 0.2f, 0.18f),
                    fogHigh = new Color(0.6f, 0.55f, 0.45f),
                    fogDensity = 0.018f,
                    ambientLow = new Color(0.15f, 0.13f, 0.11f),
                    ambientHigh = new Color(0.5f, 0.45f, 0.35f),
                    loadingTip = "Synchronization was the Tartarian word for prayer.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "korath", position = new Vector3(-6, 1, 10), yRotation = 45f, requiresIntroduction = true },
                    }
                },
                // Moon 8
                new ZoneData
                {
                    assetName = "VerdantCanopy",
                    zoneName = "Verdant Canopy",
                    subtitle = "Where Life Refuses to Yield",
                    sceneName = "VerdantCanopy",
                    loreIntro = "An ancient forest where roots push through stone and bioluminescence traces the ley lines. Nature copied the architects -- or perhaps it was the other way around.",
                    rsUnlock = 700f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.12f, 0.22f, 0.12f),
                    fogHigh = new Color(0.35f, 0.65f, 0.35f),
                    fogDensity = 0.04f,
                    ambientLow = new Color(0.08f, 0.15f, 0.08f),
                    ambientHigh = new Color(0.3f, 0.55f, 0.3f),
                    loadingTip = "The strongest structures let life grow through them.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(8, 1, 6), yRotation = 200f, requiresIntroduction = false },
                    }
                },
                // Moon 9
                new ZoneData
                {
                    assetName = "AuroralSpire",
                    zoneName = "Auroral Spire",
                    subtitle = "Where Light Sings",
                    sceneName = "AuroralSpire",
                    loreIntro = "A towering antenna that channels solar energy into Aether. At the apex, the aurora touches the metal and sings in color.",
                    rsUnlock = 800f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.15f, 0.12f, 0.2f),
                    fogHigh = new Color(0.5f, 0.45f, 0.7f),
                    fogDensity = 0.012f,
                    ambientLow = new Color(0.1f, 0.08f, 0.15f),
                    ambientHigh = new Color(0.45f, 0.4f, 0.65f),
                    loadingTip = "corruption fears light. It's the one thing they can't absorb.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "zereth", position = new Vector3(0, 5, 20), yRotation = 180f, requiresIntroduction = false },
                    }
                },
                // Moon 10
                new ZoneData
                {
                    assetName = "DeepForge",
                    zoneName = "Deep Forge",
                    subtitle = "The Universe's Bass Note",
                    sceneName = "DeepForge",
                    loreIntro = "Beneath the mountains, Tartarian smiths shaped metal with sound. The anvil rings at B flat -- the note of transformation.",
                    rsUnlock = 900f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.3f, 0.15f, 0.08f),
                    fogHigh = new Color(0.7f, 0.4f, 0.2f),
                    fogDensity = 0.05f,
                    ambientLow = new Color(0.2f, 0.1f, 0.05f),
                    ambientHigh = new Color(0.6f, 0.35f, 0.15f),
                    loadingTip = "The Tartarian smiths shaped metal with sound, not muscle.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "korath", position = new Vector3(-10, 1, 5), yRotation = 90f, requiresIntroduction = false },
                    }
                },
                // Moon 11
                new ZoneData
                {
                    assetName = "TidalArchive",
                    zoneName = "Tidal Archive",
                    subtitle = "Memory in Water",
                    sceneName = "TidalArchive",
                    loreIntro = "A coastal archive older than the planetary one. The tide writes in light, preserving what was beneath its surface.",
                    rsUnlock = 1000f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.18f, 0.22f, 0.28f),
                    fogHigh = new Color(0.45f, 0.55f, 0.65f),
                    fogDensity = 0.028f,
                    ambientLow = new Color(0.12f, 0.15f, 0.2f),
                    ambientHigh = new Color(0.4f, 0.5f, 0.6f),
                    loadingTip = "Tides come and go. Echoes stay.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-6, 1, 8), yRotation = 45f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "veritas", position = new Vector3(4, 1, 12), yRotation = 315f, requiresIntroduction = false },
                    }
                },
                // Moon 12
                new ZoneData
                {
                    assetName = "CelestialObservatory",
                    zoneName = "Celestial Observatory",
                    subtitle = "Geometry at Sufficient Distance",
                    sceneName = "CelestialObservatory",
                    loreIntro = "The Observatory mapped every star with twelve-decimal precision. Now the planet can see the sky again -- and the sky can see us.",
                    rsUnlock = 1100f,
                    buildingCount = 3,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.08f, 0.08f, 0.15f),
                    fogHigh = new Color(0.2f, 0.2f, 0.4f),
                    fogDensity = 0.008f,
                    ambientLow = new Color(0.05f, 0.05f, 0.1f),
                    ambientHigh = new Color(0.2f, 0.2f, 0.35f),
                    loadingTip = "Everything is geometry at sufficient distance.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "veritas", position = new Vector3(0, 1, 18), yRotation = 0f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(12, 1, -6), yRotation = 270f, requiresIntroduction = false },
                    }
                },
                // Moon 13
                new ZoneData
                {
                    assetName = "PlanetaryNexus",
                    zoneName = "Planetary Nexus",
                    subtitle = "All Lines Converge",
                    sceneName = "PlanetaryNexus",
                    loreIntro = "Every ley line. Every frequency. Connected. The veil is thinnest here. The planet is awake -- and it's singing.",
                    rsUnlock = 1200f,
                    buildingCount = 5,
                    spawnPos = new Vector3(0, 1, 0),
                    fogLow = new Color(0.15f, 0.12f, 0.08f),
                    fogHigh = new Color(0.9f, 0.85f, 0.6f),
                    fogDensity = 0.01f,
                    ambientLow = new Color(0.1f, 0.08f, 0.05f),
                    ambientHigh = new Color(0.85f, 0.8f, 0.55f),
                    loadingTip = "After eight hundred years of silence... it's singing.",
                    npcSpawns = new[]
                    {
                        new NPCSpawnPoint { npcId = "milo", position = new Vector3(5, 1, 3), yRotation = 180f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "lirael", position = new Vector3(-4, 1, 6), yRotation = 90f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "thorne", position = new Vector3(12, 1, -2), yRotation = 270f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "cassian", position = new Vector3(-10, 1, -5), yRotation = 0f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "korath", position = new Vector3(0, 1, 20), yRotation = 180f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "veritas", position = new Vector3(8, 1, 15), yRotation = 225f, requiresIntroduction = false },
                        new NPCSpawnPoint { npcId = "zereth", position = new Vector3(0, 3, 25), yRotation = 180f, requiresIntroduction = false },
                    }
                },
            };
        }
    }
}
