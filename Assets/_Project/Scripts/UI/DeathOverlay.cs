using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Death Overlay — fullscreen IMGUI panel that fades in when PlayerHealth fires
    /// OnDeath. Auto-respawn after 4s, or skip with Space/Enter / Gamepad-South.
    /// Self-bootstraps and survives scene loads.
    /// </summary>
    [DisallowMultipleComponent]
    public class DeathOverlay : MonoBehaviour
    {
        public static DeathOverlay Instance { get; private set; }

        const float AutoRespawnSeconds = 4f;
        const float FadeSeconds = 0.6f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("DeathOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<DeathOverlay>();
        }

        bool _shown;
        float _shownAt;
        PlayerHealth _hooked;
        Texture2D _black;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _black = new Texture2D(1, 1);
            _black.SetPixel(0, 0, new Color(0f, 0f, 0f, 1f));
            _black.Apply();
        }

        void OnDestroy()
        {
            if (_hooked != null) _hooked.OnDeath -= HandleDeath;
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Lazy hook: PlayerHealth may spawn after us.
            if (_hooked == null)
            {
                _hooked = FindFirstObjectByType<PlayerHealth>();
                if (_hooked != null) _hooked.OnDeath += HandleDeath;
            }

            if (!_shown) return;

            float t = Time.unscaledTime - _shownAt;
            bool keyboard = Keyboard.current != null &&
                            (Keyboard.current.spaceKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame);
            bool gamepad  = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

            if (t >= AutoRespawnSeconds || keyboard || gamepad)
                DoRespawn();
        }

        void HandleDeath()
        {
            if (_shown) return;
            _shown = true;
            _shownAt = Time.unscaledTime;
            // Pause time so the world freezes for the death moment.
            Time.timeScale = 0f;
        }

        void DoRespawn()
        {
            _shown = false;
            Time.timeScale = 1f;
            if (_hooked != null) _hooked.Respawn();
        }

        void OnGUI()
        {
            if (!_shown) return;

            float t = Mathf.Clamp01((Time.unscaledTime - _shownAt) / FadeSeconds);
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.85f * t);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _black);
            GUI.color = new Color(1f, 1f, 1f, t);

            var title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 64,
                fontStyle = FontStyle.Bold,
            };
            title.normal.textColor = new Color(0.95f, 0.32f, 0.22f, t);
            GUI.Label(new Rect(0, Screen.height * 0.32f, Screen.width, 96f), "YOU DIED", title);

            float remaining = Mathf.Max(0f, AutoRespawnSeconds - (Time.unscaledTime - _shownAt));
            var sub = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
            };
            sub.normal.textColor = new Color(1f, 1f, 1f, t);
            GUI.Label(new Rect(0, Screen.height * 0.52f, Screen.width, 30f),
                $"Respawning in {remaining:0.0}s — press Space / A to respawn now", sub);

            GUI.color = prev;
        }
    }
}
