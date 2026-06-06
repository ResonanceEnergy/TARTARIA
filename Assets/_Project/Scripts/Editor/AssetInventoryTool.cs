using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Asset Inventory Tool — Generates comprehensive asset reports for gap analysis
    /// Scans all imported assets and creates Moon-specific requirement mapping
    /// Usage: Tools → TARTARIA → Generate Asset Inventory
    /// </summary>
    public class AssetInventoryTool : EditorWindow
    {
        [MenuItem("Tools/TARTARIA/Generate Asset Inventory")]
        static void GenerateInventory()
        {
            Debug.Log("[AssetInventory] Scanning project assets...");

            StringBuilder report = new StringBuilder();
            report.AppendLine("# TARTARIA ASSET INVENTORY & GAP ANALYSIS");
            report.AppendLine($"**Generated:** {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**Unity Version:** {Application.unityVersion}");
            report.AppendLine();
            report.AppendLine("---");
            report.AppendLine();

            // Scan for models
            string[] modelPaths = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            report.AppendLine($"## 3D MODELS ({modelPaths.Length} total)");
            report.AppendLine();
            
            var modelsByFolder = modelPaths
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .GroupBy(path => System.IO.Path.GetDirectoryName(path))
                .OrderBy(g => g.Key);

            foreach (var folder in modelsByFolder)
            {
                report.AppendLine($"### {folder.Key}");
                report.AppendLine($"- **Count:** {folder.Count()} models");
                report.AppendLine();
            }

            // Scan for prefabs
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" });
            report.AppendLine($"## PREFABS ({prefabPaths.Length} total)");
            report.AppendLine();

            var prefabsByFolder = prefabPaths
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .GroupBy(path => System.IO.Path.GetDirectoryName(path).Replace("Assets/_Project/Prefabs/", ""))
                .OrderBy(g => g.Key);

            foreach (var folder in prefabsByFolder)
            {
                report.AppendLine($"### {folder.Key}");
                report.AppendLine($"- **Count:** {folder.Count()} prefabs");
                var samples = folder.Take(5).Select(p => "  - " + System.IO.Path.GetFileName(p));
                report.AppendLine(string.Join("\n", samples));
                if (folder.Count() > 5) report.AppendLine($"  - _(+{folder.Count() - 5} more)_");
                report.AppendLine();
            }

            // Scan for materials
            string[] materialPaths = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            report.AppendLine($"## MATERIALS ({materialPaths.Length} total)");
            report.AppendLine();

            // Scan for textures
            string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            report.AppendLine($"## TEXTURES ({texturePaths.Length} total)");
            report.AppendLine();

            // Scan for audio
            string[] audioPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            report.AppendLine($"## AUDIO CLIPS ({audioPaths.Length} total)");
            report.AppendLine();

            var audioByFolder = audioPaths
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .GroupBy(path => System.IO.Path.GetDirectoryName(path))
                .OrderBy(g => g.Key);

            foreach (var folder in audioByFolder)
            {
                report.AppendLine($"### {folder.Key}");
                report.AppendLine($"- **Count:** {folder.Count()} clips");
                report.AppendLine();
            }

            // Moon Requirements Gap Analysis
            report.AppendLine("---");
            report.AppendLine();
            report.AppendLine("## MOON 1-3 GAP ANALYSIS");
            report.AppendLine();

            report.AppendLine("### Moon 1: Magnetic Moon (Cathedral Focus)");
            report.AppendLine("**Required Assets:**");
            report.AppendLine("- [ ] Modular cathedral (15-20 pieces) — **CRITICAL GAP**");
            report.AppendLine("- [ ] Cathedral spire with mercury ball top — **CRITICAL GAP**");
            report.AppendLine("- [ ] Giant skeleton prop (25ft) — **CRITICAL GAP**");
            report.AppendLine("- [?] Excavation mud materials (3 states) — **NEEDS CREATION**");
            report.AppendLine("- [x] Terrain system (Unity built-in)");
            report.AppendLine("- [x] VFX: Aurora (RestoreSparkle.prefab exists)");
            report.AppendLine("- [x] Audio: Ambience (RPG Game Tracks)");
            report.AppendLine();
            report.AppendLine("**Kitbashing Sources:**");
            report.AppendLine("- Fantasy Adventure Environment (imported)");
            report.AppendLine("- KayKit_Forest_Nature_Pack (rocks, vegetation)");
            report.AppendLine();

            report.AppendLine("### Moon 2: Lunar Moon (Fractal Interior)");
            report.AppendLine("**Required Assets:**");
            report.AppendLine("- [ ] Fractal corridor kit (Escher-style) — **CRITICAL GAP**");
            report.AppendLine("- [ ] Corruption crystal materials — **NEEDS CREATION**");
            report.AppendLine("- [x] VFX: Hovl Studio crystals (exists)");
            report.AppendLine("- [x] VFX: Explosions for purge (EffectExamples)");
            report.AppendLine();

            report.AppendLine("### Moon 3: Electric Moon (Rail Junction)");
            report.AppendLine("**Required Assets:**");
            report.AppendLine("- [ ] Spectral train (4-car modular) — **CRITICAL GAP**");
            report.AppendLine("- [ ] Rail segments with glowing ties — **NEEDS CREATION**");
            report.AppendLine("- [ ] Victorian station ruins — **PARTIAL (use Fantasy Adv Env)**");
            report.AppendLine("- [?] Ghost children NPCs (translucent shader) — **NEEDS SHADER**");
            report.AppendLine("- [x] Rain/mist VFX (EffectExamples has RainEffect)");
            report.AppendLine();

            // Save report
            string reportPath = "docs/ASSET_INVENTORY_FULL.md";
            File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[AssetInventory] Report saved: {reportPath}");

            EditorUtility.DisplayDialog(
                "Asset Inventory Complete",
                $"Full inventory generated:\n{reportPath}\n\n" +
                $"Summary:\n" +
                $"• {modelPaths.Length} models\n" +
                $"• {prefabPaths.Length} prefabs\n" +
                $"• {materialPaths.Length} materials\n" +
                $"• {texturePaths.Length} textures\n" +
                $"• {audioPaths.Length} audio clips\n\n" +
                "See report for Moon 1-3 gap analysis.",
                "OK"
            );
        }
    }
}
