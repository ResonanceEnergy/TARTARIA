#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Save.EditorTools
{
    /// <summary>
    /// SaveDiagnosticsMenu — quick "where is my save / what's in it" surface for
    /// QA + Cowork. Per HANDOFFS 2026-06-01 22:30 → Systems Architect (save-load-hardening).
    /// </summary>
    public static class SaveDiagnosticsMenu
    {
        const string SaveFileFmt = "save_slot_{0}.dat";
        const int MaxBytesPreviewed = 8192;

        [MenuItem("Tartaria/7 Diagnose/Dump Save File", false, 71)]
        public static void DumpSaveFile()
        {
            int slot = 0;
            if (Application.isPlaying && SaveManager.Instance != null)
            {
                slot = SaveManager.Instance.GetCurrentSlot();
            }
            var fileName = string.Format(SaveFileFmt, slot);
            var path = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Save File",
                    $"No save file at:\n{path}\n\n(Slot {slot} not written yet.)", "OK");
                return;
            }
            var info = new FileInfo(path);
            string preview;
            try
            {
                var bytes = File.ReadAllBytes(path);
                int take = Mathf.Min(bytes.Length, MaxBytesPreviewed);
                // Save file is binary/encrypted-ish (.dat) --- show as UTF8 best-effort + length.
                preview = System.Text.Encoding.UTF8.GetString(bytes, 0, take);
            }
            catch (System.Exception ex)
            {
                preview = "<read error: " + ex.Message + ">";
            }
            Debug.Log($"[SaveDiagnostics] slot={slot} path={path} size={info.Length}B mtime={info.LastWriteTime:O}\n----- preview ({MaxBytesPreviewed}B) -----\n{preview}");
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("Tartaria/7 Diagnose/Open Save Folder", false, 72)]
        public static void OpenSaveFolder()
        {
            var dir = Application.persistentDataPath;
            Debug.Log($"[SaveDiagnostics] persistentDataPath={dir}");
            EditorUtility.RevealInFinder(dir);
        }

        [MenuItem("Tartaria/7 Diagnose/Delete All Save Slots", false, 73)]
        public static void DeleteAllSaves()
        {
            if (!EditorUtility.DisplayDialog("Delete Save Slots",
                "Delete every save_slot_*.dat / .backup.dat / .cloud.dat in:\n" +
                Application.persistentDataPath + "\n\nThis cannot be undone.",
                "Delete", "Cancel"))
            {
                return;
            }
            int removed = 0;
            var dir = new DirectoryInfo(Application.persistentDataPath);
            foreach (var f in dir.GetFiles("save_slot_*.dat"))      { f.Delete(); removed++; }
            foreach (var f in dir.GetFiles("save_slot_*.backup.dat")){ f.Delete(); removed++; }
            foreach (var f in dir.GetFiles("save_slot_*.cloud.dat")) { f.Delete(); removed++; }
            foreach (var f in dir.GetFiles("pending_cloud_uploads_*.json")) { f.Delete(); removed++; }
            Debug.Log($"[SaveDiagnostics] removed {removed} save file(s) from {dir.FullName}");
        }
    }
}
#endif
