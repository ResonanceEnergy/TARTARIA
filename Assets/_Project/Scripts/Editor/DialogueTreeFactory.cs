using UnityEngine;
using Tartaria.Data;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Tartaria.Editor.Dialogue
{
    /// <summary>
    /// DialogueTreeFactory — utility for creating example dialogue trees in the editor.
    /// Demonstrates branching conversation structure with conditions and choices.
    ///
    /// Usage (Editor only):
    /// 1. Menu: Tools > Tartaria > Create Example Dialogue Trees
    /// 2. Trees are created in Assets/Resources/Dialogue/
    /// 3. Nodes are created as sub-assets within the tree
    ///
    /// Example Trees:
    /// - Anastasia_Intro: First meeting, introduces phasing mechanic
    /// - Cassian_Moon2: Trust-building conversation with choice consequences
    /// </summary>
    public static class DialogueTreeFactory
    {
#if UNITY_EDITOR
        const string DIALOGUE_PATH = "Assets/Resources/Dialogue";

        [MenuItem("Tools/Tartaria/Create Example Dialogue Trees")]
        public static void CreateExampleTrees()
        {
            // Ensure dialogue directory exists
            if (!Directory.Exists(DIALOGUE_PATH))
            {
                Directory.CreateDirectory(DIALOGUE_PATH);
                AssetDatabase.Refresh();
            }

            CreateAnastasiaIntroTree();
            CreateCassianMoon2Tree();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DialogueTreeFactory] Created example dialogue trees in Resources/Dialogue/");
        }

        static void CreateAnastasiaIntroTree()
        {
            var tree = ScriptableObject.CreateInstance<DialogueTreeAsset>();
            tree.treeId = "Anastasia_Intro";
            tree.description = "First conversation with Anastasia when player encounters her phasing form in Moon 1 Echohaven.";
            tree.rootNodeId = "ana_intro_01";
            tree.primarySpeaker = "Anastasia";
            tree.tags = new[] { "moon_1", "anastasia", "main_quest", "first_meeting" };
            tree.oneTimeOnly = true;

            // Node 1: Anastasia appears
            var node1 = CreateNode(
                "ana_intro_01",
                "Anastasia",
                "*A shimmering figure materializes near the restored bell tower*\n\nOh. Someone can see me? That hasn't happened in... how long has it been?",
                false,
                "ana_intro_voice_01"
            );
            node1.autoAdvanceToNode = "ana_intro_02";
            node1.autoAdvanceDelay = 3f;

            // Node 2: Anastasia explains
            var node2 = CreateNode(
                "ana_intro_02",
                "Anastasia",
                "I'm Anastasia. I was... alive, once. During the World's Fair. Then the demolition came, and something went wrong. I became... this. Neither here nor gone.",
                false,
                "ana_intro_voice_02"
            );

            var choices2 = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "What happened to you?",
                    nextNodeId = "ana_intro_03_sympathy",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "Can you help me restore these buildings?",
                    nextNodeId = "ana_intro_03_pragmatic",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "[Say nothing, just watch]",
                    nextNodeId = "ana_intro_03_silent",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                }
            };
            node2.choices = choices2;

            // Branch A: Sympathy path
            var node3a = CreateNode(
                "ana_intro_03_sympathy",
                "Anastasia",
                "*flickers slightly* I... I don't know. One moment I was running from the collapse. The next, I was... frequency. Pure vibration. Trapped in the Aether field with nowhere to go.\n\nBut you restored that bell tower. The resonance is giving me form again. Maybe... maybe there's hope.",
                false,
                "ana_intro_voice_03a"
            );
            node3a.relationshipDelta = 5; // +5 relationship with Anastasia
            node3a.autoAdvanceToNode = "ana_intro_04_join";
            node3a.autoAdvanceDelay = 4f;

            // Branch B: Pragmatic path
            var node3b = CreateNode(
                "ana_intro_03_pragmatic",
                "Anastasia",
                "*tilts head, studying you*\n\nPractical. I can respect that. Yes, I remember the resonance frequencies for every major structure in Echohaven. The blueprints are... part of me now.\n\nI'll help you. In exchange, keep restoring. Every building you bring back makes me more real.",
                false,
                "ana_intro_voice_03b"
            );
            node3b.relationshipDelta = 2; // +2 relationship (neutral approach)
            node3b.autoAdvanceToNode = "ana_intro_04_join";
            node3b.autoAdvanceDelay = 4f;

            // Branch C: Silent path
            var node3c = CreateNode(
                "ana_intro_03_silent",
                "Anastasia",
                "*phases in and out nervously*\n\nYou're... you're afraid I'm a ghost. Maybe I am. But I'm not here to haunt you. I'm trapped, same as these buildings. Buried under mud and lies.\n\nIf you're really trying to restore Tartaria... I know things that can help. Let me prove it.",
                false,
                "ana_intro_voice_03c"
            );
            node3c.relationshipDelta = 0; // No relationship change
            node3c.autoAdvanceToNode = "ana_intro_04_join";
            node3c.autoAdvanceDelay = 4f;

            // Node 4: Companion join (converging path)
            var node4 = CreateNode(
                "ana_intro_04_join",
                "Anastasia",
                "I'll stay with you. For as long as the Aether field can hold me. Every building you restore brings more of me back. Maybe... maybe one day I'll be solid enough to feel the sun again.",
                true,
                "ana_intro_voice_04"
            );
            node4.activateQuestId = "moon1_anastasia_companion";
            node4.endsConversation = true;

            // Add nodes to tree
            tree.nodes.Add(node1);
            tree.nodes.Add(node2);
            tree.nodes.Add(node3a);
            tree.nodes.Add(node3b);
            tree.nodes.Add(node3c);
            tree.nodes.Add(node4);

            // Save tree and nodes as sub-assets
            string assetPath = $"{DIALOGUE_PATH}/Anastasia_Intro.asset";
            AssetDatabase.CreateAsset(tree, assetPath);
            foreach (var node in tree.nodes)
            {
                AssetDatabase.AddObjectToAsset(node, tree);
            }

            Debug.Log($"[DialogueTreeFactory] Created Anastasia_Intro tree: {tree.nodes.Count} nodes, 3 branches");
        }

        static void CreateCassianMoon2Tree()
        {
            var tree = ScriptableObject.CreateInstance<DialogueTreeAsset>();
            tree.treeId = "Cassian_Moon2";
            tree.description = "Cassian reveals intel about corruption patterns. Player can choose to trust or question him.";
            tree.rootNodeId = "cassian_m2_01";
            tree.primarySpeaker = "Cassian";
            tree.tags = new[] { "moon_2", "cassian", "side_quest", "trust_building" };
            tree.oneTimeOnly = false; // Can replay for different outcomes

            // Node 1: Cassian offers intel
            var node1 = CreateNode(
                "cassian_m2_01",
                "Cassian",
                "*leaning against a crumbling pillar, studying a weathered map*\n\nStill here? Most restorers give up after Moon 1. The mud gets deeper, the enemies stronger. You're persistent. I'll give you that.",
                false,
                "cassian_m2_voice_01"
            );
            node1.autoAdvanceToNode = "cassian_m2_02";
            node1.autoAdvanceDelay = 3f;

            // Node 2: Intel offer
            var node2 = CreateNode(
                "cassian_m2_02",
                "Cassian",
                "I've mapped the corruption vectors in this zone. Phi-spiral patterns, radiating from three focal points. *taps map* Here, here, and here. Purge those, and the rest will collapse on its own.\n\nBut this information isn't free.",
                false,
                "cassian_m2_voice_02"
            );

            var choices2 = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "What do you want in return?",
                    nextNodeId = "cassian_m2_03_negotiate",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "Why would you help me? Who are you working for?",
                    nextNodeId = "cassian_m2_03_suspicious",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "I don't need your help. I'll find the corruption myself.",
                    nextNodeId = "cassian_m2_03_refuse",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                }
            };
            node2.choices = choices2;

            // Branch A: Negotiate
            var node3a = CreateNode(
                "cassian_m2_03_negotiate",
                "Cassian",
                "*slight smile* Smart. Nothing's free in this line of work. I need a frequency signature from the central spire. Don't ask why. Restore it, let me scan it, and we both get what we want.\n\n*hands you the map* Deal?",
                false,
                "cassian_m2_voice_03a"
            );
            node3a.relationshipDelta = 3; // +3 trust (fair negotiation)

            var choices3a = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Deal. But I'm watching you.",
                    nextNodeId = "cassian_m2_04_accept",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "No deal. Keep your secrets.",
                    nextNodeId = "cassian_m2_04_refuse_late",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                }
            };
            node3a.choices = choices3a;

            // Branch B: Suspicious
            var node3b = CreateNode(
                "cassian_m2_03_suspicious",
                "Cassian",
                "*expression hardens*\n\nCareful questions. The kind that get people buried. But fair. I'm a contractor. My employers want data on Tartarian energy flow. Whether that helps or hinders you depends on what you do with these coordinates.\n\n*pauses* I'm betting on 'help'. For now.",
                false,
                "cassian_m2_voice_03b"
            );
            node3b.relationshipDelta = -2; // -2 trust (suspicion noted)

            var choices3b = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Fine. I'll take the map. But I don't trust you.",
                    nextNodeId = "cassian_m2_04_accept_wary",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                },
                new DialogueChoice
                {
                    choiceText = "I don't work with spies. Leave.",
                    nextNodeId = "cassian_m2_04_refuse_late",
                    condition = new DialogueCondition { type = DialogueConditionType.None }
                }
            };
            node3b.choices = choices3b;

            // Branch C: Refuse immediately
            var node3c = CreateNode(
                "cassian_m2_03_refuse",
                "Cassian",
                "*nods slowly, folding the map*\n\nYour loss. The corruption will take you three times as long to clear without these vectors. But I respect the independence. Just... don't come crying when the wraiths swarm zone 7.\n\n*walks away*",
                true,
                "cassian_m2_voice_03c"
            );
            node3c.relationshipDelta = -5; // -5 trust (rejected help)
            node3c.endsConversation = true;

            // Node 4a: Accept deal
            var node4a = CreateNode(
                "cassian_m2_04_accept",
                "Cassian",
                "*hands you the map* Smart choice. The coordinates are accurate. I don't sabotage my own work. When you reach the central spire, I'll be there to scan it.\n\nUntil then... happy hunting.",
                true,
                "cassian_m2_voice_04a"
            );
            node4a.activateQuestId = "moon2_cassian_intel_quest";
            node4a.endsConversation = true;

            // Node 4b: Accept warily
            var node4b = CreateNode(
                "cassian_m2_04_accept_wary",
                "Cassian",
                "*tosses you the map without a word*\n\nTrust is earned, not given. Fine. Prove I'm wrong to help you. Or don't. Either way, the intel is real.\n\n*turns to leave* See you at the spire. Or not.",
                true,
                "cassian_m2_voice_04b"
            );
            node4b.activateQuestId = "moon2_cassian_intel_quest";
            node4b.relationshipDelta = 1; // +1 (grudging cooperation)
            node4b.endsConversation = true;

            // Node 4c: Refuse late
            var node4c = CreateNode(
                "cassian_m2_04_refuse_late",
                "Cassian",
                "*shrugs and pockets the map*\n\nSuit yourself. I'll be around if you change your mind. Just don't expect the same generosity twice.",
                true,
                "cassian_m2_voice_04c"
            );
            node4c.relationshipDelta = -3; // -3 trust
            node4c.endsConversation = true;

            // Add nodes to tree
            tree.nodes.Add(node1);
            tree.nodes.Add(node2);
            tree.nodes.Add(node3a);
            tree.nodes.Add(node3b);
            tree.nodes.Add(node3c);
            tree.nodes.Add(node4a);
            tree.nodes.Add(node4b);
            tree.nodes.Add(node4c);

            // Save tree
            string assetPath = $"{DIALOGUE_PATH}/Cassian_Moon2.asset";
            AssetDatabase.CreateAsset(tree, assetPath);
            foreach (var node in tree.nodes)
            {
                AssetDatabase.AddObjectToAsset(node, tree);
            }

            Debug.Log($"[DialogueTreeFactory] Created Cassian_Moon2 tree: {tree.nodes.Count} nodes, 5 endings");
        }

        static DialogueNodeData CreateNode(string nodeId, string speaker, string text, bool oneShot, string voiceId)
        {
            var node = ScriptableObject.CreateInstance<DialogueNodeData>();
            node.name = nodeId; // Unity asset name
            node.nodeId = nodeId;
            node.speakerName = speaker;
            node.dialogueText = text;
            node.voiceLineId = voiceId;
            node.choices = System.Array.Empty<DialogueChoice>();
            node.displayCondition = new DialogueCondition { type = DialogueConditionType.None };
            node.endsConversation = false;
            return node;
        }
#endif
    }
}
