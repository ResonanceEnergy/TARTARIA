using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Tartaria.Save;

namespace Tartaria.Tests.StabilityTests
{
    /// <summary>
    /// Save File Bloat Analyzer — tracks save file growth and identifies bloat sources.
    /// 
    /// Agent 4: Long Session Stability Auditor
    /// 
    /// Detects:
    /// - Save file growth over time
    /// - Bloated data blocks (excessive list sizes, redundant data)
    /// - Compression efficiency
    /// - Backup file cleanup
    /// - Save/load performance
    /// 
    /// Usage:
    /// 1. Attach to a persistent GameObject
    /// 2. Call AnalyzeSaveFileGrowth() periodically during testing
    /// 3. Review bloat report and optimization recommendations
    /// </summary>
    public class SaveFileBloatAnalyzer : MonoBehaviour
    {
        [Header("Analysis Configuration")]
        [SerializeField] bool analyzeOnStart = false;
        [SerializeField] bool periodicAnalysis = true;
        [SerializeField] float analysisIntervalMinutes = 15f;

        [Header("Growth Thresholds")]
        [SerializeField] long maxSaveFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        [SerializeField] float maxGrowthPercentPerHour = 20f;
        [SerializeField] long maxBackupFilesCount = 5;

        struct SaveFileSnapshot
        {
            public float timestamp;
            public long fileSizeBytes;
            public long compressedSize;
            public float compressionRatio;
            public float saveDurationMs;
            public float loadDurationMs;
        }

        readonly List<SaveFileSnapshot> _history = new();
        float _nextAnalysisTime;
        string _logFilePath;
        StringBuilder _reportBuilder;

        void Awake()
        {
            string logDir = Path.Combine(Application.persistentDataPath, "stability_logs");
            Directory.CreateDirectory(logDir);
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logDir, $"save_bloat_report_{timestamp}.log");
            
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (analyzeOnStart)
            {
                AnalyzeSaveFileGrowth();
            }
            
            if (periodicAnalysis)
            {
                _nextAnalysisTime = Time.time + (analysisIntervalMinutes * 60f);
            }
        }

        void Update()
        {
            if (periodicAnalysis && Time.time >= _nextAnalysisTime)
            {
                AnalyzeSaveFileGrowth();
                _nextAnalysisTime = Time.time + (analysisIntervalMinutes * 60f);
            }
        }

        [ContextMenu("Analyze Save File Growth")]
        public void AnalyzeSaveFileGrowth()
        {
            _reportBuilder = new StringBuilder();
            
            Log("═══════════════════════════════════════════════════════");
            Log($"[Save Bloat Analysis] Started at {DateTime.Now:HH:mm:ss}");
            Log("═══════════════════════════════════════════════════════\n");

            AnalyzeCurrentSaveFile();
            AnalyzeBackupFiles();
            AnalyzeSavePerformance();
            AnalyzeDataBlockSizes();
            GenerateGrowthTrends();
            GenerateOptimizationRecommendations();

            Log("\n═══════════════════════════════════════════════════════");
            Log("[Save Bloat Analysis] Completed");
            Log("═══════════════════════════════════════════════════════");

            try
            {
                File.AppendAllText(_logFilePath, _reportBuilder.ToString() + "\n\n");
                Debug.Log($"[SaveBloatAnalyzer] Report saved to: {_logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveBloatAnalyzer] Failed to save report: {ex.Message}");
            }
        }

