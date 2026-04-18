using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Integration;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Editor
{
    /// <summary>
    /// Scene Wiring Pass — fills in ALL serialized references that scene scaffolding
    /// and populator scripts leave null. Runs as Phase 11 of OneClickBuild after
    /// scenes are populated and visual upgrades applied.
    ///
    /// Wires:
    ///   - PlayerSpawner.playerPrefab + inputActions
    ///   - QuestManager.questDatabase (all QuestDefinition SOs)
    ///   - InteractableBuilding.definition + materials on each building
    ///   - GameLoopController.playerInput + cameraController (edit-time)
    ///   - Building/Interactable/Player/Trigger layers in TagManager
    ///   - Building collider layers for interaction raycast
    ///
    /// Menu: Tartaria > Wire Scene References
    /// </summary>
    public static class SceneWiringPass
    {
        const string PrefabsPath = "Assets/_Project/Prefabs";
        const string ConfigPath = "Assets/_Project/Config";
        const string MaterialsPath = "Assets/_Project/Materials";

        [MenuItem("Tartaria/Wire Scene References", false, 8)]
        public static void WireAll()
        {
            int wired = 0;
            wired += EnsureLayers();
            wired += WirePlayerPrefabInputActions();
            wired += WirePlayerSpawner();
            wired += WireQuestManager();
            wired += WireInteractableBuildings();
            wired += WireGameLoopController();
            wired += WireBuildingColliderLayers();
            wired += WireZoneController();

            if (wired > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[SceneWiringPass] {wired} references wired.");
        }

        // ─── Layers ──────────────────────────────────

        static int EnsureLayers()
        {
            int n = 0;
            n += SetLayerIfEmpty(8, "Building");
            n += SetLayerIfEmpty(9, "Interactable");
            n += SetLayerIfEmpty(10, "Player");
            n += SetLayerIfEmpty(11, "Trigger");
            n += SetLayerIfEmpty(12, "Enemy");
            return n;
        }

        static int SetLayerIfEmpty(int index, string name)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var layers = tagManager.FindProperty("layers");
            if (layers == null || index >= layers.arraySize) return 0;

            var prop = layers.GetArrayElementAtIndex(index);
            if (!string.IsNullOrEmpty(prop.stringValue) && prop.stringValue == name) return 0;
            if (!string.IsNullOrEmpty(prop.stringValue)) return 0; // Don't overwrite existing different layer

            prop.stringValue = name;
            tagManager.ApplyModifiedProperties();
            return 1;
        }

        // ─── Player Prefab InputActions ─────────────

        static int WirePlayerPrefabInputActions()
        {
            string prefabPath = $"{PrefabsPath}/Characters/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return 0;

            var pih = prefab.GetComponent<PlayerInputHandler>();
            if (pih == null) return 0;

            // Check via SerializedObject if already wired
            var check = new SerializedObject(pih);
            var checkProp = check.FindProperty("inputActions");
            if (checkProp == null || checkProp.objectReferenceValue != null) return 0;

            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                InputActionsFactory.AssetPath);
            if (inputAsset == null) return 0;

            // Must use PrefabUtility to persist changes into the prefab asset
            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            var handler = contents.GetComponent<PlayerInputHandler>();
            if (handler != null)
            {
                var so = new SerializedObject(handler);
                var prop = so.FindProperty("inputActions");
                if (prop != null)
                {
                    prop.objectReferenceValue = inputAsset;
                    so.ApplyModifiedProperties();
                }
            }
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
            return 1;
        }

        // ─── PlayerSpawner ───────────────────────────

        static int WirePlayerSpawner()
        {
            int n = 0;
            var spawner = Object.FindFirstObjectByType<PlayerSpawner>();
            if (spawner == null) return 0;

            var so = new SerializedObject(spawner);

            // Wire playerPrefab
            var prefabProp = so.FindProperty("playerPrefab");
            if (prefabProp != null && prefabProp.objectReferenceValue == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{PrefabsPath}/Characters/Player.prefab");
                if (prefab != null)
                {
                    prefabProp.objectReferenceValue = prefab;
                    n++;
                }
            }

            // Wire inputActions
            var inputProp = so.FindProperty("inputActions");
            if (inputProp != null && inputProp.objectReferenceValue == null)
            {
                var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    InputActionsFactory.AssetPath);
                if (inputAsset != null)
                {
                    inputProp.objectReferenceValue = inputAsset;
                    n++;
                }
            }

            so.ApplyModifiedProperties();
            return n;
        }

        // ─── QuestManager ────────────────────────────

        static int WireQuestManager()
        {
            var qm = Object.FindFirstObjectByType<QuestManager>();
            if (qm == null) return 0;

            var so = new SerializedObject(qm);
            var dbProp = so.FindProperty("questDatabase");
            if (dbProp == null) return 0;

            // Find all QuestDefinition SOs in config
            var guids = AssetDatabase.FindAssets("t:QuestDefinition",
                new[] { $"{ConfigPath}/Quests" });
            if (guids.Length == 0) return 0;

            // Check if already wired
            if (dbProp.arraySize == guids.Length)
            {
                bool allSet = true;
                for (int i = 0; i < dbProp.arraySize; i++)
                {
                    if (dbProp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    { allSet = false; break; }
                }
                if (allSet) return 0;
            }

            dbProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var questDef = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
                if (questDef == null)
                {
                    Debug.LogWarning($"[SceneWiringPass] Failed to load QuestDefinition at {path}");
                    continue;
                }
                dbProp.GetArrayElementAtIndex(i).objectReferenceValue = questDef;
            }

            so.ApplyModifiedProperties();
            return guids.Length;
        }

        // ─── InteractableBuildings ───────────────────

        static int WireInteractableBuildings()
        {
            int n = 0;
            var buildings = Object.FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);

            foreach (var building in buildings)
            {
                n += WireSingleBuilding(building);
            }

            return n;
        }

        static int WireSingleBuilding(InteractableBuilding building)
        {
            int n = 0;
            var so = new SerializedObject(building);

            // Wire buildingId from name if empty
            var idProp = so.FindProperty("buildingId");
            if (idProp != null && string.IsNullOrEmpty(idProp.stringValue))
            {
                string name = building.gameObject.name.ToLowerInvariant();
                if (name.Contains("dome")) idProp.stringValue = "dome";
                else if (name.Contains("fountain")) idProp.stringValue = "fountain";
                else if (name.Contains("spire")) idProp.stringValue = "spire";
                else idProp.stringValue = building.gameObject.name;
                n++;
            }

            // Wire definition SO
            var defProp = so.FindProperty("definition");
            if (defProp != null && defProp.objectReferenceValue == null)
            {
                string buildingId = idProp?.stringValue ?? "";
                var defGuids = AssetDatabase.FindAssets("t:BuildingDefinition",
                    new[] { ConfigPath });

                foreach (var guid in defGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var def = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
                    if (def != null && def.name.ToLowerInvariant().Contains(buildingId))
                    {
                        defProp.objectReferenceValue = def;
                        n++;
                        break;
                    }
                }
            }

            // Wire materials
            n += WireBuildingMaterial(so, "mudMaterial", "M_Mud_Fresh");
            n += WireBuildingMaterial(so, "revealedMaterial", "M_Stone_Weathered");
            n += WireBuildingMaterial(so, "activeMaterial", "M_Stone_Active");

            // Wire mainRenderer
            var rendererProp = so.FindProperty("mainRenderer");
            if (rendererProp != null && rendererProp.objectReferenceValue == null)
            {
                var renderer = building.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    rendererProp.objectReferenceValue = renderer;
                    n++;
                }
            }

            so.ApplyModifiedProperties();
            return n;
        }

        static int WireBuildingMaterial(SerializedObject so, string propName, string materialName)
        {
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/{materialName}.mat");
            if (mat == null)
            {
                // Also try Config path
                var guids = AssetDatabase.FindAssets($"{materialName} t:Material");
                if (guids.Length > 0)
                    mat = AssetDatabase.LoadAssetAtPath<Material>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            if (mat != null)
            {
                prop.objectReferenceValue = mat;
                return 1;
            }
            return 0;
        }

        // ─── GameLoopController ──────────────────────

        static int WireGameLoopController()
        {
            var glc = Object.FindFirstObjectByType<GameLoopController>();
            if (glc == null) return 0;

            int n = 0;
            var so = new SerializedObject(glc);

            // Wire playerInput (from Player prefab instance in scene)
            var piProp = so.FindProperty("playerInput");
            if (piProp != null && piProp.objectReferenceValue == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var handler = player.GetComponent<PlayerInputHandler>();
                    if (handler != null)
                    {
                        piProp.objectReferenceValue = handler;
                        n++;
                    }
                }
            }

            // Wire cameraController
            var ccProp = so.FindProperty("cameraController");
            if (ccProp != null && ccProp.objectReferenceValue == null)
            {
                var cc = Object.FindFirstObjectByType<Tartaria.Camera.CameraController>();
                if (cc != null)
                {
                    ccProp.objectReferenceValue = cc;
                    n++;
                }
            }

            so.ApplyModifiedProperties();
            return n;
        }

        // ─── Collider Layers ─────────────────────────

        static int WireBuildingColliderLayers()
        {
            int n = 0;
            int buildingLayer = LayerMask.NameToLayer("Building");
            if (buildingLayer < 0) buildingLayer = 8; // fallback

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer < 0) interactableLayer = 9;

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer < 0) playerLayer = 10;

            int triggerLayer = LayerMask.NameToLayer("Trigger");
            if (triggerLayer < 0) triggerLayer = 11;

            // Set building layers
            var buildings = Object.FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            foreach (var b in buildings)
            {
                if (b.gameObject.layer != buildingLayer)
                {
                    SetLayerRecursive(b.gameObject, buildingLayer);
                    n++;
                }
            }

            // Set trigger layers
            var triggers = Object.FindObjectsByType<ProximityTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
            {
                if (t.gameObject.layer != triggerLayer)
                {
                    t.gameObject.layer = triggerLayer;
                    n++;
                }
            }

            // Set player layer
            var player = GameObject.FindWithTag("Player");
            if (player != null && player.layer != playerLayer)
            {
                SetLayerRecursive(player, playerLayer);
                n++;
            }

            // Wire PlayerInputHandler on scene player instance
            if (player != null)
            {
                var handler = player.GetComponent<PlayerInputHandler>();
                if (handler != null)
                {
                    var so = new SerializedObject(handler);

                    // Wire inputActions if null
                    var inputProp = so.FindProperty("inputActions");
                    if (inputProp != null && inputProp.objectReferenceValue == null)
                    {
                        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                            InputActionsFactory.AssetPath);
                        if (inputAsset != null)
                        {
                            inputProp.objectReferenceValue = inputAsset;
                            so.ApplyModifiedProperties();
                            n++;
                        }
                    }

                    var layerProp = so.FindProperty("interactableLayer");
                    if (layerProp != null)
                    {
                        int mask = (1 << buildingLayer) | (1 << interactableLayer);
                        if (layerProp.intValue != mask)
                        {
                            layerProp.intValue = mask;
                            so.ApplyModifiedProperties();
                            n++;
                        }
                    }

                    var enemyProp = so.FindProperty("enemyLayer");
                    if (enemyProp != null)
                    {
                        int enemyLayer = LayerMask.NameToLayer("Enemy");
                        if (enemyLayer < 0) enemyLayer = 12;
                        int enemyMask = 1 << enemyLayer;
                        if (enemyProp.intValue != enemyMask)
                        {
                            enemyProp.intValue = enemyMask;
                            so.ApplyModifiedProperties();
                            n++;
                        }
                    }
                }
            }

            return n;
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        // ─── ZoneController ─────────────────────────

        static int WireZoneController()
        {
            var zc = Object.FindFirstObjectByType<ZoneController>();
            if (zc == null) return 0;

            var so = new SerializedObject(zc);
            int n = 0;

            // Force discovery radius to 15 (was 30, caused instant discovery at spawn)
            var radiusProp = so.FindProperty("discoveryRadius");
            if (radiusProp != null && radiusProp.floatValue > 15f)
            {
                radiusProp.floatValue = 15f;
                n++;
            }

            so.ApplyModifiedProperties();
            return n;
        }
    }
}
