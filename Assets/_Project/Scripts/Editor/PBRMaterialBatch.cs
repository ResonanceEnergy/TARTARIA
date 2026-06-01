#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu: Tartaria/Art/Generate 20 PBR Materials
    ///
    /// Creates 20 URP/Lit materials covering the canonical Tartaria palette:
    /// stone, mud, metal, wood, crystal, fabric, glow. Saved to
    /// Assets/_Project/Materials/Generated/.
    ///
    /// Per CLAUDE.md no-stubs mandate — every material has real shader, color,
    /// roughness, metallic, emission. No placeholder colors.
    /// </summary>
    public static class PBRMaterialBatch
    {
        const string OUT_DIR = "Assets/_Project/Materials/Generated";

        [MenuItem("Tartaria/4 Generate Art/20 PBR Materials", priority = 400)]
        public static void Run()
        {
            if (!Directory.Exists(OUT_DIR))
                Directory.CreateDirectory(OUT_DIR);

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            int created = 0;
            // (name, baseColor, smoothness, metallic, emission, emissionStrength)
            var defs = new (string name, Color baseColor, float smoothness, float metallic, Color emission, float emInt)[]
            {
                ("M_AetherGlow_Blue",      new Color(0.30f, 0.55f, 0.95f), 0.85f, 0.2f, new Color(0.40f, 0.70f, 1.00f), 2.0f),
                ("M_AetherGlow_Amber",     new Color(0.95f, 0.65f, 0.20f), 0.85f, 0.2f, new Color(1.00f, 0.70f, 0.20f), 2.0f),
                ("M_AetherGlow_Green",     new Color(0.40f, 0.85f, 0.50f), 0.85f, 0.2f, new Color(0.50f, 0.95f, 0.55f), 2.0f),
                ("M_MudSurface_Wet",       new Color(0.30f, 0.20f, 0.12f), 0.55f, 0.0f, Color.black, 0f),
                ("M_StoneMoss",            new Color(0.45f, 0.50f, 0.32f), 0.20f, 0.0f, Color.black, 0f),
                ("M_StoneCracked",         new Color(0.55f, 0.52f, 0.48f), 0.25f, 0.0f, Color.black, 0f),
                ("M_PlasterCream",         new Color(0.92f, 0.85f, 0.72f), 0.20f, 0.0f, Color.black, 0f),
                ("M_WoodPlank",            new Color(0.45f, 0.30f, 0.18f), 0.30f, 0.0f, Color.black, 0f),
                ("M_WoodCharred",          new Color(0.12f, 0.10f, 0.08f), 0.25f, 0.0f, Color.black, 0f),
                ("M_Marble_White",         new Color(0.95f, 0.92f, 0.85f), 0.85f, 0.0f, Color.black, 0f),
                ("M_BrassPolished",        new Color(0.78f, 0.60f, 0.28f), 0.80f, 0.85f, Color.black, 0f),
                ("M_IronRusted",           new Color(0.45f, 0.25f, 0.15f), 0.25f, 0.45f, Color.black, 0f),
                ("M_MercuryLiquid",        new Color(0.85f, 0.88f, 0.92f), 0.95f, 0.95f, Color.black, 0f),
                ("M_CrystalClear",         new Color(0.92f, 0.95f, 0.98f), 0.95f, 0.10f, new Color(0.30f, 0.50f, 0.75f), 0.4f),
                ("M_CrystalAmber",         new Color(0.95f, 0.65f, 0.20f), 0.92f, 0.10f, new Color(0.95f, 0.55f, 0.10f), 1.5f),
                ("M_CrystalBlue",          new Color(0.30f, 0.50f, 0.92f), 0.92f, 0.10f, new Color(0.20f, 0.50f, 1.00f), 1.5f),
                ("M_CymaticDust",          new Color(0.92f, 0.85f, 0.65f), 0.10f, 0.0f, Color.black, 0f),
                ("M_BurnedWood",           new Color(0.15f, 0.10f, 0.06f), 0.20f, 0.0f, new Color(0.40f, 0.10f, 0.0f), 0.3f),
                ("M_AuroraSky_Tint",       new Color(0.45f, 0.65f, 0.85f), 0.90f, 0.0f, new Color(0.55f, 0.80f, 0.95f), 1.0f),
                ("M_FabricRedRoyal",       new Color(0.55f, 0.12f, 0.18f), 0.15f, 0.0f, Color.black, 0f),
            };

            foreach (var d in defs)
            {
                var mat = new Material(shader) { name = d.name };
                Apply(mat, d.baseColor, d.smoothness, d.metallic, d.emission, d.emInt);
                string path = OUT_DIR + "/" + d.name + ".mat";
                AssetDatabase.CreateAsset(mat, path);
                created++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg = $"Generated {created} PBR materials at:\n{OUT_DIR}\n\n" +
                         "Categories: Aether glow (3), mud/stone (3), plaster (1), wood (3), " +
                         "marble (1), metals (3), crystal (3), special FX (3).";
            Debug.Log("[PBRMaterialBatch] " + msg);
            EditorUtility.DisplayDialog("PBR Material Batch", msg, "OK");
        }

        static void Apply(Material m, Color baseC, float smoothness, float metallic, Color em, float emInt)
        {
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseC);
            else m.color = baseC;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (emInt > 0.001f)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", em * emInt);
            }
        }
    }
}
#endif
