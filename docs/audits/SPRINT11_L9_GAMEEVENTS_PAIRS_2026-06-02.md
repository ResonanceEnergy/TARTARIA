# Sprint 11 Lane 9 - GameEvents Publisher / Subscriber Pair Audit

**Date:** 2026-06-02  
**Branch:** `agent/audit/gameevents-pairs` (worktree `C:\dev\_wt_s11_l9_events`)  
**Scope:** every event declared in `Assets/_Project/Scripts/Core/GameEvents.cs` audited for at least one publisher and at least one subscriber across `Assets/_Project/Scripts/**/*.cs` (548 files).  
**Companion CSV:** `docs/audits/SPRINT11_L9_GAMEEVENTS_PAIRS_2026-06-02.csv` (full file:line lists, machine-readable).  

## Summary

| Metric | Count |
|---|---:|
| Total events declared | 64 |
| HEALTHY (>=1 publisher AND >=1 subscriber) | 35 |
| UNUSED (0 publishers AND 0 subscribers) | 4 |
| BROKEN (publishers but no subscribers, OR subscribers but no publishers) | 25 |

## Methodology

1. Parsed every `public static event Action` line in `GameEvents.cs` (declared events).
2. For each event `OnFoo`, parsed `GameEvents.cs` to discover which `Fire*` / `Raise*` helper methods invoke that event. The publisher count is the number of CALLERS of those helpers (plus any direct `OnFoo?.Invoke` outside `GameEvents.cs`).
3. For each event `OnFoo`, counted `OnFoo +=` and `OnFoo -=` occurrences outside `GameEvents.cs` as subscribers.
4. Classification: HEALTHY = both sides present; UNUSED = both sides absent (dead event); BROKEN = exactly one side missing.

Note: `+=`/`-=` pairs on the same target both count as subscriber sites (so 1 logical subscriber typically shows as 2 hits — one Subscribe, one Unsubscribe). This is intentional: each line is an independent claim against the wiring contract.

## Headline finding: CLAUDE.md canonical-facts mismatch

The `CLAUDE.md` canonical-facts table mentions eight events. Three of them DO NOT EXIST in `GameEvents.cs`:

| Event | Status in code |
|---|---|
| `OnBuildingRestored` | HEALTHY - 5 publishers, 27 subscriber sites (Moon 1 ship-gate core) |
| `OnMoonCompleted` | HEALTHY - 2 publishers, 8 subscriber sites |
| `OnPlayerDamaged` | HEALTHY - 1 publisher, 2 subscriber sites |
| `OnQuestStatusChanged` | HEALTHY - 6 publishers, 8 subscriber sites |
| `OnHUDShowDialogue` | HEALTHY - 2 publishers, 4 subscriber sites |
| `OnBrazierLit` | **DOES NOT EXIST** - 0 declarations in `GameEvents.cs` |
| `OnBrazierRingComplete` | **DOES NOT EXIST** - 0 declarations in `GameEvents.cs` |
| `OnDayChanged` | **DOES NOT EXIST** - 0 declarations (`Moon1LiraelDay25Gate.cs:7` notes the gap; `Moon1BuildOutNPCs.cs:63,89` and `Moon1DaySmokeMenus.cs:44,66,128` all carry TODOs waiting for it; `API_CONTRACT.md sec 2` claimed it at line 461 but that claim is stale) |

