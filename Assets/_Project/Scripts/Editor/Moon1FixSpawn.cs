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
            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            EditorSceneManager.SaveScene(spawner.gameObject.scene);

            string msg = $"PlayerSpawner moved to {chosenPos}, facing north.\n" +
                         $"Created 14×14 brown platform under spawn at y={chosenPos.y - 1.5f}.\n\n" +
                         $"Exit Play Mode if running, then re-enter to spawn on solid ground with the buildings ahead.";
            Debug.Log("[Moon1FixSpawn] " + msg);
            EditorUtility.DisplayDialog("Spawn Fix", msg, "OK");
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
