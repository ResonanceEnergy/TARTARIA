# AGENT 8: TECH DEBT QUICK REFERENCE

**Last Updated:** 2026-05-24  
**Full Report:** [LIVEOPS_AGENT8_TECH_DEBT_REPORT.md](LIVEOPS_AGENT8_TECH_DEBT_REPORT.md)

---

## 🔴 P0 CRITICAL DEBT (4 BLOCKERS)

### 1. Circular Dependency (DialogueNodeData)
- **Impact:** Blocks dialogue system expansion
- **Files:** DialogueNodeData.cs (5 TODOs)
- **Fix:** Extract `IDialogueCondition` interface to Core
- **Effort:** 40 hours (Sprint 1)
- **Deadline:** End of Q3 2026

### 2. Non-Thread-Safe Singletons (23 classes)
- **Impact:** Race conditions, no multiplayer support
- **Files:** SaveManager, AudioManager, UIManager, +20 more
- **Fix:** Convert to `RuntimeInitializeOnLoadMethod` bootstrap
- **Effort:** 80 hours (Sprint 3)
- **Deadline:** End of Q3 2026

### 3. God Objects (3 classes)
- **Impact:** 50% velocity reduction, untestable
- **Files:** Moon10ContentSpawner (1600 lines), AudioManager (600 lines), UIManager (150 lines)
- **Fix:** Split into single-responsibility classes
- **Effort:** 120 hours (Sprints 1-2)
- **Deadline:** End of Q3 2026

### 4. Magic Numbers (500+ occurrences)
- **Impact:** Impossible to tune, copy-paste errors
- **Files:** All Moon spawners, combat, progression
- **Fix:** Extract to `GameBalanceConfig` ScriptableObject
- **Effort:** 60 hours (Sprint 2)
- **Deadline:** End of Q3 2026

---

## 📊 DEBT TOTALS

| Category | Count | Priority | Effort |
|----------|-------|----------|--------|
| TODO/FIXME Comments | 90+ | P1-P2 | 120h |
| Legacy Systems | 137 | P0-P2 | 200h |
| Code Duplication | 45+ | P1 | 80h |
| Architectural Issues | 23 | P0 | 160h |
| Magic Numbers | 500+ | P0 | 60h |
| God Objects | 3 | P0 | 120h |
| Missing Patterns | 3 | P1 | 80h |
| Test Coverage Gaps | ~40% | P2 | 240h |
| **TOTAL** | **237+** | — | **1,060h** |

---

## 🗓️ REDUCTION ROADMAP

### Q3 2026: P0 ELIMINATION (320 hours)
- ✅ **Sprint 1-2:** Split God objects (Moon10, Audio, UI)
- ✅ **Sprint 2:** Extract magic numbers to config
- ✅ **Sprint 3:** Fix singleton thread-safety
- ✅ **Sprint 4:** Consolidate dialogue systems
- **Goal:** Zero P0 blockers, DLC unblocked

### Q4 2026: P1 REDUCTION (240 hours)
- ✅ **Sprint 5:** Migrate legacy quest system, implement Factory pattern
- ✅ **Sprint 6:** Implement Strategy pattern, eliminate duplication
- ✅ **Sprint 7:** Resolve 38 P1 TODOs
- **Goal:** Design patterns complete, legacy systems migrated

### Q1 2027: P2 POLISH (192 hours @ 20% capacity)
- Complete Factory/Command patterns
- UI/Audio test coverage (120h investment)
- Resolve 40 P2 TODOs (polish items)
- **Goal:** 60% test coverage

### Q2 2027: P3 MODERNIZATION (96 hours @ 10% capacity)
- Naming conventions, documentation
- Continuous test improvement
- **Goal:** 80% test coverage, zero untracked TODOs

---

## 🛡️ PREVENTION STRATEGY

### Code Review Checklist (Auto-fail if violated)
- ❌ No new TODO without Jira ticket
- ❌ No magic numbers (use constant/config)
- ❌ No God objects (max 200 lines/class)
- ❌ No non-thread-safe singletons
- ✅ Must have unit test

### CI/CD Debt Gates
```bash
# Fails build if:
- TODO count > 100
- Magic numbers > 10 per file
- Code duplication > 5%
- God objects > 200 lines
```

### Refactor Fridays (20% sprint capacity)
- **Last 8 hours of every sprint reserved for debt**
- Week 1: Tests | Week 2: TODOs | Week 3: Duplication | Week 4: Patterns

### Debt Budget
- **Max 10 new TODOs per sprint**
- 3 sprints over budget → Mandatory debt sprint
- 6 sprints over budget → Feature freeze

---

## 📈 DEBT HEATMAP

```
Integration (Moon spawners)  ██████████████████████ 68 items (29%)
UI (Managers, overlays)      ████████████████       47 items (20%)
Gameplay (Combat, systems)   ████████████           35 items (15%)
Save (Persistence)           ████████               23 items (10%)
Core (Events, state)         ██████                 18 items (8%)
Audio (Manager, SFX)         █████                  15 items (6%)
Data (ScriptableObjects)     ████                   12 items (5%)
Other                        ███████                19 items (8%)
```

---

## 🚨 IMMEDIATE ACTIONS (This Week)

1. **Present roadmap** to engineering team (get Q3 budget approval)
2. **Set up CI/CD gates** (TODO counter, magic number detector)
3. **Create Jira epic** "Tech Debt Q3-Q4 2026" with 237 items
4. **Begin Sprint 1** (Week of 2026-05-27):
   - Assign: Fix DialogueNodeData circular dependency (40h)
   - Assign: Split Moon10ContentSpawner (40h)
5. **Schedule weekly debt review** (Fridays 3-4pm)

---

## 📞 ESCALATION

- **Tech Debt >150 items:** Email tech lead (warning)
- **3 sprints over budget:** Mandatory 1-week debt sprint
- **6 sprints over budget:** Feature freeze, all-hands debt reduction
- **P0 debt not resolved in Q3:** Escalate to CTO

---

## 📚 RELATED DOCS

- [Full Report](LIVEOPS_AGENT8_TECH_DEBT_REPORT.md) (9,000+ words)
- [Agent 6 Pattern Audit](AGENT6_DESIGN_PATTERNS_AUDIT_REPORT.md)
- [State Management Audit](docs/reports/audits/STATE_MANAGEMENT_AUDIT_REPORT.md)
- [Known Issues](KNOWN_ISSUES.md)

---

**Quick Stats:**
- **Total Debt:** 237+ items
- **P0 Items:** 4 blockers (320h to resolve)
- **Timeline:** Q3 2026 → Q2 2027 (9 months)
- **Debt Budget:** ≤10 TODOs/sprint
- **Next Milestone:** Q3 2026 P0 Elimination
