using UnityEditor;
using UnityEngine;
using Tartaria.Editor;
using Tartaria.EditorTools;

namespace Tartaria.Build
{
    /// <summary>
    /// Beta build entry point - forces Mono backend then calls standard build pipeline.
    /// </summary>
    public static class BetaBuild
    {
        public static void BuildMonoStandalone()
        {
            Debug.Log("[BetaBuild] === BETA BUILD START ===");
            Debug.Log("[BetaBuild] Forcing Mono2x backend...");
            
            // Force development mode settings (uses Mono2x)
            OneClickBuild.ConfigureRecommendedPlayerSettings(forDevelopment: true);
            
            Debug.Log("[BetaBuild] Calling standard build pipeline...");
            
            // Call the standard build method
            BuildPlayerPipeline.BuildWindows();
            
            Debug.Log("[BetaBuild] === BETA BUILD COMPLETE ===");
        }
    }
}
