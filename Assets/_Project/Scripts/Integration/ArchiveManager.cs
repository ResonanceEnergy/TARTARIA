using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Manages the Old World Archive at runtime:
    ///   - Holds the ArchiveDatabase reference (loaded from Resources or Build phase)
    ///   - Tracks which entries are unlocked / seen by the player
    ///   - Hooks into Anastasia's line delivery to auto-unlock matching entries
    ///   - Persists unlock state via SaveManager
    ///
    /// Unlock triggers:
    ///   • Building restoration  → UnlockEntry(buildingId)
    ///   • Anastasia lore whisper→ UnlockByTrigger(triggerContext)
    ///   • Quest completion      → UnlockEntry(questArchiveTag)
    ///   • Hidden collectible    → UnlockEntry(collectibleId)
    ///   • Zone first-visit      → UnlockByCategory(relevant category)
    /// </summary>
    [DefaultExecutionOrder(-55)]
    [DisallowMultipleComponent]
    public class ArchiveManager : MonoBehaviour
    {
        public static ArchiveManager Instance { get; private set; }

        [SerializeField] ArchiveDatabase database;

        // Unlock state
        readonly HashSet<string> _unlocked = new();
        readonly HashSet<string> _newBadge  = new();

        // RS tier tracking (cumulative)
        float _cumulativeRS;
        bool _rsTier25;
        bool _rsTier50;
        bool _rsTier75;

        public ArchiveDatabase Database => database;

        // ─── Lifecycle ────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            TryLoadDatabase();
        }

        void OnEnable()
        {
            // Unlock all entries flagged unlockedByDefault on startup
            if (database != null)
                foreach (var e in database.entries)
                    if (e != null && e.unlockedByDefault)
                        _unlocked.Add(e.entryId);

            // Hook Anastasia line delivery
            if (AnastasiaController.Instance != null)
                AnastasiaController.Instance.OnLineDelivered += OnAnastasiaLine;

            // Hook RS changes — unlock science entries as RS rises
            GameEvents.OnRSChanged += OnRSChanged;

            // Hook save / load
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
                SaveManager.Instance.OnAfterLoad  += HandleAfterLoad;
            }
        }

        void OnDisable()
        {
            if (AnastasiaController.Instance != null)
                AnastasiaController.Instance.OnLineDelivered -= OnAnastasiaLine;
            GameEvents.OnRSChanged -= OnRSChanged;

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
                SaveManager.Instance.OnAfterLoad  -= HandleAfterLoad;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Database Loading ─────────────────────────

        void TryLoadDatabase()
        {
            if (database != null) return;

            // Try Resources folder first
            database = Resources.Load<ArchiveDatabase>("ArchiveDatabase");
            if (database == null)
                Debug.LogWarning("[ArchiveManager] ArchiveDatabase not found in Resources. " +
                    "Run Tartaria > Build Assets > Archive Database to generate it.");
            else
                database.BuildLookup();
        }

        // ─── Unlock API ───────────────────────────────

        public bool IsUnlocked(string entryId) => _unlocked.Contains(entryId);
        public bool IsNew(string entryId)      => _newBadge.Contains(entryId);

        /// Unlock a specific entry by id. Notifies ArchiveUI.
        public void UnlockEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId)) return;
            if (_unlocked.Contains(entryId)) return;  // already known

            _unlocked.Add(entryId);
            _newBadge.Add(entryId);

            ArchiveUI.Instance?.OnEntryUnlocked(entryId);
            Debug.Log($"[Archive] Unlocked: {entryId}");

            // If Anastasia exists, have her whisper the entry's quote
            var entry = database?.GetById(entryId);
            if (entry != null && !string.IsNullOrEmpty(entry.anastasiaQuote))
                AnastasiaController.Instance?.TryDeliverLine(entry.unlockTrigger);
        }

        /// Mark that the player has read/seen this entry (removes the "NEW" badge).
        public void MarkSeen(string entryId)
        {
            _newBadge.Remove(entryId);
        }

        /// Unlock all entries with a matching unlockTrigger context key.
        public void UnlockByTrigger(string triggerContext)
        {
            if (database == null) return;
            foreach (var e in database.entries)
                if (e != null && e.unlockTrigger == triggerContext)
                    UnlockEntry(e.entryId);
        }

        /// Unlock a random locked entry from a category (used on zone discovery).
        public void UnlockRandomFromCategory(ArchiveCategory cat)
        {
            if (database == null) return;
            var candidates = new List<ArchiveEntry>();
            foreach (var e in database.entries)
                if (e != null && e.category == cat && !_unlocked.Contains(e.entryId))
                    candidates.Add(e);
            if (candidates.Count == 0) return;
            UnlockEntry(candidates[Random.Range(0, candidates.Count)].entryId);
        }

        // ─── Event Handlers ───────────────────────────

        void OnAnastasiaLine(AnastasiaLine line)
        {
            // A lore whisper delivery unlocks matching archive entries
            UnlockByTrigger(line.triggerContext);
        }

        void OnRSChanged(float delta)
        {
            _cumulativeRS += delta;
            if (!_rsTier25 && _cumulativeRS >= 25f)  { _rsTier25 = true; UnlockByTrigger("rs_25"); }
            if (!_rsTier50 && _cumulativeRS >= 50f)  { _rsTier50 = true; UnlockByTrigger("rs_50"); }
            if (!_rsTier75 && _cumulativeRS >= 75f)  { _rsTier75 = true; UnlockByTrigger("rs_75"); }
        }

        // ─── Save Integration ─────────────────────────

        public string[] GetUnlockedIds()
        {
            var arr = new string[_unlocked.Count];
            _unlocked.CopyTo(arr);
            return arr;
        }

        public void RestoreFromSave(string[] ids)
        {
            if (ids == null) return;
            foreach (var id in ids)
                if (!string.IsNullOrEmpty(id))
                    _unlocked.Add(id);
        }

        void HandleBeforeSave(SaveData data)
        {
            if (data.archive == null) data.archive = new ArchiveSaveBlock();
            data.archive.unlockedEntryIds = GetUnlockedIds();
            data.archive.cumulativeRS     = _cumulativeRS;
        }

        void HandleAfterLoad(SaveData data)
        {
            if (data.archive == null) return;
            RestoreFromSave(data.archive.unlockedEntryIds);
            _cumulativeRS = data.archive.cumulativeRS;
            // Re-apply tier flags based on restored RS
            if (_cumulativeRS >= 25f) _rsTier25 = true;
            if (_cumulativeRS >= 50f) _rsTier50 = true;
            if (_cumulativeRS >= 75f) _rsTier75 = true;
            // Push unlocked state to UI
            ArchiveUI.Instance?.SetUnlockedIds(_unlocked);
        }
    }
}
