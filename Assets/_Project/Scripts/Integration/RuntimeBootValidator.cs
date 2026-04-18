using UnityEngine;
using System.IO;
using System.Text;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime boot validator — runs at Start and logs the health of all critical systems.
    /// Attaches to the GameBootstrap scene. Shows a brief on-screen overlay in development builds.
    /// Green = system found, Red = missing, Yellow = found but not configured.
    ///
    /// Add to Boot scene or any persistent manager object.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class RuntimeBootValidator : MonoBehaviour
    {
        public static RuntimeBootValidator Instance { get; private set; }

        [SerializeField, Tooltip("Show on-screen validation overlay in dev builds")]
        bool showOverlay = true;

        [SerializeField, Tooltip("How long overlay stays visible (seconds)")]
        float overlayDuration = 8f;

        string _report;
        float _overlayTimer;
        int _passed;
        int _failed;
        bool _validated;
        static GUIStyle _overlayStyle;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            // Wait for scene loading to complete before validating
            StartCoroutine(WaitThenValidate());
        }

        System.Collections.IEnumerator WaitThenValidate()
        {
            // Wait until SceneLoader transitions to Exploration (all scenes loaded)
            while (GameStateManager.Instance == null ||
                   GameStateManager.Instance.CurrentState != GameState.Exploration)
                yield return null;

            // Extra frame for spawners/controllers to initialize
            yield return null;
            RunValidation();
        }

        void RunValidation()
        {
            var sb = new StringBuilder();
            _passed = 0;
            _failed = 0;

            sb.AppendLine("=== TARTARIA BOOT VALIDATION ===");
            sb.AppendLine();

            // Core singletons
            Check(sb, "GameStateManager", GameStateManager.Instance != null);
            Check(sb, "GameLoopController", GameLoopController.Instance != null);
            Check(sb, "SceneLoader", FindAnyObjectByType<SceneLoader>() != null);
            Check(sb, "PlayerSpawner", FindAnyObjectByType<PlayerSpawner>() != null);
            Check(sb, "BuildingSpawner", FindAnyObjectByType<BuildingSpawner>() != null);

            // Gameplay managers
            Check(sb, "TutorialSystem", TutorialSystem.Instance != null);
            Check(sb, "QuestManager", QuestManager.Instance != null);
            Check(sb, "DialogueManager", DialogueManager.Instance != null);
            Check(sb, "CampaignFlowController", CampaignFlowController.Instance != null);
            Check(sb, "ZoneTransitionSystem", ZoneTransitionSystem.Instance != null);
            Check(sb, "CombatWaveManager", CombatWaveManager.Instance != null);
            Check(sb, "WorkshopSystem", WorkshopSystem.Instance != null);

            // UI
            Check(sb, "UIManager", UI.UIManager.Instance != null);
            Check(sb, "HUDController", UI.HUDController.Instance != null);

            // Audio
            Check(sb, "AudioManager", Audio.AudioManager.Instance != null);

            // Save
            Check(sb, "SaveManager", Save.SaveManager.Instance != null);

            // Gameplay systems
            Check(sb, "EconomySystem", EconomySystem.Instance != null);
            Check(sb, "SkillTreeSystem", Gameplay.SkillTreeSystem.Instance != null);
            Check(sb, "ResonanceScanner", Gameplay.ResonanceScannerSystem.Instance != null);

            // Player
            var player = GameObject.FindWithTag("Player");
            Check(sb, "Player (tagged)", player != null);
            if (player != null)
            {
                Check(sb, "PlayerInputHandler", player.GetComponent<Input.PlayerInputHandler>() != null);
                Check(sb, "CharacterController", player.GetComponent<CharacterController>() != null);
            }

            // Camera
            var cam = UnityEngine.Camera.main;
            if (cam == null)
                cam = FindAnyObjectByType<UnityEngine.Camera>();
            Check(sb, "Main Camera", cam != null);
            Check(sb, "CameraController", FindAnyObjectByType<Tartaria.Camera.CameraController>() != null);

            sb.AppendLine();
            sb.AppendLine($"RESULT: {_passed} passed, {_failed} failed");
            if (_failed == 0)
                sb.AppendLine("ALL SYSTEMS GO");
            else
                sb.AppendLine($"WARNING: {_failed} system(s) missing — gameplay may be impaired");

            _report = sb.ToString();
            _overlayTimer = overlayDuration;
            _validated = true;

            if (_failed == 0)
                Debug.Log($"[BootValidator] {_report}");
            else
                Debug.LogWarning($"[BootValidator] {_report}");

            // Write canary file for pipeline monitoring (bypasses Unity log buffering)
            try
            {
                string dir = Path.Combine(Application.dataPath, "_Project/Logs");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "boot-validator-canary.txt"),
                    $"passed={_passed}\nfailed={_failed}\ntimestamp={Time.realtimeSinceStartup:F2}\n" +
                    (_failed == 0 ? "ALL SYSTEMS GO\n" : $"WARNING: {_failed} system(s) missing\n") +
                    _report);
            }
            catch { /* ignore IO errors during diagnostics */ }
        }

        void Check(StringBuilder sb, string name, bool present)
        {
            string status = present ? "OK" : "MISSING";
            sb.AppendLine($"  [{status,-7}] {name}");
            if (present) _passed++; else _failed++;
        }

        void OnGUI()
        {
            _overlayStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                richText = false,
                alignment = TextAnchor.UpperLeft,
            };

            if (_validated && showOverlay && _overlayTimer > 0f)
            {
                _overlayTimer -= Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(_overlayTimer / 2f);
                bool allGood = _failed == 0;
                var bgColor = allGood
                    ? new Color(0f, 0.2f, 0f, 0.75f * alpha)
                    : new Color(0.3f, 0f, 0f, 0.85f * alpha);
                _overlayStyle.normal.textColor = new Color(1f, 1f, 1f, alpha);
                float width = 360f;
                float height = (_passed + _failed + 6) * 16f;
                var rect = new Rect(10f, 10f, width, height);
                GUI.color = bgColor;
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(14f, 12f, width - 8f, height - 4f), _report, _overlayStyle);
            }

            // Persistent debug HUD
            if (_validated)
            {
                var player = GameObject.FindWithTag("Player");
                var cam = UnityEngine.Camera.main;
                var pih = player != null ? player.GetComponent<Input.PlayerInputHandler>() : null;
                var gs = GameStateManager.Instance;
                string debugInfo = "=== TARTARIA DEBUG ===\n";
                debugInfo += $"State: {gs?.CurrentState}\n";
                debugInfo += $"IsPlaying: {gs?.IsPlaying}\n";
                debugInfo += $"Player: {(player != null ? player.transform.position.ToString("F1") : "NULL")}\n";
                debugInfo += $"Camera: {(cam != null ? cam.transform.position.ToString("F1") : "NULL")}\n";
                debugInfo += $"CamEnabled: {(cam != null ? cam.enabled.ToString() : "NULL")}\n";
                debugInfo += $"Moving: {(pih != null ? pih.IsMoving.ToString() : "?")}\n";
                debugInfo += $"FPS: {(1f / Time.unscaledDeltaTime):F0}\n";
                _overlayStyle.normal.textColor = Color.yellow;
                float dw = 300f;
                float dh = 130f;
                var dRect = new Rect(10f, Screen.height - dh - 10f, dw, dh);
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(dRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(14f, Screen.height - dh - 6f, dw - 8f, dh - 4f), debugInfo, _overlayStyle);
            }
        }
    }
}
