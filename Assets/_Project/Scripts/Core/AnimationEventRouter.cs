using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tartaria.Core
{
    /// <summary>
    /// AnimationEventRouter — bridges Unity Animation Events to C# callbacks.
    /// Attach to GameObject with Animator, define event handlers, trigger via Animation Event timeline.
    /// Avoids hardcoded method names in Animation Events (better type safety + refactoring).
    /// 
    /// Unity Animation Event Setup:
    /// 1. Open Animation window
    /// 2. Add Event marker on timeline
    /// 3. Set Function: "TriggerEvent"
    /// 4. Set String parameter: "footstep", "swing", "cast", etc.
    /// 
    /// Code Setup:
    /// - RegisterHandler("footstep", () => PlayFootstepSFX())
    /// - RegisterHandler("swing", () => PlaySwingSFX())
    /// - Animation calls TriggerEvent("footstep") → fires callback
    /// 
    /// Usage:
    /// - Attach to player/NPC with Animator
    /// - Register handlers in Start()
    /// - Animation Events trigger via string ID
    /// 
    /// GDD refs: §05 (Animation Polish), §09 (Combat Feedback)
    /// </summary>
    public class AnimationEventRouter : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] bool logEvents = false;

        Dictionary<string, Action> _eventHandlers = new();
        Dictionary<string, Action<float>> _eventHandlersFloat = new();
        Dictionary<string, Action<int>> _eventHandlersInt = new();
        Dictionary<string, Action<string>> _eventHandlersString = new();

        /// <summary>
        /// Register handler for animation event (no parameter).
        /// </summary>
        public void RegisterHandler(string eventName, Action callback)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                Debug.LogWarning($"[AnimEvent] Handler '{eventName}' already registered, overwriting");
            }

            _eventHandlers[eventName] = callback;
        }

        /// <summary>
        /// Register handler for animation event with float parameter.
        /// </summary>
        public void RegisterHandlerFloat(string eventName, Action<float> callback)
        {
            if (_eventHandlersFloat.ContainsKey(eventName))
            {
                Debug.LogWarning($"[AnimEvent] Float handler '{eventName}' already registered, overwriting");
            }

            _eventHandlersFloat[eventName] = callback;
        }

        /// <summary>
        /// Register handler for animation event with int parameter.
        /// </summary>
        public void RegisterHandlerInt(string eventName, Action<int> callback)
        {
            if (_eventHandlersInt.ContainsKey(eventName))
            {
                Debug.LogWarning($"[AnimEvent] Int handler '{eventName}' already registered, overwriting");
            }

            _eventHandlersInt[eventName] = callback;
        }

        /// <summary>
        /// Register handler for animation event with string parameter.
        /// </summary>
        public void RegisterHandlerString(string eventName, Action<string> callback)
        {
            if (_eventHandlersString.ContainsKey(eventName))
            {
                Debug.LogWarning($"[AnimEvent] String handler '{eventName}' already registered, overwriting");
            }

            _eventHandlersString[eventName] = callback;
        }

        /// <summary>
        /// Unregister handler.
        /// </summary>
        public void UnregisterHandler(string eventName)
        {
            _eventHandlers.Remove(eventName);
            _eventHandlersFloat.Remove(eventName);
            _eventHandlersInt.Remove(eventName);
            _eventHandlersString.Remove(eventName);
        }

        // ===== Unity Animation Event Entry Points (called from Animation timeline) =====

        /// <summary>
        /// Trigger animation event with no parameter.
        /// Called from Unity Animation Event with String parameter = event name.
        /// </summary>
        public void TriggerEvent(string eventName)
        {
            if (logEvents)
            {
                Debug.Log($"[AnimEvent] {gameObject.name}: {eventName}");
            }

            if (_eventHandlers.TryGetValue(eventName, out var handler))
            {
                handler?.Invoke();
            }
            else
            {
                if (logEvents)
                {
                    Debug.LogWarning($"[AnimEvent] No handler registered for '{eventName}'");
                }
            }
        }

        /// <summary>
        /// Trigger animation event with float parameter.
        /// </summary>
        public void TriggerEventFloat(string eventName, float value)
        {
            if (logEvents)
            {
                Debug.Log($"[AnimEvent] {gameObject.name}: {eventName}({value})");
            }

            if (_eventHandlersFloat.TryGetValue(eventName, out var handler))
            {
                handler?.Invoke(value);
            }
            else
            {
                if (logEvents)
                {
                    Debug.LogWarning($"[AnimEvent] No float handler registered for '{eventName}'");
                }
            }
        }

        /// <summary>
        /// Trigger animation event with int parameter.
        /// </summary>
        public void TriggerEventInt(string eventName, int value)
        {
            if (logEvents)
            {
                Debug.Log($"[AnimEvent] {gameObject.name}: {eventName}({value})");
            }

            if (_eventHandlersInt.TryGetValue(eventName, out var handler))
            {
                handler?.Invoke(value);
            }
            else
            {
                if (logEvents)
                {
                    Debug.LogWarning($"[AnimEvent] No int handler registered for '{eventName}'");
                }
            }
        }

        /// <summary>
        /// Trigger animation event with string parameter.
        /// </summary>
        public void TriggerEventString(string eventName, string value)
        {
            if (logEvents)
            {
                Debug.Log($"[AnimEvent] {gameObject.name}: {eventName}(\"{value}\")");
            }

            if (_eventHandlersString.TryGetValue(eventName, out var handler))
            {
                handler?.Invoke(value);
            }
            else
            {
                if (logEvents)
                {
                    Debug.LogWarning($"[AnimEvent] No string handler registered for '{eventName}'");
                }
            }
        }

        // ===== Convenience Methods for Common Events =====

        /// <summary>
        /// Common footstep event (called from walk/run animations).
        /// </summary>
        public void OnFootstep()
        {
            TriggerEvent("footstep");
        }

        /// <summary>
        /// Common attack start event.
        /// </summary>
        public void OnAttackStart()
        {
            TriggerEvent("attack_start");
        }

        /// <summary>
        /// Common attack hit event (damage window).
        /// </summary>
        public void OnAttackHit()
        {
            TriggerEvent("attack_hit");
        }

        /// <summary>
        /// Common attack end event.
        /// </summary>
        public void OnAttackEnd()
        {
            TriggerEvent("attack_end");
        }

        /// <summary>
        /// Common ability cast event.
        /// </summary>
        public void OnAbilityCast()
        {
            TriggerEvent("ability_cast");
        }

        /// <summary>
        /// Common jump land event.
        /// </summary>
        public void OnJumpLand()
        {
            TriggerEvent("jump_land");
        }

        /// <summary>
        /// Common dodge event.
        /// </summary>
        public void OnDodge()
        {
            TriggerEvent("dodge");
        }

        /// <summary>
        /// Clear all handlers (useful for cleanup).
        /// </summary>
        public void ClearAllHandlers()
        {
            _eventHandlers.Clear();
            _eventHandlersFloat.Clear();
            _eventHandlersInt.Clear();
            _eventHandlersString.Clear();
        }
    }
}
