using UnityEngine;
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
        [SerializeField, Tooltip("Show on-screen validation overlay in dev builds")]
        bool showOverlay = true;

        [SerializeField, Tooltip("How long overlay stays visible (seconds)")]
        float overlayDuration = 8f;

        string _report;
        float _overlayTimer;
        int _passed;
        int _failed;
        bool _validated;

        void Start()
        {
            // Delay validation by one frame to let all Awake/Start calls complete
            Invoke(nameof(RunValidation), 0.1f);
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
            Check(sb, "Main Camera", cam != null);
            if (cam != null)
                Check(sb, "CameraController", cam.GetComponent<Tartaria.Camera.CameraController>() != null
                    || cam.GetComponentInParent<Tartaria.Camera.CameraController>() != null);

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
        }

        void Check(StringBuilder sb, string name, bool present)
        {
            string status = present ? "OK" : "MISSING";
            sb.AppendLine($"  [{status,-7}] {name}");
            if (present) _passed++; else _failed++;
        }

        void OnGUI()
        {
            if (!_validated || !showOverlay || _overlayTimer <= 0f) return;
            _overlayTimer -= Time.unscaledDeltaTime;

            float alpha = Mathf.Clamp01(_overlayTimer / 2f); // Fade out last 2 seconds
            bool allGood = _failed == 0;

            // Background
            var bgColor = allGood
                ? new Color(0f, 0.2f, 0f, 0.75f * alpha)
                : new Color(0.3f, 0f, 0f, 0.85f * alpha);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                richText = false,
                alignment = TextAnchor.UpperLeft,
            };
            style.normal.textColor = new Color(1f, 1f, 1f, alpha);

            float width = 360f;
            float height = (_passed + _failed + 6) * 16f;
            var rect = new Rect(10f, 10f, width, height);

            GUI.color = bgColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(14f, 12f, width - 8f, height - 4f), _report, style);
        }
    }
}
