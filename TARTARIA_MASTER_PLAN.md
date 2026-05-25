# TARTARIA — Master Plan

**Owner:** NATRIX
**Date:** 2026-05-24
**Status:** Draft v0.1 — for review and revision

This document is the single anchor for what TARTARIA is becoming. It supersedes the scatter of session reports, agent audits, and one-off strategy notes living in the repo root. If something in those older docs conflicts with this plan, this plan wins until updated.

---

## 1. The Two Tracks

TARTARIA is one project running on two parallel tracks that reinforce each other but should not be confused with each other.

**Track A — Ship the Game.** Finite. The artifact. Moon 1 (Echohaven) is in playable beta. The work is finishing it, polishing it, and getting it in players' hands. Without this artifact, there is nothing for a community to gather around. The game is sacred until shipped — no architectural rewrites in flight, no "while we're at it" refactors that risk the build.

**Track B — Build the Platform.** Infinite. The community. A modular, open-source contribution system around the game so anyone — builders, lore authors, level designers, sound designers, engineers — can add to the world and have it appear in-game. This is the long game and the educational/community angle.

The relationship: Track A produces the *thing* the community rallies around. Track B turns players into contributors. Track A ships in months. Track B grows over years. Both run in parallel, but Track A's stability is never sacrificed for Track B's ambition.

---

## 2. Where Things Stand

**The game:** v1.0.0-beta build exists. Echohaven Vertical Slice — 3-5 hours of gameplay, tuning mechanics, combat, save/load, haptics, 60 FPS target. Moons 2-13 are stub/prototype. 54 GDD documents drafted, 184 quests catalogued, full lore bible and dialogue trees in place. The design work is *complete*; the implementation work is partial.

**The repo:** Substantial. Dozens of AGENT audit reports, multiple build logs, real Unity assemblies (Core, AI, Audio, Gameplay, Save, Quest, UI, etc.). Pre-production is over; mid-production is well underway.

**The platform layer:** Drafted on paper. Empty `Core/`, `Extensions/`, `Experimental/`, `Modules/` folders and a `templates/module_template/` exist as scaffolding from an earlier session. They are inert — they don't load anything yet — and should be treated as drafts until the platform work is scheduled. The runtime module loader is not built.

**The community:** Does not exist yet. No central hub, no Discord, no public landing page, no contributors beyond the project owner.

**The honest read:** Track A is closer to "ship" than the AGENT report sprawl suggests. Track B is closer to "blank page" than the draft folders suggest. Both gaps are normal for a project at this stage.

---

## 3. Track A — Ship Moon 1

The deliverable is a downloadable Moon 1 build that a stranger can install, play for 3-5 hours, and tell a friend about. Nothing more, nothing less, for the first ship.

**Phase A1 — Stabilize (Weeks 1-4)**
- Audit the open issues from existing AUDIT/AGENT reports and produce a single close-out punch list
- Lock the build branch — no new features, only bug fixes and polish
- Clean up the repo root: move all AGENT reports into `docs/agent_reports/` so the front of the repo isn't intimidating

**Phase A2 — Polish (Weeks 4-8)**
- Run the Moon 1 vertical slice end-to-end and fix every break
- Tune the harmonic puzzles, combat encounters, and progression pacing
- Final pass on UI/UX, audio mix, haptic feedback
- Performance budget: hold 60 FPS on recommended spec (RTX 3060 / 16 GB)

**Phase A3 — Soft Launch (Weeks 8-12)**
- itch.io page goes live (no gatekeeping, free distribution)
- Closed beta to a small list (10-30 testers)
- Iterate on feedback, fix the breakers, ship a v1.0.1 patch within two weeks of beta

**Phase A4 — Public Beta (Weeks 12+)**
- Open itch.io page to the public, free or pay-what-you-want
- Begin Steam Coming Soon page setup
- If timing aligns, target a Steam Next Fest window (these run ~quarterly)

**Mandate:** Track A is one path. Resist the urge to ship Moon 2 in the same launch. Moon 1 alone is the milestone.

---

## 4. Track B — The Platform

The deliverable is a working contribution loop: a stranger can clone the repo, drop a folder into `/Modules`, launch the game, and see their content in the world.

**Phase B1 — Decide and Document (Weeks 1-4, can run alongside A1)**
- Decide what to do with the draft platform folders (see Section 11 Open Decisions)
- Write the contributor docs *for real* — handbook, module guide, engine guide — once the architecture is committed
- The TRUE MODEL document already exists in `tartaria_system_summary.txt`; the platform docs should be derivatives of that, not parallel inventions

