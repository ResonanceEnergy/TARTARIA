using UnityEditor;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Data;

namespace Tartaria.Editor
{
    /// <summary>
    /// Factory that generates QuestData and ObjectiveData ScriptableObject assets.
    /// Creates example quests for Moon 1-3 with full prerequisite chains.
    /// Menu: Tartaria > Build Assets > Quest Database Assets
    /// </summary>
    public static class QuestDataFactory
    {
        const string QuestsPath = "Assets/_Project/Config/Quests";
        const string ObjectivesPath = "Assets/_Project/Config/Quests/Objectives";

        [MenuItem("Tartaria/Build Assets/Quest Database Assets")]
        public static void BuildExampleQuests()
        {
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Config"))
                AssetDatabase.CreateFolder("Assets/_Project", "Config");
            if (!AssetDatabase.IsValidFolder(QuestsPath))
                AssetDatabase.CreateFolder("Assets/_Project/Config", "Quests");
            if (!AssetDatabase.IsValidFolder(ObjectivesPath))
                AssetDatabase.CreateFolder(QuestsPath, "Objectives");

            int created = 0;
            created += CreateMoon1Quests();
            created += CreateMoon2Quests();
            created += CreateMoon3Quests();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[QuestDataFactory] Created {created} example quest assets in {QuestsPath}");
        }

