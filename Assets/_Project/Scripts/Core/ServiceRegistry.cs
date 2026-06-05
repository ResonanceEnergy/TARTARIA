using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Service locator pattern for runtime dependency resolution.
    /// Replaces singleton Instance properties with centralized registration.
    /// Registered by GameStateManager during Awake phase.
    /// </summary>
    public static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _locked = false;

        /// <summary>
        /// Register a service instance for runtime resolution.
        /// Must be called during initialization (before any Get calls).
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (_locked)
            {
                Debug.LogError($"[ServiceRegistry] Cannot register {typeof(T).Name} after lock. All registrations must occur during initialization.");
                return;
            }

            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceRegistry] Overwriting existing service: {type.Name}");
            }

            _services[type] = service;
            Debug.Log($"[ServiceRegistry] Registered: {type.Name}");
        }

        /// <summary>
        /// Retrieve a registered service by type.
        /// Returns null if service not registered.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"[ServiceRegistry] Service not registered: {type.Name}. Ensure it was registered during GameStateManager.Awake().");
            return null;
        }

        /// <summary>
        /// Check if a service is registered without logging errors.
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Lock the registry to prevent further registrations.
        /// Called by GameStateManager after all services registered.
        /// </summary>
        public static void Lock()
        {
            _locked = true;
            Debug.Log($"[ServiceRegistry] Locked. {_services.Count} services registered.");
        }

        /// <summary>
        /// Clear all services (for testing or scene reload).
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _locked = false;
            Debug.Log("[ServiceRegistry] Cleared.");
        }

        /// <summary>
        /// Get count of registered services (for debug/validation).
        /// </summary>
        public static int Count => _services.Count;
    }
}
