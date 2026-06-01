using System;
using UnityEngine;

namespace Tartaria.Integration
{
    public enum AnastasiaLineCategory
    {
        LoreWhisper,
        MemoryFragment,
        CompanionReaction,
        BuildingCommentary,
        StoryBeat,
        PersonalReflection,
        EasterEgg,
        TheFinalLine
    }

    [Serializable]
    public struct AnastasiaLine
    {
        public int id;                        // 0-111, maps to bitmask bit
        public int moon;                      // 1-13, 0 = DotT
        public AnastasiaLineCategory category;
        public string triggerContext;          // proximity tag or event key
        [TextArea(2, 4)]
        public string text;
    }

    [CreateAssetMenu(fileName = "AnastasiaDialogue", menuName = "Tartaria/Anastasia Dialogue Database")]
    public class AnastasiaDialogueDatabase : ScriptableObject
    {
        public AnastasiaLine[] lines = Array.Empty<AnastasiaLine>();
    }
}
