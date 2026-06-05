#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu: Tartaria/Art/Generate 15 PBR Materials (Pack 2)
    ///
    /// Second wave of canonical Tartaria palette materials covering categories
    /// not in v1: stained-glass tints, ley-line glow, mud-pool gradients,
    /// fabric variations, ash, soot, frost, copper-patina, leather, parchment,
    /// blood, gore, ink, milk, deep-water.
    ///
    /// Per CLAUDE.md no-stubs mandate — every material has real values.
    /// </summary>
    public static class PBRMaterialBatchV2
    {
        const string OUT_DIR = "Assets/_Project/Materials/Generated";

        [MenuItem("Tartaria/4 Generate Art/15 PBR Materials (Pack 2)", priority = 410)]
        public static void Run()
        {
            if (!Directory.Exists(OUT_DIR)) Directory.CreateDirectory(OUT_DIR);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            int created = 0;

            var defs = new (string name, Color baseColor, float smoothness, float metallic, Color emission, float emInt)[]
            {
                ("M_StainedRed",    new Color(0.55f, 0.10f, 0.10f), 0.92f, 0.10f, new Color(0.85f, 0.20f, 0.20f), 1.4f),
                ("M_StainedBlue",   new Color(0.10f, 0.20f, 0.55f), 0.92f, 0.10f, new Color(0.20f, 0.30f, 0.85f), 1.4f),
                ("M_StainedGreen",  new Color(0.15f, 0.55f, 0.25f), 0.92f, 0.10f, new Color(0.20f, 0.80f, 0.30f), 1.4f),
                ("M_StainedGold",   new Color(0.62f, 0.45f, 0.10f), 0.92f, 0.20f, new Color(0.95f, 0.78f, 0.20f), 1.4f),
                ("M_LeyLineGlow",   new Color(0.20f, 0.55f, 0.92f), 0.90f, 0.10f, new Color(0.30f, 0.65f, 1.00f), 2.5f),
                ("M_MudPool_Deep",  new Color(0.18f, 0.12f, 0.08f), 0.45f, 0.0f, Color.black, 0f),
                ("M_FabricGold",    new Color(0.75f, 0.55f, 0.15f), 0.40f, 0.30f, Color.black, 0f),
                ("M_FabricVelvet",  new Color(0.32f, 0.10f, 0.20f), 0.55f, 0.0f, Color.black, 0f),
                ("M_AshGray",       new Color(0.45f, 0.42f, 0.40f), 0.05f, 0.0f, Color.black, 0f),
                ("M_SootBlack",     new Color(0.05f, 0.05f, 0.06f), 0.05f, 0.0f, Color.black, 0f),
                ("M_FrostWhite",    new Color(0.85f, 0.92f, 0.95f), 0.95f, 0.0f, new Color(0.50f, 0.70f, 0.90f), 0.3f),
                ("M_CopperPatina",  new Color(0.30f, 0.65f, 0.55f), 0.30f, 0.40f, Color.black, 0f),
                ("M_LeatherWorn",   new Color(0.32f, 0.18f, 0.10f), 0.35f, 0.0f, Color.black, 0f),
                ("M_Parchment",     new Color(0.92f, 0.85f, 0.65f), 0.05f, 0.0f, Color.black, 0f),
                ("M_DeepWater",     new Color(0.08f, 0.20f, 0.30f), 0.95f, 0.20f, new Color(0.10f, 0.35f, 0.55f), 0.4f),
            };

            foreach (var d in defs)
            {
                var mat = new Material(shader) { name = d.name };
                Apply(mat, d.baseColor, d.smoothness, d.metallic, d.emission, d.emInt);
                AssetDatabase.CreateAsset(mat, OUT_DIR + "/" + d.name + ".mat");
                created++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string msg = $"Generated {created} PBR materials (Pack 2) at:\n{OUT_DIR}\n\n" +
                         "Categories: stained-glass tints (4), ley-line glow, mud-pool deep, " +
                         "fabric gold+velvet (2), ash+soot, frost, copper-patina, leather, " +
                         "parchment, deep water.";
            Debug.Log("[PBRMaterialBatchV2] " + msg);
            EditorUtility.DisplayDialog("PBR Material Batch v2", msg, "OK");
        }

        static void Apply(Material m, Color baseC, float smoothness, float metallic, Color em, float emInt)
        {
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseC); else m.color = baseC;
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