        [MenuItem("Tartaria/Build Assets/Create Quest Database")]
        public static void CreateQuestDatabase()
        {
            string dbPath = "Assets/_Project/Config/MasterQuestDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<QuestDatabase>(dbPath);

            if (db == null)
            {
                db = ScriptableObject.CreateInstance<QuestDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
                Debug.Log($"[QuestDataFactory] Created QuestDatabase at {dbPath}");
            }
            else
            {
                Debug.Log($"[QuestDataFactory] QuestDatabase already exists at {dbPath}");
            }

            // Collect all QuestData assets
            var allQuests = LoadAllQuestDataAssets();
            var field = typeof(QuestDatabase).GetField("allQuests", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(db, allQuests);
                EditorUtility.SetDirty(db);
                Debug.Log($"[QuestDataFactory] Populated database with {allQuests.Length} quests");
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = db;
        }

        static QuestData[] LoadAllQuestDataAssets()
        {
            var guids = AssetDatabase.FindAssets("t:QuestData", new[] { QuestsPath });
            var quests = new QuestData[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                quests[i] = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            }
            return quests;
        }

        // ─── MOON 1 QUESTS ───────────────────────────

        static int CreateMoon1Quests()
        {
            int count = 0;

            // Quest 1: Tutorial awakening (no prerequisites)
            var awakening = CreateQuest("echohaven_awakening", "Echohaven Awakening", 1, QuestCategory.Tutorial,
                "Meet Milo, discover your first buried structure, and complete your first restoration to awaken the Aether field.",
                rsReward: 50f, xpReward: 100);
            awakening.autoActivate = true;
            awakening.isMainQuest = true;

            var obj1 = CreateObjective("meet_milo", "Meet Milo the companion", QuestObjectiveType.CompanionMilestone, "milo", 1);
            var obj2 = CreateObjective("discover_building", "Discover a Tartarian building", QuestObjectiveType.DiscoverBuilding, "", 1);
            var obj3 = CreateObjective("restore_first", "Complete first building restoration", QuestObjectiveType.RestoreBuilding, "", 1);

            awakening.objectiveData = new[] { obj1, obj2, obj3 };
            SaveAsset(awakening, "Quest_echohaven_awakening");
            count++;

            // Quest 2: Exploration quest (prerequisite: awakening)
            var exploration = CreateQuest("echohaven_exploration", "City of Echoes", 1, QuestCategory.Main,
                "Explore the ruins of Echohaven and discover 3 more buried structures.",
                rsReward: 80f, xpReward: 150);
            exploration.prerequisiteQuestIds = new[] { "echohaven_awakening" };
            exploration.isMainQuest = true;

            var obj4 = CreateObjective("discover_3_buildings", "Discover 3 additional buildings", QuestObjectiveType.DiscoverBuilding, "", 3);
            exploration.objectiveData = new[] { obj4 };
            SaveAsset(exploration, "Quest_echohaven_exploration");
            count++;

            // Quest 3: Combat side quest (unlocks at 25 RS)
            var combat = CreateQuest("golem_graveyard", "Golem Graveyard", 1, QuestCategory.Combat,
                "Clear the corrupted golems defending the eastern quarter. Defeat 5 Mud Golems.",
                rsReward: 120f, xpReward: 200, prerequisiteRS: 25f);
            combat.isMainQuest = false;

            var obj5 = CreateObjective("defeat_golems", "Defeat 5 Mud Golems", QuestObjectiveType.DefeatEnemies, "mud_golem", 5);
            combat.objectiveData = new[] { obj5 };
            SaveAsset(combat, "Quest_golem_graveyard");
            count++;

            // Quest 4: Companion quest (prerequisite: awakening, level 2)
            var companion = CreateQuest("milos_frequency", "Milo's Frequency", 1, QuestCategory.Companion,
                "Help Milo tune his resonance crystal to unlock deeper companion abilities.",
                rsReward: 60f, xpReward: 100, prerequisiteLevel: 2);
            companion.prerequisiteQuestIds = new[] { "echohaven_awakening" };
            companion.itemRewards = new[] { "resonance_crystal" };
            companion.isMainQuest = false;

            var obj6 = CreateObjective("companion_trust", "Reach 25 trust with Milo", QuestObjectiveType.CompanionMilestone, "milo", 25);
            companion.objectiveData = new[] { obj6 };
            SaveAsset(companion, "Quest_milos_frequency");
            count++;

            return count;
        }

        // ─── MOON 2 QUESTS ───────────────────────────

        static int CreateMoon2Quests()
        {
            int count = 0;

            // Quest 1: Main story quest (prerequisite: Moon 1 complete)
            var lunar = CreateQuest("lunar_challenge", "The Lunar Challenge", 2, QuestCategory.Main,
                "Shadow & Purge. Discover the corrupted cathedral, purge dissonance crystals, and complete the 5-beat FTUE.",
                rsReward: 520f, xpReward: 500);
            lunar.prerequisiteQuestIds = new[] { "echohaven_exploration" };
            lunar.prerequisiteRS = 100f;
            lunar.isMainQuest = true;

            var obj1 = CreateObjective("discover_cathedral", "Witness Lirael fracture + Cassian scholar beckon", QuestObjectiveType.DiscoverBuilding, "moon2_discovery", 1);
            var obj2 = CreateObjective("tune_crystal", "Complete micro-giant crystal tuning", QuestObjectiveType.RestoreBuilding, "micro_giant_tune", 1);
            var obj3 = CreateObjective("defeat_golem", "Defeat first Mud Golem + Cassian trust/doubt tick", QuestObjectiveType.CompanionMilestone, "mud_golem_first", 1);
            var obj4 = CreateObjective("purify_fountain", "Purify ionized fountain + storm dome cleanse", QuestObjectiveType.RestoreBuilding, "moon2_fountain", 1);
            var obj5 = CreateObjective("revelation", "Cassian diary choice + The Crystal Remembers experience", QuestObjectiveType.CompanionMilestone, "crystal_remembers", 1);

            lunar.objectiveData = new[] { obj1, obj2, obj3, obj4, obj5 };
            SaveAsset(lunar, "Quest_lunar_challenge");
            count++;

            // Quest 2: Lirael companion quest
            var lirael = CreateQuest("lirael_crystal_choir", "Lirael's Fractured Crystal Choir", 2, QuestCategory.Companion,
                "Accompany Lirael to 3 corrupted crystal nodes deep in the cathedral. Tune while she sings pre-corruption memories.",
                rsReward: 180f, xpReward: 250);
            lirael.prerequisiteQuestIds = new[] { "lunar_challenge" };

            var obj6 = CreateObjective("purge_nodes", "Lirael crystal node purges (3) + physical tell", QuestObjectiveType.CompanionMilestone, "lirael", 3);
            lirael.objectiveData = new[] { obj6 };
            SaveAsset(lirael, "Quest_lirael_crystal_choir");
            count++;

            return count;
        }

        // ─── MOON 3 QUESTS ───────────────────────────

        static int CreateMoon3Quests()
        {
            int count = 0;

            // Quest 1: Main orphan train escort
            var escort = CreateQuest("orphan_train_escort", "Compassion & Rails: The Orphan Train Escort", 3, QuestCategory.Main,
                "Discover the Dissonant Orphan Train. Build trust with spectral orphans, escort through corruption, defeat the Dissonance Leviathan.",
                rsReward: 380f, xpReward: 600);
            escort.prerequisiteQuestIds = new[] { "lunar_challenge" };
            escort.prerequisiteRS = 200f;
            escort.isMainQuest = true;
            escort.unlockRewards = new[] { "continental_rail_network", "worlds_fair_access" };

            var obj1 = CreateObjective("discover_train", "Discover the spectral orphan train and rail stations", QuestObjectiveType.DiscoverBuilding, "orphan_train", 1);
            var obj2 = CreateObjective("adopt_orphans", "Adopt spectral orphans via trust-building (3)", QuestObjectiveType.CompanionMilestone, "spectral_orphans", 3);
            var obj3 = CreateObjective("escort_complete", "Complete the full rail escort climax", QuestObjectiveType.CompleteTuning, "rail_escort", 1);
            var obj4 = CreateObjective("defeat_leviathan", "Defeat Dissonance Leviathan with orphan lullaby", QuestObjectiveType.HiddenDiscovery, "leviathan_moon3", 1);

            escort.objectiveData = new[] { obj1, obj2, obj3, obj4 };
            SaveAsset(escort, "Quest_orphan_train_escort");
            count++;

            // Quest 2: Korath giant synergy quest
            var korath = CreateQuest("escort_giant_song", "Orphan Train Giant Song Match", 3, QuestCategory.Companion,
                "Complete rail escort with high bond (Korath/Veritas) — Giant's Song auto-match + Companion Giant payoff.",
                rsReward: 220f, xpReward: 300);
            korath.prerequisiteQuestIds = new[] { "orphan_train_escort" };

            var obj5 = CreateObjective("giant_synergy", "Escort physical tells + giant synergy + Veritas precision", QuestObjectiveType.CompanionMilestone, "korath", 1);
            korath.objectiveData = new[] { obj5 };
            SaveAsset(korath, "Quest_escort_giant_song");
            count++;

            return count;
        }

        // ─── HELPERS ─────────────────────────────────

        static QuestData CreateQuest(string id, string name, int moonId, QuestCategory category, string description,
            float rsReward = 0f, int xpReward = 0, float prerequisiteRS = 0f, int prerequisiteLevel = 0)
        {
            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.questId = id;
            quest.displayName = name;
            quest.description = description;
            quest.moonId = moonId;
            quest.category = category;
            quest.rsReward = rsReward;
            quest.xpReward = xpReward;
            quest.prerequisiteRS = prerequisiteRS;
            quest.prerequisiteLevel = prerequisiteLevel;
            return quest;
        }

        static ObjectiveData CreateObjective(string id, string desc, QuestObjectiveType type, string targetId, int count)
        {
            var obj = ScriptableObject.CreateInstance<ObjectiveData>();
            obj.objectiveId = id;
            obj.description = desc;
            obj.targetType = type;
            obj.targetId = targetId;
            obj.targetCount = count;
            return obj;
        }

        static void SaveAsset(Object asset, string fileName)
        {
            string path = $"{QuestsPath}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath(path, asset.GetType());

            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(asset);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }
    }
}
