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
        // G5/G6 playtest gap fix: once-per-session warning gates to stop console flood.
        static bool _warnedRestoreSparkle;
        static bool _warnedKayKitMissing;

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

        [Header("Built Variants (post-restoration prefabs)")]
        [SerializeField, Tooltip("Echohaven_StarDome_Built.prefab — composed Cathedral kit, " +
                                  "swapped in on GameEvents.OnBuildingRestored(\"dome\"). Sprint 11 L8 fix.")]
        GameObject starDomeBuiltVariantPrefab;

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

            // Defensive guard — Decorate() or earlier paths may have destroyed `building`,
            // or AddComponent may fail if [RequireComponent] dependencies aren't satisfied.
            if (building == null)
            {
                Debug.LogError($"[BuildingSpawner] WireBuilding({buildingId}): `building` became null after fallback/decorate. " +
                               $"Skipping InteractableBuilding wiring. position={position} sceneNames=[{string.Join(",", sceneNames)}]");
                return;
            }

            // Ensure InteractableBuilding component
            var interactable = building.GetComponent<InteractableBuilding>();
            if (interactable == null)
            {
                // [RequireComponent(typeof(Collider))] — make sure the prerequisite is there
                // before AddComponent, so the call doesn't silently fail and return null.
                if (building.GetComponent<Collider>() == null)
                    building.AddComponent<BoxCollider>();
                interactable = building.AddComponent<InteractableBuilding>();
            }
            if (interactable == null)
            {
                Debug.LogError($"[BuildingSpawner] WireBuilding({buildingId}): AddComponent<InteractableBuilding> returned null on '{building.name}'. " +
                               $"This usually means [RequireComponent] dependencies couldn't be satisfied or the type failed to compile. Skipping wiring.");
                return;
            }

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

            // Register as scanner POI (real ScanPOI signature, Phase2Stubs extension retired 2026-06-03).
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

                // Once-per-session warning: the runtime ParticleSystem is a working fallback,
                // not a bug. Spamming this on every brazier wire was console noise (G5 playtest gap).
                if (!_warnedRestoreSparkle)
                {
                    Debug.Log("[BuildingSpawner] RestoreSparkle prefab not assigned — using runtime ParticleSystem fallback (this is fine, but assigning the prefab gives a tighter look).");
                    _warnedRestoreSparkle = true;
                }
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
                // G6 playtest gap fix: try Resources fallback before bailing.
                // Look in Resources/Moon1/Props for any prefab matching common rock/bush/tree names.
                var fallback = new System.Collections.Generic.List<GameObject>();
                foreach (var name in new[] { "Rock_A", "Rock_B", "Rock_C", "Bush_A", "Bush_B", "Tree_A", "Tree_Pine" })
                {
                    var p = Resources.Load<GameObject>($"Moon1/Props/{name}");
                    if (p != null) fallback.Add(p);
                }
                if (fallback.Count > 0)
                {
                    kayKitRockPrefabs = fallback.ToArray();
                    Debug.Log($"[BuildingSpawner] kayKitRockPrefabs unassigned — loaded {fallback.Count} from Resources/Moon1/Props.");
                }
                else
                {
                    if (!_warnedKayKitMissing)
                    {
                        Debug.Log("[BuildingSpawner] kayKitRockPrefabs unassigned and no Resources fallback found — skipping scatter (this is fine if you don't want vegetation).");
                        _warnedKayKitMissing = true;
                    }
                    return;
                }
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
                // 2026-06-05 SCALE FIX: same as trees/bushes — SET absolute, not multiply.
                float rockScale = 0.6f + (float)rng.NextDouble() * 1.2f; // 0.6-1.8m rocks
                rock.transform.localScale = new Vector3(rockScale, rockScale, rockScale);
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
                    // 2026-06-05 SCALE FIX: prior code multiplied by 0.8-1.4 — but KayKit/Blender prefabs
                    // ship with baked-in root scale of 75-148x (importer quirk). Multiplying yields
                    // 60-200m bushes that eclipse the village. SET absolute scale to 0.8-1.4m.
                    float bushScale = 0.8f + (float)rng.NextDouble() * 0.6f;
                    bush.transform.localScale = new Vector3(bushScale, bushScale, bushScale);
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
                    // 2026-06-05 SCALE FIX: same as bushes — set absolute scale, not multiply.
                    float treeScale = 2.5f + (float)rng.NextDouble() * 1.5f; // 2.5-4m trees
                    tree.transform.localScale = new Vector3(treeScale, treeScale, treeScale);
                }
            }

            Debug.Log($"[BuildingSpawner] Ambient village scattered: {rockCount} rocks, {bushCount} bushes, {treeCount} trees.");
        }

        /// <summary>
        /// Create Star Dome — Hammer Lane 4 (Sprint 11 L8 50ff78ea) rewrite.
        ///
        /// Replaced the 30+ runtime Instantiate calls (12 walls + 25 floor tiles + 4 pillars
        /// + 8 torches) with a single Instantiate of the authored prefab variant
        /// `Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome_Built.prefab`, composed from
        /// the Cathedral kit (foundation/walls/columns/dome segments/spire/ornaments) and
        /// shipped as text-mode YAML so it survives merges and diffs cleanly.
        ///
        /// The built variant starts HIDDEN — the pre-restoration ruin/mound stays visible
        /// at game-start. It swaps in when GameEvents.OnBuildingRestored fires for the
        /// "dome" building id (grep-verified: GameEvents.cs:56 declares the canonical
        /// `public static event Action&lt;string&gt; OnBuildingRestored`).
        /// </summary>
        GameObject CreateModularDungeonStarDome(Vector3 basePosition)
        {
            var domeRoot = new GameObject("StarDome_ModularComposite");
            domeRoot.transform.position = basePosition;

            // Authored built variant — single Instantiate replaces the 30+ runtime calls.
            // Resolution order: serialized inspector ref -> Resources.Load fallback ->
            // Editor-only AssetDatabase fallback so Editor smoke tests work without scene wiring.
            // 2026-06-04 REORG-5: file moved Prefabs/Moon1/ -> Prefabs/Moon1/Buildings/.
            // Inspector ref in Echohaven_VerticalSlice.unity is the canonical runtime path;
            // Resources.Load remains a no-op fallback (file not in Resources/) and the Editor
            // AssetDatabase fallback now resolves the new categorized path.
            GameObject builtVariantPrefab = starDomeBuiltVariantPrefab;
            if (builtVariantPrefab == null)
                builtVariantPrefab = Resources.Load<GameObject>("Prefabs/Moon1/Buildings/Echohaven_StarDome_Built");
#if UNITY_EDITOR
            if (builtVariantPrefab == null)
                builtVariantPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/Moon1/Buildings/Echohaven_StarDome_Built.prefab");
#endif
            if (builtVariantPrefab == null)
            {
                // Final fallback only when the authored prefab is genuinely missing (which
                // would only happen in a stripped build). Surfaces the issue loudly rather
                // than silently shipping a magenta sphere.
                Debug.LogError("[BuildingSpawner] Echohaven_StarDome_Built.prefab missing. " +
                               "Run Tartaria/6 Bake/Bake StarDome Built Variant in the Editor.");

                var fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallback.name = "StarDome_Fallback";
                fallback.transform.position = basePosition + new Vector3(0f, 3f, 0f);
                fallback.transform.localScale = new Vector3(8f, 6f, 8f);
                fallback.transform.SetParent(domeRoot.transform);

                var renderer = fallback.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.SetColor("_BaseColor", new Color(0.45f, 0.35f, 0.25f)); // URP-safe
                    renderer.material = mat;
                }
                return domeRoot;
            }

            var built = Instantiate(builtVariantPrefab, basePosition, Quaternion.identity, domeRoot.transform);
            built.name = "StarDome_Built";

            // Pre-restoration: built variant is hidden so the ruin/excavation state stays.
            built.SetActive(false);

            // Attach the listener that swaps the visible state on OnBuildingRestored("dome").
            var visibility = domeRoot.AddComponent<StarDomeBuiltVisibility>();
            visibility.Configure(built, buildingId: "dome");

            Debug.Log($"[BuildingSpawner] Star Dome composed from Echohaven_StarDome_Built.prefab " +
                      $"(36 kit children — 1 foundation / 12 walls / 8 columns / 8 dome / 4 ornaments / 3 spire). " +
                      $"Hidden until OnBuildingRestored(\"dome\").");

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
                // 2026-06-05 SCALE FIX: same as scatter sites — SET absolute scale (1.0-1.8m for cluster rocks)
                float clusterRockScale = 1.0f + (float)rng.NextDouble() * 0.8f;
                rock.transform.localScale = new Vector3(clusterRockScale, clusterRockScale, clusterRockScale);

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
            // NO-STUBS mandate: URP/Lit is the only valid shader. NO Standard-shader fallback —
            // a Standard-shader material would paint magenta under URP, violating the visible-to-player
            // quality bar. If URP/Lit is missing the project is misconfigured; fail loud and return null.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError($"[BuildingSpawner] URP/Lit shader not found — URP not installed? Cannot build material '{name}'. Check Player Settings > Graphics > Scriptable Render Pipeline Settings is set to a URP asset.");
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
