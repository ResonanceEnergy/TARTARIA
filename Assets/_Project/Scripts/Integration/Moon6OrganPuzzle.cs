using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;

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
                HUDController.Instance?.ShowObjective("Dissonant note! Try again...");
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
                    HUDController.Instance?.ShowObjective("⚡ Foundation Sequence Complete! ⚡");
                    OnFoundationComplete?.Invoke();
                    Audio.AudioManager.Instance?.PlaySFX2D("OrganFoundationComplete");
                    
                    // Progress to Harmony
                    currentSequence = 1;
                    _playerInput.Clear();
                    
                    StartCoroutine(ShowHint("Next: Harmony Sequence (5 pipes)"));
                    break;

                case 1:
                    HUDController.Instance?.ShowObjective("⚡ Harmony Sequence Complete! ⚡");
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
                    HUDController.Instance?.ShowObjective("⚡⚡⚡ CYMATIC REQUIEM COMPLETE! ⚡⚡⚡");
                    OnRequiemComplete?.Invoke();
                    Audio.AudioManager.Instance?.PlaySFX2D("OrganRequiemComplete");
                    
                    puzzleSolved = true;
                    
                    // Trigger Lirael full manifestation
                    StartCoroutine(TriggerCymaticRequiemCinematic());
                    break;
            }

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.Interact, $"organ_sequence_{currentSequence}");
        }

        IEnumerator ShowHint(string hint)
        {
            yield return new WaitForSeconds(2f);
            HUDController.Instance?.ShowObjective(hint);
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
            HUDController.Instance?.ShowDialogue("Lirael", "Do you hear it? The ninth band... frozen in the pipes. Zereth's perfect calibration.");

            yield return new WaitForSeconds(2f);

            // Moon 6 arc complete
            Moon6ContentSpawner.Instance?.MarkRevelationUnlocked();
        }

        public int CurrentSequence => currentSequence;
        public bool IsSolved => puzzleSolved;
    }

    /// <summary>
    /// Individual crystal pipe interactable.
    /// </summary>
    public class CrystalPipe : MonoBehaviour, IInteractable
    {
        public int pipeIndex;
        public event System.Action<int> OnPlayed;

        [SerializeField] bool isRepaired = false;

        Material _material;
        Color _brokenColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        Color _repairedColor = new Color(0.5f, 0.8f, 1f, 0.9f);

        void Start()
        {
            _material = GetComponent<Renderer>()?.material;
            UpdateVisual();

            // Register with puzzle controller
            Moon6OrganPuzzle.Instance?.RegisterPipe(this);
        }

        public string GetInteractPrompt()
        {
            if (!isRepaired) return "[E] Repair Crystal Pipe";
            return $"[E] Play Pipe {pipeIndex}";
        }

        public void Interact(GameObject player)
        {
            if (!isRepaired)
            {
                RepairPipe();
            }
            else
            {
                PlayPipe();
            }
        }

        void RepairPipe()
        {
            isRepaired = true;
            UpdateVisual();

            Debug.Log($"[CrystalPipe {pipeIndex}] Repaired!");
            HUDController.Instance?.ShowObjective($"Pipe {pipeIndex} repaired!");

            Audio.AudioManager.Instance?.PlaySFX2D("PipeRepair");

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.Interact, $"repair_pipe_{pipeIndex}");
        }

        void PlayPipe()
        {
            Debug.Log($"[CrystalPipe {pipeIndex}] Playing note...");

            // Visual: pipe glows briefly
            StartCoroutine(PlayGlowEffect());

            // Audio: harmonic tone
            Audio.AudioManager.Instance?.PlaySFX2D($"OrganPipe_{pipeIndex}");

            // Notify puzzle controller
            OnPlayed?.Invoke(pipeIndex);
        }

        IEnumerator PlayGlowEffect()
        {
            if (_material == null) yield break;

            Color glowColor = new Color(1f, 0.9f, 0.5f);
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _material.color = Color.Lerp(glowColor, _repairedColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _material.color = _repairedColor;
        }

        void UpdateVisual()
        {
            if (_material == null) return;
            _material.color = isRepaired ? _repairedColor : _brokenColor;
        }

        public void MarkRepaired()
        {
            isRepaired = true;
            UpdateVisual();
        }
    }

    /// <summary>
    /// Hydraulic fountain that feeds organ bellows.
    /// </summary>
    public class HydraulicFountain : MonoBehaviour, IInteractable
    {
        public int fountainIndex;
        public event System.Action<HydraulicFountain> OnRestored;

        [SerializeField] bool isRestored = false;

        Material _material;
        Color _dryColor = new Color(0.4f, 0.4f, 0.4f);
        Color _flowingColor = new Color(0.3f, 0.6f, 0.9f);

        void Start()
        {
            _material = GetComponent<Renderer>()?.material;
            UpdateVisual();
        }

        public string GetInteractPrompt()
        {
            if (isRestored) return "Fountain Flowing ✓";
            return $"[E] Restore Fountain {fountainIndex + 1}";
        }

        public void Interact(GameObject player)
        {
            if (isRestored) return;

            RestoreFountain();
        }

        void RestoreFountain()
        {
            isRestored = true;
            UpdateVisual();

            Debug.Log($"[HydraulicFountain {fountainIndex}] Restored! Water flows to organ bellows.");
            HUDController.Instance?.ShowObjective($"Fountain {fountainIndex + 1} restored!");

            Audio.AudioManager.Instance?.PlaySFX2D("FountainRestore");

            OnRestored?.Invoke(this);

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.Interact, $"restore_fountain_{fountainIndex}");
        }

        void UpdateVisual()
        {
            if (_material == null) return;
            _material.color = isRestored ? _flowingColor : _dryColor;
        }

        public void MarkRestored()
        {
            isRestored = true;
            UpdateVisual();
        }
    }
}
