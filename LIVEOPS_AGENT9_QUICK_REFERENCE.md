# LIVEOPS AGENT 9: Monitoring Quick Reference

**Agent:** Agent 9 (Monitoring & Alert System Builder)  
**Mission:** Production health monitoring + alerting  
**Status:** ✅ COMPLETE

---

## 🚀 Quick Start

### For Developers

**Check logs in-game:**
- Press `F2` → Performance overlay (FPS, memory, draw calls)
- Press `F3` → Stability overlay (quest health, economy, errors)

**Find log files:**
```
%USERPROFILE%\AppData\LocalLow\<CompanyName>\TARTARIA\Logs\
  - metrics_2026-05-24.json   (telemetry data)
  - health_2026-05-24.json    (health checks)
```

**Analyze logs:**
```powershell
# View all P0 alerts
Get-Content Logs\metrics_*.json | jq '.severity == "critical"'

# Calculate average FPS
Get-Content Logs\metrics_*.json | jq -s 'map(select(.eventType == "performance")) | map(.data.fps) | add/length'

# Top 5 errors
Get-Content Logs\metrics_*.json | jq -s 'map(select(.eventType == "error")) | group_by(.data.message) | map({error: .[0].data.message, count: length}) | sort_by(.count) | reverse | .[0:5]'
```

### For Live Ops Team

**Check game health:**
1. Open latest `health_YYYY-MM-DD.json`
2. Look for `"status": "RED"` or `"status": "YELLOW"`
3. Review subsystem reports for failures

**Critical alerts to watch:**
- `P0_CRASH_SPIKE` → >5 crashes/hour
- `P0_FPS_CRITICAL` → <30 FPS sustained
- `P0_MEMORY_CRITICAL` → >500MB memory
- `P0_SAVE_CORRUPTION` → Data loss risk

**Manual health check:**
```csharp
// In Unity console or debug script
HealthCheckSystem.Instance.ForceHealthCheck();
```

---

## 📊 Key Metrics

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| FPS | >50 avg | <30 = P0, <50 = P1 |
| Memory | <400MB | >500MB = P0, >400MB = P1 |
| Error Rate | <5/min | >30/min = P0, >10/min = P1 |
| Crash Rate | <1/hour | >5/hour = P0 |
| Quest Completion | >70% | <70% = P1 |
| Session Length | >15min median | <10min = P2 |

---

## 🔔 Alert Priority Guide

### P0 (CRITICAL) — Immediate Action
- 🔴 Game unplayable
- 🔴 Data loss risk
- 🔴 Mass crashes
- **Response:** <30 minutes
- **Notify:** Slack + SMS + Email

### P1 (HIGH) — Urgent Investigation
- 🟡 Performance degraded
- 🟡 High error rate
- 🟡 Player churn spike
- **Response:** <1 hour
- **Notify:** Slack + Email

### P2 (MEDIUM) — Monitor & Triage
- 🟢 Short sessions
- 🟢 Economy imbalance
- 🟢 Minor issues
- **Response:** <24 hours
- **Notify:** Slack only

---

## 🛠️ Systems

### MetricsCollector
**Location:** `Scripts/Monitoring/MetricsCollector.cs`  
**Purpose:** Core telemetry collection  
**Output:** `Logs/metrics_YYYY-MM-DD.json`  
**Overhead:** <3% CPU, <5MB RAM  

**Features:**
- Performance snapshots (every 30s)
- Player action tracking
- Error/exception logging
- Alert triggering
- Privacy-compliant (no PII)

**API:**
```csharp
// Record custom player action
MetricsCollector.Instance.RecordPlayerAction("quest_completed", "moon2_quest3");

// Get session stats
var stats = MetricsCollector.Instance.GetSessionStats();
Debug.Log($"Average FPS: {stats["averageFPS"]}");

// Export logs
string logFile = MetricsCollector.Instance.ExportMetrics();
```

### HealthCheckSystem
**Location:** `Scripts/Monitoring/HealthCheckSystem.cs`  
**Purpose:** Periodic health checks + watchdog  
**Output:** `Logs/health_YYYY-MM-DD.json`  
**Overhead:** <1% CPU, <2MB RAM  