**Recommendation:** either declare the three missing events and wire them (Lirael's Day 25 gate, Anastasia brazier ring, day-cycle progression all depend on them), or update CLAUDE.md and `API_CONTRACT.md` to remove the false canonical claim. Both are documented as load-bearing for Moon 1 ship-gate.

## Moon 1 ship-gate event spotlight

The four events that must fire reliably for the Moon 1 happy path (per `PHASE_1_SCOPE.md` + `STATUS.md`):

| Event | Publishers | Subscribers | Status | Notes |
|---|---:|---:|---|---|
| `OnBuildingRestored` | 5 | 27 | HEALTHY (white) | Core restoration event. `Raise/FireBuildingRestored` reachable from `InteractableBuilding.cs:647`, `CathedralRestorationSystem.cs:187`, `DomeRestorationSystem.cs:23`, `FountainRestorationSystem.cs:22`, `SpireRestorationSystem.cs:22`. Wide subscriber base across HUD, Quest, Audio, Camera. Healthy. |
| `OnMoonCompleted` | 2 | 8 | HEALTHY (white) | `RaiseMoonCompleted` called twice (verify: `GameLoopController` / `MoonCompletionTracker`). 8 subscriber sites. Healthy but verify publisher actually triggers when last building of Moon 1 restored. |
| `OnBrazierLit` | n/a | n/a | **MISSING** | Event not declared in `GameEvents.cs`. Wiring expected by CLAUDE.md but unfulfilled. |
| `OnDayChanged` | n/a | n/a | **MISSING** | Event not declared in `GameEvents.cs`. Wiring expected by CLAUDE.md but unfulfilled. |

## Full event audit

Cells truncate at 240 chars when long; consult the CSV for full lists.

### HEALTHY (white) - 35 events

| Event | Decl line | Publisher count (file:line) | Subscriber count (file:line) |
|---|---:|---|---|
| `OnBuildingDiscovered` | 57 | **1** Assets/_Project/Scripts/Integration/CathedralRestorationSystem.cs:69 | **2** Assets/_Project/Scripts/Integration/MiloFollowBehaviour.cs:53; Assets/_Project/Scripts/Integration/MiloFollowBehaviour.cs:59 |
| `OnBuildingRestored` | 56 | **5** Assets/_Project/Scripts/Integration/InteractableBuilding.cs:647; Assets/_Project/Scripts/Integration/CathedralRestorationSystem.cs:187; Assets/_Project/Scripts/Integration/DomeRestorationSystem.cs:23; Assets/_Project/Scripts/Integration/Fou ... | **27** Assets/_Project/Scripts/Integration/_moon2_archive/Moon2ProgressionSystem.cs:106; Assets/_Project/Scripts/Integration/_moon2_archive/Moon2ProgressionSystem.cs:123; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:435; Assets/_Projec ... |
| `OnBuildingRestoredTyped` | 47 | **1** Assets/_Project/Scripts/Integration/InteractableBuilding.cs:647 | **20** Assets/_Project/Scripts/UI/LeyLineMinimap.cs:38; Assets/_Project/Scripts/UI/LeyLineMinimap.cs:43; Assets/_Project/Scripts/UI/LeyLineMap.cs:59; Assets/_Project/Scripts/UI/LeyLineMap.cs:64; Assets/_Project/Scripts/Integration/Moon1DialogueBin ... |
| `OnCloudConflictDetected` | 330 | **1** Assets/_Project/Scripts/Save/SaveManager.cs:1935 | **2** Assets/_Project/Scripts/Save/SaveManager.cs:102; Assets/_Project/Scripts/Save/SaveManager.cs:157 |
| `OnCombatEnded` | 437 | **1** Assets/_Project/Scripts/Integration/Phase2Stubs.cs:119 | **2** Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:434; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:445 |
| `OnCombatStarted` | 436 | **1** Assets/_Project/Scripts/Integration/Phase2Stubs.cs:118 | **2** Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:433; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:444 |
| `OnCriticalSaveTrigger` | 329 | **14** Assets/_Project/Scripts/Integration/EchohavenProgressionSystem.cs:116; Assets/_Project/Scripts/Integration/EchohavenProgressionSystem.cs:148; Assets/_Project/Scripts/Gameplay/SkillTreeSystem.cs:421; Assets/_Project/Scripts/Gameplay/Spectral ... | **4** Assets/_Project/Scripts/UI/GameCompleteOverlay.cs:73; Assets/_Project/Scripts/UI/GameCompleteOverlay.cs:78; Assets/_Project/Scripts/Save/SaveManager.cs:101; Assets/_Project/Scripts/Save/SaveManager.cs:156 |
| `OnEnemyKilled` | 71 | **2** Assets/_Project/Scripts/Integration/MudGolemEnemy.cs:217; Assets/_Project/Scripts/AI/MudGolemHealth.cs:169 | **6** Assets/_Project/Scripts/Integration/VFXWiringController.cs:36; Assets/_Project/Scripts/Integration/VFXWiringController.cs:45; Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:139; Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:1 ... |
| `OnHUDAchievementToast` | 332 | **10** Assets/_Project/Scripts/Save/SaveManager.cs:242; Assets/_Project/Scripts/Save/SaveManager.cs:249; Assets/_Project/Scripts/Save/SaveManager.cs:759; Assets/_Project/Scripts/Save/SaveManager.cs:1323; Assets/_Project/Scripts/Save/SaveManager.cs ... | **2** Assets/_Project/Scripts/UI/HUDController.cs:140; Assets/_Project/Scripts/UI/HUDController.cs:176 |
| `OnHUDCloudQueueToast` | 333 | **14** Assets/_Project/Scripts/Save/SaveManager.cs:527; Assets/_Project/Scripts/Save/SaveManager.cs:535; Assets/_Project/Scripts/Save/SaveManager.cs:544; Assets/_Project/Scripts/Save/SaveManager.cs:612; Assets/_Project/Scripts/Save/SaveManager.cs: ... | **2** Assets/_Project/Scripts/UI/HUDController.cs:141; Assets/_Project/Scripts/UI/HUDController.cs:177 |
| `OnHUDHideInteractionPrompt` | 285 | **4** Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:35; Assets/_Project/Scripts/Integration/CombatWaveManager.cs:467; Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:13; Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:486 | **4** Assets/_Project/Scripts/Integration/Moon1InteractionPrompt.cs:48; Assets/_Project/Scripts/Integration/Moon1InteractionPrompt.cs:55; Assets/_Project/Scripts/UI/HUDController.cs:155; Assets/_Project/Scripts/UI/HUDController.cs:191 |
| `OnHUDSaveConflictPrompt` | 334 | **1** Assets/_Project/Scripts/Save/SaveManager.cs:195 | **2** Assets/_Project/Scripts/UI/HUDController.cs:142; Assets/_Project/Scripts/UI/HUDController.cs:178 |
| `OnHUDShowBanner` | 243 | **23** Assets/_Project/Scripts/Integration/Moon1AnastasiaController.cs:18; Assets/_Project/Scripts/Integration/Moon1AnastasiaController.cs:81; Assets/_Project/Scripts/Integration/EchohavenProgressionSystem.cs:247; Assets/_Project/Scripts/Integrati ... | **2** Assets/_Project/Scripts/UI/HUDController.cs:148; Assets/_Project/Scripts/UI/HUDController.cs:184 |
| `OnHUDShowDialogue` | 237 | **2** Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:10; Assets/_Project/Scripts/AI/MiloTutorialFlow.cs:447 | **4** Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:68; Assets/_Project/Scripts/Integration/YarnTutorialBinding.cs:79; Assets/_Project/Scripts/UI/HUDController.cs:147; Assets/_Project/Scripts/UI/HUDController.cs:183 |
| `OnHUDShowInteractionPrompt` | 279 | **21** Assets/_Project/Scripts/Integration/InteractableBuilding.cs:237; Assets/_Project/Scripts/Integration/InteractableBuilding.cs:263; Assets/_Project/Scripts/Integration/InteractableBuilding.cs:268; Assets/_Project/Scripts/Integration/Interacta ... | **4** Assets/_Project/Scripts/Integration/Moon1InteractionPrompt.cs:47; Assets/_Project/Scripts/Integration/Moon1InteractionPrompt.cs:54; Assets/_Project/Scripts/UI/HUDController.cs:154; Assets/_Project/Scripts/UI/HUDController.cs:190 |
| `OnHUDShowObjective` | 231 | **15** Assets/_Project/Scripts/Integration/_moon2_archive/Moon2ProgressionSystem.cs:251; Assets/_Project/Scripts/Integration/_moon2_archive/Moon2ProgressionSystem.cs:354; Assets/_Project/Scripts/Integration/Moon1QuestTriggers.cs:132; Assets/_Proje ... | **2** Assets/_Project/Scripts/UI/HUDController.cs:146; Assets/_Project/Scripts/UI/HUDController.cs:182 |
| `OnHUDShowSubtitle` | 249 | **1** Assets/_Project/Scripts/Integration/PointOfInterest.cs:119 | **2** Assets/_Project/Scripts/UI/HUDController.cs:149; Assets/_Project/Scripts/UI/HUDController.cs:185 |
| `OnHUDUpdateFrequencyWheel` | 315 | **1** Assets/_Project/Scripts/Integration/CombatBridge.cs:393 | **2** Assets/_Project/Scripts/UI/HUDController.cs:160; Assets/_Project/Scripts/UI/HUDController.cs:196 |
| `OnInventoryChanged` | 172 | **12** Assets/_Project/Scripts/Gameplay/InventoryManager.cs:92; Assets/_Project/Scripts/Gameplay/InventoryManager.cs:131; Assets/_Project/Scripts/Gameplay/InventoryManager.cs:175; Assets/_Project/Scripts/Gameplay/InventoryManager.cs:235; Assets/_P ... | **8** Assets/_Project/Scripts/Integration/VFXWiringController.cs:37; Assets/_Project/Scripts/Integration/VFXWiringController.cs:46; Assets/_Project/Scripts/UI/InventoryUIPanel.cs:41; Assets/_Project/Scripts/UI/InventoryUIPanel.cs:55; Assets/_Proj ... |
| `OnItemPickup` | 160 | **1** Assets/_Project/Scripts/Gameplay/InventorySystem.cs:203 | **4** Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:129; Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:149; Assets/_Project/Scripts/UI/RewardToastController.cs:71; Assets/_Project/Scripts/UI/RewardToastController.cs:78 |
| `OnItemRemoved` | 166 | **3** Assets/_Project/Scripts/Gameplay/InventoryManager.cs:130; Assets/_Project/Scripts/Gameplay/InventorySystem.cs:247; Assets/_Project/Scripts/Gameplay/InventorySystem.cs:256 | **4** Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:121; Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:144; Assets/_Project/Scripts/UI/InventoryUIPanel.cs:43; Assets/_Project/Scripts/UI/InventoryUIPanel.cs:57 |
| `OnLevelUp` | 106 | **2** Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:295; Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:296 | **4** Assets/_Project/Scripts/Security/EconomyAnomalyDetector.cs:113; Assets/_Project/Scripts/Security/EconomyAnomalyDetector.cs:126; Assets/_Project/Scripts/UI/RewardToastController.cs:70; Assets/_Project/Scripts/UI/RewardToastController.cs:77 |
| `OnMoon3FastTravelUnlocked` | 196 | **1** Assets/_Project/Scripts/Gameplay/SpectralOrphanAdoption.cs:193 | **2** Assets/_Project/Scripts/Integration/CampaignFlowController.cs:63; Assets/_Project/Scripts/Integration/CampaignFlowController.cs:95 |
| `OnMoonCleared` | 195 | **1** Assets/_Project/Scripts/Integration/Phase2Stubs.cs:272 | **4** Assets/_Project/Scripts/Integration/MoonBeatRunner.cs:43; Assets/_Project/Scripts/Integration/MoonBeatRunner.cs:48; Assets/_Project/Scripts/UI/HUDController.cs:143; Assets/_Project/Scripts/UI/HUDController.cs:179 |
| `OnMoonCompleted` | 192 | **2** Assets/_Project/Scripts/Integration/EchohavenProgressionSystem.cs:154; Assets/_Project/Scripts/Integration/CampaignFlowController.cs:228 | **8** Assets/_Project/Scripts/Integration/Moon1PostRestorationVisuals.cs:67; Assets/_Project/Scripts/Integration/Moon1PostRestorationVisuals.cs:72; Assets/_Project/Scripts/Integration/Moon1AnastasiaController.cs:42; Assets/_Project/Scripts/Integr ... |
| `OnPlayerDamaged` | 123 | **1** Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:125 | **2** Assets/_Project/Scripts/Combat/HitFeedback.cs:128; Assets/_Project/Scripts/Combat/HitFeedback.cs:133 |
| `OnPOIDiscovered` | 442 | **1** Assets/_Project/Scripts/Integration/PointOfInterest.cs:110 | **2** Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:431; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:442 |
| `OnQuestStatusChanged` | 89 | **6** Assets/_Project/Scripts/Integration/Moon1QuestTriggers.cs:123; Assets/_Project/Scripts/Integration/QuestManager.cs:297; Assets/_Project/Scripts/Integration/QuestManager.cs:298; Assets/_Project/Scripts/Integration/QuestManager.cs:473; Assets ... | **8** Assets/_Project/Scripts/Integration/CampaignFlowController.cs:86; Assets/_Project/Scripts/Integration/CampaignFlowController.cs:97; Assets/_Project/Scripts/Integration/QuestLogUIPanel.cs:37; Assets/_Project/Scripts/Integration/QuestLogUIPan ... |
| `OnRequestPurgeCorruption` | 30 | **1** Assets/_Project/Scripts/Gameplay/CraftingSystem.cs:211 | **2** Assets/_Project/Scripts/Integration/CorruptionSystem.cs:69; Assets/_Project/Scripts/Integration/CorruptionSystem.cs:74 |
| `OnResonanceScoreChanged` | 322 | **1** Assets/_Project/Scripts/Core/AetherFieldManager.cs:51 | **2** Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:25; Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:37 |
| `OnRSChanged` | 321 | **12** Assets/_Project/Scripts/AI/ResetScout.cs:136; Assets/_Project/Scripts/Integration/Moon1MudPoolPuzzle.cs:222; Assets/_Project/Scripts/Integration/Moon1MudPoolPuzzle.cs:328; Assets/_Project/Scripts/Integration/EchohavenContentSpawner.cs:2941; ... | **10** Assets/_Project/Scripts/Integration/TartariaPostProcessing.cs:52; Assets/_Project/Scripts/Integration/TartariaPostProcessing.cs:57; Assets/_Project/Scripts/Integration/RunProgressTracker.cs:53; Assets/_Project/Scripts/Integration/RunProgres ... |
| `OnSeventeenthHour` | 446 | **1** Assets/_Project/Scripts/Integration/TartarianHourCycle.cs:90 | **4** Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:24; Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:25; Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs:33; Assets/_Project/Scripts/Integration/Moon1Cinemati ... |
| `OnToggleAetherVision` | 28 | **1** Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:162 | **10** Assets/_Project/Scripts/UI/UIManager.cs:78; Assets/_Project/Scripts/UI/UIManager.cs:90; Assets/_Project/Scripts/Integration/Moon1FirstTimeHints.cs:65; Assets/_Project/Scripts/Integration/Moon1FirstTimeHints.cs:70; Assets/_Project/Scripts/In ... |
| `OnTogglePause` | 29 | **1** Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:914 | **6** Assets/_Project/Scripts/UI/UIManager.cs:79; Assets/_Project/Scripts/UI/UIManager.cs:91; Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:99; Assets/_Project/Scripts/Integration/RuntimeHUDBuilder.cs:236; Assets/_Project/Scripts/UI/HU ... |
| `OnXPGained` | 112 | **2** Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:259; Assets/_Project/Scripts/Gameplay/PlayerProgression.cs:260 | **2** Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:126; Assets/_Project/Scripts/Core/EconomyBalanceMonitor.cs:148 |

### UNUSED (yellow) - 4 events

| Event | Decl line | Publisher count (file:line) | Subscriber count (file:line) |
|---|---:|---|---|
| `OnDialogueStateChanged` | 210 | **0** _(none)_ | **0** _(none)_ |
| `OnMoonUnlocked` | 185 | **0** _(none)_ | **0** _(none)_ |
| `OnRemotePushNotificationReceived` | 331 | **0** _(none)_ | **0** _(none)_ |
| `OnTartarianHourChanged` | 450 | **0** _(none)_ | **0** _(none)_ |

### BROKEN (red) - 25 events

| Event | Decl line | Publisher count (file:line) | Subscriber count (file:line) |
|---|---:|---|---|
| `OnAetherEnergyChanged` | 149 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:27; Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:39 |
| `OnAetherVisionToggledTyped` | 221 | **1** Assets/_Project/Scripts/Gameplay/PlayerAbilityController.cs:162 | **0** _(none)_ |
| `OnBossDefeated` | 78 | **1** Assets/_Project/Scripts/Integration/BossEncounterSystem.cs:1209 | **0** _(none)_ |
| `OnBuildingDiscoveredTyped` | 53 | **0** _(none)_ | **2** Assets/_Project/Scripts/Integration/Moon1DialogueBindings.cs:39; Assets/_Project/Scripts/Integration/Moon1DialogueBindings.cs:45 |
| `OnCollectibleGathered` | 435 | **1** Assets/_Project/Scripts/Integration/Phase2Stubs.cs:117 | **0** _(none)_ |
| `OnHUDFlashRSGain` | 291 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:156; Assets/_Project/Scripts/UI/HUDController.cs:192 |
| `OnHUDHideBossHealth` | 273 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:153; Assets/_Project/Scripts/UI/HUDController.cs:189 |
| `OnHUDShowBossHealth` | 261 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:151; Assets/_Project/Scripts/UI/HUDController.cs:187 |
| `OnHUDShowBossNameplate` | 297 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:157; Assets/_Project/Scripts/UI/HUDController.cs:193 |
| `OnHUDShowCorruptionWhisper` | 309 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:159; Assets/_Project/Scripts/UI/HUDController.cs:195 |
| `OnHUDShowEnemyBark` | 303 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:158; Assets/_Project/Scripts/UI/HUDController.cs:194 |
| `OnHUDShowMoonTrophy` | 255 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:150; Assets/_Project/Scripts/UI/HUDController.cs:186 |
| `OnHUDUpdateBossHealth` | 267 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDController.cs:152; Assets/_Project/Scripts/UI/HUDController.cs:188 |
| `OnNewGamePlusStarted` | 359 | **1** Assets/_Project/Scripts/Save/NewGamePlusSystem.cs:118 | **0** _(none)_ |
| `OnPerformanceFallback` | 347 | **2** Assets/_Project/Scripts/Core/GameBootstrap.cs:238; Assets/_Project/Scripts/Core/GameBootstrap.cs:262 | **0** _(none)_ |
| `OnPermanentUnlockEarned` | 360 | **1** Assets/_Project/Scripts/Save/NewGamePlusSystem.cs:205 | **0** _(none)_ |
| `OnPlayerDeath` | 130 | **2** Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:170; Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:171 | **0** _(none)_ |
| `OnPlayerHealthChanged` | 143 | **0** _(none)_ | **2** Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:26; Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:38 |
| `OnPlayerRespawned` | 137 | **2** Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:214; Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:216 | **0** _(none)_ |
| `OnQuestObjectiveProgressed` | 95 | **1** Assets/_Project/Scripts/Integration/QuestManager.cs:343 | **0** _(none)_ |
| `OnRequestActivateRSBuff` | 31 | **1** Assets/_Project/Scripts/Gameplay/CraftingSystem.cs:226 | **0** _(none)_ |
| `OnTuningNodeActivated` | 438 | **1** Assets/_Project/Scripts/Integration/Phase2Stubs.cs:126 | **0** _(none)_ |
| `OnTuningProgress` | 454 | **0** _(none)_ | **2** Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:432; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:443 |
| `OnWeatherHazardEnded` | 349 | **1** Assets/_Project/Scripts/Gameplay/WeatherHazardSystem.cs:151 | **0** _(none)_ |
| `OnWeatherHazardStarted` | 348 | **1** Assets/_Project/Scripts/Gameplay/WeatherHazardSystem.cs:130 | **0** _(none)_ |

## Top broken events - recommended fixes

### `OnAetherEnergyChanged` (pub=0, sub=2)
- Decl: `GameEvents.cs:149`
- Helpers: `RaiseAetherEnergyChanged`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:27; Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:39
- Recommended fix: **Subscribers wait forever.** Fix: AetherEnergyController.UpdateAether should call `GameEvents.RaiseAetherEnergyChanged(value)` when value changes; HUD aether meter stays stuck without it.

### `OnTuningProgress` (pub=0, sub=2)
- Decl: `GameEvents.cs:454`
- Helpers: `FireTuningProgress`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:432; Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:443
- Recommended fix: Subscribers wait forever. `FireTuningProgress` callers exist but no per-frame subscriber. Confirmed broken - HUD frequency wheel listens to `OnHUDUpdateFrequencyWheel` instead. Either delete `OnTuningProgress` (dead alt path) or migrate consumers.

### `OnPlayerRespawned` (pub=2, sub=0)
- Decl: `GameEvents.cs:137`
- Helpers: `RaisePlayerRespawned`
- Publishers: Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:214; Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:216
- Subscribers: _(none)_
- Recommended fix: **Publishers fire into void.** `RaisePlayerRespawned` called from respawn logic but nothing listens. Fix: HUDController subscribe to clear death overlay; PlayerInputHandler to re-enable input.

### `OnPlayerHealthChanged` (pub=0, sub=2)
- Decl: `GameEvents.cs:143`
- Helpers: `RaisePlayerHealthChanged`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:26; Assets/_Project/Scripts/UI/HUDLiveDataWiring.cs:38
- Recommended fix: **Subscribers wait forever.** HUD health bar listens but the change event is never raised. Fix: PlayerHealthController should call `GameEvents.RaisePlayerHealthChanged(current, max)` inside its damage / heal paths. Currently the HUD only updates via the typed `OnPlayerDamaged` route, missing heals.

### `OnPlayerDeath` (pub=2, sub=0)
- Decl: `GameEvents.cs:130`
- Helpers: `RaisePlayerDeath`
- Publishers: Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:170; Assets/_Project/Scripts/Gameplay/PlayerHealthController.cs:171
- Subscribers: _(none)_
- Recommended fix: **Publishers fire into void.** `RaisePlayerDeath` is called from PlayerHealth, but no subscriber re-enables player respawn UI / camera fade. Fix: subscribe HUDController.HandlePlayerDeath and CameraController.OnPlayerDeath.

### `OnPerformanceFallback` (pub=2, sub=0)
- Decl: `GameEvents.cs:347`
- Helpers: `FirePerformanceFallback`
- Publishers: Assets/_Project/Scripts/Core/GameBootstrap.cs:238; Assets/_Project/Scripts/Core/GameBootstrap.cs:262
- Subscribers: _(none)_
- Recommended fix: Publishers in PerformanceManager fire but no overlay/diagnostic subscriber. Fix: wire diagnostic HUD or remove if fallback is silent-only.

### `OnHUDShowMoonTrophy` (pub=0, sub=2)
- Decl: `GameEvents.cs:255`
- Helpers: `RaiseHUDShowMoonTrophy`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:150; Assets/_Project/Scripts/UI/HUDController.cs:186
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDShowMoonTrophy(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDShowMoonTrophy`). Until then, the listed HUD feature is dead UI code.

### `OnHUDShowEnemyBark` (pub=0, sub=2)
- Decl: `GameEvents.cs:303`
- Helpers: `RaiseHUDShowEnemyBark`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:158; Assets/_Project/Scripts/UI/HUDController.cs:194
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDShowEnemyBark(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDShowEnemyBark`). Until then, the listed HUD feature is dead UI code.

### `OnHUDShowCorruptionWhisper` (pub=0, sub=2)
- Decl: `GameEvents.cs:309`
- Helpers: `RaiseHUDShowCorruptionWhisper`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:159; Assets/_Project/Scripts/UI/HUDController.cs:195
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDShowCorruptionWhisper(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDShowCorruptionWhisper`). Until then, the listed HUD feature is dead UI code.

### `OnHUDUpdateBossHealth` (pub=0, sub=2)
- Decl: `GameEvents.cs:267`
- Helpers: `RaiseHUDUpdateBossHealth`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:152; Assets/_Project/Scripts/UI/HUDController.cs:188
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDUpdateBossHealth(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDUpdateBossHealth`). Until then, the listed HUD feature is dead UI code.

### `OnHUDShowBossHealth` (pub=0, sub=2)
- Decl: `GameEvents.cs:261`
- Helpers: `RaiseHUDShowBossHealth`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:151; Assets/_Project/Scripts/UI/HUDController.cs:187
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDShowBossHealth(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDShowBossHealth`). Until then, the listed HUD feature is dead UI code.

### `OnHUDHideBossHealth` (pub=0, sub=2)
- Decl: `GameEvents.cs:273`
- Helpers: `RaiseHUDHideBossHealth`
- Publishers: _(none)_
- Subscribers: Assets/_Project/Scripts/UI/HUDController.cs:153; Assets/_Project/Scripts/UI/HUDController.cs:189
- Recommended fix: **Subscribers wait forever.** HUDController.cs (and HUDFreeFunctions.cs) listen, but no system calls `RaiseHUDHideBossHealth(...)`. Fix: wire the publisher in the Integration system that owns the trigger (e.g. for `OnHUDShowBossNameplate`, the boss-encounter trigger should call `GameEvents.RaiseHUDHideBossHealth`). Until then, the listed HUD feature is dead UI code.

## Unused events - prune or wire

- `OnDialogueStateChanged` (decl `GameEvents.cs:210`, helper `RaiseDialogueStateChanged`) - no publisher, no subscriber.
- `OnMoonUnlocked` (decl `GameEvents.cs:185`, helper `RaiseMoonUnlocked`) - no publisher, no subscriber.
- `OnRemotePushNotificationReceived` (decl `GameEvents.cs:331`, helper `FireRemotePushNotification`) - no publisher, no subscriber.
- `OnTartarianHourChanged` (decl `GameEvents.cs:450`, helper `FireTartarianHourChanged`) - no publisher, no subscriber.

These are dead declarations. Either delete the event + helper, or document why they are preserved for forward compatibility (e.g. `OnTartarianHourChanged` is referenced as a planned hook by Moon 1 day-cycle smoke tests - has a `FireTartarianHourChanged` helper but nothing calls it). `OnMoonUnlocked` and `OnDialogueStateChanged` are typed-modern variants for which the legacy events `OnMoonCleared` / direct DialogueManager calls still carry traffic.

## Constraints + provenance

- Docs-only audit. No source files modified.
- Generator script: `scripts/audits/sprint11_l9_gameevents_pairs.ps1` + `scripts/audits/sprint11_l9_render_markdown.ps1`. Re-run from this worktree to refresh.
- All file:line citations are from `git ls-files` at branch `agent/audit/gameevents-pairs` head.

---
*Sprint 11 Lane 9 - 2026-06-02*
