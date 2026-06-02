using System;

namespace Tartaria.UI
{
    /// <summary>
    /// Plain data record for one collected lore page in <see cref="LorebookPanel"/>.
    /// Mirrors <see cref="QuestLogEntry"/> in shape (POCO, no MonoBehaviour) — the panel
    /// renders these imperatively via UnityEngine.UI.Text components.
    ///
    /// One instance per discovered lore id. Discovery time is captured at first AddEntry()
    /// (idempotent by id) so re-encountering the same collectible never resets the timestamp.
    /// </summary>
    public class LorebookEntry
    {
        /// <summary>Stable id used to dedupe collection events and key PlayerPrefs persistence.</summary>
        public string id;

        /// <summary>Display title shown in the lorebook list and the banner toast.</summary>
        public string title;

        /// <summary>Full body text shown in the right-hand reader panel when an entry is selected.</summary>
        public string body;

        /// <summary>Realtime seconds since process start when this entry was first collected.</summary>
        public float discoveredAt;

        public LorebookEntry(string id, string title, string body, float discoveredAt)
        {
            this.id = id;
            this.title = string.IsNullOrEmpty(title) ? id : title;
            this.body = body ?? string.Empty;
            this.discoveredAt = discoveredAt;
        }

        /// <summary>
        /// Formatted single-line representation:
        ///   "[LORE 12s] The Mud-Flood — Long before the first chime..."
        /// Used by LorebookPanel for list rendering and diagnostic Debug.Log calls.
        /// Body is truncated to 60 chars in the diagnostic form to keep logs readable.
        /// </summary>
        public override string ToString()
        {
            float age = UnityEngine.Time.realtimeSinceStartup - discoveredAt;
            string preview = body;
            if (!string.IsNullOrEmpty(preview) && preview.Length > 60)
            {
                preview = preview.Substring(0, 57) + "...";
            }
            return $"[LORE {age:F0}s] {title} - {preview}";
        }
    }
}
