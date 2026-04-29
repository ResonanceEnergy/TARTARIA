using System;
using UnityEngine;

namespace Tartaria.Core
{
    public enum GameState
    {
        Boot,
        Loading,
        Exploration,
        Tuning,
        Combat,
        Cinematic,
        Paused,
        Menu
    }

    /// <summary>
    /// Central game state machine. Manages transitions between exploration,
    /// tuning mini-games, combat, cinematics, and menus.
    /// Singleton — lives for the entire application lifetime.
    /// </summary>
    public class GameStateManager
    {
        static readonly Lazy<GameStateManager> _instance = new(() => new GameStateManager());
        public static GameStateManager Instance => _instance.Value;

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public GameState PreviousState { get; private set; } = GameState.Boot;

        /// <summary>
        /// Current moon phase in 13-moon campaign (0-12).
        /// Affects Aether yield and certain quest triggers.
        /// </summary>
        public int CurrentMoonPhase { get; set; } = 0;

        public event Action<GameState, GameState> OnStateChanged;

        public void TransitionTo(GameState newState)
        {
            if (newState == CurrentState) return;

            PreviousState = CurrentState;
            var oldState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameState] {oldState} → {newState}\n{System.Environment.StackTrace}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Returns to the previous state (e.g., unpause → exploration).
        /// Clears the previous state after use to prevent toggle oscillation.
        /// </summary>
        public void ReturnToPrevious()
        {
            var target = PreviousState;
            TransitionTo(target);
            PreviousState = target; // Pin so double-call is a no-op
        }

        public bool IsPlaying =>
            CurrentState == GameState.Exploration ||
            CurrentState == GameState.Tuning ||
            CurrentState == GameState.Combat;

        public bool IsPaused => CurrentState == GameState.Paused;
    }
}
