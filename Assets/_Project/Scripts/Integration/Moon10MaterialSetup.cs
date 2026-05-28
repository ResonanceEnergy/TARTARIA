using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Material Setup — The Temporal Rift
    /// Apply time-distortion materials: shifting temporal layers, paradox crystals
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon10MaterialSetup : MonoBehaviour
    {
        Material pastMat;
        Material presentMat;
        Material futureMat;
        Material vortexMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon10MaterialSetup] 🎨 Applying temporal rift materials...");

            // Time layer palette
            pastMat = CreateMaterial("PavingStones150", new Color(0.8f, 0.6f, 0.4f)); // Sepia past
            presentMat = CreateMaterial("Marble006", new Color(0.9f, 0.9f, 0.9f)); // Neutral present
            futureMat = CreateMaterial("Marble006", new Color(0.5f, 0.7f, 1f)); // Blue future
            vortexMat = CreateMaterial("Marble006", new Color(0.8f, 0.9f, 1f)); // Bright temporal energy

            vortexMat.EnableKeyword("_EMISSION");
            vortexMat.SetColor("_EmissionColor", new Color(1.6f, 1.8f, 2f)); // Bright white-blue
            vortexMat.SetFloat("_Smoothness", 0.95f);

            // Apply to level geometry
            ApplyMaterialToChildren("TimeVortex", vortexMat);
            ApplyMaterialToChildren("TemporalAnchor", presentMat);
            ApplyMaterialToChildren("PastLayer", pastMat);
            ApplyMaterialToChildren("PresentLayer", presentMat);
            ApplyMaterialToChildren("FutureLayer", futureMat);
            ApplyMaterialToChildren("Paradox", vortexMat);
            ApplyMaterialToChildren("TimeWall", presentMat);
            ApplyMaterialToChildren("Pillar", presentMat);

            Debug.Log("[Moon10MaterialSetup] ✅ Temporal materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon10_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.6f);
            mat.SetFloat("_Metallic", 0.2f);
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
