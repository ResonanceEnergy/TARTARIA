using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1InteractionPrompt — fade-in/out screen-space prompt driven by
    /// <see cref="Tartaria.Core.GameEvents.OnHUDShowInteractionPrompt"/> /
    /// <see cref="Tartaria.Core.GameEvents.OnHUDHideInteractionPrompt"/>.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → UI Programmer (interaction-prompt-polish).
    ///
    /// Auto-bootstrap (no scene wiring): a singleton GameObject builds its own
    /// ScreenSpaceOverlay Canvas (sortingOrder = 31000, below Moon1WinScreen 32000)
    /// with a single bottom-centered Text. Source of truth for the prompt text is
    /// upstream (<see cref="InteractableBuilding.GetInteractPrompt"/>); this
    /// component swaps "[E]" with "[A]" when a gamepad is present and animates
    /// the alpha. CanvasGroup alpha drives the fade; Time.unscaledDeltaTime so it
    /// keeps working through pauses.
    /// </summary>
    [DefaultExecutionOrder(-30)]
    public class Moon1InteractionPrompt : MonoBehaviour
    {
        const int SortingOrder = 31000;
        const float FadeInSeconds = 0.3f;
        const float FadeOutSeconds = 0.3f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<Moon1InteractionPrompt>(FindObjectsInactive.Include) != null) return;
            var host = new GameObject(nameof(Moon1InteractionPrompt));
            DontDestroyOnLoad(host);
            host.AddComponent<Moon1InteractionPrompt>();
        }

        CanvasGroup _group;
        Text _label;
        Coroutine _fade;
        bool _visible;

        void Awake()
        {
            BuildCanvas();
            Tartaria.Core.GameEvents.OnHUDShowInteractionPrompt += HandleShow;
            Tartaria.Core.GameEvents.OnHUDHideInteractionPrompt += HandleHide;
            Debug.Log("[Moon1InteractionPrompt] Bootstrapped + subscribed to interaction prompt events.");
        }

        void OnDestroy()
        {
            Tartaria.Core.GameEvents.OnHUDShowInteractionPrompt -= HandleShow;
            Tartaria.Core.GameEvents.OnHUDHideInteractionPrompt -= HandleHide;
        }

        void BuildCanvas()
        {
            // Own GameObject — separate from QuestObjectiveTrackerUI canvas. We
            // parent the canvas under DontDestroyOnLoad-singleton host so it
            // survives scene loads alongside Moon1WinScreen.
            var canvasGo = new GameObject("InteractionPromptCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            _group = canvasGo.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;

            var textGo = new GameObject("PromptLabel");
            textGo.transform.SetParent(canvasGo.transform, false);
            _label = textGo.AddComponent<Text>();
            _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _label.fontSize = 32;
            _label.alignment = TextAnchor.MiddleCenter;
            _label.color = Color.white;
            _label.text = string.Empty;

            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 120f);
            rt.sizeDelta = new Vector2(900f, 80f);

            // Subtle drop shadow for legibility over bright daytime scenes.
            var shadow = textGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        void HandleShow(string message)
        {
            if (_label == null) return;
            _label.text = SwapForGamepad(message);
            if (_visible)
            {
                // Already visible — just refresh text, no re-fade.
                return;
            }
            _visible = true;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(FadeTo(1f, FadeInSeconds));
        }

        void HandleHide()
        {
            if (!_visible) return;
            _visible = false;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(FadeTo(0f, FadeOutSeconds));
        }

        static string SwapForGamepad(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;
            if (Gamepad.current == null) return message;
            // Cheap textual swap. Upstream prompt format is "[E] <verb> <name>".
            return message.Replace("[E]", "[A]");
        }

        IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = _group.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                yield return null;
            }
            _group.alpha = targetAlpha;
            _fade = null;
        }
    }
}
