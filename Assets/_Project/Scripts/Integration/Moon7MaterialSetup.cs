using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Material Setup — The Abyssal Depths
    /// Apply underwater materials: coral, wet stone, bioluminescent organisms
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon7MaterialSetup : MonoBehaviour
    {
        Material coralMat;
        Material wetStoneMat;
        Material bioMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon7MaterialSetup] 🎨 Applying underwater materials...");

            // Abyssal palette
            coralMat = CreateMaterial("Rocks023", new Color(0.4f, 0.6f, 0.7f)); // Blue-gray coral
            wetStoneMat = CreateMaterial("PavingStones150", new Color(0.2f, 0.3f, 0.4f)); // Dark wet stone
            bioMat = CreateMaterial("Marble006", new Color(0.2f, 0.8f, 1f)); // Cyan bioluminescence

            wetStoneMat.SetFloat("_Smoothness", 0.7f); // Wet and slick
            bioMat.EnableKeyword("_EMISSION");
            bioMat.SetColor("_EmissionColor", new Color(0.4f, 1.6f, 2f)); // Bright cyan glow

            // Apply to level geometry
            ApplyMaterialToChildren("CoralFormation", coralMat);
            ApplyMaterialToChildren("RuinWall", wetStoneMat);
            ApplyMaterialToChildren("SeaFloor", wetStoneMat);
            ApplyMaterialToChildren("BiolumCoral", bioMat);
            ApplyMaterialToChildren("Kelp", coralMat);
            ApplyMaterialToChildren("Trench", wetStoneMat);
            ApplyMaterialToChildren("Pillar", wetStoneMat);
            ApplyMaterialToChildren("Temple", wetStoneMat);

            Debug.Log("[Moon7MaterialSetup] ✅ Underwater materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon7_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.6f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        void ApplyMaterialToChildren(string namePattern, Material mat)
        {
            foreach (var obj in FindObjectsOfType<GameObject>())
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
