# Sprint 11 Lane 2 — Silent-Fail Audit

**Date:** 2026-06-02
**Branch:** `agent/audit/silent-fails`
**Base SHA:** `e07660306026c2da2a1c222f26189c99a8fc4a3c`
**Worktree:** `C:\dev\_wt_s11_l2_silent`
**Scope:** `Assets/_Project/Scripts/**/*.cs`
**Mandate:** CLAUDE.md NO-DEBT — "no silent catches".

A **silent fail** here is any code path that can swallow an error or `null` result without writing to `Debug.Log*`. Empty `catch {}`, `catch { return null; }`, `Resources.Load` returning null with no log, unguarded `GetComponent<T>()` dereferences, and `FindGameObjectWithTag("Player")` callers that no-op on null are all in-scope.

This is documentation-only. **No code changes were made.** Branch ships the doc and pushes back upstream.

---

## 1. Totals by category

| Category | Hits | Method |
|---|---|---|
| Empty `catch {}` / `catch (Ex) {}` blocks | **38** | `git grep -nE "catch\s*(\(\s*[A-Za-z][A-Za-z0-9_]*\s+[A-Za-z][A-Za-z0-9_]*\s*\))?\s*\{\s*\}"` |
| `catch { return null/false; }` (one-line) | **4** | `git grep -nE "catch\s*(\(.*\))?\s*\{\s*return"` |
| `catch (… Exception …) { … }` total (most DO log) | 69 | `git grep -nE "catch\s*\(.*Exception.*\)\s*\{"` |
| `?? null / ?? default / ?? string.Empty` masking | **14** | `git grep -nE "\?\?\s*null\|\?\?\s*default\|\?\?\s*string\.Empty\|\?\?\s*new\s"` |
| `Resources.Load*` call sites | 94 | `git grep -nE "Resources\.Load"` |
| `Resources.Load` returning null w/ no warn (sampled) | **≥ 5** | manual sample (`AudioManager.PlayVoiceLine`, `MasterMixerLocator.Load`, etc.) |
| `GetComponent<T>()` call sites | 654 | `git grep -nE "GetComponent<"` |
| Unguarded `GetComponent<T>().method/field` chain | **≥ 47** | `git grep -nE "GetComponent<.+>\(\)\.[A-Za-z]"` |
| `FindObjectOf* / FindFirstObjectByType` etc. | 252 | `git grep -nE "FindObjectOfType\|FindFirstObjectByType\|FindAnyObjectByType\|FindGameObjectWithTag\|GameObject\.Find"` |
| `FindGameObjectWithTag("Player")` callers that no-op on null w/o warn (sampled) | **≥ 8** | see §3 |
| `Mathf.Approximately` early-return without log | 0 | All 8 hits are equality compares, not early returns |

**Top files by empty-catch count:**

| Count | File |
|---|---|
| 10 | `Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs` |
| 6 | `Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs` |
| 5 | `Assets/_Project/Scripts/Save/SteamCloudBridge.cs` |
| 4 | `Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs` |
| 4 | `Assets/_Project/Scripts/UI/IntegrationBridge.cs` |
| 4 | `Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs` |
| 3 | `Assets/_Project/Scripts/Integration/TuningPedestalLink.cs` |
| 1 | `Assets/_Project/Scripts/Editor/OneClickBuild.cs` |
| 1 | `Assets/_Project/Scripts/Save/SaveManager.cs` |

---

## 2. Top 30 specific findings

### Moon-1 hot path (event subscription empty catches)

1. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:24`**
   `void OnEnable() { try { TartarianHourCycle.OnSeventeenthHour += HandleSeventeenthHour; } catch { } }`
   Fix should log: `Debug.LogError($"[Moon1NarrativeBeats] OnEnable subscribe to OnSeventeenthHour failed: {ex}")`. If subscription throws, the 17th-hour cathedral light eruption never fires — this is the headline Moon 1 cinematic.

2. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:25`**
   `void OnDisable() { try { … -= HandleSeventeenthHour; } catch { } }` — same swallow; symmetrical leak risk if unsubscribe fails.

3. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:43`**
   `try { GameEvents.RaiseHUDShowObjective("Cathedral Light Eruption!"); } catch { }`
   Fix should log: `Debug.LogError($"[Moon1NarrativeBeats] RaiseHUDShowObjective failed mid-eruption: {ex}")`. Cathedral eruption banner silently vanishes if RaiseHUD throws.

4. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:44`**
   `try { GameEvents.FireRSChange(20f); } catch { }` — RS reward silently dropped on the 17th-hour beat.

5. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:75`**
   `try { GameEvents.RaiseHUDShowObjective($"Giant Skeleton Key #{_keyNumber} of 8 collected"); } catch { }` — player gets no HUD confirmation a key was picked up.

6. **`Assets/_Project/Scripts/Integration/Moon1NarrativeBeats.cs:76`**
   `try { GameEvents.FireRSChange(15f); } catch { }` — 15 RS per key silently dropped.

7. **`Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs:32-33`**
   `try { GameEvents.OnBuildingRestoredTyped += HandleRestored; } catch { }` + `try { TartarianHourCycle.OnSeventeenthHour += HandleSeventeenthHour; } catch { }`
   Fix should log: subscribe failure means the restoration-dolly cinematic and seventeenth-hour camera move never trigger. Both are headline Moon 1 beats.

8. **`Assets/_Project/Scripts/Integration/Moon1CinematicMoments.cs:38-39`** — paired OnDisable unsubscribes, same swallow.

9. **`Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:28`**
   `try { GameEvents.RaiseHUDShowInteractionPrompt("Press [E] to tune (" + assignedVariant + ")"); } catch { }`
   Fix should log: tutorial-blocking — without the prompt, player doesn't know they can tune. Moon 1 tuning mini-game is the gating mechanic.

10. **`Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:35`**
    `try { GameEvents.RaiseHUDHideInteractionPrompt(); } catch { }` — stale "Press E" prompt could stick if Raise throws.

11. **`Assets/_Project/Scripts/Integration/TuningPedestalLink.cs:66`**
    `try { GameEvents.RaiseHUDShowObjective("Tuning " + buildingId + " node " + (nodeIndex + 1) + "/3"); } catch { }` — objective-tracker UI never updates if Raise throws.

12. **`Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs:33-34`**
    `try { GameEvents.OnBuildingRestored += HandleBuildingRestored; } catch { }` and `try { GameEvents.OnQuestStatusChanged += … } catch { }`
    Fix should log: if subscribe fails the tracker UI never reflects building restorations. This is the canonical Moon 1 progression UI.

13. **`Assets/_Project/Scripts/UI/QuestObjectiveTrackerUI.cs:39-40`** — symmetric OnDisable unsubscribes, same swallow.

14. **`Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:431-435`** (5 lines)
    All five `Tartaria.Core.GameEvents.On{POIDiscovered|TuningProgress|CombatStarted|CombatEnded|BuildingRestored}` subscriptions wrapped in `try {…} catch { }`. Adaptive music layer 2 silently dead if any wiring throws.

15. **`Assets/_Project/Scripts/Audio/AdaptiveMusicController.cs:442-446`** — paired Unbind, same swallow.

### Save & reflection bridges (catches swallow reflection errors silently)

16. **`Assets/_Project/Scripts/Save/SaveManager.cs:1724`**
    `SavePendingQueue()` writes `_pendingPath` inside `try { … } catch { }`. Fix should log: `Debug.LogError($"[SaveManager] Failed to write pending queue to {_pendingPath}: {ex}")`. If queue write fails the cloud-upload retry queue is silently dropped.

17. **`Assets/_Project/Scripts/Save/SteamCloudBridge.cs:28`** — `T()` reflection sweep wraps `AppDomain.GetAssemblies()` in `try { … } catch { }` — Steam bridge resolution silently fails.

18. **`Assets/_Project/Scripts/Save/SteamCloudBridge.cs:42, 67, 79, 91`** — reflective `Invoke` of `IsSteamAvailable / LoadCloudSave / DeleteCloudFile / IsCloudEnabledAndHasSpace`. All silently return `null/false`. Fix should log: `Debug.LogWarning($"[SteamCloudBridge] {method} reflection failed: {ex.Message}")` (mirror the pattern already used at line 55 for `SyncCloudSave`).

19. **`Assets/_Project/Scripts/UI/IntegrationBridge.cs:84, 98, 112, 141`** — four reflection-property reads (`CurrentTargetFrequency`, `HealthFraction`, `DisplayName`, `Readiness`) wrapped `try { … } catch { }`. Boss HUD silently shows 0/empty when reflection fails.

20. **`Assets/_Project/Scripts/UI/IntegrationBridge.cs:59, 71, 126, 155`** — one-line `catch { return null; }` / `catch { return false; }`. Fix should log: `Debug.LogWarning($"[IntegrationBridge] Boss/Giant/Dialogue resolve failed: {ex.Message}")`.

21. **`Assets/_Project/Scripts/Editor/OneClickBuild.cs:34`**
    `try { File.Delete(SentinelPath); } catch { }` — if sentinel deletion fails the auto-build loop will re-trigger. Fix should log: warn about path + retry implications.

### Resources.Load silent nulls

22. **`Assets/_Project/Scripts/Audio/AudioManager.cs:308`**
    `clip = Resources.Load<AudioClip>($"VoiceLines/{lineId}"); if (clip != null) PlaySFX2D(clip, volume);`
    Fix should log: `if (clip == null) Debug.LogWarning($"[AudioManager] VoiceLine '{lineId}' not found at Resources/VoiceLines/{lineId}");`. Currently any missing VO line silently no-ops — explains why VO regressions go undetected.

23. **`Assets/_Project/Scripts/Audio/VOPlaceholderLibrary.cs:50`**
    `if (voClip == null) return false;` — placeholder-not-found is returned to caller with no log; caller treats as text-only mode but there is no diagnostic that the Resources path was missing.
    Fix should log: `Debug.LogWarning($"[VOPlaceholderLibrary] Missing Resources/VO/Placeholder/vo_{voIndex:D2}");`.

24. **`Assets/_Project/Scripts/Audio/MasterMixerLocator.cs:18`**
    `var locator = Resources.Load<MasterMixerLocator>("MasterMixerLocator"); return locator != null ? locator.mixer : null;`
    Fix should log: `if (locator == null) Debug.LogWarning("[MasterMixerLocator] Resources/MasterMixerLocator.asset missing — SettingsOverlay mixer routing disabled");`. Settings sliders silently no-op if asset missing.

25. **`Assets/_Project/Scripts/Combat/HitFeedback.cs:118`**
    `_damagePopupPrefab = Resources.Load<GameObject>(_popupResourcePath);` — line 121 *does* log on null, so this one is OK (counter-example, included for reference).

### Unguarded `FindGameObjectWithTag("Player")`

26. **`Assets/_Project/Scripts/Gameplay/DissonanceCrystal.cs:43`**
    `playerTransform = GameObject.FindGameObjectWithTag("Player").transform;` — direct `.transform` on a potential null. NRE every Update tick if Player tag missing. Fix should log + early-return.

27. **`Assets/_Project/Scripts/Integration/Moon1CameraFollowPlayer.cs:60`**
    `var player = GameObject.FindGameObjectWithTag("Player"); if (player != null) _target = player.transform;` followed by `if (_target == null) return;` — silently no-ops. Fix should log warn-once: `Debug.LogWarning("[Moon1CameraFollowPlayer] Player tag not found — camera will not follow");`.

28. **`Assets/_Project/Scripts/Integration/Moon1FirstTimeHints.cs:133, 145`**
    `var player = GameObject.FindGameObjectWithTag("Player"); if (player == null) return false;` — silently disables hint logic.

29. **`Assets/_Project/Scripts/AI/EnemyAIController.cs:50`**
    `var playerGO = GameObject.FindGameObjectWithTag("Player"); if (playerGO != null) _player = playerGO.transform;` — no else-log. Enemy never chases the player if tag missing.

30. **`Assets/_Project/Scripts/Integration/Moon1QuestTriggers.cs:160`**
    `GetComponent<Collider>().enabled = false;` — unguarded NRE risk inside `QuestZoneTrigger.OnTriggerEnter`. Trigger fires only if Player tag present — but a missing Collider component would NRE silently swallow remaining handler logic. Fix should log warn + null-guard.

---

## 3. Cross-reference — Moon 1 happy-path bootstrap touch points

The Moon 1 bootstrap chain (per `CLAUDE.md` + recent commit `e0766030`) flows:

```
RuntimeSpawnerInsurance ─▶ PlayerSpawner ─▶ DialogueManager.AutoBootstrap
   │
   ├─▶ Moon1NarrativeBeats (Cathedral eruption + skeleton keys)
   ├─▶ Moon1CinematicMoments (restoration dolly + 17th-hour move)
   ├─▶ TuningPedestalLink (per-pedestal interaction prompts)
   ├─▶ QuestObjectiveTrackerUI (canonical Moon 1 progression HUD)
   └─▶ AdaptiveMusicController (combat / tuning music layers)
