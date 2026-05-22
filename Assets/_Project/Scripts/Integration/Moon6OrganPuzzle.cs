using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 pipe organ puzzle mechanics.
    /// 12 crystal pipes must be played in specific harmonic sequences.
    /// Three sequences: Foundation (3-pipe), Harmony (5-pipe), Requiem (12-pipe).
    /// </summary>
    public class Moon6OrganPuzzle : MonoBehaviour
    {
        public static Moon6OrganPuzzle Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] int totalPipes = 12;

        [Header("State")]
        [SerializeField] int currentSequence = 0; // 0=Foundation, 1=Harmony, 2=Requiem
        [SerializeField] bool puzzleSolved = false;

        // Harmonic sequences (pipe indices)
        readonly int[][] _sequences = {
            new[] { 0, 4, 7 },                              // Foundation: Root, Fourth, Fifth (C-F-G)
            new[] { 0, 2, 4, 7, 9 },                        // Harmony: Major pentatonic
            new[] { 0, 2, 3, 5, 7, 8, 10, 12, 15, 17, 19, 21 } // Requiem: Full chromatic octave
        };

        readonly List<int> _playerInput = new();
        readonly List<CrystalPipe> _pipes = new();

        public event System.Action OnFoundationComplete;
        public event System.Action OnHarmonyComplete;
        public event System.Action OnRequiemComplete;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterPipe(CrystalPipe pipe)
        {
            _pipes.Add(pipe);
            pipe.OnPlayed += OnPipePlayed;
        }

        void OnPipePlayed(int pipeIndex)
        {
            _playerInput.Add(pipeIndex);

            Debug.Log($"[OrganPuzzle] Pipe {pipeIndex} played. Sequence: [{string.Join(", ", _playerInput)}]");

            // Check if input matches current sequence
            int[] targetSequence = _sequences[currentSequence];

            // Check partial match
            bool matchesSoFar = true;
            for (int i = 0; i < _playerInput.Count; i++)
            {
                if (i >= targetSequence.Length || _playerInput[i] != targetSequence[i])
                {
                    matchesSoFar = false;
                    break;
                }
            }

            if (!matchesSoFar)
            {
                // Wrong note — reset sequence
                Debug.Log("[OrganPuzzle] Wrong note! Sequence reset.");
                GameEvents.RaiseHUDShowObjective("Dissonant note! Try again...");
                Audio.AudioManager.Instance?.PlaySFX2D("OrganWrongNote");
                _playerInput.Clear();
                return;
            }

            // Check complete match
            if (_playerInput.Count == targetSequence.Length)
            {
                OnSequenceComplete();
            }
            else
            {
                // Correct so far, continue
                Audio.AudioManager.Instance?.PlaySFX2D($"OrganPipe_{pipeIndex}");
            }
        }

        void OnSequenceComplete()
        {
            Debug.Log($"[OrganPuzzle] Sequence {currentSequence} complete!");

            switch (currentSequence)
            {
                case 0:
                    GameEvents.RaiseHUDShowObjective("⚡ Foundation Sequence Complete! ⚡");
                    OnFoundationComplete?.Invoke();
                    Audio.AudioManager.Instance?.PlaySFX2D("OrganFoundationComplete");
                    
                    // Progress to Harmony
                    currentSequence = 1;
                    _playerInput.Clear();
                    
                    StartCoroutine(ShowHint("Next: Harmony Sequence (5 pipes)"));
                    break;

                case 1:
                    GameEvents.RaiseHUDShowObjective("⚡ Harmony Sequence Complete! ⚡");
                    OnHarmonyComplete?.Invoke();
                    Audio.AudioManager.Instance?.PlaySFX2D("OrganHarmonyComplete");
                    
                    // Progress to Requiem
                    currentSequence = 2;
                    _playerInput.Clear();
                    
                    StartCoroutine(ShowHint("Final: Cymatic Requiem (12 pipes) — Lirael will guide you"));
                    
                    // Lirael becomes semi-solid
                    LiraelSolidificationController.Instance?.ProgressToSemiSolid();
                    break;

                case 2:
                    GameEvents.RaiseHUDShowObjective("⚡⚡⚡ CYMATIC REQUIEM COMPLETE! ⚡⚡⚡");
                    OnRequiemComplete?.Invoke();
                    Audio.AudioManager.Instance?.PlaySFX2D("OrganRequiemComplete");
                    
                    puzzleSolved = true;
                    
                    // Trigger Lirael full manifestation
                    StartCoroutine(TriggerCymaticRequiemCinematic());
                    break;
            }

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompleteMiniGame, $"organ_sequence_{currentSequence}");
        }

        IEnumerator ShowHint(string hint)
        {
            yield return new WaitForSeconds(2f);
            GameEvents.RaiseHUDShowObjective(hint);
        }

        IEnumerator TriggerCymaticRequiemCinematic()
        {
            yield return new WaitForSeconds(1f);

            Debug.Log("[OrganPuzzle] CYMATIC REQUIEM CINEMATIC: City-wide ionized mist rain, Lirael conducts choir");

            // Lirael becomes fully corporeal
            LiraelSolidificationController.Instance?.ProgressToCorporeal();

            yield return new WaitForSeconds(3f);

            // Quest completion
            QuestManager.Instance?.CompleteQuest("moon6_organ_puzzle");

            // Reveal 9-band purity note
            GameEvents.RaiseHUDShowDialogue("Lirael", "Do you hear it? The ninth band... frozen in the pipes. Zereth's perfect calibration.");

            yield return new WaitForSeconds(2f);

            // Moon 6 arc complete
            Moon6ContentSpawner.Instance?.MarkRevelationUnlocked();
        }

        public int CurrentSequence => currentSequence;
        public bool IsSolved => puzzleSolved;
    }
}
