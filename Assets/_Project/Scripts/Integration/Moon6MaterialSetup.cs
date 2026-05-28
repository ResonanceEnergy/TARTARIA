using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Material Setup — The Molten Forge
    /// Apply volcanic materials: lava rock, molten metal, hot coals
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon6MaterialSetup : MonoBehaviour
    {
        Material lavaRockMat;
        Material metalMat;
        Material lavaMat;

        void Start()
        {
            SetupMaterials();
        }

        void SetupMaterials()
        {
            Debug.Log("[Moon6MaterialSetup] 🎨 Applying volcanic forge materials...");

            // Volcanic palette
            lavaRockMat = CreateMaterial("Rocks023", new Color(0.3f, 0.15f, 0.1f)); // Dark volcanic rock
            metalMat = CreateMaterial("Bricks075A", new Color(0.5f, 0.4f, 0.35f)); // Hot metal
            lavaMat = CreateMaterial("Ground037", new Color(1f, 0.3f, 0f)); // Bright orange lava

            metalMat.SetFloat("_Metallic", 0.8f);
            metalMat.SetFloat("_Smoothness", 0.6f);
            lavaMat.EnableKeyword("_EMISSION");
            lavaMat.SetColor("_EmissionColor", new Color(2f, 0.6f, 0f)); // Glowing lava

            // Apply to level geometry
            ApplyMaterialToChildren("ForgeWall", lavaRockMat);
            ApplyMaterialToChildren("Anvil", metalMat);
            ApplyMaterialToChildren("Furnace", metalMat);
            ApplyMaterialToChildren("LavaPool", lavaMat);
            ApplyMaterialToChildren("MoltenRiver", lavaMat);
            ApplyMaterialToChildren("VolcanicRock", lavaRockMat);
            ApplyMaterialToChildren("Pillar", lavaRockMat);
            ApplyMaterialToChildren("Floor", lavaRockMat);

            Debug.Log("[Moon6MaterialSetup] ✅ Volcanic materials applied!");
        }

        Material CreateMaterial(string name, Color tint)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = $"Moon6_{name}";
            mat.color = tint;
            mat.SetFloat("_Smoothness", 0.4f);
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
