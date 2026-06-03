using System;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// QuestObjectiveTrackerUI — top-right single-line objective HUD.
    /// Subscribes to GameEvents.OnQuestStatusChanged + GameEvents.OnBuildingRestored.
    /// Cross-asmdef-safe: avoids hard Tartaria.UI → Tartaria.Integration reference.
    /// </summary>
    public class QuestObjectiveTrackerUI : MonoBehaviour
    {
        const float REPAINT_PERIOD = 2.5f;
        const float WIDTH = 360f;
        const float HEIGHT = 90f;

        string _primaryTitle = "Awaken the Star Dome";
        string _subline = "Restore the first hero building.";
        float _nextRepaint;
        GUIStyle _titleStyle, _subStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindFirstObjectByType<QuestObjectiveTrackerUI>() != null) return;
            var go = new GameObject("__QuestObjectiveTrackerUI");
            DontDestroyOnLoad(go);
            go.AddComponent<QuestObjectiveTrackerUI>();
        }

        void OnEnable()
        {
            try { GameEvents.OnBuildingRestored += HandleBuildingRestored; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(QuestObjectiveTrackerUI)}] {nameof(OnEnable)} failed to subscribe to GameEvents.OnBuildingRestored: {ex.GetType().Name}: {ex.Message}\n  context: tracker will not refresh on building restoration; periodic Update poll still runs every {REPAINT_PERIOD}s\n{ex.StackTrace}");
                // Non-fatal: RecomputeFromState() polls PlayerPrefs every 2.5s, so the tracker still updates — just not instantly.
            }
            try { GameEvents.OnQuestStatusChanged += HandleQuestStatusChanged; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(QuestObjectiveTrackerUI)}] {nameof(OnEnable)} failed to subscribe to GameEvents.OnQuestStatusChanged: {ex.GetType().Name}: {ex.Message}\n  context: quest status changes will not push to tracker; falls back to restoration-count derived state\n{ex.StackTrace}");
                // Non-fatal: tracker falls back to RecomputeFromState (hero-building restoration count) for objective text.
            }
        }

        void OnDisable()
        {
            try { GameEvents.OnBuildingRestored -= HandleBuildingRestored; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(QuestObjectiveTrackerUI)}] {nameof(OnDisable)} failed to unsubscribe from GameEvents.OnBuildingRestored: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
            try { GameEvents.OnQuestStatusChanged -= HandleQuestStatusChanged; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(QuestObjectiveTrackerUI)}] {nameof(OnDisable)} failed to unsubscribe from GameEvents.OnQuestStatusChanged: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
        }

        void Update()
        {
            if (Time.unscaledTime >= _nextRepaint)
            {
                _nextRepaint = Time.unscaledTime + REPAINT_PERIOD;
                RecomputeFromState();
            }
        }

        void HandleBuildingRestored(string buildingId)
        {
            RecomputeFromState();
        }

        void HandleQuestStatusChanged(QuestStatusChangedEventArgs args)
        {
            if (args == null) return;
            if (args.newStatus == Tartaria.Core.Enums.QuestStatus.Active)
            {
                _primaryTitle = args.questId ?? "Quest";
                _subline = "Quest active";
            }
            else if (args.newStatus == Tartaria.Core.Enums.QuestStatus.Completed)
            {
                _subline = "Completed: " + (args.questId ?? "");
            }
        }

        void RecomputeFromState()
        {
            int restored = CountRestoredBuildings();
            if (restored <= 0)      { _primaryTitle = "Awaken the Star Dome";  _subline = "Restore the first hero building."; }
            else if (restored == 1) { _primaryTitle = "Awaken the Fountain";    _subline = "Restore the second hero building."; }
            else if (restored == 2) { _primaryTitle = "Raise the Spire";        _subline = "Place the mercury-ball finial during the 17th hour."; }
            else                    { _primaryTitle = "Moon 1 Complete";        _subline = "Rest at Bob's Inn to advance."; }
        }

        int CountRestoredBuildings()
        {
            int n = 0;
            string[] heroIds = { "EchohavenStarDome", "EchohavenHarmonicFountain", "EchohavenCrystalSpire" };
            foreach (var id in heroIds)
            {
                if (PlayerPrefs.GetInt("TARTARIA_Restored_" + id, 0) == 1) n++;
            }
            return n;
        }

        void OnGUI()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
                _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.55f);
                _subStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
                _subStyle.normal.textColor = new Color(0.95f, 0.92f, 0.85f);
            }
            float x = Screen.width - WIDTH - 16f;
            float y = 16f;
            GUI.Box(new Rect(x, y, WIDTH, HEIGHT), "");
            GUI.Label(new Rect(x + 12, y + 8,  WIDTH - 24, 24), _primaryTitle, _titleStyle);
            GUI.Label(new Rect(x + 12, y + 36, WIDTH - 24, 48), _subline,      _subStyle);
        }
    }
}
