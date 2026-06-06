# SPRINT_7_DISPATCH.md

> The Sprint 7 dispatch plan, ready to fire after Sprint 6 PRs are merged.
> 2026-06-02 - Director: Cowork. Worker pool: 10 parallel general-purpose agents.

---

## Theme: PR LANDING + REAL CONTENT FILL-IN

Sprint 6 shipped 10 lanes of polish but most are on un-merged branches. Sprint 7's first half is "land what we shipped"; second half is "fill the content gaps Lane 7 surfaced + finish the Moon 1 ship-gate."

---

## Director pre-work (BEFORE dispatching any agent)

Per `docs/agents/WORKTREE_MANDATE.md`, create all 10 worktrees up front:

```powershell
cd C:\dev\TARTARIA_new
git fetch origin

# Lane prep - branch from feature/consolidate-moon-architecture
$pairs = @(
  @('s7_l1_merge_polish',       'agent/integration/sprint6-merge'),
  @('s7_l2_lane7_apply_sites',  'agent/gameplay/difficulty-apply-sites'),
  @('s7_l3_audio_mixer_fix',    'agent/audio/mixer-controller-rename'),
  @('s7_l4_save_thumbs',        'agent/save/thumbnail-pipeline'),
  @('s7_l5_pause_settings',     'agent/ui/pause-settings-extract'),
  @('s7_l6_tutorial_runner',    'agent/integration/yarn-tutorial-binding'),
  @('s7_l7_hit_call_sites',     'agent/combat/hit-feedback-call-sites'),
  @('s7_l8_post_resto_assets',  'agent/level/post-restoration-asset-wiring'),
  @('s7_l9_itch_smoke',         'agent/tools/itch-build-smoke'),
  @('s7_l10_acceptance_pass',   'agent/qa/moon1-acceptance-audit')
)
foreach ($p in $pairs) {
  git worktree add "C:\dev\_wt_$($p[0])" -b $p[1] feature/consolidate-moon-architecture
}
```

Verify with `git worktree list` - should show 10 new entries each at `6094136c`.

---

## Per-lane dispatch prompts

Each lane prompt opens with the worktree mandate. The Director fires all 10 in a SINGLE message (parallel mandate).

### Lane 1 - Sprint 6 Merge Polish

```
You are Sprint 7 Lane 1. Worktree: C:\dev\_wt_s7_l1_merge_polish. Branch: agent/integration/sprint6-merge.

Read CLAUDE.md, docs/agents/API_CONTRACT.md (v2), docs/agents/WORKTREE_MANDATE.md, docs/agents/COORDINATION.md.

Your job: merge Sprint 6 lanes into feature/consolidate-moon-architecture in the documented order:
1. Settings (agent/ui/settings-menu-real) + Save UI (agent/ui/save-slot-ui) + Ambient (agent/audio/world-ambient-zones)
2. Main Menu (agent/ui/main-menu-scene) + Credits (agent/narrative/credits-scene)
3. Tutorial (agent/ai/milo-tutorial-flow) + Hit Feedback (agent/anim/combat-hit-feedback) + Difficulty (agent/gameplay/difficulty-modes)
4. Post-Restoration (agent/level/post-restoration-world-state) + itch Assets (agent/tools/itch-page-assets)

After each merge, run: tundra.log.json check for compile errors. If errors, fix in the merge branch with a follow-up commit (NEVER modify the source lane branch). Push the merge result.

Report: each merge SHA + tundra.log.json error count + any conflicts you resolved.
```

### Lane 2 - Difficulty Apply-Sites Restore

