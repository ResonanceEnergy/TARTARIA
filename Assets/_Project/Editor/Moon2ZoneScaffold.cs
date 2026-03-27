using UnityEngine;
using UnityEditor;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 2 Scaffolding -- generates all zone-specific assets for
    /// Crystalline Caverns (Moon 2: Lunar Moon -- Shadow & Purge):
    ///   - 3 BuildingDefinition SOs (Cathedral Dome, Bell Tower, Purification Fountain)
    ///   - Scene hierarchy template with spawn points, fog volumes, lighting
    ///   - Enemy spawn configuration for Fractal Wraiths
    ///
    /// Menu: Tartaria > Build Assets > Moon 2 Scaffolding
    /// </summary>
    public static class Moon2ZoneScaffold
    {
        const string BuildingPath = "Assets/_Project/Config/Buildings/Moon2";
        const string PrefabPath = "Assets/_Project/Prefabs/Moon2";
        const string MaterialPath = "Assets/_Project/Materials/Moon2";

        [MenuItem("Tartaria/Build Assets/Moon 2 Scaffolding", false, 30)]
        public static void BuildAll()
        {
            EnsureFolders();
            BuildBuildingDefinitions();
            BuildPlaceholderPrefabs();
            BuildSceneTemplate();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] Moon 2 scaffolding complete.");
        }

        // ─── Building Definitions ────────────────────

        [MenuItem("Tartaria/Build Assets/Moon 2 -- Buildings Only", false, 31)]
        public static void BuildBuildingDefinitions()
        {
            EnsureFolders();

            CreateBuilding(new BuildingData
            {
                id = "moon2_cathedral_dome",
                name = "Fractured Cathedral Dome",
                lore = "The dome that once sang is now silent. Dissonance crystals -- black, angular, wrong -- " +
                       "have embedded themselves in the inner fractal architecture. Micro-giant mode is required " +
                       "to enter the cathedral's impossible interior and root out the corruption at its source.",
                archetype = BuildingArchetype.Dome,
                width = 35f,
                height = 21.63f, // 35 / phi
                aetherStrength = 1.5f,
                aetherRadius = 65f,
                band = HarmonicBand.Harmonic,
                nodeCount = 4,
                dissolutionDuration = 7f,
                nodes = new[]
                {
                    Node(432f, 20f, 0.10f, 0.35f, TuningVariant.FrequencyDial),
                    Node(528f, 18f, 0.08f, 0.40f, TuningVariant.WaveformMatch),
                    Node(396f, 15f, 0.06f, 0.50f, TuningVariant.FrequencyDial),
                    Node(432f, 12f, 0.05f, 0.60f, TuningVariant.WaveformMatch),
                }
            });

            CreateBuilding(new BuildingData
            {
                id = "moon2_bell_tower",
                name = "Resonance Bell Tower",
                lore = "Bell towers are the immune system of the Aether grid. When rung in the correct sequence, " +
                       "scalar waves pulse across the sky as visible golden ripples. Three connected towers rung " +
                       "simultaneously create a permanent corruption ward.",
                archetype = BuildingArchetype.Tower,
                width = 8f,
                height = 28f,
                aetherStrength = 1.2f,
                aetherRadius = 80f,
                band = HarmonicBand.Resonant,
                nodeCount = 3,
                dissolutionDuration = 5f,
                nodes = new[]
                {
                    Node(432f, 18f, 0.10f, 0.30f, TuningVariant.FrequencyDial),
                    Node(528f, 15f, 0.08f, 0.40f, TuningVariant.FrequencyDial),
                    Node(639f, 12f, 0.06f, 0.50f, TuningVariant.WaveformMatch),
                }
            });

            CreateBuilding(new BuildingData
            {
                id = "moon2_fountain",
                name = "Purification Fountain",
                lore = "Ionized mist from this fountain repels corruption. When fully restored, the spray " +
                       "reaches 20 feet -- creating miniature auroras and a cleansing wave that purges the " +
                       "entire dome. Mud golems cannot approach active fountains.",
                archetype = BuildingArchetype.Fountain,
                width = 12f,
                height = 7.42f, // 12 / phi
                aetherStrength = 0.8f,
                aetherRadius = 40f,
                band = HarmonicBand.Ethereal,
                nodeCount = 3,
                dissolutionDuration = 4f,
                nodes = new[]
                {
                    Node(396f, 15f, 0.12f, 0.25f, TuningVariant.FrequencyDial),
                    Node(432f, 12f, 0.10f, 0.35f, TuningVariant.WaveformMatch),
                    Node(528f, 10f, 0.08f, 0.45f, TuningVariant.FrequencyDial),
                }
            });

            Debug.Log("[Tartaria] Moon 2 building definitions created.");
        }

        // ─── Placeholder Prefabs ─────────────────────

        static void BuildPlaceholderPrefabs()
        {
            // Crystal cavern environment material
            var crystalMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            crystalMat.name = "M_CrystalCavern";
            crystalMat.color = new Color(0.2f, 0.3f, 0.5f);
            crystalMat.SetFloat("_Smoothness", 0.85f);
            crystalMat.SetFloat("_Metallic", 0.3f);
            crystalMat.EnableKeyword("_EMISSION");
            crystalMat.SetColor("_EmissionColor", new Color(0.1f, 0.15f, 0.3f) * 0.5f);
            AssetDatabase.CreateAsset(crystalMat, $"{MaterialPath}/M_CrystalCavern.mat");

            // Dissonance crystal material
            var dissonanceMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            dissonanceMat.name = "M_DissonanceCrystal";
            dissonanceMat.color = new Color(0.05f, 0.02f, 0.08f);
            dissonanceMat.SetFloat("_Smoothness", 0.95f);
            dissonanceMat.SetFloat("_Metallic", 0.6f);
            dissonanceMat.EnableKeyword("_EMISSION");
            dissonanceMat.SetColor("_EmissionColor", new Color(0.3f, 0.0f, 0.4f) * 2f);
            AssetDatabase.CreateAsset(dissonanceMat, $"{MaterialPath}/M_DissonanceCrystal.mat");

            // Purified crystal material
            var pureMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pureMat.name = "M_PurifiedCrystal";
            pureMat.color = new Color(0.6f, 0.75f, 0.9f);
            pureMat.SetFloat("_Smoothness", 0.9f);
            pureMat.SetFloat("_Metallic", 0.2f);
            pureMat.EnableKeyword("_EMISSION");
            pureMat.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 0.8f) * 1.5f);
            AssetDatabase.CreateAsset(pureMat, $"{MaterialPath}/M_PurifiedCrystal.mat");

            Debug.Log("[Tartaria] Moon 2 materials created.");
        }

        // ─── Scene Template ──────────────────────────

        [MenuItem("Tartaria/Build Assets/Moon 2 -- Scene Template", false, 32)]
        public static void BuildSceneTemplate()
        {
            // Create a root GameObject that serves as the zone template
            // Designers duplicate this into their scene
            var root = new GameObject("--- MOON2_CRYSTALLINE_CAVERNS ---");

            // Spawn point
            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetParent(root.transform);
            spawn.transform.localPosition = new Vector3(0, 1, 0);

            // Building positions (triangular layout)
            var buildingsRoot = new GameObject("Buildings");
            buildingsRoot.transform.SetParent(root.transform);

            CreateBuildingSlot(buildingsRoot, "Slot_CathedralDome", new Vector3(0, 0, 40), "moon2_cathedral_dome");
            CreateBuildingSlot(buildingsRoot, "Slot_BellTower", new Vector3(-30, 0, 15), "moon2_bell_tower");
            CreateBuildingSlot(buildingsRoot, "Slot_Fountain", new Vector3(30, 0, 15), "moon2_fountain");

            // Enemy spawn points
            var enemiesRoot = new GameObject("EnemySpawns");
            enemiesRoot.transform.SetParent(root.transform);

            CreateSpawnPoint(enemiesRoot, "FractalWraith_Spawn_01", new Vector3(-20, 0, 50), 25f);
            CreateSpawnPoint(enemiesRoot, "FractalWraith_Spawn_02", new Vector3(20, 0, 50), 50f);
            CreateSpawnPoint(enemiesRoot, "FractalWraith_Spawn_03", new Vector3(0, 0, -10), 75f);
            CreateSpawnPoint(enemiesRoot, "MirrorWraith_Spawn_Boss", new Vector3(0, 0, 60), 90f);

            // Corruption zones (areas that pulse with dissonance)
            var corruptionRoot = new GameObject("CorruptionZones");
            corruptionRoot.transform.SetParent(root.transform);

            CreateCorruptionZone(corruptionRoot, "Corruption_DomeInterior", new Vector3(0, 0, 40), 15f);
            CreateCorruptionZone(corruptionRoot, "Corruption_TunnelNorth", new Vector3(-10, 0, 55), 8f);
            CreateCorruptionZone(corruptionRoot, "Corruption_FountainApproach", new Vector3(25, 0, 25), 10f);

            // Lighting anchors
            var lightingRoot = new GameObject("Lighting");
            lightingRoot.transform.SetParent(root.transform);

            // Main directional (moonlight -- dim blue)
            var moonLight = new GameObject("MoonLight");
            moonLight.transform.SetParent(lightingRoot.transform);
            moonLight.transform.rotation = Quaternion.Euler(35f, -30f, 0);
            var dl = moonLight.AddComponent<Light>();
            dl.type = LightType.Directional;
            dl.color = new Color(0.3f, 0.35f, 0.5f);
            dl.intensity = 0.4f;

            // Crystal ambient point lights
            CreateCrystalLight(lightingRoot, "CrystalGlow_01", new Vector3(-15, 3, 30), new Color(0.2f, 0.4f, 0.8f));
            CreateCrystalLight(lightingRoot, "CrystalGlow_02", new Vector3(10, 2, 20), new Color(0.3f, 0.5f, 0.7f));
            CreateCrystalLight(lightingRoot, "CrystalGlow_03", new Vector3(5, 4, 50), new Color(0.15f, 0.3f, 0.6f));
            CreateCrystalLight(lightingRoot, "CrystalGlow_Corruption", new Vector3(0, 3, 42), new Color(0.4f, 0.0f, 0.5f));

            // Fog volume anchor
            var fog = new GameObject("FogVolumeAnchor");
            fog.transform.SetParent(lightingRoot.transform);
            fog.transform.localPosition = Vector3.zero;
            // Note: actual URP volume profile assigned via ZoneDefinition SO

            // Interactive triggers
            var triggersRoot = new GameObject("Triggers");
            triggersRoot.transform.SetParent(root.transform);

            CreateTrigger(triggersRoot, "Trigger_EnterMicroGiant", new Vector3(0, 0, 38), 3f,
                "Shrink to enter the cathedral's inner fractal architecture");
            CreateTrigger(triggersRoot, "Trigger_BellSequence", new Vector3(-30, 8, 15), 2f,
                "Ring the bell tower to create a resonance shield");
            CreateTrigger(triggersRoot, "Trigger_FountainActivate", new Vector3(30, 0, 15), 4f,
                "Activate the purification fountain");
            CreateTrigger(triggersRoot, "Trigger_CassianIntro", new Vector3(5, 0, 10), 5f,
                "Cassian appears: charming scholar studying the corruption");

            // Mote collectible positions (Moon 2 golden mote)
            var motesRoot = new GameObject("GoldenMotes");
            motesRoot.transform.SetParent(root.transform);

            CreateMoteSlot(motesRoot, "Mote_Moon2", new Vector3(0, 2, 42));
            CreateMoteSlot(motesRoot, "Mote_Hidden_01", new Vector3(-25, 1, 35));
            CreateMoteSlot(motesRoot, "Mote_Hidden_02", new Vector3(15, 3, 55));

            // Save as prefab
            string prefabPath = $"{PrefabPath}/Moon2_SceneTemplate.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[Tartaria] Moon 2 scene template saved: {prefabPath}");
        }

        // ─── Helpers ─────────────────────────────────

        static void CreateBuilding(BuildingData data)
        {
            string path = $"{BuildingPath}/Building_{data.id}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path) != null) return;

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
            bd.dissolutionDuration = data.dissolutionDuration;

            AssetDatabase.CreateAsset(bd, path);
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

        static void CreateBuildingSlot(GameObject parent, string name, Vector3 pos, string buildingId)
        {
            var slot = new GameObject(name);
            slot.transform.SetParent(parent.transform);
            slot.transform.localPosition = pos;

            // Add a placeholder cube to mark position in editor
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Placeholder";
            vis.transform.SetParent(slot.transform);
            vis.transform.localPosition = Vector3.zero;
            vis.transform.localScale = new Vector3(2, 2, 2);
            // Tag for identification
            var label = new GameObject($"_ID:{buildingId}");
            label.transform.SetParent(slot.transform);
        }

        static void CreateSpawnPoint(GameObject parent, string name, Vector3 pos, float rsThreshold)
        {
            var sp = new GameObject(name);
            sp.transform.SetParent(parent.transform);
            sp.transform.localPosition = pos;

            // Gizmo sphere to show spawn radius in editor
            var label = new GameObject($"_RS_Threshold:{rsThreshold}");
            label.transform.SetParent(sp.transform);
        }

        static void CreateCorruptionZone(GameObject parent, string name, Vector3 pos, float radius)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent.transform);
            zone.transform.localPosition = pos;

            // Trigger collider for corruption area
            var col = zone.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            var label = new GameObject($"_Radius:{radius}");
            label.transform.SetParent(zone.transform);
        }

        static void CreateCrystalLight(GameObject parent, string name, Vector3 pos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = pos;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = 2f;
            light.range = 12f;
        }

        static void CreateTrigger(GameObject parent, string name, Vector3 pos, float radius, string tooltip)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = pos;

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            var label = new GameObject($"_Hint:{tooltip}");
            label.transform.SetParent(go.transform);
        }

        static void CreateMoteSlot(GameObject parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = pos;

            // Gold sphere marker
            var vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vis.name = "MoteMarker";
            vis.transform.SetParent(go.transform);
            vis.transform.localPosition = Vector3.zero;
            vis.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project/Config", "Buildings");
            EnsureFolder("Assets/_Project/Config/Buildings", "Moon2");
            EnsureFolder("Assets/_Project/Prefabs", "Moon2");
            EnsureFolder("Assets/_Project/Materials", "Moon2");
        }

        static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    string[] parts = parent.Split('/');
                    string cur = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = $"{cur}/{parts[i]}";
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(cur, parts[i]);
                        cur = next;
                    }
                }
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        struct BuildingData
        {
            public string id, name, lore;
            public BuildingArchetype archetype;
            public float width, height, aetherStrength, aetherRadius, dissolutionDuration;
            public HarmonicBand band;
            public int nodeCount;
            public TuningPuzzleConfig[] nodes;
        }
    }
}
