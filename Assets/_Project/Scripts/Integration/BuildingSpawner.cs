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

            // Spawn 8-12 buildings around Echohaven (not 3) for visual density
            SpawnBuildingCluster("dome", domePosition, DomeNames, new Vector3(8f, 6f, 8f), count: 4);
            SpawnBuildingCluster("fountain", fountainPosition, FountainNames, new Vector3(4f, 3f, 4f), count: 4);
            SpawnBuildingCluster("spire", spirePosition, SpireNames, new Vector3(3f, 12f, 3f), count: 4);

            Debug.Log("[BuildingSpawner] 12 buildings spawned (4 per zone) with interaction + discovery triggers.");
        }

        void SpawnBuildingCluster(string buildingId, Vector3 centerPos, string[] sceneNames,
            Vector3 baseScale, int count)
        {
            // First building is the primary interactable (original WireBuilding logic)
            WireBuilding(buildingId, centerPos, sceneNames, PrimitiveType.Sphere, baseScale);

            // Spawn additional background buildings in a ring (visual variety, no interaction)
            for (int i = 1; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float radius = Random.Range(10f, 18f);
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 pos = centerPos + offset;

                float scaleVariation = Random.Range(0.8f, 1.4f);
                Vector3 variedScale = baseScale * scaleVariation;

                var building = CreateGreyboxBuilding($"{buildingId}_bg_{i}", pos, PrimitiveType.Sphere, variedScale);
                building.name = $"Building_{buildingId}_background_{i}";
                building.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                // Apply mud material to all renderers
                ApplyMudMaterialRecursive(building, _mudFresh);
            }
        }

        void ApplyMudMaterialRecursive(GameObject go, Material mat)
        {
            if (mat == null) return;
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
                r.material = mat;
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
                building = CreateGreyboxBuilding(buildingId, position, fallbackShape, fallbackScale);
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

            // Inject materials (AddComponent leaves SerializeFields null)
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

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = markerName;
            marker.transform.SetParent(building.transform);

            // Position above building top
            var rend = building.GetComponentInChildren<Renderer>();
            float topY = rend != null ? rend.bounds.max.y - building.transform.position.y + 3f : 8f;
            marker.transform.localPosition = new Vector3(0f, topY, 0f);
            marker.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);

            // Remove collider so it doesn't interfere with raycasts
            var col = marker.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            // Bright golden glow material
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.3f));
                mat.SetFloat("_Smoothness", 0.8f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 2f);
                marker.GetComponent<MeshRenderer>().material = mat;
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

        GameObject CreateGreyboxBuilding(string id, Vector3 position,
            PrimitiveType shape, Vector3 scale)
        {
            // No longer a single primitive — compose REAL ruined building from wall segments.
            var root = new GameObject($"Building_{id}");
            root.transform.position = position;

            // Mud material (warm brown, low smoothness)
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mudMat = shader != null ? new Material(shader) : null;
            if (mudMat != null)
            {
                mudMat.SetColor("_BaseColor", new Color(0.35f, 0.25f, 0.18f));
                mudMat.SetFloat("_Smoothness", 0.2f);
                mudMat.SetFloat("_Metallic", 0.0f);
            }

            // 4 wall segments (scaled cubes)
            float wallHeight = scale.y * 0.6f;
            float wallThick = 0.3f;
            float sizeX = scale.x;
            float sizeZ = scale.z;

            CreateWall(root, "Wall_North", new Vector3(0f, wallHeight * 0.5f, sizeZ * 0.5f), new Vector3(sizeX, wallHeight, wallThick), mudMat);
            CreateWall(root, "Wall_South", new Vector3(0f, wallHeight * 0.5f, -sizeZ * 0.5f), new Vector3(sizeX, wallHeight, wallThick), mudMat);
            CreateWall(root, "Wall_East", new Vector3(sizeX * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThick, wallHeight, sizeZ), mudMat);
            CreateWall(root, "Wall_West", new Vector3(-sizeX * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThick, wallHeight, sizeZ), mudMat);

            // Partial roof (tilted plane)
            var roof = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roof.name = "Roof";
            roof.transform.SetParent(root.transform, false);
            roof.transform.localPosition = new Vector3(0f, wallHeight + 0.2f, 0f);
            roof.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            roof.transform.localScale = new Vector3(sizeX * 0.4f, 1f, sizeZ * 0.4f);
            if (mudMat != null) roof.GetComponent<MeshRenderer>().material = mudMat;

            // Broken doorway (scaled cube with cut)
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Doorway";
            door.transform.SetParent(root.transform, false);
            door.transform.localPosition = new Vector3(0f, wallHeight * 0.4f, sizeZ * 0.5f);
            door.transform.localScale = new Vector3(1.5f, wallHeight * 0.7f, 0.2f);
            var doorCol = door.GetComponent<BoxCollider>();
            if (doorCol != null) Object.Destroy(doorCol); // Open doorway
            if (mudMat != null) door.GetComponent<MeshRenderer>().material = mudMat;

            // 2-3 debris piles around base
            int debrisCount = Random.Range(2, 4);
            for (int i = 0; i < debrisCount; i++)
            {
                float angle = (i / (float)debrisCount) * Mathf.PI * 2f;
                float radius = Mathf.Max(sizeX, sizeZ) * 0.6f;
                Vector3 debrisPos = new Vector3(Mathf.Cos(angle) * radius, 0.15f, Mathf.Sin(angle) * radius);
                CreateDebrisPile(root, $"Debris_{i}", debrisPos, mudMat);
            }

            return root;
        }

        void CreateWall(GameObject parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = localScale;
            if (mat != null) wall.GetComponent<MeshRenderer>().material = mat;
        }

        void CreateDebrisPile(GameObject parent, string name, Vector3 localPos, Material mat)
        {
            var pile = new GameObject(name);
            pile.transform.SetParent(parent.transform, false);
            pile.transform.localPosition = localPos;

            for (int i = 0; i < 3; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Rock_{i}";
                rock.transform.SetParent(pile.transform, false);
                float s = Random.Range(0.2f, 0.5f);
                rock.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    s * 0.4f,
                    Random.Range(-0.3f, 0.3f));
                rock.transform.localScale = new Vector3(s, s * 0.8f, s);
                if (mat != null) rock.GetComponent<MeshRenderer>().material = mat;
            }
        }

        static Material CreateBuildingMaterial(string name)
        {
            // Always create materials directly — no scene search dependency.
            // This guarantees valid materials even if editor assets aren't loaded.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
                Debug.LogWarning($"[BuildingSpawner] URP/Lit shader not found, using Standard for {name}");
            }
            if (shader == null)
            {
                Debug.LogError($"[BuildingSpawner] No valid shader found for {name}!");
                return null;
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
            // Gentle bob + spin
            float bob = Mathf.Sin(Time.time * 1.5f + _phase) * 0.4f;
            var pos = transform.localPosition;
            pos.y = _baseY + bob;
            transform.localPosition = pos;
            transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}
