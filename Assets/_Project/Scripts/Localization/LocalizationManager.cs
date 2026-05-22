using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Localization
{
    /// <summary>
    /// LocalizationManager — singleton managing all translated strings.
    /// 
    /// Features:
    /// - CSV/JSON string table loading from Resources
    /// - Language switching at runtime
    /// - Zero-allocation GetText() via cached dictionaries
    /// - Fallback to English for missing translations
    /// - Editor tools integration for string extraction
    /// 
    /// Architecture:
    /// - String tables stored in Resources/Localization/{category}_{lang}.csv
    /// - Format: key,en,es,fr,de,jp,cn,ru,pt
    /// - Separate tables per category (items, quests, dialogue, etc.)
    /// - Dictionary-based lookup: O(1) access time
    /// 
    /// Usage:
    ///     LocalizationManager.Instance.SetLanguage(SystemLanguage.Spanish);
    ///     string text = LocalizationManager.Instance.GetText(localizationKey);
    /// 
    /// CSV Format Example:
    ///     key,en,es,fr,de,jp,cn,ru,pt
    ///     aether_shard,Aether Shard,Fragmento de Éter,Éclat d'Éther,...
    ///     aether_shard_desc,"Crystallized resonance...","Resonancia cristalizada...",...
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LocalizationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[LocalizationManager]");
                        _instance = go.AddComponent<LocalizationManager>();
                        DontDestroyOnLoad(go);
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [Tooltip("Current language (runtime switchable)")]
        [SerializeField] private SystemLanguage currentLanguage = SystemLanguage.English;

        [Tooltip("Languages to load on startup")]
        [SerializeField] private SystemLanguage[] supportedLanguages = new[]
        {
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.French,
            SystemLanguage.German,
            SystemLanguage.Japanese,
            SystemLanguage.Chinese,
            SystemLanguage.Russian,
            SystemLanguage.Portuguese
        };

        [Header("Paths")]
        [Tooltip("Base path in Resources folder for string tables")]
        [SerializeField] private string stringTablePath = "Localization";

        [Header("Debug")]
        [Tooltip("Log missing key warnings")]
        [SerializeField] private bool logMissingKeys = true;

        [Tooltip("Show key paths instead of translated text (debug mode)")]
        [SerializeField] private bool debugShowKeys = false;

        // String table cache: [language][fullKeyPath] = text
        private Dictionary<SystemLanguage, Dictionary<string, string>> _stringTables = new();

        // Missing key cache to avoid spam
        private HashSet<string> _missingKeysLogged = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        /// <summary>
        /// Initialize localization system — load string tables for current language.
        /// </summary>
        private void Initialize()
        {
            // Auto-detect system language if supported
            if (Array.Exists(supportedLanguages, lang => lang == Application.systemLanguage))
            {
                currentLanguage = Application.systemLanguage;
            }
            else
            {
                currentLanguage = SystemLanguage.English; // Fallback
            }

            LoadStringTables(currentLanguage);

            Debug.Log($"[LocalizationManager] Initialized with language: {currentLanguage}");
        }

        /// <summary>
        /// Switch to a different language at runtime.
        /// Reloads all string tables and triggers UI refresh events.
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            if (currentLanguage == language)
                return;

            if (!Array.Exists(supportedLanguages, lang => lang == language))
            {
                Debug.LogWarning($"[LocalizationManager] Language {language} not supported. Using English.");
                language = SystemLanguage.English;
            }

            currentLanguage = language;
            LoadStringTables(language);

            // Trigger UI refresh event
            OnLanguageChanged?.Invoke(language);

            Debug.Log($"[LocalizationManager] Language changed to: {language}");
        }

        /// <summary>
        /// Event fired when language changes (for UI refresh).
        /// </summary>
        public event Action<SystemLanguage> OnLanguageChanged;

        /// <summary>
        /// Get translated text for a localization key.
        /// Falls back to English if translation missing, then to empty string.
        /// Zero-allocation dictionary lookup.
        /// </summary>
        public string GetText(LocalizationKey key)
        {
            if (!key.IsValid)
                return string.Empty;

            if (debugShowKeys)
                return $"[{key.FullPath}]";

            string fullPath = key.FullPath;

            // Try current language
            if (_stringTables.TryGetValue(currentLanguage, out var currentTable))
            {
                if (currentTable.TryGetValue(fullPath, out string text))
                    return text;
            }

            // Fallback to English
            if (currentLanguage != SystemLanguage.English)
            {
                if (_stringTables.TryGetValue(SystemLanguage.English, out var englishTable))
                {
                    if (englishTable.TryGetValue(fullPath, out string text))
                    {
                        if (logMissingKeys && !_missingKeysLogged.Contains(fullPath))
                        {
                            Debug.LogWarning($"[LocalizationManager] Missing {currentLanguage} translation for key: {fullPath}. Using English fallback.");
                            _missingKeysLogged.Add(fullPath);
                        }
                        return text;
                    }
                }
            }

            // Missing key
            if (logMissingKeys && !_missingKeysLogged.Contains(fullPath))
            {
                Debug.LogWarning($"[LocalizationManager] Missing localization key: {fullPath}");
                _missingKeysLogged.Add(fullPath);
            }

            return $"[MISSING: {fullPath}]";
        }

        /// <summary>
        /// Get translated text with format args (e.g., "Collect {0} items").
        /// </summary>
        public string GetTextFormatted(LocalizationKey key, params object[] args)
        {
            string text = GetText(key);
            if (string.IsNullOrEmpty(text) || args == null || args.Length == 0)
                return text;

            try
            {
                return string.Format(text, args);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"[LocalizationManager] Format error for key {key.FullPath}: {ex.Message}");
                return text;
            }
        }

        /// <summary>
        /// Load string tables for a specific language from Resources.
        /// Loads all category tables (items, quests, dialogue, etc.).
        /// </summary>
        private void LoadStringTables(SystemLanguage language)
        {
            if (!_stringTables.ContainsKey(language))
            {
                _stringTables[language] = new Dictionary<string, string>();
            }
            else
            {
                _stringTables[language].Clear();
            }

            var table = _stringTables[language];
            string langCode = GetLanguageCode(language);

            // Load each category table
            foreach (string category in LocalizationCategory.All)
            {
                string tableName = $"{category}_{langCode}";
                string resourcePath = $"{stringTablePath}/{tableName}";

                TextAsset csvAsset = Resources.Load<TextAsset>(resourcePath);
                if (csvAsset == null)
                {
                    Debug.LogWarning($"[LocalizationManager] String table not found: {resourcePath}");
                    continue;
                }

                ParseCSV(csvAsset.text, category, table);
                Debug.Log($"[LocalizationManager] Loaded {tableName}: {table.Count} keys");
            }
        }

        /// <summary>
        /// Parse CSV string table into dictionary.
        /// Format: key,en,es,fr,de,jp,cn,ru,pt
        /// </summary>
        private void ParseCSV(string csvText, string category, Dictionary<string, string> table)
        {
            if (string.IsNullOrEmpty(csvText))
                return;

            string[] lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
                return;

            // Parse header to find language column index
            string[] header = ParseCSVLine(lines[0]);
            int langIndex = GetLanguageColumnIndex(header, currentLanguage);
            if (langIndex < 0)
            {
                Debug.LogWarning($"[LocalizationManager] Language column not found in {category} table for {currentLanguage}");
                return;
            }

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = ParseCSVLine(lines[i]);
                if (columns.Length < 2)
                    continue;

                string key = columns[0].Trim();
                if (string.IsNullOrEmpty(key) || key.StartsWith("#")) // Skip comments
                    continue;

                // Build full path: category.subcategory.key
                string fullPath = key.Contains(".") ? key : $"{category}.{key}";

                if (langIndex < columns.Length)
                {
                    string text = columns[langIndex].Trim();
                    table[fullPath] = text;
                }
            }
        }

        /// <summary>
        /// Parse a single CSV line, handling quoted fields with commas.
        /// </summary>
        private string[] ParseCSVLine(string line)
        {
            List<string> fields = new();
            bool inQuotes = false;
            int fieldStart = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(line.Substring(fieldStart, i - fieldStart).Trim('"'));
                    fieldStart = i + 1;
                }
            }

            // Add last field
            fields.Add(line.Substring(fieldStart).Trim('"'));

            return fields.ToArray();
        }

        /// <summary>
        /// Get column index for a language in CSV header.
        /// </summary>
        private int GetLanguageColumnIndex(string[] header, SystemLanguage language)
        {
            string langCode = GetLanguageCode(language);
            for (int i = 1; i < header.Length; i++)
            {
                if (header[i].Trim().Equals(langCode, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Convert SystemLanguage to 2-letter code.
        /// </summary>
        private string GetLanguageCode(SystemLanguage language)
        {
            return language switch
            {
                SystemLanguage.English => "en",
                SystemLanguage.Spanish => "es",
                SystemLanguage.French => "fr",
                SystemLanguage.German => "de",
                SystemLanguage.Japanese => "jp",
                SystemLanguage.Chinese => "cn",
                SystemLanguage.ChineseSimplified => "cn",
                SystemLanguage.ChineseTraditional => "tw",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Portuguese => "pt",
                _ => "en"
            };
        }

        /// <summary>
        /// Current language property (read/write).
        /// </summary>
        public SystemLanguage CurrentLanguage => currentLanguage;

        /// <summary>
        /// Check if a key exists in the current language table.
        /// </summary>
        public bool HasKey(LocalizationKey key)
        {
            if (!key.IsValid)
                return false;

            string fullPath = key.FullPath;

            if (_stringTables.TryGetValue(currentLanguage, out var table))
            {
                return table.ContainsKey(fullPath);
            }

            return false;
        }

        /// <summary>
        /// Get all keys in a category (for editor tools).
        /// </summary>
        public string[] GetKeysInCategory(string category)
        {
            if (!_stringTables.TryGetValue(currentLanguage, out var table))
                return Array.Empty<string>();

            List<string> keys = new();
            foreach (string key in table.Keys)
            {
                if (key.StartsWith(category + "."))
                    keys.Add(key);
            }

            return keys.ToArray();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Reload all string tables (for live editing).
        /// </summary>
        public void ReloadStringTables()
        {
            _stringTables.Clear();
            _missingKeysLogged.Clear();
            LoadStringTables(currentLanguage);
            Debug.Log($"[LocalizationManager] String tables reloaded for {currentLanguage}");
        }

        /// <summary>
        /// Editor-only: Export all localization keys from scene objects implementing ILocalizable.
        /// Used by string extraction tool.
        /// </summary>
        public Dictionary<string, string> ExtractAllKeys()
        {
            Dictionary<string, string> extractedKeys = new();

            // Find all ScriptableObjects implementing ILocalizable
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

                if (asset is ILocalizable localizable)
                {
                    LocalizationKey[] keys = localizable.GetLocalizationKeys();
                    foreach (var key in keys)
                    {
                        if (key.IsValid && !extractedKeys.ContainsKey(key.FullPath))
                        {
                            string fallback = localizable.GetFallbackText(key);
                            extractedKeys[key.FullPath] = fallback ?? string.Empty;
                        }
                    }
                }
            }

            return extractedKeys;
        }
#endif
    }
}
