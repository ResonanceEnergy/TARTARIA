using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
            CleanDuplicateBuildingMarkers();   // Fix 13: remove stale duplicates from prior runs
            added += CreateTerrain();
            FixupPlazaCollider();
            FixupPlayerSpawn();
            added += CreatePlayerSpawn();
            added += CreateBuildingMarkers();
            added += CreateEnemySpawnMarkers();
            added += CreateLighting();
            added += CreateCameraRig();
            added += CreateBoundary();
            added += EnsureV37Managers();
            added += CreatePostProcessing();

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

            // Wire input actions on PlayerSpawner for fallback player
            var spawner = Object.FindFirstObjectByType<Tartaria.Integration.PlayerSpawner>();
            if (spawner != null)
            {
                var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputActionsFactory.AssetPath);
                if (inputAsset != null)
                {
                    var so = new SerializedObject(spawner);
                    var prop = so.FindProperty("inputActions");
                    if (prop != null && prop.objectReferenceValue == null)
                    {
                        prop.objectReferenceValue = inputAsset;
                        so.ApplyModifiedProperties();
                    }
                }
            }

            if (Object.FindFirstObjectByType<Tartaria.Integration.BuildingSpawner>() == null)
            {
                var go = new GameObject("BuildingSpawner");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Integration.BuildingSpawner>();
                n++;
            }

            // RuntimeHUDBuilder — creates Canvas + all HUD panels at runtime
            if (Object.FindFirstObjectByType<Tartaria.Integration.RuntimeHUDBuilder>() == null)
            {
                var go = new GameObject("RuntimeHUDBuilder");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Integration.RuntimeHUDBuilder>();
                n++;
            }

            // EchohavenContentSpawner — populates NPCs, enemies, collectibles, VFX
            if (Object.FindFirstObjectByType<Tartaria.Integration.EchohavenContentSpawner>() == null)
            {
                var go = new GameObject("EchohavenContentSpawner");
                go.transform.SetParent(parent);
                go.AddComponent<Tartaria.Integration.EchohavenContentSpawner>();
                n++;
            }

            // SceneLoader and GameBootstrap live in Boot.unity only — do NOT scaffold here.
            // Clean up any duplicates left by earlier builds.
            foreach (var dup in Object.FindObjectsByType<Tartaria.Core.SceneLoader>(FindObjectsSortMode.None))
                Object.DestroyImmediate(dup.gameObject);
            foreach (var dup in Object.FindObjectsByType<Tartaria.Core.GameBootstrap>(FindObjectsSortMode.None))
                Object.DestroyImmediate(dup.gameObject);

            return n;
        }

        // ─── Collider Fixup ──────────────────────────

        static void FixupPlazaCollider()
        {
            var plaza = GameObject.Find("CentralPlaza");
            if (plaza == null) return;
            var capsule = plaza.GetComponent<CapsuleCollider>();
            if (capsule == null) return;
            Object.DestroyImmediate(capsule);
            if (plaza.GetComponent<BoxCollider>() == null)
                plaza.AddComponent<BoxCollider>();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // ─── Spawn Position Fixup ───────────────────

        static readonly Vector3 DesiredSpawnPos = new Vector3(10f, 1f, 5f);

        static void FixupPlayerSpawn()
        {
            var spawn = GameObject.Find("PlayerSpawn");
            if (spawn == null) return;
            if (Vector3.Distance(spawn.transform.position, DesiredSpawnPos) > 0.1f)
            {
                spawn.transform.position = DesiredSpawnPos;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            // Remove any baked Player prefab instance from the scene
            // PlayerSpawner handles instantiation at runtime from the prefab
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Object.DestroyImmediate(player);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }

        // ─── Terrain ─────────────────────────────────

        static int CreateTerrain()
        {
            if (GameObject.Find("EchohavenTerrain") != null) return 0;

            var parent = new GameObject("EchohavenTerrain");

            // Main ground — use procedural terrain mesh if available
            var terrainMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                "Assets/_Project/Models/Generated/Terrain.asset");
            GameObject ground;
            if (terrainMesh != null)
            {
                ground = new GameObject("GroundPlane");
                ground.AddComponent<MeshFilter>().sharedMesh = terrainMesh;
                ground.AddComponent<MeshRenderer>();
                ground.AddComponent<MeshCollider>().sharedMesh = terrainMesh;
                ground.transform.localScale = Vector3.one;
            }
            else
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.transform.localScale = new Vector3(20f, 1f, 20f);
            }
            ground.name = "GroundPlane";
            ground.transform.SetParent(parent.transform);
            ground.transform.localPosition = Vector3.zero;
            ground.isStatic = true;
            AssignMaterial(ground, "M_Ground_Terrain");

            // Central plaza — slightly raised, lighter
            // Use Cylinder visual but replace CapsuleCollider with BoxCollider.
            // CapsuleCollider on (15, 0.1, 15) scale becomes a sphere of radius 7.5
            // that the player slides off of. BoxCollider gives a flat walkable surface.
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "CentralPlaza";
            plaza.transform.SetParent(parent.transform);
            plaza.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            plaza.transform.localScale = new Vector3(15f, 0.1f, 15f);
            plaza.isStatic = true;
            Object.DestroyImmediate(plaza.GetComponent<CapsuleCollider>());
            plaza.AddComponent<BoxCollider>();
            AssignMaterial(plaza, "M_Stone_Plaza");

            // Mud mounds near each building — visible from center, imply buried structures
            // StarDome area (30, 0, 20) — two mounds nearby
            PlaceMudMound(parent.transform, "MudMound_0", new Vector3(25f, 0.3f, 14f),
                new Vector3(6f, 1.5f, 5f));
            PlaceMudMound(parent.transform, "MudMound_1", new Vector3(35f, 0.2f, 25f),
                new Vector3(5f, 1.2f, 6f));
            // Fountain area (-20, 0, 35)
            PlaceMudMound(parent.transform, "MudMound_2", new Vector3(-15f, 0.3f, 28f),
                new Vector3(7f, 1.8f, 5f));
            // Spire area (0, 0, -30) — visible as you orbit camera
            PlaceMudMound(parent.transform, "MudMound_3", new Vector3(5f, 0.2f, -24f),
                new Vector3(5f, 1.3f, 7f));

            return 1;
        }

        static void PlaceMudMound(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            if (parent.Find(name) != null) return;
            var mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = name;
            mound.transform.SetParent(parent);
            mound.transform.localPosition = pos;
            mound.transform.localScale = scale;
            mound.isStatic = true;
            AssignMaterial(mound, "M_Mud_Fresh");
        }

        // ─── Player Spawn ────────────────────────────

        static int CreatePlayerSpawn()
        {
            if (GameObject.Find("PlayerSpawn") != null) return 0;

            var spawn = new GameObject("PlayerSpawn");
            spawn.transform.position = new Vector3(10f, 1f, 5f);

            // Visible marker (editor only)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "SpawnMarker";
            marker.transform.SetParent(spawn.transform);
            marker.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            marker.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
            AssignMaterial(marker, "M_Aether_Bright");

            // Player is NOT baked into the scene — PlayerSpawner handles instantiation at runtime
            // This avoids position-override loss when VisualUpgradeBuilder re-saves the prefab

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

        /// <summary>
        /// Fix 13: Remove duplicate building marker GameObjects accumulated from repeated
        /// Populate() calls. Keeps only the first object found per building name.
        /// </summary>
        static void CleanDuplicateBuildingMarkers()
        {
            string[] buildingNames = { "StarDome_Placeholder", "HarmonicFountain_Placeholder", "CrystalSpire_Placeholder" };
            foreach (var name in buildingNames)
            {
                var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                var matches = new System.Collections.Generic.List<GameObject>();
                foreach (var t in all)
                    if (t.name == name) matches.Add(t.gameObject);

                // Keep first, destroy the rest
                for (int i = 1; i < matches.Count; i++)
                {
                    Debug.Log($"[EchohavenScenePopulator] Removing duplicate building marker: {name} #{i}");
                    Object.DestroyImmediate(matches[i]);
                }
            }
        }

        static int CreateBuildingMarkers()
        {
            var parent = FindOrCreate("--- BUILDINGS ---");
            int n = 0;

            n += PlaceBuildingMarker(parent.transform, "StarDome_Placeholder",
                new Vector3(30f, 0f, 20f), PrimitiveType.Sphere,
                new Vector3(12f, 8f, 12f), "M_Mud_Fresh");

            n += PlaceBuildingMarker(parent.transform, "HarmonicFountain_Placeholder",
                new Vector3(-20f, 0f, 35f), PrimitiveType.Cylinder,
                new Vector3(6f, 3f, 6f), "M_Mud_Fresh");

            n += PlaceBuildingMarker(parent.transform, "CrystalSpire_Placeholder",
                new Vector3(0f, 0f, -30f), PrimitiveType.Cylinder,
                new Vector3(3f, 15f, 3f), "M_Mud_Fresh");

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
                instance.name = name; // Keep placeholder name for VisualUpgradeBuilder
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
            AssignMaterial(ring, "M_Corruption");

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
            aether.shadows = LightShadows.None; // Prevent shadow atlas overflow

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
            var existingRig = GameObject.Find("CameraRig");
            if (existingRig != null)
            {
                // Ensure existing camera has MainCamera tag (may be missing from older builds)
                var existingCam = existingRig.GetComponentInChildren<UnityEngine.Camera>();
                if (existingCam != null && !existingCam.CompareTag("MainCamera"))
                {
                    existingCam.gameObject.tag = "MainCamera";
                    Debug.Log("[EchohavenScenePopulator] Fixed MainCamera tag on existing CameraRig.");
                }
                return 0;
            }

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
            // Do NOT add AudioListener here — SceneLoader manages the single
            // listener at runtime after disabling Boot camera.

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

            // Rocky wall tint — muted grey-brown
            var wallShader = Shader.Find("Universal Render Pipeline/Lit");

            for (int i = 0; i < 4; i++)
            {
                // Visible wall: cube primitive with mesh + collider
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{dirs[i]}";
                wall.transform.SetParent(boundary.transform);
                wall.transform.position = positions[i];
                wall.transform.localScale = scales[i];

                // Replace default box collider (already created by CreatePrimitive)
                // — keeps collider sized correctly via transform.localScale

                // Apply a rocky boundary material
                var mr = wall.GetComponent<MeshRenderer>();
                if (wallShader != null)
                {
                    var mat = new Material(wallShader);
                    mat.SetColor("_BaseColor", new Color(0.25f, 0.22f, 0.18f)); // dark earth/stone
                    mr.material = mat;
                }
            }

            return 1;
        }

        // ─── Post-Processing ─────────────────────────

        static int CreatePostProcessing()
        {
            if (GameObject.Find("PostProcessVolume") != null) return 0;

            // Create a VolumeProfile with post-processing overrides
            string profilePath = "Assets/_Project/Config/EchohavenVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();

                // Bloom — makes aether glow effects pop
                var bloom = profile.Add<Bloom>(true);
                bloom.threshold.value = 0.9f;
                bloom.threshold.overrideState = true;
                bloom.intensity.value = 0.8f;
                bloom.intensity.overrideState = true;
                bloom.scatter.value = 0.65f;
                bloom.scatter.overrideState = true;

                // Tonemapping — ACES for cinematic look
                var tonemap = profile.Add<Tonemapping>(true);
                tonemap.mode.value = TonemappingMode.ACES;
                tonemap.mode.overrideState = true;

                // Color Adjustments — warm, slightly saturated
                var colorAdj = profile.Add<ColorAdjustments>(true);
                colorAdj.postExposure.value = 0.3f;
                colorAdj.postExposure.overrideState = true;
                colorAdj.contrast.value = 12f;
                colorAdj.contrast.overrideState = true;
                colorAdj.saturation.value = 15f;
                colorAdj.saturation.overrideState = true;
                colorAdj.colorFilter.value = new Color(1f, 0.97f, 0.92f); // slight warm tint
                colorAdj.colorFilter.overrideState = true;

                // Vignette — subtle edge darkening
                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.value = 0.25f;
                vignette.intensity.overrideState = true;
                vignette.smoothness.value = 0.4f;
                vignette.smoothness.overrideState = true;

                // Color Curves — lift shadows slightly for atmosphere
                var curves = profile.Add<LiftGammaGain>(true);
                curves.lift.value = new Vector4(0.02f, 0.03f, 0.06f, 0f); // cool shadow tint
                curves.lift.overrideState = true;
                curves.gamma.value = new Vector4(0f, 0f, 0f, 0.05f); // slight gamma boost
                curves.gamma.overrideState = true;

                string dir = System.IO.Path.GetDirectoryName(
                    System.IO.Path.Combine(Application.dataPath, "..", profilePath));
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                AssetDatabase.CreateAsset(profile, profilePath);
                Debug.Log("[Tartaria] VolumeProfile created with Bloom, Tonemapping, Color, Vignette.");
            }

            // Create global Volume in scene
            var volumeGO = new GameObject("PostProcessVolume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.profile = profile;

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

        static void AssignInputActionsToPlayer(GameObject player)
        {
            var handler = player.GetComponent<Tartaria.Input.PlayerInputHandler>();
            if (handler == null) return;
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                InputActionsFactory.AssetPath);
            if (inputAsset == null) return;
            var so = new SerializedObject(handler);
            var prop = so.FindProperty("inputActions");
            if (prop != null) prop.objectReferenceValue = inputAsset;
            so.ApplyModifiedProperties();
            Debug.Log("[EchohavenScenePopulator] Input actions assigned to Player.");
        }

        static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) go = new GameObject(name);
            return go;
        }
    }
}
