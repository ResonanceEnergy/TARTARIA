using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// One-shot Editor utility to configure post-processing volume for Echohaven scene.
    /// Adds Bloom, Tonemapping (ACES), and Vignette to the global volume profile.
    /// </summary>
    public static class ConfigurePostProcessing
    {
        [MenuItem("Tartaria/Configure Post-Processing Volume")]
        public static void ConfigureVolume()
        {
            // Load the VolumeProfile asset
            string profilePath = "Assets/_Project/Config/EchohavenVolumeProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            
            if (profile == null)
            {
                Debug.LogError($"[PostFX] VolumeProfile not found at {profilePath}");
                return;
            }

            // Clear existing components (they're all null anyway)
            profile.components.Clear();

            // Add Bloom
            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
            }
            bloom.intensity.Override(0.3f);
            bloom.threshold.Override(0.9f);
            bloom.scatter.Override(0.7f);
            
            // Add Tonemapping (ACES)
            if (!profile.TryGet<Tonemapping>(out var tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(true);
            }
            tonemapping.mode.Override(TonemappingMode.ACES);

            // Add Vignette
            if (!profile.TryGet<Vignette>(out var vignette))
            {
                vignette = profile.Add<Vignette>(true);
            }
            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.4f);

            // Mark dirty and save
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PostFX] Global volume configured: Bloom (0.3, threshold 0.9), Tonemapping (ACES), Vignette (0.2)");

            // Now ensure the scene has a Volume GameObject
            EnsureSceneVolume(profile);
        }

        private static void EnsureSceneVolume(VolumeProfile profile)
        {
            // Open Echohaven scene if not already open
            string scenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            var currentScene = EditorSceneManager.GetActiveScene();
            
            if (currentScene.path != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Debug.Log($"[PostFX] Opened scene: {scenePath}");
            }

            // Check if Global Volume already exists
            Volume existingVolume = GameObject.FindFirstObjectByType<Volume>();
            
            if (existingVolume != null)
            {
                existingVolume.profile = profile;
                existingVolume.isGlobal = true;
                existingVolume.priority = 0;
                EditorUtility.SetDirty(existingVolume.gameObject);
                Debug.Log($"[PostFX] Updated existing Volume on GameObject: {existingVolume.gameObject.name}");
            }
            else
            {
                // Create new Volume GameObject
                GameObject volumeGO = new GameObject("Global Volume");
                Volume volume = volumeGO.AddComponent<Volume>();
                volume.profile = profile;
                volume.isGlobal = true;
                volume.priority = 0;
                
                UnityEditor.Undo.RegisterCreatedObjectUndo(volumeGO, "Create Global Volume");
                Debug.Log("[PostFX] Created new Global Volume GameObject in scene");
            }

            // Save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[PostFX] Scene saved with Volume configuration");
        }
    }
}
