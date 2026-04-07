using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Post-build scene validation — iterates every scene in Build Settings
    /// and checks each MonoBehaviour against <see cref="SceneComponentManifest"/>
    /// to catch components placed in the wrong scene.
    /// </summary>
    public static class SceneValidator
    {
        /// <summary>
        /// Validate all scenes in Build Settings. Returns the number of violations found.
        /// </summary>
        public static int ValidateAll()
        {
            int violations = 0;
            var scenes = EditorBuildSettings.scenes;

            foreach (var buildScene in scenes)
            {
                if (!buildScene.enabled) continue;

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset == null) continue;

                string sceneName = sceneAsset.name;
                var scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var components = root.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (var mb in components)
                    {
                        if (mb == null) continue;
                        var type = mb.GetType();
                        if (SceneComponentManifest.IsForbiddenInScene(type, sceneName))
                        {
                            Debug.LogError(
                                $"[SceneValidator] FORBIDDEN: {type.Name} found in scene '{sceneName}' " +
                                $"on GameObject '{mb.gameObject.name}'. " +
                                $"This component belongs elsewhere — see SceneComponentManifest.");
                            violations++;
                        }
                    }
                }
            }

            if (violations == 0)
                Debug.Log("[SceneValidator] All scenes clean — no forbidden components found.");

            return violations;
        }
    }
}
