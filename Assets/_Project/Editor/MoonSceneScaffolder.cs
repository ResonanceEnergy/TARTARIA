using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon Scene Scaffolder — populates empty moon scenes with bootstrap components,
    /// fog volumes, ground planes, quest giver placeholders, boss spawn markers.
    /// Menu: Tartaria → Build → Scaffold All Empty Moons
    /// </summary>
    public static class MoonSceneScaffolder
    {
        [MenuItem("Tartaria/Build/Scaffold All Empty Moons")]
        public static void ScaffoldAllMoons()
        {
            Debug.Log("[MoonSceneScaffolder] Starting moon scene scaffold pass...");

            // Load all MoonDefinition assets
            string[] guids = AssetDatabase.FindAssets("t:MoonDefinition", new[] { "Assets/_Project/Config/Moons" });
            int scaffolded = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MoonDefinition moon = AssetDatabase.LoadAssetAtPath<MoonDefinition>(path);
                if (moon != null && ShouldScaffold(moon))
                {
                    ScaffoldMoon(moon);
                    scaffolded++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MoonSceneScaffolder] Scaffolded {scaffolded} moon scenes.");

            EditorUtility.DisplayDialog("Moon Scaffolder", 
                $"Scaffolded {scaffolded} moon scenes.\n\n" +
                $"Each moon now has:\n" +
                "• MoonRuntimeBootstrapper\n" +
                "• Fog volume\n" +
                "• Ground plane\n" +
                "• Quest giver placeholder\n" +
                "• Boss spawn marker", 
                "OK");
        }

        static bool ShouldScaffold(MoonDefinition moon)
        {
            // Check if scene exists and is empty (stub logic)
            if (string.IsNullOrEmpty(moon.sceneName))
                return false;

            // If scene doesn't exist in build settings, scaffold it
            bool foundInBuild = false;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains(moon.sceneName))
                {
                    foundInBuild = true;
                    break;
                }
            }

            return !foundInBuild; // Scaffold if not in build settings
        }

        static void ScaffoldMoon(MoonDefinition moon)
        {
            string scenePath = $"Assets/_Project/Scenes/Moons/{moon.sceneName}.unity";

            // Create or load scene
            Scene scene;
            if (System.IO.File.Exists(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, scenePath);
            }

            // Add MoonRuntimeBootstrapper
            var bootstrap = new GameObject("MoonRuntimeBootstrapper");
            bootstrap.AddComponent<MoonRuntimeBootstrapper>();

            // Add fog volume (placeholder - requires Volume component from URP)
            var fogVolume = new GameObject("FogVolume");
            // fogVolume.AddComponent<Volume>(); // Would require URP Volume setup

            // Add ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.localScale = new Vector3(20, 1, 20); // 200x200m

            // Add quest giver placeholder
            var questGiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            questGiver.name = "QuestGiver_Placeholder";
            questGiver.transform.position = new Vector3(5, 1, 5);
            // questGiver.AddComponent<QuestGiverInteractable>(); // Would need to set quest ID

            // Add boss spawn marker
            var bossMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bossMarker.name = "BossSpawnMarker";
            bossMarker.transform.position = new Vector3(0, 0, 50);
            bossMarker.transform.localScale = Vector3.one * 2f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // Add to build settings if not present
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool inBuild = false;
            foreach (var s in buildScenes)
            {
                if (s.path == scenePath)
                {
                    inBuild = true;
                    break;
                }
            }
            if (!inBuild)
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            Debug.Log($"[MoonSceneScaffolder] Scaffolded {moon.sceneName}");
        }
    }
}
