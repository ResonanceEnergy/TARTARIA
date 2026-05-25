using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Tartaria.Localization;

namespace Tartaria.Editor
{
    /// <summary>
    /// LocalizationExtractor — editor tool for extracting localizable strings from ScriptableObjects.
    /// 
    /// Features:
    /// - Scans all ScriptableObjects implementing ILocalizable
    /// - Extracts localization keys and fallback text
    /// - Generates CSV string tables by category (items, quests, dialogue, etc.)
    /// - Auto-updates ScriptableObjects to use generated keys
    /// - Validates all keys exist in string tables
    /// 
    /// Workflow:
    /// 1. Tools → Tartaria → Extract Localizable Strings → generates CSVs from existing data
    /// 2. Tools → Tartaria → Update ScriptableObject Keys → batch updates all assets
    /// 3. Tools → Tartaria → Validate Localization Keys → checks for missing keys
    /// 
    /// Output:
    /// - CSVs written to: Assets/_Project/Resources/Localization/{category}_en.csv
    /// - Format: key,en,es,fr,de,jp,cn,ru,pt
    /// - Placeholder columns for non-English languages (empty strings)
    /// </summary>
    public static class LocalizationExtractor
    {
        private const string OUTPUT_PATH = "Assets/_Project/Resources/Localization";
        private const string MENU_PATH = "Tools/Tartaria/Localization/";

        #region Menu Items

        [MenuItem(MENU_PATH + "Extract Localizable Strings", priority = 1)]
        public static void ExtractAllStrings()
        {
            Debug.Log("[LocalizationExtractor] Starting string extraction...");

            // Ensure output directory exists
            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
                AssetDatabase.Refresh();
            }

            // Extract strings by category
            var extractedByCategory = new Dictionary<string, Dictionary<string, string>>();

            foreach (string category in LocalizationCategory.All)
            {
                extractedByCategory[category] = new Dictionary<string, string>();
            }

            // Scan all ScriptableObjects
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            int processed = 0;
            int extracted = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (asset is ILocalizable localizable)
                {
                    processed++;
                    LocalizationKey[] keys = localizable.GetLocalizationKeys();

                    foreach (var key in keys)
                    {
                        if (!key.IsValid)
                            continue;

                        string fullPath = key.FullPath;
                        string fallbackText = localizable.GetFallbackText(key);

                        if (string.IsNullOrEmpty(fallbackText))
                            continue;

                        // Determine category from key
                        string category = GetCategoryFromKey(key);
                        if (category == null)
                        {
                            Debug.LogWarning($"[LocalizationExtractor] Unknown category for key: {fullPath}");
                            continue;
                        }

                        if (!extractedByCategory.ContainsKey(category))
                        {
                            extractedByCategory[category] = new Dictionary<string, string>();
                        }

                        // Add or update entry
                        extractedByCategory[category][fullPath] = fallbackText;
                        extracted++;
                    }
                }
            }

