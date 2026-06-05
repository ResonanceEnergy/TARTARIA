using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 7: Feedback prioritizer for beta testing.
    /// Automatically ranks feedback reports by:
    /// - Frequency (how many reports mention the same issue)
    /// - Severity (P0 blocks progression, P1 major annoyance, P2 polish)
    /// - Recency (recent reports weighted higher)
    /// 
    /// Scans Logs/Feedback/*.txt and generates priority rankings.
    /// Run this tool manually or via CI/CD after collecting feedback batches.
    /// 
    /// Usage:
    /// FeedbackPrioritizer.AnalyzeFeedback();
    /// FeedbackPrioritizer.GeneratePriorityReport();
    /// </summary>
    public static class FeedbackPrioritizer
    {
        static string _feedbackDir;
        static string _outputDir;
        static List<FeedbackIssue> _issues = new List<FeedbackIssue>();
        static bool _initialized;

        static void Initialize()
        {
            if (_initialized) return;
            
            _feedbackDir = Path.Combine(Application.dataPath, "..", "Logs", "Feedback");
            _outputDir = Path.Combine(Application.dataPath, "..", "Logs", "FeedbackAnalysis");
            
            if (!Directory.Exists(_feedbackDir))
                Directory.CreateDirectory(_feedbackDir);
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);
            
            _initialized = true;
        }

        /// <summary>
        /// Analyze all feedback reports and cluster by issue.
        /// </summary>
        public static void AnalyzeFeedback()
        {
            Initialize();
            
            _issues.Clear();
            
            var files = Directory.GetFiles(_feedbackDir, "feedback-*.txt");
            Debug.Log($"[FeedbackPrioritizer] Analyzing {files.Length} feedback reports...");
            
            foreach (var file in files)
            {
                try
                {
                    var report = ParseFeedbackFile(file);
                    if (report != null)
                    {
                        ProcessReport(report);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[FeedbackPrioritizer] Failed to parse {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            
            // Calculate priority scores
            foreach (var issue in _issues)
            {
                issue.CalculatePriorityScore();
            }
            
            // Sort by priority (highest first)
            _issues = _issues.OrderByDescending(i => i.priorityScore).ToList();
            
            Debug.Log($"[FeedbackPrioritizer] Analysis complete. Found {_issues.Count} unique issues.");
        }

        /// <summary>
        /// Generate a priority report (markdown format).
        /// </summary>
        public static void GeneratePriorityReport()
        {
            Initialize();
            
            if (_issues.Count == 0)
            {
                Debug.LogWarning("[FeedbackPrioritizer] No issues to report. Run AnalyzeFeedback() first.");
                return;
            }
            
            string filename = $"FeedbackPriorityReport-{DateTime.Now:yyyy-MM-dd}.md";
            string filepath = Path.Combine(_outputDir, filename);
            
            try
            {
                using (var writer = new StreamWriter(filepath, false))
                {
                    writer.WriteLine("# TARTARIA Beta Feedback — Priority Report");
                    writer.WriteLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"**Total Reports:** {CountTotalReports()}");
                    writer.WriteLine($"**Unique Issues:** {_issues.Count}");
                    writer.WriteLine();
                    
                    // Summary by priority
                    var p0 = _issues.Count(i => i.priority == Priority.P0);
                    var p1 = _issues.Count(i => i.priority == Priority.P1);
                    var p2 = _issues.Count(i => i.priority == Priority.P2);
                    
                    writer.WriteLine("## Priority Summary");
                    writer.WriteLine($"- **P0 (Critical):** {p0} issues — blocks progression");
                    writer.WriteLine($"- **P1 (High):** {p1} issues — major annoyances");
                    writer.WriteLine($"- **P2 (Medium):** {p2} issues — polish/QoL");
                    writer.WriteLine();
                    
                    // Top 10 by frequency
                    writer.WriteLine("## Top 10 Most-Reported Issues");
                    var top10 = _issues.OrderByDescending(i => i.reportCount).Take(10).ToList();
                    for (int i = 0; i < top10.Count; i++)
                    {
                        var issue = top10[i];
                        writer.WriteLine($"{i + 1}. **[{issue.priority}]** {issue.title}");
                        writer.WriteLine($"   - Reports: {issue.reportCount}");
                        writer.WriteLine($"   - Category: {issue.category}");
                        writer.WriteLine($"   - Priority Score: {issue.priorityScore:F1}");
                        writer.WriteLine();
                    }
                    
                    // P0 issues (critical)
                    var p0Issues = _issues.Where(i => i.priority == Priority.P0).ToList();
                    if (p0Issues.Count > 0)
                    {
                        writer.WriteLine("## P0 Issues (Critical — Fix Immediately)");
                        foreach (var issue in p0Issues)
                        {
                            writer.WriteLine($"### {issue.title}");
                            writer.WriteLine($"- **Category:** {issue.category}");
                            writer.WriteLine($"- **Reports:** {issue.reportCount}");
                            writer.WriteLine($"- **First Seen:** {issue.firstReportDate:yyyy-MM-dd}");
                            writer.WriteLine($"- **Last Seen:** {issue.lastReportDate:yyyy-MM-dd}");
                            writer.WriteLine($"- **Description:** {issue.description}");
                            writer.WriteLine();
                        }
                    }
                    
                    // P1 issues (high priority)
                    var p1Issues = _issues.Where(i => i.priority == Priority.P1).ToList();
                    if (p1Issues.Count > 0)
                    {
                        writer.WriteLine("## P1 Issues (High Priority)");
                        foreach (var issue in p1Issues)
                        {
                            writer.WriteLine($"### {issue.title}");
                            writer.WriteLine($"- **Category:** {issue.category}");
                            writer.WriteLine($"- **Reports:** {issue.reportCount}");
                            writer.WriteLine($"- **Description:** {issue.description}");
                            writer.WriteLine();
                        }
                    }
                    
                    // P2 issues (polish)
                    var p2Issues = _issues.Where(i => i.priority == Priority.P2).ToList();
                    if (p2Issues.Count > 0)
                    {
                        writer.WriteLine("## P2 Issues (Polish/QoL)");
                        foreach (var issue in p2Issues)
                        {
                            writer.WriteLine($"- **{issue.title}** ({issue.reportCount} reports)");
                        }
                        writer.WriteLine();
                    }
                    
                    writer.WriteLine("---");
                    writer.WriteLine("*Report generated by FeedbackPrioritizer (Agent 7)*");
                }
                
                Debug.Log($"[FeedbackPrioritizer] Priority report generated: {filename}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FeedbackPrioritizer] Failed to generate report: {ex.Message}");
            }
        }

        static FeedbackReportData ParseFeedbackFile(string filepath)
        {
            var lines = File.ReadAllLines(filepath);
            if (lines.Length < 5) return null;
            
            var report = new FeedbackReportData();
            
            foreach (var line in lines)
            {
                if (line.StartsWith("Timestamp:"))
                {
                    report.timestamp = DateTime.Parse(line.Substring(11).Trim());
                }
                else if (line.StartsWith("Type:"))
                {
                    report.type = (FeedbackType)Enum.Parse(typeof(FeedbackType), line.Substring(6).Trim());
                }
                else if (line.StartsWith("Title:"))
                {
                    report.title = line.Substring(7).Trim();
                }
                else if (line.StartsWith("Description:"))
                {
                    // Multi-line description
                    int descIndex = Array.IndexOf(lines, line);
                    var descLines = new List<string>();
                    for (int i = descIndex + 1; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("===") || lines[i].StartsWith("Scene:"))
                            break;
                        descLines.Add(lines[i]);
                    }
                    report.description = string.Join(" ", descLines).Trim();
                }
                else if (line.StartsWith("Scene:"))
                {
                    report.sceneName = line.Substring(7).Trim();
                }
            }
            
            return report;
        }

        static void ProcessReport(FeedbackReportData report)
        {
            // Try to match with existing issue
            var matchingIssue = FindMatchingIssue(report);
            
            if (matchingIssue != null)
            {
                // Increment report count
                matchingIssue.reportCount++;
                matchingIssue.lastReportDate = report.timestamp;
            }
            else
            {
                // Create new issue
                var issue = new FeedbackIssue
                {
                    title = report.title,
                    description = report.description ?? "No description provided",
                    category = report.type,
                    reportCount = 1,
                    firstReportDate = report.timestamp,
                    lastReportDate = report.timestamp,
                    sceneName = report.sceneName
                };
                
                // Auto-assign priority based on keywords and category
                issue.priority = InferPriority(report);
                
                _issues.Add(issue);
            }
        }

        static FeedbackIssue FindMatchingIssue(FeedbackReportData report)
        {
            // Simple matching: same title (case-insensitive)
            // In production, use fuzzy matching or NLP clustering
            return _issues.FirstOrDefault(i => 
                string.Equals(i.title, report.title, StringComparison.OrdinalIgnoreCase));
        }

        static Priority InferPriority(FeedbackReportData report)
        {
            // Keyword-based priority inference
            string combined = $"{report.title} {report.description}".ToLower();
            
            // P0: blocks progression
            if (combined.Contains("softlock") || combined.Contains("stuck") || 
                combined.Contains("can't progress") || combined.Contains("quest broke") ||
                combined.Contains("crash") || combined.Contains("game won't load"))
            {
                return Priority.P0;
            }
            
            // P1: major annoyances
            if (combined.Contains("broken") || combined.Contains("doesn't work") ||
                combined.Contains("unresponsive") || combined.Contains("freeze") ||
                combined.Contains("lag") || combined.Contains("fps drop") ||
                report.type == FeedbackType.Bug)
            {
                return Priority.P1;
            }
            
            // P2: polish/QoL
            return Priority.P2;
        }

        static int CountTotalReports()
        {
            int total = 0;
            foreach (var issue in _issues)
                total += issue.reportCount;
            return total;
        }

        // Public API
        public static List<FeedbackIssue> GetIssuesByPriority(Priority priority)
        {
            return _issues.Where(i => i.priority == priority).ToList();
        }

        public static List<FeedbackIssue> GetTopIssues(int count)
        {
            return _issues.OrderByDescending(i => i.priorityScore).Take(count).ToList();
        }
    }

    // Priority levels
    public enum Priority
    {
        P0,  // Critical — blocks progression (quest bugs, softlocks, crashes)
        P1,  // High — major annoyances (UI bugs, performance dips)
        P2   // Medium — polish/QoL (tooltips, settings, suggestions)
    }

    // Feedback issue (clustered from multiple reports)
    [Serializable]
    public class FeedbackIssue
    {
        public string title;
        public string description;
        public FeedbackType category;
        public Priority priority;
        public int reportCount;
        public DateTime firstReportDate;
        public DateTime lastReportDate;
        public string sceneName;
        public float priorityScore; // Calculated composite score

        public void CalculatePriorityScore()
        {
            // Priority score formula:
            // BaseScore + Frequency bonus + Recency bonus
            
            float baseScore = priority switch
            {
                Priority.P0 => 1000f,
                Priority.P1 => 500f,
                Priority.P2 => 100f,
                _ => 0f
            };
            
            // Frequency: +10 per report
            float frequencyBonus = reportCount * 10f;
            
            // Recency: +50 if seen in last 24 hours, +25 if last 7 days
            float daysSinceLastReport = (float)(DateTime.Now - lastReportDate).TotalDays;
            float recencyBonus = daysSinceLastReport switch
            {
                < 1f => 50f,
                < 7f => 25f,
                _ => 0f
            };
            
            priorityScore = baseScore + frequencyBonus + recencyBonus;
        }
    }

    // Parsed feedback report data
    class FeedbackReportData
    {
        public DateTime timestamp;
        public FeedbackType type;
        public string title;
        public string description;
        public string sceneName;
    }
}
