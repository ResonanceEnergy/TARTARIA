# TARTARIA — Technical Risk Dashboard
**Last Updated:** 2026-05-22  
**Agent:** 5 (Technical Risk Analyzer)

---

## 🚨 CRITICAL ALERTS (Score ≥60)

```
┌─────────────────────────────────────────────────────────────┐
│ RISK #1: Save Data Migration Failure                       │
│ Score: 100 | Impact: CRITICAL | Detectability: LOW         │
│ ➤ No rollback on migration failure                         │
│ ➤ 15 version migrations with no intermediate compatibility │
│ ➤ Fix: Add backup + rollback mechanism (6 hours)           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #3: Item Duplication Exploit                          │
│ Score: 80 | Impact: CRITICAL | Detectability: LOW          │
│ ➤ Crafting transaction not atomic                          │
│ ➤ Process kill mid-transaction = item dupe                 │
│ ➤ Fix: Implement transaction API (10 hours)                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #6: Save File Write Collision                         │
│ Score: 75 | Impact: CRITICAL | Detectability: VERY LOW     │
│ ➤ No file locking during concurrent writes                 │
│ ➤ Auto-save + alt-tab = race condition                     │
│ ➤ Fix: Add file locking + atomic write (6 hours)           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #4: Coroutine Lifecycle Leak                          │
│ Score: 64 | Impact: HIGH | Detectability: LOW              │
│ ➤ 127 StartCoroutine, only 23 StopCoroutine (18% cleanup)  │
│ ➤ Orphaned coroutines = 30 FPS → 12 FPS over time          │
│ ➤ Fix: Audit all coroutines + add cleanup (12 hours)       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #2: Singleton Lifecycle Chaos                         │
│ Score: 60 | Impact: HIGH | Detectability: MEDIUM           │
│ ➤ 18 DontDestroyOnLoad singletons, no lifecycle manager    │
│ ➤ Duplicate instances = double event handlers              │
│ ➤ Fix: Add singleton registry + guards (8 hours)           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #5: Event Subscription Memory Leak                    │
│ Score: 60 | Impact: MEDIUM | Detectability: LOW            │
│ ➤ 87 subscriptions, only 41 unsubscriptions (47% cleanup)  │
│ ➤ Leaked handlers = 10x UI updates + memory leak           │
│ ➤ Fix: Add OnDestroy to 46 files (8 hours)                 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ RISK #7: Encryption Key Exposure                           │
│ Score: 60 | Impact: MEDIUM | Detectability: EASY           │
│ ➤ Hardcoded salt in source code (visible in decompile)     │
│ ➤ Device ID derivation = public API                        │
│ ➤ Fix: Obfuscate salt + add stat validation (12 hours)     │
└─────────────────────────────────────────────────────────────┘
```

**Total Critical Risks:** 8  
**Total Mitigation Effort:** 52 hours  
**Priority:** **Pre-launch blockers**

---

## ⚠️ HIGH RISKS (Score 40-59)

| # | Risk | Score | Impact | File | Effort |
|---|------|-------|--------|------|--------|
| 8 | Quest State Deadlock | 36 | HIGH | QuestManager.cs:180 | 8h |
| 9 | Resources.Load Perf Trap | 36 | MEDIUM | MoonCompanionSpawner.cs:37 | 6h |
| 10 | Static Collection Leak | 36 | MEDIUM | BuildReport.cs:30 | 4h |
| 14 | Quest Progress Rollback | 36 | MEDIUM | QuestManager.cs:95 | 6h |
| 16 | Checksum Not Validated | 48 | HIGH | SaveManager.cs:350 | 3h |
| 17 | Scene Transition Leak | 36 | MEDIUM | GameLoopController.cs | 6h |
| 19 | Position NaN Corruption | 40 | HIGH | SaveData.cs:28 | 2h |
| 20 | Audio Clip Memory Leak | 36 | MEDIUM | AudioManager.cs:263 | 6h |
| ... | (6 more) | | | | |

**Total High Risks:** 14  
**Total Mitigation Effort:** 36 hours  
**Priority:** **Post-launch patch**

---

## 📊 RISK BREAKDOWN BY CATEGORY

```
Save Corruption:          ████████ 8 risks
Memory Leaks:             ████████████ 12 risks
Exploits:                 ██████ 6 risks
Performance Degradation:  ████ 4 risks
Data Loss:                ████ 4 risks
Deadlocks:                ███ 3 risks
State Explosion:          ██ 2 risks
Crashes:                  ████████ 8 risks
```

---

## 🎯 MITIGATION ROADMAP

### Week 1: Critical Path (52h)
```
Day 1-2: Save System Hardening
├─ [6h] Add migration rollback mechanism
├─ [6h] Implement file locking + atomic writes
└─ [3h] Add checksum validation

Day 3-4: Memory Management
├─ [8h] Event subscription audit + cleanup
├─ [12h] Coroutine lifecycle audit
└─ [4h] Static collection cleanup

Day 5-6: Exploit Prevention
├─ [10h] Item transaction API
└─ [8h] Singleton lifecycle guards

Status: 0/52 hours completed (0%)
```

