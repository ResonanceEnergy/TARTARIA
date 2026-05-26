using System;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// Interface for quest provider, used by UI to avoid direct asmdef reference to Integration.
    /// </summary>
    public interface IQuestProvider
    {
        event Action<string, QuestStatus> OnQuestStatusChanged;
        event Action<string, int> OnObjectiveProgressed;
        List<string> GetActiveQuestIds();
        List<string> GetCompletedQuestIds();
        QuestState GetQuestState(string questId);
        QuestDefinition GetQuestDefinition(string questId);
    }

    /// <summary>
    /// Service locator for IQuestProvider to avoid direct singleton coupling.
    /// </summary>
    public static class QuestProviderLocator
    {
        public static IQuestProvider Current { get; set; }
    }
}
