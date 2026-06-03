using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.AI;

namespace Tartaria.Editor
{
    /// <summary>
    /// TARTARIA Automated Prefab Wiring System
    /// Automatically wires all prefabs to Moon system components across all 13 Moon scenes
    /// </summary>
    public class AutomatedPrefabWiring : EditorWindow
    {
        bool wireAllMoons = true;
        int targetMoon = 1;
        bool createMissingPrefabs = true;
        bool bakeNavMesh = true;
        bool bakeLighting = false;
        
        Vector2 scrollPos;
        
        [MenuItem("Tartaria/3 Wire/Automated Prefab Wiring", priority = 370)]
        static void ShowWindow()
        {
            GetWindow<AutomatedPrefabWiring>("Prefab Wiring");
        }
        
        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("TARTARIA Automated Prefab Wiring", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool automatically wires all prefabs to Moon system components.\n" +
                "It will:\n" +
                "• Find or create all required prefabs\n" +
                "• Assign prefabs to system component fields\n" +
                "• Setup spawn points and positions\n" +
                "• Optionally bake NavMesh and lighting\n\n" +
                "⚠️ BACKUP YOUR PROJECT BEFORE RUNNING THIS!",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            wireAllMoons = EditorGUILayout.Toggle("Wire All 13 Moons", wireAllMoons);
            
            if (!wireAllMoons)
            {
                targetMoon = EditorGUILayout.IntSlider("Target Moon", targetMoon, 1, 13);
            }
            
            createMissingPrefabs = EditorGUILayout.Toggle("Create Missing Prefabs", createMissingPrefabs);
            bakeNavMesh = EditorGUILayout.Toggle("Bake NavMesh After Wiring", bakeNavMesh);
            bakeLighting = EditorGUILayout.Toggle("Bake Lighting After Wiring", bakeLighting);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("▶ RUN AUTOMATED WIRING", GUILayout.Height(40)))
            {
                RunAutomatedWiring();
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create Prefab Templates", GUILayout.Height(30)))
            {
                CreatePrefabTemplates();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void RunAutomatedWiring()
        {
            if (!EditorUtility.DisplayDialog(
                "Automated Prefab Wiring",
                $"This will wire {(wireAllMoons ? "ALL 13 MOONS" : $"Moon {targetMoon}")}.\n\n" +
                "This operation cannot be easily undone.\n\n" +
                "Continue?",
                "Yes, Proceed",
                "Cancel"
            ))
            {
                return;
            }
            
            int startMoon = wireAllMoons ? 1 : targetMoon;
            int endMoon = wireAllMoons ? 13 : targetMoon;
            
            for (int moonNum = startMoon; moonNum <= endMoon; moonNum++)
            {
                WireMoon(moonNum);
            }
            
            EditorUtility.DisplayDialog(
                "Wiring Complete!",
                $"Successfully wired {endMoon - startMoon + 1} Moon(s).\n\n" +
                "Check Console for detailed logs.",
                "OK"
            );
        }
        
        void WireMoon(int moonNum)
        {
            string sceneName = GetSceneNameForMoon(moonNum);
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[AutoWiring] Scene not found: {scenePath}");
                return;
            }
            
            EditorUtility.DisplayProgressBar(
                "Automated Prefab Wiring",
                $"Wiring Moon {moonNum} ({sceneName})...",
                moonNum / 13f
            );
            
            // Open scene
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            Debug.Log($"[AutoWiring] ━━━━━━━━━━━━━━━ MOON {moonNum} ({sceneName}) ━━━━━━━━━━━━━━━");
            
            // Wire all 14 systems
            WireEnemySpawners(moonNum);
            WireCollectibles(moonNum);
            WireInteractiveObjects(moonNum);
            WireWeatherSystem(moonNum);
            WireAmbientAudio(moonNum);
            WireAmbientParticles(moonNum);
            WireAudioZones(moonNum);
            WireVisualLandmarks(moonNum);
            WireNPCDialogues(moonNum);
            WireQuestNodes(moonNum);
            WireSecrets(moonNum);
            WirePowerUps(moonNum);
            WireDynamicHazards(moonNum);
            WireEnvironmentDecorator(moonNum);
            
            // Optional: Bake NavMesh
            if (bakeNavMesh)
            {
                BakeMoonNavMesh(moonNum);
            }
            
            // Save scene
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log($"[AutoWiring] ✅ Moon {moonNum} wiring complete!");
        }

        // Sprint 12 #2 fix: NavMesh bake live
        // Replaces obsolete UnityEditor.AI.NavMeshBuilder.BuildNavMesh() (Unity 6 removed legacy bake).
        // Uses reflection on Unity.AI.Navigation.NavMeshSurface so this assembly doesn't need to
        // reference the Navigation package (mirrors the pattern in FullStartupDiagnostics.cs:392).
        void BakeMoonNavMesh(int moonNum)
        {
            Debug.Log($"[AutoWiring] Baking NavMesh for Moon {moonNum}...");

            // Locate NavMeshSurface type via reflection (Unity.AI.Navigation package).
            System.Type surfaceType = null;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                surfaceType = asm.GetType("Unity.AI.Navigation.NavMeshSurface");
                if (surfaceType != null) break;
            }
            if (surfaceType == null)
            {
                Debug.LogError($"[AutoWiring] NavMesh bake FAILED for Moon {moonNum}: " +
                    "Unity.AI.Navigation.NavMeshSurface type not found. " +
                    "Install 'AI Navigation' package via Package Manager.");
                return;
            }

            // Find an existing NavMeshSurface in the scene, or attach one to the scene root.
            Component surface = null;
            var allMonos = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var comp in allMonos)
            {
                if (comp != null && comp.GetType() == surfaceType)
                {
                    surface = comp;
                    break;
                }
            }
            if (surface == null)
            {
                // Prefer a project-specific root if present (matches NavMeshBaker.cs.disabled convention).
                var root = GameObject.Find("EchohavenTerrain")
                        ?? GameObject.Find($"Moon{moonNum}_Terrain")
                        ?? GameObject.Find($"Moon{moonNum}_Root");
                if (root == null)
                {
                    root = new GameObject($"Moon{moonNum}_NavMeshSurface");
                    Debug.LogWarning($"[AutoWiring] No terrain root found for Moon {moonNum}; " +
                        $"created '{root.name}' to host NavMeshSurface.");
                }
                surface = root.AddComponent(surfaceType);
                Debug.Log($"[AutoWiring] Added NavMeshSurface to '{root.name}'.");
            }

