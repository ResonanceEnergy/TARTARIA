# Moon 1 finish + Moon 2 starter — LOCAL_TASKS manifest

Drop date: 2026-05-30 (updated 19:55 — total now **17 tickets**)
Total tickets: **17** (5 Moon-1 logic + 6 art-wireup + 6 batch-2: Moon-1 finish + Moon-2 starter)
Estimated local-LLM runtime: ~3-8 min per ticket on qwen2.5-coder:7b CPU (longer on cathedral/level-builder due to context size)

## Batch 2 added 2026-05-30 19:55 (after first batch was streaming)

| # | Ticket | Output | Kills / adds |
|---|---|---|---|
| 12 | `12_moon1-levelbuilder-village.md` | `Moon1LevelBuilder.cs` (REPLACE) | Kills 12 primitives. Wires 9 village structures from Cathedral kit + KayKit RPGToolsBits props. |
| 13 | `13_ai-color-urp-helper.md` | `AIMaterialHelper.cs` (NEW) | Shared URP-safe helper. After this lands, all 25+ enemy `.color = ` sites can be 1-line swapped to `AIMaterialHelper.SetColor(...)`. |
| 14 | `14_excavation-site-props.md` | `Moon1ExcavationSites.cs` (REPLACE) | Kills 2 primitives. Wires 4 themed dig sites (Workshop / Architect / Tool cache / Ceremonial) from KayKit RPGToolsBits FBX. |
| 15 | `15_tartarian-architecture-enhancer.md` | `TartarianArchitectureEnhancer.cs` (REPLACE) | Kills 10 primitives. Replaces capital/frieze/finial/rose ring primitives with Cathedral kit refs. |
| 16 | `16_moon2-dissonance-crystal.md` | `DissonanceCrystal.cs` (NEW) | **First Moon 2 content.** Per docs/03: violet/magenta/blue hazard crystals at 666/777/888 Hz that drain Aether on proximity. |
| 17 | `17_moon2-crystalline-cavern-scene-builder.md` | `Editor/Moon2BuildOutCavern.cs` (NEW) | Editor menu `Tartaria/Moon 2/Build Out Crystalline Cavern` — generates the Moon 2 cavern area west of Echohaven with 7 DissonanceCrystals + 12 stalactites + entry portal. |

## Run order recommendation

Run them all at once — they're independent. But if running one at a time, prioritize by impact:

1. **#06 Cathedral kit wireup** — kills the biggest scandal (12 primitives, 18 unused prefabs)
2. **#09 MudGolemAI prefab refactor** — combat looks proper
3. **#10 ResetScout Adventurer model** — second-most-visible enemy
4. **#11 Skeleton kit giant key** — adds the giant-bone-in-mud visual to a major narrative beat
5. **#07 KayKit Forest prefab wrappers** — Editor menu, run once to generate 210 vegetation prefabs
6. **#08 Hovl VFX bindings** — unlocks all 76 magic effects for ceremony / combat
7. **#01–#05** — logic tickets (completion tracker, tutorial hints, golem loot, magenta audit, inn rest trigger)

## Art-wiring tickets added after asset audit (`docs/agent_reports/sessions/ASSET_AUDIT_2026-05-30.md`)

| # | Ticket | Output | Why critical |
|---|---|---|---|
| 06 | `06_cathedral-kit-wireup.md` | `Moon1HeroBuildingSpawner.cs` (REPLACE) | Kills 12 primitive stubs, wires the 18 Cathedral kit prefabs that already exist but are NEVER REFERENCED. **The biggest art-vs-code scandal in the audit.** |
| 07 | `07_kaykit-forest-prefab-wrappers.md` | `Editor/KayKit_GenerateForestPrefabs.cs` (NEW) | Editor menu that wraps all 210 KayKit Forest Nature FBX models as prefabs with URP materials. Idempotent. |
| 08 | `08_hovl-vfx-restoration.md` | `Integration/HovlVFXBindings.cs` (NEW) | Curated slot mapping for 12 Hovl Magic VFX prefabs (restoration_burst, restoration_pillar, resonance_blue/green/orange, crystal_idle/attack, etc) callable from any system. Replaces hand-rolled ParticleSystem code. |

