// RuntimePBRApplier.cs
// Re-applies PBR materials at runtime to renderers spawned AFTER the editor
// PBRSceneApplier ran (e.g. EchohavenContentSpawner.EnsureBuildingVisualDetails
// which creates Detail_AntennaSpire, Detail_Buttress_*, Detail_OrbFinial,
// Detail_CrystalCluster shards via GameObject.CreatePrimitive — those start
// with default URP/Lit materials and bypass the editor PBR pass).
//
// Loads all materials from Resources/PBR (created by PBRResourceCopier on build)
// and matches them by GameObject name keyword.
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(500)]
    public class RuntimePBRApplier : MonoBehaviour
    {
        static readonly (string key, string mat)[] NameRules = new (string, string)[]
        {
            ("mud", "Ground054"), ("ground", "Ground054"), ("terrain", "Ground054"),
            ("dirt", "Ground054"), ("grass", "Ground054"), ("foliage", "Ground054"),
            ("path", "PavingStones150"), ("road", "PavingStones150"),
            ("cobble", "PavingStones150"), ("paving", "PavingStones150"),
            ("brick", "Bricks075A"), ("ruin", "Bricks075A"),
            ("plaster", "Plaster001"), ("wall", "Plaster001"),
            ("dome", "Marble006"), ("marble", "Marble006"), ("floor", "Marble006"),
            ("cathedral", "Marble006"), ("fountain", "Marble006"),
            ("body", "Marble006"), ("orb", "Marble006"),
            ("plank", "Wood063"), ("beam", "Wood063"), ("wood", "Wood063"),
            ("crate", "Wood063"), ("door", "Wood063"),
            ("copper", "Metal047B"), ("pipe", "Metal047B"),
            ("gold", "Metal048A"), ("finial", "Metal048A"), ("trim", "Metal048A"),
            ("ring", "Metal048A"), ("antenna", "Metal048A"), ("spire", "Metal048A"),
            ("crystal", "Metal048A"), ("buttress", "Metal048A"), ("shard", "Metal048A"),
            ("iron", "Metal032"), ("rust", "Metal032"), ("rail", "Metal032"),
            ("bronze", "MetalPlates006"), ("plate", "MetalPlates006"), ("panel", "MetalPlates006"),
            ("rock", "Rocks023"), ("rubble", "Rocks023"), ("stone", "Rocks023"), ("boulder", "Rocks023"),
        };

        Dictionary<string, Material> _cache;
        bool _ran;

        void Start()  { TryApply(); }
        void OnEnable() { TryApply(); }

        void TryApply()
        {
            if (_ran) return;
            _ran = true;
            _cache = new Dictionary<string, Material>();
            var all = Resources.LoadAll<Material>("PBR");
            if (all == null || all.Length == 0)
            {
                Debug.LogWarning("[RuntimePBR] No PBR materials in Resources/PBR — skipping.");
                return;
            }
            for (int i = 0; i < all.Length; i++) _cache[all[i].name] = all[i];

            int applied = 0, scanned = 0;
            var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                scanned++;
                var existing = r.sharedMaterial;
                // Only re-skin renderers whose current material is NOT already a PBR one.
                if (existing != null && _cache.ContainsKey(existing.name)) continue;

                string lookup = (r.gameObject.name + " " + (r.transform.parent != null ? r.transform.parent.name : "")).ToLowerInvariant();
                Material chosen = null;
                for (int k = 0; k < NameRules.Length; k++)
                {
                    if (lookup.Contains(NameRules[k].key) && _cache.TryGetValue(NameRules[k].mat, out chosen))
                        break;
                }
                if (chosen != null)
                {
                    r.sharedMaterial = chosen;
                    applied++;
                }
            }
            Debug.Log($"[RuntimePBR] Applied={applied} Scanned={scanned} Cache={_cache.Count}");
        }
    }
}
