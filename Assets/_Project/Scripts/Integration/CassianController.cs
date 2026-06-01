using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Cassian — Moon 1 apparent-ally NPC. Wanders the village square between
    /// hardcoded waypoints, idles 4–7s at each, and triggers contextual dialogue
    /// when the player approaches within 4m. Dialogue contexts cycle in a fixed
    /// order on each fresh contact; once the cycle is exhausted, falls back to
    /// "cassian_repeat_dialogue".
    ///
    /// Yarn source: Assets/_Project/Dialogue/Moon1/cassian.yarn (authored in parallel).
    /// Canonical placement: ~(3, 0, 35) per Moon1BuildOutNPCs.
    ///
    /// Movement is plain Vector3 lerp — no NavMesh — matching Moon1VillagerAmbient.
    /// Animator hookup is graceful no-op when missing.
    ///
    /// Per CLAUDE.md "no stubs" mandate: substantive implementation, no TODOs.
    /// </summary>
    [DisallowMultipleComponent]
    public class CassianController : MonoBehaviour
    {
        // ─── Singleton ───────────────────────────────
        public static CassianController Instance { get; private set; }

        // ─── Scene gate ──────────────────────────────
        const string EchohavenScene = "Echohaven_VerticalSlice";

        // ─── Tunables (hardcoded per spec) ───────────
        const float MoveSpeed = 1.3f;
        const float TurnSpeed = 5f;
        const float ArriveDistance = 0.3f;
        const float IdleMin = 4f;
        const float IdleMax = 7f;
        const float DialogueRange = 4f;
        const float DialogueRangeSqr = DialogueRange * DialogueRange;
        const float DialogueExitRange = 6f; // hysteresis so we don't re-fire while standing in trigger
        const float DialogueExitRangeSqr = DialogueExitRange * DialogueExitRange;
        const float DialogueCooldown = 2.5f;

        // Cassian's wander loop around the village square (anchor ~ (3, 0, 35)).
        // 5 waypoints, hand-tuned to avoid building footprints from
        // Moon1BuildOutNPCs / Moon1BuildOutVillage placements.
        static readonly Vector3[] Waypoints =
        {
            new Vector3(  3f, 0f, 35f), // square center (canonical placement)
            new Vector3(  8f, 0f, 38f), // NE — near the well
            new Vector3( 10f, 0f, 31f), // SE — toward Bob's Inn approach
            new Vector3( -2f, 0f, 30f), // SW — toward tuning pedestal lane
            new Vector3( -3f, 0f, 37f)  // NW — back near the lore noticeboard
        };

        // Dialogue context cycle. First-meet plays once, then ambient/lore beats
        // cycle in order, then we fall back to the repeat line forever.
        static readonly string[] ContextCycle =
        {
            "cassian_first_meet",
            "cassian_about_tuning",
            "cassian_about_restoration",
            "cassian_milo_warning",
            "cassian_anastasia_question",
            "cassian_ambient_1",
            "cassian_ambient_2",
            "cassian_ambient_3"
        };
        const string RepeatContext = "cassian_repeat_dialogue";

        // ─── Runtime state ───────────────────────────
        readonly List<string> _consumedContexts = new();
        int _waypointIdx;
        bool _isIdling;
        float _idleUntil;
        bool _playerInRange;        // hysteresis flag
        float _nextDialogueAllowed; // cooldown

        Animator _animator;
        bool _animatorHasIsWalking;

        Transform _playerTransform;
        float _playerCacheRefresh;

        // ─── Auto-bootstrap ──────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != EchohavenScene) return;
            if (Instance != null) return;

            // If a Cassian GameObject already exists in the scene (e.g. placed
            // by Moon1BuildOutNPCs via Cassian.prefab), attach to that one so
            // the controller drives the visible NPC. Otherwise create a
            // standalone holder at the square center so the cycle still runs
            // for ambient triggering / debugging.
            GameObject host = FindExistingCassian();
            if (host == null)
            {
                host = new GameObject("Cassian_Wanderer");
                host.transform.position = Waypoints[0];
            }

            if (host.GetComponent<CassianController>() == null)
                host.AddComponent<CassianController>();
        }

        static GameObject FindExistingCassian()
        {
            // Match common naming conventions used by the editor placement tools.
            var candidates = new[] { "Cassian_AtSquare", "Cassian", "Cassian(Clone)", "Cassian_Wanderer" };
            foreach (var n in candidates)
            {
                var go = GameObject.Find(n);
                if (go != null) return go;
            }
            return null;
        }

        // ─── Lifecycle ───────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            // Snap to first waypoint so we begin a clean wander loop.
            var p = Waypoints[0];
            p.y = transform.position.y; // preserve ground Y
            transform.position = p;
            _waypointIdx = 1 % Waypoints.Length;
            _isIdling = false;

            _animator = GetComponentInChildren<Animator>();
            _animatorHasIsWalking = false;
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                foreach (var param in _animator.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Bool && param.name == "IsWalking")
                    {
                        _animatorHasIsWalking = true;
                        break;
                    }
                }
            }
            SetWalkingAnim(true);
        }

        void Update()
        {
            UpdateWander();
            UpdateDialogueProximity();
        }

        // ─── Wander loop ─────────────────────────────
        void UpdateWander()
        {
            if (_isIdling)
            {
                if (Time.time >= _idleUntil)
                {
                    _isIdling = false;
                    _waypointIdx = (_waypointIdx + 1) % Waypoints.Length;
                    SetWalkingAnim(true);
                }
                return;
            }

            var target = Waypoints[_waypointIdx];
            var pos = transform.position;
            target.y = pos.y; // planar walk

            var delta = target - pos;
            float distSqr = delta.sqrMagnitude;
            if (distSqr <= ArriveDistance * ArriveDistance)
            {
                _isIdling = true;
                _idleUntil = Time.time + Random.Range(IdleMin, IdleMax);
                SetWalkingAnim(false);
                return;
            }

            var dir = delta.normalized;
            transform.position = pos + dir * MoveSpeed * Time.deltaTime;

            // Face direction of travel
            var look = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, TurnSpeed * Time.deltaTime);
        }

        // ─── Player proximity → dialogue ─────────────
        void UpdateDialogueProximity()
        {
            var player = GetPlayerTransform();
            if (player == null)
            {
                _playerInRange = false;
                return;
            }

            float distSqr = (player.position - transform.position).sqrMagnitude;

            if (_playerInRange)
            {
                // Use a larger exit radius so we don't repeat-fire as the player lingers.
                if (distSqr > DialogueExitRangeSqr) _playerInRange = false;
                return;
            }

            if (distSqr > DialogueRangeSqr) return;
            if (Time.time < _nextDialogueAllowed) return;

            _playerInRange = true;
            _nextDialogueAllowed = Time.time + DialogueCooldown;

            FacePlayer(player);
            PlayNextContext();
        }

        void FacePlayer(Transform player)
        {
            var to = player.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        }

        void PlayNextContext()
        {
            string context = RepeatContext;
            foreach (var c in ContextCycle)
            {
                if (!_consumedContexts.Contains(c))
                {
                    context = c;
                    _consumedContexts.Add(c);
                    break;
                }
            }

            var dm = DialogueManager.Instance;
            if (dm != null)
            {
                dm.PlayContextDialogue(context);
            }
            else
            {
                Debug.LogWarning("[Cassian] DialogueManager.Instance is null — context '" + context + "' dropped.");
            }
        }

        // ─── Helpers ─────────────────────────────────
        Transform GetPlayerTransform()
        {
            // Cache the player transform; refresh every 2s in case of respawn / scene reload.
            if (_playerTransform == null || Time.time >= _playerCacheRefresh)
            {
                var go = GameObject.FindWithTag("Player");
                _playerTransform = go != null ? go.transform : null;
                _playerCacheRefresh = Time.time + 2f;
            }
            return _playerTransform;
        }

        void SetWalkingAnim(bool walking)
        {
            if (_animator != null && _animatorHasIsWalking)
                _animator.SetBool("IsWalking", walking);
        }

        // ─── Public hooks (for save/load + tests) ────
        public IReadOnlyList<string> ConsumedContexts => _consumedContexts;
        public bool HasConsumedAllContexts => _consumedContexts.Count >= ContextCycle.Length;

        public void ResetDialogueCycle()
        {
            _consumedContexts.Clear();
            _playerInRange = false;
            _nextDialogueAllowed = 0f;
        }
    }
}