**Phase B2 — Minimal Module Loader (Weeks 4-10)**
- Implement a `ModuleRegistry` in Core that scans `/Modules` at game start
- Parse `module.json` (name, type, moon, version)
- Load `scene.unity` (or prefab) and inject into the world at the moon-specified location
- Hot reload in editor for fast contributor iteration
- One working example module shipped in-repo: `Modules/example_ruined_tower/` — a complete, simple, copy-paste reference

**Phase B3 — Open Contribution (Weeks 10-16)**
- Public GitHub repo (already public if not) with clear CONTRIBUTING.md, code of conduct, PR template
- Module submission flow: contributor PRs into `Modules/`, maintainer review, merge to `experimental` branch, eventual promotion to `verified`
- First public call for contributors goes out *only after* the example module works and the docs are real

**Phase B4 — Quality Tiers (Months 4-6+)**
- `Experimental` vs `Verified` distinction for modules (review-gated)
- Simple thumbs-up/thumbs-down rating system
- Featured Modules section on the hub
- Optional blockchain authorship tracking — explicitly deferred, not a launch dependency

**Mandate:** Do not open the contribution floodgates before the module loader works and an example module exists. A platform that promises modding but doesn't actually load mods burns trust the first time someone tries it.

---

## 5. The Central Hub

The hub is the front door. One URL a stranger can land on to understand what TARTARIA is, play the game, or contribute.

**Spine:** GitHub repo + GitHub org (you already have the repo).
**Storefront:** GitHub Pages site at `tartaria.github.io` or a custom domain (e.g., `tartaria.world`). Built from a single repo with a simple static site generator or hand-written HTML. Hosts the game pitch, screenshots, links, contributor handbook, and roadmap.
**Game distribution:** itch.io page first (immediate), Steam page later. Both link back to the hub.
**Community room:** Discord. Channels for #welcome, #devlog, #module-makers, #lore-discussion, #playtesting, #help.
**Tie-it-together:** A Linktree or single-page directory linking every channel (site, itch, Steam, Discord, GitHub, YouTube, social).

**Mandate:** All public channels point back to the hub. The hub points out to channels. The web of links should always lead a curious stranger to the same place.

---

## 6. The Content Engine (Autonomous)

A real autonomous content pipeline. The reasoning model picks what's worth saying, the media stack creates the assets, and the editorial seed keeps the voice consistent. You set the guardrails once; the loop runs itself.

**The pipeline:**

1. **Signal layer (daily scan).** A scheduled agent reads this week's repo signals: new commits, tagged builds, screenshots dropped in `/Builds/screenshots/`, AGENT report deltas, module submissions. It scores moments for post-worthiness — a finished cathedral restoration scores high, a typo fix scores zero.

2. **Reasoning layer (idea generation).** The agent takes the top-scored moments and proposes post angles: "this week's harmonic puzzle redesign deserves a 60-second short showing the 3-6-9 sequence solving itself; the Anastasia dialogue branch can become a lore drop thread; the new Echohaven lighting pass is a screenshot worth a standalone post."

3. **Media layer (asset creation).** Gameplay footage captured automatically from build sessions (FFmpeg pipeline). Narration generated via TTS with a fixed character voice — Princess Anastasia narrating her own restoration is on-brand and disarmingly effective. Supporting imagery via image-gen (Flux, DALL-E, Midjourney API) when needed. Composition (gameplay + overlay + narration) handled by an automated editing step.

4. **Editorial layer (voice consistency).** Every draft passes through a brand-tone agent loaded with the TARTARIA editorial seed — vocabulary, register, what's in-fiction, what's marketing, what's off-limits. Outputs that drift get rewritten or rejected.

5. **Distribution layer (scheduling).** Posts go out on a per-channel cadence: short-form video on TikTok/Shorts/Reels, screenshots on Twitter/Bluesky, devlogs on Reddit/YouTube/itch, threaded lore drops on socials, weekly digest on the hub. Each platform gets content shaped for it, not the same content copy-pasted.

6. **Feedback layer (metrics + learning).** Engagement signals feed back into the scoring layer so the pipeline learns which angles land with which audiences and adjusts.

**The editorial seed (one-time setup):**
- Voice register: mythic, restorative, slightly conspiratorial in a knowing-not-paranoid way
- In-fiction characters get character voices (Anastasia, Wendell, the player as "Conductor")
- Out-of-fiction posts (devlogs, build notes) get a separate "developer voice" — direct, builder's energy, no marketing fluff
- Vocabulary rules: "restoration" not "rebuilding," "Aether" not "energy," "tuning" not "playing"
- Hard no's: nothing political-on-current-events, no engagement bait, no clickbait thumbnails

