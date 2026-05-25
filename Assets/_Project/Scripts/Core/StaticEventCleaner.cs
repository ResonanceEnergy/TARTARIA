using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Static Event Cleaner — resets all static events on domain reload to prevent leaks.
    /// 
    /// Agent 4: Long Session Stability Auditor
    /// 
    /// Problem: Static events persist across domain reloads in Unity Editor, causing:
    /// - Duplicate subscriptions on play mode restart
    /// - Memory leaks from destroyed objects still subscribed
    /// - Event handlers firing multiple times
    /// 
    /// Solution: Clear all static events on SubsystemRegistration (before domain reload).
    /// 
    /// This runs automatically — no manual setup required.
    /// </summary>
    public static class StaticEventCleaner
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticEvents()
        {
            // GameEvents already has its own ResetStatics method that runs automatically
            // We just need to clear other static events here
            
            // Clear other static events
            ClearPlayerCombatEvents();
            ClearMoonBeatRunnerEvents();
            ClearVFXEventSystemEvents();
            ClearLocalizationManagerEvents();
            ClearPlayerInputHandlerEvents();
            
            Debug.Log("[StaticEventCleaner] All static events cleared (domain reload protection)");
        }

        static void ClearPlayerCombatEvents()
        {
            // PlayerCombat.OnSwing is static
            var type = System.Type.GetType("Tartaria.Gameplay.PlayerCombat, Assembly-CSharp");
            if (type != null)
            {
                var field = type.GetField("OnSwing", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field != null)
                {
                    field.SetValue(null, null);
                }
            }
        }

        static void ClearMoonBeatRunnerEvents()
        {
            var type = System.Type.GetType("Tartaria.Integration.MoonBeatRunner, Assembly-CSharp");
            if (type != null)
            {
                var fields = new[] { "OnBeatStarted", "OnBeatCompleted", "OnAllBeatsCompleted" };
                foreach (var fieldName in fields)
                {
                    var field = type.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (field != null)
                    {
                        field.SetValue(null, null);
                    }
                }
            }
        }

        static void ClearVFXEventSystemEvents()
        {
            var type = System.Type.GetType("Tartaria.Core.VFXEventSystem, Assembly-CSharp");
            if (type != null)
            {
                var field = type.GetField("OnVFXRequested", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field != null)
                {
                    field.SetValue(null, null);
                }
            }
        }

        static void ClearLocalizationManagerEvents()
        {
            // Both Core.LocalizationManager and Localization.LocalizationManager
            var types = new[]
            {
                System.Type.GetType("Tartaria.Core.LocalizationManager, Assembly-CSharp"),
                System.Type.GetType("Tartaria.Localization.LocalizationManager, Assembly-CSharp")
            };
            
            foreach (var type in types)
            {
                if (type != null)
                {
                    var field = type.GetField("OnLanguageChanged", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (field != null)
                    {
                        field.SetValue(null, null);
                    }
                }
            }
        }

        static void ClearPlayerInputHandlerEvents()
        {
            var type = System.Type.GetType("Tartaria.Input.PlayerInputHandler, Assembly-CSharp");
            if (type != null)
            {
                var field = type.GetField("OnPauseToggled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (field != null)
                {
                    field.SetValue(null, null);
                }
            }
        }
    }
}
