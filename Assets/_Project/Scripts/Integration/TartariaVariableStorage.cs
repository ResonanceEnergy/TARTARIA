using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Yarn.Unity;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// Tartaria Variable Storage — STUB for Yarn Spinner variable bridging.
    /// 
    /// Read-only game state variables:
    ///   $rs      — Current Resonance Score (0-100)
    ///   $moon    — Current Moon cleared count (0-13)
    ///   $companion — Active companion name ("Milo", "Lirael", etc.)
    ///
    /// DEFERRED: Full Yarn API integration pending Yarn 2.5.1 API surface verification in Unity Editor.
    /// Core structure implemented; runtime testing needed for final wiring.
    /// </summary>
    public class TartariaVariableStorage : InMemoryVariableStorage
    {
        // Inherit from InMemoryVariableStorage to get baseline implementation
        // Override only what's needed for game state bridging

        public override bool TryGetValue<T>(string variableName, out T result)
        {
            // Handle read-only game state variables
            if (variableName == "$rs")
            {
                float rs = GetCurrentRS();
                result = (T)(object)rs;
                return true;
            }

            if (variableName == "$moon")
            {
                // $moon returns current cleared count (0-13)
                int moon = MoonProgressTracker.Instance?.ClearedCount ?? 0;
                result = (T)(object)(float)moon;
                return true;
            }

            if (variableName == "$companion")
            {
                string companion = "Milo"; // Default; could query active companion system
                result = (T)(object)companion;
                return true;
            }

            // Fallback to base implementation for Yarn-persisted variables
            return base.TryGetValue(variableName, out result);
        }

        // ─── RS Query (ECS → Managed) ───────────────

        float GetCurrentRS()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return 0f;

            var em = world.EntityManager;
            var query = em.CreateEntityQuery(typeof(ResonanceScore));
            if (query.CalculateEntityCount() == 0)
            {
                query.Dispose();
                return 0f;
            }

            var rsData = query.GetSingleton<ResonanceScore>();
            query.Dispose();
            return rsData.CurrentRS;
        }
    }
}
