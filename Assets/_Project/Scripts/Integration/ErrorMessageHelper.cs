using System;
using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Error Message Helper — provides contextual error messages with recovery hints.
    /// Replaces generic "Error" messages with helpful, actionable feedback.
    /// 
    /// Usage:
    ///   ErrorMessageHelper.ShowInventoryFull(currentCount, maxSlots);
    ///   ErrorMessageHelper.ShowQuestPrerequisiteMissing(questId, prerequisiteId);
    ///   ErrorMessageHelper.ShowSaveFailed(reason);
    /// 
    /// All messages route through HUDController for consistent display.
    /// </summary>
    public static class ErrorMessageHelper
    {
        /// <summary>
        /// Show inventory full error with capacity info.
        /// </summary>
        public static void ShowInventoryFull(int currentCount, int maxSlots)
        {
            string message = $"Inventory Full ({currentCount}/{maxSlots} slots)\n" +
                           $"Drop or sell items to make space.";
            ShowError(message, "InventoryFull");
        }

        /// <summary>
        /// Show quest prerequisite missing error.
        /// </summary>
        public static void ShowQuestPrerequisiteMissing(string questId, string prerequisiteId)
        {
            var questMgr = QuestManager.Instance;
            string questName = questId;
            string prereqName = prerequisiteId;

            if (questMgr != null)
            {
                var questDef = questMgr.GetQuestDefinition(questId);
                var prereqDef = questMgr.GetQuestDefinition(prerequisiteId);
                if (questDef != null) questName = questDef.displayName;
                if (prereqDef != null) prereqName = prereqDef.displayName;
            }

            string message = $"Cannot start '{questName}'\n" +
                           $"Complete '{prereqName}' first.";
            ShowError(message, "QuestPrerequisite");
        }

        /// <summary>
        /// Show save failed error with reason.
        /// </summary>
        public static void ShowSaveFailed(string reason)
        {
            string message = $"Save Failed: {reason}\n";

            // Add recovery hints based on reason
            if (reason.Contains("disk") || reason.Contains("space"))
            {
                message += "Free up disk space (need ~50MB).";
            }
            else if (reason.Contains("permission") || reason.Contains("access"))
            {
                message += "Check folder permissions or run as admin.";
            }
            else if (reason.Contains("cloud") || reason.Contains("network"))
            {
                message += "Check internet connection. Local save succeeded.";
            }
            else
            {
                message += "Check logs in AppData/TARTARIA/logs/ for details.";
            }

            ShowError(message, "SaveFailed");
        }

        /// <summary>
        /// Show load failed error with reason.
        /// </summary>
        public static void ShowLoadFailed(string reason)
        {
            string message = $"Load Failed: {reason}\n";

            if (reason.Contains("corrupt") || reason.Contains("invalid"))
            {
                message += "Save file corrupted. Load backup save?";
            }
            else if (reason.Contains("version") || reason.Contains("mismatch"))
            {
                message += "Save from older game version. Migration may fail.";
            }
            else
            {
                message += "Try loading a different save slot.";
            }

            ShowError(message, "LoadFailed");
        }

        /// <summary>
        /// Show combat restriction error (e.g., cannot fast-travel during combat).
        /// </summary>
        public static void ShowCombatRestriction(string action)
        {
            string message = $"Cannot {action} during combat.\n" +
                           $"Defeat all enemies first.";
            ShowError(message, "CombatRestriction");
        }

        /// <summary>
        /// Show dialogue restriction error (e.g., cannot fast-travel during dialogue).
        /// </summary>
        public static void ShowDialogueRestriction(string action)
        {
            string message = $"Cannot {action} during dialogue.\n" +
                           $"Finish conversation first (Esc to skip).";
            ShowError(message, "DialogueRestriction");
        }

        /// <summary>
        /// Show equipment restriction error (e.g., cannot equip weapon — level too low).
        /// </summary>
        public static void ShowEquipmentRestriction(string itemName, int requiredLevel, int currentLevel)
        {
            string message = $"Cannot equip '{itemName}'\n" +
                           $"Requires Level {requiredLevel} (you are Level {currentLevel}).";
            ShowError(message, "EquipmentRestriction");
        }

        /// <summary>
        /// Show building restoration error (e.g., not enough RS).
        /// </summary>
        public static void ShowRestorationFailed(string buildingName, int rsRequired, int rsCurrent)
        {
            string message = $"Cannot restore '{buildingName}'\n" +
                           $"Need {rsRequired} RS (you have {rsCurrent} RS).\n" +
                           $"Defeat enemies or complete quests to earn RS.";
            ShowError(message, "RestorationFailed");
        }

        /// <summary>
        /// Show fast-travel restriction error.
        /// </summary>
        public static void ShowFastTravelRestriction(string reason)
        {
            string message = $"Cannot fast travel: {reason}\n";

            if (reason.Contains("locked"))
            {
                message += "Complete Moon 3 to unlock fast travel.";
            }
            else if (reason.Contains("combat"))
            {
                message += "Defeat all enemies first.";
            }
            else if (reason.Contains("dialogue"))
            {
                message += "Finish conversation first.";
            }
            else
            {
                message += "Find an obelisk to activate fast travel.";
            }

            ShowError(message, "FastTravelRestriction");
        }

        /// <summary>
        /// Show generic contextual error.
        /// </summary>
        public static void ShowGenericError(string errorTitle, string details, string recoveryHint)
        {
            string message = $"{errorTitle}\n{details}\n{recoveryHint}";
            ShowError(message, "GenericError");
        }

        /// <summary>
        /// Internal helper — routes all error messages through HUDController.
        /// </summary>
        static void ShowError(string message, string errorType)
        {
            // Log to console for debugging
            Debug.LogWarning($"[ErrorMessageHelper] {errorType}: {message}");

            // Show to player via HUD
            var hud = UI.HUDController.Instance;
            if (hud != null)
            {
                Core.GameEvents.FireHUDAchievementToast(message);
            }

            // Play error SFX
            Audio.AudioManager.Instance?.PlaySFX2D("UIError");
        }

        /// <summary>
        /// Show confirmation dialog for destructive actions.
        /// </summary>
        public static void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel = null)
        {
            var choiceDialog = UI.ChoiceDialogUI.Instance;
            if (choiceDialog != null)
            {
                choiceDialog.ShowChoices(
                    new[] { "Confirm", "Cancel" },
                    (choice) =>
                    {
                        if (choice == 0) onConfirm?.Invoke();
                        else onCancel?.Invoke();
                    },
                    title,
                    message
                );
            }
            else
            {
                // Fallback: just invoke confirmation (no dialog available)
                Debug.LogWarning($"[ErrorMessageHelper] No ChoiceDialogUI available, confirming by default: {title}");
                onConfirm?.Invoke();
            }
        }

        /// <summary>
        /// Show delete confirmation.
        /// </summary>
        public static void ConfirmDeleteItem(string itemName, string rarity, Action onConfirm)
        {
            ShowConfirmation(
                "Delete Item?",
                $"Permanently delete '{itemName}' ({rarity})?\nThis cannot be undone.",
                onConfirm
            );
        }

        /// <summary>
        /// Show discard quest confirmation.
        /// </summary>
        public static void ConfirmDiscardQuest(string questName, string progress, Action onConfirm)
        {
            ShowConfirmation(
                "Abandon Quest?",
                $"Abandon '{questName}'?\nProgress: {progress}\nYou can re-accept this quest later.",
                onConfirm
            );
        }

        /// <summary>
        /// Show quit without saving confirmation.
        /// </summary>
        public static void ConfirmQuitWithoutSaving(Action onConfirm)
        {
            ShowConfirmation(
                "Quit Without Saving?",
                "Unsaved progress will be lost.\nSave before quitting?",
                onConfirm,
                () =>
                {
                    // Auto-save and then quit
                    Save.SaveManager.Instance?.Save();
                    onConfirm?.Invoke();
                }
            );
        }

        /// <summary>
        /// Show respec confirmation.
        /// </summary>
        public static void ConfirmRespec(int rsCost, Action onConfirm)
        {
            ShowConfirmation(
                "Respec Stat Points?",
                $"Reset all stat allocations for {rsCost} RS?\nYou will regain all spent stat points.",
                onConfirm
            );
        }
    }
}