        void AnalyzeCurrentSaveFile()
        {
            Log("[1] CURRENT SAVE FILE ANALYSIS");
            Log("───────────────────────────────────────────────────────");
            
            string savePath = Path.Combine(Application.persistentDataPath, "save_slot_0.dat");
            
            if (!File.Exists(savePath))
            {
                Log("  ❌ No save file found");
                Log("");
                return;
            }
            
            FileInfo fileInfo = new FileInfo(savePath);
            long sizeBytes = fileInfo.Length;
            float sizeMB = sizeBytes / (1024f * 1024f);
            
            Log($"  File: {fileInfo.Name}");
            Log($"  Size: {sizeMB:F2} MB ({sizeBytes:N0} bytes)");
            Log($"  Last Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            
            if (sizeBytes > maxSaveFileSizeBytes)
            {
                Log($"  ⚠️ WARNING: Save file exceeds size threshold ({sizeMB:F2} MB > {maxSaveFileSizeBytes / (1024f * 1024f):F2} MB)");
            }
            else
            {
                Log($"  ✅ Size within threshold");
            }
            
            // Record snapshot
            float playTime = Time.realtimeSinceStartup;
            var snapshot = new SaveFileSnapshot
            {
                timestamp = playTime / 60f, // minutes
                fileSizeBytes = sizeBytes,
                compressedSize = sizeBytes, // Actual compressed size
                compressionRatio = 1.0f, // Would need uncompressed size to calculate
                saveDurationMs = 0,
                loadDurationMs = 0
            };
            
            _history.Add(snapshot);
            
            Log("");
        }

        void AnalyzeBackupFiles()
        {
            Log("[2] BACKUP FILE ANALYSIS");
            Log("───────────────────────────────────────────────────────");
            
            string dataPath = Application.persistentDataPath;
            var backupFiles = Directory.GetFiles(dataPath, "*.backup.dat")
                .Union(Directory.GetFiles(dataPath, "*_backup_*.dat"))
                .ToList();
            
            if (backupFiles.Count == 0)
            {
                Log("  No backup files found");
                Log("");
                return;
            }
            
            long totalBackupSize = 0;
            foreach (string backupPath in backupFiles)
            {
                FileInfo info = new FileInfo(backupPath);
                totalBackupSize += info.Length;
                
                TimeSpan age = DateTime.Now - info.LastWriteTime;
                Log($"  {Path.GetFileName(backupPath)}: {info.Length / 1024f:F1} KB (age: {age.TotalHours:F1}h)");
            }
            
            float totalBackupMB = totalBackupSize / (1024f * 1024f);
            Log($"\n  Total: {backupFiles.Count} backups, {totalBackupMB:F2} MB");
            
            if (backupFiles.Count > maxBackupFilesCount)
            {
                Log($"  ⚠️ WARNING: Excessive backup files ({backupFiles.Count} > {maxBackupFilesCount})");
                Log($"  RECOMMENDATION: Implement backup cleanup policy (keep last {maxBackupFilesCount} backups)");
            }
            else
            {
                Log($"  ✅ Backup count within limit");
            }
            
            Log("");
        }

        void AnalyzeSavePerformance()
        {
            Log("[3] SAVE/LOAD PERFORMANCE TEST");
            Log("───────────────────────────────────────────────────────");
            
            if (SaveManager.Instance == null)
            {
                Log("  ❌ SaveManager not available");
                Log("");
                return;
            }
            
            try
            {
                // Test save performance
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                SaveManager.Instance.Save();
                stopwatch.Stop();
                float saveDurationMs = stopwatch.ElapsedMilliseconds;
                
                Log($"  Save Duration: {saveDurationMs:F1} ms");
                
                if (saveDurationMs > 1000f)
                {
                    Log($"  ⚠️ WARNING: Save takes over 1 second ({saveDurationMs:F0} ms)");
                }
                else if (saveDurationMs > 500f)
                {
                    Log($"  ⚠️ CAUTION: Save duration approaching 500ms threshold");
                }
                else
                {
                    Log($"  ✅ Save performance acceptable");
                }
                
                // Test load performance
                stopwatch.Restart();
                SaveManager.Instance.Load();
                stopwatch.Stop();
                float loadDurationMs = stopwatch.ElapsedMilliseconds;
                
                Log($"  Load Duration: {loadDurationMs:F1} ms");
                
                if (loadDurationMs > 2000f)
                {
                    Log($"  ⚠️ WARNING: Load takes over 2 seconds ({loadDurationMs:F0} ms)");
                }
                else if (loadDurationMs > 1000f)
                {
                    Log($"  ⚠️ CAUTION: Load duration approaching 1s threshold");
                }
                else
                {
                    Log($"  ✅ Load performance acceptable");
                }
                
                // Update last snapshot with timing
                if (_history.Count > 0)
                {
                    var last = _history[_history.Count - 1];
                    last.saveDurationMs = saveDurationMs;
                    last.loadDurationMs = loadDurationMs;
                    _history[_history.Count - 1] = last;
                }
            }
            catch (Exception ex)
            {
                Log($"  ❌ Performance test failed: {ex.Message}");
            }
            
            Log("");
        }

        void AnalyzeDataBlockSizes()
        {
            Log("[4] DATA BLOCK SIZE ANALYSIS");
            Log("───────────────────────────────────────────────────────");
            
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null)
            {
                Log("  ❌ No save data available for analysis");
                Log("");
                return;
            }
            
            SaveData save = SaveManager.Instance.CurrentSave;
            
            // Analyze key data blocks (approximate sizes via serialization)
            var blocks = new Dictionary<string, object>
            {
                { "Player", save.player },
                { "World", save.world },
                { "Quests", save.quests },
                { "Workshop", save.workshop },
                { "Corruption", save.corruption },
                { "Campaign", save.campaign },
                { "SkillTree", save.skillTree },
                { "Economy", save.economy },
                { "Crafting", save.crafting },
                { "Archive", save.archive },
                { "AirshipFleet", save.airshipFleet },
                { "DialogueArcs", save.dialogueArcs }
            };
            
            var sizes = new List<KeyValuePair<string, long>>();
            
            foreach (var kvp in blocks)
            {
                try
                {
                    string json = JsonUtility.ToJson(kvp.Value);
                    long sizeBytes = System.Text.Encoding.UTF8.GetByteCount(json);
                    sizes.Add(new KeyValuePair<string, long>(kvp.Key, sizeBytes));
                }
                catch (Exception ex)
                {
                    Log($"  ⚠️ Failed to serialize {kvp.Key}: {ex.Message}");
                }
            }
            
            // Sort by size descending
            sizes.Sort((a, b) => b.Value.CompareTo(a.Value));
            
            Log("  Data Block Sizes (top 10):");
            foreach (var kvp in sizes.Take(10))
            {
                float sizeKB = kvp.Value / 1024f;
                Log($"    {kvp.Key}: {sizeKB:F1} KB");
                
                if (sizeKB > 500f)
                {
                    Log($"      ⚠️ Large data block (> 500 KB) — consider optimization");
                }
            }
            
            long totalSize = sizes.Sum(kvp => kvp.Value);
            Log($"\n  Total Analyzed: {totalSize / 1024f:F1} KB");
            
            Log("");
        }

