using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Material Setup — The Frostbound Citadel
    /// Apply ice fortress materials: glacial ice, frozen crystal, snow
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon5MaterialSetup : MonoBehaviour
    {
        Material iceMat;
        Material crystalMat;
        Material snowMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon5MaterialSetup] 🎨 Applying frozen citadel materials...");

            // Ice palette
            iceMat = CreateMaterial("Marble006", new Color(0.7f, 0.85f, 1f)); // Icy blue
            crystalMat = CreateMaterial("Marble006", new Color(0.8f, 0.9f, 1f)); // Bright crystal
            snowMat = CreateMaterial("Ground037", new Color(0.95f, 0.95f, 1f)); // Pure snow

            iceMat.SetFloat("_Smoothness", 0.9f); // Very smooth ice
            crystalMat.SetFloat("_Smoothness", 1f); // Glass-like crystal

            // Apply to level geometry
            ApplyMaterialToChildren("CitadelWall", iceMat);
            ApplyMaterialToChildren("Tower", iceMat);
            ApplyMaterialToChildren("Spire", crystalMat);
            ApplyMaterialToChildren("Floor", snowMat);
            ApplyMaterialToChildren("IceCrystal", crystalMat);
            ApplyMaterialToChildren("FrozenPillar", iceMat);
            ApplyMaterialToChildren("Glacier", iceMat);
            ApplyMaterialToChildren("Snowdrift", snowMat);

            Debug.Log("[Moon5MaterialSetup] ✅ Ice materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon5_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.7f);
            mat.SetFloat("_Metallic", 0.1f);
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
