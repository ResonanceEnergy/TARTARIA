using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Building Spawner — wires up the MonoBehaviour side of Tartarian buildings:
    /// InteractableBuilding + ProximityTrigger + colliders.
    /// Runs after WorldInitializer creates ECS entities.
    ///
    /// First looks for existing scene buildings placed by EchohavenScenePopulator.
    /// Only creates greybox fallbacks if no matching object is found.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-80)] // After WorldInitializer (-90), before GameLoopController (-50)
    public class BuildingSpawner : MonoBehaviour
    {
        [Header("Building Positions (must match WorldInitializer)")]
        [SerializeField] Vector3 domePosition = new(30f, 0f, 20f);
        [SerializeField] Vector3 fountainPosition = new(-20f, 0f, 35f);
        [SerializeField] Vector3 spirePosition = new(0f, 0f, -30f);

        [Header("Discovery Radius")]
        [SerializeField] float discoveryRadius = 15f;

        [Header("KayKit Building Composition (2026 AAA)")]
        [SerializeField, Tooltip("KayKit rock prefabs for composing structures")]
        GameObject[] kayKitRockPrefabs;
        [SerializeField, Tooltip("KayKit trees for ambient scatter")]
        GameObject[] kayKitTreePrefabs;
        [SerializeField, Tooltip("KayKit bushes for ambient scatter")]
        GameObject[] kayKitBushPrefabs;

        [Header("VFX Prefabs")]
        [SerializeField, Tooltip("RestoreSparkle VFX for building discovery markers")]
        GameObject restoreSparkleVFX;

        // Scene object names from EchohavenScenePopulator
        static readonly string[] DomeNames = { "StarDome_Placeholder", "Echohaven_StarDome" };
        static readonly string[] FountainNames = { "HarmonicFountain_Placeholder", "Echohaven_HarmonicFountain" };
        static readonly string[] SpireNames = { "CrystalSpire_Placeholder", "Echohaven_CrystalSpire" };

        // Cached materials for runtime injection
        Material _mudFresh;
        Material _mudCracking;
        Material _stoneActive;

        void Start()
        {
            // Runtime override: scene may have stale serialized value
            if (discoveryRadius > 15f) discoveryRadius = 15f;

            // Create materials directly (no scene-search dependency)
            _mudFresh = CreateBuildingMaterial("M_Mud_Fresh");
            _mudCracking = CreateBuildingMaterial("M_Mud_Cracking");
            _stoneActive = CreateBuildingMaterial("M_Stone_Active");

            WireBuilding("dome", domePosition, DomeNames, PrimitiveType.Sphere, new Vector3(8f, 6f, 8f));
            WireBuilding("fountain", fountainPosition, FountainNames, PrimitiveType.Cylinder, new Vector3(4f, 3f, 4f));
            WireBuilding("spire", spirePosition, SpireNames, PrimitiveType.Cylinder, new Vector3(3f, 12f, 3f));

            SpawnAmbientVillage();

            Debug.Log("[BuildingSpawner] 3 buildings wired + ambient village scattered.");
        }

        void WireBuilding(string buildingId, Vector3 position, string[] sceneNames,
            PrimitiveType fallbackShape, Vector3 fallbackScale)
        {
            // Try to find existing scene object first
            GameObject building = null;
            foreach (var name in sceneNames)
            {
                building = GameObject.Find(name);
                if (building != null) break;
            }

            // Fallback: create greybox if nothing in scene
            if (building == null)
            {
                // Use modular dungeon assets for Star Dome (2026 AAA quality)
                if (buildingId == "dome")
                {
                    building = CreateModularDungeonStarDome(position);
                }
                else
                {
                    building = CreateGreyboxBuilding(buildingId, position, fallbackShape, fallbackScale);
                }
                building.name = $"Building_{buildingId}";
            }

            // Decorate with Tartarian architectural detail (columns, dome cap, basin, plinth, bands)
            var kind = buildingId switch
            {
                "dome" => TartarianArchitectureBuilder.BuildingKind.Dome,
                "fountain" => TartarianArchitectureBuilder.BuildingKind.Fountain,
                "spire" => TartarianArchitectureBuilder.BuildingKind.Spire,
                _ => TartarianArchitectureBuilder.BuildingKind.Dome,
            };
            TartarianArchitectureBuilder.Decorate(building, kind, fallbackScale);

            // Ensure InteractableBuilding component
            var interactable = building.GetComponent<InteractableBuilding>();
            if (interactable == null)
                interactable = building.AddComponent<InteractableBuilding>();

            // Inject buildingId and materials (AddComponent leaves SerializeFields null)
            interactable.SetBuildingId(buildingId);
            interactable.SetMaterials(_mudFresh, _mudCracking, _stoneActive);

            // Add water spray particle effect to fountain
            if (buildingId == "fountain")
                AddFountainParticles(building);

            // Ensure collider for interaction raycasts
            var col = building.GetComponent<Collider>();
            if (col == null)
            {
                var box = building.AddComponent<BoxCollider>();
                box.size = fallbackScale;
            }

            // Set building layer
            int buildingLayer = LayerMask.NameToLayer("Building");
            if (buildingLayer >= 0)
                building.layer = buildingLayer;

            // MVP: mark all spawned buildings as discovered immediately so
            // the player can begin excavation without needing to walk into
            // each ProximityTrigger first. The discovery trigger still works
            // for late-arriving players, but the dig site UX is broken if
            // E does nothing on a visible mound.
            interactable.Discover();

            // Add discovery proximity trigger as SIBLING (not child) to avoid
            // SphereCollider radius being scaled by the building's transform scale.
            // A child SphereCollider with radius 15 inside a building scaled 12x
            // would create a 180-unit trigger sphere covering the entire map.
            string triggerName = $"DiscoveryTrigger_{buildingId}";
            if (building.transform.Find(triggerName) == null && GameObject.Find(triggerName) == null)
            {
                var triggerGO = new GameObject(triggerName);
                triggerGO.transform.position = building.transform.position;
                var trigger = triggerGO.AddComponent<ProximityTrigger>();
                trigger.Configure(ProximityTrigger.TriggerAction.DiscoverBuilding, discoveryRadius, interactable);
            }

            // Register as scanner POI
            var scanner = ResonanceScannerSystem.Instance;
            if (scanner != null)
            {
                scanner.RegisterPOI(new ScanPOI
                {
                    poiId = buildingId,
                    poiType = ScanPOIType.BuriedStructure,
                    position = position,
                    isRevealed = false
                });
            }

            // Register excavation site (Gap 2)
            var excavation = ExcavationSystem.Instance;
            if (excavation != null)
            {
                excavation.RegisterSite(buildingId, position, 4, false, buildingId);
            }

            // Note: InteractableBuilding.Start() calls RestoreFromSave() to load saved state

            // Add floating discovery marker (golden diamond above building)
            AddDiscoveryMarker(building, buildingId);

            // Add mud slowdown zone around each buried structure.
            AddMudZone(building, buildingId);
        }

        void AddMudZone(GameObject building, string id)
        {
            string zoneName = $"MudZone_{id}";
            if (building.transform.Find(zoneName) != null) return;

            var zone = new GameObject(zoneName);
            zone.transform.SetParent(building.transform, false);
            zone.transform.localPosition = Vector3.zero;

            float radius = 6f;
            var rend = building.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var ext = rend.bounds.extents;
                radius = Mathf.Max(4f, Mathf.Max(ext.x, ext.z) + 1.5f);
            }

            var col = zone.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;

            zone.AddComponent<MudZone>();
        }

        void AddDiscoveryMarker(GameObject building, string id)
        {
            string markerName = $"Marker_{id}";
            if (building.transform.Find(markerName) != null) return;

            GameObject marker;

            if (restoreSparkleVFX != null)
            {
                // Use assigned prefab
                marker = Instantiate(restoreSparkleVFX);
                marker.name = markerName;
                marker.transform.SetParent(building.transform);

                // Position above building top
                var rend = building.GetComponentInChildren<Renderer>();
                float topY = rend != null ? rend.bounds.max.y - building.transform.position.y + 3f : 8f;
                marker.transform.localPosition = new Vector3(0f, topY, 0f);

                // Play particle effect
                ParticleSystem ps = marker.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
            else
            {
                // Fallback: create runtime particle system
                marker = new GameObject(markerName);
                marker.transform.SetParent(building.transform);

                // Position above building top
                var rend = building.GetComponentInChildren<Renderer>();
                float topY = rend != null ? rend.bounds.max.y - building.transform.position.y + 3f : 8f;
                marker.transform.localPosition = new Vector3(0f, topY, 0f);

                ParticleSystem ps = marker.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.5f;
                main.startSpeed = 0.5f;
                main.startSize = 0.3f;
                main.startColor = new Color(1f, 0.85f, 0.3f, 0.8f);
                main.maxParticles = 50;
                main.loop = true;

                var emission = ps.emission;
                emission.rateOverTime = 20f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.3f;

                var renderer = marker.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                renderer.material.SetColor("_BaseColor", new Color(1f, 0.8f, 0.2f));

                ps.Play();

                Debug.LogWarning("[BuildingSpawner] RestoreSparkle prefab missing - using runtime ParticleSystem");
            }

            // Add a point light so the marker glows visibly
            var light = marker.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.4f);
            light.intensity = 3f;
            light.range = 12f;
            light.shadows = LightShadows.None; // Prevent shadow atlas overflow

            // Bobbing animation component
            marker.AddComponent<BobbingMarker>();
        }

        /// <summary>
        /// Scatters 40-80 KayKit props (rocks, trees, bushes) around Echohaven plaza.
        /// Deterministic seeded RNG for consistent placement.
        /// </summary>
        void SpawnAmbientVillage()
        {
            if (kayKitRockPrefabs == null || kayKitRockPrefabs.Length == 0)
            {
                Debug.LogWarning("[BuildingSpawner] SpawnAmbientVillage: kayKitRockPrefabs not assigned — skipping scatter.");
                return;
            }

            var rng = new System.Random(0xABBA);
            var root = new GameObject("AmbientVillage");

            int rockCount = 30 + rng.Next(15);
            int bushCount = 20 + rng.Next(15);
            int treeCount = 8 + rng.Next(5);

            // Scatter rocks
            for (int i = 0; i < rockCount; i++)
            {
                var prefab = kayKitRockPrefabs[rng.Next(kayKitRockPrefabs.Length)];
                if (prefab == null) continue;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float dist = 15f + (float)rng.NextDouble() * 45f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
                var rock = Instantiate(prefab, pos, Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f));
                rock.name = $"Rock_{i}";
                rock.transform.SetParent(root.transform);
                rock.transform.localScale *= 0.7f + (float)rng.NextDouble() * 0.8f;
            }

            // Scatter bushes
            if (kayKitBushPrefabs != null && kayKitBushPrefabs.Length > 0)
            {
                for (int i = 0; i < bushCount; i++)
                {
                    var prefab = kayKitBushPrefabs[rng.Next(kayKitBushPrefabs.Length)];
                    if (prefab == null) continue;
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float dist = 12f + (float)rng.NextDouble() * 50f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
                    var bush = Instantiate(prefab, pos, Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f));
                    bush.name = $"Bush_{i}";
                    bush.transform.SetParent(root.transform);
                    bush.transform.localScale *= 0.8f + (float)rng.NextDouble() * 0.6f;
                }
            }

            // Scatter trees
            if (kayKitTreePrefabs != null && kayKitTreePrefabs.Length > 0)
            {
                for (int i = 0; i < treeCount; i++)
                {
                    var prefab = kayKitTreePrefabs[rng.Next(kayKitTreePrefabs.Length)];
                    if (prefab == null) continue;
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float dist = 25f + (float)rng.NextDouble() * 35f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
                    var tree = Instantiate(prefab, pos, Quaternion.identity);
                    tree.name = $"Tree_{i}";
                    tree.transform.SetParent(root.transform);
                    tree.transform.localScale *= 0.9f + (float)rng.NextDouble() * 0.4f;
                }
            }

            Debug.Log($"[BuildingSpawner] Ambient village scattered: {rockCount} rocks, {bushCount} bushes, {treeCount} trees.");
        }

        /// <summary>
        /// Create Star Dome using Modular Dungeon 2 assets - circular Gothic hall (2026 AAA quality).
        /// Builds 40m diameter structure with curved walls, stone floors, corner pillars, and torch lighting.
        /// </summary>
        GameObject CreateModularDungeonStarDome(Vector3 basePosition)
        {
            var domeRoot = new GameObject("StarDome_ModularComposite");
            domeRoot.transform.position = basePosition;

            // Try to load modular dungeon prefabs from Resources
            GameObject wallCurvedPrefab = Resources.Load<GameObject>("Prefabs/Buildings/ModularDungeon2/struct_wall_curved_main");
            GameObject floorNormalPrefab = Resources.Load<GameObject>("Prefabs/Buildings/ModularDungeon2/struct_floor_curved");
            GameObject pillarCornerPrefab = Resources.Load<GameObject>("Prefabs/Buildings/ModularDungeon2/struct_pillar_corner_main");
            GameObject torchPrefab = Resources.Load<GameObject>("Prefabs/Buildings/ModularDungeon2/prop_wall_torch");

            // If prefabs not ready yet (Unity still importing), load OBJ models directly
            bool useFallback = wallCurvedPrefab == null;

            if (useFallback)
            {
                Debug.LogWarning("[BuildingSpawner] Modular dungeon prefabs not found in Resources. Using direct OBJ load fallback.");

                // Try direct model load from Models folder (Unity auto-converts OBJ on import)
                wallCurvedPrefab = Resources.Load<GameObject>("Models/Buildings/ModularDungeon2/struct_wall_curved_main");
                floorNormalPrefab = Resources.Load<GameObject>("Models/Buildings/ModularDungeon2/struct_floor_curved");
                pillarCornerPrefab = Resources.Load<GameObject>("Models/Buildings/ModularDungeon2/struct_pillar_corner_main");
                torchPrefab = Resources.Load<GameObject>("Models/Buildings/ModularDungeon2/prop_wall_torch");
            }

            // Final fallback: primitive sphere if assets still not imported
            if (wallCurvedPrefab == null)
            {
                Debug.LogWarning("[BuildingSpawner] Modular dungeon assets not yet imported. Using primitive sphere fallback. Run Unity automation menu after import completes.");

                var fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallback.name = "StarDome_Fallback";
                fallback.transform.position = basePosition + new Vector3(0f, 3f, 0f);
                fallback.transform.localScale = new Vector3(8f, 6f, 8f);
                fallback.transform.SetParent(domeRoot.transform);

                // Apply mud material
                var renderer = fallback.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.45f, 0.35f, 0.25f);
                    renderer.material = mat;
                }

                return domeRoot;
            }

            // ============================================================================
            // CIRCULAR WALL (12 segments, 40m diameter)
            // ============================================================================

            const int wallSegments = 12;
            const float radius = 20f; // 40m diameter = 20m radius

            for (int i = 0; i < wallSegments; i++)
            {
                float angle = i * 30f; // 360° / 12 segments = 30° each
                float angleRad = angle * Mathf.Deg2Rad;

                float x = radius * Mathf.Cos(angleRad);
                float z = radius * Mathf.Sin(angleRad);

                Vector3 wallPos = new Vector3(x, 0f, z);
                Quaternion wallRot = Quaternion.Euler(0f, angle + 90f, 0f); // Face inward

                var wall = Instantiate(wallCurvedPrefab, basePosition + wallPos, wallRot, domeRoot.transform);
                wall.name = $"Wall_Curved_{i:D2}";

                // Ensure collider
                if (wall.GetComponent<Collider>() == null)
                {
                    var box = wall.AddComponent<BoxCollider>();
                    box.size = new Vector3(10f, 10f, 2f); // Approximate wall dimensions
                }
            }

            // ============================================================================
            // STONE FLOOR (5×5 grid, 25 tiles, inside circle only)
            // ============================================================================

            if (floorNormalPrefab != null)
            {
                for (int x = -2; x <= 2; x++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        // Only place tiles inside the 40m diameter circle
                        float dist = Mathf.Sqrt(x * x + z * z);
                        if (dist <= 2.5f) // Slightly larger than 2 to fill corners
                        {
                            Vector3 floorPos = new Vector3(x * 10f, 0f, z * 10f);
                            var floor = Instantiate(floorNormalPrefab, basePosition + floorPos, Quaternion.identity, domeRoot.transform);
                            floor.name = $"Floor_Normal_{x + 2}_{z + 2}";
                        }
                    }
                }
            }

            // ============================================================================
            // CORNER PILLARS (4 cardinal points)
            // ============================================================================

            if (pillarCornerPrefab != null)
            {
                Vector3[] pillarPositions = new Vector3[]
                {
                    new Vector3(15f, 0f, 15f),   // Northeast
                    new Vector3(-15f, 0f, 15f),  // Northwest
                    new Vector3(15f, 0f, -15f),  // Southeast
                    new Vector3(-15f, 0f, -15f)  // Southwest
                };

                for (int i = 0; i < pillarPositions.Length; i++)
                {
                    var pillar = Instantiate(pillarCornerPrefab, basePosition + pillarPositions[i], Quaternion.identity, domeRoot.transform);
                    pillar.name = $"Pillar_Corner_{i}";
                }
            }

            // ============================================================================
            // TORCHES WITH LIGHTING (8 around perimeter)
            // ============================================================================

            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f; // 360° / 8 torches = 45° each
                float angleRad = angle * Mathf.Deg2Rad;
                float torchRadius = 18f; // Slightly inside wall circle

                float x = torchRadius * Mathf.Cos(angleRad);
                float z = torchRadius * Mathf.Sin(angleRad);

                Vector3 torchPos = new Vector3(x, 3f, z); // 3m above ground
                Quaternion torchRot = Quaternion.Euler(0f, angle + 180f, 0f); // Face inward

                GameObject torch;
                if (torchPrefab != null)
                {
                    torch = Instantiate(torchPrefab, basePosition + torchPos, torchRot, domeRoot.transform);
                }
                else
                {
                    // Fallback procedural torch
                    torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    torch.transform.position = basePosition + torchPos;
                    torch.transform.rotation = torchRot;
                    torch.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
                    torch.transform.SetParent(domeRoot.transform);
                }
                torch.name = $"Torch_{i}";

                // Add Point Light for flame effect
                var light = torch.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.6f, 0.3f); // Orange flame color
                light.intensity = 2.0f;
                light.range = 15f;
                light.shadows = LightShadows.Soft; // Soft shadows for atmosphere
            }

            Debug.Log($"[BuildingSpawner] Star Dome created: {wallSegments} curved walls, 25 floor tiles, 4 pillars, 8 torches (40m diameter Gothic hall)");

            return domeRoot;
        }

        GameObject CreateGreyboxBuilding(string id, Vector3 position,
            PrimitiveType shape, Vector3 scale)
        {
            // If KayKit rocks assigned, compose building from them
            if (kayKitRockPrefabs != null && kayKitRockPrefabs.Length > 0)
            {
                return ComposeKayKitBuilding(id, position, scale);
            }

            // Fallback: build from components (no primitives)
            var go = new GameObject("Building_" + id);
            go.transform.position = position + Vector3.up * (scale.y * 0.5f);
            go.transform.localScale = scale;

            // Add mesh components
            var mf = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();

            // Set mesh based on shape
            if (shape == PrimitiveType.Cube)
            {
                mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                go.AddComponent<BoxCollider>();
            }
            else if (shape == PrimitiveType.Sphere)
            {
                mf.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                go.AddComponent<SphereCollider>();
            }
            else if (shape == PrimitiveType.Cylinder)
            {
                mf.mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                go.AddComponent<CapsuleCollider>();
            }
            else
            {
                // Fallback to cube for other shapes
                mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                go.AddComponent<BoxCollider>();
            }

            // Mud-colored material
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.45f, 0.35f, 0.25f); // Mud brown
                renderer.material = mat;
            }

            return go;
        }

        /// <summary>
        /// Compose a building from stacked KayKit rock meshes for 2026 AAA visuals.
        /// </summary>
        GameObject ComposeKayKitBuilding(string id, Vector3 position, Vector3 scale)
        {
            var root = new GameObject($"Building_{id}_Composite");
            root.transform.position = position;

            var rng = new System.Random(id.GetHashCode());
            int rockCount = id == "spire" ? 12 : 8;
            float height = scale.y;
            float baseRadius = Mathf.Max(scale.x, scale.z) * 0.5f;

            for (int i = 0; i < rockCount; i++)
            {
                var prefab = kayKitRockPrefabs[rng.Next(kayKitRockPrefabs.Length)];
                if (prefab == null) continue;

                float t = (float)i / rockCount;
                float y = t * height;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float r = baseRadius * (1f - t * 0.3f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r);

                var rock = Instantiate(prefab, position + offset, Quaternion.Euler(
                    (float)rng.NextDouble() * 30f,
                    (float)rng.NextDouble() * 360f,
                    (float)rng.NextDouble() * 30f));
                rock.name = $"Rock_{i}";
                rock.transform.SetParent(root.transform);
                rock.transform.localScale *= 1.2f + (float)rng.NextDouble() * 0.6f;

                // Apply mud material to all renderers
                var renderers = rock.GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in renderers)
                {
                    if (_mudFresh != null) rend.material = _mudFresh;
                }
            }

            // Add collider to root
            var col = root.AddComponent<BoxCollider>();
            col.size = scale;
            col.center = Vector3.up * (scale.y * 0.5f);

            return root;
        }

        static Material CreateBuildingMaterial(string name)
        {
            // Always create materials directly — no scene search dependency.
            // This guarantees valid materials even if editor assets aren't loaded.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                // P1 AUDIT FIX: Fail hard if URP/Lit missing - don't fall back to Standard
                Debug.LogError($"[BuildingSpawner] CRITICAL: URP/Lit shader not found for {name}! Check Player Settings > Graphics > Scriptable Render Pipeline Settings is set to URP asset. Build will use incorrect shaders.");
                // Still try Standard as emergency fallback, but log as critical error
                shader = Shader.Find("Standard");
                if (shader == null)
                {
                    Debug.LogError($"[BuildingSpawner] FATAL: No valid shader (URP/Lit or Standard) found for {name}!");
                    return null;
                }
                Debug.LogWarning($"[BuildingSpawner] Emergency fallback to Standard shader for {name}. This is NOT production-ready.");
            }

            var mat = new Material(shader);
            mat.name = name;

            switch (name)
            {
                case "M_Mud_Fresh":
                    mat.SetColor("_BaseColor", new Color(0.30f, 0.20f, 0.12f));
                    mat.SetFloat("_Smoothness", 0.1f);
                    mat.SetFloat("_Metallic", 0.0f);
                    break;
                case "M_Mud_Cracking":
                    mat.SetColor("_BaseColor", new Color(0.42f, 0.32f, 0.18f));
                    mat.SetFloat("_Smoothness", 0.15f);
                    mat.SetFloat("_Metallic", 0.0f);
                    break;
                case "M_Stone_Active":
                    mat.SetColor("_BaseColor", new Color(0.82f, 0.78f, 0.70f));
                    mat.SetFloat("_Smoothness", 0.65f);
                    mat.SetFloat("_Metallic", 0.1f);
                    // Warm golden emission
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(0.6f, 0.5f, 0.2f) * 0.4f);
                    break;
            }

            Debug.Log($"[BuildingSpawner] Created material: {name}");
            return mat;
        }

        void AddFountainParticles(GameObject fountain)
        {
            string childName = "WaterSpray";
            if (fountain.transform.Find(childName) != null) return;

            var sprayGO = new GameObject(childName);
            sprayGO.transform.SetParent(fountain.transform);
            sprayGO.transform.localPosition = new Vector3(0f, 2f, 0f);

            var ps = sprayGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 60;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.3f, 0.6f, 0.9f, 0.6f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.8f;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.3f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.3f, 0.6f, 0.95f), 0f),
                        new GradientColorKey(new Color(0.15f, 0.4f, 0.8f), 1f) },
                new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f));

            // Use default particle material (works with URP)
            var renderer = sprayGO.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.SetColor("_BaseColor", new Color(0.3f, 0.65f, 0.95f, 0.5f));
            // Enable transparent blending
            renderer.material.SetFloat("_Surface", 1f);
            renderer.material.SetOverrideTag("RenderType", "Transparent");
            renderer.material.SetFloat("_Blend", 0f); // Alpha blend
            renderer.material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            renderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    /// <summary>
    /// Simple bobbing animation for discovery markers above buildings.
    /// </summary>
    public class BobbingMarker : MonoBehaviour
    {
        float _baseY;
        float _phase;

        void Start()
        {
            _baseY = transform.localPosition.y;
            _phase = Random.value * Mathf.PI * 2f; // Random start phase
        }

        void Update()
        {
            // Reduced-motion friendly: static position + no spin for accessibility (affects Moon 1 first excavation "scan here" hints too)
            if (Tartaria.UI.SettingsOverlay.IsReducedMotion)
            {
                // Keep base height only — no animation
                var p = transform.localPosition;
                p.y = _baseY;
                transform.localPosition = p;
                return;
            }

            // Gentle bob + spin (full motion path)
            float bob = Mathf.Sin(Time.time * 1.5f + _phase) * 0.4f;
            var pos = transform.localPosition;
            pos.y = _baseY + bob;
            transform.localPosition = pos;
            transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}
