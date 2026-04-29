using System.Collections;
using TMPro;
using Tartaria.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Slice end-card. Listens for the Awaken Star Dome quest completion,
    /// fades a black overlay in, shows the title, holds, then unloads.
    /// Self-builds its Canvas + CanvasGroup + TMP_Text at runtime so it
    /// has zero scene-wiring cost. Per docs/33_VERTICAL_SLICE_SCRIPT.md.
    /// </summary>
    [DisallowMultipleComponent]
    public class EndCardController : MonoBehaviour
    {
        public const string TriggerQuestId = "awaken_star_dome";
        public const string TitleText = "TARTARIA";
        public const string SubtitleText = "DEMO BUILD";

        [SerializeField] float fadeInDuration = 1.5f;
        [SerializeField] float holdDuration = 3.0f;
        [SerializeField] float fadeOutDuration = 1.5f;

        Canvas _canvas;
        CanvasGroup _group;
        TMP_Text _title;
        TMP_Text _subtitle;
        bool _triggered;

        void Awake()
        {
            BuildOverlay();
            _group.alpha = 0f;
        }

        void OnEnable()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStatusChanged += HandleQuestStatusChanged;
        }

        void OnDisable()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestStatusChanged -= HandleQuestStatusChanged;
        }

        void Start()
        {
            // Late-subscribe in case QuestManager spawned after us.
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged -= HandleQuestStatusChanged;
                QuestManager.Instance.OnQuestStatusChanged += HandleQuestStatusChanged;
            }
        }

        void HandleQuestStatusChanged(string questId, QuestStatus status)
        {
            if (_triggered) return;
            if (status != QuestStatus.Completed) return;
            if (!string.Equals(questId, TriggerQuestId, System.StringComparison.OrdinalIgnoreCase)) return;

            _triggered = true;
            StartCoroutine(PlaySequence());
        }

        /// <summary>Public hook for manual testing / non-quest triggers.</summary>
        public void TriggerEnd()
        {
            if (_triggered) return;
            _triggered = true;
            StartCoroutine(PlaySequence());
        }

        IEnumerator PlaySequence()
        {
            Debug.Log("[EndCard] Slice complete. Rolling end card.");
            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);
            _triggered = false; // allow re-trigger if quest ever re-fires
        }

        IEnumerator Fade(float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(from, to, dur > 0f ? t / dur : 1f);
                yield return null;
            }
            _group.alpha = to;
        }

        void BuildOverlay()
        {
            var canvasGo = new GameObject("EndCardCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
            _group = canvasGo.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bg = bgGo.AddComponent<Image>();
            bg.color = Color.black;
            var bgRt = bg.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            _title = CreateText(canvasGo.transform, "Title", TitleText, 96, new Vector2(0.5f, 0.55f));
            _subtitle = CreateText(canvasGo.transform, "Subtitle", SubtitleText, 36, new Vector2(0.5f, 0.45f));
        }

        static TMP_Text CreateText(Transform parent, string name, string content, float size, Vector2 anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.85f, 0.45f); // Aether-Gold
            var rt = tmp.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1200, 200);
            rt.anchoredPosition = Vector2.zero;
            return tmp;
        }
    }
}
