using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Climax Sequence System — orchestrates scripted climactic moments
    /// at the end of each Moon's campaign arc per GDD §11.
    ///
    /// Each Moon climax combines:
    ///   - Multi-phase combat encounter (final boss or wave)
    ///   - Cinematic camera sequences
    ///   - Dialogue beats (NPC revelations, lore drops)
    ///   - Environmental transformations
    ///   - RS reward cascade
    ///   - Moon completion trigger
    ///
    /// Climaxes are triggered when the Moon's final quest objective
    /// completes. They cannot be skipped (but can be paused).
    /// </summary>
    public class ClimaxSequenceSystem : MonoBehaviour
    {
        public static ClimaxSequenceSystem Instance { get; private set; }

        public event Action<int> OnClimaxStarted;    // moonIndex
        public event Action<int> OnClimaxCompleted;   // moonIndex

        [Header("Timing")]
        [SerializeField] float beatPauseDuration = 2f;
        [SerializeField] float cinematicPanDuration = 4f;

        bool _isPlaying;
        int _activeMoonIndex = -1;
        Coroutine _activeSequence;

        readonly Dictionary<int, ClimaxDefinition> _climaxes = new();

        public bool IsPlaying => _isPlaying;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            RegisterClimax(Moon1Climax.Build());
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Register a climax definition for a Moon.
        /// </summary>
        public void RegisterClimax(ClimaxDefinition def)
        {
            if (def == null) return;
            _climaxes[def.moonIndex] = def;
        }

        /// <summary>
        /// Trigger the climax for a given Moon. Called by CampaignFlowController.
        /// </summary>
        public void TriggerClimax(int moonIndex)
        {
            if (_isPlaying) return;
            if (!_climaxes.TryGetValue(moonIndex, out var def))
            {
                Debug.LogWarning($"[Climax] No climax registered for Moon {moonIndex}");
                return;
            }

            _isPlaying = true;
            _activeMoonIndex = moonIndex;
            OnClimaxStarted?.Invoke(moonIndex);

            _activeSequence = StartCoroutine(RunClimax(def));
            Debug.Log($"[Climax] Moon {moonIndex + 1} climax started!");
        }

        /// <summary>
        /// Force-end the active climax (debug/testing only).
        /// </summary>
        public void ForceEnd()
        {
            if (!_isPlaying) return;
            if (_activeSequence != null) StopCoroutine(_activeSequence);
            FinishClimax();
        }

        // ─── Sequence Runner ─────────────────────────

        IEnumerator RunClimax(ClimaxDefinition def)
        {
            // Phase 0: Transition to Cinematic
            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);

            foreach (var beat in def.beats)
            {
                switch (beat.type)
                {
                    case ClimaxBeatType.Dialogue:
                        yield return RunDialogueBeat(beat);
                        break;

                    case ClimaxBeatType.CameraPan:
                        yield return RunCameraBeat(beat);
                        break;

                    case ClimaxBeatType.CombatWave:
                        yield return RunCombatBeat(beat);
                        break;

                    case ClimaxBeatType.VFXBurst:
                        yield return RunVFXBeat(beat);
                        break;

                    case ClimaxBeatType.EnvironmentShift:
                        yield return RunEnvironmentBeat(beat);
                        break;

                    case ClimaxBeatType.RSReward:
                        yield return RunRSRewardBeat(beat);
                        break;

                    case ClimaxBeatType.Wait:
                        yield return new WaitForSeconds(beat.duration);
                        break;
                }

                // Pause between beats
                if (beat.pauseAfter > 0)
                    yield return new WaitForSeconds(beat.pauseAfter);
            }

            FinishClimax();
        }

        IEnumerator RunDialogueBeat(ClimaxBeat beat)
        {
            UI.UIManager.Instance?.ShowDialogue(beat.speaker, beat.dialogueText);
            yield return new WaitForSeconds(beat.duration > 0 ? beat.duration : 4f);
            UI.UIManager.Instance?.HideDialogue();
        }

        IEnumerator RunCameraBeat(ClimaxBeat beat)
        {
            Camera.CameraController cam = FindAnyObjectByType<Camera.CameraController>();
            cam?.FocusOnPoint(beat.worldPosition, beat.duration > 0 ? beat.duration : cinematicPanDuration);
            yield return new WaitForSeconds(beat.duration > 0 ? beat.duration : cinematicPanDuration);
        }

        IEnumerator RunCombatBeat(ClimaxBeat beat)
        {
            // Transition to combat for this beat
            GameStateManager.Instance?.TransitionTo(GameState.Combat);
            AdaptiveMusicController.Instance?.PlayCombatStart();

            if (beat.waveEncounter != null)
            {
                CombatWaveManager.Instance?.StartEncounter(beat.waveEncounter, beat.worldPosition);

                // Wait for encounter to complete
                while (CombatWaveManager.Instance != null && CombatWaveManager.Instance.IsEncounterActive)
                    yield return null;
            }
            else
            {
                yield return new WaitForSeconds(beat.duration > 0 ? beat.duration : 10f);
            }

            // Return to cinematic
            GameStateManager.Instance?.TransitionTo(GameState.Cinematic);
        }

        IEnumerator RunVFXBeat(ClimaxBeat beat)
        {
            switch (beat.vfxType)
            {
                case "discovery":
                    VFXController.Instance?.PlayDiscoveryBurst(beat.worldPosition);
                    break;
                case "emergence":
                    VFXController.Instance?.PlayBuildingEmergence(beat.worldPosition);
                    break;
                case "aether_wake":
                    VFXController.Instance?.TriggerAetherWake();
                    break;
                case "zone_shift":
                    VFXController.Instance?.TriggerZoneShift();
                    break;
                case "zone_complete":
                    VFXController.Instance?.TriggerZoneComplete();
                    break;
            }
            yield return new WaitForSeconds(beat.duration > 0 ? beat.duration : 2f);
        }

        IEnumerator RunEnvironmentBeat(ClimaxBeat beat)
        {
            // Gradual RS shift to transform the environment
            float startRS = AetherFieldManager.Instance?.ResonanceScore ?? 50f;
            float targetRS = beat.floatParam;
            float elapsed = 0f;
            float dur = beat.duration > 0 ? beat.duration : 5f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                float rs = Mathf.Lerp(startRS, targetRS, t);
                VFXController.Instance?.UpdateWorldPalette(rs);
                yield return null;
            }
        }

        IEnumerator RunRSRewardBeat(ClimaxBeat beat)
        {
            GameLoopController.Instance?.QueueRSReward(beat.floatParam, $"climax_moon{_activeMoonIndex + 1}");
            HUDController.Instance?.FlashRSGain(beat.floatParam);
            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            yield return new WaitForSeconds(beat.duration > 0 ? beat.duration : 1.5f);
        }

        void FinishClimax()
        {
            int moonIdx = _activeMoonIndex;
            _isPlaying = false;
            _activeMoonIndex = -1;

            // Announce completion
            HUDController.Instance?.ShowInteractionPrompt($"Moon {moonIdx + 1} Complete!");
            GameStateManager.Instance?.TransitionTo(GameState.Exploration);

            OnClimaxCompleted?.Invoke(moonIdx);

            // Trigger campaign advance
            CampaignFlowController.Instance?.AdvanceToNextMoon();

            Debug.Log($"[Climax] Moon {moonIdx + 1} climax complete.");
        }
    }

    // ─── Data Structures ─────────────────────────

    [Serializable]
    public class ClimaxDefinition
    {
        public int moonIndex;
        public string climaxId;
        public List<ClimaxBeat> beats = new();
    }

    [Serializable]
    public class ClimaxBeat
    {
        public ClimaxBeatType type;
        public string speaker;
        public string dialogueText;
        public Vector3 worldPosition;
        public float duration;
        public float pauseAfter;
        public float floatParam;
        public string vfxType;
        public WaveEncounterDef waveEncounter;
    }

    public enum ClimaxBeatType
    {
        Dialogue = 0,
        CameraPan = 1,
        CombatWave = 2,
        VFXBurst = 3,
        EnvironmentShift = 4,
        RSReward = 5,
        Wait = 6
    }

    // ─── Moon 1 Climax: Echohaven Restored ───────

    /// <summary>
    /// Moon 1 (Echohaven) scripted climax per GDD §11:
    ///   1. Final Mud Golem wave at RS 90+
    ///   2. Anastasia manifestation dialogue
    ///   3. Camera pan of restored Echohaven
    ///   4. Celestial aurora VFX
    ///   5. RS cascade to 100
    ///   6. Moon completion
    /// </summary>
    public static class Moon1Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 0,
                climaxId = "echohaven_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Milo warns about final push
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "The last barrier is crumbling! One more push and Echohaven will sing again!",
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 2: Final golem wave (3 mud golems + 1 fractal wraith)
            var finalWave = new WaveEncounterDef
            {
                encounterId = "echohaven_final",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 5f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.MudGolem, count = 3, healthMultiplier = 1.2f },
                            new() { enemyType = EnemyTypeId.FractalWraith, count = 1, healthMultiplier = 1.0f, spawnDelay = 3f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = Vector3.zero,
                waveEncounter = finalWave,
                pauseAfter = 2f
            });

            // Beat 3: Anastasia speaks
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Anastasia",
                dialogueText = "You did it... I can feel the harmonics returning. The city remembers me, and I... I remember it.",
                duration = 5f,
                pauseAfter = 1.5f
            });

            // Beat 4: Camera pan of restored Echohaven
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, 50, 0), // Overhead panoramic
                duration = 6f,
                pauseAfter = 1f
            });

            // Beat 5: Environment shift to full restoration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f, // Target RS for env shift
                duration = 4f,
                pauseAfter = 0.5f
            });

            // Beat 6: Celestial aurora VFX
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "zone_complete",
                worldPosition = Vector3.up * 30f,
                duration = 3f,
                pauseAfter = 1f
            });

            // Beat 7: RS cascade reward
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 15f, // Bonus RS to push to 100
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 8: Milo celebration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "*howling with pure joy* ECHOHAVEN IS ALIVE! Every dome, every spire, singing in perfect harmony! This is only the beginning!",
                duration = 5f,
                pauseAfter = 2f
            });

            return def;
        }
    }
}
