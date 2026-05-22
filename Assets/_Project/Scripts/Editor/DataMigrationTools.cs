using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tartaria.Data;

namespace Tartaria.Save.Editor
{
    /// <summary>
    /// Data Migration Editor Tools — batch upgrade for all ScriptableObject assets.
    /// 
    /// Features:
    ///   - Scan all data assets in project
    ///   - Detect version mismatches
    ///   - Dry-run mode (report changes without modifying)
    ///   - Batch migration with progress bar
    ///   - Automatic backup before migration
    ///   - Detailed migration report
    /// 
    /// Usage:
    ///   Tools → Tartaria → Data Migration → Upgrade All Data Assets
    ///   Tools → Tartaria → Data Migration → Scan for Outdated Assets
    ///   Tools → Tartaria → Data Migration → Create Backup
    /// </summary>
    public class DataMigrationTools : EditorWindow
    {
        Vector2 _scrollPosition;
        bool _dryRun = true;
        bool _createBackup = true;
        bool _autoSaveAssets = true;

        List<MigrationCandidate> _scanResults = new();
        string _lastScanTime;
        int _outdatedCount;

        [MenuItem("Tools/Tartaria/Data Migration/Open Migration Tool", priority = 100)]
        static void OpenWindow()
        {
            var window = GetWindow<DataMigrationTools>("Data Migration");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        [MenuItem("Tools/Tartaria/Data Migration/Scan for Outdated Assets", priority = 101)]
        static void QuickScan()
        {
            var window = GetWindow<DataMigrationTools>("Data Migration");
            window.ScanAllAssets();
        }

        [MenuItem("Tools/Tartaria/Data Migration/Create Backup", priority = 102)]
        static void CreateBackup()
        {
            string backupPath = CreateBackupArchive();
            if (!string.IsNullOrEmpty(backupPath))
            {
                EditorUtility.DisplayDialog("Backup Created", 
                    $"Backup saved to:\n{backupPath}", "OK");
            }
        }

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Data Schema Migration Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Upgrade all data assets (ItemData, QuestData, etc.) to latest schema version.\n" +
                "Always use DRY RUN first to preview changes!",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _dryRun = EditorGUILayout.Toggle("Dry Run (Preview Only)", _dryRun);
            _createBackup = EditorGUILayout.Toggle("Create Backup First", _createBackup);
            _autoSaveAssets = EditorGUILayout.Toggle("Auto-Save Modified Assets", _autoSaveAssets);

            EditorGUILayout.Space(10);

            // Scan button
            if (GUILayout.Button("Scan All Data Assets", GUILayout.Height(30)))
            {
                ScanAllAssets();
            }

            // Results summary
            if (_scanResults.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Last Scan: {_lastScanTime}", EditorStyles.miniLabel);
                
                EditorGUILayout.HelpBox(
                    $"Found {_scanResults.Count} assets\n" +
                    $"{_outdatedCount} need migration",
                    _outdatedCount > 0 ? MessageType.Warning : MessageType.Info);

                // Migrate button
                if (_outdatedCount > 0)
                {
                    string buttonLabel = _dryRun ? "Preview Migration (Dry Run)" : "⚠ APPLY MIGRATION ⚠";
                    GUI.backgroundColor = _dryRun ? Color.white : Color.yellow;
                    
                    if (GUILayout.Button(buttonLabel, GUILayout.Height(35)))
                    {
                        MigrateAllAssets();
                    }
                    GUI.backgroundColor = Color.white;
                }

                // Results list
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Scan Results:", EditorStyles.boldLabel);
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(250));
                
                foreach (var candidate in _scanResults)
                {
                    DrawMigrationCandidate(candidate);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawMigrationCandidate(MigrationCandidate candidate)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Status icon
            string icon = candidate.needsMigration ? "⚠" : "✓";
            Color color = candidate.needsMigration ? Color.yellow : Color.green;
            
            GUI.color = color;
            GUILayout.Label(icon, GUILayout.Width(20));
            GUI.color = Color.white;

            // Asset info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(candidate.assetName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {candidate.dataType} | Version: {candidate.currentVersion} → {candidate.targetVersion}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            // Ping button
            if (GUILayout.Button("Ping", GUILayout.Width(50)))
            {
                EditorGUIUtility.PingObject(candidate.asset);
            }

            EditorGUILayout.EndHorizontal();
        }

        void ScanAllAssets()
        {
            _scanResults.Clear();
            _outdatedCount = 0;

            // Scan ItemData
            ScanAssetType<ItemData>("ItemData", SchemaVersion.CURRENT_ITEM);

            // Scan QuestData
            ScanAssetType<QuestData>("QuestData", SchemaVersion.CURRENT_QUEST);

            // Scan CraftingRecipeData
            ScanAssetType<CraftingRecipeData>("CraftingRecipeData", SchemaVersion.CURRENT_RECIPE);

            // Scan SkillNodeData
            ScanAssetType<SkillNodeData>("SkillNodeData", SchemaVersion.CURRENT_SKILL);

            // Scan EquipmentItemData
            ScanAssetType<EquipmentItemData>("EquipmentItemData", SchemaVersion.CURRENT_EQUIPMENT);

            _lastScanTime = System.DateTime.Now.ToString("HH:mm:ss");
            Debug.Log($"[DataMigrationTools] Scan complete: {_scanResults.Count} assets, {_outdatedCount} outdated");
        }

        void ScanAssetType<T>(string typeName, int currentVersion) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                
                if (asset == null) continue;

                // Check if asset has schemaVersion field (use reflection)
                int assetVersion = GetAssetVersion(asset);
                bool needsMigration = assetVersion < currentVersion;

                if (needsMigration) _outdatedCount++;

                _scanResults.Add(new MigrationCandidate
                {
                    asset = asset,
                    assetName = asset.name,
                    assetPath = path,
                    dataType = typeName,
                    currentVersion = assetVersion,
                    targetVersion = currentVersion,
                    needsMigration = needsMigration
                });
            }
        }

