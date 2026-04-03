using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tartaria.Editor
{
    /// <summary>
    /// Populates the Echohaven vertical-slice scene with greybox world content:
    /// terrain plane, player spawn, building markers, directional light, camera rig.
    /// Positions match WorldInitializer ECS spawn coordinates.
    ///
    /// Menu: Tartaria > Populate Echohaven Scene
    /// </summary>
    public static class EchohavenScenePopulator
    {
        const string MaterialsPath = "Assets/_Project/Materials";
        const string PrefabsPath = "Assets/_Project/Prefabs";

        [MenuItem("Tartaria/Populate Echohaven Scene", false, 6)]
        public static void Populate()
        {
            int added = 0;
            added += CreateTerrain();
            added += CreatePlayerSpawn();
            added += CreateBuildingMarkers();
            added += CreateEnemySpawnMarkers();
            added += CreateLighting();
            added += CreateCameraRig();
            added += CreateBoundary();
            added += EnsureV37Managers();

            if (added > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log($"[Tartaria] Echohaven populated — {added} objects added.");
        }

        // ─── v37 Manager Wiring ──────────────────────

        static int EnsureV37Managers()
        {
            int n = 0;
            var parent = FindOrCreate("--- GAME MANAGERS ---").transform;

            if (Object.FindFirstObjectByType<Tartaria.Integration.PlayerSpawner>() == null)
            {
                var go = new GameObject("PlayerSpawner");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Integration.PlayerSpawner>();
                n++;
            }

            if (Object.FindFirstObjectByType<Tartaria.Integration.BuildingSpawner>() == null)
            {
                var go = new GameObject("BuildingSpawner");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Integration.BuildingSpawner>();
                n++;
            }

            if (Object.FindFirstObjectByType<Tartaria.Core.SceneLoader>() == null)
            {
                var go = new GameObject("SceneLoader");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Core.SceneLoader>();
                n++;
            }

            return n;
        }

        // ─── Terrain ─────────────────────────────────

        static int CreateTerrain()
        {
            if (GameObject.Find("EchohavenTerrain") != null) return 0;

            var parent = new GameObject("EchohavenTerrain");

            // Main ground plane (200×200 units)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.SetParent(parent.transform);
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.isStatic = true;
            AssignMaterial(ground, "Ground_Plaza");

            // Central plaza — slightly raised, lighter
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "CentralPlaza";
            plaza.transform.SetParent(parent.transform);
            plaza.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            plaza.transform.localScale = new Vector3(15f, 0.1f, 15f);
            plaza.isStatic = true;
            AssignMaterial(plaza, "Stone_Active");

            // Mud field markers (4 mounds at edges to imply buried structures)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mound.name = $"MudMound_{i}";
                mound.transform.SetParent(parent.transform);
                mound.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 60f, 0.5f, Mathf.Cos(angle) * 60f);
                mound.transform.localScale = new Vector3(8f, 2f, 8f);
                mound.isStatic = true;
                AssignMaterial(mound, "Mud_Buried");
            }

            return 1;
        }

        // ─── Player Spawn ────────────────────────────

        static int CreatePlayerSpawn()
        {
            if (GameObject.Find("PlayerSpawn") != null) return 0;

            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = new Vector3(0f, 1f, -20f);

            // Visible marker (editor only)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "SpawnMarker";
            marker.transform.SetParent(spawn.transform);
            marker.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            marker.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
            AssignMaterial(marker, "Aether_Glow");

            // Try to place player prefab
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabsPath}/Characters/Player.prefab");
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = spawn.transform.position;
            }

            // Try to place Milo prefab
            var miloPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabsPath}/Characters/Milo.prefab");
            if (miloPrefab != null)
            {
                var milo = (GameObject)PrefabUtility.InstantiatePrefab(miloPrefab);
                milo.transform.position = spawn.transform.position + new Vector3(2f, 0f, -1f);
            }

            return 1;
        }

        // ─── Building Markers ────────────────────────
        // Positions match WorldInitializer.cs SerializeField defaults

        static int CreateBuildingMarkers()
        {
            var parent = FindOrCreate("--- BUILDINGS ---");
            int n = 0;

            n += PlaceBuildingMarker(parent.transform, "StarDome_Placeholder",
                new Vector3(30f, 0f, 20f), PrimitiveType.Sphere,
                new Vector3(12f, 8f, 12f), "Mud_Buried");

            n += PlaceBuildingMarker(parent.transform, "HarmonicFountain_Placeholder",
                new Vector3(-20f, 0f, 35f), PrimitiveType.Cylinder,
                new Vector3(6f, 3f, 6f), "Mud_Buried");

            n += PlaceBuildingMarker(parent.transform, "CrystalSpire_Placeholder",
                new Vector3(0f, 0f, -30f), PrimitiveType.Cylinder,
                new Vector3(3f, 15f, 3f), "Mud_Buried");

            return n;
        }

        static int PlaceBuildingMarker(Transform parent, string name, Vector3 pos,
            PrimitiveType shape, Vector3 scale, string matName)
        {
            if (GameObject.Find(name) != null) return 0;

            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos + new Vector3(0f, scale.y * 0.5f, 0f);
            go.transform.localScale = scale;
            go.isStatic = true;
            AssignMaterial(go, matName);

            // Try to load real prefab
            string defName = name.Replace("_Placeholder", "");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabsPath}/Buildings/Echohaven_{defName}.prefab");
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = pos;
                Object.DestroyImmediate(go);
                instance.transform.SetParent(parent);
                return 1;
            }

            return 1;
        }

        // ─── Enemy Spawn Markers ─────────────────────
        // Positions match WorldInitializer.cs spawn triggers

        static int CreateEnemySpawnMarkers()
        {
            var parent = FindOrCreate("--- ENEMY SPAWNS ---");
            int n = 0;

            n += CreateSpawnPoint(parent.transform, "GolemSpawn_RS25",
                new Vector3(40f, 0f, 30f), 25f);
            n += CreateSpawnPoint(parent.transform, "GolemSpawn_RS50",
                new Vector3(-35f, 0f, 45f), 50f);
            n += CreateSpawnPoint(parent.transform, "GolemSpawn_RS75",
                new Vector3(10f, 0f, -50f), 75f);

            return n;
        }

        static int CreateSpawnPoint(Transform parent, string name, Vector3 pos, float rsThreshold)
        {
            if (GameObject.Find(name) != null) return 0;

            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            // Visual ring marker
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "SpawnRing";
            ring.transform.SetParent(go.transform);
            ring.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ring.transform.localScale = new Vector3(4f, 0.02f, 4f);
            AssignMaterial(ring, "MudGolem_Body");

            return 1;
        }

        // ─── Lighting ────────────────────────────────

        static int CreateLighting()
        {
            if (GameObject.Find("Echohaven_Lighting") != null) return 0;

            var parent = new GameObject("Echohaven_Lighting");

            // Main directional light (golden hour)
            var sunGO = new GameObject("Sun_GoldenHour");
            sunGO.transform.SetParent(parent.transform);
            sunGO.transform.rotation = Quaternion.Euler(42f, -60f, 0f);
            var sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.92f, 0.75f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;

            // Fill light (cool, from opposite direction)
            var fillGO = new GameObject("FillLight");
            fillGO.transform.SetParent(parent.transform);
            fillGO.transform.rotation = Quaternion.Euler(30f, 120f, 0f);
            var fill = fillGO.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.6f, 0.7f, 0.9f);
            fill.intensity = 0.3f;
            fill.shadows = LightShadows.None;

            // Aether point light at central plaza
            var aetherGO = new GameObject("AetherWellLight");
            aetherGO.transform.SetParent(parent.transform);
            aetherGO.transform.position = new Vector3(0f, 3f, 0f);
            var aether = aetherGO.AddComponent<Light>();
            aether.type = LightType.Point;
            aether.color = new Color(0.2f, 0.6f, 1f);
            aether.intensity = 2f;
            aether.range = 25f;

            // Ambient settings
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.4f, 0.55f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.35f, 0.3f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.15f, 0.1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.5f, 0.55f, 0.65f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 60f;
            RenderSettings.fogEndDistance = 150f;

            return 1;
        }

        // ─── Camera Rig ──────────────────────────────

        static int CreateCameraRig()
        {
            if (GameObject.Find("CameraRig") != null) return 0;

            var rig = new GameObject("CameraRig");
            rig.transform.position = new Vector3(0f, 10f, -30f);
            rig.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

            // Main camera
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.SetParent(rig.transform);
            camGO.transform.localPosition = Vector3.zero;
            camGO.transform.localRotation = Quaternion.identity;
            var cam = camGO.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 300f;
            camGO.AddComponent<AudioListener>();

            // Camera controller (if exists)
            var ccType = typeof(Tartaria.Camera.CameraController);
            rig.AddComponent(ccType);

            return 1;
        }

        // ─── Boundary ────────────────────────────────

        static int CreateBoundary()
        {
            if (GameObject.Find("WorldBoundary") != null) return 0;

            var boundary = new GameObject("WorldBoundary");

            // 4 walls just beyond the terrain edge
            string[] dirs = { "North", "South", "East", "West" };
            Vector3[] positions = {
                new(0f, 5f, 100f), new(0f, 5f, -100f),
                new(100f, 5f, 0f), new(-100f, 5f, 0f)
            };
            Vector3[] scales = {
                new(200f, 10f, 1f), new(200f, 10f, 1f),
                new(1f, 10f, 200f), new(1f, 10f, 200f)
            };

            for (int i = 0; i < 4; i++)
            {
                var wall = new GameObject($"Wall_{dirs[i]}");
                wall.transform.SetParent(boundary.transform);
                wall.transform.position = positions[i];
                var bc = wall.AddComponent<BoxCollider>();
                bc.size = scales[i];
            }

            return 1;
        }

        // ─── Helpers ─────────────────────────────────

        static void AssignMaterial(GameObject go, string matName)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/{matName}.mat");
            if (mat != null)
            {
                var r = go.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = mat;
            }
        }

        static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go;
        }
    }
}
