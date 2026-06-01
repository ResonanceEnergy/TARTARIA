#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutEnvironment — places the 4 Moon 1 POIs per docs/15 §7:
    ///   1. Mud Pools (handled separately by Moon1MudPoolPuzzle bootstrap — skipped here)
    ///   2. Carved Stone (handled by SkeletonAtCarvedStone scene GO — skipped here)
    ///   3. Overlook — south ridge vista, +10 RS discovery reward
    ///   4. Root Chamber — underground aether glow, +5 RS discovery reward
    ///
    /// Each POI uses the runtime PointOfInterest component (Integration/PointOfInterest.cs).
    /// </summary>
    public static class Moon1BuildOutEnvironment
    {
        [MenuItem("Tartaria/1 Build/Build Out Moon 1 Environment (POIs)", priority = 106)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Environment", "No active scene.", "OK");
                return;
            }

            var parent = GameObject.Find("Echohaven_POIs");
            if (parent == null)
            {
                parent = new GameObject("Echohaven_POIs");
                Undo.RegisterCreatedObjectUndo(parent, "Create POIs parent");
            }

            int placed = 0, repositioned = 0;

            placed += BuildPOI(ref repositioned, parent,
                id: "POI_Overlook",
                title: "The Overlook",
                dialogue: "Above the mist, the world remembers itself.",
                position: new Vector3(200f, 30f, 0f),
                triggerRadius: 8f,
                rsReward: 10,
                visualPrefabPath: null,
                emissive: false);

            placed += BuildPOI(ref repositioned, parent,
                id: "POI_RootChamber",
                title: "The Root Chamber",
                dialogue: "Down here, the Aether hums in its native key.",
                position: new Vector3(-100f, -3f, 50f),
                triggerRadius: 8f,
                rsReward: 5,
                visualPrefabPath: null,
                emissive: true);

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            EditorUtility.DisplayDialog("Build Out Moon 1 Environment",
                $"POIs:\n  Placed new: {placed}\n  Repositioned existing: {repositioned}\n\n" +
                "Mud Pools (3) handled by Moon1MudPoolPuzzle bootstrap.\n" +
                "Carved Stone handled by SkeletonAtCarvedStone scene object.",
                "OK");
        }

        static int BuildPOI(ref int repositioned, GameObject parent,
            string id, string title, string dialogue,
            Vector3 position, float triggerRadius, int rsReward,
            string visualPrefabPath, bool emissive)
        {
            var existing = FindInChildrenByName(parent.transform, id);
            GameObject go;
            bool isNew;
            if (existing != null)
            {
                go = existing;
                Undo.RecordObject(go.transform, "Reposition POI");
                go.transform.position = position;
                repositioned++;
                isNew = false;
            }
            else
            {
                go = new GameObject(id);
                Undo.RegisterCreatedObjectUndo(go, "Create POI");
                go.transform.SetParent(parent.transform);
                go.transform.position = position;
                isNew = true;
            }

            // Trigger sphere
            var col = go.GetComponent<SphereCollider>();
            if (col == null) col = go.AddComponent<SphereCollider>();
            col.radius = triggerRadius;
            col.isTrigger = true;

            // PointOfInterest behaviour
            var poi = go.GetComponent<PointOfInterest>();
            if (poi == null) poi = go.AddComponent<PointOfInterest>();
            poi.Configure(id, rsReward, title, dialogue);

            // Optional visual marker (subtle glow sphere if emissive)
            if (emissive && go.transform.Find("AetherGlow") == null)
            {
                var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glow.name = "AetherGlow";
                glow.transform.SetParent(go.transform, worldPositionStays: false);
                glow.transform.localScale = Vector3.one * 1.5f;
                // URP-safe Lit material with emission
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                var mat = new Material(shader);
                mat.SetColor("_BaseColor", new Color(0.2f, 0.85f, 1f, 1f));
                mat.SetColor("_EmissionColor", new Color(0.1f, 0.6f, 1f) * 2.5f);
                mat.EnableKeyword("_EMISSION");
                glow.GetComponent<Renderer>().sharedMaterial = mat;
                var glowCol = glow.GetComponent<Collider>();
                if (glowCol != null) Object.DestroyImmediate(glowCol);
            }

            return isNew ? 1 : 0;
        }

        static GameObject FindInChildrenByName(Transform t, string name)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (c.name == name) return c.gameObject;
            }
            return null;
        }
    }
}
#endif
