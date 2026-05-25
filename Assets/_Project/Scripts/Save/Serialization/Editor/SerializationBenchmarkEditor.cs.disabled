using UnityEditor;
using UnityEngine;
using Tartaria.Save;
using Tartaria.Save.Serialization;

namespace Tartaria.Editor.Save
{
    /// <summary>
    /// Editor tools for testing and benchmarking serialization.
    /// Menu: TARTARIA > Save > Benchmark Serialization
    /// </summary>
    public static class SerializationBenchmarkEditor
    {
        [MenuItem("TARTARIA/Save/Run Serialization Benchmark")]
        public static void RunBenchmark()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SerializationBenchmark] Must be in Play Mode to benchmark. Starting Play Mode...");
                EditorApplication.EnterPlaymode();
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.delayCall += RunBenchmarkInPlayMode;
                };
                return;
            }

            RunBenchmarkInPlayMode();
        }

        static void RunBenchmarkInPlayMode()
        {
            if (SaveManager.Instance?.CurrentSave == null)
            {
                Debug.LogError("[SerializationBenchmark] No save data available. Load or create a save first.");
                return;
            }

            Debug.Log("=== RUNNING SERIALIZATION BENCHMARK ===");
            SerializationBenchmark.RunComprehensiveBenchmark(SaveManager.Instance.CurrentSave);
        }

        [MenuItem("TARTARIA/Save/Create Test Save (1000 items)")]
        public static void CreateLargeSave()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SerializationBenchmark] Must be in Play Mode to create test save.");
                return;
            }

            if (SaveManager.Instance?.CurrentSave == null)
            {
                Debug.LogError("[SerializationBenchmark] SaveManager not initialized.");
                return;
            }

            var save = SaveManager.Instance.CurrentSave;

            // Populate with test data to make it large
            var questList = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 500; i++)
            {
                questList.Add($"quest_{i}");
            }
            save.quests.activeQuests = questList.ToArray();

            var skillList = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 200; i++)
            {
                skillList.Add($"skill_{i}");
            }
            save.skillTree.unlockedSkills = skillList.ToArray();

            var flagList = new System.Collections.Generic.List<string>();
            for (int i = 0; i < 300; i++)
            {
                flagList.Add($"flag_{i}");
            }
            save.moonFlags.flags = flagList.ToArray();

            SaveManager.Instance.MarkDirty();
            SaveManager.Instance.Save();

            Debug.Log($"[SerializationBenchmark] Created large test save with 1000+ items");
        }

        [MenuItem("TARTARIA/Save/Open Save Folder")]
        public static void OpenSaveFolder()
        {
            string path = Application.persistentDataPath;
            EditorUtility.RevealInFinder(path);
            Debug.Log($"[SerializationBenchmark] Save folder: {path}");
        }

        [MenuItem("TARTARIA/Save/Clear All Saves")]
        public static void ClearAllSaves()
        {
            if (!EditorUtility.DisplayDialog("Clear All Saves", 
                "This will delete ALL save files. This cannot be undone.\n\nAre you sure?", 
                "Yes, Delete All", "Cancel"))
            {
                return;
            }

            string path = Application.persistentDataPath;
            var files = System.IO.Directory.GetFiles(path, "save_slot_*.dat");
            var backups = System.IO.Directory.GetFiles(path, "save_slot_*.backup.dat");
            var clouds = System.IO.Directory.GetFiles(path, "save_slot_*.cloud.dat");
            var jsons = System.IO.Directory.GetFiles(path, "save_slot_*.json");

            int count = 0;
            foreach (var file in files.Concat(backups).Concat(clouds).Concat(jsons))
            {
                System.IO.File.Delete(file);
                count++;
            }

            Debug.Log($"[SerializationBenchmark] Deleted {count} save files");
        }

        [MenuItem("TARTARIA/Save/Show Serialization Info")]
        public static void ShowSerializationInfo()
        {
            string info = "=== TARTARIA Serialization System ===\n\n";
            info += "Serializers:\n";
            info += "  - JSON: Human-readable, debug builds\n";
            info += "  - Binary: Fast, compact, production builds\n";
            info += "  - Hybrid: JSON metadata + binary data\n\n";
            info += "Features:\n";
            info += "  - GZip/Deflate compression (10x smaller)\n";
            info += "  - AES-256 encryption (prevent cheating)\n";
            info += "  - Async I/O (non-blocking)\n";
            info += "  - Backward compatible with old JSON saves\n\n";
            info += "Performance Targets:\n";
            info += "  - Save: <10ms (main thread)\n";
            info += "  - Load: <20ms (main thread)\n";
            info += "  - File size: <50KB (compressed)\n";
            info += "  - Zero GC allocations\n\n";
            info += "File Extensions:\n";
            info += "  - .dat = New optimized format\n";
            info += "  - .json = Legacy format (auto-migrated)\n";

            EditorUtility.DisplayDialog("Serialization Info", info, "OK");
        }
    }
}
