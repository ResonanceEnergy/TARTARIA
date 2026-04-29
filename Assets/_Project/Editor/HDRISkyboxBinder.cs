// HDRISkyboxBinder.cs
// Auto-binds the latest fetched Poly Haven HDRI as the active skybox.
// Runs as part of OneClickBuild Phase 13.
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tartaria.Editor
{
    public static class HDRISkyboxBinder
    {
        const string HDRIDir       = "Assets/_Project/Textures/HDRI";
        const string SkyboxMatPath = "Assets/_Project/Materials/HDRISkybox.mat";
        const string DefaultHDRI   = "kloofendal_43d_clear_puresky";

        [MenuItem("Tartaria/Setup/Bind HDRI Skybox", false, 60)]
        public static void BindLatestHDRI()
        {
            if (!AssetDatabase.IsValidFolder(HDRIDir))
            {
                Debug.Log("[Tartaria][HDRI] No HDRI folder yet — run OpenClaw-FetchAssets.ps1 first.");
                return;
            }

            // Prefer default; otherwise first .hdr found.
            string preferred = $"{HDRIDir}/{DefaultHDRI}.hdr";
            string chosen = File.Exists(preferred) ? preferred : null;
            if (chosen == null)
            {
                var candidates = Directory.GetFiles(HDRIDir, "*.hdr");
                if (candidates.Length == 0)
                {
                    Debug.Log("[Tartaria][HDRI] No .hdr files present — skipping.");
                    return;
                }
                chosen = candidates[0].Replace("\\", "/");
            }

            // Force Cubemap import settings on the .hdr so it can drive the skybox.
            var importer = AssetImporter.GetAtPath(chosen) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureShape != TextureImporterShape.TextureCube)
                {
                    importer.textureShape = TextureImporterShape.TextureCube;
                    importer.generateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
                    importer.sRGBTexture = false;
                    importer.SaveAndReimport();
                }
            }

            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(chosen);
            if (cube == null)
            {
                Debug.LogWarning($"[Tartaria][HDRI] Could not load as Cubemap: {chosen}");
                return;
            }

            // Create or update HDRISkybox material.
            var shader = Shader.Find("Skybox/Cubemap");
            if (shader == null)
            {
                Debug.LogWarning("[Tartaria][HDRI] Skybox/Cubemap shader missing.");
                return;
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMatPath);
            if (mat == null)
            {
                EnsureDirectory("Assets/_Project/Materials");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, SkyboxMatPath);
            }
            mat.SetTexture("_Tex", cube);
            mat.SetFloat("_Exposure", 1.2f);

            RenderSettings.skybox = mat;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            DynamicGI.UpdateEnvironment();

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Tartaria][HDRI] Bound skybox to {Path.GetFileName(chosen)}");
        }

        static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
