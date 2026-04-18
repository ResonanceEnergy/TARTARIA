using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// The complete Old World Archive database.
    ///
    /// Contains all educational ArchiveEntry assets.
    /// At runtime, ArchiveManager queries this to find entries by id or category.
    ///
    /// The Editor populator (ArchiveDatabasePopulator.cs) fills this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "ArchiveDatabase", menuName = "Tartaria/Archive/Database")]
    public class ArchiveDatabase : ScriptableObject
    {
        public ArchiveEntry[] entries = System.Array.Empty<ArchiveEntry>();

        // ─── Fast lookup (built at runtime) ──────────────
        Dictionary<string, ArchiveEntry> _lookup;

        public void BuildLookup()
        {
            _lookup = new Dictionary<string, ArchiveEntry>(entries.Length);
            foreach (var e in entries)
                if (e != null && !string.IsNullOrEmpty(e.entryId))
                    _lookup[e.entryId] = e;
        }

        public ArchiveEntry GetById(string id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out var e) ? e : null;
        }

        public ArchiveEntry[] GetByCategory(ArchiveCategory cat)
        {
            var list = new System.Collections.Generic.List<ArchiveEntry>();
            foreach (var e in entries)
                if (e != null && e.category == cat)
                    list.Add(e);
            return list.ToArray();
        }

        public ArchiveEntry[] Search(string query)
        {
            if (string.IsNullOrEmpty(query)) return entries;
            query = query.ToLower();
            var list = new System.Collections.Generic.List<ArchiveEntry>();
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.title.ToLower().Contains(query)
                    || e.summary.ToLower().Contains(query)
                    || e.entryId.ToLower().Contains(query))
                    list.Add(e);
            }
            return list.ToArray();
        }
    }
}
