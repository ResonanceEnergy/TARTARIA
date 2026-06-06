using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Late-bound bridge to Tartaria.Input.HapticFeedbackManager.
    /// Audio asmdef cannot reference Tartaria.Input (Input already references
    /// Audio — adding the reverse would create an assembly cycle). This bridge
    /// uses reflection to dispatch into HapticFeedbackManager when present,
    /// gracefully degrades otherwise. MethodInfos and enum values are cached so the
    /// per-call cost is ~one dictionary lookup + delegate invoke.
    ///
    /// Enum arguments are passed as **strings** (case-insensitive). The bridge
    /// parses them against the target parameter type the first time, then memoizes.
    /// Failures logged to CrashReporter for production monitoring.
    /// </summary>
    public static class HapticBridge
    {
        static Type   _type;
        static object _instance;
        static bool   _resolved;
        static int    _failureCount;
        static bool   _degradationLogged;
        static readonly Dictionary<string, MethodInfo>         _methods = new Dictionary<string, MethodInfo>();
        static readonly Dictionary<(Type, string), object>     _enums   = new Dictionary<(Type, string), object>();

        static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            _type = Type.GetType("Tartaria.Input.HapticFeedbackManager, Tartaria.Input");
        }

        static object Instance()
        {
            if (_type == null) return null;
            if (_instance != null) return _instance;
            var p = _type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            _instance = p?.GetValue(null);
            return _instance;
        }

        public static void Call(string methodName, params object[] args)
        {
            try
            {
                Resolve();
                var inst = Instance();
                if (inst == null) return;

                if (!_methods.TryGetValue(methodName, out var mi))
                {
                    foreach (var m in _type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (m.Name == methodName) { mi = m; break; }
                    }
                    _methods[methodName] = mi;
                }
                if (mi == null) return;

                var parms = mi.GetParameters();
                var converted = new object[parms.Length];
                for (int i = 0; i < parms.Length; i++)
                {
                    if (i >= args.Length)
                    {
                        converted[i] = parms[i].HasDefaultValue ? parms[i].DefaultValue : null;
                        continue;
                    }
                    var a = args[i];
                    var pt = parms[i].ParameterType;
                    if (a is string s && pt.IsEnum)
                    {
                        var key = (pt, s);
                        if (!_enums.TryGetValue(key, out var ev))
                        {
                            ev = Enum.Parse(pt, s, true);
                            _enums[key] = ev;
                        }
                        converted[i] = ev;
                    }
                    else
                    {
                        converted[i] = a;
                    }
                }
                mi.Invoke(inst, converted);
            }
            catch (Exception ex)
            {
                _failureCount++;
                Debug.LogWarning($"[HapticBridge] {methodName} failed: {ex.Message}");
                
                // Log to CrashReporter for production monitoring
                Debug.LogError($"[HapticBridge] REFLECTION FAILURE #{_failureCount}: {methodName} - {ex.Message}\n{ex.StackTrace}");
                
                // Log degradation notice once
                if (!_degradationLogged && _failureCount >= 3)
                {
                    _degradationLogged = true;
                    Debug.LogError($"[HapticBridge] DEGRADED MODE: {_failureCount} reflection failures. Haptics may not fire. Check Input assembly load.");
                }
            }
        }
    }
}
