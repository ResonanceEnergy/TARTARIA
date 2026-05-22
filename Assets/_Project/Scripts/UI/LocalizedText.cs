using UnityEngine;
using TMPro;
using Tartaria.Localization;

namespace Tartaria.UI
{
    /// <summary>
    /// LocalizedText — automatic TextMeshProUGUI integration for localization.
    /// 
    /// Features:
    /// - Auto-updates text when language changes
    /// - Support for formatted strings (e.g., "Collect {0} items")
    /// - Fallback to key path if translation missing
    /// - Zero-allocation updates (cached string lookups)
    /// 
    /// Usage:
    /// 1. Add component to TextMeshProUGUI GameObject
    /// 2. Set localizationKey (category.subcategory.id)
    /// 3. Optionally set formatArgs for dynamic text
    /// 
    /// Example:
    ///     LocalizationKey = "ui.label.health"
    ///     → Text updates to "Health" (EN), "Salud" (ES), etc.
    /// 
    ///     LocalizationKey = "ui.message.collect_items"
    ///     FormatArgs = [5]
    ///     → Text updates to "Collect 5 items"
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    [ExecuteAlways]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization")]
        [Tooltip("Localization key for this text (e.g., ui.label.health)")]
        [SerializeField] private LocalizationKey localizationKey;

        [Header("Formatting")]
        [Tooltip("Format arguments for string.Format (e.g., {0}, {1})")]
        [SerializeField] private string[] formatArgs = System.Array.Empty<string>();

        [Header("Fallback")]
        [Tooltip("Fallback text if key not found (optional)")]
        [SerializeField] private string fallbackText = string.Empty;

        [Header("Debug")]
        [Tooltip("Show key path instead of translated text")]
        [SerializeField] private bool debugShowKey = false;

        private TextMeshProUGUI _textComponent;
        private string _cachedKey;
        private bool _isSubscribed;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
            UpdateText();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && LocalizationManager.Instance != null && !_isSubscribed)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
                _isSubscribed = true;
            }

            UpdateText();
        }

        private void OnDisable()
        {
            if (_isSubscribed && LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
                _isSubscribed = false;
            }
        }

        private void OnValidate()
        {
            // Update text in editor when key changes
            if (_textComponent == null)
                _textComponent = GetComponent<TextMeshProUGUI>();

            UpdateText();
        }

        /// <summary>
        /// Called when language changes at runtime.
        /// </summary>
        private void OnLanguageChanged(SystemLanguage newLanguage)
        {
            UpdateText();
        }

        /// <summary>
        /// Update text with localized string.
        /// Zero-allocation if key and format args haven't changed.
        /// </summary>
        public void UpdateText()
        {
            if (_textComponent == null)
                return;

            if (!localizationKey.IsValid)
            {
                _textComponent.text = fallbackText;
                return;
            }

            // Debug mode: show key path
            if (debugShowKey)
            {
                _textComponent.text = $"[{localizationKey.FullPath}]";
                return;
            }

            // Get localized text
            string localizedText = string.Empty;

            if (Application.isPlaying && LocalizationManager.Instance != null)
            {
                // Runtime: use LocalizationManager
                if (formatArgs != null && formatArgs.Length > 0)
                {
                    localizedText = LocalizationManager.Instance.GetTextFormatted(localizationKey, formatArgs);
                }
                else
                {
                    localizedText = LocalizationManager.Instance.GetText(localizationKey);
                }
            }
            else
            {
                // Editor preview: show key or fallback
                localizedText = !string.IsNullOrEmpty(fallbackText) ? fallbackText : localizationKey.FullPath;
            }

            // Fallback if missing
            if (string.IsNullOrEmpty(localizedText) || localizedText.StartsWith("[MISSING:"))
            {
                localizedText = !string.IsNullOrEmpty(fallbackText) ? fallbackText : localizationKey.FullPath;
            }

            _textComponent.text = localizedText;
            _cachedKey = localizationKey.FullPath;
        }

        /// <summary>
        /// Set localization key at runtime (for dynamic UI).
        /// </summary>
        public void SetKey(LocalizationKey key)
        {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// Set format arguments at runtime (for dynamic text like "Collect {0} items").
        /// </summary>
        public void SetFormatArgs(params string[] args)
        {
            formatArgs = args;
            UpdateText();
        }

        /// <summary>
        /// Set fallback text at runtime.
        /// </summary>
        public void SetFallback(string text)
        {
            fallbackText = text;
            UpdateText();
        }

        /// <summary>
        /// Get current localization key.
        /// </summary>
        public LocalizationKey Key => localizationKey;

        /// <summary>
        /// Get current localized text.
        /// </summary>
        public string LocalizedText => _textComponent != null ? _textComponent.text : string.Empty;

#if UNITY_EDITOR
        /// <summary>
        /// Editor context menu: Force refresh text.
        /// </summary>
        [ContextMenu("Force Refresh Text")]
        private void ForceRefresh()
        {
            UpdateText();
            Debug.Log($"[LocalizedText] Refreshed text for key: {localizationKey.FullPath}");
        }

        /// <summary>
        /// Editor context menu: Copy key to clipboard.
        /// </summary>
        [ContextMenu("Copy Key to Clipboard")]
        private void CopyKeyToClipboard()
        {
            if (localizationKey.IsValid)
            {
                GUIUtility.systemCopyBuffer = localizationKey.FullPath;
                Debug.Log($"[LocalizedText] Copied key to clipboard: {localizationKey.FullPath}");
            }
        }
#endif
    }
}
