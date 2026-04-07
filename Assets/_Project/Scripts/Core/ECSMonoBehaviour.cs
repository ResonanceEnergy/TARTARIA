using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Base class for MonoBehaviours that interact with the ECS world.
    /// Provides automatic EntityQuery lifecycle management — subclasses
    /// register queries via <see cref="TrackQuery"/> and disposal with
    /// World.IsCreated guards is handled automatically in OnDestroy.
    ///
    /// Usage:
    ///   // In your subclass, create and register:
    ///   _rsQuery = em.CreateEntityQuery(typeof(ResonanceScore));
    ///   TrackQuery(ref _rsQuery);
    ///
    /// No need to write OnDestroy guards or bool flags.
    /// If you override OnDestroy, call base.OnDestroy() first.
    /// </summary>
    public abstract class ECSMonoBehaviour : MonoBehaviour
    {
        struct TrackedQuery
        {
            public EntityQuery Query;
            public World World;
        }

        readonly List<TrackedQuery> _trackedQueries = new();

        /// <summary>
        /// Register an EntityQuery for automatic disposal on OnDestroy.
        /// Call this immediately after creating the query, passing the world
        /// it was created on so disposal is safe across world rebuilds.
        /// </summary>
        protected void TrackQuery(EntityQuery query, World world)
        {
            _trackedQueries.Add(new TrackedQuery { Query = query, World = world });
        }

        /// <summary>
        /// Override point for subclass cleanup.
        /// Always call base.OnDestroy() if you override.
        /// </summary>
        protected virtual void OnDestroy()
        {
            foreach (var t in _trackedQueries)
            {
                if (t.World != null && t.World.IsCreated)
                    t.Query.Dispose();
            }
            _trackedQueries.Clear();
        }
    }
}
