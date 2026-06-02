using System;

namespace Tartaria.UI
{
    /// <summary>
    /// Plain data record for one quest line in <see cref="QuestLogPanel"/>.
    /// Not a MonoBehaviour — we render it imperatively via UnityEngine.UI.Text components,
    /// so this class is just the model. One instance per known quest.
    /// </summary>
    public class QuestLogEntry
    {
        /// <summary>Stable id used by <c>GameEvents.OnQuestActivated/Completed</c> and <c>QuestSystem.GetQuest</c>.</summary>
        public string questId;

        /// <summary>Display title pulled from <c>Quest.title</c> at activation time. Falls back to questId if unknown.</summary>
        public string title;

        /// <summary>Display description pulled from <c>Quest.description</c>. May be empty.</summary>
        public string description;

        /// <summary>True once OnQuestCompleted fires for this entry. Drives strike-through styling in the panel.</summary>
        public bool completed;

        /// <summary>Realtime seconds since process start when this quest was first added to the log.</summary>
        public float startTime;

        /// <summary>Realtime seconds since process start when this quest moved to the completed list. Null while active.</summary>
        public float? completionTime;

        public QuestLogEntry(string questId, string title, string description, float startTime)
        {
            this.questId = questId;
            this.title = string.IsNullOrEmpty(title) ? questId : title;
            this.description = description ?? string.Empty;
            this.completed = false;
            this.startTime = startTime;
            this.completionTime = null;
        }

        /// <summary>
        /// Formatted single-line representation:
        ///   "[ACTIVE 12s] First Resonance — Restore the buried cathedral"
        ///   "[DONE 47s] First Resonance — Restore the buried cathedral"
        /// Used by QuestLogPanel when rendering and for diagnostic Debug.Log calls.
        /// </summary>
        public override string ToString()
        {
            if (completed && completionTime.HasValue)
            {
                float duration = completionTime.Value - startTime;
                return $"[DONE {duration:F0}s] {title} - {description}";
            }
            float age = UnityEngine.Time.realtimeSinceStartup - startTime;
            return $"[ACTIVE {age:F0}s] {title} - {description}";
        }
    }
}
