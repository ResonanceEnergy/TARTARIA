using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Material Setup — The Verdant Labyrinth
    /// Apply jungle temple materials: overgrown stone, moss, vegetation
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon3MaterialSetup : MonoBehaviour
    {
        Material groundMat;
        Material stoneMat;
        Material mossyMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon3MaterialSetup] 🎨 Applying jungle temple materials...");

            // Create materials
            groundMat = CreateMaterial("Ground037", new Color(0.3f, 0.5f, 0.2f)); // Dark green ground
            stoneMat = CreateMaterial("PavingStones150", new Color(0.6f, 0.6f, 0.5f)); // Gray temple stone
            mossyMat = CreateMaterial("Ground037", new Color(0.2f, 0.6f, 0.3f)); // Bright moss

            // Apply to level geometry
            ApplyMaterialToChildren("OuterWalls", stoneMat);
            ApplyMaterialToChildren("MazeWall", stoneMat);
            ApplyMaterialToChildren("MazeSection", stoneMat);
            ApplyMaterialToChildren("CentralShrine", mossyMat);
            ApplyMaterialToChildren("TemplePlatform", stoneMat);
            ApplyMaterialToChildren("Floor", groundMat);
            ApplyMaterialToChildren("Tree", mossyMat);
            ApplyMaterialToChildren("Vegetation", mossyMat);

            Debug.Log("[Moon3MaterialSetup] ✅ Jungle materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon3_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.3f);
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
