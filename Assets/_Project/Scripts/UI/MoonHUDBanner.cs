using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// B1 — Top-center IMGUI banner used by MoonBeatRunner to announce each beat.
    /// IMGUI on purpose: zero TMP/Canvas dependency so it can't be broken by
    /// a missing prefab. Auto fades in/out, single live instance, DontDestroyOnLoad.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonHUDBanner : MonoBehaviour
    {
        public static MoonHUDBanner Instance { get; private set; }

        const float FadeIn   = 0.4f;
        const float HoldDef  = 4.0f;
        const float FadeOut  = 0.6f;

        string _title;
        string _subtitle;
        Color  _tint = Color.white;
        float  _t0 = -10f;
        float  _hold = HoldDef;
        bool   _visible;

        GUIStyle _titleStyle;
        GUIStyle _subStyle;

        public static void Show(string title, string subtitle, Color tint, float hold = HoldDef)
        {
            EnsureInstance();
            Instance._title    = title;
            Instance._subtitle = subtitle;
            Instance._tint     = tint;
            Instance._t0       = Time.unscaledTime;
            Instance._hold     = hold;
            Instance._visible  = true;
        }

        public static void Hide()
        {
            if (Instance != null) Instance._visible = false;
        }

        static void EnsureInstance()
        {
            if (Instance != null) return;
            var go = new GameObject("MoonHUDBanner");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MoonHUDBanner>();
        }

        void OnGUI()
        {
            if (!_visible) return;
            EnsureStyles();
            float t = Time.unscaledTime - _t0;
            float total = FadeIn + _hold + FadeOut;
            if (t >= total) { _visible = false; return; }

            float alpha;
            if (t < FadeIn)            alpha = t / FadeIn;
            else if (t < FadeIn + _hold) alpha = 1f;
            else                       alpha = 1f - ((t - FadeIn - _hold) / FadeOut);
            alpha = Mathf.Clamp01(alpha);

            float w = Mathf.Min(720f, Screen.width * 0.85f);
            float h = 80f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.06f;

            // Backdrop
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f * alpha);
            GUI.DrawTexture(new Rect(x - 16, y - 8, w + 32, h + 16), Texture2D.whiteTexture);

            // Tint bar
            GUI.color = new Color(_tint.r, _tint.g, _tint.b, alpha);
            GUI.DrawTexture(new Rect(x - 16, y - 8, 6f, h + 16), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + w + 10, y - 8, 6f, h + 16), Texture2D.whiteTexture);

            // Title
            var c = _tint; c.a = alpha;
            _titleStyle.normal.textColor = c;
            GUI.Label(new Rect(x, y, w, 40f), _title ?? string.Empty, _titleStyle);

            // Subtitle
            _subStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f, alpha);
            GUI.Label(new Rect(x, y + 38f, w, 40f), _subtitle ?? string.Empty, _subStyle);

            GUI.color = prev;
        }

        void EnsureStyles()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    richText  = true,
                };
            }
            if (_subStyle == null)
            {
                _subStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    richText  = true,
                };
            }
        }
    }
}
