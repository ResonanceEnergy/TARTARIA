using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.UI
{
    /// <summary>
    /// DebugConsole — in-game command terminal for testing + cheats.
    /// Toggle with ~ (tilde) or F1. Auto-complete with Tab. Command history with Up/Down arrows.
    ///
    /// Built-in commands:
    /// - /help → list all commands
    /// - /moon <N> → unlock Moon N
    /// - /rs <amount> → set Resonance Score
    /// - /tp <x> <y> <z> → teleport player
    /// - /spawn <prefabName> → spawn prefab at player
    /// - /kill → kill player
    /// - /god → toggle godmode
    /// - /speed <N> → set player speed multiplier
    /// - /clear → clear console log
    ///
    /// Usage:
    /// - Attach to Canvas GameObject
    /// - RegisterCommand("cmdname", callback) for custom commands
    /// - ExecuteCommand("cmdname arg1 arg2") programmatically
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] GameObject consolePanel;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] TextMeshProUGUI outputText;
        [SerializeField] ScrollRect scrollRect;

        [Header("Settings")]
        [SerializeField] Key toggleKey = Key.Backquote; // ~ (tilde)
        [SerializeField] Key toggleKeyAlt = Key.F1;
        [SerializeField] int maxOutputLines = 100;
        [SerializeField] bool startHidden = true;

        Dictionary<string, ConsoleCommand> _commands = new();
        List<string> _commandHistory = new();
        int _historyIndex = -1;
        List<string> _outputLines = new();
        bool _isVisible;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterBuiltInCommands();

            if (consolePanel != null)
            {
                consolePanel.SetActive(!startHidden);
                _isVisible = !startHidden;
            }
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Toggle console
            if (keyboard[toggleKey].wasPressedThisFrame || keyboard[toggleKeyAlt].wasPressedThisFrame)
            {
                ToggleConsole();
            }

            if (!_isVisible) return;

            // Command history navigation
            if (keyboard[Key.UpArrow].wasPressedThisFrame)
            {
                NavigateHistory(-1);
            }
            else if (keyboard[Key.DownArrow].wasPressedThisFrame)
            {
                NavigateHistory(1);
            }

            // Submit command
            if (keyboard[Key.Enter].wasPressedThisFrame || keyboard[Key.NumpadEnter].wasPressedThisFrame)
            {
                if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
                {
                    ExecuteCommand(inputField.text);
                    inputField.text = "";
                    inputField.ActivateInputField();
                }
            }

            // Auto-complete with Tab
            if (keyboard[Key.Tab].wasPressedThisFrame)
            {
                AutoComplete();
            }
        }

        void RegisterBuiltInCommands()
        {
            RegisterCommand("help", "List all commands", args =>
            {
                Log("=== AVAILABLE COMMANDS ===");
                foreach (var cmd in _commands.OrderBy(c => c.Key))
                {
                    Log($"  /{cmd.Key} - {cmd.Value.description}");
                }
            });

            RegisterCommand("clear", "Clear console output", args =>
            {
                _outputLines.Clear();
                if (outputText != null) outputText.text = "";
            });

            RegisterCommand("moon", "Unlock Moon <N>", args =>
            {
                if (args.Length < 1) { LogError("Usage: /moon <N>"); return; }
                if (int.TryParse(args[0], out int moonIndex))
                {
                    if (moonIndex >= 1 && moonIndex <= 13)
                    {
                        Save.SaveManager.Instance?.SetMoonProgress(moonIndex, 100f);
                        Log($"Moon {moonIndex} unlocked (100% progress)");
                    }
                    else
                    {
                        LogError("Moon index must be 1-13");
                    }
                }
                else
                {
                    LogError("Invalid moon index");
                }
            });

            RegisterCommand("rs", "Set Resonance Score <amount>", args =>
            {
                if (args.Length < 1) { LogError("Usage: /rs <amount>"); return; }
                if (float.TryParse(args[0], out float amount))
                {
                    var aether = Core.AetherFieldManager.Instance;
                    if (aether != null)
                    {
                        // Set RS by calculating delta from current
                        float delta = amount - aether.ResonanceScore;
                        aether.AddResonanceScore(delta);
                        Log($"RS set to {aether.ResonanceScore:F1}");
                    }
                    else
                    {
                        LogError("AetherFieldManager not found");
                    }
                }
                else
                {
                    LogError("Invalid RS amount");
                }
            });

            RegisterCommand("tp", "Teleport player to <x> <y> <z>", args =>
            {
                if (args.Length < 3) { LogError("Usage: /tp <x> <y> <z>"); return; }
                if (float.TryParse(args[0], out float x) &&
                    float.TryParse(args[1], out float y) &&
                    float.TryParse(args[2], out float z))
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.transform.position = new Vector3(x, y, z);
                        Log($"Teleported to ({x}, {y}, {z})");
                    }
                    else
                    {
                        LogError("Player not found");
                    }
                }
                else
                {
                    LogError("Invalid coordinates");
                }
            });

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // Dev/Editor-only: PlayerHealth.GodMode property is only compiled under the same
            // gate, so this command would fail to compile in shipped builds without the wrap.
            RegisterCommand("god", "Toggle godmode (dev only)", args =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var health = player.GetComponent<Gameplay.PlayerHealth>();
                    if (health != null)
                    {
                        health.GodMode = !health.GodMode;
                        Log($"Godmode {(health.GodMode ? "ON" : "OFF")}");
                    }
                    else
                    {
                        LogError("Player has no health component");
                    }
                }
                else
                {
                    LogError("Player not found");
                }
            });
