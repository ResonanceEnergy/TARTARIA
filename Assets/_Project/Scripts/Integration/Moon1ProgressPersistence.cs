using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 progress persistence — writes PlayerPrefs for:
    ///   - Per-building restored bool (TARTARIA_M1_Restored_<id>)
    ///   - Total RS earned (TARTARIA_M1_TotalRS)
    ///   - Total lore artifacts collected (TARTARIA_M1_Artifacts)
    ///   - Best mini-game accuracy (TARTARIA_M1_BestAccuracy)
    ///   - Moon 1 complete flag (TARTARIA_Moon1Complete) — set when all 3 hero buildings done
    ///   - Highest Aether band unlocked (TARTARIA_M1_HighestBand)
    /// Subscribes to GameEvents and auto-saves. Per CLAUDE.md "no stubs" — real
    /// PlayerPrefs.Save() calls, real event subscriptions, no TODO bodies.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1ProgressPersistence : MonoBehaviour
    {
        static Moon1ProgressPersistence _instance;

        const string KEY_TOTAL_RS       = "TARTARIA_M1_TotalRS";
        const string KEY_ARTIFACTS      = "TARTARIA_M1_Artifacts";
        const string KEY_BEST_ACCURACY  = "TARTARIA_M1_BestAccuracy";
        const string KEY_MOON1_DONE     = "TARTARIA_Moon1Complete";
        const string KEY_HIGHEST_BAND   = "TARTARIA_M1_HighestBand";

        int _restoredCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1ProgressPersistence");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1ProgressPersistence>();
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            GameEvents.OnRSChanged        += HandleRSChanged;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnRSChanged        -= HandleRSChanged;
        }

        void Start()
        {
            // On scene load — banner the player's previous state if non-zero
            int totalRS = PlayerPrefs.GetInt(KEY_TOTAL_RS, 0);
            int artifacts = PlayerPrefs.GetInt(KEY_ARTIFACTS, 0);
            if (totalRS > 0 || artifacts > 0)
            {
                ServiceLocator.HUD?.ShowBanner("Welcome back",
                    "Carry-over RS: " + totalRS + " — Artifacts: " + artifacts, 4f);
            }
        }

        void HandleBuildingRestored(string buildingId)
        {
            var key = "TARTARIA_M1_Restored_" + buildingId;
            PlayerPrefs.SetInt(key, 1);
            _restoredCount = CountRestoredFromPrefs();

            // Moon 1 complete after 3 hero buildings
            if (_restoredCount >= 3 && PlayerPrefs.GetInt(KEY_MOON1_DONE, 0) == 0)
            {
                PlayerPrefs.SetInt(KEY_MOON1_DONE, 1);
                ServiceLocator.HUD?.ShowBanner("Moon 1 — Echohaven", "All hero buildings restored. Rest at the Inn.", 8f);
            }

            PlayerPrefs.Save();
            Debug.Log("[Moon1ProgressPersistence] Saved: " + buildingId + " restored. Total: " + _restoredCount);
        }

        void HandleRSChanged(float delta)
        {
            if (delta <= 0f) return;
            int cur = PlayerPrefs.GetInt(KEY_TOTAL_RS, 0);
            PlayerPrefs.SetInt(KEY_TOTAL_RS, cur + Mathf.RoundToInt(delta));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Called by TuningMiniGame on completion with accuracy 0..1.
        /// Tracks the best run for "Mastery" achievement banner.
        /// </summary>
        public static void ReportTuningAccuracy(float accuracy)
        {
            float best = PlayerPrefs.GetFloat(KEY_BEST_ACCURACY, 0f);
            if (accuracy > best)
            {
                PlayerPrefs.SetFloat(KEY_BEST_ACCURACY, accuracy);
                PlayerPrefs.Save();
                if (accuracy >= 0.92f)
                    ServiceLocator.HUD?.ShowBanner("Mastery", "New best tuning accuracy: " + Mathf.RoundToInt(accuracy * 100f) + "%", 4f);
            }
        }

        int CountRestoredFromPrefs()
        {
            int n = 0;
            // Known hero IDs
            string[] heroIds = { "EchohavenStarDome", "EchohavenHarmonicFountain", "EchohavenCrystalSpire",
                                 "Cathedral", "HarmonicFountain", "CrystalSpire",
                                 "StarDome", "Fountain", "Spire" };
            foreach (var id in heroIds)
            {
                if (PlayerPrefs.GetInt("TARTARIA_M1_Restored_" + id, 0) == 1) n++;
            }
            return n;
        }

        public static int GetTotalRS() => PlayerPrefs.GetInt(KEY_TOTAL_RS, 0);
        public static int GetArtifactCount() => PlayerPrefs.GetInt(KEY_ARTIFACTS, 0);
        public static float GetBestAccuracy() => PlayerPrefs.GetFloat(KEY_BEST_ACCURACY, 0f);
        public static bool IsMoon1Complete() => PlayerPrefs.GetInt(KEY_MOON1_DONE, 0) == 1;
        public static string GetHighestBand() => PlayerPrefs.GetString(KEY_HIGHEST_BAND, "");

        public static void IncrementArtifactCount()
        {
            int cur = PlayerPrefs.GetInt(KEY_ARTIFACTS, 0);
            PlayerPrefs.SetInt(KEY_ARTIFACTS, cur + 1);
            PlayerPrefs.Save();
        }

        public static void SetHighestBand(string band)
        {
            PlayerPrefs.SetString(KEY_HIGHEST_BAND, band);
            PlayerPrefs.Save();
        }
    }
}
