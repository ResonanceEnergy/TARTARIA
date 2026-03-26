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

        public event Action<GameState, GameState> OnStateChanged;

        public void TransitionTo(GameState newState)
        {
            if (newState == CurrentState) return;

            PreviousState = CurrentState;
            var oldState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameState] {oldState} → {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Returns to the previous state (e.g., unpause → exploration).
        /// </summary>
        public void ReturnToPrevious()
        {
            TransitionTo(PreviousState);
        }

        public bool IsPlaying =>
            CurrentState == GameState.Exploration ||
            CurrentState == GameState.Tuning ||
            CurrentState == GameState.Combat;

        public bool IsPaused => CurrentState == GameState.Paused;
    }
}
