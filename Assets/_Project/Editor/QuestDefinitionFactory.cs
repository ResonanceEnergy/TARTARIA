using UnityEditor;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Factory that generates QuestDefinition ScriptableObject assets for all 13 Moons.
    /// Menu: Tartaria > Build Assets > Quest Definitions
    /// </summary>
    public static class QuestDefinitionFactory
    {
        const string BasePath = "Assets/_Project/Config/Quests";

        [MenuItem("Tartaria/Build Assets/Quest Definitions")]
        public static void BuildAllQuests()
        {
            if (!AssetDatabase.IsValidFolder(BasePath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Config", "Quests");
            }

            // ── Moon 1: Echohaven ──────────────────────
            CreateQuest("echohaven_main", "Echoes of the Buried City",
                "Discover and restore all three Echohaven structures to awaken the zone's Resonance network.",
                isMain: true, autoActivate: true, rsReq: 0f, rsReward: 25f,
                objectives: new[]
                {
                    Obj("Discover the Star Dome", QuestObjectiveType.DiscoverBuilding, "echohaven_dome_01", 1),
                    Obj("Discover the Harmonic Fountain", QuestObjectiveType.DiscoverBuilding, "echohaven_fountain_01", 1),
                    Obj("Discover the Crystal Spire", QuestObjectiveType.DiscoverBuilding, "echohaven_spire_01", 1),
                    Obj("Restore the Star Dome", QuestObjectiveType.RestoreBuilding, "echohaven_dome_01", 1),
                    Obj("Restore the Harmonic Fountain", QuestObjectiveType.RestoreBuilding, "echohaven_fountain_01", 1),
                    Obj("Restore the Crystal Spire", QuestObjectiveType.RestoreBuilding, "echohaven_spire_01", 1),
                },
                followUps: new[] { "echohaven_milo", "echohaven_golems", "crystalline_main" });

            CreateQuest("echohaven_milo", "Milo's Frequency",
                "Follow Milo's instincts to find hidden frequency caches scattered across Echohaven.",
                isMain: false, autoActivate: false, rsReq: 10f, rsReward: 10f,
                objectives: new[]
                {
                    Obj("Talk to Milo at the Star Dome", QuestObjectiveType.TalkToNPC, "milo", 1),
                    Obj("Collect hidden frequency caches", QuestObjectiveType.CollectItem, "freq_cache", 3),
                    Obj("Return to Milo", QuestObjectiveType.TalkToNPC, "milo", 1),
                },
                followUps: null);

            CreateQuest("echohaven_golems", "Golem Graveyard",
                "Defeat the Mud Golems drawn to Echohaven's rising Resonance before they corrupt the restored buildings.",
                isMain: false, autoActivate: false, rsReq: 25f, rsReward: 15f,
                objectives: new[]
                {
                    Obj("Defeat Mud Golems", QuestObjectiveType.DefeatEnemies, "mud_golem", 5),
                    Obj("Reach RS 50 to stabilize the zone", QuestObjectiveType.ReachRS, null, 50),
                },
                followUps: null);

            CreateQuest("echohaven_anastasia", "The Ghost in the Rubble",
                "Anastasia has manifested for the first time. Observe her reactions near each restored building.",
                isMain: false, autoActivate: false, rsReq: 15f, rsReward: 8f,
                objectives: new[]
                {
                    Obj("Witness Anastasia's first manifestation", QuestObjectiveType.TalkToNPC, "anastasia", 1),
                    Obj("Visit restored buildings with Anastasia following", QuestObjectiveType.DiscoverBuilding, null, 3),
                },
                followUps: null);

            // ── Moon 2: Crystalline Caverns ──────────────
            CreateQuest("crystalline_main", "The First Antenna",
                "Navigate the Crystalline Caverns and restore the ancient signal amplifier hidden within.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 30f,
                objectives: new[]
                {
                    Obj("Enter the Crystalline Caverns", QuestObjectiveType.CompleteZone, "crystalline_caverns", 1),
                    Obj("Discover the Signal Amplifier", QuestObjectiveType.DiscoverBuilding, "crystalline_amplifier_01", 1),
                    Obj("Restore cavern structures", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Complete the Antenna Tuning", QuestObjectiveType.CompleteTuning, "crystalline_antenna", 1),
                },
                followUps: new[] { "highlands_main" });

            CreateQuest("crystalline_lirael", "Lirael's Echo",
                "Lirael senses something familiar in the crystal formations. Help her investigate.",
                isMain: false, autoActivate: false, rsReq: 120f, rsReward: 12f,
                objectives: new[]
                {
                    Obj("Speak with Lirael at the cavern entrance", QuestObjectiveType.TalkToNPC, "lirael", 1),
                    Obj("Find crystal memory fragments", QuestObjectiveType.CollectItem, "crystal_memory", 4),
                    Obj("Return to Lirael with the fragments", QuestObjectiveType.TalkToNPC, "lirael", 1),
                },
                followUps: null);

            // ── Moon 3: Windswept Highlands ──────────────
            CreateQuest("highlands_main", "The Orphan Route",
                "Cross the Windswept Highlands following the trail of displaced Tartarian refugees.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 35f,
                objectives: new[]
                {
                    Obj("Enter the Windswept Highlands", QuestObjectiveType.CompleteZone, "windswept_highlands", 1),
                    Obj("Discover highland structures", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore highland structures", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Defeat Highland Sentinels", QuestObjectiveType.DefeatEnemies, "highland_sentinel", 4),
                },
                followUps: new[] { "starfort_main" });

            // ── Moon 4: Star Fort Bastion ────────────────
            CreateQuest("starfort_main", "The Royal Amplifier",
                "Breach the Star Fort Bastion and reactivate the royal Resonance amplifier at its heart.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 40f,
                objectives: new[]
                {
                    Obj("Enter the Star Fort Bastion", QuestObjectiveType.CompleteZone, "starfort_bastion", 1),
                    Obj("Discover all 4 bastion structures", QuestObjectiveType.DiscoverBuilding, null, 4),
                    Obj("Restore all 4 bastion structures", QuestObjectiveType.RestoreBuilding, null, 4),
                    Obj("Activate the Royal Amplifier", QuestObjectiveType.CompleteTuning, "royal_amplifier", 1),
                },
                followUps: new[] { "colosseum_main" });

            // ── Moon 5: Sunken Colosseum ─────────────────
            CreateQuest("colosseum_main", "The Resonance Stage",
                "Descend into the Sunken Colosseum and prove mastery of combat and tuning in the arena.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 40f,
                objectives: new[]
                {
                    Obj("Enter the Sunken Colosseum", QuestObjectiveType.CompleteZone, "sunken_colosseum", 1),
                    Obj("Complete arena challenges", QuestObjectiveType.DefeatEnemies, "arena_champion", 3),
                    Obj("Restore colosseum structures", QuestObjectiveType.RestoreBuilding, null, 3),
                },
                followUps: new[] { "library_main" });

            // ── Moon 6: Living Library ───────────────────
            CreateQuest("library_main", "The Archive Interface",
                "Unlock the Living Library and decode the Tartarian knowledge preserved within its walls.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 45f,
                objectives: new[]
                {
                    Obj("Enter the Living Library", QuestObjectiveType.CompleteZone, "living_library", 1),
                    Obj("Discover all 4 library archives", QuestObjectiveType.DiscoverBuilding, null, 4),
                    Obj("Restore all 4 library archives", QuestObjectiveType.RestoreBuilding, null, 4),
                    Obj("Complete the Knowledge Tuning", QuestObjectiveType.CompleteTuning, "archive_interface", 1),
                },
                followUps: new[] { "clockwork_main" });

            // ── Moon 7: Clockwork Citadel ────────────────
            CreateQuest("clockwork_main", "Mechanical Prophecy",
                "Navigate the Clockwork Citadel's shifting mechanisms to reveal the prophecy within.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 45f,
                objectives: new[]
                {
                    Obj("Enter the Clockwork Citadel", QuestObjectiveType.CompleteZone, "clockwork_citadel", 1),
                    Obj("Discover citadel mechanisms", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore citadel mechanisms", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Defeat the Clockwork Guardian", QuestObjectiveType.DefeatEnemies, "clockwork_guardian", 1),
                },
                followUps: new[] { "canopy_main" });

            // ── Moon 8: Verdant Canopy ───────────────────
            CreateQuest("canopy_main", "Where Life Refuses to Yield",
                "Ascend through the Verdant Canopy where nature and Tartarian tech have merged into something new.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 50f,
                objectives: new[]
                {
                    Obj("Enter the Verdant Canopy", QuestObjectiveType.CompleteZone, "verdant_canopy", 1),
                    Obj("Discover bio-structures", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore bio-structures", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Collect golden motes in the canopy", QuestObjectiveType.CollectItem, "golden_mote", 3),
                },
                followUps: new[] { "spire_main" });

            // ── Moon 9: Auroral Spire ────────────────────
            CreateQuest("spire_main", "Where Light Sings",
                "Climb the Auroral Spire and attune to the frequencies of pure light.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 50f,
                objectives: new[]
                {
                    Obj("Enter the Auroral Spire", QuestObjectiveType.CompleteZone, "auroral_spire", 1),
                    Obj("Discover spire chambers", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore spire chambers", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Complete the Light Tuning", QuestObjectiveType.CompleteTuning, "auroral_tuning", 1),
                },
                followUps: new[] { "forge_main" });

            // ── Moon 10: Deep Forge ──────────────────────
            CreateQuest("forge_main", "The Universe's Bass Note",
                "Descend into the Deep Forge where the lowest resonance frequencies shape reality itself.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 55f,
                objectives: new[]
                {
                    Obj("Enter the Deep Forge", QuestObjectiveType.CompleteZone, "deep_forge", 1),
                    Obj("Discover forge structures", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore forge structures", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Defeat the Forge Titan", QuestObjectiveType.DefeatEnemies, "forge_titan", 1),
                },
                followUps: new[] { "tidal_main" });

            // ── Moon 11: Tidal Archive ───────────────────
            CreateQuest("tidal_main", "Memory in Water",
                "Explore the Tidal Archive where Tartarian memories are preserved in flowing water.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 55f,
                objectives: new[]
                {
                    Obj("Enter the Tidal Archive", QuestObjectiveType.CompleteZone, "tidal_archive", 1),
                    Obj("Discover tidal chambers", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore tidal chambers", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Collect water memories", QuestObjectiveType.CollectItem, "water_memory", 5),
                },
                followUps: new[] { "observatory_main" });

            // ── Moon 12: Celestial Observatory ───────────
            CreateQuest("observatory_main", "Geometry at Sufficient Distance",
                "Reach the Celestial Observatory and map the star patterns that guided the ancients.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 60f,
                objectives: new[]
                {
                    Obj("Enter the Celestial Observatory", QuestObjectiveType.CompleteZone, "celestial_observatory", 1),
                    Obj("Discover observatory instruments", QuestObjectiveType.DiscoverBuilding, null, 3),
                    Obj("Restore observatory instruments", QuestObjectiveType.RestoreBuilding, null, 3),
                    Obj("Complete the Star Tuning", QuestObjectiveType.CompleteTuning, "star_tuning", 1),
                },
                followUps: new[] { "nexus_main" });

            // ── Moon 13: Planetary Nexus ─────────────────
            CreateQuest("nexus_main", "All Lines Converge",
                "Enter the Planetary Nexus, the heart of Tartaria, and restore the world's Resonance network.",
                isMain: true, autoActivate: false, rsReq: 0f, rsReward: 100f,
                objectives: new[]
                {
                    Obj("Enter the Planetary Nexus", QuestObjectiveType.CompleteZone, "planetary_nexus", 1),
                    Obj("Discover all 5 Nexus cores", QuestObjectiveType.DiscoverBuilding, null, 5),
                    Obj("Restore all 5 Nexus cores", QuestObjectiveType.RestoreBuilding, null, 5),
                    Obj("Complete the Final Tuning", QuestObjectiveType.CompleteTuning, "nexus_tuning", 1),
                    Obj("Trigger Anastasia's Solidification", QuestObjectiveType.TalkToNPC, "anastasia", 1),
                },
                followUps: null);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QuestDefinitionFactory] All quest definitions created.");
        }

        static void CreateQuest(string questId, string displayName, string description,
            bool isMain, bool autoActivate, float rsReq, float rsReward,
            QuestObjective[] objectives, string[] followUps)
        {
            string path = $"{BasePath}/Quest_{questId}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
            if (existing != null)
            {
                EditorUtility.SetDirty(existing);
                // Update existing
                existing.questId = questId;
                existing.displayName = displayName;
                existing.description = description;
                existing.isMainQuest = isMain;
                existing.autoActivate = autoActivate;
                existing.rsRequirement = rsReq;
                existing.rsReward = rsReward;
                existing.objectives = objectives;
                existing.followUpQuestIds = followUps;
                return;
            }

            var quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = questId;
            quest.displayName = displayName;
            quest.description = description;
            quest.isMainQuest = isMain;
            quest.autoActivate = autoActivate;
            quest.rsRequirement = rsReq;
            quest.rsReward = rsReward;
            quest.objectives = objectives;
            quest.followUpQuestIds = followUps;

            AssetDatabase.CreateAsset(quest, path);
        }

        static QuestObjective Obj(string desc, QuestObjectiveType type, string targetId, int count)
        {
            return new QuestObjective
            {
                description = desc,
                type = type,
                targetId = targetId ?? "",
                targetCount = count
            };
        }
    }
}
