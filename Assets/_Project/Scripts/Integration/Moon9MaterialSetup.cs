using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 9 Material Setup — The Blighted Wastes
    /// Apply corruption materials: twisted spires, dark energy, corrupted ground
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon9MaterialSetup : MonoBehaviour
    {
        Material corruptedMat;
        Material spireMat;
        Material energyMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon9MaterialSetup] 🎨 Applying blighted corruption materials...");

            // Corruption palette
            corruptedMat = CreateMaterial("Ground037", new Color(0.3f, 0.25f, 0.35f)); // Purple-gray wasteland
            spireMat = CreateMaterial("Rocks023", new Color(0.2f, 0.15f, 0.25f)); // Dark twisted rock
            energyMat = CreateMaterial("Marble006", new Color(0.6f, 0.2f, 0.8f)); // Purple corruption energy

            energyMat.EnableKeyword("_EMISSION");
            energyMat.SetColor("_EmissionColor", new Color(1.2f, 0.4f, 1.6f)); // Bright purple glow

            // Apply to level geometry
            ApplyMaterialToChildren("WastelandFloor", corruptedMat);
            ApplyMaterialToChildren("TwistedSpire", spireMat);
            ApplyMaterialToChildren("Monolith", spireMat);
            ApplyMaterialToChildren("CorruptionNexus", energyMat);
            ApplyMaterialToChildren("DarkEnergy", energyMat);
            ApplyMaterialToChildren("BrokenPillar", spireMat);
            ApplyMaterialToChildren("Crater", corruptedMat);
            ApplyMaterialToChildren("Rift", energyMat);

            Debug.Log("[Moon9MaterialSetup] ✅ Corruption materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon9_{name}";
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
