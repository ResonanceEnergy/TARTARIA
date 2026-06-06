using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.UI
{
    /// <summary>
    /// Input Remapping UI — allows players to rebind actions from the Input System.
    /// Uses InputActionRebindingExtensions for interactive rebinding.
    ///
    /// Displays all actions in a scrollable list with "Press key to rebind" prompts.
    /// Saves bindings to PlayerPrefs.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputRemappingUI : MonoBehaviour
    {
        public static InputRemappingUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] GameObject rebindButtonPrefab;
        [SerializeField] Transform rebindListParent;

        [Header("Rebind UI")]
        [SerializeField] GameObject rebindPromptPanel;
        [SerializeField] TMPro.TextMeshProUGUI rebindPromptText;

        readonly Dictionary<string, InputActionRebindingExtensions.RebindingOperation> _activeRebinds = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;

            // Cleanup active rebinds
            foreach (var op in _activeRebinds.Values)
                op?.Dispose();
            _activeRebinds.Clear();
        }

        void Start()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("[InputRemapping] No InputActionAsset assigned");
                return;
            }

            LoadBindingOverrides();
            PopulateRebindList();
        }

        // ─── Rebind List Population ──────────────────

        void PopulateRebindList()
        {
            if (rebindListParent == null || rebindButtonPrefab == null) return;

            // Clear existing buttons
            foreach (Transform child in rebindListParent)
                Destroy(child.gameObject);

            // Create a button for each action
            foreach (var actionMap in inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    if (action.bindings.Count == 0) continue;

                    var buttonGO = Instantiate(rebindButtonPrefab, rebindListParent);
                    var button = buttonGO.GetComponent<UnityEngine.UI.Button>();
                    var label = buttonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                    if (label != null)
                        label.text = $"{action.name}: {GetBindingDisplayString(action)}";

                    if (button != null)
                    {
                        string actionName = action.name;
                        button.onClick.AddListener(() => StartRebind(actionName));
                    }
                }
            }
        }

        string GetBindingDisplayString(InputAction action)
        {
            if (action.bindings.Count == 0) return "None";
            return action.bindings[0].effectivePath ?? action.bindings[0].path ?? "None";
        }

        // ─── Rebind Flow ─────────────────────────────

        void StartRebind(string actionName)
        {
            var action = inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"[InputRemapping] Action not found: {actionName}");
                return;
            }

            // Show prompt
            if (rebindPromptPanel != null) rebindPromptPanel.SetActive(true);
            if (rebindPromptText != null) rebindPromptText.text = $"Press key to rebind '{actionName}'...";

            // Cancel any existing rebind for this action
            if (_activeRebinds.TryGetValue(actionName, out var existingOp))
            {
                existingOp?.Dispose();
                _activeRebinds.Remove(actionName);
            }

            // Start interactive rebind
            var rebindOp = action.PerformInteractiveRebinding(0) // Binding index 0 (primary binding)
                .OnComplete(op =>
                {
                    CompleteRebind(actionName);
                    op.Dispose();
                    _activeRebinds.Remove(actionName);
                })
                .OnCancel(op =>
                {
                    CancelRebind(actionName);
                    op.Dispose();
                    _activeRebinds.Remove(actionName);
                })
                .Start();

            _activeRebinds[actionName] = rebindOp;

            Debug.Log($"[InputRemapping] Rebinding '{actionName}'...");
        }

        void CompleteRebind(string actionName)
        {
            // Hide prompt
            if (rebindPromptPanel != null) rebindPromptPanel.SetActive(false);

            // Save binding override
            SaveBindingOverrides();

            // Refresh list
            PopulateRebindList();

            Debug.Log($"[InputRemapping] Rebind complete: {actionName}");
        }

        void CancelRebind(string actionName)
        {
            if (rebindPromptPanel != null) rebindPromptPanel.SetActive(false);
            Debug.Log($"[InputRemapping] Rebind cancelled: {actionName}");
        }

        // ─── Persistence ─────────────────────────────

        void SaveBindingOverrides()
        {
            if (inputActions == null) return;

            string overrides = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("InputBindingOverrides", overrides);
            PlayerPrefs.Save();
            Debug.Log("[InputRemapping] Bindings saved");
        }

        void LoadBindingOverrides()
        {
            if (inputActions == null) return;

            string overrides = PlayerPrefs.GetString("InputBindingOverrides", string.Empty);
            if (!string.IsNullOrEmpty(overrides))
            {
                inputActions.LoadBindingOverridesFromJson(overrides);
                Debug.Log("[InputRemapping] Bindings loaded");
            }
        }

        public void ResetAllBindings()
        {
            if (inputActions == null) return;

            inputActions.RemoveAllBindingOverrides();
            SaveBindingOverrides();
            PopulateRebindList();
            Debug.Log("[InputRemapping] All bindings reset to defaults");
        }
    }
}
