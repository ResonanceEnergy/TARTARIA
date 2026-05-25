using UnityEngine;
using UnityEditor;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates BuildingDefinition ScriptableObjects for Moons 3-13.
    /// Moon 1 buildings live in AssetFactoryWizard; Moon 2 in Moon2ZoneScaffold.
    /// Menu: Tartaria > Build Assets > Moon 3-13 Buildings
    /// </summary>
    public static class MoonBuildingFactory
    {
        const string BasePath = "Assets/_Project/Config/Buildings";

        [MenuItem("Tartaria/Build Assets/Moon 3-13 Buildings", false, 33)]
        public static void BuildAllMoonBuildings()
        {
            int created = 0;
            created += BuildMoon3();
            created += BuildMoon4();
            created += BuildMoon5();
            created += BuildMoon6();
            created += BuildMoon7();
            created += BuildMoon8();
            created += BuildMoon9();
            created += BuildMoon10();
            created += BuildMoon11();
            created += BuildMoon12();
            created += BuildMoon13();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Tartaria] Moon 3-13 buildings: {created} definitions created.");
        }

        // ── Moon 3: Windswept Highlands ──────────────

        static int BuildMoon3()
        {
            string path = $"{BasePath}/Moon3";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon3_watchtower",
                name = "Highland Watchtower",
                lore = "Watchtowers on the orphan route served as relay stations. " +
                       "Each tower's bell transmitted coded messages across the highland chain " +
                       "faster than any horse could ride.",
                archetype = BuildingArchetype.Tower,
                width = 10f, height = 32f,
                aetherStrength = 1.0f, aetherRadius = 70f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.BellTower),
                    Node(396f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon3_waystation",
                name = "Orphan Waystation",
                lore = "A shelter built for the displaced children of the Mud Flood. " +
                       "The walls still hold their lullabies -- encoded as harmonic patterns " +
                       "in the crystal-threaded mortar.",
                archetype = BuildingArchetype.Dome,
                width = 20f, height = 12.36f, // 20/phi
                aetherStrength = 0.8f, aetherRadius = 45f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 4f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.12f, 0.25f, TuningVariant.WaveformTrace),
                    Node(432f, 12f, 0.10f, 0.35f, TuningVariant.FrequencySlider),
                    Node(528f, 10f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                }
            });

            c += Create(path, new BD
            {
                id = "moon3_bridge",
                name = "Wind Bridge",
                lore = "Suspended between two cliff faces, the Wind Bridge vibrates at a " +
                       "perfect fifth interval. Walking across it was itself a tuning exercise.",
                archetype = BuildingArchetype.Gate,
                width = 40f, height = 8f,
                aetherStrength = 1.2f, aetherRadius = 55f,
                band = HarmonicBand.Resonant, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.10f, 0.30f, TuningVariant.HarmonicPattern),
                    Node(648f, 15f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(432f, 12f, 0.06f, 0.50f, TuningVariant.WaveformTrace),
                }
            });

            return c;
        }

        // ── Moon 4: Star Fort Bastion ────────────────

        static int BuildMoon4()
        {
            string path = $"{BasePath}/Moon4";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon4_star_chamber",
                name = "Star Chamber",
                lore = "The five-pointed chamber at the fort's heart. Each wall resonates " +
                       "at a different Solfeggio frequency. When all five align, the Archive opens.",
                archetype = BuildingArchetype.StarFort,
                width = 30f, height = 18.54f, // 30/phi
                aetherStrength = 2.0f, aetherRadius = 80f,
                band = HarmonicBand.Celestial, nodeCount = 5,
                dissolution = 8f,
                nodes = new[]
                {
                    Node(396f, 20f, 0.10f, 0.35f, TuningVariant.FrequencyDial),
                    Node(432f, 18f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(528f, 15f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(639f, 12f, 0.05f, 0.50f, TuningVariant.HarmonicPattern),
                    Node(741f, 10f, 0.04f, 0.55f, TuningVariant.WaveformTrace),
                }
            });

            c += Create(path, new BD
            {
                id = "moon4_bastion_gate",
                name = "Bastion Gate",
                lore = "The main gate opens only to resonant keys. Forced entry has been " +
                       "attempted for centuries -- the gate absorbed every impact and got stronger.",
                archetype = BuildingArchetype.Gate,
                width = 15f, height = 20f,
                aetherStrength = 1.0f, aetherRadius = 50f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.BellTower),
                    Node(432f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon4_amplifier_tower",
                name = "Royal Amplifier Tower",
                lore = "This tower amplified the Star Chamber's signal across the entire " +
                       "continental network. Its crystal tip still catches sunrise.",
                archetype = BuildingArchetype.Tower,
                width = 12f, height = 40f,
                aetherStrength = 1.8f, aetherRadius = 100f,
                band = HarmonicBand.Resonant, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.35f, TuningVariant.FrequencySlider),
                    Node(528f, 15f, 0.08f, 0.45f, TuningVariant.WaveformMatch),
                    Node(741f, 12f, 0.06f, 0.55f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon4_garrison_hall",
                name = "Garrison Hall",
                lore = "Not a barracks but a training hall for resonance practitioners. " +
                       "The acoustic design lets a whisper at one end arrive amplified at the other.",
                archetype = BuildingArchetype.Dome,
                width = 25f, height = 15.45f,
                aetherStrength = 0.8f, aetherRadius = 40f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.12f, 0.25f, TuningVariant.WaveformTrace),
                    Node(432f, 12f, 0.10f, 0.35f, TuningVariant.HarmonicPattern),
                    Node(528f, 10f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                }
            });

            return c;
        }

        // ── Moon 5: Sunken Colosseum ─────────────────

        static int BuildMoon5()
        {
            string path = $"{BasePath}/Moon5";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon5_arena_stage",
                name = "Resonance Stage",
                lore = "This was the performance stage -- not for entertainment but for " +
                       "collective tuning. Five thousand people humming in unison could heal " +
                       "a fractured ley line.",
                archetype = BuildingArchetype.Amphitheatre,
                width = 50f, height = 8f,
                aetherStrength = 2.0f, aetherRadius = 90f,
                band = HarmonicBand.Harmonic, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.10f, 0.35f, TuningVariant.HarmonicPattern),
                    Node(528f, 18f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(639f, 15f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                    Node(432f, 12f, 0.05f, 0.55f, TuningVariant.WaveformTrace),
                }
            });

            c += Create(path, new BD
            {
                id = "moon5_echo_gallery",
                name = "Echo Gallery",
                lore = "The gallery preserves five thousand echo imprints " +
                       "from the last performance. Each seat holds a different harmonic.",
                archetype = BuildingArchetype.Archive,
                width = 30f, height = 12f,
                aetherStrength = 1.0f, aetherRadius = 50f,
                band = HarmonicBand.Ethereal, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.10f, 0.30f, TuningVariant.WaveformTrace),
                    Node(432f, 12f, 0.08f, 0.40f, TuningVariant.FrequencySlider),
                    Node(528f, 10f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            c += Create(path, new BD
            {
                id = "moon5_tidal_gate",
                name = "Tidal Gate",
                lore = "The colosseum floods and drains with the tide. " +
                       "When submerged, the acoustics shift to underwater frequencies " +
                       "that resonate with the planet's deep core.",
                archetype = BuildingArchetype.Gate,
                width = 20f, height = 15f,
                aetherStrength = 1.2f, aetherRadius = 55f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(396f, 18f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(432f, 15f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(528f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            return c;
        }

        // ── Moon 6: Living Library ───────────────────

        static int BuildMoon6()
        {
            string path = $"{BasePath}/Moon6";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon6_archive_interface",
                name = "Archive Interface",
                lore = "The direct terminal to the planetary Archive. Knowledge here isn't " +
                       "read -- it's heard. Each book is a frequency pattern that unfolds " +
                       "when you match its resonance.",
                archetype = BuildingArchetype.Archive,
                width = 25f, height = 35f,
                aetherStrength = 2.5f, aetherRadius = 100f,
                band = HarmonicBand.Celestial, nodeCount = 4,
                dissolution = 8f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(528f, 18f, 0.06f, 0.45f, TuningVariant.WaveformMatch),
                    Node(639f, 15f, 0.05f, 0.50f, TuningVariant.WaveformTrace),
                    Node(741f, 12f, 0.04f, 0.55f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon6_cymatic_cathedral",
                name = "Cymatic Cathedral",
                lore = "Veritas's domain. The cathedral's organ doesn't play music -- " +
                       "it plays sand patterns on enormous brass plates. " +
                       "Each pattern is a sentence in the old language.",
                archetype = BuildingArchetype.Cathedral,
                width = 40f, height = 24.72f, // 40/phi
                aetherStrength = 2.0f, aetherRadius = 85f,
                band = HarmonicBand.Resonant, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.10f, 0.35f, TuningVariant.BellTower),
                    Node(528f, 18f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(639f, 15f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                    Node(741f, 12f, 0.05f, 0.55f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon6_reading_spire",
                name = "Reading Spire",
                lore = "A needle-thin tower where scholars meditated on single frequencies " +
                       "for days at a time. The spire resonates at 528 Hz -- the frequency " +
                       "of DNA repair.",
                archetype = BuildingArchetype.Spire,
                width = 6f, height = 45f,
                aetherStrength = 1.5f, aetherRadius = 60f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(528f, 18f, 0.10f, 0.30f, TuningVariant.FrequencySlider),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(528f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon6_memory_well",
                name = "Memory Well",
                lore = "Drop a thought into the well and it returns as a harmonic memory. " +
                       "The water preserves intention the way amber preserves insects.",
                archetype = BuildingArchetype.Fountain,
                width = 8f, height = 4.94f,
                aetherStrength = 0.6f, aetherRadius = 30f,
                band = HarmonicBand.Ethereal, nodeCount = 3,
                dissolution = 3f,
                nodes = new[]
                {
                    Node(396f, 12f, 0.12f, 0.25f, TuningVariant.FrequencyDial),
                    Node(432f, 10f, 0.10f, 0.35f, TuningVariant.WaveformTrace),
                    Node(528f, 8f, 0.08f, 0.45f, TuningVariant.FrequencySlider),
                }
            });

            return c;
        }

        // ── Moon 7: Clockwork Citadel ────────────────

        static int BuildMoon7()
        {
            string path = $"{BasePath}/Moon7";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon7_orrery",
                name = "Grand Orrery",
                lore = "A mechanical model of every celestial body visible from Earth. " +
                       "Each gear ratio is a musical interval. The entire machine is a symphony " +
                       "frozen in brass.",
                archetype = BuildingArchetype.Dome,
                width = 35f, height = 21.63f,
                aetherStrength = 2.0f, aetherRadius = 90f,
                band = HarmonicBand.Celestial, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(528f, 18f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(639f, 15f, 0.05f, 0.50f, TuningVariant.WaveformMatch),
                    Node(741f, 12f, 0.04f, 0.55f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon7_clocktower",
                name = "Prophecy Clock Tower",
                lore = "This clock predicted the Mud Flood four hours in advance. " +
                       "It doesn't measure time -- it measures resonance divergence.",
                archetype = BuildingArchetype.Tower,
                width = 12f, height = 50f,
                aetherStrength = 1.5f, aetherRadius = 75f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.35f, TuningVariant.BellTower),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(639f, 12f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            c += Create(path, new BD
            {
                id = "moon7_gear_hall",
                name = "Gear Hall",
                lore = "The manufacturing floor where Korath once oversaw the great machines. " +
                       "Every gear is tuned. Every bearing sings. Silence here means something is broken.",
                archetype = BuildingArchetype.Forge,
                width = 30f, height = 15f,
                aetherStrength = 1.0f, aetherRadius = 50f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.12f, 0.25f, TuningVariant.HarmonicPattern),
                    Node(432f, 12f, 0.10f, 0.35f, TuningVariant.FrequencySlider),
                    Node(528f, 10f, 0.08f, 0.40f, TuningVariant.WaveformTrace),
                }
            });

            return c;
        }

        // ── Moon 8: Verdant Canopy ───────────────────

        static int BuildMoon8()
        {
            string path = $"{BasePath}/Moon8";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon8_canopy_shrine",
                name = "Canopy Shrine",
                lore = "Where roots and architecture became indistinguishable. " +
                       "The shrine grows. It repairs itself. It breathes.",
                archetype = BuildingArchetype.Dome,
                width = 22f, height = 13.59f,
                aetherStrength = 1.5f, aetherRadius = 60f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.FrequencySlider),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.WaveformTrace),
                    Node(639f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon8_root_bridge",
                name = "Living Root Bridge",
                lore = "Grown over centuries, the root bridge is both structure and organism. " +
                       "It conducts Aether through its living tissue like blood through veins.",
                archetype = BuildingArchetype.Gate,
                width = 35f, height = 10f,
                aetherStrength = 1.2f, aetherRadius = 55f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(396f, 18f, 0.10f, 0.30f, TuningVariant.WaveformTrace),
                    Node(432f, 15f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(528f, 12f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon8_bioluminescent_well",
                name = "Bioluminescent Well",
                lore = "The well glows with the same blue-green light as deep-sea creatures. " +
                       "This light IS Aether made visible -- photons carrying frequency data.",
                archetype = BuildingArchetype.Fountain,
                width = 10f, height = 6.18f,
                aetherStrength = 0.8f, aetherRadius = 40f,
                band = HarmonicBand.Ethereal, nodeCount = 3,
                dissolution = 4f,
                nodes = new[]
                {
                    Node(432f, 15f, 0.12f, 0.25f, TuningVariant.FrequencyDial),
                    Node(528f, 12f, 0.10f, 0.35f, TuningVariant.WaveformMatch),
                    Node(639f, 10f, 0.08f, 0.40f, TuningVariant.FrequencySlider),
                }
            });

            return c;
        }

        // ── Moon 9: Auroral Spire ────────────────────

        static int BuildMoon9()
        {
            string path = $"{BasePath}/Moon9";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon9_solar_antenna",
                name = "Solar Antenna",
                lore = "The Auroral Spire's crown jewel. At dawn, " +
                       "the antenna converts sunlight directly into Aether frequencies. " +
                       "The aurora is a side effect of the power involved.",
                archetype = BuildingArchetype.Spire,
                width = 8f, height = 60f,
                aetherStrength = 3.0f, aetherRadius = 120f,
                band = HarmonicBand.Celestial, nodeCount = 4,
                dissolution = 8f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(528f, 18f, 0.06f, 0.45f, TuningVariant.WaveformMatch),
                    Node(741f, 15f, 0.05f, 0.50f, TuningVariant.FrequencyDial),
                    Node(852f, 12f, 0.04f, 0.55f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon9_prism_chamber",
                name = "Prism Chamber",
                lore = "Light enters as white and exits as seven distinct frequencies. " +
                       "Each colour channel carries a different instruction set for the ley lines.",
                archetype = BuildingArchetype.Dome,
                width = 18f, height = 11.12f,
                aetherStrength = 1.5f, aetherRadius = 55f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.WaveformTrace),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(639f, 12f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            c += Create(path, new BD
            {
                id = "moon9_aurora_gallery",
                name = "Aurora Gallery",
                lore = "An open-air gallery where the aurora paints new patterns every night. " +
                       "The patterns are not random -- they're the planet's status report.",
                archetype = BuildingArchetype.Archive,
                width = 25f, height = 6f,
                aetherStrength = 1.0f, aetherRadius = 50f,
                band = HarmonicBand.Resonant, nodeCount = 3,
                dissolution = 4f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.10f, 0.30f, TuningVariant.FrequencySlider),
                    Node(432f, 12f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(528f, 10f, 0.06f, 0.50f, TuningVariant.WaveformTrace),
                }
            });

            return c;
        }

        // ── Moon 10: Deep Forge ──────────────────────

        static int BuildMoon10()
        {
            string path = $"{BasePath}/Moon10";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon10_resonance_anvil",
                name = "Resonance Anvil",
                lore = "The anvil rings at B flat -- the note of transformation. " +
                       "Every strike reshapes not just metal but the local frequency field.",
                archetype = BuildingArchetype.Forge,
                width = 15f, height = 8f,
                aetherStrength = 2.5f, aetherRadius = 70f,
                band = HarmonicBand.Telluric, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(396f, 20f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(432f, 18f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.05f, 0.50f, TuningVariant.BellTower),
                    Node(639f, 12f, 0.04f, 0.55f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon10_lava_conduit",
                name = "Lava Conduit",
                lore = "The conduit channels geothermal energy directly into the Aether grid. " +
                       "The heat is a frequency too -- just one our instruments took centuries to decode.",
                archetype = BuildingArchetype.Gate,
                width = 12f, height = 20f,
                aetherStrength = 1.8f, aetherRadius = 65f,
                band = HarmonicBand.Telluric, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(396f, 18f, 0.10f, 0.35f, TuningVariant.FrequencyDial),
                    Node(432f, 15f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(528f, 12f, 0.06f, 0.50f, TuningVariant.FrequencySlider),
                }
            });

            c += Create(path, new BD
            {
                id = "moon10_crystal_crucible",
                name = "Crystal Crucible",
                lore = "Raw crystal enters. Tuned crystal exits. The crucible doesn't melt -- " +
                       "it sings the crystal into its optimal lattice structure.",
                archetype = BuildingArchetype.Dome,
                width = 20f, height = 12.36f,
                aetherStrength = 1.2f, aetherRadius = 50f,
                band = HarmonicBand.Harmonic, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 15f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(528f, 12f, 0.08f, 0.40f, TuningVariant.WaveformTrace),
                    Node(639f, 10f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            return c;
        }

        // ── Moon 11: Tidal Archive ───────────────────

        static int BuildMoon11()
        {
            string path = $"{BasePath}/Moon11";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon11_tidal_archive",
                name = "Tidal Archive Chamber",
                lore = "Memories written in water. Each tide cycle overwrites the surface " +
                       "but preserves the deeper layers. A thousand years of memory, " +
                       "readable only at low tide.",
                archetype = BuildingArchetype.Archive,
                width = 30f, height = 10f,
                aetherStrength = 2.0f, aetherRadius = 80f,
                band = HarmonicBand.Ethereal, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(396f, 20f, 0.08f, 0.40f, TuningVariant.WaveformTrace),
                    Node(432f, 18f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.05f, 0.50f, TuningVariant.WaveformMatch),
                    Node(639f, 12f, 0.04f, 0.55f, TuningVariant.FrequencySlider),
                }
            });

            c += Create(path, new BD
            {
                id = "moon11_coral_spire",
                name = "Coral Spire",
                lore = "A spire grown from resonance-attuned coral. " +
                       "It converts ocean currents into Aether the way " +
                       "a windmill converts wind into rotation.",
                archetype = BuildingArchetype.Spire,
                width = 8f, height = 30f,
                aetherStrength = 1.5f, aetherRadius = 60f,
                band = HarmonicBand.Resonant, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(639f, 12f, 0.06f, 0.50f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon11_moon_pool",
                name = "Moon Pool",
                lore = "A perfectly circular pool that reflects the moon with impossible clarity. " +
                       "The reflection is brighter than the original on certain nights.",
                archetype = BuildingArchetype.Fountain,
                width = 15f, height = 3f,
                aetherStrength = 0.8f, aetherRadius = 40f,
                band = HarmonicBand.Celestial, nodeCount = 3,
                dissolution = 4f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.12f, 0.25f, TuningVariant.FrequencySlider),
                    Node(432f, 12f, 0.10f, 0.35f, TuningVariant.WaveformTrace),
                    Node(528f, 10f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                }
            });

            return c;
        }

        // ── Moon 12: Celestial Observatory ───────────

        static int BuildMoon12()
        {
            string path = $"{BasePath}/Moon12";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon12_star_lens",
                name = "Star Lens",
                lore = "Not a telescope but a frequency separator. Each star emits a unique " +
                       "harmonic signature. The Lens decodes them all simultaneously.",
                archetype = BuildingArchetype.Observatory,
                width = 20f, height = 30f,
                aetherStrength = 3.0f, aetherRadius = 100f,
                band = HarmonicBand.Celestial, nodeCount = 4,
                dissolution = 8f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(528f, 18f, 0.05f, 0.50f, TuningVariant.WaveformMatch),
                    Node(741f, 15f, 0.04f, 0.55f, TuningVariant.WaveformTrace),
                    Node(852f, 12f, 0.03f, 0.60f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon12_zodiac_ring",
                name = "Zodiac Ring",
                lore = "Twelve pillars, each tuned to a different zodiacal frequency. " +
                       "When all twelve activate, they project a star map onto the sky " +
                       "that shows how reality ACTUALLY looks above the firmament.",
                archetype = BuildingArchetype.Obelisk,
                width = 40f, height = 15f,
                aetherStrength = 2.0f, aetherRadius = 80f,
                band = HarmonicBand.Resonant, nodeCount = 3,
                dissolution = 6f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.08f, 0.40f, TuningVariant.HarmonicPattern),
                    Node(528f, 15f, 0.06f, 0.45f, TuningVariant.FrequencyDial),
                    Node(639f, 12f, 0.05f, 0.50f, TuningVariant.BellTower),
                }
            });

            c += Create(path, new BD
            {
                id = "moon12_astral_dome",
                name = "Astral Dome",
                lore = "The dome's interior surface is a perfect mirror of the night sky. " +
                       "But it shows stars that no modern telescope has ever seen.",
                archetype = BuildingArchetype.Dome,
                width = 28f, height = 17.3f,
                aetherStrength = 1.5f, aetherRadius = 65f,
                band = HarmonicBand.Ethereal, nodeCount = 3,
                dissolution = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.WaveformTrace),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.FrequencySlider),
                    Node(741f, 12f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            return c;
        }

        // ── Moon 13: Planetary Nexus ─────────────────

        static int BuildMoon13()
        {
            string path = $"{BasePath}/Moon13";
            EnsurePath(path);
            int c = 0;

            c += Create(path, new BD
            {
                id = "moon13_nexus_core",
                name = "Nexus Core",
                lore = "The heart of the planetary resonance network. Every ley line, " +
                       "every antenna, every restored building feeds into this single point. " +
                       "When it activates, the planet remembers what it was.",
                archetype = BuildingArchetype.Dome,
                width = 50f, height = 30.9f, // 50/phi
                aetherStrength = 5.0f, aetherRadius = 200f,
                band = HarmonicBand.Resonant, nodeCount = 5,
                dissolution = 10f,
                nodes = new[]
                {
                    Node(396f, 25f, 0.06f, 0.50f, TuningVariant.HarmonicPattern),
                    Node(432f, 22f, 0.05f, 0.55f, TuningVariant.FrequencyDial),
                    Node(528f, 20f, 0.04f, 0.60f, TuningVariant.WaveformMatch),
                    Node(639f, 18f, 0.03f, 0.65f, TuningVariant.BellTower),
                    Node(741f, 15f, 0.03f, 0.70f, TuningVariant.WaveformTrace),
                }
            });

            c += Create(path, new BD
            {
                id = "moon13_convergence_obelisk",
                name = "Convergence Obelisk",
                lore = "Thirteen faces for thirteen moons. Each face displays the restoration " +
                       "status of its corresponding zone. When all faces glow gold, " +
                       "the obelisk opens a portal to the True Timeline.",
                archetype = BuildingArchetype.Obelisk,
                width = 5f, height = 40f,
                aetherStrength = 3.0f, aetherRadius = 120f,
                band = HarmonicBand.Celestial, nodeCount = 4,
                dissolution = 7f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                    Node(528f, 18f, 0.05f, 0.55f, TuningVariant.WaveformMatch),
                    Node(639f, 15f, 0.04f, 0.60f, TuningVariant.HarmonicPattern),
                    Node(741f, 12f, 0.03f, 0.65f, TuningVariant.FrequencyDial),
                }
            });

            c += Create(path, new BD
            {
                id = "moon13_veil_gate",
                name = "Veil Gate",
                lore = "The boundary between timelines made physical. " +
                       "Step through and the world shifts -- not place, but time. " +
                       "The veil is thinnest where resonance is strongest.",
                archetype = BuildingArchetype.Gate,
                width = 20f, height = 25f,
                aetherStrength = 4.0f, aetherRadius = 150f,
                band = HarmonicBand.Ethereal, nodeCount = 4,
                dissolution = 8f,
                nodes = new[]
                {
                    Node(432f, 22f, 0.06f, 0.50f, TuningVariant.WaveformTrace),
                    Node(528f, 20f, 0.05f, 0.55f, TuningVariant.FrequencyDial),
                    Node(639f, 18f, 0.04f, 0.60f, TuningVariant.WaveformMatch),
                    Node(741f, 15f, 0.03f, 0.65f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon13_anastasia_shrine",
                name = "Anastasia's Shrine",
                lore = "Built not by Tartarians but by Anastasia herself -- " +
                       "assembled from memory fragments across all thirteen zones. " +
                       "When complete, it anchors her spirit permanently to the physical world.",
                archetype = BuildingArchetype.Cathedral,
                width = 18f, height = 25f,
                aetherStrength = 3.5f, aetherRadius = 100f,
                band = HarmonicBand.Resonant, nodeCount = 4,
                dissolution = 9f,
                nodes = new[]
                {
                    Node(432f, 22f, 0.05f, 0.55f, TuningVariant.FrequencyDial),
                    Node(528f, 20f, 0.04f, 0.60f, TuningVariant.WaveformMatch),
                    Node(639f, 18f, 0.03f, 0.65f, TuningVariant.WaveformTrace),
                    Node(741f, 15f, 0.03f, 0.70f, TuningVariant.HarmonicPattern),
                }
            });

            c += Create(path, new BD
            {
                id = "moon13_true_timeline_spire",
                name = "True Timeline Spire",
                lore = "The final structure. Not buried -- hidden in plain sight " +
                       "across all timelines simultaneously. Restoring it means " +
                       "choosing which timeline becomes real.",
                archetype = BuildingArchetype.Spire,
                width = 10f, height = 80f,
                aetherStrength = 5.0f, aetherRadius = 250f,
                band = HarmonicBand.Resonant, nodeCount = 5,
                dissolution = 12f,
                nodes = new[]
                {
                    Node(396f, 25f, 0.05f, 0.55f, TuningVariant.FrequencyDial),
                    Node(432f, 22f, 0.04f, 0.60f, TuningVariant.HarmonicPattern),
                    Node(528f, 20f, 0.03f, 0.65f, TuningVariant.WaveformMatch),
                    Node(639f, 18f, 0.03f, 0.70f, TuningVariant.BellTower),
                    Node(741f, 15f, 0.02f, 0.75f, TuningVariant.WaveformTrace),
                }
            });

            return c;
        }

        // ── Helpers ──────────────────────────────────

        static int Create(string folder, BD data)
        {
            string path = $"{folder}/Building_{data.id}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path) != null) return 0;

            var bd = ScriptableObject.CreateInstance<BuildingDefinition>();
            bd.buildingName = data.name;
            bd.loreDescription = data.lore;
            bd.archetype = data.archetype;
            bd.width = data.width;
            bd.height = data.height;
            bd.aetherSourceStrength = data.aetherStrength;
            bd.aetherSourceRadius = data.aetherRadius;
            bd.outputBand = data.band;
            bd.nodeCount = data.nodeCount;
            bd.nodePuzzles = data.nodes;
            bd.dissolutionDuration = data.dissolution;

            AssetDatabase.CreateAsset(bd, path);
            return 1;
        }

        static TuningPuzzleConfig Node(float freq, float time, float tol, float speed, TuningVariant variant)
        {
            return new TuningPuzzleConfig
            {
                targetFrequency = freq,
                timeLimitSeconds = time,
                tolerancePercent = tol,
                difficultySpeed = speed,
                variant = variant
            };
        }

        static void EnsurePath(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string[] parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        struct BD
        {
            public string id, name, lore;
            public BuildingArchetype archetype;
            public float width, height, aetherStrength, aetherRadius, dissolution;
            public HarmonicBand band;
            public int nodeCount;
            public TuningPuzzleConfig[] nodes;
        }
    }
}