            // Write CSV files
            int filesWritten = 0;
            foreach (var kvp in extractedByCategory)
            {
                string category = kvp.Key;
                var entries = kvp.Value;

                if (entries.Count == 0)
                    continue;

                string csvPath = Path.Combine(OUTPUT_PATH, $"{category}_en.csv");
                WriteCSV(csvPath, entries);
                filesWritten++;

                Debug.Log($"[LocalizationExtractor] Wrote {entries.Count} keys to {category}_en.csv");
            }

            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationExtractor] ✅ Complete! Processed {processed} assets, extracted {extracted} keys into {filesWritten} files.");
            EditorUtility.DisplayDialog(
                "Localization Extraction Complete",
                $"Extracted {extracted} keys from {processed} assets into {filesWritten} CSV files.\n\nFiles written to: {OUTPUT_PATH}",
                "OK"
            );
        }

        [MenuItem(MENU_PATH + "Update ScriptableObject Keys", priority = 2)]
        public static void UpdateScriptableObjectKeys()
        {
            Debug.Log("[LocalizationExtractor] Updating ScriptableObject localization keys...");

            int updated = 0;
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (asset is ILocalizable)
                {
                    // Trigger OnValidate by marking asset dirty
                    EditorUtility.SetDirty(asset);
                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationExtractor] ✅ Updated {updated} ScriptableObject assets.");
            EditorUtility.DisplayDialog(
                "ScriptableObject Update Complete",
                $"Updated localization keys for {updated} assets.\n\nKeys auto-generated from IDs in OnValidate().",
                "OK"
            );
        }

        [MenuItem(MENU_PATH + "Validate Localization Keys", priority = 3)]
        public static void ValidateKeys()
        {
            Debug.Log("[LocalizationExtractor] Validating localization keys...");

            // Load all string tables
            var loadedKeys = new HashSet<string>();
            foreach (string category in LocalizationCategory.All)
            {
                string csvPath = Path.Combine(OUTPUT_PATH, $"{category}_en.csv");
                if (File.Exists(csvPath))
                {
                    string[] lines = File.ReadAllLines(csvPath);
                    for (int i = 1; i < lines.Length; i++) // Skip header
                    {
                        string line = lines[i].Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            continue;

                        string[] columns = line.Split(',');
                        if (columns.Length > 0)
                        {
                            loadedKeys.Add(columns[0].Trim());
                        }
                    }
                }
            }

            // Validate all ILocalizable assets
            var missingKeys = new List<string>();
            var validatedAssets = 0;

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (asset is ILocalizable localizable)
                {
                    validatedAssets++;
                    LocalizationKey[] keys = localizable.GetLocalizationKeys();

                    foreach (var key in keys)
                    {
                        if (!key.IsValid)
                            continue;

                        string fullPath = key.FullPath;
                        if (!loadedKeys.Contains(fullPath))
                        {
                            missingKeys.Add($"{asset.name}: {fullPath}");
                        }
                    }
                }
            }

            if (missingKeys.Count == 0)
            {
                Debug.Log($"[LocalizationExtractor] ✅ Validation passed! {validatedAssets} assets checked, all keys present in string tables.");
                EditorUtility.DisplayDialog(
                    "Validation Passed",
                    $"All {validatedAssets} assets have valid localization keys!",
                    "OK"
                );
            }
            else
            {
                Debug.LogWarning($"[LocalizationExtractor] ⚠️ Found {missingKeys.Count} missing keys:");
                foreach (string missing in missingKeys)
                {
                    Debug.LogWarning($"  - {missing}");
                }

                EditorUtility.DisplayDialog(
                    "Validation Warnings",
                    $"Found {missingKeys.Count} missing keys.\n\nSee Console for details.",
                    "OK"
                );
            }
        }

        [MenuItem(MENU_PATH + "Reload String Tables (Runtime)", priority = 10)]
        public static void ReloadStringTables()
        {
            if (Application.isPlaying && LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.ReloadStringTables();
                Debug.Log("[LocalizationExtractor] String tables reloaded in runtime.");
            }
            else
            {
                Debug.LogWarning("[LocalizationExtractor] Can only reload in Play Mode.");
            }
        }

        #endregion

        #region CSV Writing

        /// <summary>
        /// Write CSV file with header: key,en,es,fr,de,jp,cn,ru,pt
        /// Placeholder columns for non-English languages.
        /// </summary>
        private static void WriteCSV(string path, Dictionary<string, string> entries)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("key,en,es,fr,de,jp,cn,ru,pt");

            // Data rows (sorted by key)
            foreach (var kvp in entries.OrderBy(x => x.Key))
            {
                string key = kvp.Key;
                string text = EscapeCSV(kvp.Value);

                // English text + empty placeholders for other languages
                sb.AppendLine($"{key},{text},,,,,,");
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Escape CSV special characters (commas, quotes, newlines).
        /// </summary>
        private static string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            bool needsQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\n");

            if (needsQuotes)
            {
                value = value.Replace("\"", "\"\""); // Escape quotes
                return $"\"{value}\"";
            }

            return value;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Determine category from localization key path.
        /// Example: items.name.aether_shard → "items"
        /// </summary>
        private static string GetCategoryFromKey(LocalizationKey key)
        {
            string fullPath = key.FullPath;
            foreach (string category in LocalizationCategory.All)
            {
                if (fullPath.StartsWith(category + "."))
                    return category;
            }
            return null;
        }

        #endregion
    }
}