```
You are Sprint 7 Lane 2. Worktree: C:\dev\_wt_s7_l2_lane7_apply_sites. Branch: agent/gameplay/difficulty-apply-sites.

Per docs/HANDOFFS.md (Lane 7 entry), re-apply the 4 difficulty multipliers at their real sites. The 3 ScriptableObjects + DifficultyController + DifficultyProfile already landed via PR `agent/gameplay/difficulty-modes` (00eb8bc6). You need to wire them at:

- MudGolemAI.cs: damage output multiplied by DifficultyController.EnemyDamageMultiplier (grep for melee/damage emit)
- MudLordBoss.cs: hp init multiplied by DifficultyController.EnemyHpMultiplier (Awake)
- TuningMiniGame.cs: tolerance expanded by DifficultyController.MiniGameForgiveness (StartTuning override)
- AetherVisionOverlay.cs: stamina drain divided by DifficultyController.AetherStaminaMultiplier (Update)

Grep each file FIRST. Quote the exact line you edit. No silent fallbacks.

Report: 4 file:line citations of your edits + DifficultyController.cs:line where you read the multiplier + branch SHA.
```

### Lane 3 - AudioMixerController Rename

```
You are Sprint 7 Lane 3. Worktree: C:\dev\_wt_s7_l3_audio_mixer_fix. Branch: agent/audio/mixer-controller-rename.

Per API_CONTRACT v2 section 4.1: AudioMixerController.cs has wrong defaults (MasterVolume/MusicVolume/SFXVolume). The mixer asset uses MasterVol/MusicVol/SFXVol/UIVol/AmbienceVol/VoiceVol.

Edit Assets/_Project/Scripts/Audio/AudioMixerController.cs - rename the 3 wrong defaults and add the 3 missing ones (UI, Ambience, Voice). Update any tests that reference the wrong names.

Report: file:line of each rename + branch SHA.
```

### Lane 4 - Save Thumbnail Pipeline

```
You are Sprint 7 Lane 4. Worktree: C:\dev\_wt_s7_l4_save_thumbs. Branch: agent/save/thumbnail-pipeline.

Sprint 6 Lane 3 added per-slot screenshot capture but it polls OnBeforeSave. Strengthen it:
- On scene unload (SceneManager.activeSceneChanged), persist a final thumbnail.
- Add a Tartaria/Save/Capture Current Thumbnail Editor menu for manual capture.
- Compress PNGs >256KB via System.Drawing or built-in EncodeToPNG quality knob.

No invented SaveManager events. Use OnBeforeSave + activeSceneChanged only.

Report: files + branch SHA.
```

### Lane 5 - PauseMenu Settings Extract

```
You are Sprint 7 Lane 5. Worktree: C:\dev\_wt_s7_l5_pause_settings. Branch: agent/ui/pause-settings-extract.

PauseMenu.cs has a no-op settings stub. SettingsOverlay.cs has the real settings (added in Sprint 6 Lane 2 as IMGUI). Extract settings into a real Canvas prefab so:
- Main Menu (Sprint 6 Lane 1) can call it from the Settings button
- PauseMenu can call it from in-game pause

Reuse SettingsMenu/SettingsPersistence from Sprint 6 Lane 2. Build the prefab via an Editor menu Tartaria/UI/Build Settings Panel Prefab. No silent fallback if SettingsMenu.cs is missing - log error.

Report: prefab path + files + branch SHA.
```

### Lane 6 - Yarn Tutorial Runner Binding

```
You are Sprint 7 Lane 6. Worktree: C:\dev\_wt_s7_l6_tutorial_runner. Branch: agent/integration/yarn-tutorial-binding.

Sprint 6 Lane 6 shipped milo_tutorial.yarn + MiloTutorialFlow.cs in Tartaria.AI but couldn't directly play yarn (asmdef circular). Add a YarnTutorialBinding.cs in Tartaria.Integration that:
- Subscribes to GameEvents.RaiseHUDShowDialogue (verify line in GameEvents.cs - per API_CONTRACT it's line 617)
- Looks up the current Yarn node based on the speaker arg
- Calls DialogueRunner.StartDialogue(nodeName)

Report: GameEvents.cs:line for the event + branch SHA.
```

### Lane 7 - HitFeedback Call-Sites

```
You are Sprint 7 Lane 7. Worktree: C:\dev\_wt_s7_l7_hit_call_sites. Branch: agent/combat/hit-feedback-call-sites.

Sprint 6 Lane 5 shipped HitFeedback.NotifyHit() static method as the enemy-side hook. Wire actual call sites:
- MudGolemAI.cs: on melee swing land
- MudLordBoss.cs: on each phase damage tick
- ResetScout.cs: on patrol attack
- AnyOtherEnemyAI: search for Health.TakeDamage call sites and prepend HitFeedback.NotifyHit at each

No invented signatures - call exactly HitFeedback.NotifyHit(Vector3 pos, float dmg, bool isCrit).

Report: file:line of each call site you added + branch SHA.
```

