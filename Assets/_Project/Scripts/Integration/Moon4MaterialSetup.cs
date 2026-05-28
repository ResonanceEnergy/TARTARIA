using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Material Setup — The Sunscorched Oasis
    /// Apply desert materials: sand, weathered stone, sun-bleached ruins
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon4MaterialSetup : MonoBehaviour
    {
        Material sandMat;
        Material ruinsMat;
        Material oasisMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon4MaterialSetup] 🎨 Applying desert materials...");

            // Desert palette
            sandMat = CreateMaterial("Ground037", new Color(0.9f, 0.8f, 0.6f)); // Warm sand
            ruinsMat = CreateMaterial("PavingStones150", new Color(0.8f, 0.7f, 0.5f)); // Sun-bleached stone
            oasisMat = CreateMaterial("Marble006", new Color(0.3f, 0.6f, 0.8f)); // Water blue

            // Apply to level geometry
            ApplyMaterialToChildren("DesertFloor", sandMat);
            ApplyMaterialToChildren("Dune", sandMat);
            ApplyMaterialToChildren("TempleRuin", ruinsMat);
            ApplyMaterialToChildren("Pillar", ruinsMat);
            ApplyMaterialToChildren("Wall", ruinsMat);
            ApplyMaterialToChildren("OasisPool", oasisMat);
            ApplyMaterialToChildren("Fountain", oasisMat);
            ApplyMaterialToChildren("Statue", ruinsMat);

            Debug.Log("[Moon4MaterialSetup] ✅ Desert materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon4_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Metallic", 0f);
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
