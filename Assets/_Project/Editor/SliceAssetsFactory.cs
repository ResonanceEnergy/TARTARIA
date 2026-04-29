using System.IO;
using Tartaria.Core;
using Tartaria.Integration;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Materializes the two ScriptableObject assets the vertical slice needs:
    /// the Awaken Star Dome quest definition and the Anastasia 4-line dialogue
    /// for that quest. Idempotent — safe to re-run every build.
    /// Per docs/33_VERTICAL_SLICE_SCRIPT.md.
    /// </summary>
    public static class SliceAssetsFactory
    {
        const string QuestDir = "Assets/_Project/Config/Quests";
        const string DialogueDir = "Assets/_Project/Config/Dialogue";
        const string QuestPath = QuestDir + "/Quest_AwakenStarDome.asset";
        const string DialoguePath = DialogueDir + "/Dialogue_Anastasia_AwakenStarDome.asset";

        [MenuItem("TARTARIA/Slice/Ensure Slice Assets")]
        public static void EnsureSliceAssetsMenu() => EnsureSliceAssets();

        public static void EnsureSliceAssets()
        {
            EnsureDir(QuestDir);
            EnsureDir(DialogueDir);
            EnsureQuest();
            EnsureDialogue();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(QuestPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(DialoguePath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();
        }

        static void EnsureQuest()
        {
            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestDefinition>();
                AssetDatabase.CreateAsset(quest, QuestPath);
                Debug.Log($"[SliceAssets] Created {QuestPath}");
            }

            quest.questId = EndCardController.TriggerQuestId;
            quest.displayName = "Awaken the Star Dome";
            quest.description = "Approach the Star Dome and channel the Resonance Tool to restore its light.";
            quest.isMainQuest = true;
            quest.autoActivate = false;
            quest.rsRequirement = 0f;
            quest.rsReward = 25f;
            quest.followUpQuestIds = new[] { "awaken_fountain" };

            quest.objectives = new[]
            {
                new QuestObjective
                {
                    description = "Discover the Star Dome",
                    type = QuestObjectiveType.DiscoverBuilding,
                    targetId = "Echohaven_StarDome",
                    targetCount = 1,
                },
                new QuestObjective
                {
                    description = "Restore the Star Dome",
                    type = QuestObjectiveType.RestoreBuilding,
                    targetId = "Echohaven_StarDome",
                    targetCount = 1,
                },
            };

            EditorUtility.SetDirty(quest);
        }

        static void EnsureDialogue()
        {
            var db = AssetDatabase.LoadAssetAtPath<AnastasiaDialogueDatabase>(DialoguePath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<AnastasiaDialogueDatabase>();
                AssetDatabase.CreateAsset(db, DialoguePath);
                Debug.Log($"[SliceAssets] Created {DialoguePath}");
            }

            // Multi-type .cs file workaround: Unity sometimes writes m_Script: {fileID: 0}
            // when the SO type lives alongside other classes (AnastasiaController.cs).
            // Force the MonoScript reference via SerializedObject.
            ForceScriptReference(db);

            db.lines = new[]
            {
                new AnastasiaLine
                {
                    id = 1,
                    moon = 1,
                    category = AnastasiaLineCategory.StoryBeat,
                    triggerContext = "awaken_star_dome_intro",
                    text = "Elara. The dome remembers your voice. It only waits to be asked.",
                },
                new AnastasiaLine
                {
                    id = 2,
                    moon = 1,
                    category = AnastasiaLineCategory.LoreWhisper,
                    triggerContext = "awaken_star_dome_lore",
                    text = "This was the first lens of the choir. Twelve more sleep beyond the horizon.",
                },
                new AnastasiaLine
                {
                    id = 3,
                    moon = 1,
                    category = AnastasiaLineCategory.CompanionReaction,
                    triggerContext = "awaken_star_dome_prompt",
                    text = "Hold the Resonance. Let it find the note the stone has forgotten.",
                },
                new AnastasiaLine
                {
                    id = 4,
                    moon = 1,
                    category = AnastasiaLineCategory.StoryBeat,
                    triggerContext = "awaken_star_dome_task",
                    text = "Restore the Dome. Channel the resonance. Echohaven is listening.",
                },
            };

            EditorUtility.SetDirty(db);
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        static void ForceScriptReference(ScriptableObject so)
        {
            var script = MonoScript.FromScriptableObject(so);
            if (script == null)
            {
                Debug.LogWarning($"[SliceAssets] No MonoScript for {so.GetType().FullName} - dialogue may not bind correctly.");
                return;
            }
            var serialized = new SerializedObject(so);
            var prop = serialized.FindProperty("m_Script");
            if (prop != null)
            {
                prop.objectReferenceValue = script;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
