using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Editor
{
    /// <summary>
    /// Builds the EchohavenVolumeProfile (Bloom, Vignette, Tonemapping, ColorAdjustments,
    /// ChromaticAberration, FilmGrain) and adds a Global Volume to the scene.
    /// </summary>
    public static class PostFXVolumeFactory
    {
        public const string ProfilePath = "Assets/_Project/Config/EchohavenVolumeProfile.asset";

        [MenuItem("Tartaria/Build Assets/Post-FX Volume", false, 12)]
        public static void BuildVolumeProfile()
        {
            EnsureFolder("Assets/_Project/Config");

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            // Strip any old components so we always rebuild fresh
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(profile.components[i], true);
            }
            profile.components.Clear();

            // Bloom — soft halo on emissive surfaces (Feature 1: intensity 0.3, threshold 0.9)
            var bloom = profile.Add<UnityEngine.Rendering.Universal.Bloom>(true);
            bloom.intensity.Override(0.3f);
            bloom.threshold.Override(0.9f);
            bloom.scatter.Override(0.7f);
            AssetDatabase.AddObjectToAsset(bloom, profile);

            // Tonemapping (Feature 1: ACES)
            var tonemap = profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true);
            tonemap.mode.Override(UnityEngine.Rendering.Universal.TonemappingMode.ACES);
            AssetDatabase.AddObjectToAsset(tonemap, profile);

            // Vignette — pulls focus to centre (Feature 1: intensity 0.2)
            var vignette = profile.Add<UnityEngine.Rendering.Universal.Vignette>(true);
            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.4f);
            AssetDatabase.AddObjectToAsset(vignette, profile);

            // Color Adjustments (optional enhancement)
            var color = profile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
            color.postExposure.Override(0.15f);
            color.contrast.Override(12f);
            color.saturation.Override(14f);
            color.colorFilter.Override(new Color(1.02f, 1.0f, 0.95f));
            AssetDatabase.AddObjectToAsset(color, profile);

            // Chromatic Aberration — subtle lens fringing
            var chromatic = profile.Add<UnityEngine.Rendering.Universal.ChromaticAberration>(true);
            chromatic.intensity.Override(0.12f);
            AssetDatabase.AddObjectToAsset(chromatic, profile);

            // Film Grain — gentle organic noise
            var grain = profile.Add<UnityEngine.Rendering.Universal.FilmGrain>(true);
            grain.type.Override(UnityEngine.Rendering.Universal.FilmGrainLookup.Thin1);
            grain.intensity.Override(0.18f);
            grain.response.Override(0.85f);
            AssetDatabase.AddObjectToAsset(grain, profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PostFX] Global volume configured: Bloom (0.3, threshold 0.9), Tonemapping (ACES), Vignette (0.2)");
        }

        /// <summary>
        /// Ensure a Global Volume GameObject exists in the open scene, referencing
        /// the EchohavenVolumeProfile. Idempotent.
        /// </summary>
        public static void EnsureSceneVolume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                BuildVolumeProfile();
                profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            }

            var existing = GameObject.Find("PostFX_Volume");
            if (existing == null)
            {
                existing = new GameObject("PostFX_Volume");
                existing.layer = 0;
            }

            var volume = existing.GetComponent<Volume>();
            if (volume == null) volume = existing.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1;
            volume.weight = 1f;
            volume.sharedProfile = profile;

            // Ensure the main camera has post-processing enabled
            var camGo = GameObject.Find("CameraRig");
            if (camGo != null)
            {
                var cam = camGo.GetComponent<UnityEngine.Camera>();
                if (cam != null)
                {
                    var data = cam.GetUniversalAdditionalCameraData();
                    if (data != null)
                    {
                        data.renderPostProcessing = true;
                        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        data.antialiasingQuality = AntialiasingQuality.High;
                    }
                }
            }

            Debug.Log("[PostFX] PostFX_Volume placed in scene; camera post-processing enabled.");
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
