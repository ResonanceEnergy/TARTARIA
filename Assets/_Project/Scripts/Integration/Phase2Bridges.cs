// Phase2Bridges.cs — Integration-layer shims rebuilt 2026-06-03 (NATRIX NO-STUBS mandate).
// Renamed 2026-06-04 from Phase2Stubs.cs (Sprint REORG-4). The "Stubs" name was a lie —
// every `[Stub]` Debug.Log has been replaced with a real implementation that talks to
// existing systems (GameEvents, ServiceLocator, EconomySystem, RunProgressTracker,
// PlayerPrefs persistence, CameraController shake). The .meta GUID was preserved via
// `git mv` so prefab/scene references continue to resolve.
//
// Removed (zero active callers):
//   * GameEventsPhase2 static class + duplicate EventArgs types
//   * ResonanceScannerSystemStub extensions (callers migrated to real ScanPOI signature)
//   * LeanTweenStub, AirshipFleetManager, TutorialStepExtended, ResonanceShardComponent,
//     TutorialSaveData, QuestReward, ZoneEventArgs, TuningNodeEventArgsLegacy
//
// Kept and implemented for real:
//   * CameraShakeController       — routes Shake() through ServiceLocator.CameraShake
//   * AchievementSystem           — Unlock + IsUnlocked + GetProgress + Definitions + event
//                                   + PlayerPrefs persistence for the unlocked set
//   * GameLoopController          — AwardRS/QueueRSReward fire GameEvents.FireRSChange
//                                   so RunProgressTracker can persist; GetCurrentRS
//                                   reads from RunProgressTracker (or local fallback)
//   * MoonProgressTracker         — IsCleared/MarkMoonCleared backed by PlayerPrefs
//                                   so Echohaven Obelisk shows real progress across runs;
//                                   raises GameEvents.RaiseMoonUnlocked + OnMoonCleared
//   * VFXManager                  — kept (RuntimeVFXSetup writes prefab refs via reflection)
using System;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    // ─────────────────────────────────────────────────────────────────────
    // Camera shake — thin wrapper that routes through ServiceLocator so the
    // Integration assembly doesn't take a hard reference on Tartaria.Camera.
    // CameraController registers itself as ICameraShakeService in Awake().
    // ─────────────────────────────────────────────────────────────────────

    public class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance { get; private set; }

        Coroutine _localShakeRoutine;
        Vector3 _localShakeOrigin;
        UnityEngine.Camera _localShakeCamera;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Trigger a camera shake. Prefers the registered ICameraShakeService
        /// (CameraController). Falls back to a transient local Main Camera jitter
        /// coroutine if no service is registered yet (e.g. early scene boot).
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            if (duration <= 0f || intensity <= 0f) return;

            var svc = ServiceLocator.CameraShake;
            if (svc != null)
            {
                svc.TriggerShake(intensity, duration);
                return;
            }

            // Fallback path — Main Camera transform jitter.
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            if (_localShakeRoutine != null) StopCoroutine(_localShakeRoutine);
            _localShakeCamera = cam;
            _localShakeOrigin = cam.transform.localPosition;
            _localShakeRoutine = StartCoroutine(LocalShake(intensity, duration));
        }

        System.Collections.IEnumerator LocalShake(float intensity, float duration)
        {
            float t = 0f;
            while (t < duration && _localShakeCamera != null)
            {
                float falloff = 1f - (t / duration);
                Vector3 offset = UnityEngine.Random.insideUnitSphere * intensity * falloff;
                offset.z = 0f;
                _localShakeCamera.transform.localPosition = _localShakeOrigin + offset;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (_localShakeCamera != null)
                _localShakeCamera.transform.localPosition = _localShakeOrigin;
            _localShakeRoutine = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // AchievementSystem — real implementation with PlayerPrefs persistence,
    // unlock event, and a Definitions list so AchievementListOverlay's
    // reflection-driven UI works end-to-end.
    // ─────────────────────────────────────────────────────────────────────

    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        const string PrefKeyPrefix = "TARTARIA_ACHIEVE_";

        /// <summary>Raised once when an achievement transitions from locked to unlocked.</summary>
        public event Action<string> OnAchievementUnlocked;

        [Serializable]
        public class AchievementDef
        {
            public string id;
            public string title;
            public string description;
            public bool hidden;
            public int aetherReward;
            public float rsReward;
        }

        readonly HashSet<string> _unlocked = new HashSet<string>();
        readonly Dictionary<string, float> _progress = new Dictionary<string, float>();
        readonly List<AchievementDef> _definitions = new List<AchievementDef>
        {
            new AchievementDef { id = "first_restoration",         title = "First Restoration",      description = "Restore your first Tartarian structure.", aetherReward = 5,  rsReward = 10f },
            new AchievementDef { id = "three_buildings_restored",  title = "Three Pillars",          description = "Restore three structures in a single run.", aetherReward = 10, rsReward = 25f },
            new AchievementDef { id = "echohaven_fully_restored",  title = "Echohaven Reborn",       description = "Restore every building in Echohaven.",      aetherReward = 25, rsReward = 100f },
            new AchievementDef { id = "perfect_restoration",       title = "Resonant Perfection",    description = "Restore three structures with perfect tuning accuracy.", aetherReward = 15, rsReward = 50f },
            new AchievementDef { id = "solidification",            title = "Solidification",         description = "Witness Anastasia regain solid form.",      aetherReward = 0,  rsReward = 0f,   hidden = true },
            new AchievementDef { id = "day_out_of_time",           title = "Day Out of Time",        description = "Survive the 17th hour.",                    aetherReward = 0,  rsReward = 0f,   hidden = true },
            new AchievementDef { id = "zereth_redeemed",           title = "Zereth Redeemed",        description = "Find a path of mercy.",                     aetherReward = 0,  rsReward = 0f,   hidden = true }
        };

        /// <summary>Exposed for AchievementListOverlay reflection.</summary>
        public IReadOnlyList<AchievementDef> Definitions => _definitions;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPlayerPrefs();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void LoadFromPlayerPrefs()
        {
            foreach (var def in _definitions)
            {
                if (PlayerPrefs.GetInt(PrefKeyPrefix + def.id, 0) == 1)
                    _unlocked.Add(def.id);
            }
        }

        public bool IsUnlocked(string id) => !string.IsNullOrEmpty(id) && _unlocked.Contains(id);

        public float GetProgress(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0f;
            if (_unlocked.Contains(id)) return 1f;
            return _progress.TryGetValue(id, out var p) ? Mathf.Clamp01(p) : 0f;
        }

        public void SetProgress(string id, float p01)
        {
            if (string.IsNullOrEmpty(id)) return;
            _progress[id] = Mathf.Clamp01(p01);
            if (_progress[id] >= 1f) Unlock(id);
        }

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_unlocked.Add(id)) return;

            PlayerPrefs.SetInt(PrefKeyPrefix + id, 1);
            PlayerPrefs.Save();

            // Apply rewards if this id matches a known definition.
            var def = _definitions.Find(d => d.id == id);
            if (def != null)
            {
                if (def.aetherReward > 0)
                    EconomySystem.Instance?.AddCurrency(CurrencyType.AetherShards, def.aetherReward);
                if (def.rsReward > 0f)
                    GameEvents.FireRSChange(def.rsReward);
            }

            // Notify observers (AchievementToastOverlay, telemetry, etc.).
            try { OnAchievementUnlocked?.Invoke(id); }
            catch (Exception ex) { Debug.LogError($"[AchievementSystem] OnAchievementUnlocked listener threw: {ex.Message}"); }

            // HUD banner (works regardless of toast overlay reflection wiring).
            string title = def?.title ?? id;
            ServiceLocator.HUD?.ShowAchievementToast(title, def?.description ?? "");

            Debug.Log($"[AchievementSystem] Unlocked: {id}");
        }

        // ── Convenience check hooks (called from gameplay systems) ──

        public void CheckBuildingRestored(int totalRestored, bool allPerfect)
        {
            if (totalRestored >= 1)  Unlock("first_restoration");
            if (totalRestored >= 3)  Unlock("three_buildings_restored");
            if (totalRestored >= 12) Unlock("echohaven_fully_restored");
            if (allPerfect)          Unlock("perfect_restoration");

            // Progress bars for the locked tiers.
            if (totalRestored < 3)
                SetProgress("three_buildings_restored", totalRestored / 3f);
            if (totalRestored < 12)
                SetProgress("echohaven_fully_restored", totalRestored / 12f);
        }

        public void CheckMoonCompleted(int moonIndex) => Unlock($"moon_{moonIndex + 1}_complete");

        public void CheckEnemyDefeated(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            Unlock($"defeat_{enemyId}");
        }

        public void CheckEnemyDefeated(int waveIndex, string enemyTag, bool wasBoss)
        {
            Unlock($"wave_{waveIndex}_{enemyTag}{(wasBoss ? "_boss" : "")}");
        }

        public void CheckSolidification()  => Unlock("solidification");
        public void CheckDayOutOfTime()    => Unlock("day_out_of_time");
        public void CheckZerethRedeemed()  => Unlock("zereth_redeemed");
    }

    // ─────────────────────────────────────────────────────────────────────
    // GameLoopController — light RS-routing surface. The full 3,653 LOC
    // controller is in the archived legacy folder; this version forwards
    // every AwardRS / QueueRSReward into GameEvents.OnRSChanged so
    // RunProgressTracker persists the value across scenes.
    // ─────────────────────────────────────────────────────────────────────

    public class GameLoopController : MonoBehaviour
    {
        public static GameLoopController Instance { get; private set; }

        float _sessionRS;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Immediate RS award. Delta flows through GameEvents.OnRSChanged so
        /// RunProgressTracker writes the new total to PlayerPrefs for the run.
        /// </summary>
        public void AwardRS(float amount, string reason = "")
        {
            if (amount == 0f) return;
            _sessionRS += amount;
            GameEvents.FireRSChange(amount);
            Debug.Log($"[GameLoop] AwardRS({amount:F1}) reason='{reason}' sessionTotal={_sessionRS:F1}");
        }

        /// <summary>
        /// Queued RS reward — currently semantically identical to AwardRS
        /// (no pending queue), preserved for caller API compatibility.
        /// </summary>
        public void QueueRSReward(float amount, string reason = "")
        {
            AwardRS(amount, reason);
        }

        /// <summary>
        /// Read the run's accumulated RS. Prefers RunProgressTracker (persistent),
        /// falls back to the session-local accumulator.
        /// </summary>
        public float GetCurrentRS()
        {
            var run = RunProgressTracker.Instance;
            return run != null ? run.TotalRS : _sessionRS;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // MoonProgressTracker — per-Moon clear/beat tracking with PlayerPrefs
    // persistence. Real impl (not a stub): IsCleared returns the saved flag
    // so the Echohaven Obelisk overlay shows accurate progress across runs.
    // ─────────────────────────────────────────────────────────────────────

    public class MoonProgressTracker : MonoBehaviour
    {
        public static MoonProgressTracker Instance { get; private set; }
        public const int MoonCount = 13;

        const string PrefMoonClearedPrefix = "TARTARIA_MOON_CLEARED_";
        const string PrefBeatPrefix        = "TARTARIA_BEAT_";
        const string PrefMoon3RailUnlocked = "TARTARIA_MOON3_RAIL_UNLOCKED";

        public static event Action<int> OnMoonCleared;

        readonly HashSet<int>    _clearedMoons = new HashSet<int>();
        readonly HashSet<string> _clearedBeats = new HashSet<string>();
        bool _moon3ContinentalRailUnlocked;

        public int ClearedCount => _clearedMoons.Count;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromPlayerPrefs();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void LoadFromPlayerPrefs()
        {
            for (int i = 1; i <= MoonCount; i++)
            {
                if (PlayerPrefs.GetInt(PrefMoonClearedPrefix + i, 0) == 1)
                    _clearedMoons.Add(i);
            }
            _moon3ContinentalRailUnlocked = PlayerPrefs.GetInt(PrefMoon3RailUnlocked, 0) == 1;
        }

        public bool IsCleared(int moonIndex) => _clearedMoons.Contains(moonIndex);

        public void ResetAll()
        {
            // Clear in-memory state.
            foreach (var idx in _clearedMoons)
                PlayerPrefs.DeleteKey(PrefMoonClearedPrefix + idx);
            foreach (var beat in _clearedBeats)
                PlayerPrefs.DeleteKey(PrefBeatPrefix + beat);
            PlayerPrefs.DeleteKey(PrefMoon3RailUnlocked);
            PlayerPrefs.Save();

            _clearedMoons.Clear();
            _clearedBeats.Clear();
            _moon3ContinentalRailUnlocked = false;
            Debug.Log("[MoonProgressTracker] ResetAll — cleared all moon/beat progress");
        }

        public void MarkMoonCleared(int moonIndex)
        {
            if (!_clearedMoons.Add(moonIndex)) return;

            PlayerPrefs.SetInt(PrefMoonClearedPrefix + moonIndex, 1);
            PlayerPrefs.Save();

            Debug.Log($"[MoonProgressTracker] Cleared moon {moonIndex} (total cleared={_clearedMoons.Count})");

            try { OnMoonCleared?.Invoke(moonIndex); }
            catch (Exception ex) { Debug.LogError($"[MoonProgressTracker] OnMoonCleared listener threw: {ex.Message}"); }

            // Fire the canonical GameEvents.OnMoonUnlocked so global listeners react too.
            try
            {
                GameEvents.RaiseMoonUnlocked(new MoonUnlockedEventArgs
                {
                    moonIndex   = moonIndex,
                    moonName    = $"Moon {moonIndex:D2}",
                    portalPosition = Vector3.zero
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoonProgressTracker] GameEvents.RaiseMoonUnlocked failed: {ex.Message}");
            }

            // Achievement hook.
            AchievementSystem.Instance?.CheckMoonCompleted(moonIndex - 1);
        }

        public void MarkBeatCleared(int moonIndex, int beatIndex)
        {
            string key = $"{moonIndex}:{beatIndex}";
            if (!_clearedBeats.Add(key)) return;
            PlayerPrefs.SetInt(PrefBeatPrefix + key, 1);
            PlayerPrefs.Save();
            Debug.Log($"[MoonProgressTracker] Beat cleared moon={moonIndex} beat={beatIndex}");
        }

        public bool IsBeatCleared(int moonIndex, int beatIndex)
        {
            string key = $"{moonIndex}:{beatIndex}";
            if (_clearedBeats.Contains(key)) return true;
            // Fall through to PlayerPrefs in case we haven't lazy-loaded this one yet.
            if (PlayerPrefs.GetInt(PrefBeatPrefix + key, 0) == 1)
            {
                _clearedBeats.Add(key);
                return true;
            }
            return false;
        }

        public void MarkMoon3ContinentalRailUnlocked()
        {
            if (_moon3ContinentalRailUnlocked) return;
            _moon3ContinentalRailUnlocked = true;
            PlayerPrefs.SetInt(PrefMoon3RailUnlocked, 1);
            PlayerPrefs.Save();
            Debug.Log("[MoonProgressTracker] Moon 3 Continental Rail unlocked");
        }

        public bool IsMoon3ContinentalRailUnlocked() => _moon3ContinentalRailUnlocked;
    }

    // ─────────────────────────────────────────────────────────────────────
    // VFXManager — minimal MonoBehaviour singleton. RuntimeVFXSetup writes
    // the three placeholder prefab references onto private fields via
    // reflection, so the fields are declared here. The richer effect API
    // lives behind ServiceLocator.VFX (IVFXService).
    // ─────────────────────────────────────────────────────────────────────

    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        // ── Reflection target fields (written by RuntimeVFXSetup) ──
        #pragma warning disable CS0414 // assigned via reflection, never read here
        [SerializeField] GameObject scanPulseVFXPrefab;
        [SerializeField] GameObject restoreSparkleVFXPrefab;
        [SerializeField] GameObject shardCollectVFXPrefab;
        #pragma warning restore CS0414

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public GameObject ScanPulsePrefab      => scanPulseVFXPrefab;
        public GameObject RestoreSparklePrefab => restoreSparkleVFXPrefab;
        public GameObject ShardCollectPrefab   => shardCollectVFXPrefab;
    }
}
