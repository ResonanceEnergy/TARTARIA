using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.EditorTools
{
    /// <summary>
    /// Generates a procedural application icon (radial gold/cyan glyph on dark
    /// background) and assigns it to PlayerSettings as the standalone icon.
    /// Idempotent - skips if icon already present.
    /// </summary>
    public static class AppIconFactory
    {
        const string IconPath = "Assets/_Project/Branding/TartariaIcon.png";
        const int IconSize = 512;

        [MenuItem("Tartaria/Fix/Generate App Icon")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Branding"))
                AssetDatabase.CreateFolder("Assets/_Project", "Branding");

            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (existing == null)
            {
                var tex = BuildIcon(IconSize);
                File.WriteAllBytes(IconPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport);

                var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
                existing = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            }

            if (existing == null)
            {
                Debug.LogError("[AppIconFactory] Failed to create or load icon texture.");
                return;
            }

            // Assign to all standalone icon kinds.
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, new[] { existing });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,    new[] { existing });

            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AppIconFactory] App icon set to {IconPath}");
        }

        static Texture2D BuildIcon(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            Color bgInner = new Color(0.04f, 0.06f, 0.12f, 1f);
            Color bgOuter = new Color(0.01f, 0.01f, 0.03f, 1f);
            Color goldHi  = new Color(1.00f, 0.86f, 0.42f, 1f);
            Color goldLo  = new Color(0.62f, 0.42f, 0.10f, 1f);
            Color cyan    = new Color(0.30f, 0.85f, 1.00f, 1f);

            float cx = size * 0.5f;
            float cy = size * 0.5f;
            float maxR = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float n = r / maxR;

                    Color c;
                    if (n > 1f)
                    {
                        c = new Color(0, 0, 0, 0); // Transparent corners (rounded)
                    }
                    else
                    {
                        // Background radial gradient
                        c = Color.Lerp(bgInner, bgOuter, n);

                        // Outer gold ring
                        if (n > 0.86f && n < 0.96f)
                            c = Color.Lerp(goldLo, goldHi, 1f - Mathf.Abs(n - 0.91f) / 0.05f);

                        // 13-pointed star (13 moons)
                        float angle = Mathf.Atan2(dy, dx);
                        float petal = 0.5f + 0.5f * Mathf.Cos(angle * 13f);
                        float petalRadius = 0.35f + petal * 0.18f;
                        if (Mathf.Abs(n - petalRadius) < 0.025f)
                            c = goldHi;

                        // Central cyan core
                        if (n < 0.18f)
                            c = Color.Lerp(cyan, Color.white, 1f - n / 0.18f);

                        // Inner gold ring
                        if (n > 0.22f && n < 0.26f)
                            c = goldLo;
                    }

                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
