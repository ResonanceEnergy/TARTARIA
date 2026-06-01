using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 NPC Dialogues — Milo companion conversation system
    /// Progressive dialogue tree that responds to player progress and discoveries
    /// Provides tutorial hints, lore exposition, emotional support
    /// </summary>
    [DefaultExecutionOrder(-79)]
    public class Moon1NPCDialogues : MonoBehaviour
    {
        [Header("Dialogue Configuration")]
        [SerializeField] MiloDialogueTree dialogueTree;
        [SerializeField] float dialogueCooldown = 30f;     // 30s between auto-dialogues
        [SerializeField] float proximityTriggerDistance = 5f;
        
        [Header("Milo Reference")]
        [SerializeField] GameObject milo;
        
        readonly Dictionary<string, MiloDialogueNode> _dialogueNodes = new();
        MiloDialogueNode _currentNode;
        float _nextDialogueTime;
        bool _inConversation;
        GameObject _player;
        
        // Dialogue state tracking
        int _nodesCompleted;
        bool _tutorialComplete;
        bool _firstShardCollected;
        bool _firstNodeTuned;
        bool _firstEnemyDefeated;
        bool _halfwayEncouraged;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            InitializeDialogueTree();
            WireGameEvents();
            
            // Trigger intro dialogue after 3 seconds
            Invoke(nameof(TriggerIntroDialogue), 3f);
            
            Debug.Log("[Moon1NPCDialogues] ✅ Initialized - Milo dialogue system active");
        }
        
        void OnDestroy()
        {
            UnwireGameEvents();
        }
        
        void InitializeDialogueTree()
        {
            // Define Milo's dialogue progression
            _dialogueNodes = new Dictionary<string, MiloDialogueNode>
            {
                ["intro"] = new MiloDialogueNode
                {
                    id = "intro",
                    dialogue = "Welcome to Echohaven, Architect. I'm Milo, your companion on this journey. This place... it's seen better days. But together, we can restore its resonance.",
                    emotion = MiloEmotion.Hopeful,
                    nextNodes = new[] { "tutorial_movement" }
                },
                
                ["tutorial_movement"] = new MiloDialogueNode
                {
                    id = "tutorial_movement",
                    dialogue = "Try moving around. WASD to walk, Space to jump. Get a feel for the space.",
                    emotion = MiloEmotion.Helpful,
                    nextNodes = new[] { "tutorial_combat" }
                },
                
                ["tutorial_combat"] = new MiloDialogueNode
                {
                    id = "tutorial_combat",
                    dialogue = "Watch out! A Mud Golem approaches. These creatures are born from dissonance. Use Left Click to attack. You've got this!",
                    emotion = MiloEmotion.Concerned,
                    nextNodes = new[] { "first_enemy_defeated" }
                },
                
                ["first_enemy_defeated"] = new MiloDialogueNode
                {
                    id = "first_enemy_defeated",
                    dialogue = "Excellent work! As you defeat dissonant enemies, you'll restore harmony to this place. Keep it up!",
                    emotion = MiloEmotion.Proud,
                    nextNodes = new[] { "introduce_shards" }
                },
                
                ["introduce_shards"] = new MiloDialogueNode
                {
                    id = "introduce_shards",
                    dialogue = "See that glowing crystal? That's an Aether Shard. Collect them to increase your Resonance Strength. Walk near it to collect.",
                    emotion = MiloEmotion.Helpful,
                    nextNodes = new[] { "first_shard_collected" }
                },
                
                ["first_shard_collected"] = new MiloDialogueNode
                {
                    id = "first_shard_collected",
                    dialogue = "Perfect! You can feel the resonance flowing through you, can't you? There are more shards scattered throughout Echohaven. Find them all!",
                    emotion = MiloEmotion.Excited,
                    nextNodes = new[] { "introduce_tuning_nodes" }
                },
                
                ["introduce_tuning_nodes"] = new MiloDialogueNode
                {
                    id = "introduce_tuning_nodes",
                    dialogue = "Those tall crystalline structures are Tuning Nodes. They're the key to restoring Echohaven's resonance grid. Press E when near one to tune it.",
                    emotion = MiloEmotion.Helpful,
                    nextNodes = new[] { "first_node_tuned" }
                },
                
                ["first_node_tuned"] = new MiloDialogueNode
                {
                    id = "first_node_tuned",
                    dialogue = "Brilliant! Did you feel that surge of energy? Each node you tune brings us closer to full restoration. Seven more to go!",
                    emotion = MiloEmotion.Excited,
                    nextNodes = new[] { "explore_encourage" }
                },
                
                ["explore_encourage"] = new MiloDialogueNode
                {
                    id = "explore_encourage",
                    dialogue = "You're doing great! Explore the cathedral, defeat enemies, collect shards, and tune the nodes. I'll be right here if you need me.",
                    emotion = MiloEmotion.Supportive,
                    nextNodes = new[] { "halfway_check" }
                },
                
                ["halfway_check"] = new MiloDialogueNode
                {
                    id = "halfway_check",
                    dialogue = "We're halfway there! I can feel the resonance strengthening. The mud recedes with each victory. Keep going, Architect!",
                    emotion = MiloEmotion.Proud,
                    nextNodes = new[] { "final_push" }
                },
                
                ["final_push"] = new MiloDialogueNode
                {
                    id = "final_push",
                    dialogue = "We're so close! Just a little more and Echohaven will sing with pure resonance again. I believe in you!",
                    emotion = MiloEmotion.Hopeful,
                    nextNodes = new[] { "completion" }
                },
                
                ["completion"] = new MiloDialogueNode
                {
                    id = "completion",
                    dialogue = "You did it! Echohaven is restored! Can you hear the resonance? It's beautiful... But this is just the beginning. There are twelve more Moons waiting for us.",
                    emotion = MiloEmotion.Joyful,
                    nextNodes = new string[0]  // End of Moon 1 dialogue
                }
            };
            
            _currentNode = _dialogueNodes["intro"];
        }
        
        void WireGameEvents()
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnCollectibleGathered += OnCollectibleGathered;
            GameEvents.OnTuningNodeActivated += OnTuningNodeActivated;
        }
        
        void UnwireGameEvents()
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnCollectibleGathered -= OnCollectibleGathered;
            GameEvents.OnTuningNodeActivated -= OnTuningNodeActivated;
        }
        
        void Update()
        {
            // Check for proximity-based dialogue triggers
            if (!_inConversation && milo != null && _player != null)
            {
                float distance = Vector3.Distance(milo.transform.position, _player.transform.position);
                
                if (distance <= proximityTriggerDistance && Time.time >= _nextDialogueTime)
                {
                    // Trigger contextual dialogue
                    CheckContextualDialogue();
                }
            }
        }
        
        void TriggerIntroDialogue()
        {
            ShowDialogue("intro");
        }
        
        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            if (!_firstEnemyDefeated && args.enemyType == "MudGolem")
            {
                _firstEnemyDefeated = true;
                ShowDialogue("first_enemy_defeated");
            }
        }
        
        void OnCollectibleGathered(CollectibleEventArgs args)
        {
            if (!_firstShardCollected && args.collectibleType == "AetherShard")
            {
                _firstShardCollected = true;
                ShowDialogue("first_shard_collected");
            }
        }
        
        void OnTuningNodeActivated(int nodeID)
        {
            if (!_firstNodeTuned)
            {
                _firstNodeTuned = true;
                ShowDialogue("first_node_tuned");
            }
        }
        
        void CheckContextualDialogue()
        {
            if (GameStateManager.Instance == null) return;
            
            float progress = GameStateManager.Instance.GetMoonProgress(1);
            
            // Halfway encouragement
            if (!_halfwayEncouraged && progress >= 0.5f)
            {
                _halfwayEncouraged = true;
                ShowDialogue("halfway_check");
            }
            
            // Final push
            else if (progress >= 0.8f)
            {
                ShowDialogue("final_push");
            }
            
            // Completion
            else if (progress >= 0.95f)
            {
                ShowDialogue("completion");
            }
        }
        
        void ShowDialogue(string nodeID)
        {
            if (!_dialogueNodes.ContainsKey(nodeID)) return;
            
            _currentNode = _dialogueNodes[nodeID];
            _inConversation = true;
            _nextDialogueTime = Time.time + dialogueCooldown;
            
            // Display in UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowDialogue("Milo", _currentNode.dialogue, _currentNode.emotion.ToString());
            }
            else
            {
                // Fallback: Debug log
                Debug.Log($"[Milo - {_currentNode.emotion}]: {_currentNode.dialogue}");
            }
            
            // Play voice line (if audio clip exists)
            // TODO: AudioManager.Instance.PlayVoiceLine("milo", nodeID);
            
            _nodesCompleted++;
            
            // Auto-close dialogue after 5 seconds
            Invoke(nameof(CloseDialogue), 5f);
        }
        
        void CloseDialogue()
        {
            _inConversation = false;
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideDialogue();
            }
        }
        
        /// <summary>
        /// Manual dialogue trigger (for E key interaction near Milo)
        /// </summary>
        public void TriggerManualDialogue()
        {
            if (_inConversation) return;
            
            // Show next available node
            if (_currentNode != null && _currentNode.nextNodes.Length > 0)
            {
                string nextNodeID = _currentNode.nextNodes[0];
                ShowDialogue(nextNodeID);
            }
            else
            {
                // Generic "talk to Milo" dialogue
                ShowDialogue("explore_encourage");
            }
        }
    }
    
    /// <summary>
    /// Dialogue node structure for Milo's conversation tree
    /// </summary>
    [System.Serializable]
    public class MiloDialogueNode
    {
        public string id;
        public string dialogue;
        public MiloEmotion emotion;
        public string[] nextNodes;
    }
    
    /// <summary>
    /// Milo's emotional states (for animation/voice modulation)
    /// </summary>
    public enum MiloEmotion
    {
        Neutral,
        Hopeful,
        Helpful,
        Concerned,
        Proud,
        Excited,
        Supportive,
        Joyful
    }
    
    [System.Serializable]
    public class MiloDialogueTree
    {
        public MiloDialogueNode[] nodes;
    }
}
