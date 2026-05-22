using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.AI;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.Camera;

namespace Tartaria.Integration
{
    /// <summary>
    /// Boss Dialogue Controller — dramatic pre-fight introductions, mid-fight taunts, defeat lines.
    /// Integrates with companion reactions for emotional weight.
    /// </summary>
    [DisallowMultipleComponent]
    public class BossDialogueController : MonoBehaviour
    {
        [Header("Boss Identity")]
        [SerializeField] string bossName = "Zereth the Dissonant";
        [SerializeField] string bossTitle = "The Architect of Silence";
        [SerializeField, TextArea(3, 8)] string introLine = "You think you can restore what I have buried? Foolish spark. The resonance died for a reason.";

        [Header("Battle Dialogue")]
        [SerializeField, TextArea(2, 4)] string[] midFightTaunts;
        [SerializeField, TextArea(3, 6)] string defeatLine = "The frequency... it remembers. You've retuned the dissonance. Perhaps... I was wrong.";
        [SerializeField, TextArea(3, 6)] string victoryLine = "The old world ends here. Let silence reign.";

        [Header("Phase Triggers")]
        [SerializeField, Range(0f, 1f)] float[] dialogueHealthThresholds = { 0.75f, 0.5f, 0.25f };

        EnemyHealth _health;
        HashSet<int> _triggeredTaunts = new();
        bool _introPlayed;
        bool _outroPlayed;

        void Start()
        {
            _health = GetComponent<EnemyHealth>();
            if (_health != null)
            {
                _health.OnDeath += OnBossDefeated;
            }
        }

        void OnDestroy()
        {
            if (_health != null)
                _health.OnDeath -= OnBossDefeated;
        }

        void Update()
        {
            if (_health == null || _outroPlayed) return;

            // Check health thresholds for mid-fight taunts
            float healthPercent = _health.CurrentHealth / _health.MaxHealth;
            for (int i = 0; i < dialogueHealthThresholds.Length; i++)
            {
                if (healthPercent <= dialogueHealthThresholds[i] && !_triggeredTaunts.Contains(i))
                {
                    _triggeredTaunts.Add(i);
                    if (i < midFightTaunts.Length && !string.IsNullOrEmpty(midFightTaunts[i]))
                        PlayBossTaunt(midFightTaunts[i]);
                }
            }
        }

        /// <summary>
        /// Trigger boss intro (called by combat system or proximity trigger).
        /// </summary>
        public void PlayIntro()
        {
            if (_introPlayed) return;
            _introPlayed = true;

            // Cinematic camera focus on boss (FocusOn method pending implementation)
            // var cinemaCam = FindObjectOfType<Camera.CinematicCameraController>();
            // cinemaCam?.FocusOn(transform, 3f);

            // Show boss nameplate + intro line
            HUDController.Instance?.ShowBossNameplate(bossName, bossTitle);
            HUDController.Instance?.ShowSubtitle($"<b>{bossName}:</b> {introLine}", 6f);
            
            AudioManager.Instance?.PlayVoiceLine($"boss_{bossName.ToLower()}_intro");

            // Companion reactions to boss intro
            TriggerCompanionReactions("boss_intro");

            Debug.Log($"[BossDialogue] {bossName} intro played.");
        }

        void PlayBossTaunt(string tauntLine)
        {
            HUDController.Instance?.ShowSubtitle($"<b>{bossName}:</b> {tauntLine}", 4f);
            AudioManager.Instance?.PlayVoiceLine($"boss_{bossName.ToLower()}_taunt");

            // Companion mid-fight reactions
            TriggerCompanionReactions("boss_taunt");

            Debug.Log($"[BossDialogue] {bossName} taunt: {tauntLine}");
        }

        void OnBossDefeated()
        {
            if (_outroPlayed) return;
            _outroPlayed = true;

            // Play defeat line
            HUDController.Instance?.ShowSubtitle($"<b>{bossName}:</b> {defeatLine}", 8f);
            AudioManager.Instance?.PlayVoiceLine($"boss_{bossName.ToLower()}_defeat");

            // Companion victory reactions
            TriggerCompanionReactions("boss_defeat");

            // Quest progression
            QuestManager.Instance?.ProgressObjective($"defeat_{bossName.ToLower()}", 0, 1);

            Debug.Log($"[BossDialogue] {bossName} defeated — outro played.");
        }

        void TriggerCompanionReactions(string context)
        {
            // Trigger companion dialogue based on context
            var milo = MiloController.Instance;
            var lirael = LiraelController.Instance;
            var thorne = FindObjectOfType<ThorneController>();

            switch (context)
            {
                case "boss_intro":
                    milo?.OnBossEncountered(bossName);
                    lirael?.OnBossEncountered(bossName);
                    DialogueManager.Instance?.PlayContextDialogue($"{bossName.ToLower()}_companion_reaction");
                    break;

                case "boss_taunt":
                    if (Random.value < 0.3f) // 30% chance for companion to respond
                        DialogueManager.Instance?.PlayContextDialogue($"{bossName.ToLower()}_companion_counter");
                    break;

                case "boss_defeat":
                    milo?.OnBossDefeated(bossName);
                    lirael?.OnBossDefeated(bossName);
                    DialogueManager.Instance?.PlayContextDialogue($"{bossName.ToLower()}_companion_victory");
                    break;
            }
        }
    }

