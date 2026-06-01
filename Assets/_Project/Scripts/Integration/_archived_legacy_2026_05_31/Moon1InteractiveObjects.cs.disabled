using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Interactive Objects — Tuning Nodes, doors, levers, puzzles
    /// Manages all interactive elements in Echohaven that respond to player input.
    /// </summary>
    [DefaultExecutionOrder(-76)]
    public class Moon1InteractiveObjects : MonoBehaviour
    {
        [Header("Tuning Nodes")]
        [SerializeField] GameObject tuningNodePrefab;
        [SerializeField] int totalTuningNodes = 8;        // 8 nodes to restore resonance grid
        [SerializeField] float tuningRadius = 3f;
        
        [Header("Doors & Gates")]
        [SerializeField] GameObject[] resonanceDoors;     // Doors that unlock with RS
        [SerializeField] float doorUnlockCost = 10f;      // 10 RS to unlock doors
        
        [Header("Levers & Switches")]
        [SerializeField] GameObject[] mechanicalLevers;   // Clockwork mechanisms
        
        readonly List<TuningNode> _tuningNodes = new();
        readonly List<InteractiveDoor> _doors = new();
        int _nodesTuned;
        
        public int NodesTuned => _nodesTuned;
        public float TuningProgress => _nodesTuned / (float)totalTuningNodes;
        
        void Start()
        {
            SpawnTuningNodes();
            SetupDoors();
            SetupLevers();
            
            Debug.Log($"[Moon1InteractiveObjects] ✅ Initialized - {totalTuningNodes} tuning nodes spawned");
        }
        
        void SpawnTuningNodes()
        {
            if (tuningNodePrefab == null)
            {
                Debug.LogWarning("[Moon1InteractiveObjects] No Tuning Node prefab assigned!");
                return;
            }
            
            // Predefined strategic positions for tutorial and progression
            Vector3[] nodePositions = new Vector3[]
            {
                new Vector3(10f, 1f, 15f),    // Tutorial node near spawn
                new Vector3(-12f, 1f, 20f),   // West cathedral wing
                new Vector3(18f, 1f, -10f),   // East plaza
                new Vector3(-5f, 3f, -15f),   // Lower catacombs entrance
                new Vector3(25f, 2f, 25f),    // North bell tower
                new Vector3(-20f, 1f, -8f),   // West courtyard
                new Vector3(8f, 4f, 30f),     // Upper gallery
                new Vector3(0f, 1f, -25f),    // South gate
            };
            
            for (int i = 0; i < Mathf.Min(nodePositions.Length, totalTuningNodes); i++)
            {
                GameObject nodeObj = Instantiate(tuningNodePrefab, nodePositions[i], Quaternion.identity);
                nodeObj.name = $"TuningNode_{i}";
                
                // Add/configure component
                TuningNode node = nodeObj.GetOrAddComponent<TuningNode>();
                node.nodeID = i;
                node.tuningRadius = tuningRadius;
                node.onTuned += () => OnNodeTuned(i);
                
                _tuningNodes.Add(node);
            }
        }
        
        void SetupDoors()
        {
            if (resonanceDoors == null) return;
            
            foreach (GameObject doorObj in resonanceDoors)
            {
                if (doorObj == null) continue;
                
                InteractiveDoor door = doorObj.GetOrAddComponent<InteractiveDoor>();
                door.unlockCost = doorUnlockCost;
                door.doorType = DoorType.ResonanceLocked;
                _doors.Add(door);
            }
        }
        
        void SetupLevers()
        {
            if (mechanicalLevers == null) return;
            
            foreach (GameObject leverObj in mechanicalLevers)
            {
                if (leverObj == null) continue;
                
                InteractiveLever lever = leverObj.GetOrAddComponent<InteractiveLever>();
                lever.leverID = leverObj.name;
            }
        }
        
        void OnNodeTuned(int nodeID)
        {
            _nodesTuned++;
            
            Debug.Log($"[Moon1InteractiveObjects] Tuning Node {nodeID} tuned ({_nodesTuned}/{totalTuningNodes})");
            
            // Reward RS for tuning
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.AddResonancePoints(5f);
            }
            
            // VFX and audio
            GameEvents.FireTuningNodeActivated(nodeID);
            
            // Check progression
            if (_nodesTuned >= totalTuningNodes)
            {
                OnAllNodesTuned();
            }
        }
        
        void OnAllNodesTuned()
        {
            Debug.Log("[Moon1InteractiveObjects] ✨ ALL TUNING NODES ACTIVATED!");
            
            // Major progression event
            GameEvents.FireMoonProgressUpdate(1, 0.30f);  // 30% Moon 1 progress
            
            // Unlock all resonance doors
            foreach (var door in _doors)
            {
                if (door.doorType == DoorType.ResonanceLocked)
                {
                    door.Unlock();
                }
            }
            
            // Achievement
            GameEvents.FireAchievementUnlocked("echohaven_grid_restored");
        }
    }
    
    /// <summary>
    /// Tuning Node - Player must interact to restore resonance
    /// </summary>
    public class TuningNode : MonoBehaviour
    {
        public int nodeID;
        public float tuningRadius = 3f;
        public System.Action onTuned;
        
        bool _tuned;
        GameObject _player;
        MeshRenderer _renderer;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _renderer = GetComponent<MeshRenderer>();
            
            // Visual feedback when not tuned
            if (!_tuned && _renderer != null)
            {
                _renderer.material.SetColor("_EmissionColor", Color.red * 0.5f);
            }
        }
        
        void Update()
        {
            if (_tuned || _player == null) return;
            
            float distance = Vector3.Distance(transform.position, _player.transform.position);
            
            if (distance <= tuningRadius)
            {
                // Show prompt to interact
                // TODO: Show "Press E to Tune Node" UI
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TuneNode();
                }
            }
        }
        
        void TuneNode()
        {
            if (_tuned) return;
            
            _tuned = true;
            onTuned?.Invoke();
            
            // Visual change
            if (_renderer != null)
            {
                _renderer.material.SetColor("_EmissionColor", Color.cyan * 2f);
            }
            
            // Animation
            LeanTween.scale(gameObject, transform.localScale * 1.2f, 0.3f)
                .setEaseOutBack()
                .setLoopPingPong(1);
                
            Debug.Log($"[TuningNode] Node {nodeID} tuned!");
        }
    }
    
    /// <summary>
    /// Interactive Door - Can be locked by various mechanisms
    /// </summary>
    public enum DoorType
    {
        ResonanceLocked,  // Requires RS to unlock
        KeyLocked,        // Requires key item
        PuzzleLocked,     // Unlocked by solving puzzle
        Open              // Always accessible
    }
    
    public class InteractiveDoor : MonoBehaviour
    {
        public DoorType doorType = DoorType.ResonanceLocked;
        public float unlockCost = 10f;
        bool _unlocked;
        
        public void Unlock()
        {
            if (_unlocked) return;
            
            _unlocked = true;
            
            // Open animation
            LeanTween.moveLocalY(gameObject, transform.localPosition.y + 4f, 2f)
                .setEaseInOutQuad();
                
            Debug.Log($"[InteractiveDoor] {name} unlocked!");
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            if (!_unlocked && doorType == DoorType.ResonanceLocked)
            {
                // Show unlock prompt
                // TODO: "Press E to Unlock (10 RS)" UI
            }
        }
    }
    
    /// <summary>
    /// Interactive Lever - Mechanical puzzle element
    /// </summary>
    public class InteractiveLever : MonoBehaviour
    {
        public string leverID;
        public bool isPulled;
        
        public void Pull()
        {
            if (isPulled) return;
            
            isPulled = true;
            
            // Rotate lever
            LeanTween.rotateX(gameObject, -45f, 0.5f).setEaseOutBounce();
            
            // Fire event for puzzle systems
            GameEvents.FireLeverPulled(leverID);
            
            Debug.Log($"[InteractiveLever] {leverID} pulled!");
        }
    }
}
