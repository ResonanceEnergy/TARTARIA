using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 12 Material Setup — The Umbral Sanctum
    /// Apply shadow realm materials: void darkness, obelisks, minimal light
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon12MaterialSetup : MonoBehaviour
    {
        Material voidMat;
        Material obeliskMat;
        Material shadowMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon12MaterialSetup] 🎨 Applying umbral shadow materials...");

            // Shadow palette
            voidMat = CreateMaterial("Rocks023", new Color(0.05f, 0.05f, 0.1f)); // Near-black void
            obeliskMat = CreateMaterial("Marble006", new Color(0.1f, 0.1f, 0.15f)); // Dark stone
            shadowMat = CreateMaterial("Ground037", new Color(0.08f, 0.08f, 0.12f)); // Shadow floor

            obeliskMat.SetFloat("_Smoothness", 0.8f); // Polished dark stone
            voidMat.SetFloat("_Smoothness", 0.2f); // Rough void material

            // Apply to level geometry
            ApplyMaterialToChildren("VoidCore", voidMat);
            ApplyMaterialToChildren("Obelisk", obeliskMat);
            ApplyMaterialToChildren("ShadowSpire", obeliskMat);
            ApplyMaterialToChildren("UmbralFloor", shadowMat);
            ApplyMaterialToChildren("DarkPillar", obeliskMat);
            ApplyMaterialToChildren("Bridge", obeliskMat);
            ApplyMaterialToChildren("Void", voidMat);
            ApplyMaterialToChildren("Sanctum", obeliskMat);

            Debug.Log("[Moon12MaterialSetup] ✅ Shadow materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon12_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.5f);
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
