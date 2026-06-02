using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Tartaria.Camera;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 fixers — spawn position, fall-through safety, camera inversion toggle.
    /// </summary>
    public static class Moon1FixSpawn
    {
        [MenuItem("Tartaria/8 Fix/PlayerSpawner Position", false, 51)]
        public static void FixSpawnPosition()
        {
            var spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (spawner == null)
            {
                EditorUtility.DisplayDialog("Spawn Fix", "No PlayerSpawner GameObject in scene. Add one first.", "OK");
                return;
            }

            // 2026-06-01 update: pulled spawn 25 units closer (was Z=-10, now Z=+15).
            // Signpost sits at Z=+25, town hall at Z=+50. New position puts the village
            // squarely in frame from t=0 — player sees at least 4 buildings + signpost
            // within 5 seconds of pressing Play, no walk required. Director ship-gate.
            Vector3 chosenPos = new Vector3(0f, 2f, 15f);

            // Always create a guaranteed platform directly under the spawn — no relying on existing geometry.
            string platformName = "_SpawnPlatform";
            var existing = GameObject.Find(platformName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = platformName;
            platform.transform.position = new Vector3(chosenPos.x, chosenPos.y - 1.5f, chosenPos.z);
            platform.transform.localScale = new Vector3(14f, 0.4f, 14f);

            // Use a fresh URP/Lit material so the platform doesn't render magenta.
            var rend = platform.GetComponent<Renderer>();
            if (rend != null)
            {
                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    var mat = new Material(urpLit);
                    mat.SetColor("_BaseColor", new Color(0.35f, 0.30f, 0.25f)); // muddy brown
                    rend.sharedMaterial = mat;
                }
            }

            // Face the player north (toward the building triangle)
            Undo.RecordObject(spawner.transform, "Fix PlayerSpawner Position");
            spawner.transform.position = chosenPos;
            spawner.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // facing +Z

            // Update serialized default spawn position too
            var so = new SerializedObject(spawner);
            var defaultSpawn = so.FindProperty("defaultSpawnPosition");
            if (defaultSpawn != null)
            {
                defaultSpawn.vector3Value = chosenPos;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(spawner.gameObject);
            EditorUtility.SetDirty(platform);

            // 2026-06-01 22:07 spawn-override-fix: also patch any Moon1PlayerSetup in the scene.
            // It runs at DefaultExecutionOrder(-78) AFTER PlayerSpawner and re-yanks the player to
            // its own serialized spawnPosition. If that field still holds (0,2,-10) the village
            // is 25m behind the player on frame 1. Force its serialized default to match.
            int playerSetupPatched = 0;
            var playerSetup = UnityEngine.Object.FindFirstObjectByType<Moon1PlayerSetup>(FindObjectsInactive.Include);
            if (playerSetup != null)
            {
                var psSo = new SerializedObject(playerSetup);
                var psProp = psSo.FindProperty("spawnPosition");
                if (psProp != null)
                {
                    psProp.vector3Value = chosenPos;
                    psSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(playerSetup);
                    playerSetupPatched = 1;
                    Debug.Log($"[Moon1FixSpawn] Patched Moon1PlayerSetup.spawnPosition → {chosenPos}");
                }
            }

            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            EditorSceneManager.SaveScene(spawner.gameObject.scene);

            string msg = $"PlayerSpawner moved to {chosenPos}, facing north.\n" +
                         $"Created 14×14 brown platform under spawn at y={chosenPos.y - 1.5f}.\n" +
                         (playerSetupPatched > 0
                             ? $"Moon1PlayerSetup.spawnPosition serialized override → {chosenPos}.\n"
                             : "No Moon1PlayerSetup in scene (skipped override patch).\n") +
                         $"\nExit Play Mode if running, then re-enter to spawn on solid ground with the buildings ahead.";
            Debug.Log("[Moon1FixSpawn] " + msg);
            EditorUtility.DisplayDialog("Spawn Fix", msg, "OK");
        }

        [MenuItem("Tartaria/8 Fix/Force All Spawn Refs To (0,2,15)", false, 53)]
        public static void ForceAllSpawnRefs()
        {
            // Idempotent sweep — patches every known reference that controls Moon 1 spawn location.
            // Use this after pulling a branch that may have stale serialized values, or any time
            // the player is appearing in the wrong place at frame 1.
            Vector3 spawnPos       = new Vector3(0f, 2f, 15f);
            Vector3 platformPos    = new Vector3(0f, 0.5f, 15f);

            var sb = new StringBuilder();
            int patched = 0;
            UnityEngine.SceneManagement.Scene targetScene = default;
            bool haveScene = false;

            // 1. PlayerSpawner GO + defaultSpawnPosition serialized field
            var spawner = UnityEngine.Object.FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (spawner != null)
            {
                Undo.RecordObject(spawner.transform, "Force Spawn Refs — PlayerSpawner transform");
                spawner.transform.position = spawnPos;
                spawner.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var so = new SerializedObject(spawner);
                var defProp = so.FindProperty("defaultSpawnPosition");
                if (defProp != null)
                {
                    defProp.vector3Value = spawnPos;
                    so.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(spawner.gameObject);
                targetScene = spawner.gameObject.scene;
                haveScene = true;
                sb.AppendLine($"✓ PlayerSpawner.transform + defaultSpawnPosition → {spawnPos}");
                Debug.Log($"[ForceAllSpawnRefs] PlayerSpawner → {spawnPos}");
                patched++;
            }
            else
            {
                sb.AppendLine("– PlayerSpawner: not found");
            }

            // 2. Moon1PlayerSetup.spawnPosition serialized override
            var playerSetup = UnityEngine.Object.FindFirstObjectByType<Moon1PlayerSetup>(FindObjectsInactive.Include);
            if (playerSetup != null)
            {
                var so = new SerializedObject(playerSetup);
                var prop = so.FindProperty("spawnPosition");
                if (prop != null)
                {
                    prop.vector3Value = spawnPos;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(playerSetup);
                    if (!haveScene) { targetScene = playerSetup.gameObject.scene; haveScene = true; }
                    sb.AppendLine($"✓ Moon1PlayerSetup.spawnPosition → {spawnPos}");
                    Debug.Log($"[ForceAllSpawnRefs] Moon1PlayerSetup.spawnPosition → {spawnPos}");
                    patched++;
                }
                else
                {
                    sb.AppendLine("– Moon1PlayerSetup: spawnPosition field not found");
                }
            }
            else
            {
                sb.AppendLine("– Moon1PlayerSetup: not in scene");
            }

            // 3. _SpawnPlatform position
            var platform = GameObject.Find("_SpawnPlatform");
            if (platform != null)
            {
                Undo.RecordObject(platform.transform, "Force Spawn Refs — platform");
                platform.transform.position = platformPos;
                EditorUtility.SetDirty(platform);
                if (!haveScene) { targetScene = platform.scene; haveScene = true; }
                sb.AppendLine($"✓ _SpawnPlatform.position → {platformPos}");
                Debug.Log($"[ForceAllSpawnRefs] _SpawnPlatform → {platformPos}");
                patched++;
            }
            else
            {
                sb.AppendLine("– _SpawnPlatform: not in scene (use 'PlayerSpawner Position' menu to create)");
            }

            // 4. _FallSafetyFloor — untouched per spec (already correct at y=-20)
            var safety = GameObject.Find("_FallSafetyFloor");
            sb.AppendLine(safety != null
                ? "= _FallSafetyFloor: untouched (already correct)"
                : "– _FallSafetyFloor: not in scene (use 'Add Fall-Through Safety Net' menu)");

            if (haveScene)
            {
                EditorSceneManager.MarkSceneDirty(targetScene);
                EditorSceneManager.SaveScene(targetScene);
                sb.AppendLine($"\nScene saved: {targetScene.name}");
            }
            else
            {
                sb.AppendLine("\nNo scene dirtied (nothing to patch).");
            }

            string summary = $"Patched {patched} spawn ref(s) → (0, 2, 15):\n\n{sb}";
            Debug.Log("[ForceAllSpawnRefs]\n" + summary);
            EditorUtility.DisplayDialog("Force All Spawn Refs", summary, "OK");
        }

        [MenuItem("Tartaria/8 Fix/Add Fall-Through Safety Net", false, 52)]
        public static void AddSafetyNetFloor()
        {
            string nameTag = "_FallSafetyFloor";
            var existing = GameObject.Find(nameTag);
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Safety Net", "Already exists.", "OK");
                return;
            }
            var go = new GameObject(nameTag);
            go.transform.position = new Vector3(0f, -20f, 0f);
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(500f, 1f, 500f);
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(go.scene);
            EditorSceneManager.SaveScene(go.scene);
            EditorUtility.DisplayDialog("Safety Net", "Added 500x500 invisible floor at y=-20.", "OK");
        }

        [MenuItem("Tartaria/8 Fix/Toggle Camera Y Inversion", false, 60)]
        public static void ToggleCameraYInversion()
        {
            bool current = CameraController.InvertCameraY;
            CameraController.SetInvertCameraY(!current);
            string state = (!current) ? "INVERTED (push up = look down)" : "NORMAL (push up = look up)";
            EditorUtility.DisplayDialog("Camera Y", $"Y axis is now {state}.\n\nSaved to PlayerPrefs.", "OK");
        }

        [MenuItem("Tartaria/8 Fix/Toggle Camera X Inversion", false, 61)]
        public static void ToggleCameraXInversion()
        {
            bool current = CameraController.InvertCameraX;
            CameraController.SetInvertCameraX(!current);
            string state = (!current) ? "INVERTED (push right = orbit left)" : "NORMAL (push right = orbit right)";
            EditorUtility.DisplayDialog("Camera X", $"X axis is now {state}.\n\nSaved to PlayerPrefs.", "OK");
        }
    }
}
