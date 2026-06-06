using System;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Single source of truth: which MonoBehaviour types belong in which scene.
    /// Used by post-build validation and scene populators to prevent
    /// Boot-only singletons from leaking into gameplay scenes (and vice versa).
    /// </summary>
    public static class SceneComponentManifest
    {
        /// <summary>
        /// Components that must ONLY exist in Boot.unity.
        /// If found in any other scene, the build validator will flag an error.
        /// </summary>
        public static readonly HashSet<Type> BootOnly = new()
        {
            typeof(Core.GameBootstrap),
            typeof(Core.SceneLoader),
        };

        /// <summary>
        /// Components that must ONLY exist in the gameplay scene (Echohaven).
        /// These are managers that expect a loaded world and would break in Boot.
        /// </summary>
        public static readonly HashSet<Type> GameplayOnly = new()
        {
            typeof(Integration.PlayerSpawner),
            typeof(Integration.BuildingSpawner),
            typeof(Integration.WorldInitializer),
            typeof(Integration.GameLoopController),
            typeof(Integration.RuntimeGlueBridge),
        };

        /// <summary>
        /// Components created in Boot that persist across scenes via DontDestroyOnLoad.
        /// Populators must NOT re-create these in gameplay scenes.
        /// </summary>
        public static readonly HashSet<Type> BootPersistent = new()
        {
            typeof(Audio.AudioManager),
            typeof(Save.SaveManager),
            typeof(UI.AccessibilityManager),
            typeof(Input.HapticFeedbackManager),
        };

        /// <summary>
        /// Check if a type is forbidden in a given scene.
        /// </summary>
        public static bool IsForbiddenInScene(Type componentType, string sceneName)
        {
            bool isBoot = sceneName.Contains("Boot", StringComparison.OrdinalIgnoreCase);

            if (isBoot && GameplayOnly.Contains(componentType))
                return true;

            if (!isBoot && BootOnly.Contains(componentType))
                return true;

            return false;
        }
    }
}