### Lane 8 - Post-Restoration Asset Wiring

```
You are Sprint 7 Lane 8. Worktree: C:\dev\_wt_s7_l8_post_resto_assets. Branch: agent/level/post-restoration-asset-wiring.

Sprint 6 Lane 9 shipped Moon1PostRestorationVisuals.cs that looks for child objects "FountainWater", "FountainAudio", "StarProjection" via transform.Find. Author these as real prefab variants/children via an Editor menu Tartaria/Level/Wire Post-Restoration Children that:
- Adds a ParticleSystem child named FountainWater to Building_fountain (URP particles)
- Adds an AudioSource child named FountainAudio with a fountain.ogg AudioClip (use Resources/Audio/Ambient if exists)
- Adds a Skybox/Particles child named StarProjection to Building_dome
- Skybox swap material to a starry skybox if one exists in Assets

No silent fallbacks - if any audio/material asset is missing, log warn with expected resource path + skip THAT subtask, not the whole wiring.

Report: 3+ file:line citations of asset paths + branch SHA.
```

### Lane 9 - itch Build Smoke Test

```
You are Sprint 7 Lane 9. Worktree: C:\dev\_wt_s7_l9_itch_smoke. Branch: agent/tools/itch-build-smoke.

Sprint 6 Lane 8 shipped capture-itch-screenshots.ps1 + Moon1ItchScreenshotCapture.cs. Run the capture pipeline end-to-end:
1. Build Moon1ItchBuild.cs (Editor menu) Win64 -> Builds/itch_assets/TARTARIA_Moon1.zip
2. Run the screenshot capture menu -> Builds/itch_assets/shot_01..08.png
3. Validate every PNG is <2MB, between 1280x720 and 1920x1080
4. Write a build_report.txt with: SHA, build size, screenshot count, dimensions

If build fails, capture the tundra.log.json error and the BuildPlayer error to a docs/build_failures/2026-06-02-itch-smoke.md.

Report: build SHA + screenshot count + branch SHA.
```

### Lane 10 - Moon 1 Acceptance Audit

```
You are Sprint 7 Lane 10. Worktree: C:\dev\_wt_s7_l10_acceptance_pass. Branch: agent/qa/moon1-acceptance-audit.

Read docs/15_MVP_BUILD_SPEC.md cover to cover. Produce docs/audits/MOON1_ACCEPTANCE_2026-06-02.md that scores each spec item:
- ✓ Shipped + grep evidence file:line
- ⚠ Partial + what's missing
- ❌ Not started

Spec sections to check: Hero Buildings, Village Buildings, POIs, Vegetation, Mini-Game Variants, NPCs (Milo / Anastasia / Lirael / Cassian), Combat (Mud Golem, Reset Scout, Mud Lord), Lore Beats (Brazier ritual, Lullaby, 17th Hour, Rose Window, Pipe Organ, Spire Placement), Audio (Cymatic Engine, Ambient Zones, Adaptive Music), Save/Load, Difficulty Modes, Tutorial Flow.

For each ⚠ or ❌, propose the smallest follow-up that ships it.

Report: total ✓/⚠/❌ counts + branch SHA + audit file path.
```

---

## Director post-work (AFTER all 10 report)

```powershell
# Cleanup
foreach ($p in $pairs) {
  git worktree remove "C:\dev\_wt_$($p[0])" --force
}

# Push CLAUDE.md update referencing Sprint 7 outcome
# Update STATUS.md with new ship state
```

---

## Success criteria

- All 10 branches pushed to origin
- Lane 1 reports clean tundra.log.json after each merge
- Lane 10 audit pinpoints the remaining ship-gate items
- Zero ` .git/config` corruption events (worktree mandate working)

---

*Born from Sprint 6 forensics. Enforced from Sprint 7.*
