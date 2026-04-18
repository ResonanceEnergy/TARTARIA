using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Gameplay;
using Tartaria.UI;
using Tartaria.Audio;

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
        static readonly int DissolveProgressId = Shader.PropertyToID("_DissolveProgress");
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        MaterialPropertyBlock _mpb;

        string _promptCache;
        BuildingRestorationState _promptState = (BuildingRestorationState)255;
        int _promptNodes = -1;

        public string BuildingId => buildingId;
        public BuildingRestorationState State => _state;
        public BuildingDefinition Definition => definition;

        /// <summary>
        /// Inject materials at runtime when InteractableBuilding is added via AddComponent
        /// (SerializeField references are null in that case).
        /// </summary>
        public void SetMaterials(Material mud, Material revealed, Material active)
        {
            if (mudMaterial == null) mudMaterial = mud;
            if (revealedMaterial == null) revealedMaterial = revealed;
            if (activeMaterial == null) activeMaterial = active;
        }

        void Start()
        {
            if (mainRenderer == null)
                mainRenderer = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();

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
            RegisterWithScanner();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (_tuningController != null)
            {
                _tuningController.OnTuningComplete -= OnTuningComplete;
                _tuningController.OnTuningFailed -= OnTuningFailed;
            }
            // Unregister scanner POI
            Gameplay.ResonanceScannerSystem.Instance?.UnregisterPOI(buildingId);
        }

        void RegisterWithScanner()
        {
            var scanner = Gameplay.ResonanceScannerSystem.Instance;
            if (scanner == null || string.IsNullOrEmpty(buildingId)) return;

            scanner.RegisterPOI(new Gameplay.ScanPOI
            {
                poiId = buildingId,
                poiType = Gameplay.ScanPOIType.BuriedStructure,
                position = transform.position,
                isRevealed = _isDiscovered
            });
        }

        // ─── IInteractable Implementation ────────────

        public void Interact(GameObject player)
        {
            switch (_state)
            {
                case BuildingRestorationState.Buried:
                    // Player is trying to interact with buried structure — start excavation
                    if (_isDiscovered)
                    {
                        // Already discovered: auto-reveal via resonance pulse
                        Discover();
                        AudioManager.Instance?.PlaySFX("BuildingReveal", transform.position);
                        if (HUDController.Instance != null)
                            HUDController.Instance.ShowInteractionPrompt(
                                $"The mud crumbles! {GetDisplayName()} is revealed!");
                        DialogueManager.Instance?.PlayContextDialogue(DialogueContext.Discovery);
                    }
                    else
                    {
                        // Not yet discovered — give hint
                        if (HUDController.Instance != null)
                            HUDController.Instance.ShowInteractionPrompt(
                                "Something is buried here... Move closer to investigate.");
                    }
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
                BuildingRestorationState.Buried => _isDiscovered
                    ? $"[E] Excavate {GetDisplayName()}"
                    : "[E] Investigate mound",
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
                AudioManager.Instance?.PlaySFX("BuildingReveal", transform.position);
                UpdateVisuals();
            }

            GameLoopController.Instance?.OnBuildingDiscovered(
                GetDisplayName(), transform.position);
            Save.SaveManager.Instance?.MarkDirty();
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
            AudioManager.Instance?.PlaySFX("TuneSuccess", transform.position);

            // Notify game loop
            GameLoopController.Instance?.OnTuningNodeComplete(buildingId, _nodesCompleted - 1, accuracy);

            // Tutorial: first tuning completion
            TutorialSystem.Instance?.ForceComplete(TutorialStep.Tuning);
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompleteTuning, buildingId);

            // All nodes done?
            if (_nodesCompleted >= 3)
                BeginEmergence();
            else
                GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            UpdateVisuals();
        }

        void OnTuningFailed()
        {
            Debug.Log($"[Building] {GetDisplayName()} tuning failed -- retry available");
            AudioManager.Instance?.PlaySFX("TuneFail", transform.position);
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);
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

            // Material colour waypoints: mud-fresh → mud-cracking → stone
            // (revealedMaterial is M_Mud_Cracking, activeMaterial is M_Stone_Active)
            Color mudColor    = new Color(0.30f, 0.20f, 0.12f);
            Color crackColor  = new Color(0.42f, 0.32f, 0.18f);
            Color stoneColor  = new Color(0.82f, 0.78f, 0.70f);
            Color emitOff     = Color.black;
            Color emitMid     = new Color(0.35f, 0.27f, 0.08f) * 1.2f;
            Color emitFull    = new Color(0.6f,  0.5f,  0.2f)  * 2.5f;

            bool swappedToCracking = false;
            bool swappedToStone    = false;

            // Camera shake + discovery flash at sequence start
            FindAnyObjectByType<Tartaria.Camera.CameraController>()?.TriggerShake(0.35f, 0.5f);
            RuntimeHUDBuilder.Instance?.FlashDiscovery();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Scale up from 30% buried to full
                transform.localScale = new Vector3(
                    initialScale.x,
                    Mathf.Lerp(buriedY, fullY, Mathf.SmoothStep(0f, 1f, t)),
                    initialScale.z);

                // Material swap at milestones
                if (!swappedToCracking && t >= 0.25f)
                {
                    swappedToCracking = true;
                    if (revealedMaterial != null)
                        mainRenderer.sharedMaterial = revealedMaterial;
                }
                if (!swappedToStone && t >= 0.75f)
                {
                    swappedToStone = true;
                    if (activeMaterial != null)
                        mainRenderer.sharedMaterial = activeMaterial;
                }

                // Continuous colour/emission blend via MaterialPropertyBlock
                if (mainRenderer != null)
                {
                    Color baseCol;
                    Color emitCol;
                    if (t < 0.25f)
                    {
                        float lt = t / 0.25f;
                        baseCol = Color.Lerp(mudColor, crackColor, lt);
                        emitCol = Color.Lerp(emitOff, emitMid, lt);
                    }
                    else if (t < 0.75f)
                    {
                        float lt = (t - 0.25f) / 0.5f;
                        baseCol = Color.Lerp(crackColor, stoneColor, lt);
                        emitCol = Color.Lerp(emitMid, emitFull, lt);
                    }
                    else
                    {
                        baseCol = stoneColor;
                        emitCol = emitFull;
                    }

                    mainRenderer.GetPropertyBlock(_mpb);
                    if (mainRenderer.sharedMaterial.HasProperty(BaseColorId))
                        _mpb.SetColor(BaseColorId, baseCol);
                    else if (mainRenderer.sharedMaterial.HasProperty(ColorId))
                        _mpb.SetColor(ColorId, baseCol);
                    _mpb.SetColor(EmissionColorId, emitCol);
                    mainRenderer.SetPropertyBlock(_mpb);
                }

                yield return null;
            }

            CompleteRestoration();
        }

        void CompleteRestoration()
        {
            _state = BuildingRestorationState.Active;
            AudioManager.Instance?.PlaySFX("BuildingActive", transform.position);
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

            // Tutorial: first building fully restored
            TutorialSystem.Instance?.ForceComplete(TutorialStep.BuildingRestore);
            AchievementSystem.Instance?.CheckBuildingRestored(
                ZoneController.Instance?.GetRestoredBuildingCount() ?? 1, allPerfect);
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

            // Failsafe: NEVER allow null material (prevents pink)
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Standard");
                if (shader != null)
                {
                    mat = new Material(shader);
                    mat.SetColor("_BaseColor", new Color(0.35f, 0.25f, 0.15f));
                    mat.SetFloat("_Smoothness", 0.1f);
                    Debug.LogWarning($"[Building] {buildingId}: created failsafe material for state {_state}");
                }
            }

            if (mat != null)
                mainRenderer.sharedMaterial = mat;

            // Ensure emission keyword is on for pulsing
            if (mainRenderer.sharedMaterial != null)
                mainRenderer.sharedMaterial.EnableKeyword("_EMISSION");

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

            mainRenderer.GetPropertyBlock(_mpb);
            if (mainRenderer.sharedMaterial.HasProperty(BaseColorId))
                _mpb.SetColor(BaseColorId, tint);
            else if (mainRenderer.sharedMaterial.HasProperty(ColorId))
                _mpb.SetColor(ColorId, tint);
            mainRenderer.SetPropertyBlock(_mpb);
        }

        /// <summary>Subtle pulsing glow on non-active buildings so player can find them.</summary>
        void Update()
        {
            if (mainRenderer == null || _state == BuildingRestorationState.Active) return;

            // Pulse emission to draw player attention
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f; // 0..1
            Color emissionColor = _state switch
            {
                BuildingRestorationState.Buried => Color.Lerp(Color.black, new Color(0.3f, 0.2f, 0.05f), pulse * 0.4f),
                BuildingRestorationState.Revealed => Color.Lerp(new Color(0.2f, 0.15f, 0f), new Color(0.5f, 0.4f, 0.1f), pulse),
                BuildingRestorationState.Tuning => Color.Lerp(new Color(0.3f, 0.25f, 0f), new Color(0.8f, 0.6f, 0.1f), pulse),
                BuildingRestorationState.Emerging => new Color(0.9f, 0.7f, 0.2f) * pulse,
                _ => Color.black
            };

            mainRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorId, emissionColor);
            mainRenderer.SetPropertyBlock(_mpb);
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
