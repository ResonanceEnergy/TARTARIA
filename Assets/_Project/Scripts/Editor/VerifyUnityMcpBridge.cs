#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Text;

namespace Tartaria.Editor
{
    /// <summary>
    /// Smoke-test that the Unity AI Assistant MCP bridge is installed + reachable.
    /// Runs after `com.unity.ai.assistant` resolves and is enabled in Preferences.
    /// </summary>
    public static class VerifyUnityMcpBridge
    {
        [MenuItem("Tartaria/9 Debug/Verify Unity MCP Bridge", priority = 950)]
        public static void Verify()
        {
            var report = new StringBuilder();
            report.AppendLine("=== Unity MCP Bridge Verification ===");

            // 1. Is the AI Assistant assembly loaded?
            var aiAssemblies = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.IndexOf("Unity.AI.Assistant", System.StringComparison.OrdinalIgnoreCase) >= 0
                         || a.GetName().Name.IndexOf("Unity.Mcp", System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            if (aiAssemblies.Count == 0)
            {
                report.AppendLine("❌ No Unity.AI.Assistant or Unity.Mcp assembly loaded.");
                report.AppendLine("   Open Window → Package Manager and verify 'AI Assistant' is installed + resolved.");
                Show(report.ToString());
                return;
            }
            report.AppendLine($"✓ Found {aiAssemblies.Count} MCP-related assemblies:");
            foreach (var a in aiAssemblies) report.AppendLine($"   • {a.GetName().Name} v{a.GetName().Version}");

            // 2. Look for an MCP server / bridge class via reflection.
            var mcpType = aiAssemblies
                .SelectMany(a => { try { return a.GetTypes(); } catch { return new System.Type[0]; } })
                .FirstOrDefault(t => t.Name.IndexOf("McpServer", System.StringComparison.OrdinalIgnoreCase) >= 0
                                  || t.Name.IndexOf("McpBridge", System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (mcpType != null)
                report.AppendLine($"✓ MCP server type found: {mcpType.FullName}");
            else
                report.AppendLine("⚠ Could not locate an McpServer/McpBridge type — package may name it differently. Check Edit → Preferences → AI Assistant → MCP Server panel.");

            // 3. Tell NATRIX what to do next.
            report.AppendLine();
            report.AppendLine("=== Next steps ===");
            report.AppendLine("1. Edit → Preferences → AI Assistant → MCP Server → enable + note the port.");
            report.AppendLine("2. In your Claude/VS Code MCP client config, add a server entry pointing at the");
            report.AppendLine("   port from step 1 (typically http://localhost:<port>/mcp or stdio).");
            report.AppendLine("3. See docs/integration/UNITY_MCP_SETUP.md for the exact client config snippets.");
            report.AppendLine("4. From the client, call list_tools to confirm the bridge exposes menu execution,");
            report.AppendLine("   console reading, Play mode toggle, and scene query.");

            Show(report.ToString());
        }

        static void Show(string body)
        {
            Debug.Log(body);
            EditorUtility.DisplayDialog("Unity MCP Bridge Verify", body, "OK");
        }
    }
}
#endif
