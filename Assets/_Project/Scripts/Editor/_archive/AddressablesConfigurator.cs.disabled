using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Addressables Configuration Tool — Sets up asset groups for Moon1-13 scenes
    /// Usage: Tools → TARTARIA → Configure Addressables for Moons
    /// </summary>
    public class AddressablesConfigurator : EditorWindow
    {
        [MenuItem("Tools/TARTARIA/Configure Addressables for Moons")]
        static void ConfigureAddressables()
        {
            Debug.Log("[AddressablesConfigurator] Starting Addressables configuration for Moon 1-13...");

            // Create Addressables groups structure
            string[] moonLabels = new string[13];
            for (int i = 0; i < 13; i++)
            {
                moonLabels[i] = $"moon{i + 1}";
            }

            // Log configuration plan
            Debug.Log("[AddressablesConfigurator] Planned groups:");
            Debug.Log("  - Moon1_Assets through Moon13_Assets (LoadMode: Explicit)");
            Debug.Log("  - SharedArchitecture (LoadMode: Cached)");
            Debug.Log("  - SharedMaterials (LoadMode: Cached)");
            Debug.Log("  - SharedVFX (LoadMode: Cached)");

            // Note: Actual Addressables API integration requires com.unity.addressables package
            // This script logs the configuration plan for manual setup
            
            EditorUtility.DisplayDialog(
                "Addressables Configuration",
                "Addressables group structure logged to Console.\n\n" +
                "Manual Setup Required:\n" +
                "1. Install: com.unity.addressables (Package Manager)\n" +
                "2. Window → Asset Management → Addressables → Groups\n" +
                "3. Create groups as logged in Console\n\n" +
                "Moon Assets: moon1-moon13 labels (Explicit load)\n" +
                "Shared Assets: shared/materials/vfx labels (Cached)",
                "OK"
            );

            Debug.Log("[AddressablesConfigurator] Configuration plan ready. See dialog for next steps.");
        }

        [MenuItem("Tools/TARTARIA/Generate Addressables Report")]
        static void GenerateReport()
        {
            string report = "# TARTARIA ADDRESSABLES STRUCTURE\n\n";
            report += "## Moon Asset Groups (13 total)\n\n";

            for (int i = 1; i <= 13; i++)
            {
                report += $"### Moon{i}_Assets\n";
                report += $"- **Label:** moon{i}\n";
                report += $"- **Load Mode:** Explicit (load only when Moon{i} scene active)\n";
                report += $"- **Assets:** Prefabs/Moon{i}/, Materials/Moon{i}/, Textures/Moon{i}/\n\n";
            }

            report += "## Shared Asset Groups\n\n";
            report += "### SharedArchitecture\n";
            report += "- **Label:** shared\n";
            report += "- **Load Mode:** Cached (loaded once, persist across Moons)\n";
            report += "- **Assets:** Modular building pieces, reusable props\n\n";

            report += "### SharedMaterials\n";
            report += "- **Label:** materials\n";
            report += "- **Load Mode:** Cached\n";
            report += "- **Assets:** Master PBR materials (Stone_Tartarian, Metal_Ornate, Crystal_Aether)\n\n";

            report += "### SharedVFX\n";
            report += "- **Label:** vfx\n";
            report += "- **Load Mode:** Cached\n";
            report += "- **Assets:** Hovl Studio VFX, EffectExamples, custom particle systems\n\n";

            string path = "docs/ADDRESSABLES_STRUCTURE.md";
            File.WriteAllText(path, report);
            Debug.Log($"[AddressablesConfigurator] Report generated: {path}");
            EditorUtility.DisplayDialog("Report Generated", $"Addressables structure documented:\n{path}", "OK");
        }
    }
}
