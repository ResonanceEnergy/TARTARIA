using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime quest factory — builds QuestDefinition ScriptableObjects for all 13 Moons.
    /// Each Moon has a main quest chain plus companion/side quests.
    /// Called from GameLoopController or an editor tool.
    ///
    /// Cross-ref: docs/20_QUEST_DATABASE.md
    /// </summary>
    public static class QuestDatabaseBuilder
    {
        public static QuestDefinition[] BuildAll()
        {
            var quests = new List<QuestDefinition>();

            // ─── Moon 1: Magnetic Moon — Echohaven ───────
            quests.Add(Build("echo_main_discover", "Echoes of the Buried City",
                "Discover and restore three buildings in Echohaven.",
                isMain: true, autoActivate: true, rsReward: 10f,
                new QuestObjective { description = "Discover buildings", type = QuestObjectiveType.DiscoverBuilding, targetCount = 3 },
                new QuestObjective { description = "Restore buildings", type = QuestObjectiveType.RestoreBuilding, targetCount = 3 }));

            quests.Add(Build("echo_side_golem", "Golem Graveyard",
                "Defeat all corruption-spawned enemies in Echohaven.",
                rsReward: 5f,
                new QuestObjective { description = "Defeat enemies", type = QuestObjectiveType.DefeatEnemies, targetCount = 10 }));

            quests.Add(Build("echo_companion_cassian", "A Soldier's Doubt",
                "Speak with Cassian about the garrison ruins.",
                rsReward: 3f,
                new QuestObjective { description = "Talk to Cassian", type = QuestObjectiveType.TalkToNPC, targetId = "cassian", targetCount = 1 }));

            // ─── Moon 2: Lunar Moon — Crystal Caverns ────
            quests.Add(Build("lunar_main_caverns", "The Crystal Frequency",
                "Tune the resonance crystals in the underground caverns.",
                isMain: true, rsReward: 12f, rsRequirement: 10f,
                followUp: new[] { "lunar_side_lirael" },
                new QuestObjective { description = "Complete crystal tuning nodes", type = QuestObjectiveType.CompleteTuning, targetCount = 5 },
                new QuestObjective { description = "Reach RS 25", type = QuestObjectiveType.ReachRS, targetCount = 25 }));

            quests.Add(Build("lunar_side_lirael", "Lirael's First Song",
                "Help Lirael remember a fragment of her lost melody.",
                rsReward: 5f,
                new QuestObjective { description = "Talk to Lirael", type = QuestObjectiveType.TalkToNPC, targetId = "lirael", targetCount = 3 }));

            // ─── Moon 3: Electric Moon — The Orphan Train ─
            quests.Add(Build("orphan_train_main", "The Orphan Train",
                "Uncover the orphan train route and the children's fate.",
                isMain: true, rsReward: 15f, rsRequirement: 20f,
                new QuestObjective { description = "Discover rail stations", type = QuestObjectiveType.DiscoverBuilding, targetCount = 4 },
                new QuestObjective { description = "Restore telegraph office", type = QuestObjectiveType.RestoreBuilding, targetId = "telegraph_office", targetCount = 1 },
                new QuestObjective { description = "Defeat corruption guardians", type = QuestObjectiveType.DefeatEnemies, targetCount = 8 }));

            quests.Add(Build("orphan_companion_milo", "Milo's Connection",
                "Help Milo process the orphan train discovery.",
                rsReward: 5f,
                new QuestObjective { description = "Witness Milo's reaction", type = QuestObjectiveType.CompanionMilestone, targetId = "milo_orphan_train", targetCount = 1 }));

            // ─── Moon 4: Self-Existing Moon — Star Fort ───
            quests.Add(Build("star_fort_main", "Star Fort Siege",
                "Defend the star fort against the corruption siege.",
                isMain: true, rsReward: 18f, rsRequirement: 30f,
                new QuestObjective { description = "Restore five star points", type = QuestObjectiveType.RestoreBuilding, targetCount = 5 },
                new QuestObjective { description = "Defeat Siege Golems", type = QuestObjectiveType.DefeatBoss, targetId = "siege_golem_alpha", targetCount = 1 },
                new QuestObjective { description = "Synchronize bell towers", type = QuestObjectiveType.CompleteTuning, targetCount = 3 }));

            quests.Add(Build("star_fort_cassian", "Cassian's Reconnaissance",
                "Join Cassian's scouting mission beyond the walls.",
                rsReward: 6f,
                new QuestObjective { description = "Complete reconnaissance route", type = QuestObjectiveType.CompleteZone, targetId = "star_fort_outer", targetCount = 1 }));

            // ─── Moon 5: Overtone Moon — White City ───────
            quests.Add(Build("white_city_main", "The White City Revelation",
                "Explore the ruins of the demolished White City and restore the intercontinental relay.",
                isMain: true, rsReward: 20f, rsRequirement: 40f,
                new QuestObjective { description = "Discover White City foundations", type = QuestObjectiveType.DiscoverBuilding, targetCount = 3 },
                new QuestObjective { description = "Activate underground telegraph", type = QuestObjectiveType.RestoreBuilding, targetId = "telegraph_relay", targetCount = 1 },
                new QuestObjective { description = "Complete intercontinental bridge", type = QuestObjectiveType.CompleteTuning, targetCount = 5 }));

            quests.Add(Build("white_city_milo", "Milo's Outburst",
                "Witness Milo confront the truth about the White City demolition.",
                rsReward: 5f,
                new QuestObjective { description = "Milo's White City moment", type = QuestObjectiveType.CompanionMilestone, targetId = "milo_white_city", targetCount = 1 }));

            // ─── Moon 6: Rhythmic Moon — Cymatic Cathedral ─
            quests.Add(Build("cathedral_main", "The Cymatic Requiem",
                "Restore the five organ registers and perform the Requiem.",
                isMain: true, rsReward: 22f, rsRequirement: 50f,
                new QuestObjective { description = "Restore organ registers", type = QuestObjectiveType.RestoreBuilding, targetCount = 5 },
                new QuestObjective { description = "Complete performance mini-game", type = QuestObjectiveType.CompleteMiniGame, targetId = "requiem_performance", targetCount = 1 },
                new QuestObjective { description = "Defeat Dissonant Conductor", type = QuestObjectiveType.DefeatBoss, targetId = "dissonant_conductor", targetCount = 1 }));

            quests.Add(Build("cathedral_veritas", "Veritas: The Organist's Trust",
                "Earn Veritas's trust and learn the Silver Passage.",
                rsReward: 8f,
                new QuestObjective { description = "Reach Harmony trust tier", type = QuestObjectiveType.CompanionMilestone, targetId = "veritas_harmony", targetCount = 1 }));

            quests.Add(Build("cathedral_lirael", "Lirael Sings the Silver Passage",
                "Help Lirael perform the vocal solo during the Requiem.",
                rsReward: 6f,
                new QuestObjective { description = "Lirael's solo performance", type = QuestObjectiveType.CompanionMilestone, targetId = "lirael_silver_passage", targetCount = 1 }));

            // ─── Moon 7: Resonant Moon — Giant's Awakening ─
            quests.Add(Build("giants_awakening_main", "Giant's Awakening",
                "Free the last giant from stasis beneath the citadel.",
                isMain: true, rsReward: 25f, rsRequirement: 60f,
                new QuestObjective { description = "Thaw stasis chambers", type = QuestObjectiveType.RestoreBuilding, targetCount = 3 },
                new QuestObjective { description = "Defeat Titan Golem", type = QuestObjectiveType.DefeatBoss, targetId = "titan_golem", targetCount = 1 },
                new QuestObjective { description = "Witness Korath's sacrifice", type = QuestObjectiveType.CompanionMilestone, targetId = "korath_sacrifice", targetCount = 1 }));

            quests.Add(Build("korath_thaw", "Korath's Teaching",
                "Learn the mathematics of sacrifice from Korath.",
                rsReward: 8f,
                new QuestObjective { description = "Attend Korath's teachings", type = QuestObjectiveType.TalkToNPC, targetId = "korath", targetCount = 3 }));

            quests.Add(Build("cassian_confrontation", "Cassian's Choice",
                "Confront Cassian about the giant awakening decision.",
                rsReward: 6f,
                new QuestObjective { description = "Cassian confrontation", type = QuestObjectiveType.CompanionMilestone, targetId = "cassian_confrontation", targetCount = 1 }));

            // ─── Moon 8: Galactic Moon — Airship Armada ───
            quests.Add(Build("airship_armada_main", "The Airship Armada",
                "Restore the fleet and establish aerial supply lines.",
                isMain: true, rsReward: 22f, rsRequirement: 65f,
                new QuestObjective { description = "Restore airships", type = QuestObjectiveType.RestoreBuilding, targetCount = 3 },
                new QuestObjective { description = "Complete mercury orb tuning", type = QuestObjectiveType.CompleteTuning, targetCount = 4 },
                new QuestObjective { description = "Defeat Sky Reavers", type = QuestObjectiveType.DefeatEnemies, targetCount = 12 }));

            quests.Add(Build("fleet_restoration", "Fleet Restoration",
                "Rebuild the aerial construction framework.",
                rsReward: 7f,
                new QuestObjective { description = "Collect mercury orbs", type = QuestObjectiveType.CollectItem, targetId = "mercury_orb", targetCount = 6 }));

            quests.Add(Build("thorne_companion", "Thorne: Wings of War",
                "Help Thorne develop aerial combat tactics.",
                rsReward: 6f,
                new QuestObjective { description = "Complete aerial training", type = QuestObjectiveType.CompleteMiniGame, targetId = "aerial_training", targetCount = 1 }));

            // ─── Moon 9: Solar Moon — Ley Line Prophecy ───
            quests.Add(Build("ley_line_prophecy_main", "The Ley Line Prophecy",
                "Activate the 12 prophecy stones and decode Zereth's messages.",
                isMain: true, rsReward: 25f, rsRequirement: 70f,
                new QuestObjective { description = "Activate prophecy stones", type = QuestObjectiveType.CompleteMiniGame, targetId = "prophecy_stone", targetCount = 12 },
                new QuestObjective { description = "Decode Zereth's ciphers", type = QuestObjectiveType.CollectItem, targetId = "zereth_cipher", targetCount = 5 }));

            quests.Add(Build("prophecy_stones", "Stones of Sight",
                "Locate and cleanse the scattered prophecy stones.",
                rsReward: 8f,
                new QuestObjective { description = "Discover prophecy stones", type = QuestObjectiveType.DiscoverBuilding, targetCount = 6 }));

            quests.Add(Build("zereth_whispers", "Zereth's Whispers",
                "Respond to Zereth's voice and uncover his true intent.",
                rsReward: 7f,
                new QuestObjective { description = "Hear Zereth's voice responses", type = QuestObjectiveType.CompanionMilestone, targetId = "zereth_voice", targetCount = 3 }));

            // ─── Moon 10: Planetary Moon — The Living Grid ─
            quests.Add(Build("living_grid_main", "The Living Grid",
                "Connect the continental rail network and power the Trigger Room.",
                isMain: true, rsReward: 25f, rsRequirement: 75f,
                new QuestObjective { description = "Restore rail segments", type = QuestObjectiveType.RestoreBuilding, targetCount = 6 },
                new QuestObjective { description = "Activate Trigger Room", type = QuestObjectiveType.CompleteTuning, targetId = "trigger_room", targetCount = 1 }));

            quests.Add(Build("continental_rail", "Continental Rail",
                "Survey and restore the intercontinental rail links.",
                rsReward: 8f,
                new QuestObjective { description = "Survey rail stations", type = QuestObjectiveType.DiscoverBuilding, targetCount = 4 }));

            quests.Add(Build("trigger_room", "The Trigger Room",
                "Enter the chamber where Zereth's control originates.",
                rsReward: 10f,
                new QuestObjective { description = "Complete Trigger Room puzzle", type = QuestObjectiveType.CompleteMiniGame, targetId = "trigger_room_puzzle", targetCount = 1 },
                new QuestObjective { description = "Confront Zereth projection", type = QuestObjectiveType.DefeatBoss, targetId = "zereth_projection", targetCount = 1 }));

            // ─── Moon 11: Spectral Moon — Veil Between Worlds ─
            quests.Add(Build("veil_main", "Veil Between Worlds",
                "Purify the aquifer and restore the Fountain of Youth.",
                isMain: true, rsReward: 28f, rsRequirement: 80f,
                new QuestObjective { description = "Purify aquifer sections", type = QuestObjectiveType.RestoreBuilding, targetCount = 5 },
                new QuestObjective { description = "Cleanse the fountain", type = QuestObjectiveType.CompleteTuning, targetId = "fountain_of_youth", targetCount = 1 },
                new QuestObjective { description = "Defeat Sludge Leviathan", type = QuestObjectiveType.DefeatBoss, targetId = "sludge_leviathan", targetCount = 1 }));

            quests.Add(Build("aquifer_purification", "Aquifer Purification",
                "Navigate the underground waterways and purge corruption.",
                rsReward: 8f,
                new QuestObjective { description = "Purify water nodes", type = QuestObjectiveType.CompleteTuning, targetCount = 4 }));

            quests.Add(Build("fountain_chain", "Fountain Chain",
                "Link the purified springs to the central fountain.",
                rsReward: 7f,
                new QuestObjective { description = "Restore spring connections", type = QuestObjectiveType.RestoreBuilding, targetCount = 3 }));

            // ─── Moon 12: Crystal Moon — Bell-Tower Ring ───
            quests.Add(Build("bell_tower_ring_main", "Planetary Bell-Tower Ring",
                "Synchronize all 12 bell towers for the planetary resonance cascade.",
                isMain: true, rsReward: 30f, rsRequirement: 85f,
                new QuestObjective { description = "Synchronize bell towers", type = QuestObjectiveType.CompleteMiniGame, targetId = "bell_tower_sync", targetCount = 12 },
                new QuestObjective { description = "Complete synchronization mini-game", type = QuestObjectiveType.CompleteMiniGame, targetId = "bell_tower_sync_game", targetCount = 1 }));

            quests.Add(Build("tower_synchronization", "Tower Synchronization",
                "Tune individual bell towers to earth's Schumann resonance.",
                rsReward: 9f,
                new QuestObjective { description = "Tune towers", type = QuestObjectiveType.CompleteTuning, targetCount = 6 }));

            quests.Add(Build("final_prophecy_stone", "The Final Prophecy Stone",
                "Place the last prophecy stone and trigger Veritas's bell-tower revelation.",
                rsReward: 10f,
                new QuestObjective { description = "Place final stone", type = QuestObjectiveType.CompanionMilestone, targetId = "veritas_bell_tower", targetCount = 1 }));

            // ─── Moon 13: Cosmic Moon — True Timeline ─────
            quests.Add(Build("convergence_final", "True Timeline Convergence",
                "Unite all companions and restore the true timeline.",
                isMain: true, rsReward: 50f, rsRequirement: 90f,
                new QuestObjective { description = "Gather all 7 companions", type = QuestObjectiveType.CompanionMilestone, targetId = "all_companions", targetCount = 7 },
                new QuestObjective { description = "Defeat the Echo of Reset", type = QuestObjectiveType.DefeatBoss, targetId = "echo_of_reset", targetCount = 1 },
                new QuestObjective { description = "Activate the True Timeline", type = QuestObjectiveType.CompleteTuning, targetId = "true_timeline", targetCount = 1 }));

            quests.Add(Build("anastasia_finale", "Anastasia's Final Manifestation",
                "Help Princess Anastasia fully manifest and deliver her message.",
                rsReward: 30f,
                new QuestObjective { description = "Reach 100% Anastasia solidity", type = QuestObjectiveType.CompanionMilestone, targetId = "anastasia_manifest", targetCount = 1 }));

            quests.Add(Build("echo_realm_travel", "Echo Realm Passage",
                "Travel through the echo realm to the Day Out of Time.",
                rsReward: 20f,
                new QuestObjective { description = "Navigate echo realm", type = QuestObjectiveType.CompleteZone, targetId = "echo_realm", targetCount = 1 }));

            return quests.ToArray();
        }

        // ─── Builder Helpers ─────────────────────────

        static QuestDefinition Build(
            string id, string name, string desc,
            bool isMain = false, bool autoActivate = false,
            float rsReward = 0f, float rsRequirement = 0f,
            string[] followUp = null,
            params QuestObjective[] objectives)
        {
            var def = ScriptableObject.CreateInstance<QuestDefinition>();
            def.questId = id;
            def.displayName = name;
            def.description = desc;
            def.isMainQuest = isMain;
            def.autoActivate = autoActivate;
            def.rsReward = rsReward;
            def.rsRequirement = rsRequirement;
            def.objectives = objectives;
            def.followUpQuestIds = followUp;
            def.name = id;
            return def;
        }

        static QuestDefinition Build(
            string id, string name, string desc,
            float rsReward,
            params QuestObjective[] objectives)
        {
            return Build(id, name, desc,
                isMain: false, autoActivate: false,
                rsReward: rsReward, rsRequirement: 0f,
                followUp: null, objectives);
        }
    }
}