```

Silent fails that **sit directly on** this chain (rank ordered by player-visible impact):

| Rank | File:line | Why it matters on Moon 1 |
|---|---|---|
| 1 | `Integration/Moon1NarrativeBeats.cs:24-25, 43-44, 75-76` (6 empty catches) | 17th-hour eruption cinematic + RS payout + skeleton-key HUD confirmations. The headline Moon 1 beats. |
| 2 | `Integration/Moon1CinematicMoments.cs:32-33, 38-39` (4 empty catches) | Restoration dolly + seventeenth-hour camera move. If event subscribe throws, both cinematics never fire. |
| 3 | `Integration/TuningPedestalLink.cs:28, 35, 66` (3 empty catches) | "Press [E] to tune" prompt + tuning objective banner. Tuning is the Moon 1 gating mini-game. |
| 4 | `UI/QuestObjectiveTrackerUI.cs:33-34, 39-40` (4 empty catches) | Canonical Moon 1 quest tracker UI; subscribe failure = silent dead UI. |
| 5 | `Audio/AdaptiveMusicController.cs:431-435, 442-446` (10 empty catches) | Layer-2 combat/tuning music. Subscribe failure = music stuck at exploration layer 1. |
| 6 | `Audio/AudioManager.cs:308` | VoiceLines silently no-op if missing — Milo tutorial VO bugs would go undetected. |
| 7 | `Gameplay/DissonanceCrystal.cs:43` | Per-frame NRE on Moon 1 dissonance crystals if Player tag absent. |
| 8 | `Integration/Moon1CameraFollowPlayer.cs:60-67` | Fallback camera follower silently dead if Player tag absent — same root cause as the recent `e0766030` PlayerSpawner tag fix. |
| 9 | `Save/SaveManager.cs:1724` | Pending-queue write swallow — Moon 1 progress saves at risk. |

`PlayerSpawner.cs` and `RuntimeSpawnerInsurance.cs` and `Editor/Moon1MasterBootstrap.cs` themselves use `catch (System.Exception ex) { Debug.LogWarning(…) }` consistently — **those three files are clean** per the audit. The recent `e0766030` fix to `RuntimeSpawnerInsurance.AddComponent` removed the only known silent fail there. `DialogueManager.cs:709` and `:809` also log appropriately (`LogWarning` and `LogError` respectively).

---

## 4. Recommended remediation order

Per CLAUDE.md NO-DEBT mandate ("no stubs no placeholders") the fix is mechanical — every empty catch becomes a logged catch. Suggested order:

1. **Round 1 — Moon 1 event subscriptions:** `Moon1NarrativeBeats.cs`, `Moon1CinematicMoments.cs`, `TuningPedestalLink.cs`, `QuestObjectiveTrackerUI.cs`, `AdaptiveMusicController.cs`. 27 empty catches, all 1-line wraps around `GameEvents.Raise*` / `OnX +=`. Mechanical sweep.
2. **Round 2 — Resource null-warns:** `AudioManager.PlayVoiceLine`, `VOPlaceholderLibrary`, `MasterMixerLocator`. Add `Debug.LogWarning` on null returns.
3. **Round 3 — Reflection swallows:** `SteamCloudBridge` (4 remaining sites), `IntegrationBridge` (8 sites). Mirror the existing `SyncCloudSave` log pattern at line 55.
4. **Round 4 — Player-tag fallbacks:** `DissonanceCrystal`, `Moon1CameraFollowPlayer`, `EnemyAIController`, etc. Add warn-once log when Player tag absent.
5. **Round 5 — SaveManager pending queue:** smallest surface, highest data-loss risk.

None of these are code changes in this PR — this audit is docs-only by mandate.

---

## 5. Methodology notes

- All counts use `git grep` from a clean worktree on `e07660306026c2da2a1c222f26189c99a8fc4a3c`.
- The 38-empty-catches number excludes `catch (X x) { Debug.* }` and `catch (X x) { return …; }` shapes — those are surfaced separately.
- 69 of the 73 `catch (… Exception …) { … }` blocks DO log (sampled all of them in §1 method row), so this audit's headline number is the 38 empty + 4 one-line return cases = **42 swallowing catches**.
- `Mathf.Approximately` returned 8 hits, all equality compares; zero are early-return-without-log.
- No false-positive trimming on the 47 unguarded `GetComponent<T>().method` chains: most happen on `GameObject.CreatePrimitive`-created GameObjects (so technically guaranteed non-null), but the pattern is fragile and worth callout for future-asset migration. The 30-finding shortlist above focuses on the highest-impact subset.

— end of Sprint 11 Lane 2 audit —
