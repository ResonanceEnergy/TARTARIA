#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Three Editor menus for Moon 1 character art pipeline:
    /// (1) Build ResetScout prefab from Char_Rogue_Hooded (Victorian palette)
    /// (2) Triage .corrupt character siblings
    /// (3) Attach KayKit equipment FBX to character prefabs
    ///
    /// Per CLAUDE.md "no stubs" — every menu actually moves real files
    /// and creates real prefab variants, not log spam.
    /// </summary>
    public static class Moon1CharacterPipeline
    {
        const string OUT_DIR_ENEMIES   = "Assets/_Project/Prefabs/Enemies";
        const string OUT_DIR_CHARS     = "Assets/_Project/Prefabs/Characters";

        // ─────────────────────────────────────────────────────────────
        // MENU 1 — Build ResetScout.prefab from KayKit Char_Rogue_Hooded
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/4 Generate Art/ResetScout Prefab", priority = 498)]
        public static void BuildResetScoutPrefab()
        {
            const string srcPath = "Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab";
            const string outPath = OUT_DIR_ENEMIES + "/ResetScout.prefab";

            var src = AssetDatabase.LoadAssetAtPath<GameObject>(srcPath);
            if (src == null)
            {
                EditorUtility.DisplayDialog("ResetScout",
                    "Source prefab not found: " + srcPath, "OK");
                return;
            }

            EnsureDir(OUT_DIR_ENEMIES);

            // Instantiate, tint Victorian (dark coat + pale skin tones), save as new prefab
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(src);
            instance.name = "ResetScout";

            // Apply Victorian black-coat URP material to all renderers
            var victorianCoat = new Color(0.10f, 0.09f, 0.12f);
            var paleSkin     = new Color(0.83f, 0.74f, 0.66f);
            var crimsonAccent= new Color(0.55f, 0.10f, 0.12f);

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            int colored = 0;
            foreach (var r in renderers)
            {
                if (r.sharedMaterial == null) continue;
                var lower = r.name.ToLowerInvariant();
                Color tint = victorianCoat;
                if (lower.Contains("head") || lower.Contains("face") || lower.Contains("hand")) tint = paleSkin;
                else if (lower.Contains("belt") || lower.Contains("trim") || lower.Contains("accent")) tint = crimsonAccent;

                var mat = new Material(r.sharedMaterial);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
                else mat.color = tint;
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
                r.sharedMaterial = mat;
                colored++;
            }

            // Save as a new top-level prefab (not a variant — full ownership)
            var saved = PrefabUtility.SaveAsPrefabAsset(instance, outPath);
            Object.DestroyImmediate(instance);

            EditorUtility.DisplayDialog("ResetScout",
                "Saved " + outPath + "\nTinted " + colored + " renderers in Victorian palette.", "OK");
            AssetDatabase.SaveAssets();
            Selection.activeObject = saved;
        }

        // ─────────────────────────────────────────────────────────────
        // MENU 2 — Triage .corrupt character siblings
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/6 Scene Tools/Triage Corrupt Characters", priority = 670)]
        public static void TriageCorruptCharacters()
        {
            var sb = new System.Text.StringBuilder();
            int found = 0, deleted = 0;

            string[] characterDirs = { OUT_DIR_CHARS };
            foreach (var dir in characterDirs)
            {
                string full = Path.Combine(Directory.GetCurrentDirectory(), dir);
                if (!Directory.Exists(full)) continue;
                foreach (var corruptPath in Directory.GetFiles(full, "*.prefab.corrupt", SearchOption.AllDirectories))
                {
                    found++;
                    var leaf = Path.GetFileName(corruptPath);
                    var siblingPrefab = corruptPath.Substring(0, corruptPath.Length - ".corrupt".Length);
                    var siblingExists = File.Exists(siblingPrefab);
                    long corruptSize = new FileInfo(corruptPath).Length;
                    long siblingSize = siblingExists ? new FileInfo(siblingPrefab).Length : 0;

                    // If a valid .prefab sibling exists AND is larger than the corrupt, delete corrupt
                    if (siblingExists && siblingSize > corruptSize)
                    {
                        File.Delete(corruptPath);
                        var meta = corruptPath + ".meta";
                        if (File.Exists(meta)) File.Delete(meta);
                        sb.AppendLine("[DEL] " + leaf + " (sibling " + siblingSize + "b > corrupt " + corruptSize + "b)");
                        deleted++;
                    }
                    else
                    {
                        sb.AppendLine("[KEEP] " + leaf + " (no live sibling or corrupt larger — manual review)");
                    }
                }
            }
            string header = "Found " + found + " .corrupt files, deleted " + deleted + ".\n\n";
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Corrupt Character Triage", header + sb.ToString(), "OK");
            Debug.Log("[Moon1CharacterPipeline] Triage: " + header + sb.ToString());
        }

        // ─────────────────────────────────────────────────────────────
        // MENU 3 — Attach KayKit weapons/equipment to characters
        // ─────────────────────────────────────────────────────────────

        [MenuItem("Tartaria/3 Wire/Attach KayKit Equipment", priority = 330)]
        public static void AttachKayKitEquipment()
        {
            // Per-character archetype: model + equipment leaf path inside KayKit_Adventurers
            // (Best-effort — if a weapon isn't found, the character keeps existing geometry only.)
            var pairs = new (string charPrefabName, string equipmentFbx)[]
            {
                ("Char_Knight",        "Sword"),
                ("Char_Mage",          "Staff"),
                ("Char_Ranger",        "Bow"),
                ("Char_Rogue",         "Knife"),
                ("Char_Barbarian",     "Axe"),
                ("Char_Rogue_Hooded",  "Knife"),
            };

            int attached = 0;
            foreach (var (charName, weaponName) in pairs)
            {
                var charPath = "Assets/_Project/Prefabs/Characters/KayKit/" + charName + ".prefab";
                var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(charPath);
                if (charPrefab == null) continue;

                // Find weapon FBX anywhere in KayKit_Adventurers
                var weaponGuids = AssetDatabase.FindAssets(weaponName + " t:Model", new[] { "Assets/KayKit_Adventurers_2.0_FREE" });
                if (weaponGuids == null || weaponGuids.Length == 0) continue;
                var weaponPath = AssetDatabase.GUIDToAssetPath(weaponGuids[0]);
                var weaponFbx = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
                if (weaponFbx == null) continue;

                // Edit prefab contents — add weapon as a child of root
                var instanceRoot = PrefabUtility.LoadPrefabContents(charPath);
                // Don't double-add
                if (instanceRoot.transform.Find("Weapon_" + weaponName) == null)
                {
                    var weaponInst = (GameObject)PrefabUtility.InstantiatePrefab(weaponFbx, instanceRoot.transform);
                    weaponInst.name = "Weapon_" + weaponName;
                    // Approximate hand-grip offset (right-handed hold)
                    weaponInst.transform.localPosition = new Vector3(0.32f, 1.2f, 0.05f);
                    weaponInst.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    PrefabUtility.SaveAsPrefabAsset(instanceRoot, charPath);
                    attached++;
                }
                PrefabUtility.UnloadPrefabContents(instanceRoot);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Equipment Attach",
                "Attached weapons to " + attached + " character prefabs.", "OK");
        }

        static void EnsureDir(string projectRelative)
        {
            string full = Path.Combine(Directory.GetCurrentDirectory(), projectRelative);
            if (!Directory.Exists(full)) Directory.CreateDirectory(full);
            AssetDatabase.Refresh();
        }
    }
}
#endif
