# TARTARIA Hotfix Pipeline — Quick Reference

**Target SLA:** <4 hours from bug report to production fix  
**Rollback SLA:** <30 minutes

---

## 🚨 Emergency Hotfix Workflow

### 1️⃣ START HOTFIX (15 min)
```powershell
# Create hotfix branch and tracking doc
git checkout main && git pull
git checkout -b hotfix/ISSUE-123-brief-description
.\scripts\hotfix-start.ps1 -IssueNumber 123 -Description "Fix description" -Priority P0
```

**Deliverables:**
- ✅ Hotfix branch created
- ✅ Tracking document: `Logs/Hotfix/ISSUE-123.md`
- ✅ Backup snapshot created

---

### 2️⃣ FIX & TEST (2 hours)

#### Implement Fix (~60 min)
- Make minimal, focused code changes
- Add unit tests for the fix
- Update integration tests if needed

#### Local Testing (~30 min)
```powershell
# Quick sanity check (3 min)
.\scripts\run-automated-tests.ps1 -Mode Smoke

# Critical path validation (12 min)
.\scripts\run-automated-tests.ps1 -Mode CriticalPath

# Full test suite (optional, 60 min)
.\scripts\run-automated-tests.ps1 -Mode Full
```

#### Commit Changes (~10 min)
```powershell
git add .
git commit -m "hotfix(ISSUE-123): Brief description

- Root cause: ...
- Solution: ...
- Tests added: ...
- Regression risk: LOW"

git push origin hotfix/ISSUE-123-description
```

---

### 3️⃣ VALIDATE (30 min)

```powershell
# Run full validation suite
.\scripts\hotfix-validate.ps1 -Branch "hotfix/ISSUE-123-description"
```

**Validation Checks:**
- ✅ Code compiles without errors
- ✅ Smoke tests pass (8/8)
- ✅ Critical path tests pass (18/18)
- ✅ Asset integrity verified
- ✅ Save data compatibility confirmed
- ✅ Generates validation report

**Exit Codes:**
- `0` = PASS (ready for deployment)
- `1` = FAIL (fix errors before deploying)
- `2` = WARN (review warnings before proceeding)

---

### 4️⃣ DEPLOY (30 min)

```powershell
# Deploy to production
.\scripts\hotfix-deploy.ps1 -IssueNumber 123 -Environment Production
```

**Deployment Steps:**
1. ✅ Merge hotfix to main
2. ✅ Tag release (e.g., `v1.0.1-hotfix.123`)
3. ✅ Build production package (~10 min)
4. ✅ Run final smoke test (~3 min)
5. ✅ Upload to distribution
6. ✅ Update version manifest
7. ✅ Push to remote

**Manual Steps Required:**
- Upload build to CDN/distribution server
- Update auto-updater config

---

### 5️⃣ MONITOR (30 min)

```powershell
# Monitor deployment for 30 minutes
.\scripts\hotfix-monitor.ps1 -IssueNumber 123 -Duration 30

# With auto-rollback (if metrics exceed thresholds)
.\scripts\hotfix-monitor.ps1 -IssueNumber 123 -Duration 30 -AutoRollback
```

**Monitored Metrics:**
- 📊 Crash rate (target: <1%)
- 📊 Error rate (target: <2%)
- 📊 Performance (FPS, load times)
- 📊 Player feedback

**Rollback Triggers:**
- Crash rate >3%
- Error rate >5%
- FPS drop >20%
- Memory spike >30%

---

## 🔄 Emergency Rollback

```powershell
# Emergency rollback to previous version
.\scripts\hotfix-rollback.ps1 -ToVersion v1.0.0 -Reason "Critical bug detected"

# Skip confirmation (for automation)
.\scripts\hotfix-rollback.ps1 -ToVersion v1.0.0 -Reason "Description" -Force
```

**Rollback Process (≤30 min):**
1. ✅ Detect issue (~2 min)
2. ✅ Fetch previous build (~5 min)
3. ✅ Deploy previous version (~10 min)
4. ✅ Verify with smoke tests (~10 min)
5. ✅ Document rollback (~3 min)

---

## 🧪 Test Modes

| Mode | Tests | Duration | Use Case |
|------|-------|----------|----------|
| **Smoke** | 8 | ~3 min | Quick sanity check, every commit |
| **CriticalPath** | 18 | ~12 min | Hotfix validation, pre-deploy |
| **Full** | 84 | ~60 min | Complete regression testing |
| **Regression** | All | ~60 min | Compare against baseline |

```powershell
# Smoke tests (ultra-fast)
.\scripts\run-automated-tests.ps1 -Mode Smoke

# Critical path tests (hotfix standard)
.\scripts\run-automated-tests.ps1 -Mode CriticalPath

# Full test suite (pre-release)
.\scripts\run-automated-tests.ps1 -Mode Full

# Regression detection
.\scripts\run-automated-tests.ps1 -Mode Regression
```

