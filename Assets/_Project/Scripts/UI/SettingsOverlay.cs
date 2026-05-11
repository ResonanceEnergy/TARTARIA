using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.UI
{
    /// <summary>
    /// Polish: Settings overlay (F10 toggle, also opened by Main Menu / Pause).
    /// Master volume, mouse sensitivity, colorblind mode, text scale.
    /// </summary>
    [DisallowMultipleComponent]
    public class SettingsOverlay : MonoBehaviour
    {
        const string PP_VOLUME = "TARTARIA_MasterVolume";
        const string PP_SENS   = "TARTARIA_MouseSens";
        const string PP_TEXT   = "TARTARIA_TextScale";
        const string PP_CB     = "TARTARIA_ColorblindMode";

        static SettingsOverlay _instance;
        public static bool IsOpen => _instance != null && _instance._visible;

        bool _visible;
        float _volume = 1f;
        float _sens = 1f;
        float _textScale = 1f;
        int _colorblind; // 0=None,1=Protanopia,2=Deuteranopia,3=Tritanopia
        readonly string[] _cbLabels = { "None", "Protanopia", "Deuteranopia", "Tritanopia" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("SettingsOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SettingsOverlay>();
        }

        void Awake()
        {
            _volume    = PlayerPrefs.GetFloat(PP_VOLUME, 1f);
            _sens      = PlayerPrefs.GetFloat(PP_SENS, 1f);
            _textScale = PlayerPrefs.GetFloat(PP_TEXT, 1f);
            _colorblind = PlayerPrefs.GetInt(PP_CB, 0);
            ApplyAll();
        }

        public static void Open()
        {
            if (_instance == null) Bootstrap();
            _instance!._visible = true;
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f10Key.wasPressedThisFrame)
                _visible = !_visible;
            if (_visible && kb != null && kb.escapeKey.wasPressedThisFrame)
                _visible = false;
        }

        void OnGUI()
        {
            if (!_visible) return;

            const int W = 480, H = 360;
            int x = (Screen.width - W) / 2;
            int y = (Screen.height - H) / 2;

            // Dim
            var c = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = c;

            GUI.Box(new Rect(x, y, W, H), "");
            var title = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            GUI.Label(new Rect(x, y + 8, W, 32), "SETTINGS", title);

            int row = y + 56;
            int lx = x + 24, sx = x + 180, sw = W - 220;

            GUI.Label(new Rect(lx, row, 160, 24), $"Master Volume: {_volume:P0}");
            float v = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _volume, 0f, 1f);
            if (!Mathf.Approximately(v, _volume)) { _volume = v; AudioListener.volume = _volume; PlayerPrefs.SetFloat(PP_VOLUME, _volume); }
            row += 40;

            GUI.Label(new Rect(lx, row, 160, 24), $"Mouse Sensitivity: {_sens:F2}");
            float s = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _sens, 0.25f, 3f);
            if (!Mathf.Approximately(s, _sens)) { _sens = s; PlayerPrefs.SetFloat(PP_SENS, _sens); }
            row += 40;

            GUI.Label(new Rect(lx, row, 160, 24), $"Text Scale: {_textScale:F2}x");
            float t = GUI.HorizontalSlider(new Rect(sx, row + 6, sw, 18), _textScale, 0.7f, 2f);
            if (!Mathf.Approximately(t, _textScale)) { _textScale = t; PlayerPrefs.SetFloat(PP_TEXT, _textScale); ApplyTextScale(); }
            row += 40;

            GUI.Label(new Rect(lx, row, 160, 24), "Colorblind Mode:");
            int newCB = _colorblind;
            for (int i = 0; i < _cbLabels.Length; i++)
            {
                if (GUI.Toggle(new Rect(sx + i * 80, row, 78, 22), _colorblind == i, _cbLabels[i]))
                    newCB = i;
            }
            if (newCB != _colorblind) { _colorblind = newCB; PlayerPrefs.SetInt(PP_CB, _colorblind); ApplyColorblind(); }
            row += 40;

            // Quit‑skip toggle for menu
            bool skip = PlayerPrefs.GetInt("TARTARIA_SkipMainMenu", 0) == 1;
            bool newSkip = GUI.Toggle(new Rect(lx, row, W - 48, 22), skip, "Skip main menu next launch (dev)");
            if (newSkip != skip) PlayerPrefs.SetInt("TARTARIA_SkipMainMenu", newSkip ? 1 : 0);
            row += 36;

            if (GUI.Button(new Rect(x + W - 120, y + H - 40, 96, 28), "Close"))
            {
                _visible = false;
                PlayerPrefs.Save();
            }
        }

        void ApplyAll()
        {
            AudioListener.volume = _volume;
            ApplyTextScale();
            ApplyColorblind();
        }

        void ApplyTextScale()
        {
            // Bridge to AccessibilityManager via reflection (asmdef-cycle safe).
            TryInvoke("Tartaria.UI.AccessibilityManager", "SetTextScale", _textScale);
        }

        void ApplyColorblind()
        {
            // ColorblindMode enum: 0=None,1=Protanopia,2=Deuteranopia,3=Tritanopia
            TryInvokeEnum("Tartaria.UI.AccessibilityManager", "SetColorblindMode", "Tartaria.UI.ColorblindMode", _colorblind);
        }

        static void TryInvoke(string typeName, string method, object arg)
        {
            try
            {
                var t = System.Type.GetType(typeName) ?? System.Type.GetType($"{typeName}, Tartaria.UI");
                if (t == null) return;
                var inst = t.GetProperty("Instance")?.GetValue(null);
                if (inst == null) return;
                t.GetMethod(method)?.Invoke(inst, new[] { arg });
            }
            catch { }
        }

        static void TryInvokeEnum(string typeName, string method, string enumTypeName, int value)
        {
            try
            {
                var t = System.Type.GetType(typeName) ?? System.Type.GetType($"{typeName}, Tartaria.UI");
                var et = System.Type.GetType(enumTypeName) ?? System.Type.GetType($"{enumTypeName}, Tartaria.UI");
                if (t == null || et == null) return;
                var inst = t.GetProperty("Instance")?.GetValue(null);
                if (inst == null) return;
                object enumVal = System.Enum.ToObject(et, value);
                t.GetMethod(method)?.Invoke(inst, new[] { enumVal });
            }
            catch { }
        }
    }
}
