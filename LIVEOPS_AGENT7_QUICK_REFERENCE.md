# TARTARIA Beta — Feedback System Quick Reference

**Agent 7 Deliverable** — Post-Launch Feedback Integration Framework

---

## 🎮 For Beta Testers

### How to Report Issues

**In-Game (Preferred):**
1. Press **F8** to open feedback reporter
2. Select type: **Bug** / **Balance** / **UX** / **Feature**
3. Enter title (required) and description
4. Click "Submit Feedback" (30s cooldown between submissions)

**Privacy Settings:**
- Screenshots: ON/OFF (Settings → Privacy)
- Device Info: ON/OFF (Settings → Privacy)
- Game Context: ON/OFF (Settings → Privacy)

**Discord:**
- Post in `#beta-feedback` channel
- Use template: `[Bug/Balance/UX/Feature] Title — Description`

---

## 👨‍💻 For Developers

### Integration Checklist

**Hook these events in your systems:**

```csharp
// PlayerHealth.cs → OnDeath()
PlayerSentimentTracker.RecordPlayerDeath(currentQuestID);
BreadcrumbLogger.LogPlayerDeath("Enemy Attack", sceneName);

// QuestManager.cs → OnQuestStart()
BreadcrumbLogger.LogQuestStart(questID, questName);

// QuestManager.cs → OnQuestComplete()
PlayerSentimentTracker.RecordQuestSuccess(questID);
BreadcrumbLogger.LogQuestComplete(questID, questName);

// InventorySystem.cs → OnItemPickup()
BreadcrumbLogger.LogItemPickup(itemName, count);

// CombatSystem.cs → OnHit()
BreadcrumbLogger.LogCombatHit(enemyName, damage, isCritical);

// LevelUpSystem.cs → OnLevelUp()
BreadcrumbLogger.LogLevelUp(newLevel, statPointsGained);
```

---

### Weekly Workflow

**Monday:** Collect feedback reports
```bash
# Navigate to project root
cd c:\dev\TARTARIA_new

# Review feedback files
dir Logs\Feedback\feedback-*.txt
dir Logs\Sentiment\sentiment-*.txt
```

**Tuesday:** Run prioritization
```csharp
// In Unity Editor, Tools → Run Feedback Analysis
FeedbackPrioritizer.AnalyzeFeedback();
FeedbackPrioritizer.GeneratePriorityReport();
// Output: Logs/FeedbackAnalysis/FeedbackPriorityReport-{date}.md
```

**Wednesday:** Triage issues
- Review `FeedbackPriorityReport-{date}.md`
- Assign P0/P1 issues to devs
- Update GitHub/Trello issue tracker

**Thursday-Friday:** Fix & test
- Implement fixes for P0/P1 issues
- Test in dev build
- Prepare patch notes

**Friday EOD:** Deploy patch
- Build & upload beta patch (Steam/itch.io)
- Generate `BetaTesterUpdate-WeekN.md` from template
- Post to Discord + email beta list

---

### Privacy Settings (Settings Menu)

```csharp
// In SettingsManager.cs or similar
FeedbackReporter.AllowScreenshots = playerSettings.privacyAllowScreenshots;
FeedbackReporter.AllowDeviceInfo = playerSettings.privacyAllowDeviceInfo;
FeedbackReporter.AllowGameContext = playerSettings.privacyAllowGameContext;
```

---

## 📊 Dashboard Generation

**Manual (End of Week):**
1. Copy `templates/FeedbackDashboard.md`
2. Fill in placeholders with actual data:
   - `{{REPORTS_THIS_WEEK}}` from file count
   - `{{P0_ISSUES_LIST}}` from FeedbackPriorityReport.md
   - `{{RAGE_QUIT_RATE}}` from sentiment logs
   - etc.
3. Post to Discord + internal wiki

**Automated (Future Enhancement):**
- C# script to auto-generate dashboard from logs
- Scheduled task (Windows Task Scheduler, runs every Friday)

---

## 🐛 Troubleshooting

**"F8 doesn't open feedback UI"**
- Check FeedbackReporter is initialized (should auto-bootstrap)
- Check console for errors: `[FeedbackReporter] Initialized`

**"Feedback reports not saving"**
- Check write permissions on `Logs/Feedback/` directory
- Check disk space (reports are ~10KB each)
- Check console for exceptions

**"Screenshots not capturing"**
- Check `FeedbackReporter.AllowScreenshots == true`
- Check `ScreenCapture.CaptureScreenshot()` works (Unity 2021.3+)
- Check GPU supports screenshot capture

**"Sentiment tracker not detecting rage quits"**
- Check `PlayerSentimentTracker.RecordPlayerDeath()` is called
- Check player actually died within 60s of quit
- Check `OnApplicationQuit()` is firing

---

## 📁 File Locations

**Code:**
- `Assets/_Project/Scripts/Core/FeedbackReporter.cs`
- `Assets/_Project/Scripts/Core/PlayerSentimentTracker.cs`
- `Assets/_Project/Scripts/Core/BreadcrumbLogger.cs`
- `Assets/_Project/Scripts/Core/FeedbackPrioritizer.cs`

**Templates:**
- `templates/FeedbackDashboard.md`
- `templates/BetaTesterUpdate-WeekN.md`
- `templates/BugAcknowledgment.md`

**Logs (Generated):**
- `Logs/Feedback/feedback-{timestamp}.txt`
- `Logs/Feedback/screenshot-{timestamp}.png`
- `Logs/Sentiment/sentiment-{timestamp}.txt`
- `Logs/Sentiment/sentiment-history.txt`
- `Logs/Breadcrumbs/breadcrumbs-{date}.log`
- `Logs/FeedbackAnalysis/FeedbackPriorityReport-{date}.md`

---

## 🔗 Resources

- **Full Report:** `LIVEOPS_AGENT7_FEEDBACK_REPORT.md`
- **Discord:** #beta-feedback, #bug-reports, #dev-updates
- **Issue Tracker:** GitHub Issues / Trello board

---

*Quick Reference v1.0 — Agent 7*
