using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    /// <summary>
    /// Choice Dialog UI - presents multiple choice options to player with callbacks.
    /// Used for critical story moments, especially the final ending choice in Moon 13.
    /// Auto-builds UI at runtime, no scene wiring required.
    /// </summary>
    public class ChoiceDialogUI : MonoBehaviour
    {
        static ChoiceDialogUI _instance;
        public static ChoiceDialogUI Instance => _instance;

        [Header("Settings")]
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] Color backgroundColor = new(0f, 0f, 0f, 0.9f);
        [SerializeField] Color buttonNormalColor = new(0.3f, 0.3f, 0.4f, 1f);
        [SerializeField] Color buttonHighlightColor = new(0.5f, 0.7f, 1f, 1f);
        [SerializeField] Color buttonPressedColor = new(0.2f, 0.4f, 0.8f, 1f);

        Canvas _canvas;
        CanvasGroup _canvasGroup;
        GameObject _dialogPanel;
        TMP_Text _titleText;
        TMP_Text _descriptionText;
        Transform _buttonContainer;

        readonly List<Button> _buttons = new();
        Action<int> _currentCallback;
        bool _isShowing;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildUI();
            HideImmediate();
        }

        void BuildUI()
        {
            // Root canvas
            var canvasGo = new GameObject("ChoiceDialogCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10000; // Above everything
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            _canvasGroup = canvasGo.AddComponent<CanvasGroup>();

            // Background overlay
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = backgroundColor;
            var bgRt = bgImage.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Dialog panel
            _dialogPanel = new GameObject("DialogPanel");
            _dialogPanel.transform.SetParent(canvasGo.transform, false);
            var panelImage = _dialogPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            var panelRt = panelImage.rectTransform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(1200, 800);
            panelRt.anchoredPosition = Vector2.zero;

            // Title text
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(_dialogPanel.transform, false);
            _titleText = titleGo.AddComponent<TextMeshProUGUI>();
            _titleText.fontSize = 72;
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.color = new Color(1f, 0.9f, 0.5f); // Aether gold
            var titleRt = _titleText.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 0.8f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.offsetMin = new Vector2(50, -50);
            titleRt.offsetMax = new Vector2(-50, -50);

            // Description text
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(_dialogPanel.transform, false);
            _descriptionText = descGo.AddComponent<TextMeshProUGUI>();
            _descriptionText.fontSize = 36;
            _descriptionText.alignment = TextAlignmentOptions.Center;
            _descriptionText.color = Color.white;
            var descRt = _descriptionText.rectTransform;
            descRt.anchorMin = new Vector2(0f, 0.5f);
            descRt.anchorMax = new Vector2(1f, 0.75f);
            descRt.offsetMin = new Vector2(80, 0);
            descRt.offsetMax = new Vector2(-80, 0);

            // Button container
            var containerGo = new GameObject("ButtonContainer");
            containerGo.transform.SetParent(_dialogPanel.transform, false);
            _buttonContainer = containerGo.transform;
            var containerRt = containerGo.AddComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0f, 0f);
            containerRt.anchorMax = new Vector2(1f, 0.45f);
            containerRt.offsetMin = new Vector2(100, 100);
            containerRt.offsetMax = new Vector2(-100, -50);

            var layoutGroup = containerGo.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 30;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
        }

        /// <summary>
        /// Show choice dialog with title, description, and choice buttons
        /// </summary>
        public void ShowChoices(string[] choices, Action<int> onChoiceSelected, string title = "Make Your Choice", string description = "")
        {
            if (_isShowing)
            {
                Debug.LogWarning("[ChoiceDialogUI] Already showing a dialog");
                return;
            }

            _isShowing = true;
            _currentCallback = onChoiceSelected;

            // Set texts
            _titleText.text = title;
            _descriptionText.text = description;

            // Clear existing buttons
            ClearButtons();

            // Create buttons for each choice
            for (int i = 0; i < choices.Length; i++)
            {
                CreateChoiceButton(choices[i], i);
            }

            // Show with fade
            StartCoroutine(FadeIn());

            Debug.Log($"[ChoiceDialogUI] Showing {choices.Length} choices: {string.Join(", ", choices)}");
        }

        void CreateChoiceButton(string text, int index)
        {
            var buttonGo = new GameObject($"Button_{index}");
            buttonGo.transform.SetParent(_buttonContainer, false);

            // Button component
            var button = buttonGo.AddComponent<Button>();
            button.onClick.AddListener(() => OnChoiceClicked(index));

            // Button image
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = buttonNormalColor;

            // Button colors
            var colors = button.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHighlightColor;
            colors.pressedColor = buttonPressedColor;
            colors.selectedColor = buttonHighlightColor;
            button.colors = colors;

            // Rect transform
            var rt = buttonGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 100);

            // Button text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 42;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRt = tmp.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20, 10);
            textRt.offsetMax = new Vector2(-20, -10);

            _buttons.Add(button);
        }

        void OnChoiceClicked(int index)
        {
            Debug.Log($"[ChoiceDialogUI] Choice {index} selected");

            // Hide dialog
            StartCoroutine(FadeOut(() =>
            {
                // Invoke callback
                _currentCallback?.Invoke(index);
                _currentCallback = null;
            }));
        }

        void ClearButtons()
        {
            foreach (var button in _buttons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _buttons.Clear();
        }

        void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvas.enabled = false;
            _isShowing = false;
        }

        System.Collections.IEnumerator FadeIn()
        {
            _canvas.enabled = true;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        System.Collections.IEnumerator FadeOut(Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvas.enabled = false;
            _isShowing = false;

            onComplete?.Invoke();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
