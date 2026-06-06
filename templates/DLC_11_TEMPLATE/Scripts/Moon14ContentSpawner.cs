using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;
using Tartaria.DLC;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 14 ContentSpawner — DLC 11: CELESTIAL MOON
    /// "The Manifestation of Cosmic Resonance"
    /// 
    /// DLC Design:
    ///   - 3 cosmic altars scattered across zone (puzzle sequence)
    ///   - Reality shift mechanic (toggle between two overlapping realities)
    ///   - Celestial gear tier (upgrades to player abilities)
    ///   - Leviathan boss fight (3-phase encounter)
    ///   - 8-10 hours of content
    /// 
    /// Save Compatibility:
    ///   - Adds Moon14SaveBlock to SaveData (see DLCSaveCompatibility)
    ///   - Save version: v18 → v19
    ///   - Base game can load DLC saves (forward compat)
    /// 
    /// Production Status:
    ///   - TEMPLATE STUB: Replace with real implementation
    ///   - Copy from Moon10ContentSpawner.cs as reference
    ///   - Hook into DLCLoader.OnDLCLoaded event
    /// </summary>
    public class Moon14ContentSpawner : DLCContentSpawner
    {
        [Header("Moon 14 State")]
        [SerializeField] bool moon14Unlocked;
        [SerializeField] bool altar1Activated;
        [SerializeField] bool altar2Activated;
        [SerializeField] bool altar3Activated;
        [SerializeField] bool leviathanDefeated;
        [SerializeField] bool realityShiftUnlocked;

        [Header("Altars")]
        [SerializeField] Vector3[] altarPositions = new Vector3[3];
        [SerializeField] GameObject altarPrefab;

        [Header("Boss")]
        [SerializeField] Vector3 leviathanSpawnPoint = new(500f, 50f, 500f);
        [SerializeField] GameObject leviathanPrefab;

        readonly List<GameObject> _altars = new();
        readonly List<GameObject> _celestialEnemies = new();
        GameObject _leviathan;
        bool _contentSpawned;

        public bool IsMoon14Active => moon14Unlocked && !leviathanDefeated;
        public int AltarsActivated => (altar1Activated ? 1 : 0) + (altar2Activated ? 1 : 0) + (altar3Activated ? 1 : 0);
        public float CompletionPercent => AltarsActivated / 3f;

        public override void Initialize(DLCManifest manifest, string contentPath)
        {
            base.Initialize(manifest, contentPath);

            // Check if Moon 14 is unlocked
            moon14Unlocked = SaveManager.Instance?.GetMoonProgress(14) > 0f;

            Debug.Log($"[Moon14ContentSpawner] Initialized. Unlocked: {moon14Unlocked}");
        }

        void Start()
        {
            if (moon14Unlocked && !_contentSpawned)
            {
                SpawnMoon14Content();
            }
        }

        protected override void OnSave(SaveData sd)
        {
            base.OnSave(sd);

            // Save Moon 14 state
            sd.SetMoonFlag(14, "unlocked", moon14Unlocked);
            sd.SetMoonFlag(14, "altar1", altar1Activated);
            sd.SetMoonFlag(14, "altar2", altar2Activated);
            sd.SetMoonFlag(14, "altar3", altar3Activated);
            sd.SetMoonFlag(14, "leviathanDefeated", leviathanDefeated);
            sd.SetMoonFlag(14, "realityShiftUnlocked", realityShiftUnlocked);

            Debug.Log($"[Moon14ContentSpawner] Saved state: {AltarsActivated}/3 altars, Leviathan: {leviathanDefeated}");
        }

        protected override void OnLoad(SaveData sd)
        {
            base.OnLoad(sd);

            // Restore Moon 14 state
            moon14Unlocked = sd.GetMoonFlag(14, "unlocked");
            altar1Activated = sd.GetMoonFlag(14, "altar1");
            altar2Activated = sd.GetMoonFlag(14, "altar2");
            altar3Activated = sd.GetMoonFlag(14, "altar3");
            leviathanDefeated = sd.GetMoonFlag(14, "leviathanDefeated");
            realityShiftUnlocked = sd.GetMoonFlag(14, "realityShiftUnlocked");

            Debug.Log($"[Moon14ContentSpawner] Loaded state: {AltarsActivated}/3 altars, Leviathan: {leviathanDefeated}");

            // Respawn content if needed
            if (moon14Unlocked && !_contentSpawned)
            {
                SpawnMoon14Content();
            }
        }

        void SpawnMoon14Content()
        {
            Debug.Log("[Moon14ContentSpawner] Spawning Moon 14 content...");

            // Spawn cosmic altars
            for (int i = 0; i < altarPositions.Length; i++)
            {
                if (altarPrefab != null)
                {
                    var altar = Instantiate(altarPrefab, altarPositions[i], Quaternion.identity, transform);
                    altar.name = $"CosmicAltar_{i + 1}";
                    _altars.Add(altar);

                    // TODO: Hook up altar activation logic
                }
            }

            // Spawn leviathan boss (if not defeated)
            if (!leviathanDefeated && leviathanPrefab != null)
            {
                _leviathan = Instantiate(leviathanPrefab, leviathanSpawnPoint, Quaternion.identity, transform);
                _leviathan.name = "CelestialLeviathan";

                // TODO: Hook up boss fight logic
            }

            // Fire DLC content spawned event
            GameEvents.FireDLCContentSpawned(_manifest.dlcId);

            _contentSpawned = true;
            Debug.Log("[Moon14ContentSpawner] ✓ Moon 14 content spawned.");
        }

        public void ActivateAltar(int altarIndex)
        {
            if (altarIndex < 0 || altarIndex >= 3) return;

            switch (altarIndex)
            {
                case 0: altar1Activated = true; break;
                case 1: altar2Activated = true; break;
                case 2: altar3Activated = true; break;
            }

            SaveManager.Instance?.MarkDirty();
            Debug.Log($"[Moon14ContentSpawner] Altar {altarIndex + 1} activated. Progress: {AltarsActivated}/3");

            // Check if all altars activated → unlock reality shift
            if (AltarsActivated == 3 && !realityShiftUnlocked)
            {
                UnlockRealityShift();
            }
        }

        void UnlockRealityShift()
        {
            realityShiftUnlocked = true;
            SaveManager.Instance?.MarkDirty();

            // Show tutorial for reality shift mechanic
            GameEvents.RaiseHUDShowBanner("Reality Shift Unlocked", "Press [R] to toggle between realities");

            Debug.Log("[Moon14ContentSpawner] ✓ Reality Shift mechanic unlocked!");
        }

        public void OnLeviathanDefeated()
        {
            leviathanDefeated = true;
            SaveManager.Instance?.SetMoonProgress(14, 100f);
            SaveManager.Instance?.MarkDirty();

            // Show Moon 14 completion trophy
            GameEvents.RaiseHUDShowMoonTrophy("Celestial Moon Complete", "The cosmos bends to your will");

            Debug.Log("[Moon14ContentSpawner] ✓✓✓ Moon 14 COMPLETE! Leviathan defeated.");
        }

        void OnDestroy()
        {
            // Cleanup spawned content
            foreach (var altar in _altars)
            {
                if (altar != null) Destroy(altar);
            }

            foreach (var enemy in _celestialEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }

            if (_leviathan != null) Destroy(_leviathan);
        }

        // ─── GIZMOS ──────────────────────────────────────────────────────────

        void OnDrawGizmos()
        {
            // Draw altar positions
            Gizmos.color = Color.cyan;
            foreach (var pos in altarPositions)
            {
                Gizmos.DrawWireSphere(pos, 2f);
            }

            // Draw leviathan spawn point
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leviathanSpawnPoint, 5f);
        }
    }
}
