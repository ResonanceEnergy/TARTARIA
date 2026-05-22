using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Echo garrison NPC dialogue (confused fort soldiers).
    /// Shows fragments of memories about "the commander" (Maelix).
    /// </summary>
    public class EchoGarrisonDialogue : MonoBehaviour, IInteractable
    {
        static readonly string[] _dialogueLines = {
            "The commander... something happened to the commander...",
            "We were holding the line, then... the song... it went WRONG.",
            "Maelix was... he was our strongest. Then the dissonance came.",
            "The moats... they must stay filled. Conductive water, the orders said.",
            "Twelve points. Perfect alignment. That's what held them back."
        };

        int _lineIndex = 0;

        public string GetInteractPrompt() => "[E] Talk to Echo Soldier";

        public void Interact(GameObject player)
        {
            string line = _dialogueLines[_lineIndex % _dialogueLines.Length];
            Debug.Log($"[EchoGarrison] {line}");
            
            HUDController.Instance?.ShowDialogue("Echo Garrison", line);
            Audio.AudioManager.Instance?.PlaySFX2D("Echo_Voice");
            
            _lineIndex++;
        }
    }

    /// <summary>
    /// Inscription trigger for Zereth's message on bastion 0.
    /// "For my brother, the Builder. Hold the line. — Z."
    /// </summary>
    public class InscriptionTrigger : MonoBehaviour, IInteractable
    {
        public string inscriptionText = "";
        bool _hasRead = false;

        public string GetInteractPrompt() => _hasRead ? "Inscription (already read)" : "[E] Read Inscription";

        public void Interact(GameObject player)
        {
            if (string.IsNullOrEmpty(inscriptionText)) return;

            Debug.Log($"[Inscription] {inscriptionText}");
            HUDController.Instance?.ShowDialogue("Ancient Inscription", inscriptionText);
            Audio.AudioManager.Instance?.PlaySFX2D("InscriptionRead");

            if (!_hasRead)
            {
                _hasRead = true;
                QuestManager.Instance?.ProgressByType(QuestObjectiveType.FindLocation, "inscription_zereth");
            }
        }
    }

    /// <summary>
    /// Bastion alignment mechanic — φ-snap golden ratio timing puzzle.
    /// Player must align bastion within φ-timing window (1.618 second window).
    /// </summary>
    public class BastionAlignment : MonoBehaviour, IInteractable
    {
        const float PHI = 1.618033988749f;
        const float ALIGNMENT_WINDOW = PHI; // φ-second timing window
        const float PULSE_INTERVAL = 5f; // Bastions pulse every 5 seconds

        public event System.Action<BastionAlignment> OnAligned;

        bool _isAligned = false;
        float _lastPulseTime = 0f;
        Material _material;
        Color _baseColor = new Color(0.4f, 0.35f, 0.3f);
        Color _pulseColor = new Color(1f, 0.8f, 0.2f);

        void Start()
        {
            _material = GetComponent<Renderer>()?.material;
            _lastPulseTime = Time.time;
        }

        void Update()
        {
            if (_isAligned) return;

            // Pulse visual every PULSE_INTERVAL seconds
            float timeSincePulse = Time.time - _lastPulseTime;
            if (timeSincePulse >= PULSE_INTERVAL)
            {
                _lastPulseTime = Time.time;
                StartCoroutine(PulseEffect());
            }
        }

        System.Collections.IEnumerator PulseEffect()
        {
            if (_material == null) yield break;

            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                float t = elapsed / 0.5f;
                _material.color = Color.Lerp(_baseColor, _pulseColor, Mathf.Sin(t * Mathf.PI));
                elapsed += Time.deltaTime;
                yield return null;
            }
            _material.color = _baseColor;
        }

        public string GetInteractPrompt()
        {
            if (_isAligned) return "Bastion Aligned ✓";
            
            float timeSincePulse = Time.time - _lastPulseTime;
            bool inWindow = timeSincePulse <= ALIGNMENT_WINDOW;
            return inWindow ? "[E] ALIGN NOW! (φ-window)" : "Wait for golden pulse...";
        }

        public void Interact(GameObject player)
        {
            if (_isAligned) return;

            float timeSincePulse = Time.time - _lastPulseTime;
            bool success = timeSincePulse <= ALIGNMENT_WINDOW;

            if (success)
            {
                _isAligned = true;
                
                // Visual: permanent golden glow
                if (_material != null)
                    _material.color = _pulseColor;

                // Spawn golden snap VFX
                GameObject vfx = new GameObject("PhiSnapVFX");
                vfx.transform.position = transform.position + Vector3.up * 2f;
                
                ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.5f;
                main.startSpeed = 2f;
                main.startSize = 0.3f;
                main.loop = false;
                main.maxParticles = 100;

                var emission = ps.emission;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });

                Destroy(vfx, 2f);

                Debug.Log($"[BastionAlignment] SUCCESS! φ-snap alignment achieved (timing: {timeSincePulse:F3}s)");
                OnAligned?.Invoke(this);
            }
            else
            {
                Debug.Log($"[BastionAlignment] MISS! Outside φ-window (timing: {timeSincePulse:F3}s)");
                HUDController.Instance?.ShowObjective("Too early/late! Wait for golden pulse...");
                Audio.AudioManager.Instance?.PlaySFX2D("AlignmentFail");
            }
        }

        /// <summary>
        /// Mark this bastion as aligned (for save/load restoration).
        /// </summary>
        public void MarkAligned()
        {
            _isAligned = true;
            if (_material != null)
                _material.color = _pulseColor;
        }
    }

    /// <summary>
    /// Moat pipe interaction — water flow puzzle.
    /// Player channels pure water through pipe to flood moat segment.
    /// </summary>
    public class MoatPipeInteraction : MonoBehaviour, IInteractable
    {
        public event System.Action<MoatPipeInteraction> OnFlooded;

        bool _isFlooded = false;
        float _floodProgress = 0f;
        const float FLOOD_DURATION = 3f;

        Material _material;
        Color _dryColor = new Color(0.3f, 0.3f, 0.3f);
        Color _floodedColor = new Color(0.2f, 0.5f, 0.8f);

        void Start()
        {
            _material = GetComponent<Renderer>()?.material;
        }

        public string GetInteractPrompt() => _isFlooded ? "Moat Flooded ✓" : "[Hold E] Channel Pure Water";

        public void Interact(GameObject player)
        {
            if (_isFlooded) return;

            // Start flooding process
            StartCoroutine(FloodMoat());
        }

        System.Collections.IEnumerator FloodMoat()
        {
            Debug.Log("[MoatPipe] Flooding moat segment...");
            HUDController.Instance?.ShowObjective("Channeling pure water...");

            float elapsed = 0f;
            while (elapsed < FLOOD_DURATION)
            {
                elapsed += Time.deltaTime;
                _floodProgress = elapsed / FLOOD_DURATION;

                // Visual: pipe fills with water (color shift)
                if (_material != null)
                    _material.color = Color.Lerp(_dryColor, _floodedColor, _floodProgress);

                yield return null;
            }

            _isFlooded = true;
            _floodProgress = 1f;

            // Spawn water flow VFX
            GameObject vfx = new GameObject("WaterFlowVFX");
            vfx.transform.position = transform.position;
            
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startColor = new Color(0.3f, 0.6f, 1f);
            main.loop = false;

            Destroy(vfx, 3f);

            Debug.Log("[MoatPipe] Moat segment flooded!");
            OnFlooded?.Invoke(this);
        }
    }

    /// <summary>
    /// 17-Hour Clock Fragment collectible (Moon 4 revelation).
    /// First hint of Tartarian 17-hour time system.
    /// </summary>
    public class ClockFragmentCollectible : MonoBehaviour, IInteractable
    {
        bool _collected = false;

        public string GetInteractPrompt() => _collected ? "Clock Fragment (collected)" : "[E] Collect 17-Hour Clock Fragment";

        public void Interact(GameObject player)
        {
            if (_collected) return;

            _collected = true;

            Debug.Log("[ClockFragment] 17-Hour Clock Fragment recovered! First hint of Tartarian time system.");
            HUDController.Instance?.ShowObjective("17-Hour Clock Fragment discovered!");
            
            // Add to inventory/quest items
            QuestManager.Instance?.CompleteQuest("moon4_clock_fragment");
            
            // Visual: fragment dissolves into golden light
            Destroy(gameObject, 2f);
        }
    }
}
