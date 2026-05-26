using UnityEngine;
using UnityEditor;
using Tartaria.Testing;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor menu items for accessibility testing and validation.
    /// </summary>
    public static class AccessibilityTestingMenu
    {
        [MenuItem("Tools/Tartaria/Accessibility/Validate WCAG Contrast", false, 100)]
        public static void ValidateWCAGContrast()
        {
            // Create temporary validator
            var go = new GameObject("_TempWCAGValidator");
            var validator = go.AddComponent<WCAGContrastValidator>();
            validator.ValidateAllTextContrast();
            Object.DestroyImmediate(go);
            
            EditorUtility.DisplayDialog(
                "WCAG Contrast Validation", 
                "Contrast validation complete. Check Console for results.", 
                "OK");
        }

        [MenuItem("Tools/Tartaria/Accessibility/Test Input Latency", false, 101)]
        public static void TestInputLatency()
        {
            // Create test object in scene
            var go = new GameObject("InputLatencyMeasurement");
            go.AddComponent<InputLatencyMeasurement>();
            Selection.activeGameObject = go;
            
            EditorUtility.DisplayDialog(
                "Input Latency Test", 
                "InputLatencyMeasurement component added to scene.\n\n" +
                "Enter Play Mode and press [Space] to measure latency.\n" +
                "Target: <100ms for accessibility compliance.", 
                "OK");
        }

        [MenuItem("Tools/Tartaria/Accessibility/Audit Summary", false, 102)]
        public static void ShowAuditSummary()
        {
            var summary = @"TARTARIA ACCESSIBILITY AUDIT SUMMARY

✓ IMPLEMENTED:
• Full colorblind support (Protanopia, Deuteranopia, Tritanopia)
• Text scaling (0.7x-2.0x)
• Input remapping system
• Gamepad support (Xbox, PlayStation, Logitech F310)
• Haptic feedback with intensity control
• Subtitle system with background opacity
• High contrast mode
• Reduced motion option
• Screen reader mode (Narrator/NVDA/JAWS)
• SFX captions
• Volume controls (Master/Music/SFX/Ambience)
• Tutorial skip option
• Autosave system
• Motor accessibility (hold duration, button sizing)

✓ NEW (Agent 7):
• Dynamic button prompts (KB/gamepad icons)
• Difficulty settings (Story/Balanced/Challenge)
• Auto-evade assistance
• WCAG contrast validator
• Input latency measurement tool

⚠ RECOMMENDED IMPROVEMENTS:
• Add TTS for UI text (requires external plugin)
• Add one-hand control presets
• Add navigation assist trails
• Add mini-game timing adjustment
• Add quick-time event extended windows

Run individual tests via:
  Tools > Tartaria > Accessibility > [Test Name]";

            EditorUtility.DisplayDialog("Accessibility Audit", summary, "OK");
            Debug.Log(summary);
        }
    }
}
