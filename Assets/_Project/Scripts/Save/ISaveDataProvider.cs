using System;

namespace Tartaria.Save
{
    /// <summary>
    /// Interface for modular save/load providers. Enables extensibility without
    /// modifying SaveData core — adheres to Open/Closed principle.
    /// 
    /// Design:
    ///   - Providers register themselves with SaveManager in Awake()
    ///   - SaveManager discovers all implementations via registration
    ///   - Each provider returns a serializable object (plain C# class, no MonoBehaviour)
    ///   - SaveManager stores providers in Dictionary&lt;string, object&gt; keyed by type name
    ///   - Backward compatible: existing SaveData blocks coexist with provider data
    /// 
    /// Usage Example:
    ///   public class SkillTreeSaveDataProvider : MonoBehaviour, ISaveDataProvider
    ///   {
    ///       public string GetProviderKey() => "SkillTree";
    ///       
    ///       public object GetSaveData()
    ///       {
    ///           return new SkillTreeData { unlockedSkills = _unlocked.ToArray() };
    ///       }
    ///       
    ///       public void RestoreSaveData(object data)
    ///       {
    ///           if (data is SkillTreeData std) _unlocked = new List&lt;string&gt;(std.unlockedSkills);
    ///       }
    ///       
    ///       [Serializable]
    ///       class SkillTreeData { public string[] unlockedSkills; }
    ///   }
    /// 
    /// Thread-safety: All calls happen on main thread (Unity lifecycle).
    /// Performance: O(n) registration at startup, O(n) serialize/deserialize on save/load.
    /// </summary>
    public interface ISaveDataProvider
    {
        /// <summary>
        /// Unique key for this provider (used as dictionary key in SaveData).
        /// Recommendation: use type name or a stable identifier.
        /// Must be consistent across sessions for save compatibility.
        /// </summary>
        string GetProviderKey();

        /// <summary>
        /// Returns current state as a serializable object.
        /// Object must be JSON-serializable (no MonoBehaviour, no Unity objects).
        /// Called by SaveManager before writing to disk.
        /// </summary>
        object GetSaveData();

        /// <summary>
        /// Restores state from previously saved data.
        /// Called by SaveManager after loading from disk.
        /// Implementation should handle null/invalid data gracefully.
        /// </summary>
        /// <param name="data">Previously saved data from GetSaveData(), or null if no prior save</param>
        void RestoreSaveData(object data);
    }
}
