using System;
using System.Reflection;
using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// Reflection bridge from Tartaria.UI into Tartaria.Integration types without
    /// adding an asmdef reference (which would create a Integration→Gameplay→UI cycle).
    /// Mirrors the pattern used by Tartaria.Audio.HapticBridge / VFXBridge.
    /// </summary>
    public static class IntegrationBridge
    {
        static Type _combatBridge;
        static Type _bossEncounter;
        static Type _giantMode;
        static Type _dialogueManager;
        static bool _resolved;

        static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            try
            {
                _combatBridge   = Type.GetType("Tartaria.Integration.CombatBridge, Tartaria.Integration");
                _bossEncounter  = Type.GetType("Tartaria.Integration.BossEncounterSystem, Tartaria.Integration");
                _giantMode      = Type.GetType("Tartaria.Integration.GiantModeController, Tartaria.Integration");
                _dialogueManager = Type.GetType("Tartaria.Integration.DialogueManager, Tartaria.Integration");
            }
            catch (Exception e) { Debug.LogWarning($"[IntegrationBridge] resolve failed: {e.Message}"); }
        }

        // --- CombatBridge --------------------------------------------------
        public static float GetPlayerCurrentFrequency()
        {
            Resolve();
            if (_combatBridge == null) return -1f; // Sentinel: CombatBridge not loaded
            try
            {
                var m = _combatBridge.GetMethod("GetPlayerCurrentFrequency", BindingFlags.Public | BindingFlags.Static);
                if (m != null && m.ReturnType == typeof(float))
                    return (float)m.Invoke(null, null);
            }
            catch (Exception e) { Debug.LogWarning($"[IntegrationBridge] GetPlayerCurrentFrequency: {e.Message}"); }
            return -1f; // Sentinel: error during reflection call
        }

        // --- BossEncounterSystem -------------------------------------------
        static object GetBossInstance()
        {
            Resolve();
            if (_bossEncounter == null) return null;
            try
            {
                var prop = _bossEncounter.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                return prop?.GetValue(null);
            }
            catch { return null; }
        }

        public static bool IsBossActive()
        {
            var inst = GetBossInstance();
            if (inst == null) return false;
            try
            {
                var p = inst.GetType().GetProperty("IsActive", BindingFlags.Public | BindingFlags.Instance);
                return p != null && (bool)p.GetValue(inst);
            }
            catch { return false; }
        }

        public static float BossCurrentTargetFrequency()
        {
            var inst = GetBossInstance();
            if (inst == null) return 0f;
            try
            {
                var p = inst.GetType().GetProperty("CurrentTargetFrequency", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(float))
                    return (float)p.GetValue(inst);
            }
            catch { }
            return 0f;
        }

        public static float BossHealthFraction()
        {
            var inst = GetBossInstance();
            if (inst == null) return 0f;
            try
            {
                var p = inst.GetType().GetProperty("HealthFraction", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(float))
                    return Mathf.Clamp01((float)p.GetValue(inst));
            }
            catch { }
            return 0f;
        }

        public static string BossDisplayName()
        {
            var inst = GetBossInstance();
            if (inst == null) return string.Empty;
            try
            {
                var p = inst.GetType().GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(string))
                    return (string)p.GetValue(inst) ?? string.Empty;
            }
            catch { }
            return string.Empty;
        }

        // --- GiantModeController -------------------------------------------
        static object GetGiantInstance()
        {
            Resolve();
            if (_giantMode == null) return null;
            try
            {
                var prop = _giantMode.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                return prop?.GetValue(null);
            }
            catch { return null; }
        }

        public static bool HasGiantInstance() => GetGiantInstance() != null;

        public static float GiantReadiness()
        {
            var inst = GetGiantInstance();
            if (inst == null) return 0f;
            try
            {
                var p = inst.GetType().GetProperty("Readiness", BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.PropertyType == typeof(float))
                    return (float)p.GetValue(inst);
            }
            catch { }
            return 0f;
        }

        // --- DialogueManager -----------------------------------------------
        static object GetDialogueManagerInstance()
        {
            Resolve();
            if (_dialogueManager == null) return null;
            try
            {
                var prop = _dialogueManager.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                return prop?.GetValue(null);
            }
            catch { return null; }
        }

        /// <summary>
        /// Play a dialogue line via reflection to DialogueManager.PlayLine(contextId, lineId).
        /// Avoids direct Tartaria.Integration dependency from Tartaria.UI.
        /// </summary>
        public static void PlayDialogueLine(string contextId, string lineId)
        {
            var inst = GetDialogueManagerInstance();
            if (inst == null)
            {
                Debug.LogWarning($"[IntegrationBridge] DialogueManager.Instance is null. Cannot play line: {contextId}/{lineId}");
                return;
            }
            try
            {
                var method = inst.GetType().GetMethod("PlayLine", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(string) }, null);
                if (method != null)
                {
                    method.Invoke(inst, new object[] { contextId, lineId });
                }
                else
                {
                    Debug.LogWarning("[IntegrationBridge] DialogueManager.PlayLine(string, string) method not found.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[IntegrationBridge] PlayDialogueLine failed: {e.Message}");
            }
        }
    }
}
