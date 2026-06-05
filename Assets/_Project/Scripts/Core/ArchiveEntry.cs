using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Category tags for the Old World Archive.
    /// Each entry is tagged so the player can filter by topic.
    /// </summary>
    public enum ArchiveCategory
    {
        Architecture    = 0,   // Buildings, domes, towers, mud flood evidence
        Technology      = 1,   // Free energy, resonance machines, aether tech
        Astronomy       = 2,   // Star forts, ley lines, celestial alignment
        Culture         = 3,   // Art, fashion, world fairs, lost civilisation customs
        Mystery         = 4,   // Anomalies, suppressed history, hidden timelines
        Science         = 5,   // Sacred geometry, 432 Hz, vortex mathematics
        People          = 6,   // Notable figures, rulers, explorers of the old world
        Evidence        = 7,   // Photographs, maps, artefacts that survived the reset
    }

    /// <summary>
    /// A single educational entry in the Tartaria Old World Archive.
    ///
    /// Each entry represents a real concept, structure, or mystery from the
    /// suppressed history of the Tartarian civilisation.
    ///
    /// Unlocked by:
    ///   - Discovering a related building in the world
    ///   - Anastasia delivering the matching lore whisper
    ///   - Completing a quest tagged with this entry's id
    ///   - Finding a hidden document collectable
    /// </summary>
    [CreateAssetMenu(fileName = "ArchiveEntry", menuName = "Tartaria/Archive/Entry")]
    public class ArchiveEntry : ScriptableObject
    {
        [Header("Identity")]
        public string entryId;              // Unique key, e.g. "tartaria_domes"
        public string title;                // Display name: "Tartarian Star Domes"
        public ArchiveCategory category;

        [Header("Content")]
        [TextArea(3, 6)]
        public string summary;              // 2–3 sentences, shown in the list

        [TextArea(6, 20)]
        public string fullText;             // Full educational text shown in detail view

        [Header("Visuals")]
        public Sprite thumbnail;            // Small icon (optional — fallback = category icon)

        [Header("Connections")]
        public string[] relatedEntryIds;    // Other entries this links to

        [Header("Unlock")]
        public string unlockTrigger;        // Event key or trigger context that unlocks this
        public bool unlockedByDefault;      // Always visible (intro entries)
        public int moonIndex;               // 0 = global, 1–13 = specific moon

        [Header("Quote")]
        [TextArea(2, 4)]
        public string anastasiaQuote;       // Whisper Anastasia says when this is discovered
    }
}
