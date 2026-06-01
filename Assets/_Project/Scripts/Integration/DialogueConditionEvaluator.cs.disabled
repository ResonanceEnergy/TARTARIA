using UnityEngine;
using Tartaria.Data;
using Tartaria.Gameplay;
using Tartaria.Core;
using Tartaria.Core.Enums;
using DataDialogueCondition = Tartaria.Data.DialogueCondition;
using DataDialogueConditionType = Tartaria.Data.DialogueConditionType;

namespace Tartaria.Integration
{
    /// <summary>
    /// DialogueConditionEvaluator — bridges Data assembly (DialogueCondition) with Integration/Gameplay assemblies.
    /// Provides actual condition evaluation logic that requires access to QuestManager and PlayerProgression.
    ///
    /// Architecture:
    ///   - Data assembly: DialogueCondition.Evaluate() calls static EvaluateDelegate
    ///   - Integration assembly: DialogueConditionEvaluator sets delegate in Awake()
    ///   - This avoids assembly reference violations (Data cannot reference Integration/Gameplay)
    ///
    /// Supported conditions:
    ///   - QuestComplete: Checks if quest is in Completed status
    ///   - QuestActive: Checks if quest is in Active status
    ///   - MinPlayerLevel: Checks if PlayerProgression.CurrentLevel >= threshold
    ///   - StatCheck: Checks if player stat (Vitality, Strength, etc.) >= threshold
    ///   - Custom: Delegates to DialogueConditionHandler.EvaluateCustom()
    ///
    /// Execution order: -60 (after QuestManager -50, before DialogueManager -40)
    /// </summary>
    [DefaultExecutionOrder(-60)]
    public class DialogueConditionEvaluator : MonoBehaviour
    {
        static DialogueConditionEvaluator Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("DialogueConditionEvaluator");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<DialogueConditionEvaluator>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Wire evaluation delegate to bridge Data → Integration/Gameplay
            DataDialogueCondition.EvaluateDelegate = EvaluateCondition;
            Debug.Log("[DialogueConditionEvaluator] Condition evaluation delegate registered");
        }

        void OnDestroy()
        {
            // Cleanup delegate
            if (DataDialogueCondition.EvaluateDelegate == EvaluateCondition)
            {
                DataDialogueCondition.EvaluateDelegate = null;
            }
        }

        /// <summary>
        /// Evaluates a dialogue condition with access to all game systems.
        /// </summary>
        static bool EvaluateCondition(DataDialogueCondition condition)
        {
            switch (condition.type)
            {
                case DataDialogueConditionType.None:
                    return true;

                case DataDialogueConditionType.QuestComplete:
                    if (string.IsNullOrEmpty(condition.questId))
                    {
                        Debug.LogWarning("[DialogueCondition] QuestComplete condition missing questId");
                        return false;
                    }
                    return QuestManager.Instance != null && QuestManager.Instance.IsQuestComplete(condition.questId);

                case DataDialogueConditionType.QuestActive:
                    if (string.IsNullOrEmpty(condition.questId))
                    {
                        Debug.LogWarning("[DialogueCondition] QuestActive condition missing questId");
                        return false;
                    }
                    // QuestManager.IsQuestActive doesn't exist, check state directly
                    if (QuestManager.Instance != null)
                    {
                        var state = QuestManager.Instance.GetQuestState(condition.questId);
                        return state.status == Core.Enums.QuestStatus.Active;
                    }
                    return false;

                case DataDialogueConditionType.MinPlayerLevel:
                    if (PlayerProgression.Instance == null)
                    {
                        Debug.LogWarning("[DialogueCondition] PlayerProgression not initialized");
                        return false;
                    }
                    return PlayerProgression.Instance.CurrentLevel >= condition.minPlayerLevel;

                case DataDialogueConditionType.StatCheck:
                    if (PlayerProgression.Instance == null)
                    {
                        Debug.LogWarning("[DialogueCondition] PlayerProgression not initialized for stat check");
                        return false;
                    }

                    // Convert DialogueStatType to PlayerProgression.StatType
                    PlayerProgression.StatType playerStatType;
                    switch (condition.statType)
                    {
                        case DialogueStatType.Strength:
                            playerStatType = PlayerProgression.StatType.Strength;
                            break;
                        case DialogueStatType.Agility:
                            playerStatType = PlayerProgression.StatType.Agility;
                            break;
                        case DialogueStatType.Vitality:
                            playerStatType = PlayerProgression.StatType.Vitality;
                            break;
                        case DialogueStatType.Resonance:
                            playerStatType = PlayerProgression.StatType.Attunement; // Resonance maps to Attunement in PlayerProgression
                            break;
                        case DialogueStatType.Intelligence:
                        case DialogueStatType.Charisma:
                            Debug.LogWarning($"[DialogueCondition] {condition.statType} not implemented in PlayerProgression, returning false");
                            return false;
                        default:
                            Debug.LogWarning($"[DialogueCondition] Unknown DialogueStatType: {condition.statType}");
                            return false;
                    }

                    int statValue = PlayerProgression.Instance.GetStatValue(playerStatType);
                    return statValue >= condition.minStatValue;

                case DataDialogueConditionType.Custom:
                    if (string.IsNullOrEmpty(condition.customConditionKey))
                    {
                        Debug.LogWarning("[DialogueCondition] Custom condition missing key");
                        return false;
                    }
                    return Data.DialogueConditionHandler.EvaluateCustom(condition.customConditionKey);

                default:
                    Debug.LogWarning($"[DialogueCondition] Unknown condition type: {condition.type}");
                    return true; // Graceful fallback
            }
        }
    }
}
