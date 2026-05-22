using UnityEngine;
using Tartaria.Integration;
using Tartaria.AI;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Tests
{
    /// <summary>
    /// Core Loop Test Scene Setup — Programmatic scene population for validation.
    /// 
    /// Creates a minimal playable environment with:
    /// - Player spawn (with PlayerInputHandler)
    /// - 3 buildings (InteractableBuilding with tuning nodes)
    /// - 5 enemies (MudGolem with 300 HP)
    /// - Collectibles (RS crystals, items)
    /// - Ground plane
    /// 
    /// Usage:
    /// 1. Create empty scene: CoreLoopTestScene.unity
    /// 2. Add GameObject → attach CoreLoopTestSceneSetup
    /// 3. Right-click → "Setup Test Scene" (Context Menu)
    /// 4. Save scene
    /// 5. Attach CoreLoopValidator for automated testing
    /// 
    /// Layout:
    /// - Player at origin (0, 0, 0)
    /// - Buildings in triangle: (-10, 0, 0), (10, 0, 0), (0, 0, 15)
    /// - Enemies scattered: 5m radius around buildings
    /// - Collectibles: between player and buildings
    /// </summary>
    public class CoreLoopTestSceneSetup : MonoBehaviour
    {
        [Header("Prefab References (Auto-load from Resources)")]
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject buildingPrefab;
        [SerializeField] GameObject golemPrefab;
        [SerializeField] GameObject collectiblePrefab;
        
        [Header("Scene Configuration")]
        [SerializeField] bool createGround = true;
        [SerializeField] bool createLighting = true;
        [SerializeField] bool createCamera = true;
        
        [Header("Layout Settings")]
        [SerializeField] Vector3 playerSpawnPosition = Vector3.zero;
        [SerializeField] float buildingSpacing = 10f;
        [SerializeField] int enemyCount = 5;
        [SerializeField] int collectibleCount = 8;
        
        [ContextMenu("Setup Test Scene")]
        public void SetupTestScene()
        {
            Debug.Log("[TestSetup] Starting Core Loop Test Scene setup...");
            
            // Load resources if not assigned
            LoadPrefabs();
            
            // Create scene elements
            CreateGround();
            CreateLighting();
            CreateCamera();
            CreatePlayer();
            CreateBuildings();
            CreateEnemies();
            CreateCollectibles();
            CreateSystems();
            
            Debug.Log("[TestSetup] ✓ Scene setup complete!");
        }
        
        void LoadPrefabs()
        {
            if (playerPrefab == null)
            {
                // Try to find player prefab
                playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/Char_Knight");
                if (playerPrefab == null)
                {
                    Debug.LogWarning("[TestSetup] Player prefab not found — will create placeholder");
                }
            }
            
            if (buildingPrefab == null)
            {
                // Try to find building prefab
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/TownHall");
                if (buildingPrefab == null)
                {
                    Debug.LogWarning("[TestSetup] Building prefab not found — will create placeholder");
                }
            }
            
            if (golemPrefab == null)
            {
                golemPrefab = Resources.Load<GameObject>("Prefabs/Characters/MudGolem");
                if (golemPrefab == null)
                {
                    Debug.LogWarning("[TestSetup] Golem prefab not found — will create placeholder");
                }
            }
            
            if (collectiblePrefab == null)
            {
                // Use generic pickup
                collectiblePrefab = Resources.Load<GameObject>("Prefabs/Pickups/ResonanceCrystal");
            }
        }
        
        void CreateGround()
        {
            if (!createGround) return;
            
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);  // 100x100m
            
            // Gray material
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.35f, 0.3f);
            ground.GetComponent<Renderer>().material = mat;
            
            Debug.Log("[TestSetup] Created ground plane");
        }
        
        void CreateLighting()
        {
            if (!createLighting) return;
            
            // Directional light
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Debug.Log("[TestSetup] Created directional light");
        }
        
        void CreateCamera()
        {
            if (!createCamera) return;
            
            // Check if main camera exists
            if (Camera.main != null)
            {
                Debug.Log("[TestSetup] Main camera already exists");
                return;
            }
            
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 15f, -20f);
            camGO.transform.LookAt(Vector3.zero);
            
            Debug.Log("[TestSetup] Created main camera");
        }
        
        void CreatePlayer()
        {
            GameObject player;
            
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
                player.name = "Player";
            }
            else
            {
                // Create placeholder player
                player = new GameObject("Player");
                player.transform.position = playerSpawnPosition;
                
                // Add capsule visual
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "PlayerMesh";
                capsule.transform.SetParent(player.transform);
                capsule.transform.localPosition = Vector3.up;
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.6f, 1f);  // Blue
                capsule.GetComponent<Renderer>().material = mat;
                
                // Add controller
                player.AddComponent<CharacterController>();
            }
            
            // Ensure PlayerInputHandler exists
            if (player.GetComponent<PlayerInputHandler>() == null)
            {
                player.AddComponent<PlayerInputHandler>();
            }
            
            // Tag
            player.tag = "Player";
            
            Debug.Log($"[TestSetup] Created player at {playerSpawnPosition}");
        }
        
        void CreateBuildings()
        {
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-buildingSpacing, 0f, 0f),
                new Vector3(buildingSpacing, 0f, 0f),
                new Vector3(0f, 0f, buildingSpacing * 1.5f)
            };
            
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject building;
                
                if (buildingPrefab != null)
                {
                    building = Instantiate(buildingPrefab, positions[i], Quaternion.identity);
                    building.name = $"Building_{i + 1}";
                }
                else
                {
                    // Create placeholder building
                    building = new GameObject($"Building_{i + 1}");
                    building.transform.position = positions[i];
                    
                    // Add cube visual
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "BuildingMesh";
                    cube.transform.SetParent(building.transform);
                    cube.transform.localPosition = Vector3.up * 2.5f;
                    cube.transform.localScale = new Vector3(5f, 5f, 5f);
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.7f, 0.5f, 0.3f);  // Brown
                    cube.GetComponent<Renderer>().material = mat;
                    
                    // Add collider
                    var collider = building.AddComponent<BoxCollider>();
                    collider.center = Vector3.up * 2.5f;
                    collider.size = new Vector3(5f, 5f, 5f);
                }
                
                // Ensure InteractableBuilding component
                if (building.GetComponent<InteractableBuilding>() == null)
                {
                    var interactable = building.AddComponent<InteractableBuilding>();
                    // Set building ID
                    typeof(InteractableBuilding)
                        .GetField("buildingId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(interactable, $"test_building_{i + 1}");
                }
                
                // Layer
                building.layer = LayerMask.NameToLayer("Interactable") != -1 
                    ? LayerMask.NameToLayer("Interactable") 
                    : 0;
            }
            
            Debug.Log($"[TestSetup] Created {positions.Length} buildings");
        }
        
        void CreateEnemies()
        {
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-5f, 0f, 3f),
                new Vector3(5f, 0f, 3f),
                new Vector3(0f, 0f, 8f),
                new Vector3(-7f, 0f, 10f),
                new Vector3(7f, 0f, 10f)
            };
            
            for (int i = 0; i < Mathf.Min(enemyCount, positions.Length); i++)
            {
                GameObject golem;
                
                if (golemPrefab != null)
                {
                    golem = Instantiate(golemPrefab, positions[i], Quaternion.identity);
                    golem.name = $"Golem_{i + 1}";
                }
                else
                {
                    // Create placeholder golem
                    golem = new GameObject($"Golem_{i + 1}");
                    golem.transform.position = positions[i];
                    
                    // Add sphere visual
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.name = "GolemMesh";
                    sphere.transform.SetParent(golem.transform);
                    sphere.transform.localPosition = Vector3.up;
                    sphere.transform.localScale = Vector3.one * 2f;
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.4f, 0.25f, 0.15f);  // Mud brown
                    sphere.GetComponent<Renderer>().material = mat;
                    
                    // Add capsule collider
                    var collider = golem.AddComponent<CapsuleCollider>();
                    collider.center = Vector3.up;
                    collider.radius = 1f;
                    collider.height = 2f;
                }
                
                // Ensure MudGolemHealth component with proper HP
                var health = golem.GetComponent<MudGolemHealth>();
                if (health == null)
                {
                    health = golem.AddComponent<MudGolemHealth>();
                }
                
                // Set max health via reflection (private field)
                var maxHealthField = typeof(MudGolemHealth)
                    .GetField("_maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maxHealthField != null)
                {
                    maxHealthField.SetValue(health, CombatBalance.DefaultEnemyHP);
                }
                
                // Set current health
                var currentHealthField = typeof(MudGolemHealth)
                    .GetField("_currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (currentHealthField != null)
                {
                    currentHealthField.SetValue(health, CombatBalance.DefaultEnemyHP);
                }
                
                // Tag
                golem.tag = "Enemy";
            }
            
            Debug.Log($"[TestSetup] Created {Mathf.Min(enemyCount, positions.Length)} enemies (HP: {CombatBalance.DefaultEnemyHP})");
        }
        
        void CreateCollectibles()
        {
            // Scatter collectibles between player and buildings
            for (int i = 0; i < collectibleCount; i++)
            {
                float angle = (i / (float)collectibleCount) * Mathf.PI * 2f;
                float radius = Random.Range(3f, 7f);
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0.5f,
                    Mathf.Sin(angle) * radius
                );
                
                GameObject collectible;
                
                if (collectiblePrefab != null)
                {
                    collectible = Instantiate(collectiblePrefab, pos, Quaternion.identity);
                    collectible.name = $"Collectible_{i + 1}";
                }
                else
                {
                    // Create placeholder collectible (rotating crystal)
                    collectible = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    collectible.name = $"Collectible_{i + 1}";
                    collectible.transform.position = pos;
                    collectible.transform.localScale = Vector3.one * 0.5f;
                    collectible.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
                    
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(1f, 0.8f, 0.2f);  // Gold
                    mat.SetFloat("_Metallic", 0.8f);
                    collectible.GetComponent<Renderer>().material = mat;
                    
                    // Make trigger
                    var collider = collectible.GetComponent<Collider>();
                    collider.isTrigger = true;
                    
                    // Add rotation animation
                    var rotator = collectible.AddComponent<TestCollectibleRotator>();
                }
                
                // Layer
                collectible.layer = LayerMask.NameToLayer("Interactable") != -1 
                    ? LayerMask.NameToLayer("Interactable") 
                    : 0;
            }
            
            Debug.Log($"[TestSetup] Created {collectibleCount} collectibles");
        }
        
        void CreateSystems()
        {
            // Create singleton managers if not present
            
            // ResonanceScore
            if (ResonanceScore.Instance == null)
            {
                var rsGO = new GameObject("ResonanceScore");
                rsGO.AddComponent<ResonanceScore>();
            }
            
            // PlayerProgression
            if (PlayerProgression.Instance == null)
            {
                var progGO = new GameObject("PlayerProgression");
                progGO.AddComponent<PlayerProgression>();
            }
            
            // InventorySystem
            if (InventorySystem.Instance == null)
            {
                var invGO = new GameObject("InventorySystem");
                invGO.AddComponent<InventorySystem>();
            }
            
            // QuestManager
            if (QuestManager.Instance == null)
            {
                var questGO = new GameObject("QuestManager");
                questGO.AddComponent<QuestManager>();
            }
            
            // SaveManager
            if (SaveManager.Instance == null)
            {
                var saveGO = new GameObject("SaveManager");
                saveGO.AddComponent<SaveManager>();
            }
            
            Debug.Log("[TestSetup] Created system singletons");
        }
    }
    
    /// <summary>
    /// Simple component to rotate collectibles for visual feedback.
    /// </summary>
    public class TestCollectibleRotator : MonoBehaviour
    {
        [SerializeField] float rotationSpeed = 45f;
        [SerializeField] float bobSpeed = 2f;
        [SerializeField] float bobHeight = 0.3f;
        
        Vector3 _startPos;
        
        void Start()
        {
            _startPos = transform.position;
        }
        
        void Update()
        {
            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            
            // Bob up/down
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = _startPos + Vector3.up * bobOffset;
        }
    }
}