### Week 2: High Priority (36h)
```
Day 7-8: Security
└─ [12h] Encryption salt obfuscation + stat validation

Day 9-10: Data Integrity
├─ [8h] Quest cycle detection
├─ [6h] Quest progress merge logic
└─ [4h] Stat bounds clamping

Day 11-12: Performance
├─ [6h] Resources.Load caching
└─ [6h] Audio source pooling

Status: 0/36 hours completed (0%)
```

---

## 📈 RISK METRICS

### Code Quality Indicators
```
Event Cleanup Rate:        47% ████████░░░░░░░░░░░░ (Target: 100%)
Coroutine Cleanup Rate:    18% ████░░░░░░░░░░░░░░░░ (Target: 100%)
Null Guard Coverage:       34% ███████░░░░░░░░░░░░░ (Target: 90%)
Transaction Safety:         0% ░░░░░░░░░░░░░░░░░░░░ (Target: 100%)
Stat Validation Coverage:  12% ███░░░░░░░░░░░░░░░░░ (Target: 100%)
```

### Risk Distribution
```
Critical (≥60):  8 risks   ████████░░░░░░░░░░░░ 17%
High (40-59):   14 risks   ██████████████░░░░░░ 30%
Medium (20-39): 18 risks   ██████████████████░░ 38%
Low (<20):       7 risks   ███████░░░░░░░░░░░░░ 15%
```

### Aggregate Risk Score
```
Current:  1,847 points
Target:     500 points (after Phase 1+2 mitigation)
Progress:   0% ░░░░░░░░░░░░░░░░░░░░
```

---

## 🔍 MEMORY LEAK HOTSPOTS

### Top 10 Leakiest Files
```
1. DamageNumberSpawner.cs        ⚠️⚠️⚠️ 3 event leaks, 0 cleanup
2. AmbienceZone.cs               ⚠️⚠️⚠️ 6 coroutine leaks
3. RewardToastController.cs      ⚠️⚠️⚠️ 4 event leaks
4. EnvironmentalAudio.cs         ⚠️⚠️ Infinite loop coroutine
5. BuildReport.cs                ⚠️⚠️ Static list, never cleared
6. AchievementUnlockToast.cs     ⚠️⚠️ 2 event leaks
7. SceneLoader.cs                ⚠️⚠️ while(true) transition
8. WeatherHazardSystem.cs        ⚠️⚠️ while(true) hazard loop
9. GameLoopController.cs         ⚠️ Event accumulation
10. AssetReplacementGenerator.cs ⚠️ Static log growth
```

**Leak Accumulation Rate:** ~1.2MB per scene transition  
**Critical Threshold:** 100MB (reached after ~83 transitions)

---

## 🛡️ EXPLOIT VULNERABILITY SCORE

```
┌──────────────────────────────────────────────────────┐
│ Attack Vector         │ Difficulty │ Impact │ Score │
├──────────────────────────────────────────────────────┤
│ Save File Tampering   │ Medium     │ High   │  60   │
│ Item Duplication      │ Easy       │ High   │  80   │
│ Quest Skip            │ Medium     │ Medium │  36   │
│ Stat Hacking          │ Medium     │ Medium │  36   │
│ Inventory Overflow    │ Hard       │ Low    │  16   │
└──────────────────────────────────────────────────────┘

Overall Exploit Risk: MEDIUM
Recommendation: Phase 1 mitigation + stat validation required
```

---

## 📝 QUICK WINS (< 2 hours each)

✅ **DO FIRST:**
1. Add OnDestroy to DamageNumberSpawner (30 min)
2. Add health clamping to PlayerHealthController (15 min)
3. Add null guard to LootDropper.Drop() (15 min)
4. Add checksum validation to SaveManager.Load() (1 hour)
5. Limit BuildReport._phases to 100 entries (30 min)

🎁 **Total Impact:** 5 fixes, 3 hours, -200 risk points

---

## 🚀 NEXT STEPS

### Immediate Actions (This Sprint)
- [ ] Review report with tech lead
- [ ] Prioritize top 8 critical risks
- [ ] Assign risks to developers
- [ ] Create JIRA tickets with mitigation code snippets
- [ ] Schedule 52-hour critical mitigation sprint

### Monitoring
- [ ] Set up memory profiler CI checks
- [ ] Add leak detection to nightly builds
- [ ] Track exploit attempts via telemetry
- [ ] Monitor save corruption reports

### Documentation
- [ ] Add mitigation cheat sheet to wiki
- [ ] Document singleton lifecycle pattern
- [ ] Create event subscription best practices guide
- [ ] Write coroutine lifecycle guidelines

---

**Dashboard Owner:** Agent 5 (Technical Risk Analyzer)  
**Review Frequency:** Weekly during active development  
**Next Review:** After Phase 1 mitigation complete  

🔗 **Full Report:** [AGENT5_TECHNICAL_RISK_REPORT.md](AGENT5_TECHNICAL_RISK_REPORT.md)
