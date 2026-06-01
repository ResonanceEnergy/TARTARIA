using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1FirstTimeHints — one-shot HUD prompts that fire when the player first
    /// encounters each Moon 1 system. Each hint persists a PlayerPrefs key so it
    /// never shows twice across sessions.
    ///
    /// Hints (priority order):
    ///   1. "Push LEFT STICK or WASD to walk"           — at 5s if no input registered
    ///   2. "Press Y / TAB to toggle Aether Vision"     — at 8s if Vision not toggled yet
    ///   3. "Press A / E to interact"                   — when an InteractableBuilding is in range
    ///   4. "Press RB / Right-click for Harmonic Strike"— when a Mud Golem is within 15 m
    ///
    /// Single instance, auto-bootstraps on Echohaven scene load. Routes through
    /// ServiceLocator.HUD.ShowBanner(title, body, duration) when available, else
    /// falls back to a self-rendered OnGUI banner.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-40)]
    public class Moon1FirstTimeHints : MonoBehaviour
    {
        static Moon1FirstTimeHints _instance;

        const string KEY_MOVE   = "TARTARIA_M1_Hint_Move";
        const string KEY_VISION = "TARTARIA_M1_Hint_Vision";
        const string KEY_E      = "TARTARIA_M1_Hint_Interact";
        const string KEY_STRIKE = "TARTARIA_M1_Hint_Strike";

        const float HINT_DURATION = 5f;

        bool _hintMoveShown, _hintVisionShown, _hintInteractShown, _hintStrikeShown;
        float _sceneTime;
        bool _inputSeen;
        string _activeBanner;
        float _bannerExpire;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1FirstTimeHints");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1FirstTimeHints>();
        }

        void Awake()
        {
            _hintMoveShown     = PlayerPrefs.GetInt(KEY_MOVE,   0) == 1;
            _hintVisionShown   = PlayerPrefs.GetInt(KEY_VISION, 0) == 1;
            _hintInteractShown = PlayerPrefs.GetInt(KEY_E,      0) == 1;
            _hintStrikeShown   = PlayerPrefs.GetInt(KEY_STRIKE, 0) == 1;
        }

        void OnEnable()
        {
            GameEvents.OnToggleAetherVision += HandleVisionToggledParameterless;
        }

        void OnDisable()
        {
            GameEvents.OnToggleAetherVision -= HandleVisionToggledParameterless;
        }

        void Update()
        {
            _sceneTime += Time.deltaTime;

            // Detect first real input
            if (!_inputSeen)
            {
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                if ((kb != null && kb.anyKey.isPressed) ||
                    (pad != null && pad.leftStick.ReadValue().sqrMagnitude > 0.05f))
                {
                    _inputSeen = true;
                    if (!_hintMoveShown) MarkShown(KEY_MOVE, ref _hintMoveShown);
                }
#endif
            }

            // Hint 1: movement (only if no input after 5s)
            if (!_hintMoveShown && !_inputSeen && _sceneTime >= 5f)
            {
                ShowHint("Move", "Push the LEFT STICK or use WASD to walk.");
                MarkShown(KEY_MOVE, ref _hintMoveShown);
            }

            // Hint 2: Aether Vision (after 8s if still un-toggled)
            if (!_hintVisionShown && _sceneTime >= 8f)
            {
                ShowHint("Aether Vision", "Press Y on the controller or TAB on the keyboard to reveal buried structures.");
                MarkShown(KEY_VISION, ref _hintVisionShown);
            }

            // Hint 3: interactable proximity
            if (!_hintInteractShown && PlayerNearInteractable())
            {
                ShowHint("Interact", "Press A on the controller or E on the keyboard to engage.");
                MarkShown(KEY_E, ref _hintInteractShown);
            }

            // Hint 4: enemy proximity
            if (!_hintStrikeShown && EnemyWithin(15f))
            {
                ShowHint("Combat", "Press RB / Right-click for a Harmonic Strike. LT / Ctrl raises the Frequency Shield.");
                MarkShown(KEY_STRIKE, ref _hintStrikeShown);
            }

            if (_activeBanner != null && Time.unscaledTime > _bannerExpire) _activeBanner = null;
        }

        void HandleVisionToggledParameterless()
        {
            if (!_hintVisionShown)
            {
                MarkShown(KEY_VISION, ref _hintVisionShown);
            }
        }

        bool PlayerNearInteractable()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return false;
            var hits = Physics.OverlapSphere(player.transform.position, 5f);
            foreach (var h in hits)
            {
                if (h.GetComponentInParent<InteractableBuilding>() != null) return true;
            }
            return false;
        }

        bool EnemyWithin(float radius)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return false;
            var hits = Physics.OverlapSphere(player.transform.position, radius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy") || h.name.Contains("Golem") || h.name.Contains("MudGolem")) return true;
            }
            return false;
        }

        void ShowHint(string title, string body)
        {
            try
            {
                var hud = ServiceLocator.HUD;
                if (hud != null)
                {
                    hud.ShowBanner(title, body, HINT_DURATION);
                    return;
                }
            }
            catch { /* fall through */ }
            _activeBanner = title + ": " + body;
            _bannerExpire = Time.unscaledTime + HINT_DURATION;
        }

        void MarkShown(string key, ref bool flag)
        {
            flag = true;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        void OnGUI()
        {
            if (string.IsNullOrEmpty(_activeBanner)) return;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = new Color(1f, 0.95f, 0.7f);
            float w = 720f, h = 60f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 90f;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.Label(new Rect(x, y, w, h), _activeBanner, style);
        }

        /// <summary>Debug helper — clear all hint flags so they show again.</summary>
        public static void ResetAllHints()
        {
            PlayerPrefs.DeleteKey(KEY_MOVE);
            PlayerPrefs.DeleteKey(KEY_VISION);
            PlayerPrefs.DeleteKey(KEY_E);
            PlayerPrefs.DeleteKey(KEY_STRIKE);
            PlayerPrefs.Save();
            Debug.Log("[Moon1FirstTimeHints] All hint flags cleared.");
        }
    }
}