#endif

            RegisterCommand("speed", "Set player speed multiplier <N>", args =>
            {
                if (args.Length < 1) { LogError("Usage: /speed <N>"); return; }
                if (float.TryParse(args[0], out float speed))
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        var inputHandler = player.GetComponent<Input.PlayerInputHandler>();
                        if (inputHandler != null)
                        {
                            inputHandler.SpeedMultiplier = speed;
                            Log($"Speed multiplier set to {speed}");
                        }
                        else
                        {
                            LogError("Player has no PlayerInputHandler");
                        }
                    }
                    else
                    {
                        LogError("Player not found");
                    }
                }
                else
                {
                    LogError("Invalid speed value");
                }
            });

            RegisterCommand("kill", "Kill player", args =>
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var health = player.GetComponent<Gameplay.PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(9999);
                        Log("Player killed");
                    }
                    else
                    {
                        LogError("Player has no health component");
                    }
                }
                else
                {
                    LogError("Player not found");
                }
            });

            RegisterCommand("spawn", "Spawn prefab <name> at player", args =>
            {
                if (args.Length < 1) { LogError("Usage: /spawn <prefabName>"); return; }
                string prefabName = args[0];

                // Try loading from Resources
                var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
                if (prefab != null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    Vector3 spawnPos = player != null ? player.transform.position + player.transform.forward * 3f : Vector3.zero;
                    Instantiate(prefab, spawnPos, Quaternion.identity);
                    Log($"Spawned {prefabName} at {spawnPos}");
                }
                else
                {
                    LogError($"Prefab '{prefabName}' not found in Resources/Prefabs/");
                }
            });

            Debug.Log("[DebugConsole] Registered 9 built-in commands");
        }

        public void RegisterCommand(string name, string description, Action<string[]> callback)
        {
            name = name.ToLower().TrimStart('/');

            if (_commands.ContainsKey(name))
            {
                Debug.LogWarning($"[DebugConsole] Command '{name}' already registered, overwriting");
            }

            _commands[name] = new ConsoleCommand { name = name, description = description, callback = callback };
        }

        public void ExecuteCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return;

            commandLine = commandLine.Trim();
            _commandHistory.Insert(0, commandLine);
            _historyIndex = -1;

            Log($"> {commandLine}");

            // Parse command + args
            string[] parts = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string cmdName = parts[0].ToLower().TrimStart('/');
            string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            // Execute
            if (_commands.TryGetValue(cmdName, out var cmd))
            {
                try
                {
                    cmd.callback(args);
                }
                catch (Exception ex)
                {
                    LogError($"Command error: {ex.Message}");
                }
            }
            else
            {
                LogError($"Unknown command: {cmdName}. Type /help for commands.");
            }
        }

        public void ToggleConsole()
        {
            _isVisible = !_isVisible;
            if (consolePanel != null)
            {
                consolePanel.SetActive(_isVisible);
            }

            if (_isVisible && inputField != null)
            {
                inputField.ActivateInputField();
            }
        }

        public void Log(string message)
        {
            _outputLines.Add(message);

            if (_outputLines.Count > maxOutputLines)
            {
                _outputLines.RemoveAt(0);
            }

            UpdateOutputText();
        }

        public void LogError(string message)
        {
            Log($"<color=red>[ERROR] {message}</color>");
        }

        void UpdateOutputText()
        {
            if (outputText != null)
            {
                outputText.text = string.Join("\n", _outputLines);

                // Scroll to bottom
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0) return;

            _historyIndex = Mathf.Clamp(_historyIndex + direction, -1, _commandHistory.Count - 1);

            if (inputField != null)
            {
                if (_historyIndex >= 0 && _historyIndex < _commandHistory.Count)
                {
                    inputField.text = _commandHistory[_historyIndex];
                    inputField.caretPosition = inputField.text.Length;
                }
                else
                {
                    inputField.text = "";
                }
            }
        }

        void AutoComplete()
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;

            string partial = inputField.text.ToLower().TrimStart('/');
            var matches = _commands.Keys.Where(cmd => cmd.StartsWith(partial)).ToList();

            if (matches.Count == 1)
            {
                inputField.text = "/" + matches[0];
                inputField.caretPosition = inputField.text.Length;
            }
            else if (matches.Count > 1)
            {
                Log($"Matches: {string.Join(", ", matches)}");
            }
        }

        struct ConsoleCommand
        {
            public string name;
            public string description;
            public Action<string[]> callback;
        }
    }
}