        void GenerateGrowthTrends()
        {
            Log("[5] GROWTH TREND ANALYSIS");
            Log("───────────────────────────────────────────────────────");
            
            if (_history.Count < 2)
            {
                Log("  Insufficient data for trend analysis (need 2+ snapshots)");
                Log("");
                return;
            }
            
            var first = _history.First();
            var last = _history.Last();
            
            float timeDeltaMinutes = last.timestamp - first.timestamp;
            float timeDeltaHours = timeDeltaMinutes / 60f;
            
            long sizeGrowth = last.fileSizeBytes - first.fileSizeBytes;
            float growthPercent = ((float)sizeGrowth / first.fileSizeBytes) * 100f;
            float growthPerHour = timeDeltaHours > 0 ? growthPercent / timeDeltaHours : 0f;
            
            Log($"  Time Period: {timeDeltaMinutes:F1} minutes ({timeDeltaHours:F2} hours)");
            Log($"  Initial Size: {first.fileSizeBytes / 1024f:F1} KB");
            Log($"  Final Size: {last.fileSizeBytes / 1024f:F1} KB");
            Log($"  Growth: +{sizeGrowth / 1024f:F1} KB ({growthPercent:F1}%)");
            Log($"  Growth Rate: {growthPerHour:F1}% per hour");
            
            if (growthPerHour > maxGrowthPercentPerHour)
            {
                Log($"  ⚠️ WARNING: Growth rate exceeds threshold ({growthPerHour:F1}% > {maxGrowthPercentPerHour}% per hour)");
                Log($"  CRITICAL: Save file growing too fast — memory leak likely");
            }
            else
            {
                Log($"  ✅ Growth rate acceptable");
            }
            
            // Extrapolate to 10 hours
            if (timeDeltaHours > 0)
            {
                float projectedSizeAfter10h = first.fileSizeBytes * (1 + (growthPerHour / 100f * 10f));
                Log($"\n  Projected size after 10 hours: {projectedSizeAfter10h / (1024f * 1024f):F2} MB");
                
                if (projectedSizeAfter10h > maxSaveFileSizeBytes)
                {
                    Log($"  ⚠️ WARNING: Projected size will exceed threshold after 10 hours");
                }
            }
            
            Log("");
        }

