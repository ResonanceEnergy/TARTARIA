#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutBuildings — one-click "100% wire" the 3 hero buildings into
    /// the Echohaven scene. Idempotent.
    ///
    /// Per docs/15_MVP_BUILD_SPEC.md § 5 (the three Listener Houses) + § 6
    /// (3-node tuning, mud→stone visual swap, RS rewards).
    ///
    /// What it does:
    ///   1. For each of CrystalSpire / StarDome / HarmonicFountain:
    ///      a. Instantiate from Assets/_Project/Prefabs/Buildings/*.prefab if missing
    ///      b. Place at thematic position (triangular around player spawn)
    ///      c. Attach SphereCollider trigger (interaction range)
    ///      d. Attach InteractableBuilding with per-building config
    ///         (unique buildingId, displayName, themed RS reward)
    ///      e. Attach NavMeshObstacle so Mud Golems / Milo path around
    ///      f. Assign M_Mud_Fresh + M_Building_Stone materials for state swap
    ///   2. Parent everything under a "Hero_Buildings" GameObject for tidiness.
    /// </summary>
    public static class Moon1BuildOutBuildings
    {
        const string CRYSTAL_SPIRE_PREFAB = "Assets/_Project/Prefabs/Buildings/Echohaven_CrystalSpire.prefab";
        const string STAR_DOME_PREFAB    = "Assets/_Project/Prefabs/Buildings/Echohaven_StarDome.prefab";
        const string HARMONIC_FOUNTAIN_PREFAB = "Assets/_Project/Prefabs/Buildings/Echohaven_HarmonicFountain.prefab";

        const string MUD_MAT_PATH   = "Assets/_Project/Materials/M_Mud_Fresh.mat";
        const string STONE_MAT_PATH = "Assets/_Project/Materials/M_Building_Stone.mat";

        // Per docs/15 § 5 — triangular arrangement around player spawn at (12, 0.5, 0)
        // far enough that the player must explore but close enough to see at start
        struct BuildingSpec
        {
            public string buildingId;
            public string displayName;
            public string prefabPath;
            public Vector3 surfacePosition;   // ground-level position (X, 0, Z)
            public float yaw;
            public int nodeCount;
            public float rsReward;
            public float interactRadius;
            public float burialDepth;          // negative Y offset = how far sunk into terrain when buried
            public float buildingHeight;       // approximate prefab height in meters
        }

        // Per docs/15_MVP_BUILD_SPEC.md §7:
        //   Dome — 18m tall, buried 80% → sink ~14.4m, only ~3.6m visible
        //   Fountain — 5m tall, buried 95% → sink ~4.75m, only ~0.25m visible
        //   Spire — 15m tall, buried 60% → sink ~9m, only ~6m visible
        // Positioned in a triangle around player spawn (12, 0.5, 0) so player walks
        // toward the only initially-visible structure (the Spire) by default.
        static readonly BuildingSpec[] SPECS = new BuildingSpec[]
        {
            new BuildingSpec
            {
                buildingId      = "echohaven_crystalspire",  // "The Spire — First Note" — 60% buried
                displayName     = "The First Note",
                prefabPath      = CRYSTAL_SPIRE_PREFAB,
                surfacePosition = new Vector3(35f, 0f, 25f),
                yaw             = -45f,
                nodeCount       = 3,
                rsReward        = 50f,
                interactRadius  = 6f,
                buildingHeight  = 15f,
                burialDepth     = -9f,                       // 60% of 15m
            },
            new BuildingSpec
            {
                buildingId      = "echohaven_stardome",      // "The Dome — Listeners' Hall" — 80% buried
                displayName     = "The Listeners' Hall",
                prefabPath      = STAR_DOME_PREFAB,
                surfacePosition = new Vector3(-30f, 0f, 30f),
                yaw             = 30f,
                nodeCount       = 3,
                rsReward        = 50f,
                interactRadius  = 8f,
                buildingHeight  = 18f,
                burialDepth     = -14.4f,                    // 80% of 18m
            },
            new BuildingSpec
            {
                buildingId      = "echohaven_harmonicfountain", // "The Fountain — Thread of Memory" — 95% buried
                displayName     = "The Thread of Memory",
                prefabPath      = HARMONIC_FOUNTAIN_PREFAB,
                surfacePosition = new Vector3(5f, 0f, 50f),
                yaw             = 0f,
                nodeCount       = 3,
                rsReward        = 50f,
                interactRadius  = 6f,
                buildingHeight  = 5f,
                burialDepth     = -4.75f,                    // 95% of 5m
            },
        };

        [MenuItem("Tartaria/1 Build/Moon 1 — Buildings (3 Hero)", priority = 100)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Build Out Buildings", "No active scene. Open Echohaven_VerticalSlice.unity first.", "OK");
                return;
            }

            // 1. Ensure parent group
            var heroParent = GameObject.Find("Hero_Buildings");
            if (heroParent == null)
            {
                heroParent = new GameObject("Hero_Buildings");
                Undo.RegisterCreatedObjectUndo(heroParent, "Create Hero_Buildings group");
            }

            // 2. Load materials (best-effort)
            var mudMat   = AssetDatabase.LoadAssetAtPath<Material>(MUD_MAT_PATH);
            var stoneMat = AssetDatabase.LoadAssetAtPath<Material>(STONE_MAT_PATH);
            if (mudMat == null)   Debug.LogWarning($"[Moon1BuildOutBuildings] Mud material not found at {MUD_MAT_PATH}");
            if (stoneMat == null) Debug.LogWarning($"[Moon1BuildOutBuildings] Stone material not found at {STONE_MAT_PATH}");

            int wired = 0;
            int created = 0;
            int reused = 0;

            foreach (var spec in SPECS)
            {
                // Find existing GameObject by buildingId (idempotent)
                GameObject existing = FindByBuildingId(spec.buildingId);
                GameObject go;

                if (existing != null)
                {
                    go = existing;
                    reused++;
                    Debug.Log($"[Moon1BuildOutBuildings] Reusing existing {spec.displayName} (id={spec.buildingId})");
                }
                else
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.prefabPath);
                    if (prefab == null)
                    {
                        Debug.LogError($"[Moon1BuildOutBuildings] Missing prefab: {spec.prefabPath} — skipping {spec.displayName}");
                        continue;
                    }
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, heroParent.transform);
                    go.name = "Building_" + spec.buildingId;
                    Undo.RegisterCreatedObjectUndo(go, "Spawn " + spec.displayName);
                    created++;
                }

                // Position — sink to burial depth (Y = negative offset means buried in terrain)
                go.transform.position = spec.surfacePosition + new Vector3(0f, spec.burialDepth, 0f);
                go.transform.rotation = Quaternion.Euler(0f, spec.yaw, 0f);

                // SphereCollider trigger for interaction radius
                var trigger = go.GetComponent<SphereCollider>();
                if (trigger == null) trigger = Undo.AddComponent<SphereCollider>(go);
                trigger.isTrigger = true;
                trigger.radius = spec.interactRadius;

                // InteractableBuilding wiring (Phase 1 minimal component)
                var ib = go.GetComponent<InteractableBuilding>();
                if (ib == null) ib = Undo.AddComponent<InteractableBuilding>(go);

                // Set fields via SerializedObject (private fields)
                var so = new SerializedObject(ib);
                SetString(so, "buildingId", spec.buildingId);
                SetString(so, "displayName", spec.displayName);
                SetInt(so, "nodeCount", spec.nodeCount);
                SetFloat(so, "interactRadius", spec.interactRadius);
                SetFloat(so, "restorationRsReward", spec.rsReward);
                if (mudMat != null)   SetObjectRef(so, "mudMaterial", mudMat);
                if (stoneMat != null) SetObjectRef(so, "stoneMaterial", stoneMat);
                so.ApplyModifiedProperties();

                // NavMeshObstacle so AI paths around (not through) the building
                var obstacle = go.GetComponent<NavMeshObstacle>();
                if (obstacle == null)
                {
                    obstacle = Undo.AddComponent<NavMeshObstacle>(go);
                    obstacle.carving = true;
                    obstacle.shape = NavMeshObstacleShape.Box;
                    obstacle.center = new Vector3(0f, 2f, 0f);
                    obstacle.size = new Vector3(6f, 4f, 6f); // approx footprint; tune per prefab if needed
                }

                wired++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = heroParent;

            string summary = $"Moon 1 Buildings: {wired}/{SPECS.Length} wired (created={created}, reused={reused})";
            Debug.Log("[Moon1BuildOutBuildings] " + summary);
            EditorUtility.DisplayDialog("Build Out Moon 1 Buildings", summary +
                "\n\nAll 3 hero buildings are now in the scene under 'Hero_Buildings'. " +
                "Each has SphereCollider trigger + InteractableBuilding + NavMeshObstacle. " +
                "Mud→Stone materials wired for buried/restored state swap. " +
                "then Play.",
                "OK");
        }

        static GameObject FindByBuildingId(string buildingId)
        {
            var all = UnityEngine.Object.FindObjectsByType<InteractableBuilding>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var ib in all)
            {
                var so = new SerializedObject(ib);
                var prop = so.FindProperty("buildingId");
                if (prop != null && prop.stringValue == buildingId)
                    return ib.gameObject;
            }
            return null;
        }

        static void SetString(SerializedObject so, string name, string val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.stringValue = val;
        }
        static void SetInt(SerializedObject so, string name, int val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.intValue = val;
        }
        static void SetFloat(SerializedObject so, string name, float val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.floatValue = val;
        }
        static void SetObjectRef(SerializedObject so, string name, Object val)
        {
            var p = so.FindProperty(name);
            if (p != null) p.objectReferenceValue = val;
        }
    }
}
#endif
