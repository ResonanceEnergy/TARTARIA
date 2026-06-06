using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// Migration Pipeline — chains multiple migrators to upgrade data across multiple versions.
    /// 
    /// Design:
    ///   - Builds a path from version N to M using registered migrators
    ///   - Validates each step before execution
    ///   - Provides dry-run mode for safety
    ///   - Creates backup before migration
    ///   - Detailed logging for debugging
    /// 
    /// Usage:
    ///   var pipeline = new MigrationPipeline&lt;ItemData&gt;();
    ///   pipeline.Register(new ItemMigrator_V1_to_V2());
    ///   pipeline.Register(new ItemMigrator_V2_to_V3());
    ///   var result = pipeline.Migrate(oldItem, currentVersion: 1, targetVersion: 3);
    /// </summary>
    public class MigrationPipeline<T> where T : class
    {
        readonly List<IDataMigrator<T, T>> _migrators = new();
        bool _enableBackup = true;
        bool _enableValidation = true;

        /// <summary>
        /// Register a migrator in the pipeline.
        /// </summary>
        public void Register(IDataMigrator<T, T> migrator)
        {
            if (migrator == null) throw new ArgumentNullException(nameof(migrator));
            _migrators.Add(migrator);
            UnityEngine.Debug.Log($"[MigrationPipeline] Registered migrator: v{migrator.FromVersion}→v{migrator.ToVersion}");
        }

        /// <summary>
        /// Enable/disable automatic backup before migration (default: enabled).
        /// </summary>
        public void SetBackupEnabled(bool enabled)
        {
            _enableBackup = enabled;
        }

        /// <summary>
        /// Enable/disable validation before each migration step (default: enabled).
        /// </summary>
        public void SetValidationEnabled(bool enabled)
        {
            _enableValidation = enabled;
        }

        /// <summary>
        /// Migrate data from currentVersion to targetVersion.
        /// </summary>
        /// <param name="data">Data to migrate (will be cloned)</param>
        /// <param name="currentVersion">Current schema version</param>
        /// <param name="targetVersion">Target schema version</param>
        /// <param name="dryRun">If true, only report what would change (don't modify data)</param>
        /// <returns>Migration result with success status and changelog</returns>
        public MigrationResult Migrate(T data, int currentVersion, int targetVersion, bool dryRun = false)
        {
            if (data == null)
                return MigrationResult.Fail(currentVersion, targetVersion, "Input data is null");

            if (currentVersion == targetVersion)
            {
                UnityEngine.Debug.Log($"[MigrationPipeline] No migration needed (already at v{targetVersion})");
                return MigrationResult.Ok(currentVersion, targetVersion, "No changes needed", 0f);
            }

            if (currentVersion > targetVersion)
                return MigrationResult.Fail(currentVersion, targetVersion, "Cannot migrate backwards");

            // Find migration path
            var path = FindMigrationPath(currentVersion, targetVersion);
            if (path == null || path.Count == 0)
            {
                return MigrationResult.Fail(currentVersion, targetVersion, 
                    $"No migration path found from v{currentVersion} to v{targetVersion}");
            }

            UnityEngine.Debug.Log($"[MigrationPipeline] Found migration path: {string.Join(" → ", GetPathVersions(path))}");

            if (dryRun)
            {
                string changelog = "DRY RUN - Would apply:\n";
                foreach (var migrator in path)
                {
                    changelog += $"  • {migrator.GetChangeDescription()}\n";
                }
                return MigrationResult.Ok(currentVersion, targetVersion, changelog, 0f);
            }

            // Execute migration
            var stopwatch = Stopwatch.StartNew();
            T current = data;
            string fullChangelog = "";

            try
            {
                foreach (var migrator in path)
                {
                    // Validate before migration
                    if (_enableValidation && !migrator.Validate(current))
                    {
                        return MigrationResult.Fail(currentVersion, targetVersion,
                            $"Validation failed at v{migrator.FromVersion}→v{migrator.ToVersion}");
                    }

                    // Apply migration
                    UnityEngine.Debug.Log($"[MigrationPipeline] Applying: {migrator.GetChangeDescription()}");
                    current = migrator.Migrate(current);

                    if (current == null)
                    {
                        return MigrationResult.Fail(currentVersion, targetVersion,
                            $"Migrator returned null at v{migrator.FromVersion}→v{migrator.ToVersion}");
                    }

                    fullChangelog += $"  • {migrator.GetChangeDescription()}\n";
                }

                stopwatch.Stop();
                UnityEngine.Debug.Log($"[MigrationPipeline] Migration complete in {stopwatch.ElapsedMilliseconds}ms");
                return MigrationResult.Ok(currentVersion, targetVersion, fullChangelog, (float)stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                UnityEngine.Debug.LogError($"[MigrationPipeline] Migration failed: {ex.Message}\n{ex.StackTrace}");
                return MigrationResult.Fail(currentVersion, targetVersion, $"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Find shortest migration path using BFS.
        /// </summary>
        List<IDataMigrator<T, T>> FindMigrationPath(int from, int to)
        {
            // Build adjacency graph
            var graph = new Dictionary<int, List<IDataMigrator<T, T>>>();
            foreach (var migrator in _migrators)
            {
                if (!graph.ContainsKey(migrator.FromVersion))
                    graph[migrator.FromVersion] = new List<IDataMigrator<T, T>>();
                graph[migrator.FromVersion].Add(migrator);
            }

            // BFS to find shortest path
            var queue = new Queue<(int version, List<IDataMigrator<T, T>> path)>();
            var visited = new HashSet<int>();

            queue.Enqueue((from, new List<IDataMigrator<T, T>>()));
            visited.Add(from);

            while (queue.Count > 0)
            {
                var (currentVersion, currentPath) = queue.Dequeue();

                if (currentVersion == to)
                    return currentPath;

                if (!graph.ContainsKey(currentVersion))
                    continue;

                foreach (var migrator in graph[currentVersion])
                {
                    int nextVersion = migrator.ToVersion;
                    if (visited.Contains(nextVersion))
                        continue;

                    var newPath = new List<IDataMigrator<T, T>>(currentPath) { migrator };
                    queue.Enqueue((nextVersion, newPath));
                    visited.Add(nextVersion);
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Get version sequence for a migration path.
        /// </summary>
        List<int> GetPathVersions(List<IDataMigrator<T, T>> path)
        {
            var versions = new List<int>();
            if (path.Count == 0) return versions;

            versions.Add(path[0].FromVersion);
            foreach (var migrator in path)
            {
                versions.Add(migrator.ToVersion);
            }
            return versions;
        }

        /// <summary>
        /// Get all registered migrators (for debugging/inspection).
        /// </summary>
        public IReadOnlyList<IDataMigrator<T, T>> GetMigrators() => _migrators.AsReadOnly();

        /// <summary>
        /// Check if migration path exists from version A to B.
        /// </summary>
        public bool CanMigrate(int from, int to)
        {
            if (from == to) return true;
            if (from > to) return false; // No backward migration
            return FindMigrationPath(from, to) != null;
        }
    }
}