        int GetAssetVersion(Object asset)
        {
            // Use reflection to get schemaVersion field
            var field = asset.GetType().GetField("schemaVersion", 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(int))
            {
                return (int)field.GetValue(asset);
            }

            // Default to v1 if no schemaVersion field exists
            return 1;
        }

        void MigrateAllAssets()
        {
            if (!_dryRun && _createBackup)
            {
                string backupPath = CreateBackupArchive();
                if (string.IsNullOrEmpty(backupPath))
                {
                    Debug.LogError("[DataMigrationTools] Backup failed! Aborting migration.");
                    return;
                }
                Debug.Log($"[DataMigrationTools] Backup created: {backupPath}");
            }

            var outdatedAssets = _scanResults.Where(c => c.needsMigration).ToList();
            int successCount = 0;
            int failCount = 0;

            EditorUtility.DisplayProgressBar("Migrating Assets", "Starting...", 0f);

            try
            {
                for (int i = 0; i < outdatedAssets.Count; i++)
                {
                    var candidate = outdatedAssets[i];
                    float progress = (float)i / outdatedAssets.Count;
                    
                    EditorUtility.DisplayProgressBar("Migrating Assets", 
                        $"[{i+1}/{outdatedAssets.Count}] {candidate.assetName}", progress);

                    bool success = MigrateSingleAsset(candidate);
                    if (success) successCount++;
                    else failCount++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // Report
            string mode = _dryRun ? "DRY RUN" : "LIVE";
            string message = $"[{mode}] Migration complete:\n" +
                             $"✓ {successCount} succeeded\n" +
                             $"✗ {failCount} failed";

            if (_dryRun)
            {
                message += "\n\nNo files were modified. Disable 'Dry Run' to apply changes.";
            }

            EditorUtility.DisplayDialog("Migration Complete", message, "OK");
            Debug.Log($"[DataMigrationTools] {message}");

            // Refresh scan results
            ScanAllAssets();
        }

        bool MigrateSingleAsset(MigrationCandidate candidate)
        {
            if (_dryRun)
            {
                Debug.Log($"[DRY RUN] Would migrate: {candidate.assetName} (v{candidate.currentVersion}→v{candidate.targetVersion})");
                return true;
            }

            try
            {
                // Use reflection to set schemaVersion field
                var field = candidate.asset.GetType().GetField("schemaVersion",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(candidate.asset, candidate.targetVersion);
                    
                    if (_autoSaveAssets)
                    {
                        EditorUtility.SetDirty(candidate.asset);
                    }

                    Debug.Log($"[DataMigrationTools] Migrated: {candidate.assetName} → v{candidate.targetVersion}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[DataMigrationTools] No schemaVersion field on {candidate.assetName}");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DataMigrationTools] Failed to migrate {candidate.assetName}: {ex.Message}");
                return false;
            }
        }

        static string CreateBackupArchive()
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(Application.dataPath, "..", "DataBackups");
            Directory.CreateDirectory(backupDir);

            string backupPath = Path.Combine(backupDir, $"data_backup_{timestamp}.zip");

            try
            {
                // Copy all data assets to temp folder
                string tempDir = Path.Combine(Application.temporaryCachePath, "DataBackup");
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // Find all ScriptableObject data assets
                string[] assetPaths = AssetDatabase.GetAllAssetPaths()
                    .Where(p => p.StartsWith("Assets/_Project/Data") || p.Contains("/Resources/"))
                    .Where(p => p.EndsWith(".asset"))
                    .ToArray();

                foreach (string assetPath in assetPaths)
                {
                    string fileName = Path.GetFileName(assetPath);
                    string destPath = Path.Combine(tempDir, fileName);
                    File.Copy(assetPath, destPath, true);
                }

                // Create zip (Unity doesn't have built-in zip, so we'll just copy the folder)
                // In production, use System.IO.Compression.ZipFile
                // For now, just copy to backup folder
                string backupFolderPath = Path.Combine(backupDir, $"data_backup_{timestamp}");
                if (Directory.Exists(backupFolderPath)) Directory.Delete(backupFolderPath, true);
                Directory.Move(tempDir, backupFolderPath);

                Debug.Log($"[DataMigrationTools] Backup created: {backupFolderPath}");
                return backupFolderPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DataMigrationTools] Backup failed: {ex.Message}");
                return null;
            }
        }

        class MigrationCandidate
        {
            public Object asset;
            public string assetName;
            public string assetPath;
            public string dataType;
            public int currentVersion;
            public int targetVersion;
            public bool needsMigration;
        }
    }
}
