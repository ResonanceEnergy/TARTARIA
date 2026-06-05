// File: Assets/_Project/Scripts/Integration/HovlVFXBindings.cs

using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    public static class HovlVFXBindings
    {
        const string HOVL_ROOT = "Assets/Hovl Studio/Magic effects pack/Prefabs/";

        static readonly System.Collections.Generic.Dictionary<string, string> _slots =
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "restoration_burst",  "AoE effects/Ground AOE explosion.prefab" },
                { "restoration_pillar", "AoE effects/Laser AOE.prefab" },
                { "resonance_blue",  "AoE effects/AoE slash blue.prefab" },
                { "resonance_green", "AoE effects/AoE slash green.prefab" },
                { "resonance_orange", "AoE effects/AoE slash orange.prefab" },
                { "crystal_idle",   "AoE effects/Crystals crossfade.prefab" },
                { "crystal_attack",  "AoE effects/Crystals front attack.prefab" },
                { "meteor_ritual", "AoE effects/Meteors AOE.prefab" },
                { "plexus_aether", "AoE effects/Plexus AoE.prefab" },
                { "hit_spark",     "AoE effects/Ground AOE explosion.prefab" },
                { "giant_slam",    "AoE effects/Meteors AOE.prefab" },
                { "dissonance_pulse", "AoE effects/AoE slash orange.prefab" },
            };

        public static GameObject Spawn(string slot, Vector3 position, Transform parent = null, float autoDestroyAfterSeconds = 4f)
        {
            if (!_slots.TryGetValue(slot, out var rel))
            {
                Debug.LogWarning($"ovlVFXBindings] Unknown slot: {slot}");
                return null;
            }
            var prefab = LoadPrefab(rel);
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab, position, Quaternion.identity, parent);
            if (autoDestroyAfterSeconds > 0f) Object.Destroy(go, autoDestroyAfterSeconds);
            return go;
        }

        static GameObject LoadPrefab(string relPath)
        {
#if UNITY_EDITOR
            var full = HOVL_ROOT + relPath;
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(full);
            if (asset == null) Debug.LogWarning($"ovlVFXBindings] Asset not found: {full}");
            return asset;
#else
            // Runtime fallback if assets are moved to a Resources folder later
            var name = System.IO.Path.GetFileNameWithoutExtension(relPath);
            return Resources.Load<GameObject>("HovlVFX/" + name);
#endif
        }
    }
}
