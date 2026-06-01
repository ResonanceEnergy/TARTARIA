using System.Collections;
using TMPro;
using Tartaria.Core;
using Tartaria.Core.Enums;
using Tartaria.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// End-card controller for both demo slice and full game endings.
    /// Handles: Demo end (awaken_star_dome), Moon 13 endings (Harmony/Echo/Reset).
    /// Self-builds its Canvas + CanvasGroup + TMP_Text at runtime so it
    /// has zero scene-wiring cost.
    /// </summary>
    [DisallowMultipleComponent]
    public class EndCardController : MonoBehaviour
    {
        // Demo slice trigger
        public const string TriggerQuestId = "awaken_star_dome";
        public const string DemoTitleText = "TARTARIA";
        public const string DemoSubtitleText = "DEMO BUILD";

        // Full game ending quests
        public const string HarmonyEndingQuestId = "moon13_harmony_ending";
        public const string EchoEndingQuestId = "moon13_echo_ending";
        public const string ResetEndingQuestId = "moon13_reset_ending";

        [SerializeField] float fadeInDuration = 2.0f;
        [SerializeField] float holdDuration = 8.0f;
        [SerializeField] float fadeOutDuration = 2.0f;
        [SerializeField] float creditsDuration = 30.0f; // Credits scroll duration
        [SerializeField] float postCreditsDuration = 10.0f; // Post-credits scene

        Canvas _canvas;
        CanvasGroup _group;
        TMP_Text _title;
        TMP_Text _subtitle;
        TMP_Text _bodyText;
        TMP_Text _creditsText;
        Image _backgroundImage;
        bool _triggered;
        bool _isPlayingEnding;

        enum EndingType
        {
            Demo,
            Harmony,
            Echo,
            Reset
        }

        void Awake()
        {
            BuildOverlay();
            _group.alpha = 0f;
        }

        void OnEnable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged += HandleQuestStatusChanged;
            }
        }

        void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged -= HandleQuestStatusChanged;
            }
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
            if (_isPlayingEnding) return;
            if (status != QuestStatus.Completed) return;

            // Check for demo end
            if (string.Equals(questId, TriggerQuestId, System.StringComparison.OrdinalIgnoreCase))
            {
                StartCoroutine(PlayDemoEnd());
                return;
            }

            // Check for full game endings
            if (string.Equals(questId, HarmonyEndingQuestId, System.StringComparison.OrdinalIgnoreCase))
            {
                StartCoroutine(PlayHarmonyEnding());
                return;
            }

            if (string.Equals(questId, EchoEndingQuestId, System.StringComparison.OrdinalIgnoreCase))
            {
                StartCoroutine(PlayEchoEnding());
                return;
            }

            if (string.Equals(questId, ResetEndingQuestId, System.StringComparison.OrdinalIgnoreCase))
            {
                StartCoroutine(PlayResetEnding());
                return;
            }
        }

        /// <summary>Public hook for manual testing / non-quest triggers.</summary>
        public void TriggerEnd()
        {
            if (_isPlayingEnding) return;
            StartCoroutine(PlayDemoEnd());
        }

        /// <summary>Trigger specific ending manually</summary>
        public void TriggerEnding(string endingType)
        {
            if (_isPlayingEnding) return;

            switch (endingType.ToLower())
            {
                case "harmony":
                    StartCoroutine(PlayHarmonyEnding());
                    break;
                case "echo":
                    StartCoroutine(PlayEchoEnding());
                    break;
                case "reset":
                    StartCoroutine(PlayResetEnding());
                    break;
                default:
                    StartCoroutine(PlayDemoEnd());
                    break;
            }
        }

        IEnumerator PlayDemoEnd()
        {
            _isPlayingEnding = true;
            Debug.Log("[EndCard] Demo slice complete. Rolling end card.");

            _title.text = DemoTitleText;
            _subtitle.text = DemoSubtitleText;
            _bodyText.text = "";
            _backgroundImage.color = Color.black;

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            _isPlayingEnding = false;
        }

        IEnumerator PlayHarmonyEnding()
        {
            _isPlayingEnding = true;
            Debug.Log("[EndCard] HARMONY ENDING - Golden Age Restored");

            _backgroundImage.color = new Color(1f, 0.9f, 0.4f, 1f); // Golden
            _title.text = "HARMONY";
            _subtitle.text = "The Golden Age Returns";
            _bodyText.text = "The mud recedes.\nBuildings rise in full glory.\nGiants walk among humans again.\n\nThe Aether never left.\nIt was waiting for someone to listen.";
            _bodyText.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark text on golden bg

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration * 1.5f);
            yield return Fade(1f, 0f, fadeOutDuration);

            // Play credits
            yield return PlayCreditsSequence();

            // Post-credits hook: Harmony ending teaser
            yield return PlayHarmonyPostCredits();

            // Save ending achieved
            SaveManager.Instance?.SetGameFlag("harmony_ending_achieved", true);

            Debug.Log("[EndCard] Harmony ending complete - Thank you for playing TARTARIA");
            _isPlayingEnding = false;
        }

        IEnumerator PlayEchoEnding()
        {
            _isPlayingEnding = true;
            Debug.Log("[EndCard] ECHO ENDING - Parallel Worlds");

            _backgroundImage.color = new Color(0.3f, 0.5f, 0.8f, 1f); // Aurora blue
            _title.text = "ECHO";
            _subtitle.text = "Between Two Worlds";
            _bodyText.text = "Both timelines preserved.\nWalk between Golden Age and Present.\n\nZereth finds peace in the threshold.\n\nTwo worlds, one heart.";
            _bodyText.color = Color.white;

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration * 1.5f);
            yield return Fade(1f, 0f, fadeOutDuration);

            // Play credits
            yield return PlayCreditsSequence();

            // Post-credits hook: Echo ending teaser
            yield return PlayEchoPostCredits();

            // Save ending achieved
            SaveManager.Instance?.SetGameFlag("echo_ending_achieved", true);

            Debug.Log("[EndCard] Echo ending complete - Thank you for playing TARTARIA");
            _isPlayingEnding = false;
        }

        IEnumerator PlayResetEnding()
        {
            _isPlayingEnding = true;
            Debug.Log("[EndCard] RESET ENDING - Controlled Power");

            _backgroundImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Muted gray
            _title.text = "RESET";
            _subtitle.text = "Power Without Freedom";
            _bodyText.text = "Immense power achieved.\nBut the wonder dims.\n\nThe sky never fully clears.\n\nSafety without song.";
            _bodyText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration * 1.5f);
            yield return Fade(1f, 0f, fadeOutDuration);

            // Play credits
            yield return PlayCreditsSequence();

            // Post-credits hook: Reset ending teaser
            yield return PlayResetPostCredits();

            // Save ending achieved
            SaveManager.Instance?.SetGameFlag("reset_ending_achieved", true);

            Debug.Log("[EndCard] Reset ending complete - Thank you for playing TARTARIA");
            _isPlayingEnding = false;
        }

        IEnumerator PlayCreditsSequence()
        {
            Debug.Log("[EndCard] Rolling credits...");

            // Reset UI for credits
            _backgroundImage.color = Color.black;
            _title.text = "";
            _subtitle.text = "";
            _bodyText.text = "";

            // Show credits text
            _creditsText.gameObject.SetActive(true);
            _creditsText.text = GenerateCreditsText();
            _creditsText.color = new Color(1f, 0.9f, 0.5f); // Aether gold

            // Fade in
            yield return Fade(0f, 1f, fadeInDuration);

            // Scroll credits (simple hold for now, could animate scroll)
            yield return new WaitForSeconds(creditsDuration);

            // Fade out
            yield return Fade(1f, 0f, fadeOutDuration);

            _creditsText.gameObject.SetActive(false);

            Debug.Log("[EndCard] Credits complete");
        }

        string GenerateCreditsText()
        {
            return @"TARTARIA
The Frequency of Forgotten Cities

A 13-Moon Journey Through Mud and Memory

━━━━━━━━━━━━━━━━━━━━━━

COMPANIONS
Milo, the Orphan
Thorne, the Engineer
Lirael, the Echo-Girl
Korath, the Last Giant

━━━━━━━━━━━━━━━━━━━━━━

THE MOONS
1. Solar — The Lighting
2. Lunar — The Feeling
3. Electric — The Activating
4. Self-Existing — The Defining
5. Overtone — The Commanding
6. Rhythmic — The Organizing
7. Resonant — The Channeling
8. Galactic — The Harmonizing
9. Solar — The Intending
10. Planetary — The Manifesting
11. Spectral — The Releasing
12. Crystal — The Cooperating
13. Cosmic — The Enduring

━━━━━━━━━━━━━━━━━━━━━━

Thank you for listening.

The Aether remembers.

━━━━━━━━━━━━━━━━━━━━━━";
        }

        IEnumerator PlayHarmonyPostCredits()
        {
            Debug.Log("[EndCard] Post-Credits: Harmony hook — Golden Age DLC tease");

            _backgroundImage.color = new Color(1f, 0.9f, 0.4f, 1f); // Golden
            _title.text = "";
            _subtitle.text = "";
            _bodyText.text = "One Year Later...\n\nThe first airship to Mars departs next moon.\n\nZereth pilots.";
            _bodyText.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(postCreditsDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            Debug.Log("[EndCard] HARMONY post-credits complete — DLC: 'Mars Awakening' teased");
        }

        IEnumerator PlayEchoPostCredits()
        {
            Debug.Log("[EndCard] Post-Credits: Echo hook — Threshold DLC tease");

            _backgroundImage.color = new Color(0.3f, 0.5f, 0.8f, 1f); // Aurora blue
            _title.text = "";
            _subtitle.text = "";
            _bodyText.text = "Between Timelines...\n\nZereth guards the gate.\n\nSomeone else is knocking.";
            _bodyText.color = Color.white;

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(postCreditsDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            Debug.Log("[EndCard] ECHO post-credits complete — DLC: 'The Threshold Keeper' teased");
        }

        IEnumerator PlayResetPostCredits()
        {
            Debug.Log("[EndCard] Post-Credits: Reset hook — Resistance DLC tease");

            _backgroundImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Muted gray
            _title.text = "";
            _subtitle.text = "";
            _bodyText.text = "Underneath the Control...\n\nMilo starts a resistance.\n\n'They took the song. We'll take it back.'";
            _bodyText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(postCreditsDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            Debug.Log("[EndCard] RESET post-credits complete — DLC: 'The Resonance Underground' teased");
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
            _backgroundImage = bgGo.AddComponent<Image>();
            _backgroundImage.color = Color.black;
            var bgRt = _backgroundImage.rectTransform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            _title = CreateText(canvasGo.transform, "Title", DemoTitleText, 96, new Vector2(0.5f, 0.65f));
            _subtitle = CreateText(canvasGo.transform, "Subtitle", DemoSubtitleText, 36, new Vector2(0.5f, 0.55f));
            _bodyText = CreateText(canvasGo.transform, "BodyText", "", 28, new Vector2(0.5f, 0.35f));
            _bodyText.alignment = TextAlignmentOptions.Center;
            _bodyText.rectTransform.sizeDelta = new Vector2(1400, 400);

            _creditsText = CreateText(canvasGo.transform, "CreditsText", "", 32, new Vector2(0.5f, 0.5f));
            _creditsText.alignment = TextAlignmentOptions.Center;
            _creditsText.rectTransform.sizeDelta = new Vector2(1600, 900);
            _creditsText.gameObject.SetActive(false);
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
