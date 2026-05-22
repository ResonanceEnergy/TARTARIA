using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// In-world quest giver / turn-in. Implements <see cref="IInteractable"/>.
    ///
    /// Behaviour:
    ///   • If the quest is not yet active and not completed → "[E] Talk to {giverName}"
    ///     pressing Interact activates the quest and shows a HUD toast + objective text.
    ///   • If the quest is active and all objectives complete → "[E] Turn in {questDisplayName}"
    ///     pressing Interact completes the quest and shows a reward toast.
    ///   • If the quest is active but objectives unfinished → shows current objective text.
    ///   • If completed → no prompt.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestGiverInteractable : MonoBehaviour, IInteractable
    {
        [Header("Quest")]
        [Tooltip("ID of the quest in QuestManager / QuestDefinition.questId.")]
        public string questId;

        [Tooltip("Optional override; falls back to QuestDefinition.displayName.")]
        public string questDisplayNameOverride;

        [Header("Giver")]
        [Tooltip("Who the player is talking to (HUD prompt only).")]
        public string giverName = "Anastasia";

        [TextArea(2, 4)]
        [Tooltip("Optional intro line shown as a HUD toast on accept.")]
        public string acceptToast = "Quest accepted.";

        [TextArea(2, 4)]
        [Tooltip("Optional line shown as a HUD toast on completion / turn-in.")]
        public string completeToast = "Quest complete!";

        QuestDefinition _cachedDef;

        QuestDefinition Definition
        {
            get
            {
                if (_cachedDef != null) return _cachedDef;
                if (QuestManager.Instance == null) return null;
                _cachedDef = QuestManager.Instance.GetQuestDefinition(questId);
                return _cachedDef;
            }
        }

        string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(questDisplayNameOverride)) return questDisplayNameOverride;
                var def = Definition;
                return def != null && !string.IsNullOrEmpty(def.displayName) ? def.displayName : questId;
            }
        }

        public string GetInteractPrompt()
        {
            // P1 AUDIT FIX: Show "Quest system offline" instead of silent null return
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning($"[QuestGiver] {giverName} has no questId assigned.");
                return "[E] Quest system offline (no quest ID)";
            }
            
            if (QuestManager.Instance == null)
            {
                Debug.LogError("[QuestGiver] QuestManager.Instance is null. Quest system offline.");
                return "[E] Quest system offline";
            }

            var state = QuestManager.Instance.GetQuestState(questId);
            switch (state.status)
            {
                case QuestStatus.Locked:
                    return $"[E] Talk to {giverName}";
                case QuestStatus.Active:
                    return AllObjectivesComplete(state) ? $"[E] Turn in: {DisplayName}" : null;
                case QuestStatus.Completed:
                case QuestStatus.Failed:
                    return null;
                default:
                    return null;
            }
        }

        public void Interact(GameObject player)
        {
            // P1 AUDIT FIX: Show toast message instead of silent fail
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning($"[QuestGiver] {giverName}: Cannot interact - no questId assigned.");
                GameEvents.FireHUDAchievementToast($"{giverName}: Quest system offline (no quest ID)");
                return;
            }
            
            if (QuestManager.Instance == null)
            {
                Debug.LogError("[QuestGiver] Cannot interact - QuestManager.Instance is null.");
                GameEvents.FireHUDAchievementToast("Quest system offline - please report this bug");
                return;
            }

            var state = QuestManager.Instance.GetQuestState(questId);

            if (state.status == QuestStatus.Locked)
            {
                QuestManager.Instance.ActivateQuest(questId);
                ShowToast(string.IsNullOrEmpty(acceptToast) ? $"Quest accepted: {DisplayName}" : acceptToast);
                ShowFirstObjective();
                Save.SaveManager.Instance?.MarkDirty();
                return;
            }

            if (state.status == QuestStatus.Active && AllObjectivesComplete(state))
            {
                // CompleteQuest is internal; force-fill any residual progress so QuestManager
                // promotes the quest to Completed via its own ProgressObjective auto-promotion.
                var def = Definition;
                if (def != null && def.objectives != null)
                {
                    for (int i = 0; i < def.objectives.Length; i++)
                    {
                        int target = Mathf.Max(1, def.objectives[i].targetCount);
                        QuestManager.Instance.ProgressObjective(questId, i, target);
                    }
                }
                ShowToast(string.IsNullOrEmpty(completeToast) ? $"Quest complete: {DisplayName}" : completeToast);
                Save.SaveManager.Instance?.MarkDirty();
            }
        }

        bool AllObjectivesComplete(QuestState state)
        {
            var def = Definition;
            if (def == null || def.objectives == null || def.objectives.Length == 0) return true;
            if (state.objectiveProgress == null || state.objectiveProgress.Length < def.objectives.Length) return false;

            for (int i = 0; i < def.objectives.Length; i++)
            {
                int target = Mathf.Max(1, def.objectives[i].targetCount);
                if (state.objectiveProgress[i] < target) return false;
            }
            return true;
        }

        void ShowFirstObjective()
        {
            var def = Definition;
            if (def == null || def.objectives == null || def.objectives.Length == 0) return;
            HUDController.Instance?.ShowObjective(def.objectives[0].description);
        }

        static void ShowToast(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            HUDController.Instance?.ShowAchievementToast(msg);
        }
    }
}
