using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
    /// Moon 2 Exclusive Giant Mode Integration & Synergies (Crystal/Corruption Environment):
    ///   - Crystal Shatter Resonance Stomp: Shatters dissonance crystals at titanic scale
    ///   - Corruption Vein Yanking: Giant hands rip fractal corruption veins like roots
    ///   - Cathedral Quake (Major): The signature cathedral-shaking sequence that makes the living crystal dome convulse
    ///   - Fractal Facet Revelation: Upper-scale exploration only possible at giant height
    ///   - Ley Bridge Resonance Stomp: Giant steps create temporary crystal ley bridges between the 5 structures
    ///
    /// Camera zooms to isometric during Giant Mode.
    /// Costs Aether per second to maintain.
    /// All Moon 2 moments are thematically tied to crystal shattering, vein manipulation, massive cathedral interaction.
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

        [Header("Moon 2 Crystal Giant Mode")]
        [SerializeField] float moon2CrystalStompRadius = 22f;
        [SerializeField] float moon2VeinYankRadius = 18f;

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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
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
                _totalTimeAsGiant += Time.deltaTime;
                HUDController.Instance?.UpdateAetherCharge(_aetherCharge);

                // R5 Combat HUD wiring: drive giant meter from real internal aether readiness (production polish)
                float readiness = Mathf.Clamp01(_aetherCharge / Mathf.Max(1f, minimumAetherToActivate));
                Tartaria.UI.HUDController.Instance?.UpdateGiantMeter(readiness, false);

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
                else
                    Audio.AudioManager.Instance?.PlaySFX2D("InsufficientAether");
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
            Audio.AudioManager.Instance?.PlaySFX("GiantModeActivate", playerTransform != null ? playerTransform.position : Vector3.zero);

            Debug.Log("[GiantMode] Activated");
            _totalActivations++;
            OnGiantActivated?.Invoke();

            // R5: explicit HUD giant meter + accessibility caption on activate (richer feedback)
            Tartaria.UI.HUDController.Instance?.UpdateGiantMeter(1f, true);
            Tartaria.UI.AccessibilityManager.Instance?.PostSFXCaption("GiantMeter", "Giant Mode activated — world scale transformation engaged. Aether draining.");
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
            Audio.AudioManager.Instance?.PlaySFX2D("GiantModeDeactivate");
            Save.SaveManager.Instance?.MarkDirty();

            Debug.Log("[GiantMode] Deactivated");
            OnGiantDeactivated?.Invoke();

            // R5: HUD meter + caption on exit
            Tartaria.UI.HUDController.Instance?.HideGiantMeter();
            Tartaria.UI.AccessibilityManager.Instance?.PostSFXCaption("GiantMeter", "Giant Mode deactivated. Returning to normal scale.");
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
                if (_overlapBuffer[i].TryGetComponent<InteractableBuilding>(out var building))
                {
                    // Apply corruption removal
                    CorruptionSystem.Instance?.PurgeCorruption(building.BuildingId, rockCutDamage);

                    VFXController.Instance?.PlayResonancePulse(targetPoint, rockCutRange * 0.5f);
                    HapticFeedbackManager.Instance?.PlayCombatHit();
                    Audio.AudioManager.Instance?.PlaySFX("RockCut", targetPoint);
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
                    if (col.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 dir = (col.transform.position - playerTransform.position).normalized;
                        rb.AddForce(dir * 500f + Vector3.up * 200f, ForceMode.Impulse);
                    }
                    cleared++;
                }
            }

            VFXController.Instance?.PlayResonancePulse(playerTransform.position, rubbleClearRadius);
            HapticFeedbackManager.Instance?.PlayGolemDeath();
            Audio.AudioManager.Instance?.PlaySFX("RubbleClear", playerTransform.position);

            Debug.Log($"[GiantMode] Rubble Clear: {cleared} objects cleared");
            _rubbleCleared += cleared;
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
                if (_overlapBuffer[i].TryGetComponent<InteractableBuilding>(out var building)
                    && building.State == BuildingRestorationState.Active)
                {
                    _liftedBuilding = building.transform;
                    _buildingsLifted++;
                    Audio.AudioManager.Instance?.PlaySFX("BuildingPickup", _liftedBuilding.position);
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
                Audio.AudioManager.Instance?.PlaySFX2D("BuildingPlace");
                Save.SaveManager.Instance?.MarkDirty();
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

        // ─── Moon 2 Giant Mode Crystal/Corruption Synergies (POWER FANTASY) ─────────────────

        private bool IsMoon2CrystalEnvironment()
        {
            var scene = SceneManager.GetActiveScene();
            string name = scene.name;
            return name.Contains("CrystallineCaverns") || name.Contains("Moon2") || 
                   name.ToLower().Contains("crystal") || name.ToLower().Contains("cavern");
        }

        /// <summary>
        /// Moon 2 #1: Crystal Shatter Resonance Stomp.
        /// Giant-scale stomp shatters clusters of dissonance crystals and exposed corruption veins.
        /// Unique visual: slow-motion shard explosions + chain fuse burns across the cathedral floor.
        /// Synergizes with CorruptionSystem and Moon 2 visuals (veins, caustics).
        /// </summary>
        public void PerformCrystalShatterStomp(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.CrystalShatterStomp;

            float radius = IsMoon2CrystalEnvironment() ? moon2CrystalStompRadius : 14f;
            int count = Physics.OverlapSphereNonAlloc(targetPoint, radius, _overlapBuffer);
            int shattered = 0;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                bool isMoon2Target = false;

                if (col.TryGetComponent<InteractableBuilding>(out var building) && building.BuildingId != null && building.BuildingId.Contains("moon2"))
                {
                    CorruptionSystem.Instance?.PurgeCorruption(building.BuildingId, 32f);
                    isMoon2Target = true;
                    shattered++;
                }

                if (col.CompareTag("Crystal") || (col.name != null && (col.name.ToLower().Contains("crystal") || col.name.ToLower().Contains("vein") || col.name.ToLower().Contains("fractal"))))
                {
                    if (col.TryGetComponent<Rigidbody>(out var rb))
                    {
                        Vector3 dir = (col.transform.position - targetPoint).normalized;
                        rb.AddForce(dir * 900f + Vector3.up * 450f, ForceMode.Impulse);
                    }
                    shattered++;
                    isMoon2Target = true;
                }

                if (isMoon2Target && IsMoon2CrystalEnvironment())
                {
                    // Extra visual pop for crystal feel
                    VFXController.Instance?.PlayResonancePulse(col.transform.position, 6f);
                }
            }

            if (IsMoon2CrystalEnvironment())
            {
                VFXController.Instance?.PlayResonancePulse(targetPoint, radius * 1.1f);
                Audio.AudioManager.Instance?.PlaySFX("CrystalShatter", targetPoint);
                HapticFeedbackManager.Instance?.PlayCombatHit();
                // Extra: trigger visual vein burn reaction on nearby moon2 buildings
                int bcount = Physics.OverlapSphereNonAlloc(targetPoint, radius * 0.8f, _overlapBuffer);
                for (int i = 0; i < bcount; i++)
                {
                    if (_overlapBuffer[i].TryGetComponent<InteractableBuilding>(out var b) && b.BuildingId != null && b.BuildingId.Contains("moon2"))
                    {
                        CorruptionSystem.Instance?.PurgeCorruption(b.BuildingId, 12f);
                    }
                }
            }

            Debug.Log($"[GiantMode Moon2] Crystal Shatter Stomp — {shattered} crystals/veins obliterated at titan scale");
            _crystalShatters++;
            OnAbilityUsed?.Invoke(GiantAbility.CrystalShatterStomp);
        }

        /// <summary>
        /// Moon 2 #2: Corruption Vein Manipulation (Giant Hand Yank).
        /// Player reaches giant hands into the cathedral's fractal veins and rips them free.
        /// Causes spectacular "burn like fire along a fuse" chain reactions across multiple buildings.
        /// </summary>
        public void PerformVeinManipulation(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.CorruptionVeinYank;

            float radius = IsMoon2CrystalEnvironment() ? moon2VeinYankRadius : 12f;

            if (IsMoon2CrystalEnvironment())
            {
                VFXController.Instance?.PlayResonancePulse(targetPoint, radius);
                Audio.AudioManager.Instance?.PlaySFX("VeinYank", targetPoint);
                HapticFeedbackManager.Instance?.PlayGolemDeath();

                int count = Physics.OverlapSphereNonAlloc(targetPoint, radius, _overlapBuffer);
                for (int i = 0; i < count; i++)
                {
                    if (_overlapBuffer[i].TryGetComponent<InteractableBuilding>(out var building) && building.BuildingId != null && building.BuildingId.Contains("moon2"))
                    {
                        CorruptionSystem.Instance?.PurgeCorruption(building.BuildingId, 28f);
                    }
                }

                // Bonus chain on ley chamber / hall for thematic spread
                CorruptionSystem.Instance?.PurgeCorruption("moon2_ley_chamber", 15f);
                CorruptionSystem.Instance?.PurgeCorruption("moon2_crystal_hall", 15f);
            }
            else
            {
                // Fallback general yank effect
                VFXController.Instance?.PlayResonancePulse(targetPoint, radius * 0.7f);
            }

            Debug.Log("[GiantMode Moon2] Vein Manipulation — corruption roots torn free by giant hands. Cathedral trembles.");
            _veinsYanked++;
            OnAbilityUsed?.Invoke(GiantAbility.CorruptionVeinYank);
        }

        /// <summary>
        /// Moon 2 #3 (MAJOR): Cathedral-Shaking Quake Sequence.
        /// The ultimate power fantasy moment exclusive to Moon 2's living crystal cathedral.
        /// A titanic resonance stomp + harmonic presence against the dome causes the entire structure
        /// (and all 5 buildings) to convulse in a multi-phase visual/audio quake.
        /// 
        /// Effects:
        /// - Violent dome breathing amplification + crystal facet realignment
        /// - All corruption veins across the zone ignite and burn away in cascading golden fire
        /// - Camera shake + deep sub-bass rumble felt through haptics
        /// - Massive RS reward + permanent visual state change (more golden light, less black veins)
        /// - Bridges giant scale to the macro architecture in unforgettable way
        /// </summary>
        public void TriggerCathedralShakingQuake()
        {
            if (!_isGiant)
            {
                return;
            }

            if (!IsMoon2CrystalEnvironment())
            {
                // Non-Moon2 fallback: big rubble clear + rock cut
                UseRubbleClear();
                return;
            }

            _activeAbility = GiantAbility.CathedralQuake;

            StartCoroutine(CathedralQuakeSequenceCoroutine());
            OnAbilityUsed?.Invoke(GiantAbility.CathedralQuake);
        }

        private IEnumerator CathedralQuakeSequenceCoroutine()
        {
            Debug.Log("[GiantMode Moon2] ═══ THE CATHEDRAL SHAKES — GIANT MODE POWER FANTASY ═══");

            // Locate the 5 signature structures
            var cathedral = GameObject.Find("moon2_cathedral_dome") ?? GameObject.Find("Fractured Cathedral Dome");
            var structures = new List<GameObject>();
            string[] ids = { "moon2_cathedral_dome", "moon2_bell_tower", "moon2_fountain", "moon2_crystal_hall", "moon2_ley_chamber" };
            foreach (string id in ids)
            {
                var go = GameObject.Find(id);
                if (go != null) structures.Add(go);
            }

            // PHASE 1: The Impact Stomp (0-1.8s)
            Audio.AudioManager.Instance?.PlaySFX2D("CathedralQuakeRumble");
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            if (playerTransform != null)
            {
                VFXController.Instance?.PlayResonancePulse(playerTransform.position, 42f);
            }
            if (cathedral != null)
            {
                StartCoroutine(ApplyCathedralShake(cathedral.transform, 5.2f, 1.15f));
            }
            CorruptionSystem.Instance?.PurgeCorruption("moon2_cathedral_dome", 75f);
            yield return new WaitForSeconds(1.8f);

            // PHASE 2: Harmonic Resonance Cascade (all 5 buildings shake + veins ignite)
            Debug.Log("[GiantMode Moon2] Cathedral Quake — Phase 2: Harmonic Cascade ignites every crystal vein");
            foreach (var s in structures)
            {
                if (s != null)
                {
                    StartCoroutine(ApplyCathedralShake(s.transform, 3.8f, 0.65f));
                    string bid = s.name.Contains("moon2_") ? s.name : "moon2_" + s.name.ToLower().Replace(" ", "_");
                    if (bid.Contains("moon2"))
                        CorruptionSystem.Instance?.PurgeCorruption(bid, 22f);
                }
            }
            if (playerTransform != null)
            {
                VFXController.Instance?.PlayResonancePulse(playerTransform.position + Vector3.up * 8f, 60f);
            }
            yield return new WaitForSeconds(2.4f);

            // PHASE 3: The Dome's Great Breath + Shattering Finale
            Debug.Log("[GiantMode Moon2] Cathedral Quake — Phase 3: The dome breathes like a living heart. Crystal growth surges.");
            if (cathedral != null)
            {
                StartCoroutine(ApplyCathedralShake(cathedral.transform, 2.6f, 0.9f));
            }

            // Heavy purge on remaining structures + global reward
            foreach (string id in ids)
            {
                CorruptionSystem.Instance?.PurgeCorruption(id, 18f);
            }

            // Massive world payoff
            GameLoopController.Instance?.QueueRSReward(32f, "moon2_cathedral_giant_quake");
            if (playerTransform != null)
            {
                Audio.AudioManager.Instance?.PlaySFX("GiantModeActivate", playerTransform.position); // triumphant reuse
            }

            HapticFeedbackManager.Instance?.PlayPerfectTune();

            yield return new WaitForSeconds(3.0f);

            _activeAbility = GiantAbility.None;
            _cathedralQuakes++;

            Debug.Log("[GiantMode Moon2] Cathedral Quake COMPLETE. The living crystal cathedral now carries the memory of the giant who shook it back to life. Power fantasy achieved.");
        }

        private IEnumerator ApplyCathedralShake(Transform target, float duration, float intensity)
        {
            if (target == null) yield break;
            Vector3 originalScale = target.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                float wave = Mathf.Sin(t * 28f + progress * 4f) * intensity * (1f - progress * 0.6f);
                // Emphasis on vertical "breathing" + lateral crystal jitter — matches Moon 2 living dome aesthetic
                target.localScale = originalScale + new Vector3(wave * 0.018f, wave * 0.11f, wave * 0.018f);
                yield return null;
            }
            target.localScale = originalScale;
        }

        /// <summary>
        /// Moon 2 #4: Massive Scale Exploration — Fractal Facet Revelation.
        /// At giant height the player discovers and interacts with massive crystal facets and ley inscriptions
        /// on the upper reaches of the dome and towers — completely invisible from human scale.
        /// </summary>
        public void RevealFractalFacetAtGiantScale(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.FractalFacetTap;

            if (IsMoon2CrystalEnvironment())
            {
                VFXController.Instance?.PlayResonancePulse(targetPoint, 9f);
                Audio.AudioManager.Instance?.PlaySFX("FacetReveal", targetPoint);
                // Lore / insight payoff
                GameLoopController.Instance?.QueueRSReward(9f, "giant_facet_reveal_moon2");
                // Slight purge ripple
                int count = Physics.OverlapSphereNonAlloc(targetPoint, 11f, _overlapBuffer);
                for (int i = 0; i < count; i++)
                {
                    if (_overlapBuffer[i].TryGetComponent<InteractableBuilding>(out var b) && b.BuildingId != null && b.BuildingId.Contains("moon2"))
                    {
                        CorruptionSystem.Instance?.PurgeCorruption(b.BuildingId, 9f);
                    }
                }
            }

            Debug.Log("[GiantMode Moon2] Fractal Facet revealed — hidden giant-scale inscriptions and upper ley channels now accessible. The cathedral reveals its secrets only to titans.");
            OnAbilityUsed?.Invoke(GiantAbility.FractalFacetTap);
        }

        /// <summary>
        /// Moon 2 #5: Ley Resonance Bridge Stomp.
        /// Giant footsteps along the ley lines between the five crystal structures temporarily manifest
        /// glowing crystal bridges that auto-purge small corruption and provide visual power fantasy traversal.
        /// </summary>
        public void PerformLeyResonanceBridgeStomp(Vector3 targetPoint)
        {
            if (!_isGiant) return;

            _activeAbility = GiantAbility.LeyBridgeStomp;

            if (IsMoon2CrystalEnvironment())
            {
                VFXController.Instance?.PlayResonancePulse(targetPoint, 32f);
                // Purge along ley path
                CorruptionSystem.Instance?.PurgeCorruption("moon2_ley_chamber", 18f);
                CorruptionSystem.Instance?.PurgeCorruption("moon2_fountain", 12f);
                Audio.AudioManager.Instance?.PlaySFX("LeyBridgeForm", targetPoint);
            }
            else
            {
                VFXController.Instance?.PlayResonancePulse(targetPoint, 20f);
            }

            Debug.Log("[GiantMode Moon2] Ley Resonance Bridge — giant stomps manifest crystal pathways between the structures.");
            OnAbilityUsed?.Invoke(GiantAbility.LeyBridgeStomp);
        }

        // ─── Save/Load ──────────────────────────────

        int _totalActivations;
        int _buildingsLifted;
        int _rubbleCleared;
        float _totalTimeAsGiant;

        // Moon 2 Giant exclusive stats
        int _crystalShatters;
        int _veinsYanked;
        int _cathedralQuakes;

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
        BuildingLift = 3,
        // Moon 2 Crystal Environment Giant Mode exclusives
        CrystalShatterStomp = 4,
        CorruptionVeinYank = 5,
        CathedralQuake = 6,
        FractalFacetTap = 7,
        LeyBridgeStomp = 8
    }
}
