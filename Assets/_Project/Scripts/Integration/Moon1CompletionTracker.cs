// File: Assets/_Project/Scripts/Integration/Moon1CompletionTracker.cs
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    public class Moon1CompletionTracker : MonoBehaviour
    {
        const string MOON1_DONE_PREF = "TARTARIA_Moon1Complete";
        static readonly string[] HERO_BUILDINGS = { "echohaven_dome", "echohaven_fountain", "echohaven_spire" };
        
        HashSet<string> _restoredThisSession;
        float _startTimeSec;
        bool _alreadyCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("Moon1CompletionTracker");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1CompletionTracker>();
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestoredTyped += HandleBuildingRestored;
            _startTimeSec = Time.realtimeSinceStartup;
            _alreadyCompleted = PlayerPrefs.GetInt(MOON1_DONE_PREF, 0) == 1;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestoredTyped -= HandleBuildingRestored;
        }

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            if (_alreadyCompleted) return;
            _restoredThisSession.Add(args.buildingId);
            Debug.Log($"[Moon1CompletionTracker] Restoration count: {_restoredThisSession.Count}/3 — id={args.buildingId}");
            if (_restoredThisSession.Count == HERO_BUILDINGS.Length)
            {
                FireMoonComplete();
            }
        }

        void FireMoonComplete()
        {
            _alreadyCompleted = true;
            float dur = Time.realtimeSinceStartup - _startTimeSec;
            Debug.Log($"[Moon1CompletionTracker] MOON 1 COMPLETE — duration: {dur:F0}s");
            ServiceLocator.HUD?.ShowBanner(
                "MOON 1 COMPLETE",
                "The Listeners' Hall, the Pure Water Font, and the Cosmic Spire are restored. Rest at the Inn to begin the Lunar Moon.",
                12f);
            PlayerPrefs.SetInt(MOON1_DONE_PREF, 1);
            PlayerPrefs.Save();
        }
    }
}
