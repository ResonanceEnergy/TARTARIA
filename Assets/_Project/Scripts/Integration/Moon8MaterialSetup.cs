using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Material Setup — The Celestial Spires
    /// Apply sky materials: clouds, white marble temples, ethereal mist
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon8MaterialSetup : MonoBehaviour
    {
        Material cloudMat;
        Material templeMat;
        Material mistMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon8MaterialSetup] 🎨 Applying celestial sky materials...");

            // Sky palette
            cloudMat = CreateMaterial("Plaster001", new Color(0.95f, 0.95f, 1f)); // Pure white clouds
            templeMat = CreateMaterial("Marble006", new Color(0.9f, 0.92f, 0.95f)); // White marble
            mistMat = CreateMaterial("Plaster001", new Color(0.85f, 0.9f, 1f)); // Blue-tinted mist

            cloudMat.SetFloat("_Smoothness", 0.1f); // Soft clouds
            templeMat.SetFloat("_Smoothness", 0.8f); // Polished marble

            // Apply to level geometry
            ApplyMaterialToChildren("Spire", templeMat);
            ApplyMaterialToChildren("Temple", templeMat);
            ApplyMaterialToChildren("Platform", cloudMat);
            ApplyMaterialToChildren("Bridge", templeMat);
            ApplyMaterialToChildren("CloudIsland", cloudMat);
            ApplyMaterialToChildren("Pillar", templeMat);
            ApplyMaterialToChildren("Altar", templeMat);
            ApplyMaterialToChildren("SkyPath", mistMat);

            Debug.Log("[Moon8MaterialSetup] ✅ Sky materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon8_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.5f);
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
