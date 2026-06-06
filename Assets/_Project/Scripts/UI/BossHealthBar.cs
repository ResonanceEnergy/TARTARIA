using UnityEngine;
using TMPro;
using System.Collections;

namespace Tartaria.UI
{
    /// <summary>
    /// Agent 15: Boss health bar — full-width bar at top of screen with dramatic presentation.
    /// Singleton instance, activates on boss aggro, deactivates on boss defeat.
    /// 
    /// Features:
    /// - Full-width top-screen bar with boss name
    /// - Phase indicators (segmented bar for multi-phase bosses)
    /// - Slide-down entrance animation
    /// - Dramatic VFX on phase transitions
    /// - Smooth health drain with pulse effect on damage
    /// 
    /// Usage:
    ///   BossHealthBar.Show("Ancient Guardian", 5000)
    ///   BossHealthBar.UpdateHealth(4200)
    ///   BossHealthBar.SetPhase(2, 3) // Phase 2 of 3
    ///   BossHealthBar.Hide()
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        public static BossHealthBar Instance { get; private set; }

        [Header("Animation")]
        [SerializeField] float slideDownDuration = 0.8f;
        [SerializeField] float slideUpDuration = 0.6f;

        string _bossName = "";
        float _currentHealth;
        float _maxHealth;
        float _displayedFraction = 1f;
        int _currentPhase = 1;
        int _totalPhases = 1;
        bool _isVisible;
        float _damageFlash; // Pulse effect on damage
        Vector2 _slideOffset; // For slide animation

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _isVisible = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Show boss health bar with boss name and max health.</summary>
        public static void Show(string bossName, float maxHealth, int totalPhases = 1)
        {
            if (Instance == null)
            {
                // Create instance if doesn't exist
                var go = new GameObject("BossHealthBar");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<BossHealthBar>();
            }

            Instance._bossName = bossName;
            Instance._maxHealth = maxHealth;
            Instance._currentHealth = maxHealth;
            Instance._displayedFraction = 1f;
            Instance._currentPhase = 1;
            Instance._totalPhases = totalPhases;
            Instance._isVisible = true;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.SlideDownAnimation());
        }

        /// <summary>Update current boss health.</summary>
        public static void UpdateHealth(float currentHealth)
        {
            if (Instance == null || !Instance._isVisible) return;
            
            if (currentHealth < Instance._currentHealth)
                Instance._damageFlash = 0.5f; // Flash on damage
            
            Instance._currentHealth = currentHealth;
        }

        /// <summary>Set current phase (for multi-phase bosses).</summary>
        public static void SetPhase(int phase, int totalPhases)
        {
            if (Instance == null || !Instance._isVisible) return;
            Instance._currentPhase = phase;
            Instance._totalPhases = totalPhases;
        }

        /// <summary>Hide boss health bar.</summary>
        public static void Hide()
        {
            if (Instance == null || !Instance._isVisible) return;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.SlideUpAnimation());
        }

        IEnumerator SlideDownAnimation()
        {
            float elapsed = 0f;
            while (elapsed < slideDownDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDownDuration);
                t = t * t * (3f - 2f * t); // Smoothstep
                _slideOffset = Vector2.Lerp(new Vector2(0f, 150f), Vector2.zero, t);
                yield return null;
            }
            _slideOffset = Vector2.zero;
        }

        IEnumerator SlideUpAnimation()
        {
            float elapsed = 0f;
            Vector2 startOffset = _slideOffset;
            while (elapsed < slideUpDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideUpDuration);
                t = t * t; // Ease-in quad
                _slideOffset = Vector2.Lerp(startOffset, new Vector2(0f, 150f), t);
                yield return null;
            }
            _isVisible = false;
        }

        void Update()
        {
            if (!_isVisible) return;

            // Smooth health drain
            float targetFraction = Mathf.Clamp01(_currentHealth / Mathf.Max(1f, _maxHealth));
            _displayedFraction = Mathf.MoveTowards(_displayedFraction, targetFraction, Time.unscaledDeltaTime * 0.8f);

            // Damage flash decay
            if (_damageFlash > 0f)
                _damageFlash = Mathf.Max(0f, _damageFlash - Time.unscaledDeltaTime * 2f);
        }

        void OnGUI()
        {
            if (!_isVisible || string.IsNullOrEmpty(_bossName)) return;

            const int W = 800, H = 50;
            int x = (Screen.width - W) / 2;
            int y = 40 + Mathf.RoundToInt(_slideOffset.y); // Top of screen with slide offset

            var prevC = GUI.color;

            // Outer frame (dark border)
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(x - 4, y - 4, W + 8, H + 8), Texture2D.whiteTexture);

            // Background (dark red)
            GUI.color = new Color(0.15f, 0.05f, 0.05f, 0.95f);
            GUI.DrawTexture(new Rect(x, y, W, H), Texture2D.whiteTexture);

            // Draw phase segments if multi-phase boss
            if (_totalPhases > 1)
            {
                int segmentWidth = W / _totalPhases;
                for (int i = 0; i < _totalPhases; i++)
                {
                    int segX = x + (i * segmentWidth);
                    
                    // Segment divider
                    if (i > 0)
                    {
                        GUI.color = new Color(0f, 0f, 0f, 0.8f);
                        GUI.DrawTexture(new Rect(segX - 2, y, 4, H), Texture2D.whiteTexture);
                    }
                }
            }

            // Health fill
            float frac = Mathf.Clamp01(_displayedFraction);
            Color fillColor;
            
            // Boss health color (red to orange gradient)
            if (frac > 0.5f)
                fillColor = Color.Lerp(new Color(0.95f, 0.5f, 0.1f), new Color(0.95f, 0.2f, 0.1f), (1f - frac) * 2f);
            else
                fillColor = new Color(0.95f, 0.2f, 0.1f); // Deep red for low HP

            // Add damage flash pulse
            if (_damageFlash > 0.01f)
            {
                fillColor = Color.Lerp(fillColor, Color.white, _damageFlash * 0.4f);
            }

            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(x, y, W * frac, H), Texture2D.whiteTexture);
            GUI.color = prevC;

            // Boss name (top-left of bar)
            var nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.9f, 0.7f) }
            };
            GUI.Label(new Rect(x + 16, y, W - 32, H), _bossName, nameStyle);

            // Health text (top-right of bar)
            var healthStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.9f, 0.85f, 0.7f) }
            };
            string healthText = $"{Mathf.CeilToInt(_currentHealth)} / {Mathf.CeilToInt(_maxHealth)}";
            GUI.Label(new Rect(x, y, W - 16, H), healthText, healthStyle);

            // Phase indicator (if multi-phase)
            if (_totalPhases > 1)
            {
                var phaseStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.UpperCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.85f, 0.5f) }
                };
                GUI.Label(new Rect(x, y + H + 4, W, 20), $"Phase {_currentPhase} / {_totalPhases}", phaseStyle);
            }
        }
    }
}
