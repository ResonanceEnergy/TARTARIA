using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Editor
{
    /// <summary>
    /// Sprint 7 Lane 4 — Editor menu Tartaria/Save/Capture Current Thumbnail.
    ///
    /// Captures the current Game view via ScreenCapture.CaptureScreenshotAsTexture(),
    /// encodes PNG (with SaveSlotPanel's 256 KB size knob — downscale to half-res if
    /// over budget), writes to {Application.persistentDataPath}/saves/slot{CurrentSlot}.png,
    /// and refreshes AssetDatabase so any Editor-side previews update.
    ///
    /// Canonical APIs used:
    ///   - SaveManager.GetCurrentSlot()                                  (Assets/_Project/Scripts/Save/SaveManager.cs:616)
    ///   - SaveSlotPanel.GetThumbnailPath(int)                           (Assets/_Project/Scripts/UI/SaveSlotPanel.cs)
    ///   - SaveSlotPanel.EncodeAndWritePngWithSizeKnob(Texture2D,string,string)
    ///   - ScreenCapture.CaptureScreenshotAsTexture()                    (UnityEngine)
    ///
    /// Constraints:
    ///   - Unity 6 API (FindFirstObjectByType + FindObjectsInactive)
    ///   - No silent catches — every catch logs GetType().Name + Message + persistentDataPath
    ///   - Texture2D dispose pattern: capture texture is destroyed in finally
    ///   - Must be in Play Mode for ScreenCapture to return non-empty pixels;
    ///     menu is enabled-state-gated to surface that requirement.
    /// </summary>
    public static class SaveThumbnailMenu
    {
        const string MenuPath = "Tartaria/Save/Capture Current Thumbnail";

        [MenuItem(MenuPath, priority = 200)]
        public static void CaptureCurrentThumbnail()
        {
            // Resolve current slot via canonical SaveManager.GetCurrentSlot() (SaveManager.cs:616).
            int slot = ResolveCurrentSlot(out string slotSource);
            if (slot < 0)
            {
                Debug.LogError($"[SaveThumbnailMenu] Cannot resolve current save slot (source='{slotSource}'). " +
                               $"Make sure SaveManager exists in the active scene and Play Mode is running. " +
                               $"persistentDataPath='{Application.persistentDataPath}'");
                return;
            }

            if (!Application.isPlaying)
            {
                Debug.LogWarning($"[SaveThumbnailMenu] Not in Play Mode — ScreenCapture.CaptureScreenshotAsTexture will not capture rendered Game view content. " +
                                 $"Aborting capture for slot {slot}. persistentDataPath='{Application.persistentDataPath}'");
                return;
            }

            Texture2D shot = null;
            try
            {
                shot = ScreenCapture.CaptureScreenshotAsTexture();
                if (shot == null)
                {
                    Debug.LogError($"[SaveThumbnailMenu] ScreenCapture.CaptureScreenshotAsTexture returned null for slot {slot}. " +
                                   $"persistentDataPath='{Application.persistentDataPath}'");
                    return;
                }

                string outputPath = SaveSlotPanel.GetThumbnailPath(slot);

                // Delegate to SaveSlotPanel's centralized encode + size-knob path so the runtime + Editor
                // pipelines stay identical (no separate Editor-only quality knob to drift).
                int finalBytes = SaveSlotPanel.EncodeAndWritePngWithSizeKnob(shot, outputPath, $"Editor menu slot {slot}");
                if (finalBytes <= 0)
                {
                    Debug.LogError($"[SaveThumbnailMenu] EncodeAndWritePngWithSizeKnob returned {finalBytes} for slot {slot} -> '{outputPath}'. " +
                                   $"persistentDataPath='{Application.persistentDataPath}'");
                    return;
                }

                Debug.Log($"[SaveThumbnailMenu] Captured Editor thumbnail for slot {slot} ({slotSource}): {finalBytes} bytes -> {outputPath}");

                // Refresh AssetDatabase so any Editor-side preview that imports from persistentDataPath updates.
                // (persistentDataPath is outside Assets/, but consumers may have copy-on-refresh hooks.)
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveThumbnailMenu] CaptureCurrentThumbnail failed for slot {slot}: {e.GetType().Name}: {e.Message}\n" +
                               $"persistentDataPath='{Application.persistentDataPath}'\n{e.StackTrace}");
            }
            finally
            {
                if (shot != null) UnityEngine.Object.DestroyImmediate(shot);
            }
        }

        [MenuItem(MenuPath, validate = true)]
        public static bool ValidateCaptureCurrentThumbnail()
        {
            // Allow menu to be visible at all times so users can discover it; the runtime check above
            // produces a loud explanation when Play Mode is off or SaveManager is missing.
            return true;
        }

        /// <summary>
        /// Resolves the current save slot via SaveManager.GetCurrentSlot() (SaveManager.cs:616).
        /// Falls back to SaveManager.Instance, then to FindFirstObjectByType, then -1.
        /// Sets <paramref name="source"/> to a human-readable origin string for logging.
        /// </summary>
        static int ResolveCurrentSlot(out string source)
        {
            try
            {
                var sm = SaveManager.Instance;
                if (sm != null)
                {
                    source = "SaveManager.Instance";
                    return sm.GetCurrentSlot();
                }

                // Unity 6 API
                sm = UnityEngine.Object.FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include);
                if (sm != null)
                {
                    source = "FindFirstObjectByType<SaveManager>";
                    return sm.GetCurrentSlot();
                }

                source = "none-found";
                return -1;
            }
            catch (Exception e)
            {
                source = $"exception:{e.GetType().Name}";
                Debug.LogError($"[SaveThumbnailMenu] ResolveCurrentSlot threw: {e.GetType().Name}: {e.Message}. " +
                               $"persistentDataPath='{Application.persistentDataPath}'");
                return -1;
            }
        }
    }
}
