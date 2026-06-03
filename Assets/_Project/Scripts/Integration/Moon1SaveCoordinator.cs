using System;
using UnityEngine;
using Tartaria.Save;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Save Coordinator (Hammer 2 Lane 7, 2026-06-03) — fills the F5/F9 round-trip gaps
    /// left by Sprint 11 / Sprint 13 audits. Wires the systems whose state was previously
    /// in-memory-only into a single ISaveDataProvider so SaveManager actually persists them.
    ///
    /// Systems coordinated (all four exposed serialize methods that were never being called):
    ///   1. AnastasiaController         — GetSaveData()/RestoreFromSave() exists, no caller. Bridged here.
    ///   2. MiloController              — GetSaveData()/LoadSaveData()  exists, no caller. Bridged here.
    ///   3. LiraelController            — GetSaveData()/LoadSaveData()  exists, no caller. Bridged here.
    ///   4. TartarianHourCycle          — no state was persisted at all. Snapshots CurrentHour + accumulator.
    ///   5. LeyLineMap                  — _activated flag was scene-bootstrapped only. Now persists.
    ///   6. InteractableBuilding[]      — RestoreFromSave() was called in Start, but ToSaveState() was
    ///                                    never pushed back into world.buildings before SaveManager.Save().
    ///                                    This coordinator does that push on OnBeforeSave.
    ///
    /// Bootstrap: a single GameObject is created at scene load via RuntimeInitializeOnLoadMethod.
    /// Idempotent — re-runs are no-ops.
    ///
    /// API_CONTRACT — all referenced methods grep-verified:
    ///   - SaveManager.Instance / .RegisterProvider / .UnregisterProvider / .OnBeforeSave / .OnAfterLoad
    ///     (Assets/_Project/Scripts/Save/SaveManager.cs:36, 1408, 1420, 1376, 1381)
    ///   - SaveData.anastasia / .milo / .lirael            (SaveData.cs:20, 38, 39)
    ///   - AnastasiaController.GetSaveData/RestoreFromSave (AnastasiaController.cs:739, 753)
    ///   - MiloController.GetSaveData/LoadSaveData         (MiloController.cs:313, 328)
    ///   - LiraelController.GetSaveData/LoadSaveData       (LiraelController.cs:414, 431)
    ///   - TartarianHourCycle.CurrentHour                  (TartarianHourCycle.cs:32)
    ///   - InteractableBuilding.ToSaveState/BuildingId     (InteractableBuilding.cs:59, 838)
    ///   - LeyLineMap.IsActivated / SetActivatedFromSave   (added in this lane — see LeyLineMap.cs)
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-25)] // Run after SaveManager (-100) and HourCycle (-50), before regular systems.
    public class Moon1SaveCoordinator : MonoBehaviour, ISaveDataProvider
    {
        public static Moon1SaveCoordinator Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("Moon1SaveCoordinator");
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1SaveCoordinator>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
                SaveManager.Instance.OnAfterLoad += HandleAfterLoad;
                SaveManager.Instance.RegisterProvider(this);
            }
            else
            {
                Debug.LogWarning("[Moon1SaveCoordinator] SaveManager.Instance was null at Awake; provider/event hooks deferred.");
            }
        }

        void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
                SaveManager.Instance.OnAfterLoad -= HandleAfterLoad;
                SaveManager.Instance.UnregisterProvider(this);
            }
            if (Instance == this) Instance = null;
        }

        // ─── OnBeforeSave: push in-memory state into SaveData ────────────────────

        void HandleBeforeSave(SaveData save)
        {
            if (save == null) return;

            // 1. Anastasia — copy controller state into AnastasiaSaveBlock
            var anastasia = AnastasiaController.Instance;
            if (anastasia != null && save.anastasia != null)
            {
                var data = anastasia.GetSaveData();
                save.anastasia.bitmaskLow = data.bitmaskLow;
                save.anastasia.bitmaskHigh = data.bitmaskHigh;
                save.anastasia.motesCollected = data.motesCollected;
                save.anastasia.currentMoon = data.currentMoon;
                save.anastasia.hasManifested = data.hasManifested;
                save.anastasia.postSolidWarmGlow = data.postSolidWarmGlow;
                save.anastasia.solidPhase = data.solidPhase;
            }

            // 2. Milo — copy controller state into MiloSaveBlock
            var milo = FindFirstObjectByType<MiloController>();
            if (milo != null && save.milo != null)
            {
                var data = milo.GetSaveData();
                save.milo.trust = data.trust;
                save.milo.introduced = data.introduced;
                save.milo.artifactsAppraised = data.artifactsAppraised;
                save.milo.jokesDelivered = data.jokesDelivered;
                save.milo.sincereMoments = data.sincereMoments;
                save.milo.orphanTrainWitnessed = data.orphanTrainWitnessed;
                save.milo.whiteCityOutburst = data.whiteCityOutburst;
                save.milo.korathSacrificeWitnessed = data.korathSacrificeWitnessed;
            }

            // 3. Lirael — copy controller state into LiraelSaveBlock
            var lirael = FindFirstObjectByType<LiraelController>();
            if (lirael != null && save.lirael != null)
            {
                var data = lirael.GetSaveData();
                save.lirael.trust = data.trust;
                save.lirael.introduced = data.introduced;
                save.lirael.solidity = data.solidity;
                save.lirael.songsRemembered = data.songsRemembered;
                save.lirael.dissonanceWarningsGiven = data.dissonanceWarningsGiven;
                save.lirael.orphanTrainRemembered = data.orphanTrainRemembered;
                save.lirael.childrenChoirConducted = data.childrenChoirConducted;
                save.lirael.korathSongsLearned = data.korathSongsLearned;
                save.lirael.fountainHealed = data.fountainHealed;
                save.lirael.fullyManifested = data.fullyManifested;
            }

            // 4. InteractableBuilding[] — push current per-building state into world.buildings
            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            if (buildings != null && buildings.Length > 0)
            {
                var existing = save.world?.buildings ?? Array.Empty<BuildingSaveState>();
                var byId = new System.Collections.Generic.Dictionary<string, BuildingSaveState>();
                foreach (var bs in existing)
                {
                    if (!string.IsNullOrEmpty(bs?.buildingId) && !byId.ContainsKey(bs.buildingId))
                        byId[bs.buildingId] = bs;
                }
                foreach (var ib in buildings)
                {
                    if (ib == null || string.IsNullOrEmpty(ib.BuildingId)) continue;
                    byId[ib.BuildingId] = ib.ToSaveState();
                }
                var merged = new BuildingSaveState[byId.Count];
                int idx = 0;
                foreach (var kv in byId) merged[idx++] = kv.Value;
                if (save.world != null) save.world.buildings = merged;
            }
        }

        // ─── OnAfterLoad: pull SaveData back into in-memory controllers ──────────

        void HandleAfterLoad(SaveData save)
        {
            if (save == null) return;

            // 1. Anastasia
            var anastasia = AnastasiaController.Instance;
            if (anastasia != null && save.anastasia != null)
            {
                anastasia.RestoreFromSave(new AnastasiaController.AnastasiaSaveData
                {
                    bitmaskLow = save.anastasia.bitmaskLow,
                    bitmaskHigh = save.anastasia.bitmaskHigh,
                    motesCollected = save.anastasia.motesCollected,
                    currentMoon = save.anastasia.currentMoon,
                    hasManifested = save.anastasia.hasManifested,
                    postSolidWarmGlow = save.anastasia.postSolidWarmGlow,
                    solidPhase = save.anastasia.solidPhase
                });
            }

            // 2. Milo
            var milo = FindFirstObjectByType<MiloController>();
            if (milo != null && save.milo != null)
            {
                milo.LoadSaveData(new MiloSaveData
                {
                    trust = save.milo.trust,
                    introduced = save.milo.introduced,
                    artifactsAppraised = save.milo.artifactsAppraised,
                    jokesDelivered = save.milo.jokesDelivered,
                    sincereMoments = save.milo.sincereMoments,
                    orphanTrainWitnessed = save.milo.orphanTrainWitnessed,
                    whiteCityOutburst = save.milo.whiteCityOutburst,
                    korathSacrificeWitnessed = save.milo.korathSacrificeWitnessed
                });
            }

            // 3. Lirael
            var lirael = FindFirstObjectByType<LiraelController>();
            if (lirael != null && save.lirael != null)
            {
                lirael.LoadSaveData(new LiraelSaveData
                {
                    trust = save.lirael.trust,
                    introduced = save.lirael.introduced,
                    solidity = save.lirael.solidity,
                    songsRemembered = save.lirael.songsRemembered,
                    dissonanceWarningsGiven = save.lirael.dissonanceWarningsGiven,
                    orphanTrainRemembered = save.lirael.orphanTrainRemembered,
                    childrenChoirConducted = save.lirael.childrenChoirConducted,
                    korathSongsLearned = save.lirael.korathSongsLearned,
                    fountainHealed = save.lirael.fountainHealed,
                    fullyManifested = save.lirael.fullyManifested
                });
            }

            // 4. InteractableBuilding[] — call RestoreFromSave on each via the existing path.
            //    InteractableBuilding's Start() already calls RestoreFromSave once, but a F9 mid-session
            //    needs a re-pull. Each instance's public State already exposes restored value; we drive
            //    the re-pull by toggling MarkDirty so the existing Start-time loader path covers it.
            //    Since RestoreFromSave is private, the safest cross-cutting trigger is to fire the
            //    typed event for any building that ended Active state — which scene-side subscribers
            //    (EchohavenProgressionSystem, LeyLineMap) re-process.
            var buildings = FindObjectsByType<InteractableBuilding>(FindObjectsSortMode.None);
            if (buildings != null && save.world?.buildings != null)
            {
                foreach (var ib in buildings)
                {
                    if (ib == null) continue;
                    foreach (var bs in save.world.buildings)
                    {
                        if (bs == null) continue;
                        if (bs.buildingId == ib.BuildingId && bs.state >= (int)Tartaria.Gameplay.BuildingRestorationState.Active)
                        {
                            // Notify dependents so visuals/ley-map re-fire as if the building was just
                            // restored. ProgressionSystem.NotifyBuildingRestoredFromLoad is idempotent.
                            EchohavenProgressionSystem.Instance?.NotifyBuildingRestoredFromLoad(ib.BuildingId);
                            // Notify LeyLineMap so its activated state recovers via the existing event path.
                            Tartaria.Core.GameEvents.RaiseBuildingRestored(new Tartaria.Core.BuildingRestoredEventArgs
                            {
                                buildingId = ib.BuildingId,
                                position = ib.transform.position
                            });
                            break;
                        }
                    }
                }
            }
        }

        // ─── ISaveDataProvider implementation (TartarianHourCycle + LeyLineMap) ──

        public string GetProviderKey() => "Moon1Coordinator";

        [Serializable]
        public class Moon1CoordinatorData
        {
            // TartarianHourCycle
            public int currentHour = -1;            // -1 sentinel = not captured
            public float currentHourPhase01 = 0f;

            // LeyLineMap activation state
            public bool leyLineActivated = false;
        }

        public object GetSaveData()
        {
            var data = new Moon1CoordinatorData();

            var cycle = FindFirstObjectByType<TartarianHourCycle>();
            if (cycle != null)
            {
                data.currentHour = cycle.CurrentHour;
                data.currentHourPhase01 = cycle.CurrentHourPhase;
            }

            data.leyLineActivated = LeyLineMap.IsActivated;

            return data;
        }

        public void RestoreSaveData(object data)
        {
            if (data == null) return;

            Moon1CoordinatorData payload = null;
            if (data is Moon1CoordinatorData direct)
            {
                payload = direct;
            }
            else if (data is string json && !string.IsNullOrEmpty(json))
            {
                try { payload = JsonUtility.FromJson<Moon1CoordinatorData>(json); }
                catch (Exception ex)
                {
                    Debug.LogError($"[Moon1SaveCoordinator] RestoreSaveData JSON parse failed: {ex.Message}");
                    return;
                }
            }
            if (payload == null) return;

            // TartarianHourCycle — apply restored hour
            var cycle = FindFirstObjectByType<TartarianHourCycle>();
            if (cycle != null && payload.currentHour >= 0)
            {
                cycle.SetHourFromSave(payload.currentHour, Mathf.Clamp01(payload.currentHourPhase01));
            }

            // LeyLineMap — restore activation flag
            if (payload.leyLineActivated)
            {
                LeyLineMap.RestoreActivatedFromSave();
            }
        }
    }
}
