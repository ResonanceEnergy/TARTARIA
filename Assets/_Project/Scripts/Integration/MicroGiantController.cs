using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Micro-Giant Controller -- shrink mechanic for Moon 2.
    ///
    /// Player enters "micro" scale to explore the inner fractal architecture
    /// of Tartarian domes and resonance towers. Inside, the macro-scale
    /// building becomes a navigable dungeon of crystalline corridors,
    /// Aether conduits, and corruption nodes.
    ///
    /// Flow:
    ///   1. Player interacts with a resonance point on a restored building
    ///   2. Camera zooms into building → screen flash → player shrinks
    ///   3. Interior layout procedurally scales up around player
    ///   4. Player navigates, purges corruption nodes, solves puzzles
    ///   5. Upon completion, player is ejected back to macro scale
    ///
    /// Inverse of Giant Mode: micro scale = 0.1x, camera shifts to first-person.
    /// </summary>
    [DisallowMultipleComponent]
    public class MicroGiantController : MonoBehaviour
    {
        public static MicroGiantController Instance { get; private set; }

        [Header("Scale")]
        [SerializeField] float microScale = 0.1f;
        [SerializeField] float scaleTransitionDuration = 1.5f;

        [Header("Interior")]
        [SerializeField] float interiorScale = 50f;
        [SerializeField] float corruptionNodeRadius = 3f;
        [SerializeField] int corruptionNodesToPurge = 3;

        [Header("Camera")]
        [SerializeField] Camera.CameraController cameraController;

        [Header("Aether")]
        [SerializeField] float aetherCostPerSecond = 1f;

        bool _isMicro;
        float _scaleTransitionTimer;
        float _currentScale = 1f;
        float _targetScale = 1f;
        float _aetherCharge;
        string _activeBuildingId;
        Vector3 _entryPosition;
        int _nodesPurged;
        Transform _playerTransform;

        // Interior corruption nodes
        readonly System.Collections.Generic.List<CorruptionNode> _activeNodes = new();

        public bool IsMicro => _isMicro;
        public int NodesPurged => _nodesPurged;
        public int TotalNodes => corruptionNodesToPurge;
        public string ActiveBuildingId => _activeBuildingId;

        public event System.Action OnMicroEntered;
        public event System.Action OnMicroExited;
        public event System.Action<int> OnNodePurged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                if (_isMicro) ExitMicroMode();
                Instance = null;
            }
        }

        void Start()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }

        void Update()
        {
            if (!_isMicro) return;

            // Scale transition
            if (Mathf.Abs(_currentScale - _targetScale) > 0.001f)
            {
                float scaleSpeed = scaleTransitionDuration > 0f
                    ? 1f / scaleTransitionDuration
                    : 100f;
                _currentScale = Mathf.MoveTowards(_currentScale, _targetScale,
                    scaleSpeed * Time.deltaTime);

                if (_playerTransform != null)
                    _playerTransform.localScale = Vector3.one * _currentScale;
            }

            // Aether drain
            _aetherCharge -= aetherCostPerSecond * Time.deltaTime;
            HUDController.Instance?.UpdateAetherCharge(_aetherCharge);

            if (_aetherCharge <= 0f)
            {
                _aetherCharge = 0f;
                ExitMicroMode();
                return;
            }

            // Check if all nodes purged
            if (_nodesPurged >= corruptionNodesToPurge)
            {
                CompleteMicroDungeon();
            }
        }

        /// <summary>
        /// Enter micro mode inside a building. Called by building interaction.
        /// </summary>
        public void EnterMicroMode(string buildingId, Vector3 entryPoint, float currentAether)
        {
            if (_isMicro) return;

            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player == null) return;
                _playerTransform = player.transform;
            }
            _activeBuildingId = buildingId;
            _entryPosition = _playerTransform.position;
            _aetherCharge = currentAether;
            _nodesPurged = 0;
            _isMicro = true;
            _targetScale = microScale;

            // Generate interior corruption nodes
            GenerateCorruptionNodes(entryPoint);

            // Shift camera to first-person/close follow
            cameraController?.SetMicroMode(true);

            // Move player to interior origin
            _playerTransform.position = entryPoint;

            HapticFeedbackManager.Instance?.PlayDiscovery();
            Audio.AudioManager.Instance?.PlaySFX2D("MicroModeEnter");
            Debug.Log($"[MicroGiant] Entering building: {buildingId}");
            OnMicroEntered?.Invoke();
        }

        /// <summary>
        /// Exit micro mode, returning player to macro scale.
        /// </summary>
        public void ExitMicroMode()
        {
            if (!_isMicro) return;

            _isMicro = false;
            _targetScale = 1f;
            _currentScale = 1f;

            if (_playerTransform != null)
            {
                _playerTransform.localScale = Vector3.one;
                _playerTransform.position = _entryPosition;
            }

            // Clear interior nodes
            ClearCorruptionNodes();

            cameraController?.SetMicroMode(false);

            Debug.Log("[MicroGiant] Exited micro mode");
            OnMicroExited?.Invoke();
            Audio.AudioManager.Instance?.PlaySFX2D("MicroModeExit");
        }

        /// <summary>
        /// Attempt to purge a corruption node at the given position.
        /// Called by player interaction.
        /// </summary>
        public bool TryPurgeNode(Vector3 position)
        {
            if (!_isMicro) return false;

            for (int i = _activeNodes.Count - 1; i >= 0; i--)
            {
                float dist = Vector3.Distance(position, _activeNodes[i].position);
                if (dist <= corruptionNodeRadius && !_activeNodes[i].purged)
                {
                    var node = _activeNodes[i];
                    node.purged = true;
                    _activeNodes[i] = node;
                    _nodesPurged++;

                    VFXController.Instance?.PlayTuningSuccess(node.position, true);
                    HapticFeedbackManager.Instance?.PlayPerfectTune();

                    Debug.Log($"[MicroGiant] Node purged: {_nodesPurged}/{corruptionNodesToPurge}");
                    OnNodePurged?.Invoke(_nodesPurged);
                    return true;
                }
            }

            return false;
        }

        void GenerateCorruptionNodes(Vector3 origin)
        {
            _activeNodes.Clear();

            for (int i = 0; i < corruptionNodesToPurge; i++)
            {
                // Distribute nodes in a circle around origin, scaled by interior size
                float angle = (360f / corruptionNodesToPurge) * i * Mathf.Deg2Rad;
                float radius = interiorScale * 0.3f;

                _activeNodes.Add(new CorruptionNode
                {
                    position = origin + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius),
                    purged = false
                });
            }
        }

        void ClearCorruptionNodes()
        {
            _activeNodes.Clear();
        }

        void CompleteMicroDungeon()
        {
            Debug.Log($"[MicroGiant] Building {_activeBuildingId} interior purged!");

            // Grant RS bonus for completing interior
            GameLoopController.Instance?.QueueRSReward(5f, $"micro_purge_{_activeBuildingId}");

            // Purge corruption from parent building
            CorruptionSystem.Instance?.PurgeCorruption(_activeBuildingId, 100f);
            Save.SaveManager.Instance?.MarkDirty();
            Audio.AudioManager.Instance?.PlaySFX2D("MicroPurgeComplete");

            ExitMicroMode();
        }

        struct CorruptionNode
        {
            public Vector3 position;
            public bool purged;
        }
    }
}
