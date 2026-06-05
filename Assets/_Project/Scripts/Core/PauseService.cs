using System;
using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Reference-counted Time.timeScale pause stack.
    ///
    /// Multiple UI surfaces (DeathOverlay, GameCompleteOverlay, DialogueChoiceOverlay,
    /// future pause menus) all want to "pause the world" without stomping each other.
    /// Direct Time.timeScale writes leave the world unpaused if any one of them clears
    /// it while another is still open.
    ///
    /// Usage:
    ///     PauseService.Push();   // on show
    ///     PauseService.Pop();    // on hide / cancel / quit
    ///
    /// While at least one Push is outstanding, Time.timeScale == 0. When the refcount
    /// returns to 0, Time.timeScale is restored to 1.
    ///
    /// Legitimate non-pause writes (init/scene-load resets in GameBootstrap and
    /// SceneLoader, plus hit-stop in Combat) stay direct — those aren't pause requests.
    /// </summary>
    public static class PauseService
    {
        static int _pauseRefCount = 0;

        /// <summary>Current outstanding pause count. Read-only for diagnostics.</summary>
        public static int RefCount => _pauseRefCount;

        /// <summary>True when at least one pause holder is active.</summary>
        public static bool IsPaused => _pauseRefCount > 0;

        /// <summary>
        /// Push a pause request. First push freezes Time.timeScale to 0; subsequent
        /// pushes only increment the refcount.
        /// </summary>
        public static void Push()
        {
            _pauseRefCount++;
            if (_pauseRefCount == 1)
            {
                Time.timeScale = 0f;
            }
        }

        /// <summary>
        /// Pop a pause request. Last pop restores Time.timeScale to 1. Refcount is
        /// clamped at 0 — extra Pops are no-ops, safe to call defensively.
        /// </summary>
        public static void Pop()
        {
            _pauseRefCount = Math.Max(0, _pauseRefCount - 1);
            if (_pauseRefCount == 0)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// Emergency reset — forces refcount to 0 and Time.timeScale to 1. Use on
        /// scene transitions / quit paths where we can't guarantee paired Pop() calls.
        /// </summary>
        public static void ForceReset()
        {
            _pauseRefCount = 0;
            Time.timeScale = 1f;
        }
    }
}
