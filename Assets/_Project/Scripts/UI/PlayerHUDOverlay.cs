using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Gameplay;

namespace Tartaria.UI
{
    /// <summary>
    /// Day-5: Always-on player HP overlay. Self-bootstraps after every scene
    /// load, finds the local PlayerHealth, and renders a chunky bottom-left
    /// HP bar with damage flash + low-HP pulse via IMGUI (no prefab wiring).
    ///
    /// Companion to HUDController (which owns RS / objective text).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHUDOverlay : MonoBehaviour
    {
        static PlayerHUDOverlay _instance;

        PlayerHealth _health;
        float _flashAlpha;          // red screen flash on damage
        float _displayedFraction;   // smoothed bar fill
        int _lastHp = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) { _instance.RebindPlayer(); return; }
            var go = new GameObject("PlayerHUDOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerHUDOverlay>();
            SceneManager.sceneLoaded += (s, m) => { if (_instance != null) _instance.RebindPlayer(); };
        }

        void OnEnable() => RebindPlayer();

        void RebindPlayer()
        {
            // Defer one frame — Player may not exist yet when scene loads
            StartCoroutine(RebindNextFrame());
        }

        System.Collections.IEnumerator RebindNextFrame()
        {
            yield return null;
            if (_health != null) _health.OnHealthChanged -= OnHealthChanged;
            var p = GameObject.FindGameObjectWithTag("Player");
            _health = p != null ? p.GetComponent<PlayerHealth>() : null;
            if (_health != null)
            {
                _health.OnHealthChanged += OnHealthChanged;
                _displayedFraction = (float)_health.CurrentHealth / Mathf.Max(1, _health.MaxHealth);
                _lastHp = _health.CurrentHealth;
            }
        }

        void OnHealthChanged(int current, int max)
        {
            if (_lastHp >= 0 && current < _lastHp)
                _flashAlpha = 0.55f;     // red flash on damage
            _lastHp = current;
        }

        void Update()
        {
            if (_flashAlpha > 0f)
                _flashAlpha = Mathf.Max(0f, _flashAlpha - Time.unscaledDeltaTime * 1.6f);

            if (_health != null)
            {
                float target = (float)_health.CurrentHealth / Mathf.Max(1, _health.MaxHealth);
                _displayedFraction = Mathf.MoveTowards(_displayedFraction, target, Time.unscaledDeltaTime * 1.2f);
            }
        }

        void OnGUI()
        {
            // Damage flash overlay (drawn before bar so bar sits on top)
            if (_flashAlpha > 0.01f)
            {
                var prev = GUI.color;
                GUI.color = new Color(0.85f, 0.1f, 0.1f, _flashAlpha * 0.35f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = prev;
            }

            if (_health == null) return;

            const int W = 320, H = 30;
            int x = 24, y = Screen.height - H - 24;

            // Frame
            var prevC = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(x - 3, y - 3, W + 6, H + 6), Texture2D.whiteTexture);

            // Empty track
            GUI.color = new Color(0.15f, 0.05f, 0.05f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, W, H), Texture2D.whiteTexture);

            // Fill — colour shifts red as it depletes; pulses when below 25%
            float frac = Mathf.Clamp01(_displayedFraction);
            Color fill;
            if (frac > 0.5f)      fill = Color.Lerp(new Color(0.95f, 0.75f, 0.2f), new Color(0.4f, 0.95f, 0.4f), (frac - 0.5f) * 2f);
            else if (frac > 0.25f) fill = Color.Lerp(new Color(0.95f, 0.4f, 0.1f), new Color(0.95f, 0.75f, 0.2f), (frac - 0.25f) * 4f);
            else
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 8f);
                fill = new Color(0.95f, 0.15f, 0.15f) * pulse;
                fill.a = 1f;
            }
            GUI.color = fill;
            GUI.DrawTexture(new Rect(x, y, W * frac, H), Texture2D.whiteTexture);
            GUI.color = prevC;

            // Numeric text
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(x, y, W, H), $"HP  {_health.CurrentHealth} / {_health.MaxHealth}", style);
        }
    }
}
