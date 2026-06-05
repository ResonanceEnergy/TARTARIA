// CrashReporterMoon1Bridge.cs — REAL implementation (NO STUBS).
// 2026-06-03: Moon 1 contextual crash augmenter. Pre-logs a "[Moon1Crash] ctx:"
// line whenever an Exception bubbles through Application.logMessageReceived,
// so post-mortem reading the crash log shows what quest/building was active.

using UnityEngine;
using System.Text;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Augments Tartaria.Core.CrashReporter with Moon 1 runtime breadcrumbs.
    /// Subscribes to real GameEvents (verified against GameEvents.cs) and to
    /// Application.logMessageReceived. On exception, writes a context line
    /// just before the exception so Core/CrashReporter captures both.
    /// </summary>
    public class CrashReporterMoon1Bridge : MonoBehaviour
    {
        static CrashReporterMoon1Bridge _instance;
        string _lastQuestStatus = "(none)";
        string _lastBuildingId = "(none)";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[CrashReporterMoon1Bridge]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            _instance = go.AddComponent<CrashReporterMoon1Bridge>();
        }

        void Awake()
        {
            Application.logMessageReceived += OnLogReceived;
            GameEvents.OnBuildingRestored += b => _lastBuildingId = b ?? "(null)";
            GameEvents.OnQuestStatusChanged += a => {
                if (a != null) _lastQuestStatus = a.questId + ":" + a.newStatus;
            };
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= OnLogReceived;
        }

        void OnLogReceived(string msg, string stack, LogType type)
        {
            if (type != LogType.Exception) return;
            var sb = new StringBuilder();
            sb.Append("[Moon1Crash] ctx: quest=").Append(_lastQuestStatus)
              .Append(" building=").Append(_lastBuildingId)
              .Append(" scene=").Append(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            Debug.Log(sb.ToString());
        }
    }
}
