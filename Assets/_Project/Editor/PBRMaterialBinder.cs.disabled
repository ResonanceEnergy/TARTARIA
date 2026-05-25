// PBRMaterialBinder.cs
// Auto-creates URP/Lit materials from ambientCG PBR texture sets.
// Each subfolder under Assets/_Project/Textures/PBR/{SetID}/ becomes one material
// at Assets/_Project/Materials/PBR/{SetID}.mat with all maps wired up.
// Runs as part of OneClickBuild Phase 13 alongside HDRI + Mixamo binders.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class PBRMaterialBinder
    {
        const string PBRDir       = "Assets/_Project/Textures/PBR";
        const string MatOutDir    = "Assets/_Project/Materials/PBR";
        const string LitShader    = "Universal Render Pipeline/Lit";

        [MenuItem("Tartaria/Setup/Bind PBR Materials", false, 62)]
        public static void BindAll()
        {
            if (!AssetDatabase.IsValidFolder(PBRDir))
            {
                Debug.Log("[Tartaria][PBR] No PBR texture folder yet — run Fetch-AmbientCG.py first.");
                return;
            }

            EnsureDirectory(MatOutDir);
            var shader = Shader.Find(LitShader);
            if (shader == null) { Debug.LogWarning("[Tartaria][PBR] URP/Lit shader missing."); return; }

            int built = 0, updated = 0;
            foreach (var subdir in Directory.GetDirectories(PBRDir))
            {
                string setId = Path.GetFileName(subdir);
                string matPath = $"{MatOutDir}/{setId}.mat";

                // Configure importers (linear for normal/rough/metal/AO, sRGB for color).
                ConfigureImporters(subdir);

                Texture color    = FindTex(subdir, "_Color");
                Texture normal   = FindTex(subdir, "_NormalGL", "_Normal");
                Texture rough    = FindTex(subdir, "_Roughness");
                Texture metal    = FindTex(subdir, "_Metalness", "_Metallic");
                Texture ao       = FindTex(subdir, "_AmbientOcclusion", "_AO");
                Texture height   = FindTex(subdir, "_Displacement", "_Height");

                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                bool isNew = mat == null;
                if (isNew) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); built++; }
                else updated++;

                if (color)  mat.SetTexture("_BaseMap", color);
                if (normal) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); }
                if (metal)  mat.SetTexture("_MetallicGlossMap", metal);
                if (ao)     { mat.SetTexture("_OcclusionMap", ao); mat.SetFloat("_OcclusionStrength", 1f); }
                if (height) { mat.SetTexture("_ParallaxMap", height); mat.EnableKeyword("_PARALLAXMAP"); }

                // URP/Lit uses smoothness, not roughness — invert via slider only (no auto pack).
                // If we have a roughness map, set smoothness slider low so the shader leans on map.
                if (rough) mat.SetFloat("_Smoothness", 0.5f);

                // Per-category tiling so 0..1 UVs on huge procedural meshes still show texture detail.
                Vector2 tile = TilingFor(setId);
                mat.SetTextureScale("_BaseMap", tile);
                mat.SetTextureScale("_BumpMap", tile);
                mat.SetTextureScale("_MetallicGlossMap", tile);
                mat.SetTextureScale("_OcclusionMap", tile);
                mat.SetTextureScale("_ParallaxMap", tile);

                EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Tartaria][PBR] Bound {built} new + {updated} existing materials in {MatOutDir}.");
        }

        static Vector2 TilingFor(string setId)
        {
            // Ground/terrain — tile aggressively across 200m mesh.
            if (setId.StartsWith("Ground") || setId.StartsWith("PavingStones"))
                return new Vector2(64f, 64f);
            // Walls / bricks / plaster on building scale (~10m).
            if (setId.StartsWith("Bricks") || setId.StartsWith("Plaster") || setId.StartsWith("Marble"))
                return new Vector2(4f, 4f);
            // Wood beams / planks.
            if (setId.StartsWith("Wood")) return new Vector2(3f, 3f);
            // Metal trim / panels — small repeating detail.
            if (setId.StartsWith("Metal")) return new Vector2(2f, 2f);
            // Rocks — natural irregular.
            if (setId.StartsWith("Rocks")) return new Vector2(2f, 2f);
            return new Vector2(4f, 4f);
        }

        static Texture FindTex(string dir, params string[] suffixes)
        {
            foreach (var suf in suffixes)
                foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".exr" })
                {
                    var matches = Directory.GetFiles(dir, $"*{suf}*{ext}");
                    if (matches.Length > 0)
                    {
                        var rel = matches[0].Replace("\\", "/");
                        var t = AssetDatabase.LoadAssetAtPath<Texture>(rel);
                        if (t != null) return t;
                    }
                }
            return null;
        }

        static void ConfigureImporters(string dir)
        {
            foreach (var f in Directory.GetFiles(dir))
            {
                if (!f.EndsWith(".png") && !f.EndsWith(".jpg") && !f.EndsWith(".jpeg")) continue;
                var rel = f.Replace("\\", "/");
                var imp = AssetImporter.GetAtPath(rel) as TextureImporter;
                if (imp == null) continue;

                bool isColor = rel.Contains("_Color");
                bool isNormal = rel.Contains("_Normal");
                bool changed = false;

                if (isNormal && imp.textureType != TextureImporterType.NormalMap)
                {
                    imp.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }
                bool wantSRGB = isColor;
                if (imp.sRGBTexture != wantSRGB)
                {
                    imp.sRGBTexture = wantSRGB;
                    changed = true;
                }
                if (changed) imp.SaveAndReimport();
            }
        }

        static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf   = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
