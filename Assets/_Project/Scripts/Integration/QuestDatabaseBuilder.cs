using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using QuestObjective = Tartaria.Core.QuestObjective;

namespace Tartaria.Integration
{
    /// <summary>
    /// Builds the complete quest database for all Moons and R7 companion systems.
    /// </summary>
    public static class QuestDatabaseBuilder
    {
        public static QuestDefinition[] BuildAll()
        {
            var quests = new List<QuestDefinition>();

            // Moon 1 Echohaven Onboarding / FTUE starter quest (per 27_TUTORIAL_ONBOARDING + 03_CAMPAIGN Moon1 start + 07_PC_UX)
            quests.Add(Build("echohaven_awakening", "Echohaven Awakening",
                "Meet Milo, discover the first buried structure, and complete your first restoration to awaken the Aether field.",
                50f,
                new QuestObjective { description = "Meet Milo the companion", type = QuestObjectiveType.CompanionMilestone, targetId = "milo", targetCount = 1 },
                new QuestObjective { description = "Discover a Tartarian building", type = QuestObjectiveType.DiscoverBuilding, targetCount = 1 },
                new QuestObjective { description = "Complete first building restoration", type = QuestObjectiveType.RestoreBuilding, targetCount = 1 }));

            // R7 Companions & Reactivity: New production-quality companion milestone, trust arc, physical beat, giant synergy, calendar claimable, world mutation quests (Moons 1-3 depth + 4-13 hooks)
            // All wired to CompanionManager trust/mutation/calendar/giant + DialogueArcs + QuestManager CompanionMilestone type

            // Moon 1 R7 trust arc + physical restoration tell
            quests.Add(Build("r7_m1_milo_trust_arc", "Milo's First Sincere Tell",
                "Reach Milo Trust 25+ after first dome restoration — permanent hidden vein mutation unlocked.",
                rsReward: 120f,
                new QuestObjective { description = "Milo trust milestone + restoration physical tell", type = QuestObjectiveType.CompanionMilestone, targetId = "milo", targetCount = 25 }));

            quests.Add(Build("r7_m1_lirael_calendar_echo", "Lirael's 17th Whisper",
                "Trigger Lirael calendar echo during 17th Hour near restored dome.",
                rsReward: 80f,
                new QuestObjective { description = "Lirael 17th Hour echo + state change", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael", targetCount = 1 }));

            // Moon 2 R7 physical combat + giant synergy hook
            quests.Add(Build("r7_m2_cassian_redemption_prep", "Cassian's Doubt Deepens",
                "Advance Cassian trust to 50 during Moon 2 corruption purge — redemption branch + world intel mutation.",
                rsReward: 150f,
                new QuestObjective { description = "Cassian trust + physical combat react", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian", targetCount = 50 }));

            // === NEW MOON 2 COMPANION CATHEDRAL / CORRUPTION / CRYSTAL ARCS (Moon 2 Companion Stories & Reactivity R7) ===
            // Lirael: Crystal Choir — corruption nodes inside cathedral, song + purge, physical fracture/solidify tells
            quests.Add(Build("r7_m2_lirael_crystal_choir", "Lirael's Fractured Crystal Choir",
                "Accompany Lirael to 3 corrupted crystal nodes deep in the cathedral. Tune while she sings pre-corruption memories. Ties directly to Moon 2 cathedral theme and purge.",
                rsReward: 180f,
                new QuestObjective { description = "Lirael crystal node purges (3) + physical tell on success", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael", targetCount = 3 }));

            // Cassian expansion: Cathedral Analysis — ambiguous path choice with physical tells and trust branch
            quests.Add(Build("r7_m2_cassian_cathedral_analysis", "Cassian's Cathedral Fracture Analysis",
                "Work with (or confront) Cassian mapping corruption veins in the living crystal cathedral. Choice impacts trust and unlocks permanent intel markers.",
                rsReward: 160f,
                new QuestObjective { description = "Cassian cathedral analysis quest + trust branch + physical dissonance tell", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian", targetCount = 1 }));

            // Korath foreshadow (early echo): Builder's Shadow in crystal heart — inscription resonance, permanent stone memory
            quests.Add(Build("r7_m2_korath_builder_echo", "Korath's Stone Shadow in the Cathedral",
                "Discover Korath's giant echo inscription in the deepest cathedral crystal chamber during purge. Resonates 'the song inverted'. Early trust seed + world mutation foreshadow.",
                rsReward: 140f,
                new QuestObjective { description = "Korath cathedral echo resonance + physical stone hum tell + early trust", type = QuestObjectiveType.CompanionMilestone, targetId = "korath", targetCount = 1 }));

            // Anastasia: Archive Facets — 17th Hour crystal interaction + Golden Mote #2 extension, motes + warmth
            quests.Add(Build("r7_m2_anastasia_crystal_archive", "Anastasia's Facets of the Archive",
                "During 17th Hour in the cathedral, share resonance with Anastasia among living crystals. Motes interact with veins; unlocks permanent warmer caustics + extra whispers.",
                rsReward: 130f,
                new QuestObjective { description = "Anastasia cathedral crystal mote share + 17th physical glow + world warmth mutation", type = QuestObjectiveType.CompanionMilestone, targetId = "anastasia", targetCount = 1 }));

            // === CORE MOON 2 5-BEAT FTUE QUEST: "lunar_challenge" (Lane 4 Narrative per 03C + 03_CAMPAIGN) ===
            // Expanded with full Cassian trust/doubt arc, Lirael memory solidifies, returning player guards, rich dialogue hooks,
            // companion physical tells, and deep replayable "The Crystal Remembers" revelation experience.
            quests.Add(Build("lunar_challenge", "The Lunar Challenge",
                "5-beat FTUE for Moon 2 — Lunar Moon: Shadow & Purge. Discovery (Lirael fracture + Cassian beckon), Restoration (micro-giant crystal tuning), Conflict (first Mud Golem + trust tick), Climax (ionized fountain storm dome purify), Revelation (Cassian diary ambiguity + The Crystal Remembers banner). Integrates QuestDatabase, Dialogue, CompanionManager physical tells, WorldChoice W1, and replayable crystal memory echoes.",
                rsReward: 520f,
                new QuestObjective { description = "Discovery: Witness Lirael fracture + Cassian scholar beckon", type = QuestObjectiveType.DiscoverBuilding, targetId = "moon2_discovery", targetCount = 1 },
                new QuestObjective { description = "Restoration: Complete micro-giant crystal tuning", type = QuestObjectiveType.RestoreBuilding, targetId = "micro_giant_tune", targetCount = 1 },
                new QuestObjective { description = "Conflict: Defeat first Mud Golem + Cassian trust/doubt tick", type = QuestObjectiveType.CompanionMilestone, targetId = "mud_golem_first", targetCount = 1 },
                new QuestObjective { description = "Climax: Purify ionized fountain + storm dome cleanse", type = QuestObjectiveType.RestoreBuilding, targetId = "moon2_fountain", targetCount = 1 },
                new QuestObjective { description = "Revelation: Cassian diary choice + The Crystal Remembers deep replayable experience", type = QuestObjectiveType.CompanionMilestone, targetId = "crystal_remembers", targetCount = 1 }));

            // Moon 3 R7 escort physical + giant song + Veritas intro synergy
            quests.Add(Build("r7_m3_escort_giant_song", "Orphan Train Giant Song Match",
                "Complete rail escort with high bond (Korath/Veritas) — Giant's Song auto-match + Companion Giant payoff.",
                rsReward: 220f,
                new QuestObjective { description = "Escort physical tells + giant synergy + Veritas precision", type = QuestObjectiveType.CompanionMilestone, targetId = "korath", targetCount = 1 }));

            quests.Add(Build("r7_m3_veritas_calendar_claim", "Veritas Bell Echo Claimable",
                "Claim Veritas 17th Hour calendar event after first train escort — permanent resonance mutation.",
                rsReward: 95f,
                new QuestObjective { description = "Veritas calendar claimable + world mutation tier", type = QuestObjectiveType.CompanionMilestone, targetId = "veritas", targetCount = 1 }));

            // === FULL MOON 3 "Compassion & Rails" ARC (M3-MS06 per quest DB + 20_QUEST + GDD + 03C) ===
            // Wires orphan adoption (trust + lullaby), rail escort, Leviathan defeat, post-escort Continental + golden rails + World's Fair
            quests.Add(Build("orphan_train_escort", "Compassion & Rails: The Orphan Train Escort (M3-MS06)",
                "Discover the Dissonant Orphan Train in the Windswept Highlands. Build trust with spectral orphans Aria, Toren and Syl through lullaby and protection. Escort the train through corruption, defeat the Dissonance Leviathan with collective song, and unlock the Continental Rail network + permanent golden rails + World's Fair access as Moon 3 completion payoffs.",
                rsReward: 380f,
                new QuestObjective { description = "Discover the spectral orphan train and rail stations", type = QuestObjectiveType.DiscoverBuilding, targetId = "orphan_train", targetCount = 1 },
                new QuestObjective { description = "Adopt spectral orphans via trust-building and lullaby contributions (3)", type = QuestObjectiveType.CompanionMilestone, targetId = "spectral_orphans", targetCount = 3 },
                new QuestObjective { description = "Complete the full rail escort climax (protect train, lullaby shield)", type = QuestObjectiveType.CompleteTuning, targetId = "rail_escort", targetCount = 1 },
                new QuestObjective { description = "Defeat Dissonance Leviathan with orphan lullaby synergy", type = QuestObjectiveType.HiddenDiscovery, targetId = "leviathan_moon3", targetCount = 1 },
                new QuestObjective { description = "Unlock post-escort Continental Rail fast travel + golden rails permanent change + World's Fair ticket", type = QuestObjectiveType.CompanionMilestone, targetId = "continental_rail_m3", targetCount = 1 }));

            // Anastasia R7 solidification + all 112 context + giant
            quests.Add(Build("r7_anastasia_solidif_giant", "Anastasia's Ground & Giant Glow",
                "Witness solidification + trigger giant synergy with Anastasia/Cassian bond — full cross-Moon memory payoff.",
                rsReward: 350f,
                new QuestObjective { description = "Anastasia solidification + giant synergy + 112 memory", type = QuestObjectiveType.CompanionMilestone, targetId = "anastasia", targetCount = 75 }));

            // General R7 live-ops / trust pricing / daily
            quests.Add(Build("r7_daily_banter_claim", "Milo's Daily Deal (Live-Ops)",
                "Claim daily Milo banter + trust pricing event (rotating calendar).",
                rsReward: 40f,
                new QuestObjective { description = "Daily banter + claimable trust event", type = QuestObjectiveType.CompanionMilestone, targetId = "milo", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 4: Self-Existing Moon — Form of Foundations
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon4_starfort_foundation", "Moon 4: The Star Fort's Foundation",
                "Construct the 12-pointed star fort in the Windswept Highlands. Align 12 bastions, purge the moat aquifer, and awaken guardian golem Maelix. Discover the 17-Hour Clock Fragment — a key to Moon 9's prophecy timeline.",
                rsReward: 450f,
                new QuestObjective { description = "Discover the star fort ruins and alignment markers", type = QuestObjectiveType.DiscoverBuilding, targetId = "starfort_discovery", targetCount = 1 },
                new QuestObjective { description = "Align all 12 bastions with golden ratio geometry", type = QuestObjectiveType.RestoreBuilding, targetId = "bastion_alignment", targetCount = 12 },
                new QuestObjective { description = "Complete aquifer purge minigame (6 moat segments)", type = QuestObjectiveType.CompleteTuning, targetId = "aquifer_purge_m4", targetCount = 6 },
                new QuestObjective { description = "Awaken Maelix the golem (Korath's brother) + discover 17-Hour Clock Fragment", type = QuestObjectiveType.HiddenDiscovery, targetId = "maelix_awakening", targetCount = 1 }));

            quests.Add(Build("r7_m4_maelix_brotherhood", "Maelix: Builder's Brotherhood",
                "Earn Maelix's trust through star fort construction. Learn Korath's past as builder and Zereth's shadow role. Permanent stone memory mutation unlocked.",
                rsReward: 180f,
                new QuestObjective { description = "Maelix trust arc + Korath/Zereth lore reveal + physical stone resonance tell", type = QuestObjectiveType.CompanionMilestone, targetId = "maelix", targetCount = 50 }));

            quests.Add(Build("r7_m4_17hour_clock_fragment", "The 17th Hour: Clock Fragment Resonance",
                "During 17th Hour, resonate with the Clock Fragment. See echoes of Moon 9's prophecy stones. World mutation: time perception shifts near star fort.",
                rsReward: 200f,
                new QuestObjective { description = "17th Hour Clock Fragment interaction + Moon 9 timeline foreshadow + world time mutation", type = QuestObjectiveType.HiddenDiscovery, targetId = "clock_fragment_17h", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 5: Overtone Moon — Radiance of Empowerment
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon5_white_city_restoration", "Moon 5: The White City Awakens",
                "Restore the 5 pavilions of the White City. Unlock 6-band healing frequencies. Meet Captain Thorne via crackling radio. Construct the central spire ley-line bridge — gateway to Moon 6's sunken cathedral.",
                rsReward: 520f,
                new QuestObjective { description = "Discover White City ruins and Thorne's radio signal", type = QuestObjectiveType.DiscoverBuilding, targetId = "white_city_discovery", targetCount = 1 },
                new QuestObjective { description = "Restore all 5 White City pavilions (Beaux-Arts architecture)", type = QuestObjectiveType.RestoreBuilding, targetId = "pavilion_restoration", targetCount = 5 },
                new QuestObjective { description = "Unlock 6-band healing frequencies (upgrade from 3-band)", type = QuestObjectiveType.CompleteTuning, targetId = "sixband_unlock", targetCount = 1 },
                new QuestObjective { description = "Complete central spire ley-line bridge + Thorne encounter", type = QuestObjectiveType.RestoreBuilding, targetId = "spire_bridge_m5", targetCount = 1 }));

            quests.Add(Build("r7_m5_thorne_introduction", "Thorne: Voice from the Clouds",
                "Build trust with Captain Thorne through radio exchanges. Learn of 200-year orbital isolation. Airship dock construction foreshadows Moon 8 fleet. Physical tell: distant engine hum.",
                rsReward: 220f,
                new QuestObjective { description = "Thorne trust arc + radio backstory + airship dock mutation + engine resonance tell", type = QuestObjectiveType.CompanionMilestone, targetId = "thorne", targetCount = 50 }));

            quests.Add(Build("r7_m5_pavilion_giant_synergy", "White City Giant Resonance",
                "Trigger giant mode during pavilion restoration. Golden glow intensifies. Permanent amplification mutation: all 5 pavilions glow warmer when giant is active.",
                rsReward: 190f,
                new QuestObjective { description = "Giant mode + pavilion synergy + world amplification mutation + physical warmth tell", type = QuestObjectiveType.CompanionMilestone, targetId = "giant_pavilion", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 6: Rhythmic Moon — Equality of Flow
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon6_cathedral_requiem", "Moon 6: The Sunken Cathedral's Requiem",
                "Descend to the sunken crystal cathedral. Restore the 12-pipe organ and 6 hydraulic fountains. Conduct the Cymatic Requiem with Lirael. Witness her solidification progress. Unlock prerequisite mechanics for Moon 12's bell tower network.",
                rsReward: 580f,
                new QuestObjective { description = "Discover the sunken cathedral deep underground", type = QuestObjectiveType.DiscoverBuilding, targetId = "cathedral_discovery_m6", targetCount = 1 },
                new QuestObjective { description = "Restore the 12-pipe crystal organ (fractured pipes)", type = QuestObjectiveType.RestoreBuilding, targetId = "organ_restoration", targetCount = 12 },
                new QuestObjective { description = "Activate all 6 hydraulic fountains (dry basins)", type = QuestObjectiveType.RestoreBuilding, targetId = "fountain_restoration_m6", targetCount = 6 },
                new QuestObjective { description = "Conduct Cymatic Requiem with Lirael + witness solidification progress", type = QuestObjectiveType.CompleteTuning, targetId = "requiem_completion", targetCount = 1 }));

            quests.Add(Build("r7_m6_lirael_choir_conductor", "Lirael: The Choir Conductor's Solidification",
                "Support Lirael as she conducts cathedral choirs. Each note brings her closer to solid form. Passive choir buff unlocked. Physical tell: voice resonance strengthens.",
                rsReward: 280f,
                new QuestObjective { description = "Lirael conductor trust arc + solidification milestone + passive choir buff + voice resonance tell", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael", targetCount = 75 }));

            quests.Add(Build("r7_m6_organ_9band_freeze", "The Frozen 9-Band Note",
                "During organ tuning, discover a single frozen 9-band note (Korath's signature). Foreshadows Moon 7 awakening. World mutation: cathedral acoustics permanently enhanced.",
                rsReward: 210f,
                new QuestObjective { description = "Discover frozen 9-band note + Korath foreshadow + world acoustics mutation", type = QuestObjectiveType.HiddenDiscovery, targetId = "frozen_note_m6", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 7: Resonant Moon — Attunement of Channeling
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon7_korath_awakening", "Moon 7: Korath Awakens — The 9-Band Resonance",
                "Thaw Korath from his ice stasis over 3 tuning sessions. Unlock 9-band frequency manipulation. Confront Cassian's ambiguity (redemption branch). Defend against golem siege. Witness Korath's sacrifice echo — his voice appears in ALL future Moons.",
                rsReward: 680f,
                new QuestObjective { description = "Discover Korath's ice block in stasis vault", type = QuestObjectiveType.DiscoverBuilding, targetId = "korath_discovery", targetCount = 1 },
                new QuestObjective { description = "Complete 3 thawing sessions (violet aurora 9-band energy)", type = QuestObjectiveType.CompleteTuning, targetId = "korath_thaw", targetCount = 3 },
                new QuestObjective { description = "Unlock 9-band frequency manipulation (upgrade from 6-band)", type = QuestObjectiveType.CompleteTuning, targetId = "nineband_unlock", targetCount = 1 },
                new QuestObjective { description = "Cassian confrontation + redemption branch choice", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian_fork_m7", targetCount = 1 },
                new QuestObjective { description = "Defend golem siege + witness Korath sacrifice echo activation", type = QuestObjectiveType.HiddenDiscovery, targetId = "korath_sacrifice", targetCount = 1 }));

            quests.Add(Build("r7_m7_korath_rock_cutting", "Korath: Harmonic Rock Cutting Master",
                "Learn harmonic rock cutting from Korath. Permanent upgrade: airships (Moon 8) and trains (Moon 10) can now cut through mountains. Physical tell: low-frequency hum in stone.",
                rsReward: 320f,
                new QuestObjective { description = "Korath rock cutting trust arc + permanent transport upgrade + stone hum tell", type = QuestObjectiveType.CompanionMilestone, targetId = "korath", targetCount = 100 }));

            quests.Add(Build("r7_m7_cassian_redemption", "Cassian: The Redemption Fork",
                "Confront Cassian during Korath awakening. Choose trust or suspicion. Cassian's fate in Moon 9 depends on this choice. World mutation: intel markers change color based on path.",
                rsReward: 350f,
                new QuestObjective { description = "Cassian redemption choice + trust branch + Moon 9 fate fork + intel mutation", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian_redemption", targetCount = 1 }));

            quests.Add(Build("r7_m7_half_grid_lit", "The Half Grid Illuminates",
                "Korath's awakening lights half the planetary ley-line grid. World mutation: global map visual transform. Giant synergy: Korath echo resonates with Giant's Song.",
                rsReward: 290f,
                new QuestObjective { description = "Half grid activation + world map mutation + giant echo synergy", type = QuestObjectiveType.HiddenDiscovery, targetId = "half_grid_m7", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 8: Galactic Moon — Integrity of Harmonizing
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon8_airship_fleet_landing", "Moon 8: The Fleet Descends — Thorne's Return",
                "Captain Thorne lands his flagship at White City dock. Restore 3 airships from the graveyard. Tune mercury orbs (4 per ship). Transport megaliths via aerial construction. Children from Moon 3 board the ships — Thorne's gruff acceptance. Unlock fast-travel backbone for continental scale.",
                rsReward: 750f,
                new QuestObjective { description = "Witness Thorne flagship landing at White City", type = QuestObjectiveType.DiscoverBuilding, targetId = "flagship_landing", targetCount = 1 },
                new QuestObjective { description = "Restore all 3 airships from the graveyard", type = QuestObjectiveType.RestoreBuilding, targetId = "airship_restoration", targetCount = 3 },
                new QuestObjective { description = "Tune all mercury orbs (12 total: 4 per ship)", type = QuestObjectiveType.CompleteTuning, targetId = "mercury_tuning", targetCount = 12 },
                new QuestObjective { description = "Complete megalith transport mission + children board ships", type = QuestObjectiveType.HiddenDiscovery, targetId = "megalith_transport", targetCount = 1 }));

            quests.Add(Build("r7_m8_thorne_fleet_commander", "Thorne: From Hermit to Fleet Commander",
                "Build trust with Thorne as he accepts leadership. Children's presence softens him. Aerial combat tutorial. Physical tell: formation lights glow when Thorne gives orders.",
                rsReward: 340f,
                new QuestObjective { description = "Thorne fleet commander arc + children bonding + aerial combat + formation lights tell", type = QuestObjectiveType.CompanionMilestone, targetId = "thorne", targetCount = 100 }));

            quests.Add(Build("r7_m8_children_engineers", "The Orphan Engineers Aloft",
                "Moon 3 orphans become airship engineers. Giant synergy: children sing lullaby during flight, ships glow golden. World mutation: airship trails leave harmonic contrails.",
                rsReward: 280f,
                new QuestObjective { description = "Children engineer role + giant lullaby synergy + contrails mutation + song resonance tell", type = QuestObjectiveType.CompanionMilestone, targetId = "orphan_engineers", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 9: Solar Moon — Intention of Pulse
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon9_prophecy_stones_timeline", "Moon 9: The 12 Prophecy Stones — Visions of Intent",
                "Discover 12 prophecy stones across the floating aurora city. Each stone reveals a timeline vision. Requires Moon 4's 17-Hour Clock Fragment to unlock correct sequence. Cassian's fate conditional on Moon 7 choice. Zereth begins confession — Korath's third brother, architect of the Mud Flood trigger. Build the prophecy clock tower.",
                rsReward: 820f,
                new QuestObjective { description = "Discover floating aurora city and first prophecy stone", type = QuestObjectiveType.DiscoverBuilding, targetId = "aurora_city_discovery", targetCount = 1 },
                new QuestObjective { description = "Collect all 12 prophecy stones (timeline visions)", type = QuestObjectiveType.HiddenDiscovery, targetId = "prophecy_stones", targetCount = 12 },
                new QuestObjective { description = "Unlock prophecy sequence with 17-Hour Clock Fragment", type = QuestObjectiveType.CompleteTuning, targetId = "prophecy_unlock", targetCount = 1 },
                new QuestObjective { description = "Cassian conditional encounter (redemption or confrontation)", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian_m9_conditional", targetCount = 1 },
                new QuestObjective { description = "Build prophecy clock tower + Zereth confession begins", type = QuestObjectiveType.RestoreBuilding, targetId = "clock_tower_m9", targetCount = 1 }));

            quests.Add(Build("r7_m9_zereth_confession", "Zereth: The Architect's Confession Begins",
                "Zereth reveals himself as Korath's third brother. Designed the trigger room (Moon 10). Guilt-driven. Trust arc begins — redemption or exile path. Physical tell: shadow echoes.",
                rsReward: 380f,
                new QuestObjective { description = "Zereth confession arc + Korath brotherhood reveal + redemption path + shadow tell", type = QuestObjectiveType.CompanionMilestone, targetId = "zereth", targetCount = 50 }));

            quests.Add(Build("r7_m9_timeline_convergence", "Timeline Convergence: The Visions Align",
                "All 12 prophecy stones resonate simultaneously. See past (Mud Flood), present (Reset war), future (3 endings). World mutation: aurora city phases between timelines. Giant synergy: timeline visions amplified.",
                rsReward: 420f,
                new QuestObjective { description = "Timeline convergence vision + 3 endings foreshadow + phasing mutation + amplified visions giant synergy", type = QuestObjectiveType.HiddenDiscovery, targetId = "timeline_convergence", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 10: Planetary Moon — Manifestation of Producing
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon10_continental_rail_network", "Moon 10: The Continental Rail Network — Children as Engineers",
                "Restore the planetary rail network connecting all continents. Moon 3 orphans become train engineers. Discover the Mud Flood trigger room — 3 giant fingerprints on the control panel. Zereth's guilt deepens. Collect prophecy stones 7-9. Korath echo guides through rock-cutting upgrades.",
                rsReward: 920f,
                new QuestObjective { description = "Discover continental rail hub and trigger room", type = QuestObjectiveType.DiscoverBuilding, targetId = "rail_hub_discovery", targetCount = 1 },
                new QuestObjective { description = "Restore all rail network segments (continental scale)", type = QuestObjectiveType.RestoreBuilding, targetId = "rail_network_restoration", targetCount = 1 },
                new QuestObjective { description = "Children become train engineers (Moon 3 orphans)", type = QuestObjectiveType.CompanionMilestone, targetId = "children_engineers_m10", targetCount = 1 },
                new QuestObjective { description = "Discover trigger room + 3 giant fingerprints (Korath, Maelix, Zereth)", type = QuestObjectiveType.HiddenDiscovery, targetId = "trigger_room_discovery", targetCount = 1 },
                new QuestObjective { description = "Collect prophecy stones 7-9 + Zereth guilt arc deepens", type = QuestObjectiveType.HiddenDiscovery, targetId = "prophecy_stones_7_9", targetCount = 3 }));

            quests.Add(Build("r7_m10_zereth_guilt_depth", "Zereth: The Weight of the Trigger",
                "Zereth confronts the trigger room. Reveals he designed it for 'mercy reset' — the Mud Flood was to purge corruption, but killed millions. Redemption path: seal the trigger. Exile path: flee. Physical tell: tremors near trigger room.",
                rsReward: 450f,
                new QuestObjective { description = "Zereth trigger room confrontation + mercy reveal + redemption fork + tremor tell", type = QuestObjectiveType.CompanionMilestone, targetId = "zereth", targetCount = 100 }));

            quests.Add(Build("r7_m10_children_rail_mastery", "The Children's Rail Mastery",
                "Orphans operate trains with precision. Giant synergy: Giant's Song harmonizes with rail hum. World mutation: all trains glow golden when children are aboard. Thorne's fleet coordinates with rail network.",
                rsReward: 360f,
                new QuestObjective { description = "Children rail mastery + giant rail synergy + golden trains mutation + coordinated transport", type = QuestObjectiveType.CompanionMilestone, targetId = "children_rail", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 11: Spectral Moon — Liberation of Release
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon11_aquifer_purification", "Moon 11: The Ancient Aquifer — Planetary Fountain Chain",
                "Purge the ancient aquifer connecting all continents. Activate the planetary fountain chain (prerequisite for Moon 12 bell sync). Ionized mist auroras heal the land. Continent-wide restoration. Lirael reaches near-solid form. Korath echo resonates through water.",
                rsReward: 880f,
                new QuestObjective { description = "Discover ancient aquifer network beneath continents", type = QuestObjectiveType.DiscoverBuilding, targetId = "aquifer_discovery_m11", targetCount = 1 },
                new QuestObjective { description = "Purge all aquifer corruption layers (planetary scale)", type = QuestObjectiveType.CompleteTuning, targetId = "aquifer_purge_m11", targetCount = 1 },
                new QuestObjective { description = "Activate planetary fountain chain (all continents)", type = QuestObjectiveType.RestoreBuilding, targetId = "fountain_chain_activation", targetCount = 1 },
                new QuestObjective { description = "Witness ionized mist auroras + continent healing + Lirael near-solid", type = QuestObjectiveType.HiddenDiscovery, targetId = "continent_healing", targetCount = 1 }));

            quests.Add(Build("r7_m11_lirael_near_solid", "Lirael: The Threshold of Solidity",
                "Lirael's voice becomes tangible during aquifer purification. She can now touch water — first physical contact. Solidification 90% complete. Physical tell: water ripples at her voice.",
                rsReward: 400f,
                new QuestObjective { description = "Lirael near-solid milestone + water touch + voice ripples tell + solidification 90%", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael", targetCount = 100 }));

            quests.Add(Build("r7_m11_korath_water_echo", "Korath: Echo Through Water",
                "Korath's echo resonates through aquifer network. Teaches water harmonics. Giant synergy: aquifer glows violet when giant is active near fountains. World mutation: all water sources hum at 432Hz.",
                rsReward: 340f,
                new QuestObjective { description = "Korath water echo + harmonics teaching + giant aquifer synergy + 432Hz water mutation", type = QuestObjectiveType.CompanionMilestone, targetId = "korath_echo_water", targetCount = 1 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 12: Crystal Moon — Cooperation of Dedicating
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon12_bell_tower_synchronization", "Moon 12: The 12 Bell Towers — Planetary Ring of Harmony",
                "Synchronize 12 bell towers across all 12 continents. Each tower must be tuned to 432Hz. At 8/12 towers, Reset forces launch assault — defend all towers. At 12/12, trigger the planetary ring: all bells sound in harmony for 60 seconds. Golden scalar waves circle the planet. Auroras bloom. Collect prophecy stone #12: Stone of Promise. Unlock Moon 13.",
                rsReward: 1020f,
                new QuestObjective { description = "Discover all 12 bell towers on 12 continents", type = QuestObjectiveType.DiscoverBuilding, targetId = "bell_tower_discovery", targetCount = 12 },
                new QuestObjective { description = "Synchronize all 12 bell towers to 432Hz", type = QuestObjectiveType.CompleteTuning, targetId = "bell_sync", targetCount = 12 },
                new QuestObjective { description = "Defend bell towers during Reset assault (8/12 trigger)", type = QuestObjectiveType.HiddenDiscovery, targetId = "reset_assault_m12", targetCount = 1 },
                new QuestObjective { description = "Trigger planetary ring (60s harmony) + golden scalar waves + collect stone #12", type = QuestObjectiveType.HiddenDiscovery, targetId = "planetary_ring", targetCount = 1 }));

            quests.Add(Build("r7_m12_korath_echo_bells", "Korath: Echo Across 12 Bell Towers",
                "Korath's echo appears at EVERY bell tower. Dialogue reveals final truth of his sacrifice. Physical tell: bell tones carry his voice harmonics. Giant synergy: bells ring louder when giant is near.",
                rsReward: 480f,
                new QuestObjective { description = "Korath echo at all 12 towers + sacrifice truth + voice harmonics tell + giant bell synergy", type = QuestObjectiveType.CompanionMilestone, targetId = "korath_echo_bells", targetCount = 12 }));

            quests.Add(Build("r7_m12_lirael_full_solid", "Lirael: Full Solidification",
                "During planetary ring, Lirael becomes fully solid for the first time. Can touch, feel, cry. Companion arc complete. Physical tell: her form stabilizes, glow fades to natural skin tone.",
                rsReward: 520f,
                new QuestObjective { description = "Lirael full solidification + first touch/tears + arc complete + stabilized form tell", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael", targetCount = 150 }));

            // ═══════════════════════════════════════════════════════════════════════════════
            // MOON 13: Cosmic Moon — Transcendence of Presence
            // ═══════════════════════════════════════════════════════════════════════════════

            quests.Add(Build("moon13_cosmic_convergence", "Moon 13: The Hidden Moon — Cosmic Convergence",
                "Enter the 3 echo realms (past, present, future). Zereth's full truth revealed. Resonance dialogue with all companions. 3-path climax: Harmony Ending (restore), Echo Ending (preserve memory), Reset Ending (purge corruption). Day Out of Time epilogue — Anastasia fully solidifies. The 13-Moon cycle completes.",
                rsReward: 1500f,
                new QuestObjective { description = "Enter 3 echo realms (past/present/future)", type = QuestObjectiveType.HiddenDiscovery, targetId = "echo_realms", targetCount = 3 },
                new QuestObjective { description = "Zereth's full truth revelation + architect guilt resolved", type = QuestObjectiveType.CompanionMilestone, targetId = "zereth_truth_m13", targetCount = 1 },
                new QuestObjective { description = "Resonance dialogue with all companions (final goodbye)", type = QuestObjectiveType.CompanionMilestone, targetId = "resonance_dialogue", targetCount = 1 },
                new QuestObjective { description = "Choose ending path: Harmony / Echo / Reset", type = QuestObjectiveType.HiddenDiscovery, targetId = "ending_choice", targetCount = 1 },
                new QuestObjective { description = "Day Out of Time epilogue + Anastasia full solidification", type = QuestObjectiveType.HiddenDiscovery, targetId = "day_out_of_time", targetCount = 1 }));

            quests.Add(Build("r7_m13_anastasia_solidification", "Anastasia: The Archive Made Flesh",
                "Anastasia becomes fully solid during Day Out of Time. All 13 golden motes converge. Can speak aloud for first time. Companion arc complete. Physical tell: golden motes orbit her, then sink into her chest.",
                rsReward: 650f,
                new QuestObjective { description = "Anastasia full solidification + 13 motes converge + first spoken words + arc complete", type = QuestObjectiveType.CompanionMilestone, targetId = "anastasia", targetCount = 150 }));

            quests.Add(Build("r7_m13_zereth_redemption_finale", "Zereth: Redemption or Exile — The Architect's Fate",
                "Zereth's redemption path culminates. If redeemed, he seals the trigger room forever (Harmony Ending support). If exiled, he flees to echo realm (Echo Ending support). Companion arc complete.",
                rsReward: 580f,
                new QuestObjective { description = "Zereth redemption or exile finale + trigger sealing or echo flight + arc complete", type = QuestObjectiveType.CompanionMilestone, targetId = "zereth", targetCount = 150 }));

            return quests.ToArray();
        }

        static QuestDefinition Build(string id, string name, string desc, float rsReward, params QuestObjective[] objectives)
        {
            return Build(id, name, desc, false, false, rsReward, 0f, null, objectives);
        }

        /// <summary>Full-arg Build overload used by main-quest registrations (R7 wiring).</summary>
        static QuestDefinition Build(string id, string name, string desc,
            bool isMainQuest, bool autoActivate, float rsReward, float rsRequirement,
            string[] followUpQuestIds, params QuestObjective[] objectives)
        {
            var q = ScriptableObject.CreateInstance<QuestDefinition>();
            q.questId = id;
            q.displayName = name;
            q.description = desc;
            q.isMainQuest = isMainQuest;
            q.autoActivate = autoActivate;
            q.rsReward = rsReward;
            q.rsRequirement = rsRequirement;
            q.followUpQuestIds = followUpQuestIds ?? System.Array.Empty<string>();
            q.objectives = objectives ?? System.Array.Empty<QuestObjective>();
            return q;
        }

        // ... existing Build overloads unchanged ...
    }
}
