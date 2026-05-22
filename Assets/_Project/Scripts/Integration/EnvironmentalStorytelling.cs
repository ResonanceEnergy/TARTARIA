using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Plaque Readable — environmental storytelling plaques (bronze, marble, stone).
    /// Provides historical lore, architectural notes, memorials, warnings.
    /// Grants codex unlocks and minor Resonance rewards on first read.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlaqueReadable : MonoBehaviour, IInteractable
    {
        [Header("Plaque Content")]
        [SerializeField] string plaqueId = "plaque_echohaven_dome";
        [SerializeField, TextArea(4, 12)] string plaqueText = "In the year 1876, this dome resonated at 432 Hz, healing the land for 50 miles. Let no corruption silence its song.";
        [SerializeField] string plaqueTitle = "The Listeners' Hall";
        
        [Header("Rewards")]
        [SerializeField] string codexUnlockId;
        [SerializeField] float resonanceReward = 50f;
        [SerializeField] bool oneTimeRead = true;

        bool _hasRead;

        public string GetInteractPrompt() => _hasRead && oneTimeRead ? null : "[E] Read Plaque";

        public void Interact(GameObject player)
        {
            if (_hasRead && oneTimeRead) return;

            // Show plaque UI with title + body text
            HUDController.Instance?.ShowLorePopup(plaqueTitle, plaqueText);
            
            // Play discovery audio
            AudioManager.Instance?.PlaySFX2D("PlaqueRead");

            if (!_hasRead)
            {
                _hasRead = true;
                
                // Grant rewards
                if (resonanceReward > 0f)
                {
                    PlayerCharacter.Instance?.AddRS(resonanceReward);
                    HUDController.Instance?.ShowAchievementToast($"+{resonanceReward:F0} RS — Lore Discovery");
                }

                // Unlock codex entry
                if (!string.IsNullOrEmpty(codexUnlockId))
                    UI.CodexSystem.Instance?.UnlockEntry(codexUnlockId);

                // Save progress
                Save.SaveManager.Instance?.MarkDirty();
                
                Debug.Log($"[PlaqueReadable] Read: {plaqueId} — {plaqueTitle}");
            }
        }
    }

    /// <summary>
    /// Readable Note — scattered documents (letters, journals, survey reports, warnings).
    /// Provides character perspectives, quest hints, emotional beats.
    /// </summary>
    [DisallowMultipleComponent]
    public class ReadableNote : MonoBehaviour, IInteractable
    {
        [Header("Note Content")]
        [SerializeField] string noteId = "note_surveyor_last_entry";
        [SerializeField] string noteTitle = "Surveyor's Journal — Final Entry";
        [SerializeField, TextArea(4, 12)] string noteBody = "Day 47: The frequency has inverted. Resonance readings show -432 Hz. This is not natural. Evacuating to the spire. If you find this, DO NOT activate the fountain without purifying the corruption first. — Marcus";
        [SerializeField] string authorName = "Marcus Thorne";
        
        [Header("Rewards")]
        [SerializeField] string questHintId;
        [SerializeField] string codexUnlockId;

        bool _hasRead;

        public string GetInteractPrompt() => _hasRead ? null : "[E] Read Note";

        public void Interact(GameObject player)
        {
            if (_hasRead) return;
            _hasRead = true;

            // Show note UI (different visual style from plaque — parchment/paper texture)
            string fullText = string.IsNullOrEmpty(authorName) 
                ? noteBody 
                : $"{noteBody}\n\n— {authorName}";
            
            HUDController.Instance?.ShowLorePopup(noteTitle, fullText);
            AudioManager.Instance?.PlaySFX2D("PaperRustle");

            // Grant quest hint if specified
            if (!string.IsNullOrEmpty(questHintId))
                QuestManager.Instance?.ActivateQuest(questHintId);

            // Unlock codex
            if (!string.IsNullOrEmpty(codexUnlockId))
                UI.CodexSystem.Instance?.UnlockEntry(codexUnlockId);

            Save.SaveManager.Instance?.MarkDirty();
            Debug.Log($"[ReadableNote] Read: {noteId} — {noteTitle}");
        }
    }

    /// <summary>
    /// Audio Log — crystal recordings, echo memories, Tartarian voice fragments.
    /// Plays audio with optional subtitle overlay. High emotional impact.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioLogPlayable : MonoBehaviour, IInteractable
    {
        [Header("Audio Log")]
        [SerializeField] string logId = "audiolog_lirael_cathedral_memory";
        [SerializeField] string logTitle = "Echo Memory — Cathedral Dedication";
        [SerializeField, TextArea(3, 8)] string subtitleText = "[Lirael's voice, young and full of wonder] The cathedral sings today! All 12 spires in harmony! The giants taught us this frequency... I'll remember it forever...";
        [SerializeField] string audioClipName = "AudioLog_LiraelCathedral";
        [SerializeField] float logDuration = 12f;
        
        [Header("Rewards")]
        [SerializeField] string codexUnlockId;
        [SerializeField] float resonanceReward = 100f;

        bool _hasPlayed;

        public string GetInteractPrompt() => _hasPlayed ? "[E] Replay Memory" : "[E] Play Echo Memory";

        public void Interact(GameObject player)
        {
            // Play audio log
            AudioManager.Instance?.PlayVoiceLine(audioClipName, 1f);
            
            // Show subtitle overlay (stays on screen for duration)
            HUDController.Instance?.ShowSubtitle(subtitleText, logDuration);
            
            // Atmospheric VFX — crystalline glow, echo ripples
            HapticFeedbackManager.Instance?.PlayContextual();

            if (!_hasPlayed)
            {
                _hasPlayed = true;

                // Grant rewards
                if (resonanceReward > 0f)
                    PlayerCharacter.Instance?.AddRS(resonanceReward);

                if (!string.IsNullOrEmpty(codexUnlockId))
                    UI.CodexSystem.Instance?.UnlockEntry(codexUnlockId);

                // Track in companion memory systems
                CompanionDialogueArcs.Instance?.OnAudioLogPlayed(logId);

                Save.SaveManager.Instance?.MarkDirty();
                Debug.Log($"[AudioLogPlayable] Played: {logId} — {logTitle}");
            }
        }
    }

    /// <summary>
    /// Inscription Stone — ancient Tartarian glyphs on standing stones.
    /// Requires Lirael companion present to translate. Unlocks Old Tartarian vocabulary.
    /// </summary>
    [DisallowMultipleComponent]
    public class InscriptionStone : MonoBehaviour, IInteractable
    {
        [Header("Inscription")]
        [SerializeField] string inscriptionId = "inscription_3_6_9_principle";
        [SerializeField, TextArea(3, 8)] string oldTartarianText = "⌘ ∴ ᚦ ∴ ⊕ ∴ ᚦ ⊕ ⌘";
        [SerializeField, TextArea(3, 8)] string translatedText = "Three, Six, Nine — the spiral of creation. Frequency governs form. Form echoes frequency. The universe is vibration incarnate.";
        [SerializeField] string vocabularyUnlock = "oldtartarian_principle_369";

        bool _hasRead;

        public string GetInteractPrompt()
        {
            if (_hasRead) return null;
            
            // Check if Lirael is present/unlocked
            var lirael = LiraelController.Instance;
            if (lirael == null || !CompanionManager.Instance.IsCompanionUnlocked("lirael"))
                return "[E] Ancient Inscription (Translation Required)";
            
            return "[E] Read Inscription (Lirael will translate)";
        }

        public void Interact(GameObject player)
        {
            if (_hasRead) return;

            var lirael = LiraelController.Instance;
            bool canTranslate = lirael != null && CompanionManager.Instance.IsCompanionUnlocked("lirael");

            if (!canTranslate)
            {
                // Show untranslated glyphs
                HUDController.Instance?.ShowLorePopup("Ancient Inscription", 
                    $"{oldTartarianText}\n\n<i>These glyphs are Old Tartarian. You need someone who can read them...</i>");
                AudioManager.Instance?.PlaySFX2D("StoneTouch");
                return;
            }

            _hasRead = true;

            // Lirael translates
            DialogueManager.Instance?.PlayLineById("lirael_inscription_reading");
            
            // Show translated text
            HUDController.Instance?.ShowLorePopup("Ancient Inscription — Translated", 
                $"<color=#888>{oldTartarianText}</color>\n\n{translatedText}");
            
            AudioManager.Instance?.PlaySFX2D("InscriptionChime");

            // Unlock vocabulary in codex
            if (!string.IsNullOrEmpty(vocabularyUnlock))
                UI.CodexSystem.Instance?.UnlockEntry(vocabularyUnlock);

            // Grant Lirael trust
            lirael?.AddTrust(5f);

            Save.SaveManager.Instance?.MarkDirty();
            Debug.Log($"[InscriptionStone] Translated: {inscriptionId}");
        }
    }
}