**What stays human (light touch):**
- Replying to comments (still tone-sensitive enough to need a real person, even if drafted)
- The occasional surprise — a real voice memo from you, a real photo from your desk
- The editorial-seed refresh every 8-12 weeks as the project evolves

**The honest caveats:**
- TikTok and Instagram detectors penalize content flagged as fully synthetic — the pipeline favors real gameplay footage with AI narration over end-to-end generated video
- Brand-new accounts get throttled if they post heavily their first week — accounts need a 2-3 week warm-up phase with lower cadence
- The editorial seed needs occasional refresh or the voice drifts toward generic-AI tone over months

**The deliverable:** A pipeline you can leave running and check in on weekly. Not a chore. A machine that sounds like TARTARIA.

---

## 7. Social Distribution — Find the Right Crowd

**Core thesis:** TARTARIA does not need to convert mainstream gamers into believers. It needs to *find the audiences who already believe* and put itself in front of them. The game is built — visually, sonically, thematically — to speak the native language of several tribes that are large, hungry, and underserved. Marketing is reach, not persuasion.

**The audiences TARTARIA already speaks to:**

- **Alternative history / lost civilization** — millions on YouTube, TikTok, Telegram. Already obsessed with the exact ideas Tartaria is built around. Will recognize their own vocabulary in one frame.
- **Sacred geometry, 432 Hz, cymatics, vibration** — devoted niches with their own influencers. Tartaria's mechanics literally *are* their interests made playable.
- **Mud flood, star fort, hidden architecture researchers** — a real subculture with active forums and channels. The game is canonically about excavating their thesis.
- **Princess Anastasia and Romanov mystique** — perennial fascination, especially Russian-speaking and history-buff audiences worldwide.
- **Solarpunk / utopian futurism / free energy curiosity** — large, idealistic, share-prone audiences who want a hopeful aesthetic.
- **Urbex, abandoned places, architectural restoration** — visual-first audiences who'll engage with anything beautiful and decaying.
- **Indie game devs and weird-game appreciators** — recognize when a game has actual identity instead of asset-flip energy.

**One short of Anastasia narrating her own restoration over real gameplay footage will travel further in these tribes than a year of paid ads at mainstream gamers.**

**Channel-by-channel:**

| Channel | Role | Content Type | Cadence |
|---------|------|--------------|---------|
| **TikTok / Shorts / Reels** | Discovery surface — where the niche audiences scroll | 30-60s gameplay clips with character narration; harmonic puzzles solving themselves; before/after restoration time-lapses | Daily once warmed up |
| **YouTube long-form** | Anchor content; ranks for years | 8-15 min devlogs, lore deep-dives, "why we built Tartaria this way" essays | Weekly to bi-weekly |
| **Twitter/X + Bluesky** | Indie dev community + alt-history Twitter | Stills, GIFs, lore threads, build status, in-character Anastasia posts | Daily, mixed automated + reactive |
| **Reddit** | High-trust deep dives | Devlogs on r/Unity3D, r/IndieDev, r/IndieGaming; thematic posts on r/AlternateHistory, r/SacredGeometry, r/StarForts (when relevant), r/Solarpunk | 2-3x weekly, substantive only |
| **Telegram** | Underrated for alt-history audiences | Channel for lore drops, build updates; let the niche find it organically | Weekly |
| **Hacker News** | One shot for the open-source platform angle | A single "Show HN" when the module system genuinely works | Once, when ready |
| **itch.io community** | Native to the indie game audience | Devlogs, player feedback, build releases | Per release |
| **Discord** | Home base — where the actual community lives | Channels for devlog, module-makers, lore-discussion, playtesting | Always-on |

**The hub** ties it all together. Every channel points back; the hub points out to every channel. A curious stranger who lands anywhere should be able to find the rest within one click.

**Mandate:** No verbatim cross-posting. The content engine drafts per-channel variants from the same source material. A short for TikTok and a Reddit post for r/AlternateHistory share a gameplay clip but speak different languages.

---

## 8. Roadmap

A realistic phasing. All weeks are calendar weeks from project start (Week 1 = 2026-05-24).