**Features:**
- Health checks every 5 minutes
- Subsystem validation (Save, Economy, Quest, Inventory)
- Watchdog timer (freeze detection)
- Auto-recovery actions
- Status codes: GREEN/YELLOW/RED

**API:**
```csharp
// Get current health status
var status = HealthCheckSystem.Instance.GetHealthStatus();
Debug.Log($"Health: {status}"); // GREEN/YELLOW/RED

// Force health check (for testing)
HealthCheckSystem.Instance.ForceHealthCheck();

// Get health stats
var stats = HealthCheckSystem.Instance.GetHealthStats();
Debug.Log($"Total checks: {stats["totalChecks"]}");
```

---

## 📈 Dashboard Setup (Phase 2)

### Grafana + Prometheus (Recommended)

**Step 1: Deploy Prometheus**
```bash
docker run -d -p 9090:9090 -v $(pwd)/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus
```

**Step 2: Configure log shipping**
```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'tartaria'
    file_sd_configs:
      - files:
        - '/path/to/Logs/metrics_*.json'
```

**Step 3: Deploy Grafana**
```bash
docker run -d -p 3000:3000 grafana/grafana
```

**Step 4: Import dashboard**
- Open Grafana (http://localhost:3000)
- Import dashboard JSON from `docs/grafana_dashboard.json` (create this file)

**Cost:** ~$50/month for 50-100 players  
**Setup Time:** 4-6 hours

---

## 🔍 Troubleshooting

### No logs generated
- Check `MetricsCollector.Instance != null` in console
- Verify opt-out preference: `PlayerPrefs.GetInt("MetricsOptOut", 0) == 0`
- Check file write permissions for `Application.persistentDataPath`

### High overhead (>5% CPU)
- Increase `collectionIntervalSeconds` (default: 30s → try 60s)
- Reduce `maxBatchSize` (default: 100 → try 50)
- Disable debug logging in production builds

### Missing health reports
- Check `HealthCheckSystem.Instance != null`
- Verify `checkIntervalSeconds` (default: 300s = 5 minutes)
- Check for exceptions in Unity console

### Alerts not triggering
- Verify thresholds in Inspector (MetricsCollector component)
- Check `enableMetrics = true`
- Review `_lastAlertTime` rate limiting (1 alert/minute per code)

---

## 🎯 Beta Testing Checklist

- [ ] Enable monitoring systems in beta build
- [ ] Verify logs generated locally
- [ ] Test F2/F3 overlays work
- [ ] Simulate low FPS (check P1 alert triggers)
- [ ] Simulate crash (check P0 alert triggers)
- [ ] Review first day logs for anomalies
- [ ] Set up Slack webhook for Phase 2
- [ ] Deploy Grafana dashboard (Phase 2)

---

## 📚 Related Documents

- **Full Report:** `LIVEOPS_AGENT9_MONITORING_REPORT.md`
- **Dashboard Specs:** See report "Dashboard Specifications" section
- **Alert Matrix:** See report "Alert Rules Matrix" section
- **Logging Best Practices:** See report "Logging Best Practices" section

---

## 🔗 Integration Hooks

### Quest System
```csharp
// In QuestManager.cs
void OnQuestCompleted(Quest quest)
{
    MetricsCollector.Instance?.RecordPlayerAction("quest_completed", quest.Id);
}
```

### Player Death
```csharp
// In PlayerHealth.cs
void OnPlayerDeath()
{
    MetricsCollector.Instance?.RecordPlayerAction("player_death", transform.position.ToString());
}
```

### Save System
```csharp
// In SaveManager.cs
void OnSaveCompleted(bool success)
{
    if (success)
    {
        MetricsCollector.Instance?.RecordPlayerAction("save_game", "success");
    }
    else
    {
        MetricsCollector.Instance?.RecordPlayerAction("save_failed", "error");
    }
}
```

---

## 📞 Support

**Agent 9 Deliverables:**
- ✅ MetricsCollector.cs (557 lines)
- ✅ HealthCheckSystem.cs (484 lines)
- ✅ Monitoring report (full spec)
- ✅ Quick reference (this doc)

**Next Agent:** Agent 10 (if applicable) or production deployment team

**Questions?** Review full report or contact @agent-coordinator

---

**Last Updated:** May 24, 2026  
**Version:** 1.0