## Running order

The tickets are independent — no inter-ticket dependencies. You can run all at once:

```powershell
pwsh tools\local-llm\Run-LocalLLM.ps1
```

Or one at a time:

```powershell
pwsh tools\local-llm\Run-LocalLLM.ps1 -OnlyTicket 01_moon1-completion-tracker
```

## What each ticket delivers

| # | Ticket | Output | Why it matters for Moon 1 done |
|---|---|---|---|
| 01 | `01_moon1-completion-tracker.md` | `Assets/_Project/Scripts/Integration/Moon1CompletionTracker.cs` | Listens for 3 hero-building restorations, fires "MOON 1 COMPLETE" banner + sets `TARTARIA_Moon1Complete` PlayerPref. **Without this, players never get the "you did it" signal.** |
| 02 | `02_moon1-tutorial-hints.md` | `Assets/_Project/Scripts/Integration/Moon1FirstTimeHints.cs` | 5-step first-time tutorial hints (WASD, Press E, Restoration, etc.). Idempotent via PlayerPrefs. **Without this, new players don't know what to do.** |
| 03 | `03_mudgolem-loot-drop.md` | `Assets/_Project/Scripts/AI/MudGolemLootDrop.cs` | Mud Golems drop clay shards + a collectable RS coin (+8 RS) on death. **Without this, combat has no reward feedback loop.** |
| 04 | `04_magenta-audit-script.md` | `tools/audits/Find-MagentaPrimitives.ps1` | CI-safe script that flags every `CreatePrimitive` without URP shader fallback. **Without this, the magenta regressions keep coming back.** |
| 05 | `05_moon1-inn-rest-trigger.md` | `Assets/_Project/Scripts/Integration/Moon1InnRestTrigger.cs` | "Rest at the Inn" interactable that appears only after `TARTARIA_Moon1Complete == 1`. Pressing E sets `TARTARIA_CurrentMoon = 2` and stages the Moon 2 transition. **This is the Moon 1 → Moon 2 hand-off.** |

## After the local model runs

1. Open `tools/local-llm/LOCAL_OUTPUTS/<ticket-name>/response.md` for each
2. Extract the fenced C# code block (or .ps1 block)
3. Compile-check brace balance: open each .cs and verify `{` count == `}` count
4. Copy to the destination path listed at the top of each ticket
5. Open Unity, let it recompile, fix any CS errors that surface (most will be fine if the local model followed the spec)
6. Commit with message like `Moon 1 finish — 5 components from local LLM`

## What Claude (me) is doing next round

I'll drive Unity to:
- Recompile after the new files land
- Trigger `Tartaria → MASTER: Bootstrap All Moon 1 Systems` (so the new auto-bootstrap MonoBehaviours wire themselves in)
- Run `Tartaria → Ready Check (Audit + Bake + Save)` to confirm scene is playable
- Restart Play mode and verify each new feature:
  - Restore 3 buildings → see "MOON 1 COMPLETE" banner (ticket 01)
  - Reset PlayerPrefs → re-enter scene → see tutorial hints in order (ticket 02)
  - Kill a Mud Golem → see clay shards + RS coin (ticket 03)
  - Run `Find-MagentaPrimitives.ps1` → expect a list of fixable sites (ticket 04)
  - Walk to (10, 0.5, 5) after Moon 1 complete → see warm-glowing cube + Rest prompt (ticket 05)

## Things I am NOT delegating to the local model

- Driving Unity (clicks, screenshots, play-mode runs)
- Cross-file refactors (e.g. updating `MudGolemHealth.OnDeath` to call `MudGolemLootDrop.DropLoot` — that's a separate Claude-side task next round once ticket 03 lands)
- The Moon 1 → Moon 2 scene transition logic (next round, after Moon 2 scene exists)
- Bug diagnosis when something doesn't compile (Claude reads the error + adjusts)

## Token-budget upside

If all 5 tickets succeed in one shot, I save ~30-40k tokens of Claude scaffolding work this round and reserve them for the actual Moon 2 design + runtime verification. That's the whole point.
