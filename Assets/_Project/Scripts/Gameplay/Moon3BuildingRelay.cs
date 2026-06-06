using System;
using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Lightweight per-building relay component used by Moon 3 zone scaffolding
    /// to broker "building restored" events between scaffold-spawned proxies and
    /// runtime gameplay systems (rail escort, quest objectives, train events).
    ///
    /// EXTRACTION HISTORY: Previously declared as a nested public class inside
    /// <c>Tartaria.Editor.Moon3ZoneScaffold</c>, which made it unreachable from
    /// the runtime <c>Tartaria.Gameplay</c> asmdef (where
    /// <c>RailEscortController</c> calls <c>AddComponent&lt;Moon3BuildingRelay&gt;()</c>).
    /// Moved here so both editor scaffold and runtime systems share one type.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon3BuildingRelay : MonoBehaviour
    {
        public string buildingId;
        public Action onRestoredAction;

        bool _fired;

        public bool HasFired => _fired;

        /// <summary>
        /// Invoked by <c>InteractableBuilding</c> (or scaffold harness) when the
        /// associated building completes restoration. Fires the relay action at most once.
        /// </summary>
        public void FireRestored()
        {
            if (_fired) return;
            _fired = true;
            onRestoredAction?.Invoke();
        }

        /// <summary>Resets the relay so it can fire again (used by save reload / scenario tests).</summary>
        public void ResetRelay() => _fired = false;
    }
}
