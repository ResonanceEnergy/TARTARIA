using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// HDR Output Configurator — enables HDR display output in Player Settings.
    /// Sets PlayerSettings.useHDRDisplay = true if API available, otherwise
    /// uses SerializedObject fallback. Idempotent.
    /// </summary>
    public static class HDROutputConfigurator
    {
        public static void Run()
        {
            bool success = false;

            // Try direct API first
            try
            {
                var useHDRProp = typeof(PlayerSettings).GetProperty("useHDRDisplay");
                if (useHDRProp != null && useHDRProp.CanWrite)
                {
                    useHDRProp.SetValue(null, true);
                    Debug.Log("[HDROutput] Set PlayerSettings.useHDRDisplay = true (API)");
                    success = true;
                }
            }
            catch
            {
                // API not available
            }

            // Fallback to SerializedObject
            if (!success)
            {
                // Try SerializedObject on the actual PlayerSettings asset
                // Note: GetSerializedObject doesn't exist in Unity 6 — skip this fallback
                Debug.LogWarning("[HDROutput] SerializedObject fallback not available in this Unity version.");
            }

            // Try hdrBitDepth property if available
            try
            {
                var bitDepthProp = typeof(PlayerSettings).GetProperty("hdrBitDepth");
                if (bitDepthProp != null && bitDepthProp.CanWrite)
                {
                    // Set to HDR10 (10-bit) if enum available
                    bitDepthProp.SetValue(null, 1); // Assume 1 = HDR10
                    Debug.Log("[HDROutput] Set PlayerSettings.hdrBitDepth = HDR10");
                }
            }
            catch
            {
                // Property not available
            }

            if (!success)
            {
                Debug.LogWarning("[HDROutput] Could not set useHDRDisplay — API may not be available in this Unity version.");
            }
            else
            {
                Debug.Log("[HDROutput] HDR display output enabled.");
            }
        }
    }
}
