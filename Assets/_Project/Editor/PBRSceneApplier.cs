// PBRSceneApplier.cs
// Aggressive PBR material binder:
//   1. Walks every Renderer in the active scene.
//   2. Tries name/parent/grandparent keyword match first.
//   3. Falls back to MATERIAL-NAME match (e.g. M_Wall_Stone -> Plaster001).
// Idempotent: skips renderers whose material is already from /Materials/PBR/.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class PBRSceneApplier
    {
        const string MatRoot = "Assets/_Project/Materials/PBR";

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
            ("crystal", "Metal048A"), ("buttress", "Metal048A"),
            ("iron", "Metal032"), ("rust", "Metal032"), ("rail", "Metal032"),
            ("bronze", "MetalPlates006"), ("plate", "MetalPlates006"), ("panel", "MetalPlates006"),
            ("rock", "Rocks023"), ("rubble", "Rocks023"), ("stone", "Rocks023"), ("boulder", "Rocks023"),
        };

        static readonly (string key, string mat)[] MatNameRules = new (string, string)[]
        {
            ("ground", "Ground054"), ("terrain", "Ground054"), ("grass", "Ground054"),
            ("dirt", "Ground054"),
            ("path", "PavingStones150"), ("paving", "PavingStones150"),
            ("wall", "Plaster001"), ("plaster", "Plaster001"),
            ("brick", "Bricks075A"),
            ("dome", "Marble006"), ("marble", "Marble006"),
            ("fountain", "Marble006"), ("orb", "Marble006"),
            ("wood", "Wood063"), ("plank", "Wood063"),
            ("metal", "Metal048A"), ("gold", "Metal048A"), ("ring", "Metal048A"),
            ("crystal", "Metal048A"), ("spire", "Metal048A"),
            ("rock", "Rocks023"), ("stone", "Rocks023"), ("rubble", "Rocks023"),
        };

        [MenuItem("Tartaria/Setup/Apply PBR To Scene", false, 66)]
        public static void Apply()
        {
            var matCache = new Dictionary<string, Material>();
            Material LoadMat(string file)
            {
                if (matCache.TryGetValue(file, out var cached)) return cached;
                var m = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/{file}.mat");
                matCache[file] = m;
                return m;
            }

            int applied = 0, skipped = 0;
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (r == null || r.gameObject == null) continue;
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) { skipped++; continue; }

                if (r.sharedMaterial != null && AssetDatabase.GetAssetPath(r.sharedMaterial).Contains("/Materials/PBR/"))
                {
                    skipped++;
                    continue;
                }

                string n = r.gameObject.name.ToLowerInvariant();
                string parent = r.transform.parent ? r.transform.parent.name.ToLowerInvariant() : "";
                string grand = r.transform.parent && r.transform.parent.parent ? r.transform.parent.parent.name.ToLowerInvariant() : "";
                string matName = r.sharedMaterial != null ? r.sharedMaterial.name.ToLowerInvariant() : "";

                Material chosen = null;
                string chosenFile = null;

                foreach (var (k, f) in NameRules)
                {
                    if (n.Contains(k) || parent.Contains(k) || grand.Contains(k))
                    {
                        chosen = LoadMat(f); chosenFile = f;
                        break;
                    }
                }

                if (chosen == null && !string.IsNullOrEmpty(matName))
                {
                    foreach (var (k, f) in MatNameRules)
                    {
                        if (matName.Contains(k))
                        {
                            chosen = LoadMat(f); chosenFile = f;
                            break;
                        }
                    }
                }

                if (chosen == null) { skipped++; continue; }

                r.sharedMaterial = chosen;
                applied++;
                if (applied <= 30)
                    Debug.Log($"[Tartaria][PBRApply] {r.gameObject.name} (parent={parent}, mat={matName}) <- {chosenFile}");
            }
            Debug.Log($"[Tartaria][PBRApply] Applied={applied} Skipped={skipped} Total={renderers.Length}");
        }
    }
}