---

## 📋 Test Suite Breakdown

### Smoke Tests (8 tests, ~3 min)
1. ✅ Game boots without crash
2. ✅ Main menu/scene loads
3. ✅ Player spawns successfully
4. ✅ Combat system initialized
5. ✅ Save/load systems accessible
6. ✅ Inventory opens
7. ✅ Quest log accessible
8. ✅ Scene transitions work

### Critical Path Tests (18 tests, ~12 min)
- **Core Loop (4 tests):** Movement, interaction, dialogue, scenes
- **Combat (3 tests):** Damage, knockback, enemy AI
- **Save/Load (2 tests):** Data integrity, load previous save
- **Quest (3 tests):** State transitions, objectives, rewards
- **Inventory/Economy (3 tests):** Stacking, equipment, transactions
- **Progression (3 tests):** XP/level up, stats, unlocks

---

## 📊 SLA Targets

| Metric | Target | Critical? |
|--------|--------|-----------|
| **Hotfix Time** | <4 hours | ✅ |
| **Rollback Time** | <30 min | ✅ |
| **Smoke Tests** | <5 min | ✅ |
| **Critical Tests** | <15 min | ✅ |
| **Post-Deploy Monitor** | 30 min | ✅ |

---

## ⚠️ Best Practices

### DO ✅
- Always run smoke tests before committing
- Run critical path tests before pushing
- Document rollback plan before deploying
- Monitor metrics for 30 min post-deploy
- Keep hotfix scope minimal (single issue)
- Test fix in isolation first
- Update test suite if new edge case found

### DON'T ❌
- Skip smoke tests (even for "quick fixes")
- Deploy without validation report
- Bundle multiple fixes in one hotfix
- Deploy on Friday afternoon (rollback risk)
- Ignore performance regression warnings
- Skip rollback documentation
- Deploy without peer review (for P0 issues)

---

## 🎯 Priority Levels

### P0 (Critical) — <4 hour SLA
- Game crash on startup
- Save data corruption
- Critical gameplay blocker
- Security vulnerability

### P1 (High) — <8 hour SLA
- Major gameplay bug (non-blocking)
- Performance regression
- Visual/audio glitch affecting experience
- UI navigation issue

---

## 📞 Escalation

### Automatic Rollback Triggers:
- Crash rate >3% within 30 min
- Error rate >5%
- Frame rate drop >20%
- Memory spike >30%
- Save corruption reports

### Manual Rollback Decision:
- Lead + QA Lead approval required for P0
- On-Call Engineer can trigger for emergencies

---

## 📁 File Locations

```
TARTARIA_new/
├── scripts/
│   ├── hotfix-start.ps1        # Initialize hotfix workflow
│   ├── hotfix-validate.ps1     # Pre-deployment validation
│   ├── hotfix-deploy.ps1       # Automated deployment
│   ├── hotfix-rollback.ps1     # Emergency rollback
│   ├── hotfix-monitor.ps1      # Post-deployment monitoring
│   └── run-automated-tests.ps1 # Test execution (with modes)
│
├── Assets/_Project/Scripts/
│   └── Tests/
│       ├── CriticalPathTestSuite.cs  # 18 critical tests
│       ├── SmokeTestSuite.cs         # 8 smoke tests
│       └── Testing/
│           ├── HotfixValidator.cs        # Validation system
│           └── HotfixRollbackManager.cs  # Rollback manager
│
└── Logs/Hotfix/
    ├── ISSUE-XXX.md                  # Hotfix tracking doc
    ├── validation-XXX.md             # Validation report
    ├── deployment-XXX.log            # Deployment log
    ├── rollback-XXX.log              # Rollback log
    └── INCIDENT-rollback-XXX.md      # Rollback incident report
```

---

## 🔗 Related Documentation

- **Full Report:** `LIVEOPS_AGENT3_HOTFIX_PIPELINE_REPORT.md`
- **Test Files:** `Assets/_Project/Scripts/Tests/`
- **Scripts:** `scripts/hotfix-*.ps1`
- **Logs:** `Logs/Hotfix/`

---

## 💡 Quick Tips

1. **Always validate before deploy:**
   ```powershell
   .\scripts\hotfix-validate.ps1 -Branch "hotfix/ISSUE-123-..."
   ```

2. **Monitor after deployment:**
   ```powershell
   .\scripts\hotfix-monitor.ps1 -IssueNumber 123 -Duration 30
   ```

3. **Emergency rollback:**
   ```powershell
   .\scripts\hotfix-rollback.ps1 -ToVersion v1.0.0 -Reason "Critical bug"
   ```

4. **Test modes for speed:**
   - Smoke: Pre-commit check (3 min)
   - CriticalPath: Pre-deploy check (12 min)
   - Full: Pre-release check (60 min)

---

**Last Updated:** 2026-05-24  
**Agent:** Agent 3 (Hotfix & Regression Tester)  
**Version:** 1.0