| Week | Track A | Track B | Hub / Community |
|------|---------|---------|-----------------|
| 1-4  | Stabilize, punch list, repo cleanup | Decide platform scope, finalize docs | GitHub Pages skeleton, Discord scaffold |
| 4-8  | Polish vertical slice | Begin module loader implementation | Hub goes live, first devlog post |
| 8-12 | Soft launch (closed beta) | Module loader working in editor | Content engine wired, weekly cadence starts |
| 12-16 | Public beta on itch.io | Example module shipped, open contributor call | First external contributors invited |
| 16-24 | Steam Coming Soon page, Next Fest prep | First verified community modules | 100+ Discord members, sustained cadence |
| 24-36 | Moon 2 work begins | Quality tier system live | Educational partnerships explored |
| 36-52 | Moon 2 ships, Moon 3 prototyping | 10+ verified community modules | First Day Out of Time event (Moon 13 thematic) |

This is aggressive but achievable for a focused solo+AI-assisted operation. Adjust weeks freely; respect the *order*.

---

## 9. Mandates (Non-Negotiable)

1. **Core does not get modified without review.** Even by the project owner. The protected foundation is what makes the platform credible.
2. **The beta build is sacred until shipped.** No architectural rewrites, no "while we're at it" refactors, no integrating Track B work into Track A's path while Track A is mid-launch.
3. **Voice consistency.** Every public-facing piece of content stays in the Tartarian/Aether thematic register. The fiction is the marketing.
4. **Find the right crowd; don't convert the wrong one.** Marketing energy goes toward audiences who already speak Tartaria's language (alt-history, sacred geometry, mud-flood, solarpunk, urbex, indie weird-game). Do not waste cycles trying to make mainstream gamers care. The game sells itself to its native tribes.
5. **No blockchain commitments at launch.** It's a future possibility, not a foundation. Mentioning it before there's a reason to attracts the wrong audience.
6. **No deletions of existing work without explicit owner approval.** Every audit report, lore doc, and old README is preserved or migrated, never overwritten.

---

## 10. Goals (Measurable)

**Track A:**
- Closed beta build to 10+ testers within 12 weeks
- Public beta on itch.io within 16 weeks
- 100+ itch.io downloads within 6 months of public beta
- Steam Coming Soon page live within 20 weeks

**Track B:**
- Module loader functional in editor within 10 weeks
- Example module shipped within 12 weeks
- 3 external contributor modules within 6 months of platform launch
- 10 verified community modules within 12 months

**Hub / Community:**
- GitHub Pages site live within 6 weeks
- Discord scaffold within 4 weeks; 100 members within 6 months
- Weekly devlog cadence sustained for 26 consecutive weeks
- One "Show HN" post when platform is genuinely ready

These are targets, not promises. Miss them honestly; do not fudge them.

---

## 11. Open Decisions

Things only the owner can answer. Listed here so they're not lost.

1. **Draft platform folders (`Core/`, `Extensions/`, `Experimental/`, `Modules/`, `templates/module_template/`, `docs/{CONTRIBUTING,MODULE_GUIDE,ENGINE_GUIDE,PHILOSOPHY}.md`).** Currently inert scaffolding from an earlier session. Options: (a) keep as drafts and let Track B Phase B2 build into them, (b) move to `docs/_platform_draft/` to clearly mark as not-yet-real, (c) delete entirely and rebuild fresh when Track B engineering begins. Recommendation: (a) — they're harmless and labeling them in the master plan disarms confusion.
2. **Hub domain.** `tartaria.github.io` (free, instant) vs a custom domain like `tartaria.world` or `tartariagame.com` (paid, professional). Recommendation: start with GitHub Pages subdomain, migrate to custom domain when Track A ships.
3. **Game pricing.** itch.io free / pay-what-you-want / fixed price. Steam pricing tier. Defer until closer to public beta but flag now.
4. **Educational angle scope.** Is this "the game happens to teach things" or "this is a serious educational tool that's also a game"? The two paths diverge in how you market it, who you partner with, and what content lives in the modules.
5. **Solo operation or recruiting.** At what point do you bring in human contributors (engineers, artists, community moderators) beyond AI agents? Affects Track B Phase B3 timing.

---

## 12. What Happens Next

If this plan is approved as-is or with edits:

- I can produce a Track A punch list pulling from the existing AGENT reports to give Phase A1 a concrete starting set.
- I can wire up scheduled tasks for the content engine drafts once we agree on cadence and target channels.
- I can scaffold the GitHub Pages landing page in this repo.
- I can write the public-facing CONTRIBUTING.md and contributor handbook (real ones, derived from `tartaria_contributor_handbook.txt`).
- I can stand up the Discord channel structure document so it's ready when you create the server.

None of those start until this plan reflects what you actually want. Mark up, push back, redirect.

---

*The Aether is patient. The plan is not.*