    /// <summary>
    /// Enemy Chatter — generic enemy dialogue (idle threats, combat barks, patrol lines).
    /// Adds personality to corruption entities, golems, wraiths.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyChatter : MonoBehaviour
    {
        [Header("Enemy Type")]
        [SerializeField] EnemyType enemyType = EnemyType.MudGolem;
        [SerializeField, TextArea(2, 4)] string[] idleLines;
        [SerializeField, TextArea(2, 4)] string[] combatBarks;
        [SerializeField, TextArea(2, 4)] string[] deathLines;

        enum EnemyType { MudGolem, FractalWraith, CorruptionSentry, DissonantEcho }

        EnemyHealth _health;
        float _lastBarkTime;
        bool _inCombat;

        void Start()
        {
            _health = GetComponent<EnemyHealth>();
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
                _health.OnDamageTaken += OnDamageTaken;
            }
        }

        void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
                _health.OnDamageTaken -= OnDamageTaken;
            }
        }

        void Update()
        {
            // Idle chatter when not in combat
            if (!_inCombat && Time.time - _lastBarkTime > 15f && idleLines.Length > 0)
            {
                if (Random.value < 0.05f) // 5% chance per second
                {
                    PlayChatter(idleLines[Random.Range(0, idleLines.Length)], false);
                    _lastBarkTime = Time.time;
                }
            }
        }

        void OnDamageTaken(float damage)
        {
            _inCombat = true;
            
            // Combat bark (occasionally)
            if (Time.time - _lastBarkTime > 8f && combatBarks.Length > 0 && Random.value < 0.4f)
            {
                PlayChatter(combatBarks[Random.Range(0, combatBarks.Length)], true);
                _lastBarkTime = Time.time;
            }
        }

        void OnDeath()
        {
            // Death line (if not already played)
            if (deathLines.Length > 0)
            {
                PlayChatter(deathLines[Random.Range(0, deathLines.Length)], true);
            }
        }

        void PlayChatter(string line, bool isCombat)
        {
            // Show subtle subtitle (not as prominent as boss dialogue)
            HUDController.Instance?.ShowEnemyBark(line, 2.5f);
            
            // Play audio
            string clipName = isCombat ? $"enemy_{enemyType.ToString().ToLower()}_combat" : $"enemy_{enemyType.ToString().ToLower()}_idle";
            AudioManager.Instance?.PlaySFX3D(clipName, transform.position, 0.6f);

            Debug.Log($"[EnemyChatter] {enemyType}: {line}");
        }
    }

    /// <summary>
    /// Corruption Voice — environmental enemy dialogue (zone-wide threats, warnings).
    /// Represents the Dissonant force itself speaking through corruption.
    /// </summary>
    [DisallowMultipleComponent]
    public class CorruptionVoice : MonoBehaviour
    {
        [Header("Corruption Presence")]
        [SerializeField, Range(0f, 1f)] float corruptionIntensity = 0.5f;
        [SerializeField, TextArea(3, 8)] string[] corruptionWhispers;
        [SerializeField] float whisperInterval = 45f;

        float _lastWhisperTime;

        void Update()
        {
            // Whisper corruption threats periodically (based on intensity)
            if (Time.time - _lastWhisperTime > whisperInterval / corruptionIntensity)
            {
                if (corruptionWhispers.Length > 0 && Random.value < 0.3f)
                {
                    PlayCorruptionWhisper();
                    _lastWhisperTime = Time.time;
                }
            }
        }

        void PlayCorruptionWhisper()
        {
            string whisper = corruptionWhispers[Random.Range(0, corruptionWhispers.Length)];
            
            // Subtle, ominous subtitle (different style from normal dialogue)
            HUDController.Instance?.ShowCorruptionWhisper(whisper, 5f);
            
            // Distorted, reverb-heavy audio
            AudioManager.Instance?.PlaySFX2D("CorruptionWhisper", 0.4f);

            // Visual VFX — screen distortion pulse
            HapticFeedbackManager.Instance?.PlayThreat();

            Debug.Log($"[CorruptionVoice] Whispered: {whisper}");
        }

        public void IncreaseIntensity(float delta)
        {
            corruptionIntensity = Mathf.Clamp01(corruptionIntensity + delta);
        }

        public void DecreaseIntensity(float delta)
        {
            corruptionIntensity = Mathf.Clamp01(corruptionIntensity - delta);
        }
    }
}
