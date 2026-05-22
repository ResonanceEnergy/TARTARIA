using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Lirael solidification arc controller.
    /// Lirael progresses from spectral → semi-solid → fully corporeal through Moon 6.
    /// Transformation triggered by completing pipe organ restoration + conducting Cymatic Requiem.
    /// </summary>
    public class LiraelSolidificationController : MonoBehaviour
    {
        public static LiraelSolidificationController Instance { get; private set; }

        public enum SolidificationStage
        {
            Spectral,      // Moon 6 start: translucent echo
            SemiSolid,     // After pipe restoration: partially tangible
            Corporeal      // After Cymatic Requiem: fully physical
        }

        [Header("State")]
        [SerializeField] SolidificationStage currentStage = SolidificationStage.Spectral;

        [Header("Spawn")]
        [SerializeField] Vector3 cathedralCenter = new Vector3(300f, -15f, 400f);

        GameObject _liraelNPC;
        Material _liraelMaterial;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnLirael()
        {
            _liraelNPC = new GameObject("Lirael_NPC");
            _liraelNPC.transform.position = cathedralCenter + new Vector3(5f, 0f, 0f);

            // Visual: humanoid capsule
            var filter = _liraelNPC.AddComponent<MeshFilter>();
            filter.mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            
            var renderer = _liraelNPC.AddComponent<MeshRenderer>();
            _liraelMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material = _liraelMaterial;

            _liraelNPC.transform.localScale = new Vector3(0.8f, 2f, 0.8f);

            // Start spectral
            UpdateVisualForStage(SolidificationStage.Spectral);

            // Add dialogue component
            var dialogue = _liraelNPC.AddComponent<LiraelDialogue>();

            Debug.Log("[Lirael] Spawned in spectral form (Moon 6 start).");
        }

        public void ProgressToSemiSolid()
        {
            if (currentStage != SolidificationStage.Spectral) return;

            currentStage = SolidificationStage.SemiSolid;
            UpdateVisualForStage(SolidificationStage.SemiSolid);

            Debug.Log("[Lirael] Solidification: Spectral → Semi-Solid (pipe restoration complete).");
            HUDController.Instance?.ShowObjective("Lirael: 'I can feel the ground... almost.'");

            DialogueManager.Instance?.PlayContextDialogue("lirael_semisolid");
        }

        public void ProgressToCorporeal()
        {
            if (currentStage != SolidificationStage.SemiSolid) return;

            currentStage = SolidificationStage.Corporeal;
            UpdateVisualForStage(SolidificationStage.Corporeal);

            Debug.Log("[Lirael] Solidification COMPLETE: Fully corporeal! (Cymatic Requiem climax)");
            HUDController.Instance?.ShowObjective("⚡ Lirael Solidified! First echo made flesh!");

            DialogueManager.Instance?.PlayContextDialogue("lirael_corporeal");

            // Achievement
            AchievementSystem.Instance?.Unlock("lirael_solidification");

            // Quest completion
            QuestManager.Instance?.CompleteQuest("moon6_lirael_solidification");
        }

        void UpdateVisualForStage(SolidificationStage stage)
        {
            if (_liraelMaterial == null) return;

            switch (stage)
            {
                case SolidificationStage.Spectral:
                    // Translucent pale blue
                    _liraelMaterial.color = new Color(0.7f, 0.8f, 1f, 0.3f);
                    _liraelMaterial.SetFloat("_Surface", 1); // Transparent
                    break;

                case SolidificationStage.SemiSolid:
                    // More opaque, slight warmth
                    _liraelMaterial.color = new Color(0.85f, 0.85f, 0.95f, 0.7f);
                    break;

                case SolidificationStage.Corporeal:
                    // Fully opaque, warm human tones
                    _liraelMaterial.color = new Color(0.95f, 0.85f, 0.8f, 1f);
                    _liraelMaterial.SetFloat("_Surface", 0); // Opaque
                    break;
            }
        }

        public SolidificationStage CurrentStage => currentStage;
    }

    /// <summary>
    /// Lirael dialogue controller.
    /// Dialogue changes based on solidification stage.
    /// </summary>
    public class LiraelDialogue : MonoBehaviour, IInteractable
    {
        int _dialogueIndex = 0;

        readonly string[][] _dialogueByStage = {
            // Spectral
            new[] {
                "I am... was... a choir conductor. The Requiem was my masterwork.",
                "The pipes are broken. The melody plays backwards. It HURTS.",
                "If you can restore the organ... perhaps I can conduct one final time."
            },
            // Semi-Solid
            new[] {
                "I can feel the ground... almost. The pipes sing true now.",
                "The Cymatic Requiem... it requires precision. Every note must resonate.",
                "When I conduct... I will pour everything into it. It may be my last act."
            },
            // Corporeal
            new[] {
                "I'm... solid. Real. After a thousand years of whispers.",
                "The Requiem worked. The city breathes again.",
                "Thank you, Resonance Seeker. I can finally rest... or perhaps, finally BEGIN."
            }
        };

        public string GetInteractPrompt() => "[E] Talk to Lirael";

        public void Interact(GameObject player)
        {
            var controller = LiraelSolidificationController.Instance;
            if (controller == null) return;

            int stageIndex = (int)controller.CurrentStage;
            string[] lines = _dialogueByStage[stageIndex];
            string line = lines[_dialogueIndex % lines.Length];

            Debug.Log($"[Lirael] {line}");
            HUDController.Instance?.ShowDialogue("Lirael", line);

            DialogueManager.Instance?.PlayContextDialogue($"lirael_dialogue_{stageIndex}_{_dialogueIndex}");
            Audio.AudioManager.Instance?.PlaySFX2D("Lirael_Voice");

            _dialogueIndex++;
        }
    }

    /// <summary>
    /// Moon 6 Rhythmic Arc cinematic sequence system.
    /// Handles major story beats: pipe discovery, restoration montage, Cymatic Requiem, revelation.
    /// </summary>
    public class Moon6RhythmicArcCinematics : MonoBehaviour
    {
        public static Moon6RhythmicArcCinematics Instance { get; private set; }

        [Header("Cinematics")]
        [SerializeField] bool discoveryPlayed = false;
        [SerializeField] bool restorationPlayed = false;
        [SerializeField] bool cymaticRequiemPlayed = false;
        [SerializeField] bool revelationPlayed = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Discovery: Sunken cathedral organ plays broken melody.
        /// </summary>
        public void PlayDiscoveryCinematic()
        {
            if (discoveryPlayed) return;
            discoveryPlayed = true;

            Debug.Log("[Moon6Arc] DISCOVERY CINEMATIC: Broken melody summons mud storms.");
            StartCoroutine(DiscoverySequence());
        }

        IEnumerator DiscoverySequence()
        {
            HUDController.Instance?.ShowObjective("The organ's broken melody echoes through the mud...");
            yield return new WaitForSeconds(3f);

            // Camera: pan to organ
            // Audio: distorted backwards melody
            
            HUDController.Instance?.ShowDialogue("Lirael (faint)", "The pipes... they sing in pain...");
            yield return new WaitForSeconds(2f);

            Debug.Log("[Moon6Arc] Discovery cinematic complete.");
        }

        /// <summary>
        /// Restoration: Montage of pipe repairs.
        /// </summary>
        public void PlayRestorationCinematic()
        {
            if (restorationPlayed) return;
            restorationPlayed = true;

            Debug.Log("[Moon6Arc] RESTORATION CINEMATIC: Pipe repair montage.");
            StartCoroutine(RestorationSequence());
        }

        IEnumerator RestorationSequence()
        {
            HUDController.Instance?.ShowObjective("Restoring the cathedral organ...");
            yield return new WaitForSeconds(2f);

            // Montage: each pipe lights up
            HUDController.Instance?.ShowDialogue("Lirael", "Yes... YES! The harmonics return!");
            yield return new WaitForSeconds(2f);

            // Lirael solidifies to semi-solid
            LiraelSolidificationController.Instance?.ProgressToSemiSolid();

            Debug.Log("[Moon6Arc] Restoration cinematic complete.");
        }

        /// <summary>
        /// Climax: Cymatic Requiem performance.
        /// </summary>
        public void PlayCymaticRequiemCinematic()
        {
            if (cymaticRequiemPlayed) return;
            cymaticRequiemPlayed = true;

            Debug.Log("[Moon6Arc] CLIMAX CINEMATIC: Cymatic Requiem! Lirael conducts.");
            StartCoroutine(CymaticRequiemSequence());
        }

        IEnumerator CymaticRequiemSequence()
        {
            HUDController.Instance?.ShowObjective("⚡ THE CYMATIC REQUIEM BEGINS ⚡");
            yield return new WaitForSeconds(2f);

            // Camera: wide shot of cathedral, Lirael conducting
            // Audio: full organ symphony + choir

            HUDController.Instance?.ShowDialogue("Lirael", "For those who could not sing...");
            yield return new WaitForSeconds(3f);

            // VFX: ionized mist rain falls across White City
            // Lirael solidifies to corporeal
            LiraelSolidificationController.Instance?.ProgressToCorporeal();

            yield return new WaitForSeconds(2f);

            Debug.Log("[Moon6Arc] Cymatic Requiem climax complete.");
        }

        /// <summary>
        /// Revelation: 9-band purity note discovered in pipes.
        /// </summary>
        public void PlayRevelationCinematic()
        {
            if (revelationPlayed) return;
            revelationPlayed = true;

            Debug.Log("[Moon6Arc] REVELATION CINEMATIC: 9-band purity frozen note (Zereth's calibration).");
            StartCoroutine(RevelationSequence());
        }

        IEnumerator RevelationSequence()
        {
            HUDController.Instance?.ShowObjective("A perfect note frozen in crystal...");
            yield return new WaitForSeconds(2f);

            HUDController.Instance?.ShowDialogue("Lirael", "Impossible. This note... it's FLAWLESS. No human could calibrate this.");
            yield return new WaitForSeconds(3f);

            HUDController.Instance?.ShowDialogue("Lirael", "The inscription: 'Z'. The Dissonant One... was once a master tuner?");
            yield return new WaitForSeconds(2f);

            Debug.Log("[Moon6Arc] Revelation: Zereth mystery deepens.");
        }
    }
}
