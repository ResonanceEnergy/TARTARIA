using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.UI
{
    /// <summary>
    /// Achievement List Overlay — J key / gamepad Select toggles a scrollable
    /// IMGUI panel showing every achievement's title, status, progress bar, and
    /// rewards. Hidden achievements display "???" until unlocked. Talks to
    /// Tartaria.Integration.AchievementSystem via reflection because UI cannot
    /// reference Integration directly.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchievementListOverlay : MonoBehaviour
    {
        public static AchievementListOverlay Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("AchievementListOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<AchievementListOverlay>();
        }

        bool _open;
        Vector2 _scroll;

        // Reflection cache.
        Type _sysType;
        PropertyInfo _instanceProp;
        PropertyInfo _definitionsProp;
        MethodInfo _isUnlocked;
        MethodInfo _getProgress;

        Type _defType;
        FieldInfo _idField, _titleField, _descField, _hiddenField, _aetherField, _rsField;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            CacheReflection();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void CacheReflection()
        {
            try
            {
                _sysType = Type.GetType("Tartaria.Integration.AchievementSystem, Tartaria.Integration");
                if (_sysType == null) return;
                _instanceProp = _sysType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                _definitionsProp = _sysType.GetProperty("Definitions", BindingFlags.Public | BindingFlags.Instance);
                _isUnlocked = _sysType.GetMethod("IsUnlocked", BindingFlags.Public | BindingFlags.Instance);
                _getProgress = _sysType.GetMethod("GetProgress", BindingFlags.Public | BindingFlags.Instance);

                _defType = _sysType.GetNestedType("AchievementDef");
                if (_defType != null)
                {
                    _idField = _defType.GetField("id");
                    _titleField = _defType.GetField("title");
                    _descField = _defType.GetField("description");
                    _hiddenField = _defType.GetField("hidden");
                    _aetherField = _defType.GetField("aetherReward");
                    _rsField = _defType.GetField("rsReward");
                }
            }
            catch { /* best effort */ }
        }

        void Update()
        {
            bool kb = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
            bool gp = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;
            if (kb || gp) _open = !_open;
        }

        void OnGUI()
        {
            if (!_open) return;
            if (_sysType == null) return;
            var sys = _instanceProp?.GetValue(null);
            if (sys == null) return;
            if (_definitionsProp?.GetValue(sys) is not IList defs) return;

            const float w = 540f;
            const float h = 480f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), "");

            int total = defs.Count;
            int unlockedCount = 0;
            for (int i = 0; i < total; i++)
            {
                var def = defs[i];
                string id = _idField?.GetValue(def) as string;
                if (!string.IsNullOrEmpty(id) &&
                    _isUnlocked?.Invoke(sys, new object[] { id }) is bool b && b)
                    unlockedCount++;
            }

            var hdr = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.92f, 0.6f) }
            };
            GUI.Label(new Rect(x, y + 8f, w, 28f),
                $"ACHIEVEMENTS  —  {unlockedCount} / {total}   (J to close)", hdr);

            var area = new Rect(x + 12f, y + 44f, w - 24f, h - 60f);
            GUILayout.BeginArea(area);
            _scroll = GUILayout.BeginScrollView(_scroll);

            var rowStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft };
            var titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, richText = true };
            var bodyStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            var italic = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic };

            for (int i = 0; i < total; i++)
            {
                var def = defs[i];
                if (def == null) continue;

                string id = _idField?.GetValue(def) as string ?? "";
                string title = _titleField?.GetValue(def) as string ?? "";
                string desc = _descField?.GetValue(def) as string ?? "";
                bool hiddenFlag = _hiddenField?.GetValue(def) is bool hf && hf;
                int aether = _aetherField?.GetValue(def) is int a ? a : 0;
                float rs = _rsField?.GetValue(def) is float r ? r : 0f;

                bool isUnlocked = _isUnlocked?.Invoke(sys, new object[] { id }) is bool u && u;
                bool hide = hiddenFlag && !isUnlocked;

                GUILayout.BeginVertical(rowStyle);
                string colorTag = isUnlocked ? "#ffd166" : "#9aa0a6";
                string state = isUnlocked ? "[UNLOCKED]" : "[LOCKED]";
                GUILayout.Label($"<color={colorTag}>[{id}] {(hide ? "???" : title)}</color>  <i>{state}</i>", titleStyle);

                if (!hide) GUILayout.Label(desc, bodyStyle);

                float p = _getProgress?.Invoke(sys, new object[] { id }) is float pf ? pf : 0f;
                if (!isUnlocked && p > 0f)
                {
                    var rect = GUILayoutUtility.GetRect(area.width - 30f, 10f);
                    GUI.Box(rect, "");
                    var fill = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(p), rect.height);
                    var c = GUI.color;
                    GUI.color = new Color(0.5f, 0.85f, 0.55f, 0.85f);
                    GUI.DrawTexture(fill, Texture2D.whiteTexture);
                    GUI.color = c;
                }

                if (!hide && (aether > 0 || rs > 0f))
                    GUILayout.Label($"Reward: +{aether} Aether, +{rs:0.#} RS", italic);

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