        void GenerateOptimizationRecommendations()
        {
            Log("[6] OPTIMIZATION RECOMMENDATIONS");
            Log("───────────────────────────────────────────────────────");
            
            bool hasIssues = false;
            
            // Check save file size
            if (_history.Any() && _history.Last().fileSizeBytes > maxSaveFileSizeBytes * 0.8f)
            {
                Log("  📋 Save file approaching size limit:");
                Log("     - Enable compression (GZip)");
                Log("     - Remove redundant data from save blocks");
                Log("     - Prune old history entries (keep last N only)");
                hasIssues = true;
            }
            
            // Check backup files
            string dataPath = Application.persistentDataPath;
            var backupFiles = Directory.GetFiles(dataPath, "*.backup.dat");
            if (backupFiles.Length > maxBackupFilesCount)
            {
                Log("\n  📋 Excessive backup files:");
                Log("     - Implement backup rotation (delete old backups)");
                Log($"     - Keep only last {maxBackupFilesCount} backups");
                Log("     - Delete backups older than 7 days");
                hasIssues = true;
            }
            
            // Check growth rate
            if (_history.Count >= 2)
            {
                var first = _history.First();
                var last = _history.Last();
                float timeDeltaHours = (last.timestamp - first.timestamp) / 60f;
                float growthPerHour = timeDeltaHours > 0 ? 
                    (((float)(last.fileSizeBytes - first.fileSizeBytes) / first.fileSizeBytes) * 100f) / timeDeltaHours : 0f;
                
                if (growthPerHour > maxGrowthPercentPerHour)
                {
                    Log("\n  📋 High growth rate detected:");
                    Log("     - Review save data for accumulating lists/arrays");
                    Log("     - Implement list size caps (e.g., max 100 quest history entries)");
                    Log("     - Clear temporary data on save");
                    Log("     - Check for memory leaks in data providers");
                    hasIssues = true;
                }
            }
            
            // Check save/load performance
            if (_history.Any())
            {
                var last = _history.Last();
                if (last.saveDurationMs > 500f || last.loadDurationMs > 1000f)
                {
                    Log("\n  📋 Save/load performance issues:");
                    Log("     - Profile serialization bottlenecks");
                    Log("     - Use binary serialization instead of JSON");
                    Log("     - Implement async save/load");
                    Log("     - Reduce data block complexity");
                    hasIssues = true;
                }
            }
            
            if (!hasIssues)
            {
                Log("  ✅ No critical optimization issues detected");
            }
            
            Log("");
        }

        void Log(string message)
        {
            Debug.Log($"[SaveBloatAnalyzer] {message}");
            _reportBuilder?.AppendLine(message);
        }

        [ContextMenu("Clear History")]
        public void ClearHistory()
        {
            _history.Clear();
            Debug.Log("[SaveBloatAnalyzer] Snapshot history cleared");
        }
    }
}
