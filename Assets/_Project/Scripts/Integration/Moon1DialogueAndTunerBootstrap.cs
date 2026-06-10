using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// R327/R328 — Bootstrap UIManager, DialoguePanel, and TuningOverlay onto whatever HUD canvas already exists.
    ///
    /// Root cause: RuntimeHUDBuilder.Bootstrap() bails when HUD_Root prefab is present in scene,
    /// but HUD_Root doesn't author UIManager, the DialoguePanel, or the TuningOverlay.
    /// Result: IsDialogueRunning=True at runtime but "Active dialogue canvases: 0" (no UI shown),
    /// and T key for Frequency Tuner stays inert.
    ///
    /// This script runs AfterSceneLoad (priority 0, after RuntimeHUDBuilder's -100) and:
    ///   1. Ensures a UIManager exists in the scene (adds component to first available GO)
    ///   2. Finds the existing HUD canvas (HUD_Root child OR runtime HUD_Canvas)
    ///   3. Builds a minimal DialoguePanel onto it and wires UIManager.dialoguePanel/Speaker/Body via reflection
    ///   4. Builds a minimal TuningOverlay onto it and toggles it on T key
    ///   5. Ensures TartariaLineView is attached to DialogueRunner (the DialogueViewBase that bridges Yarn -> UIManager.ShowDialogue)
    ///
    /// Idempotent: if everything's already wired, this script no-ops.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1DialogueAndTunerBootstrap : MonoBehaviour
    {
        public static Moon1DialogueAndTunerBootstrap Instance { get; private set; }

        GameObject _dialoguePanel;
        TextMeshProUGUI _dialogueSpeaker;
        TextMeshProUGUI _dialogueBody;
        GameObject _tuningOverlay;
        bool _prevTuneKey;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("Moon1DialogueAndTunerBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1DialogueAndTunerBootstrap>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Run once after all other Awake()s complete so HUD_Root / RuntimeHUDBuilder / UIManager have all had a chance to register.
            EnsureUIManager();
            EnsureTartariaLineView();
            BuildPanelsIfMissing();
        }

        // ─── R327 #1: ensure UIManager singleton exists ─────────────────────────────
        void EnsureUIManager()
        {
            if (UIManager.Instance != null) return;

            // Look for any existing UIManager component first
            var existing = UnityEngine.Object.FindFirstObjectByType<UIManager>();
            if (existing != null) return;

            // Add UIManager to a stable host GO so subsequent prefab reloads don't blow it away
            var host = GameObject.Find("HUD_Root");
            if (host == null) host = GameObject.Find("Moon1_Systems");
            if (host == null)
            {
                host = new GameObject("UIManager_Host");
                DontDestroyOnLoad(host);
            }
            host.AddComponent<UIManager>();
            Debug.Log($"[R327] Added UIManager to '{host.name}'");
        }

        // ─── R327 #2: ensure DialogueRunner has a TartariaLineView view ─────────────
        void EnsureTartariaLineView()
        {
            var dr = UnityEngine.Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
            if (dr == null) { Debug.LogWarning("[R327] No DialogueRunner in scene"); return; }
            var existing = dr.GetComponent<TartariaLineView>();
            if (existing != null) return;
            dr.gameObject.AddComponent<TartariaLineView>();
            Debug.Log($"[R327] Added TartariaLineView to '{dr.gameObject.name}'");
        }

        // ─── R327 #3 + R328 #1: build DialoguePanel + TuningOverlay onto existing canvas ─────
        void BuildPanelsIfMissing()
        {
            var canvasRT = FindHudCanvasRT();
            if (canvasRT == null)
            {
                Debug.LogWarning("[R327/R328] No HUD canvas found — can't build dialog/tuner panels");
                return;
            }

            // R327: DialoguePanel
            if (canvasRT.Find("DialoguePanel") == null)
            {
                BuildDialoguePanel(canvasRT);
                Debug.Log("[R327] DialoguePanel built on existing canvas");
            }
            else
            {
                _dialoguePanel = canvasRT.Find("DialoguePanel").gameObject;
                _dialogueSpeaker = _dialoguePanel.transform.Find("DialogueSpeaker")?.GetComponent<TextMeshProUGUI>();
                _dialogueBody = _dialoguePanel.transform.Find("DialogueBody")?.GetComponent<TextMeshProUGUI>();
            }

            // R328: TuningOverlay
            if (canvasRT.Find("TuningOverlay") == null)
            {
                BuildTuningOverlay(canvasRT);
                Debug.Log("[R328] TuningOverlay built on existing canvas");
            }
            else
            {
                _tuningOverlay = canvasRT.Find("TuningOverlay").gameObject;
            }

            // Wire UIManager fields via reflection so UIManager.ShowDialogue(speaker, text) lights up the panel.
            WireUIManagerFields();
        }

        RectTransform FindHudCanvasRT()
        {
            // Preferred: scene-baked HUD_Root with child Canvas
            var hudRoot = GameObject.Find("HUD_Root");
            if (hudRoot != null)
            {
                var c = hudRoot.GetComponentInChildren<Canvas>(true);
                if (c != null) return c.transform as RectTransform;
            }
            // Fallback: any active screen-overlay canvas
            foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c.transform as RectTransform;
            }
            return null;
        }

        void BuildDialoguePanel(RectTransform canvasRT)
        {
            // Bottom-center bar, anchored at 10%-90% horizontal, 0-22% vertical.
            _dialoguePanel = new GameObject("DialoguePanel");
            _dialoguePanel.transform.SetParent(canvasRT, false);
            _dialoguePanel.SetActive(false);

            var rt = _dialoguePanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0f);
            rt.anchorMax = new Vector2(0.9f, 0.22f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 16f); // 16px above bottom edge

            var bg = _dialoguePanel.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.08f, 0.82f);
            bg.raycastTarget = false;

            // Speaker line (top of panel)
            var speakerGO = new GameObject("DialogueSpeaker");
            speakerGO.transform.SetParent(_dialoguePanel.transform, false);
            var sRT = speakerGO.AddComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0f, 0.7f);
            sRT.anchorMax = Vector2.one;
            sRT.offsetMin = new Vector2(16f, 0f);
            sRT.offsetMax = new Vector2(-16f, -6f);
            _dialogueSpeaker = speakerGO.AddComponent<TextMeshProUGUI>();
            _dialogueSpeaker.text = "";
            _dialogueSpeaker.fontSize = 22f;
            _dialogueSpeaker.fontStyle = FontStyles.Bold;
            _dialogueSpeaker.color = new Color(1f, 0.85f, 0.45f); // Aether-Gold per Art Bible
            _dialogueSpeaker.alignment = TextAlignmentOptions.TopLeft;
            _dialogueSpeaker.raycastTarget = false;

            // Body (main dialogue text)
            var bodyGO = new GameObject("DialogueBody");
            bodyGO.transform.SetParent(_dialoguePanel.transform, false);
            var bRT = bodyGO.AddComponent<RectTransform>();
            bRT.anchorMin = Vector2.zero;
            bRT.anchorMax = new Vector2(1f, 0.7f);
            bRT.offsetMin = new Vector2(16f, 10f);
            bRT.offsetMax = new Vector2(-16f, -4f);
            _dialogueBody = bodyGO.AddComponent<TextMeshProUGUI>();
            _dialogueBody.text = "";
            _dialogueBody.fontSize = 18f;
            _dialogueBody.color = new Color(0.92f, 0.92f, 0.86f);
            _dialogueBody.alignment = TextAlignmentOptions.TopLeft;
            _dialogueBody.raycastTarget = false;
            _dialogueBody.textWrappingMode = TextWrappingModes.Normal;
        }

        void BuildTuningOverlay(RectTransform canvasRT)
        {
            // Centered modal panel for Frequency Tuner
            _tuningOverlay = new GameObject("TuningOverlay");
            _tuningOverlay.transform.SetParent(canvasRT, false);
            _tuningOverlay.SetActive(false);

            var rt = _tuningOverlay.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(560f, 320f);

            var bg = _tuningOverlay.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.06f, 0.12f, 0.95f);
            bg.raycastTarget = true;

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(_tuningOverlay.transform, false);
            var tRT = titleGO.AddComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0f, 0.82f);
            tRT.anchorMax = new Vector2(1f, 1f);
            tRT.offsetMin = new Vector2(16f, 4f);
            tRT.offsetMax = new Vector2(-16f, -4f);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "FREQUENCY TUNER";
            title.fontSize = 28f;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(1f, 0.85f, 0.45f); // Aether-Gold

            // Hint
            var hintGO = new GameObject("Hint");
            hintGO.transform.SetParent(_tuningOverlay.transform, false);
            var hRT = hintGO.AddComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0f, 0.15f);
            hRT.anchorMax = new Vector2(1f, 0.78f);
            hRT.offsetMin = new Vector2(24f, 8f);
            hRT.offsetMax = new Vector2(-24f, -8f);
            var hint = hintGO.AddComponent<TextMeshProUGUI>();
            hint.text =
                "Tune the buried structure to its harmonic.\n\n" +
                "<color=#88CCFF>•</color>  D-Pad <color=#FFE050>Left/Right</color> — adjust frequency\n" +
                "<color=#88CCFF>•</color>  Target band: <color=#FFE050>432 Hz</color> (Harmonic)\n" +
                "<color=#88CCFF>•</color>  Hold within range to lock\n\n" +
                "Press <color=#FFE050>[T]</color> again to close.";
            hint.fontSize = 16f;
            hint.color = new Color(0.92f, 0.92f, 0.86f);
            hint.alignment = TextAlignmentOptions.TopLeft;
            hint.textWrappingMode = TextWrappingModes.Normal;

            // Close hint at bottom
            var bottomGO = new GameObject("BottomHint");
            bottomGO.transform.SetParent(_tuningOverlay.transform, false);
            var bhRT = bottomGO.AddComponent<RectTransform>();
            bhRT.anchorMin = new Vector2(0f, 0f);
            bhRT.anchorMax = new Vector2(1f, 0.15f);
            bhRT.offsetMin = new Vector2(16f, 12f);
            bhRT.offsetMax = new Vector2(-16f, -4f);
            var bhint = bottomGO.AddComponent<TextMeshProUGUI>();
            bhint.text = "[T] Close   [ESC] Cancel";
            bhint.fontSize = 14f;
            bhint.color = new Color(0.7f, 0.7f, 0.7f);
            bhint.alignment = TextAlignmentOptions.Center;
        }

        void WireUIManagerFields()
        {
            var uiMgr = UIManager.Instance;
            if (uiMgr == null) { Debug.LogWarning("[R327] UIManager.Instance NULL after Ensure step"); return; }
            if (_dialoguePanel == null || _dialogueSpeaker == null || _dialogueBody == null)
            {
                Debug.LogWarning("[R327] Dialog refs NULL — can't wire");
                return;
            }
            SetField(uiMgr, "dialoguePanel", _dialoguePanel);
            SetField(uiMgr, "dialogueSpeakerText", _dialogueSpeaker);
            SetField(uiMgr, "dialogueBodyText", _dialogueBody);
            Debug.Log("[R327] UIManager dialoguePanel/Speaker/Body wired via reflection");
        }

        static void SetField(object target, string fieldName, object value)
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            var f = target.GetType().GetField(fieldName, flags);
            if (f == null) { Debug.LogWarning($"[R327] Field '{fieldName}' not found on {target.GetType().Name}"); return; }
            f.SetValue(target, value);
        }

        // ─── R328: T key toggles TuningOverlay (RuntimeHUDBuilder.Update has identical logic but only fires if _tuningOverlayGO non-null in THAT instance).
        // Since RuntimeHUDBuilder bails when HUD_Root exists, RuntimeHUDBuilder._tuningOverlayGO is null. Drive the toggle here instead.
        void Update()
        {
            if (_tuningOverlay == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            bool tuneDown = kb.tKey.isPressed;
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null && pad.leftTrigger.isPressed) tuneDown = true;
            if (tuneDown && !_prevTuneKey)
            {
                _tuningOverlay.SetActive(!_tuningOverlay.activeSelf);
            }
            _prevTuneKey = tuneDown;
        }
    }
}
