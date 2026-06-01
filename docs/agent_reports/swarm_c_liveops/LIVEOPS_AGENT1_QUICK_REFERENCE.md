# 🎮 TARTARIA: LIVE STABILITY MONITORING — QUICK REFERENCE

## Debug Overlays

| Key | Overlay | What It Shows |
|-----|---------|--------------|
| **F1** | DebugOverlay | Game state, RS, entity count, player position |
| **F2** | PerformanceMetrics | Real-time FPS, memory, draw calls, batches |
| **F3** | **StabilityHealth** ← NEW | Crashes, hitches, frame drops, stability grade |

---

## Log Files (Auto-Generated)

| File | Purpose | When Created |
|------|---------|--------------|
| `Logs/crash-{timestamp}.txt` | Full crash report with context | Every exception/error |
| `Logs/hitch-{timestamp}.txt` ← NEW | Frame spike details (>100ms) | Every major hitch |
| `Logs/hitch-warning-{timestamp}.txt` ← NEW | Consecutive hitch pattern | 5 hitches in 10s |

---

## Console Logs

| Message | Frequency | Action |
|---------|-----------|--------|
| `[StabilityMonitor] Health Report` | Every 5 minutes | Check metrics |
| `[CrashReporter] Crash #N logged` | Every crash | Review crash log |
| `[CrashReporter] HITCH WARNING` | 5 hitches in 10s | Investigate performance |

---

## API Quick Reference

```csharp
// Check system health
bool stable = StabilityMonitor.Instance.IsStable();
string grade = StabilityMonitor.Instance.GetStabilityGrade(); // "A" to "F"

// Get metrics
float avgFPS = StabilityMonitor.Instance.GetAverageFPS();
int crashes = CrashReporter.GetCrashCount();
int hitches = CrashReporter.GetHitchCount();
int frameDrops = StabilityMonitor.Instance.GetFrameDropCount();

// Add context to crash reports
CrashReporter.AddBreadcrumb("Player entered boss arena");

// Force health report
StabilityMonitor.Instance.ForceHealthReport();
```

---

## Stability Grades

| Grade | Criteria | Status |
|-------|----------|--------|
| **A** | 0 crashes, 60+ FPS, <50 frame drops, <5 hitches | Excellent |
| **B** | 0 crashes, 55+ FPS, <200 frame drops, <20 hitches | Good |
| **C** | 0 crashes, 45+ FPS, <500 frame drops, <50 hitches | Acceptable |
| **D** | 1+ crashes OR 30-45 FPS OR 500+ frame drops | Warning |
| **F** | Multiple crashes OR <30 FPS | Critical |

---

## Production Thresholds

| Metric | Warning | Critical |
|--------|---------|----------|
| Crash Rate | >1 per 100 sessions | >5 per 100 sessions |
| Frame Drops | >10 per hour | >100 per hour |
| Hitches | >5 per hour | >20 per hour |
| Memory | >3.5GB | >4GB |
| Budget Violations | >3 consecutive | >5 consecutive |

---

## Troubleshooting

**Overlay won't appear?**
- Check system is bootstrapped (should see console logs on start)
- Try toggling twice (F3 → F3)
- Check Keyboard.current is not null (Input System active)

**No crash logs?**
- Verify `Logs/` folder exists in project root
- Check file permissions (write access required)
- Look for `[CrashReporter] Failed to write crash log` warnings

**Health reports not appearing?**
- Wait 5 minutes from session start
- Force report: `StabilityMonitor.Instance.ForceHealthReport();`
- Check console isn't filtered to errors-only

---

## Integration Checklist

- [x] CrashReporter auto-bootstraps on launch
- [x] StabilityMonitor auto-bootstraps after scene load
- [x] F3 overlay accessible in all scenes
- [ ] Unity Cloud Diagnostics configured (RECOMMENDED)
- [ ] Grafana/Datadog dashboards (OPTIONAL)

---

## Performance Impact

**Total Overhead:** <0.05ms per frame (0.3% of 16.67ms budget)
- CrashReporter: <0.02ms
- StabilityMonitor: <0.03ms
- StabilityHealthOverlay: 0ms hidden, ~0.2ms visible

**Verdict:** ✅ NEGLIGIBLE — safe for production

---

## External Resources

- Full Audit Report: `LIVEOPS_AGENT1_STABILITY_MONITOR_REPORT.md`
- Implementation Details: `LIVEOPS_AGENT1_IMPLEMENTATION_SUMMARY.md`
- Unity Cloud Diagnostics: https://unity.com/products/cloud-diagnostics
- Sentry Unity SDK: https://docs.sentry.io/platforms/unity/
