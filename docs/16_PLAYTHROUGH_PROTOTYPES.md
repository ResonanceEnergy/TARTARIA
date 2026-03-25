# TARTARIA WORLD OF WONDER — Playthrough Prototypes
## Guided Test Scenarios for Design Validation & QA

---

> *Every prototype is a question disguised as a play session. If the tester says "one more minute" after the timer ends, the answer is yes.*

**Cross-References:**
- [00_MASTER_GDD.md](00_MASTER_GDD.md) — Core pillars & gameplay loop
- [15_MVP_BUILD_SPEC.md](15_MVP_BUILD_SPEC.md) — Phase 1 vertical slice specification
- [07_MOBILE_UX.md](07_MOBILE_UX.md) — Session pacing & touch controls
- [10_ROADMAP.md](10_ROADMAP.md) — Gate criteria per phase
- [14_HAPTIC_FEEDBACK.md](14_HAPTIC_FEEDBACK.md) — Haptic validation targets

---

## Table of Contents

1. [Prototype Design Philosophy](#1-prototype-design-philosophy)
2. [Prototype 1 — First Touch (Core Loop)](#2-prototype-1-first-touch-core-loop)
3. [Prototype 2 — Building Bliss (Construction)](#3-prototype-2-building-bliss-construction)
4. [Prototype 3 — Harmonic Combat](#4-prototype-3-harmonic-combat)
5. [Prototype 4 — Companion Chemistry](#5-prototype-4-companion-chemistry)
6. [Prototype 5 — Anastasia Integration](#6-prototype-5-anastasia-integration)
7. [Prototype 6 — Giant Mode & Scale](#7-prototype-6-giant-mode-scale)
8. [Prototype 7 — Full Moon Cycle (Moon 1)](#8-prototype-7-full-moon-cycle-moon-1)
9. [Prototype 8 — Economy Stress Test](#9-prototype-8-economy-stress-test)
10. [Prototype 9 — Onboarding Flow](#10-prototype-9-onboarding-flow)
11. [Prototype 10 — Vertical Slice (Gate 1)](#11-prototype-10-vertical-slice-gate-1)
12. [Demo Priority & Trailer Beats](#12-demo-priority-trailer-beats)
13. [Technical Requirements](#13-technical-requirements)
14. [Tester Feedback Template](#14-tester-feedback-template)

---

## 1. Prototype Design Philosophy

**Each prototype answers ONE core question:**

| Prototype | Core Question |
|---|---|
| 1 — First Touch | Does the core loop feel magical in 10 minutes? |
| 2 — Building Bliss | Does sacred-geometry construction feel satisfying? |
| 3 — Harmonic Combat | Does frequency-matching combat feel fresh? |
| 4 — Companion Chemistry | Do NPCs make the player smile, care, and return? |
| 5 — Anastasia Integration | Does the silence-first companion enhance or distract? |
| 6 — Giant Mode | Does scale-switching create genuine awe? |
| 7 — Full Moon Cycle | Does a complete 28-day arc hold attention? |
| 8 — Economy Stress | Can player progress without paying and want to pay anyway? |
| 9 — Onboarding | Do new players understand without a tutorial? |
| 10 — Vertical Slice | Does the combined experience pass Gate 1? |

**Prototype Rules:**
- Every prototype must run on target hardware (iPhone 17 Pro) at 60 FPS
- Every prototype has a hard time limit — if the experience can't land within that window, it's too slow
- Every prototype ends with a tester questionnaire (see Section 14)
- No placeholder art — even prototypes use final-quality visuals for the systems being tested
- Audio must be present — silence kills wonder

---

## 2. Prototype 1 — First Touch (Core Loop)

**Question:** Does the core loop feel magical in 10 minutes?

**Duration:** 10 minutes  
**Gate:** Phase 1, Month 3, Week 1  
**Build Requirements:** Echohaven zone, Aether system, 1 buried dome, mud excavation, basic tuning

### Setup
- Player starts at Echohaven entrance — misty dawn, ambient hum, no UI overlay
- One dome visible as a slight mound in the landscape — mud-covered, barely recognizable
- Aether particles drift lazily — visual invitation to explore

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–1:00 | Player explores freely, discovers the mound | Player moves toward it without prompting |
| 1:00–2:00 | Resonance scan reveals dome outline beneath mud | "Oh!" moment — visible surprise |
| 2:00–4:00 | Swipe-dig excavation — progressive reveal of ornate stonework | Player speeds up swiping (engagement) |
| 4:00–5:00 | Discovery moment — first glimpse of intact architecture | Slow-mo + choir sting lands emotionally |
| 5:00–7:00 | Tuning mini-game — frequency matching on 3 nodes | Player leans forward (concentration) |
| 7:00–8:00 | Dome restoration cinematic — golden light erupts, bells ring | Goosebumps / audible reaction |
| 8:00–9:00 | Aether harvest — golden energy flows visibly through ley lines | Player follows the energy trails |
| 9:00–10:00 | HUD shows global grid progress — 1% → 2% | Player says "I want to do another one" |

### Fail Conditions
- Player doesn't find the mound within 90 seconds → visual guidance too subtle
- Excavation feels tedious before stone appears → mud layers too deep
- Tuning mini-game takes >3 attempts → difficulty too high for first encounter
- No audible/visible player reaction at dome restoration → spectacle insufficient
- Player puts phone down before minute 8 → pacing broken

### Haptic Validation
- Excavation: progressive vibration intensifying near architecture
- Tuning: harmonic pulse on successful node alignment
- Restoration: deep satisfying "thrum" as dome activates
- Aether flow: gentle rhythmic pulse following energy along ley lines

---

## 3. Prototype 2 — Building Bliss (Construction)

**Question:** Does sacred-geometry construction feel satisfying?

**Duration:** 15 minutes  
**Gate:** Phase 1, Month 2, Week 2  
**Build Requirements:** Building placement system, golden-ratio snap, 3 building templates, RS scoring

### Setup
- Pre-excavated zone with cleared foundations
- 3 building templates available: Small Dome, Spire, Star Fort segment
- Materials pre-stocked (no gathering required for this prototype)

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–2:00 | Place first Small Dome — golden guides appear | Player instinctively follows the spiral |
| 2:00–4:00 | Dome snaps to golden-ratio grid — RS counter ticks up | Satisfaction at the "click" |
| 4:00–6:00 | Place Spire — height proportions enforce φ relationship | Player experiments with positioning |
| 6:00–9:00 | Attempt Star Fort segment — more complex geometry | Player takes time, cares about precision |
| 9:00–12:00 | Free building — player tries combinations | Unprompted experimentation |
| 12:00–14:00 | RS feedback shows which placements earn highest scores | Player adjusts to improve scores |
| 14:00–15:00 | Zone overview — see the small district they've built | Photo-mode impulse / screenshot |

### Fail Conditions
- Golden guides confuse rather than assist → visual language unclear
- Snap system fights player intention → alignment algorithm too rigid
- RS scoring feels arbitrary → feedback loop broken
- Player stops caring about precision after minute 5 → not enough reward for accuracy

### Key Metrics
- Average placement time per building (target: 60–90 seconds for small, 120–180 for complex)
- RS score distribution (target: 60% of testers score "Good" or better on first attempt)
- Unprompted repositioning rate (target: >50% of testers adjust at least one building for higher RS)

---

## 4. Prototype 3 — Harmonic Combat

**Question:** Does frequency-matching combat feel fresh and Tartarian?

**Duration:** 12 minutes  
**Gate:** Phase 1, Month 3, Week 3  
**Build Requirements:** Combat system, 1 enemy type (Mud Golem), 3 resonance abilities, haptic feedback

### Setup
- Pre-restored zone with active ley lines
- RS at 50% (enemy spawn threshold reached)
- 3 Mud Golems emerge from corruption pockets

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–1:00 | First Golem approaches — threat HUD activates | Player recognizes danger without alarm |
| 1:00–3:00 | Tutorial: Resonance Pulse (tap combat) — hit timing window | Player lands 3+ hits in first encounter |
| 3:00–5:00 | Second Golem — Harmonic Shield introduced (hold to block) | Player blocks at least once |
| 5:00–7:00 | Two Golems simultaneously — combo system revealed | Player discovers combo multiplier |
| 7:00–9:00 | Larger Golem variant — requires pattern-draw ability | Player attempts pattern at least twice |
| 9:00–11:00 | Combat victory — Aether shards scatter, collectible | Player collects shards eagerly |
| 11:00–12:00 | Zone calms — restoration continues, RS rises | Player feels combat served the restoration |

### Fail Conditions
- Combat feels like Generic Action Game™ → not enough harmonic flavor
- Timing windows too tight on mobile → frustration instead of flow
- Player ignores combat to continue building → combat not engaging enough
- Haptic feedback indistinguishable between hit types → sensory clarity failure
- Player dies and doesn't understand why → feedback insufficient

### Harmonic Combat Uniqueness Test
After the session, ask: "How is this combat different from other mobile games?"  
**Pass:** Tester mentions music, frequency, harmony, resonance, or tuning  
**Fail:** Tester says "tapping" or "swiping" with no reference to the harmonic layer

---

## 5. Prototype 4 — Companion Chemistry

**Question:** Do NPCs make the player smile, care, and return?

**Duration:** 20 minutes  
**Gate:** Phase 1, Month 3, Week 2  
**Build Requirements:** Milo + Lirael companion AI, 30+ contextual dialogue lines, idle chatter system

### Setup
- Full Echohaven exploration with both companions active
- Scripted trigger points for key dialogue moments
- Ambient NPC activity (background Echo NPCs walking, humming)

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–2:00 | Milo greets player at zone entrance | Player slows down to read dialogue |
| 2:00–5:00 | Exploration with Milo commentary (3 trigger points) | Player smiles at Milo's humor |
| 5:00–7:00 | Lirael appears at first dome — ethereal entrance | Player pauses to watch her manifest |
| 7:00–10:00 | Dual companion commentary during excavation | Player waits for both to speak |
| 10:00–13:00 | Lirael asks a profound question during tuning | Player stops gameplay to read |
| 13:00–16:00 | Milo/Lirael banter exchange (scripted crossfire) | Player laughs or shows emotion |
| 16:00–18:00 | Idle chatter triggers (player stands still for 15s) | Player deliberately waits for more lines |
| 18:00–20:00 | Companion reacts to combat differently | Player notices personality differences |

### Critical Dialogue Moments

**Milo — First Dome Discovery:**
> "Genuine Tartarian dome! Only slightly buried. That's a premium feature where I come from."

**Lirael — During Tuning:**
> "Can you feel it? The stone remembers being whole. It's dreaming of its dome."

**Milo + Lirael Exchange — Post-Restoration:**
> Milo: "Right, so we just… un-buried a cathedral. Is that a Tuesday here?"  
> Lirael: "You're smiling, Milo. You always smile when you pretend not to care."  
> Milo: "I'm not— that's just my face. My face does things."

### Fail Conditions
- Player skips dialogue after minute 5 → writing not compelling enough
- No visible emotional reaction to Lirael → ethereal quality not landing
- Player doesn't notice companion personality differences → too similar
- Idle chatter repeats within 20 minutes → not enough variety
- Companions feel like tooltips, not characters → personality insufficient

---

## 6. Prototype 5 — Anastasia Integration

**Question:** Does the silence-first Archive Echo companion enhance or distract from the experience?

**Duration:** 25 minutes  
**Gate:** Phase 2, Month 4  
**Build Requirements:** Princess Anastasia companion system (4 modes), golden particle effects, 20+ dialogue lines, haptic sync

### Setup
- Post-Moon 1 Echohaven — dome already restored
- Anastasia manifests for the first time after the player's restoration triggers an Archive Echo event
- Golden mote Easter egg available in the zone (hidden behind restored dome, accessible only via Aether scan overlay at the 17th Hour)

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–2:00 | Golden particles coalesce near restored dome | Player investigates without prompting |
| 2:00–4:00 | Anastasia manifests in Silent Mode — translucent, gold (#FFD700) | Player pauses to observe |
| 4:00–7:00 | Exploration — Anastasia follows but says nothing | Player glances at her periodically |
| 7:00–8:00 | Player approaches a lore fragment — Anastasia shifts to Reactive Whisper | Player notices the mode change |
| 8:00–12:00 | 3 soft whispers during exploration (non-intrusive) | Player does NOT feel interrupted |
| 12:00–15:00 | Player enters combat — Anastasia goes Invisible | Player doesn't notice absence (good) |
| 15:00–17:00 | Post-combat — Anastasia re-manifests, comments on the fight quietly | Player reads her comment |
| 17:00–20:00 | Building mode — Anastasia in Conversational Mode near architecture | Player engages with building dialogue |
| 20:00–23:00 | Nightfall — Anastasia's glow intensifies, she hums softly | Player stays in zone longer |
| 23:00–25:00 | Golden mote Easter egg discovery (if player finds it) | Delight moment |

### Mode Transition Validation

| Transition | Trigger | Expected Player Response |
|---|---|---|
| Silent → Reactive Whisper | Lore proximity | Subtle awareness |
| Reactive Whisper → Conversational | Extended building session | Natural conversation feeling |
| Any → Invisible | Combat engagement | No disruption to combat flow |
| Invisible → Silent | Combat ends | Gentle re-awareness |

### Fail Conditions
- Player forgets Anastasia exists during Silent Mode → presence too subtle
- Player feels annoyed during Reactive Whisper → whispers too frequent
- Conversational Mode competes with Milo/Lirael → NPC traffic jam
- Anastasia's golden glow is visually distracting during gameplay → VFX too bright
- Player asks "what does she do?" → unclear companion value proposition

### Key Metrics
- Time-to-first-interaction: how long before player deliberately approaches Anastasia (target: <5 min)
- Whisper-skip rate: % of Reactive Whisper lines player skips (target: <20%)
- Mode-awareness: can player describe her behavior modes after session? (target: 3/4 modes recognized)
- Net Promoter: "Would you want this companion in the full game?" (target: >80% yes)

---

## 7. Prototype 6 — Giant Mode & Scale

**Question:** Does scale-switching create genuine awe?

**Duration:** 15 minutes  
**Gate:** Phase 2, Month 4, Week 4  
**Build Requirements:** Giant mode toggle, scale physics, megalith interaction, camera system

### Setup
- Star Fort zone with mixed-scale architecture
- Giant skeleton key already discovered (prerequisite)
- 3 megaliths available for placement

### Scripted Flow

| Minute | Event | Success Metric |
|---|---|---|
| 0:00–2:00 | Normal exploration — buildings at standard scale | Baseline comfort established |
| 2:00–3:00 | Giant mode activation — dramatic scale shift | Audible "whoa" from tester |
| 3:00–5:00 | Look around as giant — buildings now small, new details visible | Player explores rooftop inscriptions |
| 5:00–8:00 | Lift and place a megalith block — physics feedback | Weight and momentum feel real |
| 8:00–10:00 | Precision rock cutting at giant scale — large stone | Satisfying precision despite size |
| 10:00–12:00 | Toggle back to human — appreciate what was built | Scale contrast creates appreciation |
| 12:00–14:00 | Rapid toggle puzzle — some parts need one scale, others need the other | Player groks dual-scale gameplay |
| 14:00–15:00 | Giant-mode combat — Mud Titan encounter | Power fantasy lands |

### Fail Conditions
- Scale transition causes motion sickness → camera interpolation too fast
- Giant mode feels like "zoom out" instead of "I'm huge" → camera/physics wrong
- Megaliths feel weightless → no sense of mass
- Player avoids giant mode → uncomfortable or unnecessary
- Rooftop inscriptions invisible even in giant mode → detail level insufficient

---

## 8. Prototype 7 — Full Moon Cycle (Moon 1)

**Question:** Does a complete 28-day arc hold attention across multiple sessions?

**Duration:** 4 hours (split across 8× 30-minute sessions over 4 days)  
**Gate:** Phase 2, Month 5, Week 4  
**Build Requirements:** Complete Moon 1 content — all mechanics, companions, combat, climax

### Session Schedule

| Session | Days | Content | Goal |
|---|---|---|---|
| 1 | Days 1–3 | First exploration, Milo intro, first scan | Hook |
| 2 | Days 4–7 | Dome discovery, excavation, Lirael intro | Discovery |
| 3 | Days 8–11 | Tuning, first restoration, RS mechanics | Mastery |
| 4 | Days 12–15 | Building construction, golden-ratio learning | Creating |
| 5 | Days 16–19 | Combat encounters, enemy spawns, skill use | Defending |
| 6 | Days 20–23 | Companion development, dialogue deepening | Caring |
| 7 | Days 24–27 | Escalation, boss preparation, tension build | Anticipation |
| 8 | Day 28 | The Dome Awakening climax sequence | Payoff |

### Retention Metrics

| Metric | Target | Fail Threshold |
|---|---|---|
| Session 2 return rate | >90% | <75% |
| Session 4 return rate | >80% | <65% |
| Session 8 return rate | >75% | <60% |
| Average session length | >25 min | <15 min |
| "Want to continue?" (post-climax) | >85% yes | <70% yes |

### Fail Conditions
- Player doesn't return for Session 3 → Week 1 pacing too slow
- Session lengths decline each day → engagement degrading
- Player skips dialogue by Session 5 → NPC fatigue
- Climax doesn't feel earned → arc buildup insufficient
- Player satisfied at end (doesn't want Moon 2) → wrong kind of conclusion

---

## 9. Prototype 8 — Economy Stress Test

**Question:** Can player progress without paying and want to pay anyway?

**Duration:** 6 hours (simulated across Moon 1–3 progression)  
**Gate:** Phase 3, Month 7  
**Build Requirements:** Full economy system, IAP framework, Battle Pass, cosmetic shop

### Test Scenarios

| Scenario | Constraint | Expected Outcome |
|---|---|---|
| **Free Player (Whale Watch)** | Zero spend, normal play | Completes Moon 1–3 in ~12 hours, sees cosmetics they want |
| **Low Spender ($5)** | One Battle Pass purchase | +15% cosmetic variety, no gameplay advantage, feels rewarded |
| **Dolphin ($20)** | Battle Pass + cosmetic bundle | Rich visual customization, still same gameplay progression |
| **Time-Limited** | 15 min/day only | Meaningful progress each session, no paywall gates |
| **Binge Player** | 3+ hours continuous | No artificial stopgates, content paces naturally |

### Economy Balance Checks

| Resource | Earn Rate (Free) | Spend Rate | Reserve Target |
|---|---|---|---|
| Aether | 100/session (15 min) | 80/building | Always 1+ session ahead |
| Resonance Crystals (premium) | 5/day (login) + 10/Moon climax | 50/cosmetic item | 1 cosmetic per Moon (free) |
| Building Materials | 3–5/excavation | 5–10/building | Never bottlenecked for >1 session |
| Skill Crystals | 1–3/level (tiered) | 1/node | Master 2 trees + invest in 2 others (free) |

### Fail Conditions
- Free player hits a wall where progress requires spending → pay-to-win detected
- Paid cosmetics affect gameplay performance → fairness violation
- Premium currency earn rate makes shop feel pointless → economy too generous
- Player feels "punished" for not spending → negative monetization sentiment
- No cosmetic item creates "I want that" impulse → shop not desirable enough

---

## 10. Prototype 9 — Onboarding Flow

**Question:** Do new players understand without a tutorial?

**Duration:** 30 minutes (first-time experience)  
**Gate:** Phase 3, Month 7, Week 3  
**Build Requirements:** Complete FTUE (First-Time User Experience), no tutorial overlays

### Design Principle
**Teach through wonder, not text.** The player should learn every mechanic by discovering it naturally. If a tooltip is needed, the design failed.

### Onboarding Sequence

| Minute | Mechanic Learned | Teaching Method | Validation |
|---|---|---|---|
| 0:00–0:30 | Movement | Open space invites exploration | Player moves without instruction |
| 0:30–1:30 | Camera control | Beautiful vista visible with rotation | Player rotates to see the view |
| 1:30–3:00 | Resonance scan | Milo says "Feel that? Hold your hand out…" | Player tap-holds, discovers pulse |
| 3:00–5:00 | Excavation (swipe) | Visible architecture edge peeking through mud | Player swipes instinctively |
| 5:00–7:00 | Tuning (touch-wheel) | Lirael hums the target frequency as hint | Player matches by ear/feel |
| 7:00–9:00 | Building (snap placement) | Golden guide lines appear at correct position | Player follows guides |
| 9:00–11:00 | Aether harvest (proximity) | Glowing energy drifts visibly toward player | Player walks near energy |
| 11:00–13:00 | Combat (tap-to-attack) | Enemy approaches slowly, telegraph obvious | Player taps to engage |
| 13:00–15:00 | RS (resonance score) | Score visibly ticks up after each action | Player connects actions to score |

### Zero-Tutorial Metrics

| Metric | Target | Fail Threshold |
|---|---|---|
| Time to first excavation | <3 min (without tutorial) | >5 min |
| Tuning success rate (first attempt) | >60% | <40% |
| Building placement (first attempt) | >70% achieve "Good" RS | <50% |
| Combat survival (first encounter) | >85% | <70% |
| Total mechanic comprehension (post-test) | >80% understand 5/6 mechanics | <60% |

### Fail Conditions
- Player stands still for >30 seconds → no visual invitation to move
- Player scans the wrong thing repeatedly → scan feedback unclear
- Player doesn't understand tuning after 3 attempts → mini-game too abstract
- Player doesn't notice RS changing → feedback too subtle
- Player asks "what am I supposed to do?" → critical design failure

---

## 11. Prototype 10 — Vertical Slice (Gate 1)

**Question:** Does the combined experience pass Gate 1?

**Duration:** 15 minutes (continuous, uninterrupted)  
**Gate:** Phase 1, Month 3, Week 4 — **THE gate**  
**Build Requirements:** Everything — full Echohaven, core loop, Milo, tuning, building, combat, Aether, haptics

### Gate 1 Criteria (from [10_ROADMAP.md](10_ROADMAP.md))

A single continuous 15-minute play session that includes:
- [ ] Exploration of Echohaven zone
- [ ] Resonance scanning of buried structure
- [ ] Mud excavation with progressive reveal
- [ ] Tuning mini-game (frequency matching, 3 nodes)
- [ ] Building restoration (cinematic payoff)
- [ ] Aether harvesting (visible energy flow)
- [ ] At least one combat encounter (Mud Golem)
- [ ] Milo companion with contextual dialogue
- [ ] Runs at 60 FPS on iPhone 17 Pro
- [ ] Haptic feedback on all core interactions

### Combined Flow

```
0:00  ─── ARRIVE ──────── Mist, hum, wonder
1:00  ─── EXPLORE ─────── Walk, discover, Milo comments
2:30  ─── SCAN ────────── Tap-hold reveals buried dome
3:30  ─── EXCAVATE ────── Swipe-dig, architecture appears
5:00  ─── DISCOVER ────── Slow-mo reveal, choir sting
5:30  ─── TUNE ────────── 3 nodes, frequency matching
7:30  ─── RESTORE ─────── Dome erupts in golden light
8:00  ─── HARVEST ─────── Aether flows through ley lines
9:00  ─── BUILD ───────── Place a complementary spire
10:30 ─── COMBAT ──────── Mud Golem attacks, resonance fight
12:00 ─── VICTORY ─────── Aether shards scatter, Milo quips
12:30 ─── EXPAND ──────── RS rises, global map updates
13:30 ─── BREATHE ─────── Quiet moment, companion dialogue
14:30 ─── TEASE ───────── Distant structure glows on horizon
15:00 ─── END ──────────── "I want to keep playing"
```

### The Ultimate Test
Play it. Put it in front of someone who has never heard of Tartaria.  
Watch their face.  
If they say "Can I play more?" — **Gate 1 passes.**

---

## 12. Demo Priority & Trailer Beats

### Investor Demo (5 min)
| Beat | Duration | Shows |
|---|---|---|
| Mist-to-dome reveal | 45s | Excavation → discovery moment |
| Golden-ratio building | 60s | Sacred geometry snap, RS feedback |
| Tuning mini-game | 45s | Frequency matching, dome activation |
| Combat encounter | 45s | Harmonic combat uniqueness |
| Companion dialogue | 45s | Milo humor + Lirael wonder |
| Global grid update | 30s | Planetary progress visualization |

### App Store Trailer (30s)
| Second | Shot | Audio |
|---|---|---|
| 0–5 | Mud-covered ruin → swipe reveals dome | Ambient → rising tone |
| 5–10 | Golden spiral snaps into place, RS ticks up | Click + harmonic chime |
| 10–15 | Tuning wheel → dome erupts in golden light | Bass thrum → choir burst |
| 15–20 | Giant mode activation → megalith lift | Scale shift whoosh |
| 20–25 | Ley lines connecting across world map | Electric golden threads |
| 25–30 | Title + tagline: "Tune the World." | Full 432 Hz chord |

### Press Kit Prototype
- 3 × 10-second GIF captures of key moments (excavation, tuning, restoration)
- 6 screenshots at 6.7" iPhone resolution (exploration, building, combat, map, companion, night)
- 1 × 30-second trailer (above spec)
- Playable build link (TestFlight internal)

---

## 13. Technical Requirements

### All Prototypes Must Meet

| Requirement | Target | Measurement |
|---|---|---|
| Frame rate | 60 FPS sustained | Xcode Instruments GPU profiler |
| RAM | <2.8 GB peak | Memory Graph Debugger |
| Thermal | No throttling in 15 min | Device temperature logged |
| Battery | <8% drain per 15 min session | Battery Health API |
| Touch latency | <16ms input-to-response | UIEvent timestamp delta |
| Haptic latency | <10ms trigger-to-feedback | Core Haptics timing log |
| Load time | <5s cold start, <2s warm | Stopwatch / os_signpost |
| Crash rate | 0% during prototype session | Xcode crash log |

### Device Matrix

| Device | Priority | Notes |
|---|---|---|
| iPhone 17 Pro Max | Primary | Full target spec |
| iPhone 17 Pro | Primary | Slightly less RAM, smaller screen |
| iPhone 16 Pro | Secondary | Backward compatibility validation |
| iPhone 15 Pro | Stretch | Minimum viable device |
| iPad Pro M4 | Tertiary | Tablet layout verification |

---

## 14. Tester Feedback Template

### Post-Session Questionnaire

```
PROTOTYPE: [Name]
TESTER:    [Anonymous ID]
DATE:      [YYYY-MM-DD]
DEVICE:    [Model]
SESSION:   [Duration in minutes]

1. WONDER SCORE (1–10):
   "How magical did the experience feel?"
   [ ] 1  2  3  4  5  6  7  8  9  10

2. FRUSTRATION MOMENTS:
   "At what point (if any) did you feel stuck or annoyed?"
   _______________________________________________

3. FAVORITE MOMENT:
   "What was the single best moment?"
   _______________________________________________

4. COMPANION PREFERENCE:
   "Which companion did you prefer and why?"
   _______________________________________________

5. RETURN INTENT:
   "Would you play this again tomorrow?"
   [ ] Definitely  [ ] Probably  [ ] Maybe  [ ] No

6. RECOMMENDATION:
   "Would you recommend this to a friend?"
   [ ] Definitely  [ ] Probably  [ ] Maybe  [ ] No

7. SPEND INTENT:
   "Would you consider spending $5 on this game?"
   [ ] Yes  [ ] Maybe  [ ] No

8. ONE WORD:
   "Describe the experience in one word."
   _______________________________________________

9. OPEN FEEDBACK:
   _______________________________________________
   _______________________________________________
```

---

**Document Status:** FINAL  
**Cross-References:** `00_MASTER_GDD.md`, `15_MVP_BUILD_SPEC.md`, `10_ROADMAP.md`, `07_MOBILE_UX.md`  
**Last Updated:** March 25, 2026
