using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// TARTARIA Prefab Generator Tool
    /// Automatically creates game system prefabs from KayKit models + Fantasy Ruins
    /// Generates characters, enemies, collectibles, interactive objects, power-ups, props
    /// </summary>
    public class PrefabGeneratorTool : EditorWindow
    {
        private enum GenerationMode
        {
            Moon1Only,
            AllMoons,
            CharactersOnly,
            EnemiesOnly,
            CollectiblesOnly,
            InteractiveOnly,
            PowerUpsOnly,
            PropsOnly
        }
        
        private GenerationMode mode = GenerationMode.Moon1Only;
        private bool createMaterials = true;
        private bool addComponents = true;
        private bool configurePhysics = true;
        private bool assignScripts = true;
        private bool createVariants = true;
        private Vector2 scrollPos;
        
        [MenuItem("Tartaria/5 Asset Database/Prefab Generator", priority = 550)]
        static void ShowWindow()
        {
            var window = GetWindow<PrefabGeneratorTool>("Prefab Generator");
            window.minSize = new Vector2(500, 600);
        }
        
        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("TARTARIA Prefab Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool creates game system prefabs from KayKit models.\n\n" +
                "• Characters (Player, NPCs, Companions)\n" +
                "• Enemies (13 Moon variants)\n" +
                "• Collectibles (Shards, Artifacts, Fragments)\n" +
                "• Interactive Objects (Tuning Nodes, Doors, Crystals)\n" +
                "• Power-Ups (RS Boost, Combat Boost, Healing)\n" +
                "• Props (Candles, Barrels, Rocks)\n\n" +
                "⚠️ This will create ~100 prefabs in Assets/_Project/Prefabs/",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Generation Mode", EditorStyles.boldLabel);
            mode = (GenerationMode)EditorGUILayout.EnumPopup("Mode", mode);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            createMaterials = EditorGUILayout.Toggle("Create Materials", createMaterials);
            addComponents = EditorGUILayout.Toggle("Add Components", addComponents);
            configurePhysics = EditorGUILayout.Toggle("Configure Physics", configurePhysics);
            assignScripts = EditorGUILayout.Toggle("Assign Scripts", assignScripts);
            createVariants = EditorGUILayout.Toggle("Create Moon Variants", createVariants);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("▶ GENERATE PREFABS", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog(
                    "Generate Prefabs",
                    $"This will create prefabs in mode: {mode}\n\n" +
                    "This operation will create new files.\n\n" +
                    "Continue?",
                    "Yes, Generate",
                    "Cancel"
                ))
                {
                    GeneratePrefabs();
                }
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Test: Find KayKit Models", GUILayout.Height(30)))
            {
                TestFindModels();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        void GeneratePrefabs()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Prefab Generator", "Initializing...", 0f);
                
                // Create directory structure
                CreateDirectoryStructure();
                
                switch (mode)
                {
                    case GenerationMode.Moon1Only:
                        GenerateMoon1Prefabs();
                        break;
                    case GenerationMode.AllMoons:
                        GenerateAllMoonsPrefabs();
                        break;
                    case GenerationMode.CharactersOnly:
                        GenerateCharacters();
                        break;
                    case GenerationMode.EnemiesOnly:
                        GenerateEnemies();
                        break;
                    case GenerationMode.CollectiblesOnly:
                        GenerateCollectibles();
                        break;
                    case GenerationMode.InteractiveOnly:
                        GenerateInteractiveObjects();
                        break;
                    case GenerationMode.PowerUpsOnly:
                        GeneratePowerUps();
                        break;
                    case GenerationMode.PropsOnly:
                        GenerateProps();
                        break;
                }
                
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                
                EditorUtility.DisplayDialog(
                    "Prefabs Generated!",
                    "Prefab generation complete.\n\n" +
                    "Check Assets/_Project/Prefabs/ for new files.\n\n" +
                    "Next: Run 'Tartaria → Automated Prefab Wiring'",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Prefab generation failed:\n\n{e.Message}", "OK");
                Debug.LogError($"[PrefabGenerator] Error: {e}");
            }
        }
        
        void GenerateMoon1Prefabs()
        {
            Debug.Log("[PrefabGenerator] ━━━━━━ MOON 1 PREFAB GENERATION ━━━━━━");
            
            GenerateCharacters();
            GenerateEnemies(1);
            GenerateCollectibles(1);
            GenerateInteractiveObjects(1);
            GeneratePowerUps();
            GenerateProps();
            
            Debug.Log("[PrefabGenerator] ✅ Moon 1 prefabs complete!");
        }
        
        void GenerateAllMoonsPrefabs()
        {
            Debug.Log("[PrefabGenerator] ━━━━━━ ALL MOONS PREFAB GENERATION ━━━━━━");
            
            GenerateCharacters();
            
            for (int moon = 1; moon <= 13; moon++)
            {
                EditorUtility.DisplayProgressBar("Prefab Generator", $"Generating Moon {moon}...", moon / 13f);
                GenerateEnemies(moon);
                GenerateCollectibles(moon);
                GenerateInteractiveObjects(moon);
            }
            
            GeneratePowerUps();
            GenerateProps();
            
            Debug.Log("[PrefabGenerator] ✅ All Moons prefabs complete!");
        }
        
        void GenerateCharacters()
        {
            Debug.Log("[PrefabGenerator] Creating characters...");
            
            // Player from Barbarian
            CreateCharacterPrefab("Barbarian", "Player", new Vector3(1f, 2f, 1f), "Player");
            
            // NPCs
            CreateCharacterPrefab("Ranger", "Milo", new Vector3(0.9f, 1.8f, 0.9f), "NPC");
            CreateCharacterPrefab("Mage", "Lirael", new Vector3(0.9f, 1.8f, 0.9f), "NPC");
            CreateCharacterPrefab("Knight", "Cassian", new Vector3(1f, 2f, 1f), "NPC");
            CreateCharacterPrefab("Rogue", "Anastasia", new Vector3(0.9f, 1.8f, 0.9f), "NPC");
            
            Debug.Log("[PrefabGenerator] ✅ Characters created (5 prefabs)");
        }
        
        void GenerateEnemies(int moonNumber = 1)
        {
            Debug.Log($"[PrefabGenerator] Creating enemies for Moon {moonNumber}...");
            
            string enemyName = GetEnemyNameForMoon(moonNumber);
            Color enemyColor = GetEnemyColorForMoon(moonNumber);
            
            // Use Skeleton_Minion as base
            CreateEnemyPrefab($"Skeleton_Minion", enemyName, moonNumber, enemyColor);
            
            Debug.Log($"[PrefabGenerator] ✅ Moon {moonNumber} enemy created: {enemyName}");
        }
        
        void GenerateCollectibles(int moonNumber = 1)
        {
            Debug.Log($"[PrefabGenerator] Creating collectibles for Moon {moonNumber}...");
            
            string primaryName = GetCollectiblePrimaryForMoon(moonNumber);
            string secondaryName = GetCollectibleSecondaryForMoon(moonNumber);
            Color primaryColor = GetCollectiblePrimaryColorForMoon(moonNumber);
            
            // Primary collectible (glowing primitive)
            CreateCollectiblePrefab(primaryName, moonNumber, primaryColor, PrimitiveType.Sphere, 0.3f);
            
            // Secondary collectible (book/tablet)
            CreateCollectiblePrefab(secondaryName, moonNumber, Color.yellow, PrimitiveType.Cube, 0.4f);
            
            Debug.Log($"[PrefabGenerator] ✅ Moon {moonNumber} collectibles created");
        }
        
        void GenerateInteractiveObjects(int moonNumber = 1)
        {
            Debug.Log($"[PrefabGenerator] Creating interactive objects for Moon {moonNumber}...");
            
            string objectName = GetInteractiveObjectForMoon(moonNumber);
            
            // Create from ruin pillar or primitive
            CreateInteractivePrefab(objectName, moonNumber);
            
            Debug.Log($"[PrefabGenerator] ✅ Moon {moonNumber} interactive objects created");
        }
        
        void GeneratePowerUps()
        {
            Debug.Log("[PrefabGenerator] Creating power-ups...");
            
            CreatePowerUpPrefab("RS_Boost", new Color(0f, 1f, 1f), 0.5f);      // Cyan
            CreatePowerUpPrefab("Combat_Boost", new Color(1f, 0.2f, 0.2f), 0.5f); // Red
            CreatePowerUpPrefab("Healing_Orb", new Color(0f, 1f, 0f), 0.5f);   // Green
            
            Debug.Log("[PrefabGenerator] ✅ Power-ups created (3 prefabs)");
        }
        
        void GenerateProps()
        {
            Debug.Log("[PrefabGenerator] Creating environment props...");
            
            // Use KayKit RPG Tools
            CreatePropPrefab("Candle", PrimitiveType.Cylinder, 0.1f, 0.3f);
            CreatePropPrefab("Barrel", PrimitiveType.Cylinder, 0.5f, 1f);
            CreatePropPrefab("Rock", PrimitiveType.Sphere, 0.8f, 0.8f);
            
            Debug.Log("[PrefabGenerator] ✅ Props created (3 prefabs)");
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PREFAB CREATION HELPERS
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        void CreateCharacterPrefab(string sourceModel, string prefabName, Vector3 scale, string tag)
        {
            // Find source model
            string searchPath = $"Assets/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/gltf/{sourceModel}.glb";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(searchPath);
            
            if (modelAsset == null)
            {
                Debug.LogWarning($"[PrefabGenerator] Model not found: {searchPath}");
                // Create placeholder
                CreatePlaceholderCharacter(prefabName, scale, tag);
                return;
            }
            
            // Instantiate in scene
            GameObject character = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            character.name = prefabName;
            character.transform.localScale = scale;
            character.tag = tag;
            
            if (addComponents)
            {
                // Add collider
                var collider = character.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
                
                // Add rigidbody
                var rb = character.AddComponent<Rigidbody>();
                rb.mass = 70f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 0.05f;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                
                if (tag == "Player")
                {
                    character.AddComponent<CharacterController>();
                    // Player script would be added here if it exists
                }
                else
                {
                    // NPC scripts
                    character.AddComponent<Animator>();
                }
            }
            
            // Save as prefab
            string prefabPath = $"Assets/_Project/Prefabs/Characters/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(character, prefabPath);
            DestroyImmediate(character);
            
            Debug.Log($"[PrefabGenerator]   ✅ {prefabName} → {prefabPath}");
        }
        
        void CreatePlaceholderCharacter(string name, Vector3 scale, string tag)
        {
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = name;
            character.transform.localScale = scale;
            character.tag = tag;
            
            // Color code
            var renderer = character.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = tag == "Player" ? Color.blue : Color.green;
            renderer.material = mat;
            
            if (addComponents)
            {
                character.AddComponent<Rigidbody>();
                character.AddComponent<CapsuleCollider>();
            }
            
            string prefabPath = $"Assets/_Project/Prefabs/Characters/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(character, prefabPath);
            DestroyImmediate(character);
            
            Debug.Log($"[PrefabGenerator]   ⚠️ {name} → Placeholder created");
        }
        
        void CreateEnemyPrefab(string sourceModel, string enemyName, int moonNumber, Color enemyColor)
        {
            string searchPath = $"Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE/characters/gltf/{sourceModel}.glb";
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(searchPath);
            
            if (modelAsset == null)
            {
                Debug.LogWarning($"[PrefabGenerator] Enemy model not found: {searchPath}");
                CreatePlaceholderEnemy(enemyName, moonNumber, enemyColor);
                return;
            }
            
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            enemy.name = enemyName;
            enemy.tag = "Enemy";
            
            // Apply color material
            if (createMaterials)
            {
                ApplyColorMaterial(enemy, enemyColor);
            }
            
            if (addComponents)
            {
                var collider = enemy.AddComponent<CapsuleCollider>();
                collider.height = 1.8f;
                collider.radius = 0.4f;
                
                var rb = enemy.AddComponent<Rigidbody>();
                rb.mass = 50f;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                
                var navAgent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
                navAgent.speed = 2.8f;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 1.5f;
            }
            
            string prefabPath = $"Assets/_Project/Prefabs/Enemies/Moon{moonNumber}_{enemyName}/{enemyName}.prefab";
            CreatePrefabDirectory(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            DestroyImmediate(enemy);
            
            Debug.Log($"[PrefabGenerator]   ✅ {enemyName} → {prefabPath}");
        }
        
        void CreatePlaceholderEnemy(string name, int moonNumber, Color color)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.tag = "Enemy";
            
            var renderer = enemy.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
            
            if (addComponents)
            {
                enemy.AddComponent<Rigidbody>();
                enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            }
            
            string prefabPath = $"Assets/_Project/Prefabs/Enemies/Moon{moonNumber}_{name}/{name}.prefab";
            CreatePrefabDirectory(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            DestroyImmediate(enemy);
            
            Debug.Log($"[PrefabGenerator]   ⚠️ {name} → Placeholder");
        }
        
        void CreateCollectiblePrefab(string name, int moonNumber, Color color, PrimitiveType shape, float size)
        {
            GameObject collectible = GameObject.CreatePrimitive(shape);
            collectible.name = name;
            collectible.tag = "Collectible";
            collectible.transform.localScale = Vector3.one * size;
            
            // Glowing material
            var renderer = collectible.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
            renderer.material = mat;
            
            // Remove collider, add trigger
            DestroyImmediate(collectible.GetComponent<Collider>());
            var trigger = collectible.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.5f; // Auto-collect radius
            
            string prefabPath = $"Assets/_Project/Prefabs/Collectibles/{name}/{name}.prefab";
            CreatePrefabDirectory(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(collectible, prefabPath);
            DestroyImmediate(collectible);
            
            Debug.Log($"[PrefabGenerator]   ✅ {name} → {prefabPath}");
        }
        
        void CreateInteractivePrefab(string name, int moonNumber)
        {
            GameObject interactive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            interactive.name = name;
            interactive.tag = "Interactive";
            interactive.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            
            // Glowing material
            var renderer = interactive.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.6f, 0.2f, 0.8f); // Purple
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.6f, 0.2f, 0.8f) * 1.5f);
            renderer.material = mat;
            
            string prefabPath = $"Assets/_Project/Prefabs/Interactive/{name}/{name}.prefab";
            CreatePrefabDirectory(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(interactive, prefabPath);
            DestroyImmediate(interactive);
            
            Debug.Log($"[PrefabGenerator]   ✅ {name} → {prefabPath}");
        }
        
        void CreatePowerUpPrefab(string name, Color color, float size)
        {
            GameObject powerup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            powerup.name = name;
            powerup.tag = "PowerUp";
            powerup.transform.localScale = Vector3.one * size;
            
            var renderer = powerup.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 3f);
            mat.SetFloat("_Metallic", 0.5f);
            renderer.material = mat;
            
            DestroyImmediate(powerup.GetComponent<Collider>());
            var trigger = powerup.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.2f;
            
            string prefabPath = $"Assets/_Project/Prefabs/PowerUps/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(powerup, prefabPath);
            DestroyImmediate(powerup);
            
            Debug.Log($"[PrefabGenerator]   ✅ {name} → {prefabPath}");
        }
        
        void CreatePropPrefab(string name, PrimitiveType shape, float radius, float height)
        {
            GameObject prop = GameObject.CreatePrimitive(shape);
            prop.name = name;
            prop.transform.localScale = new Vector3(radius, height, radius);
            
            string prefabPath = $"Assets/_Project/Prefabs/Props/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prop, prefabPath);
            DestroyImmediate(prop);
            
            Debug.Log($"[PrefabGenerator]   ✅ {name} → {prefabPath}");
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // HELPERS
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        void CreateDirectoryStructure()
        {
            string[] dirs = new string[]
            {
                "Assets/_Project/Prefabs",
                "Assets/_Project/Prefabs/Characters",
                "Assets/_Project/Prefabs/Enemies",
                "Assets/_Project/Prefabs/Collectibles",
                "Assets/_Project/Prefabs/Interactive",
                "Assets/_Project/Prefabs/PowerUps",
                "Assets/_Project/Prefabs/Props"
            };
            
            foreach (string dir in dirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }
        
        void CreatePrefabDirectory(string prefabPath)
        {
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        
        void ApplyColorMaterial(GameObject obj, Color color)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                renderer.material = mat;
            }
        }
        
        void TestFindModels()
        {
            Debug.Log("[PrefabGenerator] ━━━ TESTING MODEL DISCOVERY ━━━");
            
            // Test KayKit Adventurers
            string[] characterModels = new string[] { "Barbarian", "Knight", "Mage", "Ranger", "Rogue" };
            foreach (string model in characterModels)
            {
                string path = $"Assets/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/gltf/{model}.glb";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"  {model}: {(asset != null ? "✅ FOUND" : "❌ NOT FOUND")} at {path}");
            }
            
            // Test KayKit Skeletons
            string[] skeletonModels = new string[] { "Skeleton_Minion", "Skeleton_Warrior", "Skeleton_Rogue", "Skeleton_Mage" };
            foreach (string model in skeletonModels)
            {
                string path = $"Assets/KayKit_Skeletons_1.1_FREE/KayKit_Skeletons_1.1_FREE/characters/gltf/{model}.glb";
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Log($"  {model}: {(asset != null ? "✅ FOUND" : "❌ NOT FOUND")} at {path}");
            }
        }
        
        // Moon-specific data
        string GetEnemyNameForMoon(int moon)
        {
            string[] names = { "MudGolem", "DissonanceDefender", "WindWraith", "MagneticAnomaly",
                "LavaGolem", "CorruptedTome", "TidalGuardian", "VoidEntity", "CorruptedTreeant",
                "ClockworkSoldier", "GhostGladiator", "DimensionalRift", "DissonanceAvatar" };
            return names[moon - 1];
        }
        
        Color GetEnemyColorForMoon(int moon)
        {
            Color[] colors = {
                new Color(0.4f, 0.25f, 0.1f), // Moon 1 mud
                new Color(0.7f, 0.3f, 0.9f),  // Moon 2 purple
                new Color(0.8f, 0.8f, 0.9f),  // Moon 3 white/gray
                new Color(0.2f, 0.6f, 1f),    // Moon 4 blue
                new Color(1f, 0.3f, 0f),      // Moon 5 orange/lava
                new Color(0.9f, 0.85f, 0.7f), // Moon 6 paper
                new Color(0.3f, 0.5f, 0.7f),  // Moon 7 teal
                new Color(0.1f, 0.05f, 0.2f), // Moon 8 void black
                new Color(0.2f, 0.6f, 0.2f),  // Moon 9 green
                new Color(0.6f, 0.55f, 0.4f), // Moon 10 bronze
                new Color(0.5f, 0.5f, 0.6f),  // Moon 11 gray ghost
                new Color(0.8f, 0.2f, 0.8f),  // Moon 12 magenta
                new Color(1f, 0.8f, 0.2f)     // Moon 13 gold
            };
            return colors[moon - 1];
        }
        
        string GetCollectiblePrimaryForMoon(int moon)
        {
            string[] names = { "AetherShard", "CrystalFragment", "WindRune", "PolarShard",
                "ForgedRelic", "KnowledgeFragment", "CoralTablet", "StarFragment", "SeedOfLight",
                "CogOfTime", "VictoryCrown", "NexusCrystal", "HarmonicKey" };
            return names[moon - 1];
        }
        
        string GetCollectibleSecondaryForMoon(int moon)
        {
            string[] names = { "LoreArtifact", "CaveLoreTablet", "TrainManifest", "AuroraLog",
                "SmithingScroll", "AncientManuscript", "WaterloggedDiary", "AstralChart",
                "BotanicalJournal", "ClockmakersDiary", "CombatScroll", "PortalKey", "ZerethMemory" };
            return names[moon - 1];
        }
        
        Color GetCollectiblePrimaryColorForMoon(int moon)
        {
            Color[] colors = {
                new Color(0f, 1f, 1f),        // Moon 1 cyan
                new Color(0.7f, 0.3f, 0.9f),  // Moon 2 purple
                new Color(0.9f, 0.9f, 1f),    // Moon 3 white
                new Color(0.2f, 0.8f, 1f),    // Moon 4 light blue
                new Color(1f, 0.5f, 0f),      // Moon 5 orange
                new Color(1f, 0.9f, 0.7f),    // Moon 6 cream
                new Color(0.2f, 0.7f, 0.9f),  // Moon 7 aqua
                new Color(0.9f, 0.9f, 1f),    // Moon 8 white
                new Color(0.3f, 1f, 0.3f),    // Moon 9 green
                new Color(0.8f, 0.7f, 0.5f),  // Moon 10 bronze
                new Color(0.7f, 0.7f, 0.8f),  // Moon 11 silver
                new Color(1f, 0.2f, 1f),      // Moon 12 magenta
                new Color(1f, 0.9f, 0.3f)     // Moon 13 gold
            };
            return colors[moon - 1];
        }
        
        string GetInteractiveObjectForMoon(int moon)
        {
            string[] names = { "TuningNode", "DissonanceCrystal", "RailSwitch", "MagneticNode",
                "Anvil", "Lectern", "FloodGate", "Telescope", "AncientTree", "GearMechanism",
                "ArenaTrigger", "PortalGate", "FinalNode" };
            return names[moon - 1];
        }
    }
}