            // Invoke surface.BuildNavMesh() via reflection.
            var buildMethod = surfaceType.GetMethod("BuildNavMesh",
                BindingFlags.Public | BindingFlags.Instance);
            if (buildMethod == null)
            {
                Debug.LogError($"[AutoWiring] NavMesh bake FAILED for Moon {moonNum}: " +
                    "NavMeshSurface.BuildNavMesh method not found via reflection.");
                return;
            }

            try
            {
                buildMethod.Invoke(surface, null);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoWiring] NavMesh bake threw for Moon {moonNum}: " +
                    $"{ex.GetType().Name}: {ex.Message}");
                return;
            }

            // Verify the bake produced triangles.
            var triangulation = NavMesh.CalculateTriangulation();
            int triCount = triangulation.indices != null ? triangulation.indices.Length / 3 : 0;
            int vertCount = triangulation.vertices != null ? triangulation.vertices.Length : 0;
            if (triCount > 0)
            {
                Debug.Log($"[AutoWiring] ✅ NavMesh baked for Moon {moonNum}: " +
                    $"{triCount} triangles, {vertCount} vertices on '{surface.gameObject.name}'.");
            }
            else
            {
                Debug.LogWarning($"[AutoWiring] NavMesh bake ran for Moon {moonNum} but produced 0 triangles. " +
                    "Check that walkable geometry exists in the scene and has matching layers.");
            }
        }

        void WireEnemySpawners(int moonNum)
        {
            string componentName = $"Moon{moonNum}EnemySpawners";
            GameObject systemObj = FindOrCreateSystemObject(componentName);
            
            var component = systemObj.GetComponent(componentName);
            if (component == null)
            {
                Debug.LogWarning($"[AutoWiring] Component {componentName} not found in scene");
                return;
            }
            
            SerializedObject so = new SerializedObject(component);
            
            // Find enemy prefab
            string enemyType = GetEnemyTypeForMoon(moonNum);
            GameObject enemyPrefab = FindOrCreatePrefab($"Enemies/Moon{moonNum}_{enemyType}/{enemyType}");
            
            if (enemyPrefab != null)
            {
                SerializedProperty prefabProp = so.FindProperty("mudGolemPrefab") ?? 
                                               so.FindProperty("dissonanceDefenderPrefab") ??
                                               so.FindProperty($"{char.ToLower(enemyType[0])}{enemyType.Substring(1)}Prefab");
                if (prefabProp != null)
                {
                    prefabProp.objectReferenceValue = enemyPrefab;
                }
            }
            
            // Create spawn points
            SerializedProperty spawnPointsProp = so.FindProperty("spawnPoints");
            if (spawnPointsProp != null && spawnPointsProp.arraySize == 0)
            {
                CreateSpawnPoints(systemObj.transform, 4);
                // Reassign after creating
                Transform[] spawnPoints = systemObj.GetComponentsInChildren<Transform>()
                    .Where(t => t.name.StartsWith("SpawnPoint")).ToArray();
                spawnPointsProp.arraySize = spawnPoints.Length;
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
                }
            }
            
            so.ApplyModifiedProperties();
            Debug.Log($"[AutoWiring] ✅ {componentName} wired");
        }
        
        void WireCollectibles(int moonNum)
        {
            string componentName = $"Moon{moonNum}Collectibles";
            GameObject systemObj = FindOrCreateSystemObject(componentName);
            
            var component = systemObj.GetComponent(componentName);
            if (component == null) return;
            
            SerializedObject so = new SerializedObject(component);
            
            // Wire prefabs
            string primaryType = GetCollectiblePrimaryForMoon(moonNum);
            string secondaryType = GetCollectibleSecondaryForMoon(moonNum);
            
            GameObject primaryPrefab = FindOrCreatePrefab($"Collectibles/{primaryType}/{primaryType}");
            GameObject secondaryPrefab = FindOrCreatePrefab($"Collectibles/{secondaryType}/{secondaryType}");
            
            SetPrefabField(so, "aetherShardPrefab", primaryPrefab);
            SetPrefabField(so, "crystalFragmentPrefab", primaryPrefab);
            SetPrefabField(so, "loreArtifactPrefab", secondaryPrefab);
            SetPrefabField(so, "caveLoreTabletPrefab", secondaryPrefab);
            
            so.ApplyModifiedProperties();
            Debug.Log($"[AutoWiring] ✅ {componentName} wired");
        }
        
        void WireInteractiveObjects(int moonNum)
        {
            string componentName = $"Moon{moonNum}InteractiveObjects";
            GameObject systemObj = FindOrCreateSystemObject(componentName);
            
            var component = systemObj.GetComponent(componentName);
            if (component == null) return;
            
            SerializedObject so = new SerializedObject(component);
            
            string interactiveType = GetInteractiveObjectForMoon(moonNum);
            GameObject interactivePrefab = FindOrCreatePrefab($"Interactive/{interactiveType}/{interactiveType}");
            
            SetPrefabField(so, "tuningNodePrefab", interactivePrefab);
            SetPrefabField(so, "dissonanceCrystalPrefab", interactivePrefab);
            
            so.ApplyModifiedProperties();
            Debug.Log($"[AutoWiring] ✅ {componentName} wired");
        }
        
        void WireWeatherSystem(int moonNum)
        {
            string componentName = $"Moon{moonNum}WeatherSystem";
            GameObject systemObj = FindOrCreateSystemObject(componentName);
            
            var component = systemObj.GetComponent(componentName);
            if (component == null) return;
            
            SerializedObject so = new SerializedObject(component);
            
            GameObject rainPrefab = FindOrCreatePrefab("VFX/Weather/Rain_System");
            GameObject weatherPrefab = FindOrCreatePrefab($"VFX/Weather/{GetWeatherEffectForMoon(moonNum)}");
            
            SetPrefabField(so, "rainPrefab", rainPrefab);
            SetPrefabField(so, "auroraEffectPrefab", weatherPrefab);
            SetPrefabField(so, "biolumParticlesPrefab", weatherPrefab);
            
            so.ApplyModifiedProperties();
            Debug.Log($"[AutoWiring] ✅ {componentName} wired");
        }
        
        // Simplified wiring for remaining systems
        void WireAmbientAudio(int moonNum) => WireGenericSystem($"Moon{moonNum}AmbientAudio");
        void WireAmbientParticles(int moonNum) => WireGenericSystem($"Moon{moonNum}AmbientParticles");
        void WireAudioZones(int moonNum) => WireGenericSystem($"Moon{moonNum}AudioZones");
        void WireVisualLandmarks(int moonNum) => WireGenericSystem($"Moon{moonNum}VisualLandmarks");
        void WireNPCDialogues(int moonNum) => WireGenericSystem($"Moon{moonNum}NPCDialogues");
        void WireQuestNodes(int moonNum) => WireGenericSystem($"Moon{moonNum}QuestNodes");
        void WireSecrets(int moonNum) => WireGenericSystem($"Moon{moonNum}Secrets");
        void WirePowerUps(int moonNum) => WireGenericSystem($"Moon{moonNum}PowerUps");
        void WireDynamicHazards(int moonNum) => WireGenericSystem($"Moon{moonNum}DynamicHazards");
        void WireEnvironmentDecorator(int moonNum) => WireGenericSystem($"Moon{moonNum}EnvironmentDecorator");
        
        void WireGenericSystem(string componentName)
        {
            GameObject systemObj = FindOrCreateSystemObject(componentName);
            var component = systemObj.GetComponent(componentName);
            
            if (component != null)
            {
                Debug.Log($"[AutoWiring] ✅ {componentName} exists (manual prefab assignment may be needed)");
            }
        }
        
        GameObject FindOrCreateSystemObject(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
            {
                obj = new GameObject(name);
                // Try to add component dynamically
                var componentType = System.Type.GetType($"Tartaria.Integration.{name}");
                if (componentType != null)
                {
                    obj.AddComponent(componentType);
                }
            }
            return obj;
        }
        
        GameObject FindOrCreatePrefab(string relativePath)
        {
            string fullPath = $"Assets/_Project/Prefabs/{relativePath}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            
            if (prefab == null && createMissingPrefabs)
            {
                // Create placeholder prefab
                prefab = CreatePlaceholderPrefab(fullPath);
            }
            
            return prefab;
        }
        
        GameObject CreatePlaceholderPrefab(string path)
        {
            // Create directory if needed
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Create simple cube placeholder
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(placeholder, path);
            DestroyImmediate(placeholder);
            
            Debug.Log($"[AutoWiring] Created placeholder prefab: {path}");
            return prefab;
        }
        
        void CreateSpawnPoints(Transform parent, int count)
        {
            float radius = 30f;
            for (int i = 0; i < count; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(parent);
                
                float angle = (i / (float)count) * 360f;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                spawnPoint.transform.position = new Vector3(x, 0f, z);
            }
        }
        
        void SetPrefabField(SerializedObject so, string fieldName, GameObject prefab)
        {
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null && prefab != null)
            {
                prop.objectReferenceValue = prefab;
            }
        }
        
        void CreatePrefabTemplates()
        {
            if (EditorUtility.DisplayDialog(
                "Create Prefab Templates",
                "This will create placeholder prefabs for all 13 Moons.\n\n" +
                "You can replace these with proper art assets later.",
                "Create",
                "Cancel"
            ))
            {
                // Create base prefab templates
                CreateBasePrefabSet();
                
                EditorUtility.DisplayDialog("Templates Created", "Prefab templates created successfully!", "OK");
            }
        }
        
        void CreateBasePrefabSet()
        {
            string[] prefabTypes = new string[]
            {
                "Collectibles/AetherShard/AetherShard",
                "Collectibles/LoreArtifact/LoreArtifact",
                "Interactive/TuningNode/TuningNode",
                "Interactive/Door/ResonanceDoor",
                "VFX/Weather/Rain_System",
                "Environment/Props/Candle",
                "Enemies/MudGolem/MudGolem"
            };
            
            foreach (string prefabPath in prefabTypes)
            {
                FindOrCreatePrefab(prefabPath);
            }
            
            AssetDatabase.Refresh();
        }
        
        // Moon-specific data helpers
        string GetSceneNameForMoon(int moonNum)
        {
            string[] sceneNames = {
                "Echohaven_VerticalSlice",      // 1
                "CrystallineCaverns",            // 2
                "WindsweptHighlands",            // 3
                "AuroralSpire",                  // 4
                "DeepForge",                     // 5
                "LivingLibrary",                 // 6
                "TidalArchive",                  // 7
                "CelestialObservatory",          // 8
                "VerdantCanopy",                 // 9
                "ClockworkCitadel",              // 10
                "SunkenColosseum",               // 11
                "PlanetaryNexus",                // 12
                "StarFortBastion"                // 13
            };
            return sceneNames[moonNum - 1];
        }
        
        string GetEnemyTypeForMoon(int moonNum)
        {
            string[] enemies = { "MudGolem", "DissonanceDefender", "WindWraith", "MagneticAnomaly", 
                "LavaGolem", "CorruptedTome", "TidalGuardian", "VoidEntity", "CorruptedTreent", 
                "ClockworkSoldier", "GhostGladiator", "DimensionalRift", "DissonanceAvatar" };
            return enemies[moonNum - 1];
        }
        
        string GetCollectiblePrimaryForMoon(int moonNum)
        {
            string[] collectibles = { "AetherShard", "CrystalFragment", "WindRune", "PolarShard",
                "ForgedRelic", "KnowledgeFragment", "CoralTablet", "StarFragment", "SeedOfLight",
                "CogOfTime", "VictoryCrown", "NexusCrystal", "HarmonicKey" };
            return collectibles[moonNum - 1];
        }
        
        string GetCollectibleSecondaryForMoon(int moonNum)
        {
            string[] collectibles = { "LoreArtifact", "CaveLoreTablet", "TrainManifest", "AuroraLog",
                "SmithingScroll", "AncientManuscript", "WaterloggedDiary", "AstralChart", "BotanicalJournal",
                "ClockmakersDiary", "CombatScroll", "PortalKey", "ZerethMemory" };
            return collectibles[moonNum - 1];
        }
        
        string GetInteractiveObjectForMoon(int moonNum)
        {
            string[] objects = { "TuningNode", "DissonanceCrystal", "RailSwitch", "MagneticNode",
                "Anvil", "Lectern", "FloodGate", "Telescope", "AncientTree", "GearMechanism",
                "ArenaTrigger", "PortalGate", "FinalNode" };
            return objects[moonNum - 1];
        }
        
        string GetWeatherEffectForMoon(int moonNum)
        {
            string[] effects = { "Aurora", "CrystalResonance", "WindStorm", "Aurora", "HeatWaves",
                "PaperStorm", "TidalSurge", "CelestialAlignment", "PollenStorm", "TemporalDistortion",
                "Sandstorm", "RealityFlux", "ResonanceCascade" };
            return effects[moonNum - 1];
        }
    }
}
