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
            RegisterClimax(Moon2Climax.Build());
            RegisterClimax(Moon3Climax.Build());
            RegisterClimax(Moon4Climax.Build());
            RegisterClimax(Moon5Climax.Build());
            RegisterClimax(Moon6Climax.Build());
            RegisterClimax(Moon7Climax.Build());
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

    // ─── Moon 2 Climax: Fractal Corruption Purge ─

    /// <summary>
    /// Moon 2 (Lunar Moon) scripted climax per GDD §03C / §11:
    ///   1. Massive corruption bloom threatens the Moon 1 cathedral
    ///   2. Bell tower coordination combat
    ///   3. Micro-giant purge of deepest fractal layer
    ///   4. Pre-Flood memory revelation (architect's final moments)
    ///   5. Cathedral organ spontaneous chord — grid-wide resonance
    ///   6. Moon completion
    /// </summary>
    public static class Moon2Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 1,
                climaxId = "fractal_purge_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Cassian warns of corruption bloom
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Cassian",
                dialogueText = "The corruption is converging — it's targeting the cathedral! We need every bell tower ringing NOW!",
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 2: Environment darkens — corruption bloom VFX
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 30f,  // RS drops during crisis
                duration = 3f,
                pauseAfter = 1f
            });

            // Beat 3: Fractal Wraith wave + Mirror Wraith elite
            var purgeWave = new WaveEncounterDef
            {
                encounterId = "corruption_bloom_final",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 8f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.FractalWraith, count = 4, healthMultiplier = 1.3f },
                            new() { enemyType = EnemyTypeId.MirrorWraith, count = 1, healthMultiplier = 1.5f, spawnDelay = 5f }
                        }
                    },
                    new()
                    {
                        waveIndex = 1,
                        rsReward = 10f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.FractalWraith, count = 6, healthMultiplier = 1.4f },
                            new() { enemyType = EnemyTypeId.MirrorWraith, count = 2, healthMultiplier = 1.5f, spawnDelay = 3f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(0, 0, 10),
                waveEncounter = purgeWave,
                pauseAfter = 2f
            });

            // Beat 4: Lirael remembers — pre-Flood architect revelation
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "I can see him... the architect. He's trying to hold the walls together as the sound inverts. He's... singing the stones to sleep.",
                duration = 5f,
                pauseAfter = 1.5f
            });

            // Beat 5: Camera pan into fractal cathedral interior
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, -10, 0),  // Downward into fractal layer
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 6: Organ chord VFX — grid-wide resonance
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "aether_wake",
                worldPosition = Vector3.up * 20f,
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 7: Environment restoration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 5f,
                pauseAfter = 0.5f
            });

            // Beat 8: RS cascade
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 20f,
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 9: Milo and Cassian reaction
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "Did the organ just... play itself? That chord shook the entire grid! I felt it in my teeth!",
                duration = 4f,
                pauseAfter = 2f
            });

            return def;
        }
    }

    // ─── Moon 3 Climax: Orphan Train Escort ──────

    /// <summary>
    /// Moon 3 (Electric Moon) scripted climax per GDD §03C / §11:
    ///   1. Escort the largest spectral train across three zones
    ///   2. Children's 432 Hz lullaby creates golden dome shield
    ///   3. Rail Wraith waves during transit
    ///   4. Dissonance Leviathan boss fight on moving train
    ///   5. Children's lullaby purifies the leviathan → trapped giant echo released
    ///   6. Moon completion → Orphan Settlement expansion
    /// </summary>
    public static class Moon3Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 2,
                climaxId = "orphan_train_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Lirael rallies the children
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "The children are ready. Every echo-child I ever knew is on that train. If we can get them across... they'll sing the highland back to life.",
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 2: Camera pan — spectral train materialises on tracks
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(50, 15, 0),  // Rail overview
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 3: Golden dome VFX — children singing 432 Hz
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "aether_wake",
                worldPosition = new Vector3(50, 5, 0),
                duration = 3f,
                pauseAfter = 0.5f
            });

            // Beat 4: Zone 1 transit — Rail Wraith ambush
            var zone1Wave = new WaveEncounterDef
            {
                encounterId = "train_escort_zone1",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 6f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.RailWraith, count = 4, healthMultiplier = 1.0f },
                            new() { enemyType = EnemyTypeId.DissonanceHarvester, count = 2, healthMultiplier = 1.0f, spawnDelay = 4f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(80, 0, 0),
                waveEncounter = zone1Wave,
                pauseAfter = 1.5f
            });

            // Beat 5: Milo quip between zones
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "One down, two to go! Those kids are singing louder — I think they're actually enjoying this!",
                duration = 3f,
                pauseAfter = 1f
            });

            // Beat 6: Zone 2 transit — heavier wave
            var zone2Wave = new WaveEncounterDef
            {
                encounterId = "train_escort_zone2",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 8f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.RailWraith, count = 6, healthMultiplier = 1.2f },
                            new() { enemyType = EnemyTypeId.DissonanceHarvester, count = 3, healthMultiplier = 1.2f, spawnDelay = 3f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(150, 0, 0),
                waveEncounter = zone2Wave,
                pauseAfter = 1.5f
            });

            // Beat 7: Zone 3 — Dissonance Leviathan boss
            var bossWave = new WaveEncounterDef
            {
                encounterId = "train_escort_leviathan",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 15f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.DissonanceLeviathan, count = 1, healthMultiplier = 2.0f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(220, 0, 0),
                waveEncounter = bossWave,
                pauseAfter = 2f
            });

            // Beat 8: Children's lullaby purifies the leviathan
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "Children — SING! All of you, together! The 432 Hz lullaby — pour everything into it!",
                duration = 4f,
                pauseAfter = 0.5f
            });

            // Beat 9: Purification VFX — leviathan dissolves, giant echo released
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "zone_complete",
                worldPosition = new Vector3(220, 20, 0),
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 10: Giant echo speaks
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Giant Echo",
                dialogueText = "Free... after centuries of dissonance. The children's voices... they broke the cage. Thank you, small ones.",
                duration = 5f,
                pauseAfter = 1.5f
            });

            // Beat 11: Environment restoration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 4f,
                pauseAfter = 0.5f
            });

            // Beat 12: RS cascade
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 25f,
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 13: Milo emotional moment
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "...I'm not crying. It's the ionised mist. Shut up.",
                duration = 3f,
                pauseAfter = 2f
            });

            return def;
        }
    }

    // ─── Moon 4 Climax: Star Fort Siege ──────────

    /// <summary>
    /// Moon 4 (Self-Existing Moon) scripted climax per GDD §11:
    ///   Phase 1: The Breach — corruption breaks through outer wall
    ///   Phase 2: Coordinated Defense — bell tower + companion strategy
    ///   Phase 3: Bell Tower Stand — hold the star points
    ///   Phase 4: Resolution — Star Chamber activation
    /// </summary>
    public static class Moon4Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 3,
                climaxId = "star_fort_siege_climax",
                beats = new List<ClimaxBeat>()
            };

            // Phase 1: The Breach
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Thorne",
                dialogueText = "The outer wall is buckling! Corruption is pouring through the eastern bastion — we need to hold the five points!",
                duration = 4f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 40f,
                duration = 3f,
                pauseAfter = 0.5f
            });

            // Phase 2: Coordinated Defense
            var breachWave = new WaveEncounterDef
            {
                encounterId = "star_fort_breach",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 8f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.SiegeGolem, count = 3, healthMultiplier = 1.3f },
                            new() { enemyType = EnemyTypeId.FractalWraith, count = 4, healthMultiplier = 1.2f, spawnDelay = 2f }
                        }
                    },
                    new()
                    {
                        waveIndex = 1,
                        rsReward = 10f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.SiegeGolem, count = 5, healthMultiplier = 1.5f },
                            new() { enemyType = EnemyTypeId.MirrorWraith, count = 3, healthMultiplier = 1.4f, spawnDelay = 3f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(0, 0, 20),
                waveEncounter = breachWave,
                pauseAfter = 2f
            });

            // Phase 3: Bell Tower Stand
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Cassian",
                dialogueText = "The bell towers are resonating — if we can synchronize all five points, the star formation will amplify our frequency a thousandfold!",
                duration = 5f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "aether_wake",
                worldPosition = new Vector3(0, 30, 0),
                duration = 4f,
                pauseAfter = 1f
            });

            // Phase 4: Resolution — Star Chamber activation
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, 60, 0),
                duration = 6f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 5f,
                pauseAfter = 0.5f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 25f,
                duration = 2f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Thorne",
                dialogueText = "The Star Chamber... it's online. Five frequencies locked into a pentagonal matrix. I've never felt anything like this in my life.",
                duration = 5f,
                pauseAfter = 2f
            });

            return def;
        }
    }

    // ─── Moon 5 Climax: Intercontinental Connection ──

    /// <summary>
    /// Moon 5 (Overtone Moon) scripted climax per GDD §03C:
    ///   1. White City demolition discovery shocks companions
    ///   2. Underground telegraph network activated
    ///   3. Milo's outburst at White City demolition
    ///   4. Intercontinental resonance bridge forms
    /// </summary>
    public static class Moon5Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 4,
                climaxId = "intercontinental_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Discovery of the underwater passage
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "The passage beneath the colosseum... it connects to the resonance stage. The architects built it to amplify across continents.",
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 2: Combat — corruption defends the relay
            var relayWave = new WaveEncounterDef
            {
                encounterId = "intercontinental_relay",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 10f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.MirrorWraith, count = 4, healthMultiplier = 1.4f },
                            new() { enemyType = EnemyTypeId.DissonanceHarvester, count = 3, healthMultiplier = 1.3f, spawnDelay = 3f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(30, -5, 0),
                waveEncounter = relayWave,
                pauseAfter = 2f
            });

            // Beat 3: Milo's emotional outburst
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "They DEMOLISHED it! The White City -- the most beautiful thing humanity ever built -- and they tore it down in a year! Do you understand what that means?! They KNEW what it was!",
                duration = 6f,
                pauseAfter = 2f
            });

            // Beat 4: Camera pan — resonance bridge VFX
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, 40, 0),
                duration = 5f,
                pauseAfter = 1f
            });

            // Beat 5: Bridge activation VFX
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "zone_complete",
                worldPosition = Vector3.up * 25f,
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 6: Environment restoration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 5f,
                pauseAfter = 0.5f
            });

            // Beat 7: RS cascade
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 25f,
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 8: Lirael revelation
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "The bridge is singing. I can feel resonance from... everywhere. Every zone we've restored is connected now. We're not just saving buildings -- we're rebuilding a nervous system.",
                duration = 6f,
                pauseAfter = 2f
            });

            return def;
        }
    }

    // ─── Moon 6 Climax: The Cymatic Requiem ──────

    /// <summary>
    /// Moon 6 (Rhythmic Moon) scripted climax per GDD §03C:
    ///   1. Full 5-register organ symphony
    ///   2. Veritas plays bass and pedal
    ///   3. Lirael sings the solo Silver Passage
    ///   4. Children's choir joins from the nave
    ///   5. Cathedral overload — kaleidoscopic light + ionized mist
    ///   6. Zereth calibration revelation
    /// </summary>
    public static class Moon6Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 5,
                climaxId = "cymatic_requiem_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Veritas at the organ
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Veritas",
                dialogueText = "I have waited four hundred years to finish this piece. Every register is alive. Every pipe remembers its note. Will you conduct while I play?",
                duration = 5f,
                pauseAfter = 1.5f
            });

            // Beat 2: Combat — Harmonic Parasites attack during performance
            var parasiteWave = new WaveEncounterDef
            {
                encounterId = "requiem_parasites",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 8f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.HarmonicParasite, count = 5, healthMultiplier = 1.2f },
                        }
                    },
                    new()
                    {
                        waveIndex = 1,
                        rsReward = 12f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.HarmonicParasite, count = 6, healthMultiplier = 1.4f },
                            new() { enemyType = EnemyTypeId.DissonantConductor, count = 1, healthMultiplier = 2.0f, spawnDelay = 5f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(0, 5, 10),
                waveEncounter = parasiteWave,
                pauseAfter = 2f
            });

            // Beat 3: Lirael's Silver Passage solo
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Lirael",
                dialogueText = "I remember now... my name before. The note between silence and song. THIS note.",
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 4: Children's choir VFX
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "aether_wake",
                worldPosition = new Vector3(0, 15, 0),
                duration = 4f,
                pauseAfter = 1f
            });

            // Beat 5: Cathedral overload — kaleidoscopic light
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, 80, 0),
                duration = 6f,
                pauseAfter = 1f
            });

            // Beat 6: Environment shift to full restoration
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 5f,
                pauseAfter = 0.5f
            });

            // Beat 7: RS cascade
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 30f,
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 8: Zereth revelation
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Veritas",
                dialogueText = "The tuning records... the last calibration was signed 'Z.' The work is flawless. If Zereth was our enemy, why does his calibration ring with such perfect harmony?",
                duration = 6f,
                pauseAfter = 1.5f
            });

            // Beat 9: Milo reaction
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Milo",
                dialogueText = "...okay. That was. Um. That was really something. Don't tell anyone I said that.",
                duration = 3f,
                pauseAfter = 2f
            });

            return def;
        }
    }

    // ─── Moon 7 Climax: Korath's Sacrifice ───────

    /// <summary>
    /// Moon 7 (Resonant Moon) scripted climax per GDD §03C:
    ///   1. Giant stasis thawing — Korath channels RS to free the last giant
    ///   2. Cassian confrontation choice (ally or oppose)
    ///   3. Titan Golem boss wave
    ///   4. Korath channels his life-force into the giant
    ///   5. Giant awakens — environment transforms
    ///   6. Korath's final words
    /// </summary>
    public static class Moon7Climax
    {
        public static ClimaxDefinition Build()
        {
            var def = new ClimaxDefinition
            {
                moonIndex = 6,
                climaxId = "korath_sacrifice_climax",
                beats = new List<ClimaxBeat>()
            };

            // Beat 1: Korath at the stasis chamber
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Korath",
                dialogueText = "The last giant sleeps beneath this citadel. Eight hundred years of stasis — but the ice is singing. It wants to wake. It needs a key made of living resonance.",
                duration = 6f,
                pauseAfter = 1.5f
            });

            // Beat 2: Cassian confrontation
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Cassian",
                dialogueText = "Korath — you can't channel that much RS through a mortal body. The mathematics are clear: the energy required to thaw a giant would consume the source. You know this.",
                duration = 5f,
                pauseAfter = 1.5f
            });

            // Beat 3: Titan Golem boss wave
            var titanWave = new WaveEncounterDef
            {
                encounterId = "korath_sacrifice_titan",
                waves = new List<WaveDefinition>
                {
                    new()
                    {
                        waveIndex = 0,
                        rsReward = 12f,
                        spawns = new List<WaveSpawn>
                        {
                            new() { enemyType = EnemyTypeId.TitanGolem, count = 1, healthMultiplier = 3.0f },
                            new() { enemyType = EnemyTypeId.SiegeGolem, count = 4, healthMultiplier = 1.5f, spawnDelay = 5f }
                        }
                    }
                }
            };
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CombatWave,
                worldPosition = new Vector3(0, 0, 15),
                waveEncounter = titanWave,
                pauseAfter = 2f
            });

            // Beat 4: Korath channels life-force
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.VFXBurst,
                vfxType = "zone_complete",
                worldPosition = new Vector3(0, 20, 15),
                duration = 5f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Korath",
                dialogueText = "Every teaching I gave you... every revelation... was preparation for this moment. The giant cannot wake without a conductor. The conductor cannot survive the current. This is the mathematics of sacrifice.",
                duration = 7f,
                pauseAfter = 2f
            });

            // Beat 5: Environment transformation — giant awakens
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.CameraPan,
                worldPosition = new Vector3(0, 100, 0),
                duration = 7f,
                pauseAfter = 1f
            });

            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.EnvironmentShift,
                floatParam = 100f,
                duration = 5f,
                pauseAfter = 0.5f
            });

            // Beat 6: RS cascade
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.RSReward,
                floatParam = 35f,
                duration = 2f,
                pauseAfter = 1f
            });

            // Beat 7: Korath's final words
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Dialogue,
                speaker = "Korath",
                dialogueText = "The giant breathes. The citadel sings. Remember what I taught you about the Day Out of Time... there is a moment between heartbeats where all things are possible. I go there now.",
                duration = 7f,
                pauseAfter = 2f
            });

            // Beat 8: Milo goes silent
            def.beats.Add(new ClimaxBeat
            {
                type = ClimaxBeatType.Wait,
                duration = 4f,
                pauseAfter = 2f
            });

            return def;
        }
    }
}
