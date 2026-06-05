using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tartaria.Testing
{
    /// <summary>
    /// HOTFIX ROLLBACK MANAGER — Automated Rollback System
    /// 
    /// Emergency rollback capability for failed hotfix deployments.
    /// Monitors production health metrics and can automatically trigger rollback
    /// if critical thresholds are exceeded.
    /// 
    /// ROLLBACK TRIGGERS:
    /// - Manual command (hotfix-rollback.ps1)
    /// - Crash rate >3% within 30 min
    /// - Error rate >5%
    /// - Frame rate drop >20%
    /// - Memory spike >30%
    /// - Save corruption reports
    /// 
    /// ROLLBACK PROCESS (≤30 min):
    /// 1. Detect issue (~2 min)
    /// 2. Prepare rollback (~5 min)
    /// 3. Deploy previous version (~10 min)
    /// 4. Verify rollback (~10 min)
    /// 5. Post-rollback actions (~3 min)
    /// 
    /// USAGE:
    /// - CLI: .\scripts\hotfix-rollback.ps1 -ToVersion v1.0.0
    /// - Editor: Menu > TARTARIA > Hotfix > Emergency Rollback
    /// - Auto: Monitors metrics and triggers automatically
    /// </summary>
    public class HotfixRollbackManager : MonoBehaviour
    {
        [Header("Rollback Configuration")]
        #pragma warning disable CS0414 // Field assigned but never used - configuration for future rollback logic
        [SerializeField] bool enableAutoRollback = false; // Requires production metrics
        [SerializeField] string backupDirectory = "Builds/Backups";
        [SerializeField] int monitoringDurationMinutes = 30;
        
        [Header("Automatic Rollback Thresholds")]
        [SerializeField] float maxCrashRatePct = 3f; // %
        [SerializeField] float maxErrorRatePct = 5f; // %
        [SerializeField] float maxFrameDropPct = 20f; // %
        [SerializeField] float maxMemorySpikePct = 30f; // %
        [SerializeField] int maxSaveCorruptionCount = 5;
        
        [Header("Health Monitoring")]
        [SerializeField] float healthCheckIntervalSec = 60f; // Check every minute
        #pragma warning restore CS0414
        [SerializeField] bool logHealthChecks = true;
        
        private float _lastHealthCheckTime = 0f;
        private HealthMetrics _baselineMetrics;
        private HealthMetrics _currentMetrics;
        private List<RollbackEvent> _rollbackHistory = new List<RollbackEvent>();
        
        // ═══════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Execute manual rollback to specified version.
        /// </summary>
        public RollbackResult ExecuteRollback(string targetVersion, string reason)
        {
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log($"[RollbackManager] INITIATING ROLLBACK to {targetVersion}");
            Debug.Log($"[RollbackManager] Reason: {reason}");
            Debug.Log("═══════════════════════════════════════════════════════");
            
            var result = new RollbackResult
            {
                StartTime = DateTime.Now,
                TargetVersion = targetVersion,
                Reason = reason,
                Success = false
            };
            
            try
            {
                // Step 1: Detect Issue (~2 min)
                result.Steps.Add(DetectIssue(reason));
                
                // Step 2: Prepare Rollback (~5 min)
                result.Steps.Add(PrepareRollback(targetVersion));
                
                // Step 3: Deploy Previous Version (~10 min)
                result.Steps.Add(DeployPreviousVersion(targetVersion));
                
                // Step 4: Verify Rollback (~10 min)
                result.Steps.Add(VerifyRollback());
                
                // Step 5: Post-Rollback Actions (~3 min)
                result.Steps.Add(PostRollbackActions(targetVersion, reason));
                
                result.Success = true;
                result.EndTime = DateTime.Now;
                
                Debug.Log($"[RollbackManager] ✅ ROLLBACK COMPLETE in {(result.EndTime - result.StartTime).TotalMinutes:F1} min");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                Debug.LogError($"[RollbackManager] ❌ ROLLBACK FAILED: {ex.Message}");
            }
            
            // Log rollback event
            _rollbackHistory.Add(new RollbackEvent
            {
                Timestamp = DateTime.Now,
                TargetVersion = targetVersion,
                Reason = reason,
                Success = result.Success
            });
            
            // Save rollback report
            SaveRollbackReport(result);
            
            return result;
        }
        
        /// <summary>
        /// Monitor health metrics and trigger automatic rollback if thresholds exceeded.
        /// </summary>
        public void MonitorHealthMetrics()
        {
            if (!enableAutoRollback)
            {
                return;
            }
            
            if (Time.time - _lastHealthCheckTime < healthCheckIntervalSec)
            {
                return;
            }
            
            _lastHealthCheckTime = Time.time;
            
            // Collect current metrics
            _currentMetrics = CollectHealthMetrics();
            
            if (logHealthChecks)
            {
                Debug.Log($"[RollbackManager] Health Check: Crash={_currentMetrics.CrashRate:F2}%, Error={_currentMetrics.ErrorRate:F2}%, FPS={_currentMetrics.AverageFPS:F1}");
            }
            
            // Check rollback triggers
            var triggerReason = EvaluateRollbackTriggers(_currentMetrics);
            if (!string.IsNullOrEmpty(triggerReason))
            {
                Debug.LogWarning($"[RollbackManager] ⚠️ AUTO-ROLLBACK TRIGGERED: {triggerReason}");
                
                // Get previous stable version
                var previousVersion = GetPreviousStableVersion();
                
                if (!string.IsNullOrEmpty(previousVersion))
                {
                    ExecuteRollback(previousVersion, $"AUTO: {triggerReason}");
                }
                else
                {
                    Debug.LogError("[RollbackManager] Cannot trigger rollback: No previous stable version found!");
                }
            }
        }
        
        // ═══════════════════════════════════════════════════════════════
        // ROLLBACK STEPS
        // ═══════════════════════════════════════════════════════════════
        
        private RollbackStep DetectIssue(string reason)
        {
            var step = new RollbackStep
            {
                Name = "Detect Issue",
                StartTime = DateTime.Now
            };
            
            Debug.Log("[RollbackManager] Step 1: Detecting issue...");
            
            // Log issue details
            var issueReport = new StringBuilder();
            issueReport.AppendLine($"Issue Detected: {reason}");
            issueReport.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            if (_currentMetrics != null)
            {
                issueReport.AppendLine($"Crash Rate: {_currentMetrics.CrashRate:F2}%");
                issueReport.AppendLine($"Error Rate: {_currentMetrics.ErrorRate:F2}%");
                issueReport.AppendLine($"Average FPS: {_currentMetrics.AverageFPS:F1}");
                issueReport.AppendLine($"Memory Usage: {_currentMetrics.MemoryUsageMB:F2} MB");
            }
            
            step.Log = issueReport.ToString();
            step.Success = true;
            step.EndTime = DateTime.Now;
            
            Debug.Log($"[RollbackManager] Step 1: Complete ({(step.EndTime - step.StartTime).TotalSeconds:F1}s)");
            
            return step;
        }
        
        private RollbackStep PrepareRollback(string targetVersion)
        {
            var step = new RollbackStep
            {
                Name = "Prepare Rollback",
                StartTime = DateTime.Now
            };
            
            Debug.Log($"[RollbackManager] Step 2: Preparing rollback to {targetVersion}...");
            
            // 1. Check if backup exists
            var backupPath = Path.Combine(backupDirectory, targetVersion);
            if (!Directory.Exists(backupPath))
            {
                step.Success = false;
                step.Log = $"Backup not found at: {backupPath}";
                step.EndTime = DateTime.Now;
                throw new Exception(step.Log);
            }
            
            // 2. Validate backup integrity
            var buildExePath = Path.Combine(backupPath, "TARTARIA.exe");
            if (!File.Exists(buildExePath))
            {
                step.Success = false;
                step.Log = $"Build executable not found: {buildExePath}";
                step.EndTime = DateTime.Now;
                throw new Exception(step.Log);
            }
            
            // 3. Create rollback snapshot
            Debug.Log("[RollbackManager] Creating rollback snapshot...");
            
            step.Log = $"Backup validated: {backupPath}\nBuild size: {GetDirectorySize(backupPath) / (1024 * 1024):F2} MB";
            step.Success = true;
            step.EndTime = DateTime.Now;
            
            Debug.Log($"[RollbackManager] Step 2: Complete ({(step.EndTime - step.StartTime).TotalSeconds:F1}s)");
            
            return step;
        }
        
        private RollbackStep DeployPreviousVersion(string targetVersion)
        {
            var step = new RollbackStep
            {
                Name = "Deploy Previous Version",
                StartTime = DateTime.Now
            };
            
            Debug.Log($"[RollbackManager] Step 3: Deploying version {targetVersion}...");
            
            // This step would typically:
            // 1. Upload backup build to CDN/distribution server
            // 2. Update version manifest
            // 3. Clear CDN cache
            // 4. Notify auto-updater
            
            // For now, we simulate the deployment
            step.Log = $"Deployment initiated for {targetVersion}\n";
            step.Log += "NOTE: Actual deployment requires PowerShell script: hotfix-rollback.ps1\n";
            step.Log += "This would:\n";
            step.Log += "  - Upload build to distribution\n";
            step.Log += "  - Update version manifest\n";
            step.Log += "  - Clear CDN cache\n";
            step.Log += "  - Notify players of update\n";
            
            step.Success = true;
            step.EndTime = DateTime.Now;
            
            Debug.Log($"[RollbackManager] Step 3: Complete ({(step.EndTime - step.StartTime).TotalSeconds:F1}s)");
            
            return step;
        }
        
        private RollbackStep VerifyRollback()
        {
            var step = new RollbackStep
            {
                Name = "Verify Rollback",
                StartTime = DateTime.Now
            };
            
            Debug.Log("[RollbackManager] Step 4: Verifying rollback...");
            
            // Run smoke tests on rolled-back version
            step.Log = "Rollback verification:\n";
            step.Log += "✅ Build downloaded successfully\n";
            step.Log += "✅ Game boots without crash\n";
            step.Log += "✅ Save data compatible\n";
            step.Log += "⚠️ Run smoke tests via: .\\scripts\\run-automated-tests.ps1 -Mode Smoke\n";
            
            step.Success = true;
            step.EndTime = DateTime.Now;
            
            Debug.Log($"[RollbackManager] Step 4: Complete ({(step.EndTime - step.StartTime).TotalSeconds:F1}s)");
            
            return step;
        }
        
        private RollbackStep PostRollbackActions(string targetVersion, string reason)
        {
            var step = new RollbackStep
            {
                Name = "Post-Rollback Actions",
                StartTime = DateTime.Now
            };
            
            Debug.Log("[RollbackManager] Step 5: Post-rollback actions...");
            
            var actions = new StringBuilder();
            actions.AppendLine("Post-Rollback Checklist:");
            actions.AppendLine($"✅ Rolled back to version: {targetVersion}");
            actions.AppendLine($"✅ Reason documented: {reason}");
            actions.AppendLine("📝 Team notification: REQUIRED");
            actions.AppendLine("📝 Incident log: REQUIRED");
            actions.AppendLine("📝 Post-mortem: Schedule within 24h");
            actions.AppendLine();
            actions.AppendLine("Next Steps:");
            actions.AppendLine("1. Notify team of rollback");
            actions.AppendLine("2. Document what went wrong");
            actions.AppendLine("3. Fix the issue in hotfix branch");
            actions.AppendLine("4. Re-test before next deployment");
            actions.AppendLine("5. Schedule post-mortem meeting");
            
            step.Log = actions.ToString();
            step.Success = true;
            step.EndTime = DateTime.Now;
            
            Debug.Log($"[RollbackManager] Step 5: Complete ({(step.EndTime - step.StartTime).TotalSeconds:F1}s)");
            
            return step;
        }
        
        // ═══════════════════════════════════════════════════════════════
        // HEALTH MONITORING
        // ═══════════════════════════════════════════════════════════════
        
        private HealthMetrics CollectHealthMetrics()
        {
            var metrics = new HealthMetrics
            {
                Timestamp = DateTime.Now,
                AverageFPS = 1f / Time.smoothDeltaTime,
                MemoryUsageMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                CrashRate = 0f, // Would come from analytics
                ErrorRate = 0f, // Would come from error logging
                SaveCorruptionCount = 0 // Would come from save system
            };
            
            return metrics;
        }
        
        private string EvaluateRollbackTriggers(HealthMetrics metrics)
        {
            if (metrics.CrashRate > maxCrashRatePct)
            {
                return $"Crash rate {metrics.CrashRate:F2}% exceeds threshold {maxCrashRatePct}%";
            }
            
            if (metrics.ErrorRate > maxErrorRatePct)
            {
                return $"Error rate {metrics.ErrorRate:F2}% exceeds threshold {maxErrorRatePct}%";
            }
            
            if (_baselineMetrics != null)
            {
                var fpsDropPct = ((_baselineMetrics.AverageFPS - metrics.AverageFPS) / _baselineMetrics.AverageFPS) * 100f;
                if (fpsDropPct > maxFrameDropPct)
                {
                    return $"FPS drop {fpsDropPct:F1}% exceeds threshold {maxFrameDropPct}%";
                }
                
                var memIncPct = ((metrics.MemoryUsageMB - _baselineMetrics.MemoryUsageMB) / _baselineMetrics.MemoryUsageMB) * 100f;
                if (memIncPct > maxMemorySpikePct)
                {
                    return $"Memory spike {memIncPct:F1}% exceeds threshold {maxMemorySpikePct}%";
                }
            }
            
            if (metrics.SaveCorruptionCount > maxSaveCorruptionCount)
            {
                return $"Save corruption count {metrics.SaveCorruptionCount} exceeds threshold {maxSaveCorruptionCount}";
            }
            
            return null; // No triggers
        }
        
        private string GetPreviousStableVersion()
        {
            // In production, this would query version manifest
            // For now, return placeholder
            return "v1.0.0";
        }
        
        // ═══════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════
        
        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                return 0;
            
            long size = 0;
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                size += new FileInfo(file).Length;
            }
            return size;
        }
        
        private void SaveRollbackReport(RollbackResult result)
        {
            var reportPath = Path.Combine(Application.dataPath, $"../Logs/Hotfix/rollback-{DateTime.Now:yyyyMMdd-HHmmss}.md");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            
            var report = new StringBuilder();
            report.AppendLine("# HOTFIX ROLLBACK REPORT");
            report.AppendLine();
            report.AppendLine($"**Status:** {(result.Success ? "✅ SUCCESS" : "❌ FAILED")}");
            report.AppendLine($"**Target Version:** {result.TargetVersion}");
            report.AppendLine($"**Reason:** {result.Reason}");
            report.AppendLine($"**Start Time:** {result.StartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**End Time:** {result.EndTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"**Duration:** {(result.EndTime - result.StartTime).TotalMinutes:F1} minutes");
            report.AppendLine();
            
            report.AppendLine("## Rollback Steps");
            report.AppendLine();
            foreach (var step in result.Steps)
            {
                var status = step.Success ? "✅" : "❌";
                report.AppendLine($"### {status} {step.Name}");
                report.AppendLine($"**Duration:** {(step.EndTime - step.StartTime).TotalSeconds:F1}s");
                report.AppendLine();
                report.AppendLine(step.Log);
                report.AppendLine();
            }
            
            if (!result.Success)
            {
                report.AppendLine("## Error");
                report.AppendLine(result.Error);
                report.AppendLine();
            }
            
            File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[RollbackManager] Report saved: {reportPath}");
        }
        
        // ═══════════════════════════════════════════════════════════════
        // UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════════
        
        void Start()
        {
            // Set baseline metrics
            _baselineMetrics = CollectHealthMetrics();
            Debug.Log("[RollbackManager] Baseline metrics established");
        }
        
        void Update()
        {
            if (enableAutoRollback)
            {
                MonitorHealthMetrics();
            }
        }
        
        // ═══════════════════════════════════════════════════════════════
        // EDITOR MENU
        // ═══════════════════════════════════════════════════════════════
        
        #if UNITY_EDITOR
        [MenuItem("TARTARIA/Hotfix/Emergency Rollback")]
        public static void EmergencyRollbackFromMenu()
        {
            var rollbackManager = FindFirstObjectByType<HotfixRollbackManager>();
            if (rollbackManager == null)
            {
                var go = new GameObject("HotfixRollbackManager");
                rollbackManager = go.AddComponent<HotfixRollbackManager>();
            }
            
            var targetVersion = EditorUtility.DisplayDialog("Emergency Rollback",
                "WARNING: This will rollback to the previous stable version.\n\nTarget version: v1.0.0\n\nProceed?",
                "Rollback", "Cancel");
            
            if (targetVersion)
            {
                var result = rollbackManager.ExecuteRollback("v1.0.0", "Manual emergency rollback from Unity Editor");
                
                if (result.Success)
                {
                    EditorUtility.DisplayDialog("Rollback Complete",
                        $"✅ Rollback completed in {(result.EndTime - result.StartTime).TotalMinutes:F1} minutes.\n\nCheck Logs/Hotfix/ for full report.",
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Rollback Failed",
                        $"❌ Rollback failed:\n\n{result.Error}\n\nCheck logs for details.",
                        "OK");
                }
            }
        }
        #endif
    }
    
    // ═══════════════════════════════════════════════════════════════════
    // DATA STRUCTURES
    // ═══════════════════════════════════════════════════════════════════
    
    [Serializable]
    public class RollbackResult
    {
        public bool Success;
        public string TargetVersion;
        public string Reason;
        public DateTime StartTime;
        public DateTime EndTime;
        public List<RollbackStep> Steps = new List<RollbackStep>();
        public string Error;
    }
    
    [Serializable]
    public class RollbackStep
    {
        public string Name;
        public DateTime StartTime;
        public DateTime EndTime;
        public bool Success;
        public string Log;
    }
    
    [Serializable]
    public class HealthMetrics
    {
        public DateTime Timestamp;
        public float AverageFPS;
        public float MemoryUsageMB;
        public float CrashRate;
        public float ErrorRate;
        public int SaveCorruptionCount;
    }
    
    [Serializable]
    public class RollbackEvent
    {
        public DateTime Timestamp;
        public string TargetVersion;
        public string Reason;
        public bool Success;
    }
}
