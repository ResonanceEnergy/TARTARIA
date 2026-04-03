using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// In-game debug cheat console for playtesting.
    /// Toggle with BackQuote (`) key. Type commands to manipulate game state.
    ///
    /// Commands:
    ///   rs [amount]         — Set Resonance Score (e.g., "rs 500")
    ///   rs +[amount]        — Add to RS (e.g., "rs +50")
    ///   aether [amount]     — Set Aether charge
    ///   tp [x] [y] [z]     — Teleport player
    ///   spawn enemy         — Spawn mud golem at player position
    ///   quest [id]          — Force-activate a quest
    ///   quest complete [id] — Force-complete a quest
    ///   tutorial skip       — Complete all tutorial steps
    ///   tutorial reset      — Reset tutorial
    ///   state [name]        — Set GameState (Exploration/Combat/Tuning/etc.)
    ///   save                — Force save
    ///   load                — Force load
    ///   god                 — Toggle invulnerability
    ///   speed [mult]        — Set time scale
    ///   clear               — Clear console
    ///   help                — Show commands
    /// </summary>
    public class DebugCheatConsole : MonoBehaviour
    {
        [SerializeField] bool enabledInBuilds = false;
        [SerializeField] KeyCode toggleKey = KeyCode.BackQuote;

        bool _visible;
        string _input = "";
        string _log = "";
        Vector2 _scrollPos;
        bool _godMode;

        const int MaxLogLines = 50;
        int _logLineCount;

        void Update()
        {
            #if !UNITY_EDITOR
            if (!enabledInBuilds) return;
            #endif

            if (UnityEngine.Input.GetKeyDown(toggleKey))
            {
                _visible = !_visible;
                if (_visible) _input = "";
            }
        }

        void OnGUI()
        {
            if (!_visible) return;

            GUI.color = new Color(0f, 0f, 0f, 0.9f);
            GUI.DrawTexture(new Rect(10, 10, 500, 350), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(14, 14, 492, 342));

            // Header
            var headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = new Color(0.9f, 0.85f, 0.3f);
            GUILayout.Label("TARTARIA DEBUG CONSOLE", headerStyle);

            // Log area
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(260));
            var logStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, richText = true };
            logStyle.normal.textColor = new Color(0.8f, 0.9f, 0.8f);
            GUILayout.Label(_log, logStyle);
            GUILayout.EndScrollView();

            // Input
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("DebugInput");
            _input = GUILayout.TextField(_input, GUILayout.Height(22));

            if (GUILayout.Button("Run", GUILayout.Width(50), GUILayout.Height(22))
                || (Event.current.isKey && Event.current.keyCode == KeyCode.Return
                    && GUI.GetNameOfFocusedControl() == "DebugInput"))
            {
                if (!string.IsNullOrWhiteSpace(_input))
                {
                    ExecuteCommand(_input.Trim());
                    _input = "";
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // Auto-focus input when visible
            if (_visible) GUI.FocusControl("DebugInput");
        }

        void Log(string msg)
        {
            _log += msg + "\n";
            _logLineCount++;
            if (_logLineCount > MaxLogLines)
            {
                int idx = _log.IndexOf('\n');
                if (idx >= 0) _log = _log.Substring(idx + 1);
                _logLineCount--;
            }
            _scrollPos.y = float.MaxValue;
        }

        void ExecuteCommand(string cmd)
        {
            Log($"> {cmd}");
            var parts = cmd.ToLowerInvariant().Split(' ');
            if (parts.Length == 0) return;

            switch (parts[0])
            {
                case "rs":
                    HandleRS(parts);
                    break;

                case "aether":
                    HandleAether(parts);
                    break;

                case "tp":
                    HandleTeleport(parts);
                    break;

                case "spawn":
                    HandleSpawn(parts);
                    break;

                case "quest":
                    HandleQuest(parts);
                    break;

                case "tutorial":
                    HandleTutorial(parts);
                    break;

                case "state":
                    HandleState(parts);
                    break;

                case "save":
                    Save.SaveManager.Instance?.Save();
                    Log("Save triggered.");
                    break;

                case "load":
                    Save.SaveManager.Instance?.LoadOrCreate();
                    Log("Load triggered.");
                    break;

                case "god":
                    _godMode = !_godMode;
                    Log($"God mode: {(_godMode ? "ON" : "OFF")}");
                    break;

                case "speed":
                    if (parts.Length > 1 && float.TryParse(parts[1], out float scale))
                    {
                        Time.timeScale = Mathf.Clamp(scale, 0.1f, 10f);
                        Log($"Time scale: {Time.timeScale:F1}x");
                    }
                    else Log("Usage: speed [0.1-10]");
                    break;

                case "clear":
                    _log = "";
                    _logLineCount = 0;
                    break;

                case "help":
                    ShowHelp();
                    break;

                default:
                    Log($"Unknown command: {parts[0]}. Type 'help'.");
                    break;
            }
        }

        void HandleRS(string[] parts)
        {
            if (parts.Length < 2) { Log("Usage: rs [amount] or rs +[amount]"); return; }

            var glc = GameLoopController.Instance;
            if (glc == null) { Log("GameLoopController not found."); return; }

            string val = parts[1];
            if (val.StartsWith("+") && float.TryParse(val.Substring(1), out float add))
            {
                glc.AddResonanceScore(add);
                Log($"RS +{add:F0}");
            }
            else if (float.TryParse(val, out float set))
            {
                glc.SetResonanceScore(set);
                Log($"RS = {set:F0}");
            }
            else Log("Invalid number.");
        }

        void HandleAether(string[] parts)
        {
            if (parts.Length < 2) { Log("Usage: aether [amount]"); return; }
            if (float.TryParse(parts[1], out float val))
            {
                var glc = GameLoopController.Instance;
                glc?.SetAetherCharge(val);
                Log($"Aether = {val:F0}");
            }
        }

        void HandleTeleport(string[] parts)
        {
            if (parts.Length < 4) { Log("Usage: tp [x] [y] [z]"); return; }

            var player = GameObject.FindWithTag("Player");
            if (player == null) { Log("No player found."); return; }

            if (float.TryParse(parts[1], out float x) &&
                float.TryParse(parts[2], out float y) &&
                float.TryParse(parts[3], out float z))
            {
                var cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = new Vector3(x, y, z);
                if (cc != null) cc.enabled = true;
                Log($"Teleported to ({x:F1}, {y:F1}, {z:F1})");
            }
        }

        void HandleSpawn(string[] parts)
        {
            if (parts.Length < 2) { Log("Usage: spawn enemy"); return; }
            if (parts[1] == "enemy")
            {
                var player = GameObject.FindWithTag("Player");
                if (player == null) { Log("No player."); return; }

                Vector3 pos = player.transform.position + player.transform.forward * 5f;
                GameLoopController.Instance?.SpawnEnemyAt(pos);
                Log($"Enemy spawned at {pos}");
            }
        }

        void HandleQuest(string[] parts)
        {
            var qm = QuestManager.Instance;
            if (qm == null) { Log("QuestManager not found."); return; }

            if (parts.Length >= 3 && parts[1] == "complete")
            {
                qm.ProgressObjective(parts[2], 0, 999);
                Log($"Quest '{parts[2]}' force-completed.");
            }
            else if (parts.Length >= 2)
            {
                qm.ActivateQuest(parts[1]);
                Log($"Quest '{parts[1]}' activated.");
            }
            else Log("Usage: quest [id] or quest complete [id]");
        }

        void HandleTutorial(string[] parts)
        {
            var ts = TutorialSystem.Instance;
            if (ts == null) { Log("TutorialSystem not found."); return; }

            if (parts.Length < 2) { Log("Usage: tutorial skip|reset"); return; }

            if (parts[1] == "skip")
            {
                foreach (TutorialStep step in System.Enum.GetValues(typeof(TutorialStep)))
                    ts.ForceComplete(step);
                Log("All tutorial steps completed.");
            }
            else if (parts[1] == "reset")
            {
                ts.ResetTutorial();
                Log("Tutorial reset.");
            }
        }

        void HandleState(string[] parts)
        {
            if (parts.Length < 2) { Log("Usage: state Exploration|Combat|Tuning|Cinematic|Paused|Menu"); return; }

            if (System.Enum.TryParse<GameState>(parts[1], true, out var state))
            {
                GameStateManager.Instance?.TransitionTo(state);
                Log($"GameState -> {state}");
            }
            else Log($"Unknown state: {parts[1]}");
        }

        void ShowHelp()
        {
            Log("--- COMMANDS ---");
            Log("rs [n] / rs +[n] -- Set/add Resonance Score");
            Log("aether [n]       -- Set Aether charge");
            Log("tp [x] [y] [z]   -- Teleport player");
            Log("spawn enemy      -- Spawn enemy ahead");
            Log("quest [id]       -- Activate quest");
            Log("quest complete [id] -- Force-complete quest");
            Log("tutorial skip    -- Complete all tutorials");
            Log("tutorial reset   -- Reset tutorial");
            Log("state [name]     -- Set GameState");
            Log("save / load      -- Force save/load");
            Log("god              -- Toggle invulnerability");
            Log("speed [mult]     -- Time scale (0.1-10)");
            Log("clear            -- Clear console");
        }
    }
}
