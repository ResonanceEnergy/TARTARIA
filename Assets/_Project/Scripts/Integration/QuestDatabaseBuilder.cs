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

            // Moon 3 R7 escort physical + giant song + Veritas intro synergy
            quests.Add(Build("r7_m3_escort_giant_song", "Orphan Train Giant Song Match",
                "Complete rail escort with high bond (Korath/Veritas) — Giant's Song auto-match + Companion Giant payoff.",
                rsReward: 220f,
                new QuestObjective { description = "Escort physical tells + giant synergy + Veritas precision", type = QuestObjectiveType.CompanionMilestone, targetId = "korath", targetCount = 1 }));

            quests.Add(Build("r7_m3_veritas_calendar_claim", "Veritas Bell Echo Claimable",
                "Claim Veritas 17th Hour calendar event after first train escort — permanent resonance mutation.",
                rsReward: 95f,
                new QuestObjective { description = "Veritas calendar claimable + world mutation tier", type = QuestObjectiveType.CompanionMilestone, targetId = "veritas", targetCount = 1 }));

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

            // ... (hooks for 4-13: similar giant, calendar, mutation quests wired in full build)

            return quests.ToArray();
        }

        static QuestDefinition Build(string id, string name, string desc, float rsReward, params QuestObjective[] objectives)
        {
            return Build(id, name, desc, false, false, rsReward, 0f, null, objectives);
        }

        // ... existing Build overloads unchanged ...
    }
}