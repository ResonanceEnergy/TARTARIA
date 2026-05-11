using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Polish: Achievement / unlock toast pop-ups in the bottom-right.
    /// Other systems call AchievementToastOverlay.Show("Title", "Subtitle") to surface a notification.
    /// Hooks AchievementSystem unlock event via reflection so no asmdef edge is required.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchievementToastOverlay : MonoBehaviour
    {
        static AchievementToastOverlay _instance;

        struct Toast { public string title; public string sub; public float born; }
        readonly List<Toast> _queue = new();
        const float TOAST_LIFE = 4.5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("AchievementToastOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AchievementToastOverlay>();
            _instance.HookAchievementSystem();
        }

        public static void Show(string title, string subtitle = "")
        {
            if (_instance == null) Bootstrap();
            _instance!._queue.Add(new Toast { title = title, sub = subtitle, born = Time.unscaledTime });
        }

        void HookAchievementSystem()
        {
            try
            {
                var t = System.Type.GetType("Tartaria.Gameplay.AchievementSystem, Tartaria.Gameplay")
                       ?? System.Type.GetType("Tartaria.Gameplay.AchievementSystem");
                if (t == null) return;
                var inst = t.GetProperty("Instance")?.GetValue(null);
                if (inst == null) return;
                var ev = t.GetEvent("OnAchievementUnlocked");
                if (ev == null) return;
                var handler = new System.Action<string>(id => Show("Achievement Unlocked", id));
                var del = System.Delegate.CreateDelegate(ev.EventHandlerType, handler.Target, handler.Method, false);
                if (del != null) ev.AddEventHandler(inst, del);
            }
            catch { /* best effort */ }
        }

        void Update()
        {
            float now = Time.unscaledTime;
            _queue.RemoveAll(t => now - t.born > TOAST_LIFE);
        }

        void OnGUI()
        {
            if (_queue.Count == 0) return;
            const int W = 320, H = 56, gap = 6;
            int x = Screen.width - W - 24;
            int y = Screen.height - 120;
            float now = Time.unscaledTime;
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                var t = _queue[i];
                float age = now - t.born;
                float alpha = Mathf.Clamp01(Mathf.Min(age * 4f, (TOAST_LIFE - age) * 2f));
                var col = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.78f * alpha);
                GUI.DrawTexture(new Rect(x, y, W, H), Texture2D.whiteTexture);
                GUI.color = new Color(1f, 0.92f, 0.5f, alpha);
                var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
                GUI.Label(new Rect(x + 12, y + 6, W - 24, 22), t.title, titleStyle);
                GUI.color = new Color(0.95f, 0.95f, 0.95f, alpha);
                GUI.Label(new Rect(x + 12, y + 26, W - 24, 22), t.sub);
                GUI.color = col;
                y -= H + gap;
            }
        }
    }
}
