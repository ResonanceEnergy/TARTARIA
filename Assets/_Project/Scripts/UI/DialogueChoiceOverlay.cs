using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// Dialogue choice overlay — IMGUI panel showing 1-4 numbered choices.
    /// Controls: 1-4 keys, arrow nav + Enter, Gamepad-South confirm, Escape cancel.
    /// Pauses gameplay via PauseService (refcounted) while open, unlocks cursor.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueChoiceOverlay : MonoBehaviour
    {
        static DialogueChoiceOverlay s_instance;

        [Header("UI Settings")]
        [SerializeField] float panelWidth = 600f;
        [SerializeField] float panelHeight = 400f;
        [SerializeField] float choiceButtonHeight = 60f;
        [SerializeField] Color backgroundColor = new(0.1f, 0.1f, 0.15f, 0.95f);
        [SerializeField] Color highlightColor = new(0.3f, 0.5f, 0.8f, 1f);

        bool _isActive;
        string _prompt;
        string[] _choices;
        Action<int> _onPick;
        int _selectedIndex;
        CursorLockMode _cachedCursorLock;
        bool _cachedCursorVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (s_instance != null) return;
            var go = new GameObject("DialogueChoiceOverlay");
            DontDestroyOnLoad(go);
            s_instance = go.AddComponent<DialogueChoiceOverlay>();
        }

        void OnDestroy()
        {
            if (_isActive) Dismiss();
            if (s_instance == this) s_instance = null;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Present a choice panel with 1-4 options. Pauses game and unlocks cursor.
        /// </summary>
        public static void Present(string prompt, string[] choices, Action<int> onPick)
        {
            if (s_instance == null)
            {
                Debug.LogError("[DialogueChoice] Instance not initialized!");
                return;
            }

            if (choices == null || choices.Length == 0 || choices.Length > 4)
            {
                Debug.LogError("[DialogueChoice] Must provide 1-4 choices!");
                return;
            }

            s_instance._prompt = prompt ?? "Choose:";
            s_instance._choices = choices;
            s_instance._onPick = onPick;
            s_instance._selectedIndex = 0;
            s_instance._isActive = true;

            s_instance._cachedCursorLock = Cursor.lockState;
            s_instance._cachedCursorVisible = Cursor.visible;

            PauseService.Push();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"[DialogueChoice] Presented: {prompt} ({choices.Length} choices)");
        }

        void Update()
        {
            if (!_isActive) return;

            var kb = Keyboard.current;
            var gp = Gamepad.current;

            // Number key shortcuts
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame && _choices.Length >= 1) Pick(0);
                else if (kb.digit2Key.wasPressedThisFrame && _choices.Length >= 2) Pick(1);
                else if (kb.digit3Key.wasPressedThisFrame && _choices.Length >= 3) Pick(2);
                else if (kb.digit4Key.wasPressedThisFrame && _choices.Length >= 4) Pick(3);
                else if (kb.escapeKey.wasPressedThisFrame) Cancel();

                // Arrow navigation
                if (kb.upArrowKey.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex - 1 + _choices.Length) % _choices.Length;
                else if (kb.downArrowKey.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex + 1) % _choices.Length;

                // Enter confirms
                if (kb.enterKey.wasPressedThisFrame)
                    Pick(_selectedIndex);
            }

            // Gamepad navigation
            if (gp != null)
            {
                if (gp.dpad.up.wasPressedThisFrame || gp.leftStick.up.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex - 1 + _choices.Length) % _choices.Length;
                else if (gp.dpad.down.wasPressedThisFrame || gp.leftStick.down.wasPressedThisFrame)
                    _selectedIndex = (_selectedIndex + 1) % _choices.Length;

                // South button (A/Cross) confirms
                if (gp.buttonSouth.wasPressedThisFrame)
                    Pick(_selectedIndex);

                // East button (B/Circle) cancels
                if (gp.buttonEast.wasPressedThisFrame)
                    Cancel();
            }
        }

        void OnGUI()
        {
            if (!_isActive) return;

            float x = (Screen.width - panelWidth) / 2;
            float y = (Screen.height - panelHeight) / 2;

            // Background panel
            GUI.color = backgroundColor;
            GUI.Box(new Rect(x, y, panelWidth, panelHeight), "");
            GUI.color = Color.white;

            // Prompt text
            var promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            GUI.Label(new Rect(x + 20, y + 20, panelWidth - 40, 60), _prompt, promptStyle);

            // Choice buttons
            float choiceY = y + 100;
            for (int i = 0; i < _choices.Length; i++)
            {
                Rect btnRect = new Rect(x + 20, choiceY, panelWidth - 40, choiceButtonHeight);

                // Highlight selected
                if (i == _selectedIndex)
                {
                    GUI.color = highlightColor;
                    GUI.Box(btnRect, "");
                    GUI.color = Color.white;
                }

                if (GUI.Button(btnRect, $"{i + 1}. {_choices[i]}"))
                {
                    Pick(i);
                }

                choiceY += choiceButtonHeight + 10;
            }

            // Hint text
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(x, y + panelHeight - 30, panelWidth, 20),
                "1-4 / Arrows / Enter / Gamepad | ESC to cancel", hintStyle);
        }

        void Pick(int index)
        {
            if (index < 0 || index >= _choices.Length) return;

            Debug.Log($"[DialogueChoice] Picked option {index + 1}: {_choices[index]}");
            var callback = _onPick;
            Dismiss();
            callback?.Invoke(index);
        }

        void Cancel()
        {
            Debug.Log("[DialogueChoice] Cancelled (no choice made).");
            Dismiss();
            _onPick?.Invoke(-1);
        }

        void Dismiss()
        {
            _isActive = false;
            PauseService.Pop();
            Cursor.lockState = _cachedCursorLock;
            Cursor.visible = _cachedCursorVisible;
        }
    }
}
