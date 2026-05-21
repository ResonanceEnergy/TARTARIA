using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Late-bound bridge to Tartaria.Integration.VFXController.
    /// Audio asmdef cannot reference Tartaria.Integration (would invert the
    /// established dependency direction). Reflection dispatch with cached
    /// MethodInfos. Silently no-ops if VFXController isn't loaded.
    /// Enums (if any) are passed as strings — parsed once, then memoized.
    /// </summary>
    internal static class VFXBridge
    {
        static Type   _type;
        static object _instance;
        static bool   _resolved;
        static readonly Dictionary<string, MethodInfo>     _methods = new Dictionary<string, MethodInfo>();
        static readonly Dictionary<(Type, string), object> _enums   = new Dictionary<(Type, string), object>();

        static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            _type = Type.GetType("Tartaria.Integration.VFXController, Tartaria.Integration");
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
                Debug.LogWarning($"[VFXBridge] {methodName} failed: {ex.Message}");
            }
        }
    }
}
