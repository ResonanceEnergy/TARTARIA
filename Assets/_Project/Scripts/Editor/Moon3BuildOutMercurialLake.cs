using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    public static class Moon3BuildOutMercurialLake
    {
        const string ROOT_NAME = "Moon3_MercurialLake_Root";

        [MenuItem("Tartaria/Moon 3/Build Out Mercurial Lake")]
        public static void Run()
        {
            var existing = GameObject.Find(ROOT_NAME);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Mercurial Lake Exists",
                    "Moon3_MercurialLake_Root already in scene. Rebuild from scratch?",
                    "Rebuild", "Cancel")) return;
                Object.DestroyImmediate(existing);
            }

            var root = new GameObject(ROOT_NAME);
            root.transform.position = new Vector3(-160f, 0f, 0f);

            BuildLake(root, new Vector3(  0f, 0f,   0f), 8f);
            BuildLake(root, new Vector3(-20f, 0f,  12f), 6f);
            BuildLake(root, new Vector3( 15f, 0f, -10f), 7f);

            BuildTrackFragments(root, 5);
            BuildEntryPortal(root, new Vector3(30f, 0f, 0f));

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[Moon3BuildOutMercurialLake] Built Moon 3 zone at " + root.transform.position);
        }

        static void BuildLake(GameObject parent, Vector3 localPos, float radius) { /* ... */ }
        static void BuildTrackFragments(GameObject parent, int count) { /* ... */ }
        static void BuildEntryPortal(GameObject parent, Vector3 localPos) { /* ... */ }
    }
}
