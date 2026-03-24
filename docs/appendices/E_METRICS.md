# TARTARIA WORLD OF WONDER — Appendix E: KPI & Analytics Metrics
## Performance Tracking, Retention Analytics & Live-Ops Dashboards

---

> *What gets measured gets restored. Every metric serves the mission: keep players in awe, keep sessions meaningful, keep the grid growing.*

**Cross-References:**
- [08_MONETIZATION.md](../08_MONETIZATION.md) — Revenue model & economy design
- [19_ECONOMY_BALANCE.md](../19_ECONOMY_BALANCE.md) — Resource economics & tuning levers
- [10_ROADMAP.md](../10_ROADMAP.md) — Development phases & milestones
- [09_TECHNICAL_SPEC.md](../09_TECHNICAL_SPEC.md) — Performance budgets & technical targets

---

## Table of Contents

1. [Analytics Philosophy](#analytics-philosophy)
2. [Retention Metrics](#retention-metrics)
3. [Engagement Metrics](#engagement-metrics)
4. [Monetization Metrics](#monetization-metrics)
5. [Economy Health Metrics](#economy-health-metrics)
6. [Progression Metrics](#progression-metrics)
7. [Performance Budgets](#performance-budgets)
8. [Content Metrics](#content-metrics)
9. [A/B Testing Framework](#ab-testing-framework)
10. [Dashboard Specifications](#dashboard-specifications)
11. [Alert Thresholds](#alert-thresholds)

---

## Analytics Philosophy

### Core Principles
1. **Player respect** — Track behavior, not identity. Aggregate insights, not surveillance.
2. **Actionable only** — Every metric must answer a design question or inform a tuning decision.
3. **Signal over noise** — Track 50 meaningful metrics, not 500 vanity metrics.
4. **Real-time when needed** — Economy health and crash rates are live. Retention is daily batch.
5. **Privacy first** — GDPR/CCPA compliant, anonymized IDs, opt-in for detailed telemetry.

### Analytics Stack
- **Client SDK:** Firebase Analytics (event logging, user properties, A/B testing)
- **Backend:** PlayFab (economy tracking, player segments, live-ops automation)
- **Data Warehouse:** BigQuery (raw event storage, complex queries)
- **Dashboards:** Looker / Data Studio (real-time visualizations)
- **Alerting:** PagerDuty / Slack integration (threshold-based alerts)

---

## Retention Metrics

### Primary Retention KPIs

| Metric | Definition | Target | Red Flag |
|---|---|---|---|
| **D1 Retention** | % of new users who return Day 1 | ≥ 45% | < 35% |
| **D7 Retention** | % of new users who return Day 7 | ≥ 25% | < 18% |
| **D14 Retention** | % of new users who return Day 14 | ≥ 18% | < 12% |
| **D30 Retention** | % of new users who return Day 30 | ≥ 12% | < 8% |
| **D90 Retention** | % of new users who return Day 90 | ≥ 6% | < 4% |
| **Rolling 7-Day Retention** | % of active users returning in next 7 days | ≥ 60% | < 45% |

### Retention Cohort Analysis
- Segment by **acquisition source** (organic, paid, featured placement)
- Segment by **Moon reached** (do Moon 3+ users retain better?)
- Segment by **first session duration** (longer first sessions → better D7?)
- Segment by **first purchase** (does D1 purchase correlate with D30?)
- Segment by **companion chosen** (does companion affinity affect retention?)

### Churn Prediction Signals

| Signal | Weight | Action |
|---|---|---|
| Session length declining 3 consecutive days | High | Push notification: "The grid misses you" |
| No quest completion in 48 hours | Medium | Offer catch-up boost / daily reward |
| Aether balance hoarding (not spending) | Medium | Surface new crafting/building goals |
| Skipping companion dialogue | Low | Reduce dialogue frequency for that player |
| Uninstall from settings screen (iOS signal) | Critical | Final retention push (if available) |

---

## Engagement Metrics

### Session Metrics

| Metric | Definition | Target | Red Flag |
|---|---|---|---|
| **DAU** | Daily Active Users | Scaling goal | < 70% of D1 installs by Month 2 |
| **MAU** | Monthly Active Users | Scaling goal | DAU/MAU < 0.15 |
| **DAU/MAU Ratio** | Stickiness | ≥ 0.20 | < 0.15 |
| **Sessions per Day** | Avg sessions per DAU | 2.0 – 3.5 | < 1.5 or > 5.0 |
| **Session Length** | Avg duration per session | 8 – 15 min | < 5 min or > 25 min |
| **Session Interval** | Avg time between sessions | 4 – 8 hours | < 1 hour (grind) or > 24 hours |
| **Total Daily Time** | Sessions × Length | 15 – 35 min | < 10 min |

### Feature Engagement (% of DAU Using Feature)

| Feature | Target | Red Flag | Notes |
|---|---|---|---|
| Excavation | ≥ 80% | < 60% | Core loop — if low, tutorial failed |
| Building/Restoration | ≥ 70% | < 50% | Core loop — check blueprint accessibility |
| Combat | ≥ 50% | < 30% | Session-optional — fine if lower |
| Tuning Mini-games | ≥ 45% | < 25% | Should be rewarding, not tedious |
| Companion Dialogue | ≥ 60% | < 35% | If low, dialogue quality issue |
| Giant-Mode | ≥ 30% | < 15% | Cooldown may be too long |
| Map/Fast-Travel | ≥ 40% | < 20% | Navigation friction signal |
| Social Features | ≥ 15% | < 5% | Optional — no alarm if low |

### Quest Engagement

| Metric | Definition | Target |
|---|---|---|
| **Quest Start Rate** | % of available quests accepted | ≥ 75% for main, ≥ 40% for side |
| **Quest Completion Rate** | % of accepted quests completed | ≥ 85% for main, ≥ 60% for side |
| **Quest Abandon Rate** | % started but neither completed nor failed | < 10% |
| **Daily Task Completion** | % of daily tasks completed per DAU | ≥ 50% |
| **Hidden Quest Discovery** | % of players finding each hidden quest | 5-15% (by design) |
| **Time to Quest Complete** | Avg minutes per quest type | Within design target ±25% |

---

## Monetization Metrics

### Revenue KPIs

| Metric | Definition | Target | Red Flag |
|---|---|---|---|
| **ARPU** | Revenue / MAU | ≥ $0.50 | < $0.25 |
| **ARPPU** | Revenue / Paying Users | ≥ $8.00 | < $4.00 |
| **ARPDAU** | Revenue / DAU | ≥ $0.08 | < $0.04 |
| **Conversion Rate** | % of MAU who make any purchase | ≥ 3.5% | < 2.0% |
| **Whale Concentration** | % of revenue from top 1% spenders | < 40% | > 50% (unhealthy) |
| **First Purchase Timing** | Median days to first purchase | Day 3 – Day 7 | < Day 1 (too aggressive) or > Day 14 (too late) |
| **Monthly Revenue** | Total monthly revenue | Scaling goal | Below cost breakeven |

### Revenue Stream Breakdown (Target Mix)

| Stream | Target % | Tracking Granularity |
|---|---|---|
| Cosmetic IAP | 35% | Per-item, per-category, per-Moon |
| Season Pass | 25% | Per-season, free-vs-premium tier |
| Subscription (Aether Plus) | 20% | Monthly churn, annual conversion |
| Rewarded Ads | 15% | Fill rate, eCPM, views per DAU |
| DLC / Expansion | 5% | Per-pack, attach rate |

### IAP Funnel

| Stage | Target |
|---|---|
| **Store Visit Rate** | ≥ 30% of DAU see the store |
| **Browse Rate** | ≥ 50% of store visitors browse 2+ items |
| **Cart Rate** | ≥ 15% of browsers add an item |
| **Purchase Rate** | ≥ 70% of carts convert |
| **Repeat Purchase** | ≥ 40% of purchasers buy again within 30 days |

### Ad Metrics

| Metric | Target | Red Flag |
|---|---|---|
| **Rewarded Ad Fill Rate** | ≥ 95% | < 85% |
| **eCPM** | ≥ $15 | < $8 |
| **Ads per DAU** | 1.5 – 3.0 | > 4.0 (spam) |
| **Ad Completion Rate** | ≥ 90% | < 80% |
| **Opt-in Rate** | ≥ 60% of DAU watch ≥ 1 ad | < 40% |

---

## Economy Health Metrics

### Resource Flow Monitoring

| Metric | Definition | Target | Red Flag |
|---|---|---|---|
| **Aether Earn Rate** | AE earned per session | 200-500 (Moon 1-4), 400-800 (Moon 5-9), 600-1200 (Moon 10-13) | ±40% from target |
| **Aether Spend Rate** | AE spent per session | 60-80% of earn rate | < 40% (hoarding) or > 100% (deficit) |
| **Material Stockpile** | Avg BM held per player per Moon | 1.5-3× current build cost | > 10× (nothing to spend on) |
| **Skill Point Velocity** | SP earned per Moon | 15-25 | < 10 (stunted progression) |
| **Premium Currency Balance** | Avg RC held by free players | 50-150 | > 500 (nothing worth buying) |

### Inflation/Deflation Indicators

| Indicator | Healthy Range | Action if Outside |
|---|---|---|
| **Avg Session Aether Delta** | +50 to +200 net per session | Adjust earn/sink ratio |
| **Sink Utilization** | ≥ 5 active sinks per Moon used by > 30% of players | Add new sinks or improve discoverability |
| **Crafting Recipe Completion** | ≥ 40% of available recipes attempted | Surface recipes better or reduce cost |
| **Time to Next Milestone** | 2-4 sessions between major milestones | Tighten or loosen reward curve |

---

## Progression Metrics

### Campaign Progression

| Metric | Definition | Target |
|---|---|---|
| **Moon Completion Rate** | % of players who complete each Moon | Moon 1: 80%, Moon 7: 40%, Moon 13: 15% |
| **Moon Drop-Off** | Sharpest % drop between consecutive Moons | No single Moon > 20% drop |
| **Avg Moon Duration** | Real-world days to complete a Moon | 7-10 days |
| **Campaign Completion** | % of all installs who reach Moon 13 | ≥ 10% |
| **100% Completion** | % of completers who get all quests + motes | ≥ 2% |

### Companion Metrics

| Metric | Definition | Target |
|---|---|---|
| **Companion Interaction Rate** | % of sessions with companion dialogue read | ≥ 60% |
| **Trust Progression** | Avg trust level per companion per Moon | Smooth linear growth |
| **Favorite Companion** | Most-interacted companion per player | Even distribution ±10% |
| **Loyalty Chain Completion** | % completing each companion's 5-quest chain | ≥ 30% for most popular |
| **Anastasia Mote Discovery** | % of players finding each mote (1→13) | Mote 1: 25%, Mote 13: 3% |

### Skill & Build Metrics

| Metric | Definition | Target |
|---|---|---|
| **Skill Tree Diversity** | Distribution of players across 4 skill trees | No tree < 15% or > 40% of players |
| **Build Variety** | Unique structure types placed per player | ≥ 10 by Moon 5 |
| **RS Progression** | Global grid RS per Moon | Moon-by-Moon targets in economy doc |
| **Crafting Engagement** | % of players using each crafting station | ≥ 30% per station by unlock Moon +2 |

---

## Performance Budgets

### Frame Rate & Rendering

| Metric | Target | Hard Limit | Measurement |
|---|---|---|---|
| **FPS (Gameplay)** | 60 fps | Never < 30 fps | Metal frame timing |
| **FPS (UI/Menu)** | 60 fps | Never < 45 fps | Metal frame timing |
| **FPS (Spectacle)** | 30 fps | Never < 24 fps | Special event measurement |
| **Frame Time** | ≤ 16.6 ms | ≤ 33.3 ms | Per-frame profiling |
| **Draw Calls** | ≤ 200 per frame | ≤ 400 | GPU debugger |
| **Triangle Count** | ≤ 500K per frame | ≤ 1M | GPU debugger |
| **Shader Variants** | ≤ 50 active | ≤ 100 | Build pipeline count |

### Memory

| Metric | Target | Hard Limit | Notes |
|---|---|---|---|
| **Total RAM** | ≤ 2.5 GB | ≤ 3.5 GB | iPhone 17 Pro has 8 GB |
| **Texture Memory** | ≤ 800 MB | ≤ 1.2 GB | ASTC compressed |
| **Mesh Memory** | ≤ 200 MB | ≤ 400 MB | LOD system active |
| **Audio Memory** | ≤ 100 MB | ≤ 200 MB | Streaming + procedural |
| **Script Heap** | ≤ 256 MB | ≤ 512 MB | ECS minimizes GC |

### Thermal & Battery

| Metric | Target | Red Flag |
|---|---|---|
| **Thermal State** | Nominal for 30-min session | Serious within 15 min |
| **Battery Drain** | ≤ 8% per 15-min session | > 12% per 15 min |
| **CPU Utilization** | ≤ 60% sustained | > 80% sustained |
| **GPU Utilization** | ≤ 70% sustained | > 85% sustained |

### Network & Storage

| Metric | Target | Red Flag |
|---|---|---|
| **App Download Size** | ≤ 250 MB (initial) | > 400 MB |
| **Total Install Size** | ≤ 2 GB (all content) | > 3 GB |
| **Addressable Bundle Size** | ≤ 50 MB per Moon chunk | > 80 MB |
| **API Call Frequency** | ≤ 5 calls per minute (idle) | > 15 calls per minute |
| **Cloud Save Size** | ≤ 5 MB per player | > 10 MB |
| **Crash Rate** | ≤ 0.1% of sessions | > 0.5% |
| **ANR Rate** | ≤ 0.05% of sessions | > 0.2% |
| **Load Time (Cold Start)** | ≤ 4 seconds | > 8 seconds |
| **Load Time (Zone Transition)** | ≤ 2 seconds | > 5 seconds |

---

## Content Metrics

### Zone & Restoration Metrics

| Metric | Definition | Purpose |
|---|---|---|
| **Zone Dwell Time** | Avg minutes spent per zone per session | Identify engaging vs. dead zones |
| **Restoration Completion** | % of structures restored per zone | Track content consumption pace |
| **Discovery Rate** | Avg new discoveries per session | Ensure sufficient "wow" moments |
| **Revisit Rate** | % of sessions returning to a previously completed zone | Measure long-term zone appeal |
| **Favorite Zone** | Most time spent per player | Content design feedback |

### Mini-Game Metrics

| Metric | Definition | Target |
|---|---|---|
| **Attempt Rate** | % of encounters where player plays vs. skips | ≥ 70% |
| **Success Rate** | % of attempts completed successfully | 70-85% (sweet spot) |
| **Perfect Rate** | % of attempts with 100% accuracy | 5-15% |
| **Replay Rate** | % voluntarily replayed after completion | ≥ 10% |
| **Frustration Signal** | 3+ consecutive failures on same mini-game | Auto-difficulty adjustment trigger |

### Narrative Metrics

| Metric | Definition | Purpose |
|---|---|---|
| **Dialogue Read Rate** | % of companion lines fully displayed (not fast-skipped) | Quality signal |
| **Lore Collection** | % of lore entries found per player | Exploration engagement |
| **Choice Distribution** | % of players choosing each branching option | Balance check |
| **Cinematic Completion** | % of spectacles watched without skip | Emotional investment |
| **Replay Narrative** | % of NG+ players choosing different branches | Replayability validation |

---

## A/B Testing Framework

### Testing Infrastructure
- **Provider:** Firebase Remote Config + A/B Testing
- **Minimum Sample:** 5,000 users per variant
- **Test Duration:** 7-14 days minimum
- **Confidence Level:** 95% statistical significance before rolling out

### Candidate Test Variables

| Category | Variable | Variants |
|---|---|---|
| **First Session** | Tutorial length | 3 min / 5 min / 7 min |
| **First Session** | First companion introduction timing | Day 1 / Day 2 / Day 3 |
| **Economy** | Aether earn rate multiplier | 0.8× / 1.0× / 1.2× |
| **Economy** | First IAP timing (store surfaced) | Session 3 / Session 5 / Session 7 |
| **Economy** | Starter pack price | $0.99 / $1.99 / $2.99 |
| **Engagement** | Push notification frequency | 1/day / 2/day / 3/day |
| **Engagement** | Daily task count | 2 / 3 / 4 |
| **Progression** | Quest reward scaling curve | Linear / Front-loaded / Back-loaded |
| **Progression** | Moon 1 duration | 5 days / 7 days / 10 days |
| **Ads** | Rewarded ad placement | After quest / After death / In store only |
| **Cosmetics** | Dome skin pricing | $1.99 / $2.99 / $3.99 |
| **Social** | Grid comparison leaderboard | Visible / Hidden |

### Test Guardrails
- Never test anything that compromises gameplay fairness
- Never test ad frequency above 4 per session
- Never test loot box mechanics (project policy: no loot boxes)
- All test variants must remain playable without spending

---

## Dashboard Specifications

### Executive Dashboard (Real-Time)

| Widget | Data | Refresh |
|---|---|---|
| DAU / MAU Counter | Live count + 7-day trend line | 1 hour |
| Revenue (Today / WTD / MTD) | Total + per-stream split | 1 hour |
| D1 / D7 Retention Trend | 30-day rolling chart | Daily |
| Crash Rate | Session crash % | Real-time |
| Top 5 Errors | Error type + frequency | Real-time |

### Game Health Dashboard (Daily)

| Widget | Data | Refresh |
|---|---|---|
| Moon Funnel | Completion % per Moon, waterfall chart | Daily |
| Quest Completion Rates | Main / Side / Daily / Hidden completion heatmap | Daily |
| Session Length Distribution | Histogram (1-min buckets) | Daily |
| Economy Flow | AE/BM/SP earn vs. spend per Moon, stacked bars | Daily |
| Companion Interaction | Trust progression per companion, line chart | Daily |
| Feature Engagement Radar | Spider chart of 8 core features | Daily |

### Economy Dashboard (Real-Time)

| Widget | Data | Refresh |
|---|---|---|
| Aether Balance Distribution | Player balance histogram | 1 hour |
| Sink/Source Ratio | Aether in vs. out per hour | Real-time |
| Premium Currency Velocity | RC earned (free) vs. purchased vs. spent | Daily |
| Store Conversion Funnel | View → Browse → Cart → Purchase | Real-time |
| Resource Hoarder Detector | % of players hoarding > 5× spend rate | Daily |

### Performance Dashboard (Real-Time)

| Widget | Data | Refresh |
|---|---|---|
| FPS Distribution | P50 / P90 / P99 frame time per device | Real-time |
| Memory Usage | Avg / Peak per scene per device model | 15 min |
| Thermal State | % of sessions hitting "Serious" thermal | 15 min |
| Load Times | Cold start + zone transitions | Real-time |
| Crash Stacktraces | Top 10 crash signatures | Real-time |
| Network Errors | API failure rate per endpoint | Real-time |

---

## Alert Thresholds

### Critical (PagerDuty — Immediate)

| Condition | Threshold | Action |
|---|---|---|
| Crash rate spike | > 1% of sessions in 1 hour | Emergency hotfix investigation |
| Server error rate | > 5% of API calls failing | Backend team page |
| Revenue drop | > 50% below same-hour-last-week | Payment system check |
| D1 retention cliff | < 25% (new cohort) | Onboarding review |

### Warning (Slack — Same Day)

| Condition | Threshold | Action |
|---|---|---|
| Session length drift | < 5 min avg (rolling 24h) | Content/engagement review |
| Economy imbalance | Earn/spend ratio > 2.0 or < 0.5 | Economy tuning lever adjustment |
| Moon drop-off spike | > 25% drop at any single Moon | Content quality review |
| Ad fill rate drop | < 90% | Ad network partner check |
| Thermal warnings | > 10% of sessions | Performance optimization sprint |

### Informational (Daily Digest)

| Condition | Threshold | Purpose |
|---|---|---|
| New Moon reached | Any cohort's median reaches a new Moon | Progression health |
| Feature adoption milestone | Feature crosses 50% DAU | Celebrate + plan iteration |
| Anastasia mote discovery rate | Aggregate tracking | Hidden content reach |
| NG+ start rate | % of completers starting NG+ | Endgame engagement |

---

*The grid has metrics too — every ley line carries a signal, every dome hums a frequency. We measure so we can tune.*
