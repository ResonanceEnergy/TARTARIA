using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 11 Material Setup — The Prismatic Nexus
    /// Apply spectrum materials: rainbow crystals, refracted light, pure color chambers
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon11MaterialSetup : MonoBehaviour
    {
        Material[] spectrumMats = new Material[7];
        Material crystalMat;
        Material prismMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon11MaterialSetup] 🎨 Applying prismatic spectrum materials...");

            // Spectrum colors
            Color[] colors = {
                new Color(1f, 0f, 0f),       // Red
                new Color(1f, 0.5f, 0f),     // Orange
                new Color(1f, 1f, 0f),       // Yellow
                new Color(0f, 1f, 0f),       // Green
                new Color(0f, 1f, 1f),       // Cyan
                new Color(0f, 0f, 1f),       // Blue
                new Color(0.5f, 0f, 1f)      // Violet
            };

            // Create spectrum materials
            for (int i = 0; i < 7; i++)
            {
                spectrumMats[i] = CreateMaterial($"Spectrum{i}", colors[i]);
                spectrumMats[i].EnableKeyword("_EMISSION");
                spectrumMats[i].SetColor("_EmissionColor", colors[i] * 1.5f);
                spectrumMats[i].SetFloat("_Smoothness", 0.9f);
            }

            // Crystal and prism materials
            crystalMat = CreateMaterial("Crystal", Color.white);
            crystalMat.SetFloat("_Smoothness", 1f);
            crystalMat.SetFloat("_Metallic", 0f);

            prismMat = CreateMaterial("Prism", new Color(0.95f, 0.95f, 1f));
            prismMat.EnableKeyword("_EMISSION");
            prismMat.SetColor("_EmissionColor", Color.white * 2f);
            prismMat.SetFloat("_Smoothness", 1f);

            // Apply to level geometry
            ApplyMaterialToChildren("CentralPrism", prismMat);
            ApplyMaterialToChildren("Refractor", crystalMat);
            ApplyMaterialToChildren("Crystal", crystalMat);
            ApplyMaterialToChildren("Pillar", crystalMat);

            // Apply spectrum colors to chambers (0-6)
            for (int i = 0; i < 7; i++)
            {
                ApplyMaterialToChildren($"Chamber_{i}", spectrumMats[i]);
                ApplyMaterialToChildren($"ColorChamber_{i}", spectrumMats[i]);
            }

            Debug.Log("[Moon11MaterialSetup] ✅ Prismatic materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon11_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.1f);
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
