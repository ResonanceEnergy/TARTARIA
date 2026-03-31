using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Gameplay;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// MonoBehaviour bridge for Tartarian buildings — implements IInteractable
    /// so the player can discover, interact with, and tune buildings.
    ///
    /// Attached to building GameObjects in the scene. Manages the visual
    /// state machine (buried→revealed→tuning→emerging→active) on the
    /// MonoBehaviour side while the ECS BuildingRestorationSystem handles data.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableBuilding : MonoBehaviour, IInteractable
    {
        [Header("Building Config")]
        [SerializeField] BuildingDefinition definition;
        [SerializeField] string buildingId;

        [Header("Visual References")]
        [SerializeField] MeshRenderer mainRenderer;
        [SerializeField] Material mudMaterial;
        [SerializeField] Material revealedMaterial;
        [SerializeField] Material activeMaterial;

        [Header("Tuning")]
        [SerializeField] Transform[] tuningNodePositions;

        BuildingRestorationState _state = BuildingRestorationState.Buried;
        int _nodesCompleted;
        float[] _nodeAccuracies = new float[3];
        bool _isDiscovered;
        TuningMiniGameController _tuningController;

        string _promptCache;
        BuildingRestorationState _promptState = (BuildingRestorationState)(-1);
        int _promptNodes = -1;

        public string BuildingId => buildingId;
        public BuildingRestorationState State => _state;
        public BuildingDefinition Definition => definition;

        void Start()
        {
            if (mainRenderer == null)
                mainRenderer = GetComponent<MeshRenderer>();

            // Find or create tuning controller
            _tuningController = GetComponent<TuningMiniGameController>();
            if (_tuningController == null)
                _tuningController = gameObject.AddComponent<TuningMiniGameController>();

            _tuningController.OnTuningComplete += OnTuningComplete;
            _tuningController.OnTuningFailed += OnTuningFailed;

            // Set initial layer
            gameObject.layer = LayerMask.NameToLayer("Building") >= 0
                ? LayerMask.NameToLayer("Building") : 0;

            UpdateVisuals();
            RestoreFromSave();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (_tuningController != null)
            {
                _tuningController.OnTuningComplete -= OnTuningComplete;
                _tuningController.OnTuningFailed -= OnTuningFailed;
            }
        }

        // ─── IInteractable Implementation ────────────

        public void Interact(GameObject player)
        {
            switch (_state)
            {
                case BuildingRestorationState.Buried:
                    // Can't interact until discovered (proximity trigger)
                    if (HUDController.Instance != null)
                        HUDController.Instance.ShowInteractionPrompt("This structure is buried deep...");
                    break;

                case BuildingRestorationState.Revealed:
                    // Start tuning on first node
                    StartTuning();
                    break;

                case BuildingRestorationState.Tuning:
                    // Continue tuning next node
                    StartTuning();
                    break;

                case BuildingRestorationState.Emerging:
                    // Building is emerging — can't interact
                    if (HUDController.Instance != null)
                        HUDController.Instance.ShowInteractionPrompt("The structure is awakening...");
                    break;

                case BuildingRestorationState.Active:
                    // Fully restored — enter micro mode if available, otherwise show lore
                    if (MicroGiantController.Instance != null && !MicroGiantController.Instance.IsMicro)
                    {
                        float aether = EconomySystem.Instance != null
                            ? EconomySystem.Instance.GetBalance(Core.CurrencyType.AetherShards)
                            : 0f;
                        if (aether > 0f)
                        {
                            MicroGiantController.Instance.EnterMicroMode(
                                buildingId, transform.position, aether);
                            break;
                        }
                    }
                    UIManager.Instance?.ShowDialogue(
                        definition != null ? definition.buildingName : buildingId,
                        definition != null ? definition.loreDescription : "An ancient structure, now restored.");
                    break;
            }
        }

        public string GetInteractPrompt()
        {
            if (_state == _promptState && _nodesCompleted == _promptNodes)
                return _promptCache;

            _promptState = _state;
            _promptNodes = _nodesCompleted;
            _promptCache = _state switch
            {
                BuildingRestorationState.Buried => "",
                BuildingRestorationState.Revealed => $"[E] Tune {GetDisplayName()} (Node {_nodesCompleted + 1}/3)",
                BuildingRestorationState.Tuning => $"[E] Tune {GetDisplayName()} (Node {_nodesCompleted + 1}/3)",
                BuildingRestorationState.Emerging => "Restoration in progress...",
                BuildingRestorationState.Active => $"[E] Examine {GetDisplayName()}",
                _ => ""
            };
            return _promptCache;
        }

        // ─── Discovery ───────────────────────────────

        /// <summary>
        /// Called by ZoneController or discovery trigger when player approaches.
        /// </summary>
        public void Discover()
        {
            if (_isDiscovered) return;
            _isDiscovered = true;

            if (_state == BuildingRestorationState.Buried)
            {
                _state = BuildingRestorationState.Revealed;
                UpdateVisuals();
            }

            GameLoopController.Instance?.OnBuildingDiscovered(
                GetDisplayName(), transform.position);
        }

        // ─── Tuning ──────────────────────────────────

        void StartTuning()
        {
            if (_nodesCompleted >= 3)
            {
                BeginEmergence();
                return;
            }

            if (definition == null || definition.nodePuzzles == null ||
                _nodesCompleted >= definition.nodePuzzles.Length)
            {
                // Fallback default config
                _tuningController.StartTuning(new TuningPuzzleConfig
                {
                    variant = (TuningVariant)(_nodesCompleted % 3),
                    targetFrequency = 432f,
                    timeLimitSeconds = 15f,
                    tolerancePercent = 0.08f,
                    difficultySpeed = 0.3f + _nodesCompleted * 0.1f
                });
            }
            else
            {
                _tuningController.StartTuning(definition.nodePuzzles[_nodesCompleted]);
            }

            _state = BuildingRestorationState.Tuning;
        }

        void OnTuningComplete(float accuracy)
        {
            _nodeAccuracies[_nodesCompleted] = accuracy;
            _nodesCompleted++;

            string tier = TuningMiniGameController.GetAccuracyTier(accuracy);
            Debug.Log($"[Building] {GetDisplayName()} node {_nodesCompleted}/3 complete — {tier} ({accuracy:P0})");

            // Notify game loop
            GameLoopController.Instance?.OnTuningNodeComplete(buildingId, _nodesCompleted - 1, accuracy);

            // All nodes done?
            if (_nodesCompleted >= 3)
                BeginEmergence();
            else
                GameStateManager.Instance.TransitionTo(GameState.Exploration);

            UpdateVisuals();
        }

        void OnTuningFailed()
        {
            Debug.Log($"[Building] {GetDisplayName()} tuning failed — retry available");
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
            DialogueManager.Instance?.PlayContextDialogue("tuning_fail");
        }

        // ─── Emergence & Restoration ─────────────────

        void BeginEmergence()
        {
            _state = BuildingRestorationState.Emerging;
            UpdateVisuals();

            // 5-second dissolution sequence (modified by repair speed skill)
            float duration = definition != null ? definition.dissolutionDuration : 5f;
            float repairMod = Gameplay.SkillTreeSystem.Instance?.GetModifier(Gameplay.SkillModifierType.RepairSpeed) ?? 0f;
            duration /= (1f + repairMod); // Higher repair speed = shorter emergence
            StartCoroutine(EmergenceSequence(duration));
        }

        System.Collections.IEnumerator EmergenceSequence(float duration)
        {
            float elapsed = 0f;
            Vector3 initialScale = transform.localScale;
            float buriedY = initialScale.y * 0.3f;
            float fullY = initialScale.y;

            // Animate mud dissolution via material property
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                if (mainRenderer != null && mainRenderer.material.HasProperty("_DissolveProgress"))
                    mainRenderer.material.SetFloat("_DissolveProgress", progress);

                // Scale up building from buried (30%) to full height
                transform.localScale = new Vector3(
                    initialScale.x,
                    Mathf.Lerp(buriedY, fullY, progress),
                    initialScale.z);

                yield return null;
            }

            CompleteRestoration();
        }

        void CompleteRestoration()
        {
            _state = BuildingRestorationState.Active;
            UpdateVisuals();

            // Register building for passive income
            EconomySystem.Instance?.RegisterBuilding(
                buildingId, definition != null ? definition.baseIncome : 10);

            bool allPerfect = true;
            for (int i = 0; i < 3; i++)
            {
                if (_nodeAccuracies[i] < 0.95f) { allPerfect = false; break; }
            }

            GameLoopController.Instance?.OnBuildingRestored(
                GetDisplayName(), transform.position, allPerfect);
        }

        // ─── Visuals ─────────────────────────────────

        void UpdateVisuals()
        {
            if (mainRenderer == null) return;

            Material mat = _state switch
            {
                BuildingRestorationState.Buried => mudMaterial,
                BuildingRestorationState.Revealed => revealedMaterial,
                BuildingRestorationState.Active => activeMaterial,
                _ => mainRenderer.sharedMaterial
            };

            if (mat != null)
                mainRenderer.material = mat;

            // Color tint based on state
            Color tint = _state switch
            {
                BuildingRestorationState.Buried => new Color(0.35f, 0.25f, 0.15f),
                BuildingRestorationState.Revealed => new Color(0.6f, 0.5f, 0.3f),
                BuildingRestorationState.Tuning => new Color(0.8f, 0.7f, 0.3f),
                BuildingRestorationState.Emerging => new Color(0.9f, 0.85f, 0.5f),
                BuildingRestorationState.Active => new Color(1f, 0.95f, 0.7f),
                _ => Color.white
            };

            if (mainRenderer.material.HasProperty("_BaseColor"))
                mainRenderer.material.SetColor("_BaseColor", tint);
            else if (mainRenderer.material.HasProperty("_Color"))
                mainRenderer.material.color = tint;
        }

        // ─── Save/Load ──────────────────────────────

        void RestoreFromSave()
        {
            var save = Save.SaveManager.Instance;
            if (save?.CurrentSave?.world?.buildings == null) return;

            foreach (var bs in save.CurrentSave.world.buildings)
            {
                if (bs.buildingId == buildingId)
                {
                    _state = (BuildingRestorationState)bs.state;
                    _nodesCompleted = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        _nodeAccuracies[i] = bs.nodeAccuracy[i];
                        if (bs.nodesComplete[i]) _nodesCompleted++;
                    }
                    _isDiscovered = _state != BuildingRestorationState.Buried;
                    UpdateVisuals();
                    return;
                }
            }
        }

        /// <summary>
        /// Writes current state to the save data structure.
        /// Called by GameLoopController during save sync.
        /// </summary>
        public Save.BuildingSaveState ToSaveState()
        {
            return new Save.BuildingSaveState
            {
                buildingId = buildingId,
                state = (int)_state,
                nodesComplete = new[] {
                    _nodesCompleted > 0,
                    _nodesCompleted > 1,
                    _nodesCompleted > 2
                },
                nodeAccuracy = (float[])_nodeAccuracies.Clone(),
                restorationProgress = _state == BuildingRestorationState.Active ? 1f : 0f
            };
        }

        string GetDisplayName()
        {
            return definition != null ? definition.buildingName : buildingId;
        }

        // ─── Mini-Game Bonus Reception ───────────────

        /// <summary>
        /// Apply an RS bonus from a nearby mini-game completion.
        /// Active buildings gain a resonance amplification boost;
        /// tuning-in-progress buildings get node accuracy nudge.
        /// </summary>
        public void ReceiveMiniGameBonus(float rsBonus, string miniGameType)
        {
            if (_state == BuildingRestorationState.Buried) return;

            if (_state == BuildingRestorationState.Active)
            {
                // Active buildings: queue RS reward through game loop
                GameLoopController.Instance?.QueueRSReward(
                    rsBonus * 0.2f, $"building_bonus_{miniGameType}");
            }
            else if (_state == BuildingRestorationState.Tuning && _nodesCompleted > 0)
            {
                // In-progress buildings: small accuracy boost on last node
                int lastNode = _nodesCompleted - 1;
                _nodeAccuracies[lastNode] = Mathf.Min(_nodeAccuracies[lastNode] + 0.05f, 1f);
            }

            Debug.Log($"[Building] {GetDisplayName()} received {miniGameType} bonus: {rsBonus:F1} RS");
        }
    }
}
