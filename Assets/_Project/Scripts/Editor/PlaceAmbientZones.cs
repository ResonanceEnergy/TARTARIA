// PlaceAmbientZones.cs
// Sprint 8 Lane 5 — Audit blocker #3 remediation.
//
// The Sprint 6 Lane 4 ambient system (AmbientZoneController + AmbientZoneTrigger +
// AmbientZoneProfile + the 5 profile assets produced by AmbientZoneProfileBuilder)
// shipped fully wired in code but never had the 5 trigger volumes actually placed
// in the Echohaven_VerticalSlice scene. Result: the controller boots, the profiles
// exist on disk, but the player never enters a zone and the ambient bus stays silent.
//
// This menu places the 5 Moon 1 ambient zone GameObjects in the scene, each with a
// trigger BoxCollider sized per the audit spec and an AmbientZoneTrigger component
// pointing at its matching profile asset under Assets/_Project/Data/Audio/Ambient/.
//
// Menu: Tartaria/Audio/Place Moon 1 Ambient Zones
//
// Idempotent: if a child GameObject named "AmbientZones" already exists in the
// scene root, the menu logs and exits without mutating anything. To re-run after a
// design change, delete the "AmbientZones" parent in the scene first.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Audio;

namespace Tartaria.Editor.Audio
{
    public static class PlaceAmbientZones
    {
        const string SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string PROFILE_DIR = "Assets/_Project/Data/Audio/Ambient";
        const string PARENT_NAME = "AmbientZones";

        readonly struct ZoneSpec
        {
            public readonly string Name;
            public readonly string ProfileFile;
            public readonly Vector3 Position;
            public readonly Vector3 BoxSize;

            public ZoneSpec(string name, string profileFile, Vector3 position, Vector3 boxSize)
            {
                Name = name;
                ProfileFile = profileFile;
                Position = position;
                BoxSize = boxSize;
            }
        }

        static readonly ZoneSpec[] s_zones = new ZoneSpec[]
        {
            new ZoneSpec("VillageCenter",      "VillageCenter.asset",      new Vector3(  0f, 0f,   0f), new Vector3(40f, 10f, 40f)),
            new ZoneSpec("MudPools",           "MudPools.asset",           new Vector3( 50f, 0f, -30f), new Vector3(25f, 10f, 25f)),
            new ZoneSpec("CathedralInterior",  "CathedralInterior.asset",  new Vector3(-50f, 0f,  25f), new Vector3(30f, 15f, 30f)),
            new ZoneSpec("ForestEdge",         "ForestEdge.asset",         new Vector3( 80f, 0f,  50f), new Vector3(50f, 10f, 50f)),
            new ZoneSpec("Grotto",             "Grotto.asset",             new Vector3(-30f, 0f, -60f), new Vector3(15f,  8f, 15f)),
        };

        [MenuItem("Tartaria/Audio/Place Moon 1 Ambient Zones")]
        public static void PlaceZones()
        {
            // 1. Open Echohaven scene (single mode — this is destructive of any unsaved
            //    scene state, so prompt the user via Unity's standard save-modified flow).
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[AmbientZone/Place] Aborted: user declined to save modified scenes.");
                return;
            }

            Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AmbientZone/Place] Failed to open '{SCENE_PATH}'. Confirm the file exists. Inner: {ex.Message}");
                return;
            }
            if (!scene.IsValid())
            {
                Debug.LogError($"[AmbientZone/Place] Scene '{SCENE_PATH}' loaded but is invalid. Aborting.");
                return;
            }

            // 2. Idempotency check: look for an existing root child named PARENT_NAME.
            GameObject existingParent = FindRootChildByName(scene, PARENT_NAME);
            if (existingParent != null)
            {
                Debug.Log($"[AmbientZone/Place] '{PARENT_NAME}' parent already exists in scene '{scene.name}' (child count={existingParent.transform.childCount}) — already placed. No mutation performed. Delete the parent in the scene to re-run.");
                return;
            }

            // 3. Create the parent.
            var parent = new GameObject(PARENT_NAME);
            Undo.RegisterCreatedObjectUndo(parent, "Place Moon 1 Ambient Zones");
            parent.transform.position = Vector3.zero;
            SceneManager.MoveGameObjectToScene(parent, scene);

            // 4. For each zone spec: load profile, create child GO, add BoxCollider trigger,
            //    add AmbientZoneTrigger, wire profile via SerializedObject (private field).
            int placed = 0;
            var skipped = new List<string>(s_zones.Length);

            foreach (var spec in s_zones)
            {
                string profilePath = $"{PROFILE_DIR}/{spec.ProfileFile}";
                var profile = AssetDatabase.LoadAssetAtPath<AmbientZoneProfile>(profilePath);
                if (profile == null)
                {
                    Debug.LogError($"[AmbientZone/Place] Missing profile asset for zone '{spec.Name}'. Expected at: '{profilePath}'. Run 'Tartaria/Audio/Build Ambient Zone Profiles' first. SKIPPING this zone — no GameObject created.");
                    skipped.Add(spec.Name);
                    continue;
                }

                var go = new GameObject(spec.Name);
                Undo.RegisterCreatedObjectUndo(go, "Place Ambient Zone");
                go.transform.SetParent(parent.transform, worldPositionStays: false);
                go.transform.localPosition = spec.Position;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                // BoxCollider: trigger, sized per audit spec, centered on the GO.
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.center = Vector3.zero;
                box.size = spec.BoxSize;

                // AmbientZoneTrigger requires Collider (RequireComponent) → satisfied above.
                var trigger = go.AddComponent<AmbientZoneTrigger>();

                // The `profile` field on AmbientZoneTrigger is [SerializeField] private — we
                // must wire it via SerializedObject to survive an Inspector reload.
                var so = new SerializedObject(trigger);
                var profileProp = so.FindProperty("profile");
                if (profileProp == null)
                {
                    Debug.LogError($"[AmbientZone/Place] Could not find serialized 'profile' property on AmbientZoneTrigger for zone '{spec.Name}'. Field rename in Sprint 6 Lane 4? Aborting wire for this zone — GameObject left in scene with NULL profile.");
                }
                else
                {
                    profileProp.objectReferenceValue = profile;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                Debug.Log($"[AmbientZone/Place] Placed '{spec.Name}' at {spec.Position} (size={spec.BoxSize}) with profile '{profile.zoneId}' from '{profilePath}'.");
                placed++;
            }

            // 5. Mark dirty + save.
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            if (!saved)
            {
                Debug.LogError($"[AmbientZone/Place] SaveScene returned false for '{scene.path}'. The 5 zone GameObjects are in the scene but UNSAVED — save manually before closing the Editor.");
            }

            string summary = $"[AmbientZone/Place] Done. Placed={placed}/{s_zones.Length}, Skipped={skipped.Count} ({string.Join(", ", skipped)}). Parent='{PARENT_NAME}' under scene root '{scene.name}'.";
            Debug.Log(summary);

            EditorUtility.DisplayDialog(
                "Place Moon 1 Ambient Zones",
                $"Placed: {placed} of {s_zones.Length}\nSkipped: {skipped.Count}{(skipped.Count > 0 ? "\n  - " + string.Join("\n  - ", skipped) : "")}\n\nScene: {scene.name}\nParent: {PARENT_NAME}\nSaved: {saved}",
                "OK");
        }

        /// <summary>
        /// Finds a root-level GameObject in the given scene by name (Unity 6 — no
        /// FindObjectOfType). Returns null if not found.
        /// </summary>
        static GameObject FindRootChildByName(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == name)
                    return roots[i];
            }
            return null;
        }
    }
}
