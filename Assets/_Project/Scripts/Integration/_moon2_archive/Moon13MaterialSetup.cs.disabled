using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Material Setup — The Aether Convergence
    /// FINAL LEVEL — Apply premium materials: golden aether, tribute platforms, epic radiance
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon13MaterialSetup : MonoBehaviour
    {
        Material aetherMat;
        Material goldenMat;
        Material crystalMat;
        Material[] tributeMats = new Material[12];

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  ✨ MOON 13 MATERIAL SETUP — The Aether Convergence ✨");
            Debug.Log("  FINAL LEVEL MATERIALS");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            // Aether core material (bright cyan-white energy)
            aetherMat = CreateMaterial("AetherCore", new Color(0.95f, 1f, 1f));
            aetherMat.EnableKeyword("_EMISSION");
            aetherMat.SetColor("_EmissionColor", new Color(1.9f, 2f, 2f)); // Brilliant glow
            aetherMat.SetFloat("_Smoothness", 1f);
            aetherMat.SetFloat("_Metallic", 0.5f);

            // Golden spiral material
            goldenMat = CreateMaterial("GoldenSpiral", new Color(1f, 0.9f, 0.6f));
            goldenMat.SetFloat("_Metallic", 0.7f);
            goldenMat.SetFloat("_Smoothness", 0.9f);

            // Crystal pillar material
            crystalMat = CreateMaterial("AetherCrystal", new Color(0.9f, 0.95f, 1f));
            crystalMat.SetFloat("_Smoothness", 1f);
            crystalMat.SetFloat("_Metallic", 0.2f);

            // 12 Tribute platform materials (one for each moon)
            Color[] moonColors = {
                new Color(1f, 0.9f, 0.7f),    // Moon 1: Warm golden
                new Color(0.5f, 0.7f, 1f),    // Moon 2: Blue cavern
                new Color(0.4f, 0.8f, 0.3f),  // Moon 3: Green jungle
                new Color(1f, 0.85f, 0.6f),   // Moon 4: Desert gold
                new Color(0.7f, 0.85f, 1f),   // Moon 5: Ice blue
                new Color(1f, 0.4f, 0.1f),    // Moon 6: Lava orange
                new Color(0.2f, 0.6f, 0.8f),  // Moon 7: Deep water
                new Color(0.9f, 0.95f, 1f),   // Moon 8: Sky white
                new Color(0.6f, 0.2f, 0.8f),  // Moon 9: Corruption purple
                new Color(0.7f, 0.8f, 0.9f),  // Moon 10: Time neutral
                new Color(1f, 1f, 1f),        // Moon 11: Prismatic white
                new Color(0.3f, 0.2f, 0.5f)   // Moon 12: Shadow dark
            };

            for (int i = 0; i < 12; i++)
            {
                tributeMats[i] = CreateMaterial($"Tribute_Moon{i + 1}", moonColors[i]);
                tributeMats[i].EnableKeyword("_EMISSION");
                tributeMats[i].SetColor("_EmissionColor", moonColors[i] * 1.2f);
                tributeMats[i].SetFloat("_Smoothness", 0.8f);
                tributeMats[i].SetFloat("_Metallic", 0.4f);
            }

            // Apply to level geometry
            ApplyMaterialToChildren("AetherCore", aetherMat);
            ApplyMaterialToChildren("SpiralPath", goldenMat);
            ApplyMaterialToChildren("Pillar", crystalMat);
            ApplyMaterialToChildren("FinalAltar", goldenMat);
            ApplyMaterialToChildren("EnergyConduit", aetherMat);
            ApplyMaterialToChildren("AetherShard", crystalMat);

            // Apply tribute platform materials
            for (int i = 0; i < 12; i++)
            {
                ApplyMaterialToChildren($"TributePlatform_{i}", tributeMats[i]);
                ApplyMaterialToChildren($"Tribute_Moon{i + 1}", tributeMats[i]);
            }

            Debug.Log("[Moon13MaterialSetup] ✅ Final level materials applied!");
            Debug.Log("  • Aether Core (brilliant cyan-white)");
            Debug.Log("  • Golden Spiral (φ ratio golden material)");
            Debug.Log("  • 12 Tribute platforms (all moon colors)");
            Debug.Log("  • 108 Crystal pillars (3 concentric rings)");
            Debug.Log("  • Final Altar (peak golden material)");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon13_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_Metallic", 0.3f);
            return mat;
        }

        void ApplyMaterialToChildren(string namePattern, Material mat)
        {
            foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj.name.Contains(namePattern))
                {
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer != null) renderer.material = mat;
                }
            }
        }
    }
}
