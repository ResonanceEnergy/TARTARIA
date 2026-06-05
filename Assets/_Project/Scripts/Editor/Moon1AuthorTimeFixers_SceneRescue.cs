#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1AuthorTimeFixers_SceneRescue — aggressive scene rescue for Moon 1
    /// (dedupe duplicates, force-spawn a working Player at (0, 2, -10), parent
    /// camera to follow, raise quest HUD sortingOrder). Renamed 2026-06-04
    /// from Moon1SceneRescue — although that name did not match the Fix*
    /// quarantine pattern, the file lives in Editor/ and is author-time only,
    /// so naming it consistent with the rest of the fixer family is clearer.
    ///
    /// Menu: Tartaria / 6 Scene Tools / Scene Rescue (Dedupe + Force Player)
    /// </summary>
    public static class Moon1AuthorTimeFixers_SceneRescue
    {
        [MenuItem("Tartaria/6 Scene Tools/Scene Rescue (Dedupe + Force Player)", priority = 660)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MOON 1 SCENE RESCUE ===\n");

            int dedupedTotal = 0;
            dedupedTotal += DedupeByName("PlayerSpawner",            sb);
            dedupedTotal += DedupeByName("Milo",                     sb);
            dedupedTotal += DedupeByName("EchohavenContentSpawner",  sb);
            dedupedTotal += DedupeByName("PlayerSpawn",              sb);
            dedupedTotal += DedupeByName("_SpawnPlatform",           sb);
            sb.AppendLine("Total duplicates removed: " + dedupedTotal + "\n");

            // Force-spawn / repair Player
            var player = ForceSpawnPlayer(sb);

            // Parent camera to follow player
            ConfigureCameraFollow(player, sb);

            // Raise quest HUD sortingOrder (if QuestObjectiveTrackerUI is set up)
            BumpQuestHudPriority(sb);

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog("Moon 1 Scene Rescue", sb.ToString(), "OK");
        }

        // ─────────────────────────────────────────────────────────────
        // 1. DEDUPE — keep the FIRST GameObject of a given name, destroy siblings
        // ─────────────────────────────────────────────────────────────

        static int DedupeByName(string targetName, StringBuilder sb)
        {
            var hits = new List<GameObject>();
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                CollectByName(root.transform, targetName, hits);
            }
            if (hits.Count <= 1) return 0;

            int removed = 0;
            for (int i = 1; i < hits.Count; i++)
            {
                sb.AppendLine("  DEDUPE: destroying duplicate '" + targetName + "' #" + i + " @ " + hits[i].transform.position);
                Undo.DestroyObjectImmediate(hits[i]);
                removed++;
            }
            return removed;
        }

        static void CollectByName(Transform t, string name, List<GameObject> hits)
        {
            if (t.gameObject.name == name) hits.Add(t.gameObject);
            for (int i = 0; i < t.childCount; i++) CollectByName(t.GetChild(i), name, hits);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. FORCE PLAYER — guarantee a controllable player at (0,2,-10)
        // ─────────────────────────────────────────────────────────────

        const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Player.prefab";
        static readonly Vector3 SPAWN_POS = new Vector3(0f, 2f, -10f);

        static GameObject ForceSpawnPlayer(StringBuilder sb)
        {
            // 1) Look for an existing tagged "Player" GameObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // 2) Load the Player prefab from disk
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);

            // 3) If no live Player, instantiate from prefab; else relocate existing
            if (player == null)
            {
                if (prefab == null)
                {
                    sb.AppendLine("PLAYER: prefab missing at " + PLAYER_PREFAB_PATH + " — building procedural fallback.");
                    player = BuildProceduralPlayer();
                }
                else
                {
                    player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(player, "Spawn Player");
                    sb.AppendLine("PLAYER: spawned from prefab '" + prefab.name + "'");
                }
            }
            else
            {
                sb.AppendLine("PLAYER: found existing tagged Player — repositioning");
            }

            // 4) Position + tag + ensure components
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = SPAWN_POS;
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var cc = player.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = player.AddComponent<CharacterController>();
                cc.center = new Vector3(0f, 1f, 0f);
                cc.height = 1.8f;
                cc.radius = 0.3f;
                sb.AppendLine("PLAYER: added CharacterController");
            }

            // 5) Ensure a PlayerInputHandler-type input script is attached
            //    (probe by name to avoid hard asmdef ref)
            EnsureInputHandler(player, sb);

            sb.AppendLine("PLAYER: ready @ " + SPAWN_POS);
            return player;
        }

        static GameObject BuildProceduralPlayer()
        {
            var go = new GameObject("Player_Procedural");
            go.tag = "Player";
            go.transform.position = SPAWN_POS;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
            body.name = "Body";
            body.transform.SetParent(go.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.2f, 0.5f, 0.8f));
                body.GetComponent<Renderer>().sharedMaterial = mat;
            }

            var cc = go.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 1.8f;
            cc.radius = 0.3f;
            return go;
        }

        static void EnsureInputHandler(GameObject player, StringBuilder sb)
        {
            // Try to find Tartaria.Input.PlayerInputHandler via reflection
            var t = System.Type.GetType("Tartaria.Input.PlayerInputHandler, Tartaria.Input")
                  ?? System.Type.GetType("Tartaria.Gameplay.PlayerInputHandler, Tartaria.Gameplay");
            if (t != null)
            {
                if (player.GetComponent(t) == null)
                {
                    player.AddComponent(t);
                    sb.AppendLine("PLAYER: added " + t.FullName);
                }
                else sb.AppendLine("PLAYER: input handler already present");
                return;
            }

            // NO FALLBACK: Moon1RescueDriver (legacy Input.GetKey bypass) was banned by the
            // 2026-06-03 NATRIX mandate (Sprint 11 L4 NO-DEBT). If PlayerInputHandler can't be
            // resolved by reflection, do NOT silently attach a bypass driver — surface the
            // failure so the real input wiring gets fixed.
            sb.AppendLine("PLAYER: WARNING — PlayerInputHandler type not found via reflection. "
                        + "Cannot attach canonical input handler. Ensure Tartaria.Input asmdef "
                        + "is built and PlayerInputHandler class is present. No bypass driver "
                        + "will be attached (banned by NO-DEBT mandate 2026-06-03).");
        }

        // ─────────────────────────────────────────────────────────────
        // 3. CAMERA FOLLOW
        // ─────────────────────────────────────────────────────────────

        static void ConfigureCameraFollow(GameObject player, StringBuilder sb)
        {
            // Prefer existing CameraRig if present
            var rig = GameObject.Find("CameraRig");
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                var cams = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
                if (cams != null && cams.Length > 0) mainCam = cams[0];
            }
            if (mainCam == null)
            {
                sb.AppendLine("CAM: no Camera in scene — creating one");
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                mainCam = camGO.AddComponent<UnityEngine.Camera>();
            }

            // Park camera as child of player at third-person offset
            mainCam.transform.SetParent(player.transform, worldPositionStays: false);
            mainCam.transform.localPosition = new Vector3(0f, 2f, -6f);
            mainCam.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            sb.AppendLine("CAM: parented to Player, local offset (0, 2, -6) tilt 15°");

            // If CameraRig exists and is *not* the Camera's transform, just disable it
            if (rig != null && rig.transform != mainCam.transform)
            {
                rig.SetActive(false);
                sb.AppendLine("CAM: legacy CameraRig disabled (player now owns camera)");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 4. QUEST HUD PRIORITY
        // ─────────────────────────────────────────────────────────────

        static void BumpQuestHudPriority(StringBuilder sb)
        {
            // QuestObjectiveTrackerUI builds its own runtime Canvas — not present in Edit mode.
            // What we CAN do in Edit mode: ensure no existing Canvas in scene has sortingOrder > 9999
            // that would hide it. We scan and report.
            int conflicts = 0;
            foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.sortingOrder > 9999) conflicts++;
            }
            sb.AppendLine("HUD: scene canvases with sortingOrder>9999: " + conflicts);
        }
    }

    // Moon1RescueDriver class DELETED 2026-06-03 per NATRIX mandate (Sprint 11 L4 NO-DEBT):
    // It used UnityEngine.Input.GetKey (banned legacy Input API under Input System Package),
    // bypassed PlayerInputHandler (which has the focus-loss fix + F310 setup + EMERGENCY BYPASS
    // gates), and was a "workaround that papers over missing scene authoring". The canonical
    // path is `Tartaria.Input.PlayerInputHandler` resolved via reflection in EnsureInputHandler().
    // If that type is missing, surface the failure rather than silently bypassing it.
}
#endif
