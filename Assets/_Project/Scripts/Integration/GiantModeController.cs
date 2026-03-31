using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Giant Mode Controller -- Anastasia grows the player to giant scale during
    /// Moon 1+ to interact with architecture that's too large at normal size.
    ///
    /// 3 Abilities (GDD ref: 00_MASTER_GDD §Giant Mode):
    ///   1. Precision Rock Cut  -- slice corrupted stone from buildings
    ///   2. Rubble Clear        -- sweep debris fields with giant hands
    ///   3. Building Lift       -- physically reposition restored structures
    ///
    /// Camera zooms to isometric during Giant Mode.
    /// Costs Aether per second to maintain.
    /// </summary>
    [DisallowMultipleComponent]
    public class GiantModeController : MonoBehaviour
    {
        public static GiantModeController Instance { get; private set; }

        [Header("Scale")]
        [SerializeField] float giantScale = 5f;
        [SerializeField] float scaleTransitionSpeed = 3f;

        [Header("Aether Cost")]
        [SerializeField] float aetherCostPerSecond = 5f;
        [SerializeField] float minimumAetherToActivate = 20f;

        [Header("Abilities")]
        [SerializeField] float rockCutRange = 15f;
        [SerializeField] float rockCutDamage = 50f;
        [SerializeField] float rubbleClearRadius = 20f;
        [SerializeField] float buildingLiftRange = 12f;
        [SerializeField] float buildingLiftSpeed = 2f;

        [Header("References")]
        [SerializeField] Transform playerTransform;
        [SerializeField] Camera.CameraController cameraController;

        bool _isGiant;
        float _currentScale = 1f;
        float _targetScale = 1f;
        GiantAbility _activeAbility = GiantAbility.None;
        Transform _liftedBuilding;
        float _aetherCharge;

        // Pre-allocated buffer for Physics.OverlapSphereNonAlloc
        static readonly Collider[] _overlapBuffer = new Collider[32];

        public bool IsGiant => _isGiant;
        public GiantAbility ActiveAbility => _activeAbility;

        public event System.Action OnGiantActivated;
        public event System.Action OnGiantDeactivated;
        public event System.Action<GiantAbility> OnAbilityUsed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            // Smooth scale transition
            if (Mathf.Abs(_currentScale - _targetScale) > 0.01f)
            {
                _currentScale = Mathf.MoveTowards(_currentScale, _targetScale,
                    scaleTransitionSpeed * Time.deltaTime);

                if (playerTransform != null)
                    playerTransform.localScale = Vector3.one * _currentScale;
            }

            // Drain aether while giant
            if (_isGiant)
            {
                _aetherCharge -= aetherCostPerSecond * Time.deltaTime;
                HUDController.Instance?.UpdateAetherCharge(_aetherCharge);

                if (_aetherCharge <= 0f)
                {
                    _aetherCharge = 0f;
                    DeactivateGiantMode();
                }

                // Update lifted building position
                if (_activeAbility == GiantAbility.BuildingLift && _liftedBuilding != null)
                {
                    UpdateBuildingLift();
                }
            }
        }

        /// <summary>
        /// Toggle giant mode. Called by PlayerInputHandler or GameLoopController.
        /// </summary>
        public void ToggleGiantMode(float currentAether)
        {
            if (_isGiant)
            {
                DeactivateGiantMode();
            }
            else
            {
                _aetherCharge = currentAether;
                if (_aetherCharge >= minimumAetherToActivate)
                    ActivateGiantMode();
            }
        }

        void ActivateGiantMode()
        {
            _isGiant = true;
            _targetScale = giantScale;

            // Switch camera to isometric overview
            cameraController?.SetGiantMode(true);

            // Update ECS combat entity
            CombatBridge.Instance?.SetGiantMode(true);

            // Haptic feedback
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();

            Debug.Log("[GiantMode] Activated");
            OnGiantActivated?.Invoke();
        }

        void DeactivateGiantMode()
        {
            _isGiant = false;
            _targetScale = 1f;
            _activeAbility = GiantAbility.None;

            // Drop any lifted building
            if (_liftedBuilding != null)
            {
                _liftedBuilding = null;
            }

            cameraController?.SetGiantMode(false);
            CombatBridge.Instance?.SetGiantMode(false);

            Debug.Log("[GiantMode] Deactivated");
            OnGiantDeactivated?.Invoke();
        }

        // ─── Ability 1: Precision Rock Cut ───────────

        /// <summary>
        /// Slice corrupted stone from a building. Requires Giant Mode.
        /// Target is the nearest building within rockCutRange with corruption.
        /// </summary>
        public void UsePrecisionRockCut(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.PrecisionRockCut;

            int count = Physics.OverlapSphereNonAlloc(targetPoint, rockCutRange, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                var building = _overlapBuffer[i].GetComponent<InteractableBuilding>();
                if (building != null)
                {
                    // Apply corruption removal
                    CorruptionSystem.Instance?.PurgeCorruption(building.BuildingId, rockCutDamage);

                    VFXController.Instance?.PlayResonancePulse(targetPoint, rockCutRange * 0.5f);
                    HapticFeedbackManager.Instance?.PlayCombatHit();
                    break;
                }
            }

            OnAbilityUsed?.Invoke(GiantAbility.PrecisionRockCut);
        }

        // ─── Ability 2: Rubble Clear ─────────────────

        /// <summary>
        /// Sweep a debris field clear. Area of Effect around player.
        /// </summary>
        public void UseRubbleClear()
        {
            if (!_isGiant || playerTransform == null) return;

            _activeAbility = GiantAbility.RubbleClear;

            int count = Physics.OverlapSphereNonAlloc(playerTransform.position, rubbleClearRadius, _overlapBuffer);
            int cleared = 0;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col.CompareTag("Rubble"))
                {
                    // Launch rubble away with force
                    var rb = col.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (col.transform.position - playerTransform.position).normalized;
                        rb.AddForce(dir * 500f + Vector3.up * 200f, ForceMode.Impulse);
                    }
                    cleared++;
                }
            }

            VFXController.Instance?.PlayResonancePulse(playerTransform.position, rubbleClearRadius);
            HapticFeedbackManager.Instance?.PlayGolemDeath();

            Debug.Log($"[GiantMode] Rubble Clear: {cleared} objects cleared");
            OnAbilityUsed?.Invoke(GiantAbility.RubbleClear);
        }

        // ─── Ability 3: Building Lift ────────────────

        /// <summary>
        /// Pick up a restored building and reposition it.
        /// </summary>
        public void StartBuildingLift(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.BuildingLift;

            int count = Physics.OverlapSphereNonAlloc(targetPoint, buildingLiftRange, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                var building = _overlapBuffer[i].GetComponent<InteractableBuilding>();
                if (building != null && building.State == BuildingRestorationState.Active)
                {
                    _liftedBuilding = building.transform;
                    Debug.Log($"[GiantMode] Lifting building: {building.BuildingId}");
                    break;
                }
            }

            OnAbilityUsed?.Invoke(GiantAbility.BuildingLift);
        }

        /// <summary>
        /// Drop the currently lifted building at its current position.
        /// </summary>
        public void ReleaseBuildingLift()
        {
            if (_liftedBuilding != null)
            {
                Debug.Log($"[GiantMode] Building placed at {_liftedBuilding.position}");
                _liftedBuilding = null;
            }
            _activeAbility = GiantAbility.None;
        }

        void UpdateBuildingLift()
        {
            if (_liftedBuilding == null || playerTransform == null) return;

            // Building floats in front of player and above
            Vector3 targetPos = playerTransform.position
                + playerTransform.forward * (buildingLiftRange * 0.5f)
                + Vector3.up * giantScale * 2f;

            _liftedBuilding.position = Vector3.Lerp(
                _liftedBuilding.position, targetPos,
                buildingLiftSpeed * Time.deltaTime);
        }

        // ─── Save/Load ──────────────────────────────

        int _totalActivations;
        int _buildingsLifted;
        int _rubbleCleared;
        float _totalTimeAsGiant;

        public Save.GiantModeSaveBlock GetSaveData()
        {
            return new Save.GiantModeSaveBlock
            {
                totalActivations = _totalActivations,
                buildingsLifted = _buildingsLifted,
                rubbleCleared = _rubbleCleared,
                totalTimeAsGiant = _totalTimeAsGiant
            };
        }

        public void LoadSaveData(Save.GiantModeSaveBlock data)
        {
            if (data == null) return;
            _totalActivations = data.totalActivations;
            _buildingsLifted = data.buildingsLifted;
            _rubbleCleared = data.rubbleCleared;
            _totalTimeAsGiant = data.totalTimeAsGiant;
        }
    }

    public enum GiantAbility : byte
    {
        None = 0,
        PrecisionRockCut = 1,
        RubbleClear = 2,
        BuildingLift = 3
    }
}
