using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Gameplay;
using Tartaria.Data;

namespace Tartaria.UI
{
    /// <summary>
    /// Agent 11 UI Integration: Displays player level, XP progress, and available stat points.
    /// Self-bootstraps after scene loads, minimal IMGUI overlay (top-right corner).
    /// 
    /// Shows:
    /// - Level X (XP: current / required)
    /// - Stat Points: N available
    /// - Simple progress bar for XP
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStatsOverlay : MonoBehaviour
    {
        static PlayerStatsOverlay _instance;

        PlayerProgression _progression;
        float _displayedXPFraction;
        
        Coroutine _rebindCoroutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) { _instance.RebindPlayer(); return; }
            var go = new GameObject("PlayerStatsOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerStatsOverlay>();
            SceneManager.sceneLoaded += (s, m) => { if (_instance != null) _instance.RebindPlayer(); };
        }

        void OnEnable() => RebindPlayer();

        void OnDisable()
        {
            if (_rebindCoroutine != null)
            {
                StopCoroutine(_rebindCoroutine);
                _rebindCoroutine = null;
            }
        }

        void RebindPlayer()
        {
            // Defer one frame — PlayerProgression singleton may not exist yet
            if (_rebindCoroutine != null) StopCoroutine(_rebindCoroutine);
            _rebindCoroutine = StartCoroutine(RebindNextFrame());
        }

        System.Collections.IEnumerator RebindNextFrame()
        {
            yield return null;
            _progression = PlayerProgression.Instance;
            if (_progression != null)
            {
                int xpReq = _progression.GetXPRequiredForNextLevel();
                _displayedXPFraction = xpReq > 0 ? (float)_progression.CurrentXP / xpReq : 0f;
            }
        }

        void Update()
        {
            if (_progression != null)
            {
                int xpReq = _progression.GetXPRequiredForNextLevel();
                float targetFrac = xpReq > 0 ? (float)_progression.CurrentXP / xpReq : 0f;
                _displayedXPFraction = Mathf.MoveTowards(_displayedXPFraction, targetFrac, Time.unscaledDeltaTime * 1.5f);
            }
        }

        void OnGUI()
        {
            if (_progression == null) return;

            const int W = 280, H = 65;
            int x = Screen.width - W - 24;
            int y = 24;

            // Background frame
            var prevC = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(x - 3, y - 3, W + 6, H + 6), Texture2D.whiteTexture);
            GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, W, H), Texture2D.whiteTexture);
            GUI.color = prevC;

            // Text style
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            // Level text
            int currentLvl = _progression.CurrentLevel;
            int currentXP = _progression.CurrentXP;
            int xpRequired = _progression.GetXPRequiredForNextLevel();
            int maxLvl = GameBalanceConfig.Instance.maxLevel;
            string levelText = currentLvl >= maxLvl 
                ? $"Level {currentLvl} (MAX)" 
                : $"Level {currentLvl}";
            
            GUI.Label(new Rect(x + 8, y + 6, W - 16, 20), levelText, labelStyle);

            // Stat points indicator (highlight if > 0)
            int statPts = _progression.AvailableStatPoints;
            var statStyle = new GUIStyle(labelStyle) { fontSize = 13 };
            if (statPts > 0)
            {
                statStyle.normal.textColor = new Color(1f, 0.85f, 0.2f); // Gold highlight
            }
            GUI.Label(new Rect(x + 8, y + 26, W - 16, 18), $"Stat Points: {statPts}", statStyle);

            // XP progress bar (only if not max level)
            if (currentLvl < maxLvl)
            {
                int barX = x + 8;
                int barY = y + 46;
                int barW = W - 16;
                int barH = 12;

                // Bar background
                GUI.color = new Color(0.15f, 0.1f, 0.1f, 0.9f);
                GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

                // Bar fill
                float frac = Mathf.Clamp01(_displayedXPFraction);
                GUI.color = new Color(0.3f, 0.7f, 0.95f); // Blue XP fill
                GUI.DrawTexture(new Rect(barX, barY, barW * frac, barH), Texture2D.whiteTexture);

                // XP numeric label
                var xpStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                GUI.color = Color.white;
                GUI.Label(new Rect(barX, barY, barW, barH), $"{currentXP} / {xpRequired}", xpStyle);
            }

            GUI.color = prevC;
        }
    }
}
