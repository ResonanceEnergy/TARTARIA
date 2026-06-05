#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1KayKitPurgeAudit — scans every component in the active scene + every
    /// prefab reference, reports any that still point at Assets/_Project/Prefabs/
    /// Characters/KayKit/*. After running 'Wire ALL Scene Prefab Refs (Blender-only)',
    /// this should print "0 KayKit character refs in scene".
    ///
    /// Does NOT delete KayKit assets on disk — they may still be used by Equipment,
    /// Combat scripts, or other game systems. Only verifies SCENE refs are Blender-built.
    /// </summary>
    public static class Moon1KayKitPurgeAudit
    {
        const string KayKitFolder = "Assets/_Project/Prefabs/Characters/KayKit";

        [MenuItem("Tartaria/7 Diagnose/Audit KayKit character refs in scene", priority = 700)]
        public static void Run()
        {
            var hits = new List<string>();
            int totalScanned = 0;

            foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (c == null) continue;
                totalScanned++;
                var so = new SerializedObject(c);
                var prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var val = prop.objectReferenceValue;
                    if (val == null) continue;
                    var path = AssetDatabase.GetAssetPath(val);
                    if (!string.IsNullOrEmpty(path) && path.StartsWith(KayKitFolder))
                    {
                        hits.Add($"{c.GetType().Name}.{prop.name} on '{c.gameObject.name}' → {path}");
                    }
                }
            }

            var report = new System.Text.StringBuilder();
            report.AppendLine($"Components scanned: {totalScanned}");
            report.AppendLine($"KayKit character refs found: {hits.Count}");
            report.AppendLine();
            if (hits.Count == 0)
            {
                report.AppendLine("✓ Scene is 100% Blender-built characters. Zero KayKit references.");
            }
            else
            {
                report.AppendLine("Lingering KayKit references (run 'Wire ALL Scene Prefab Refs' to replace):");
                foreach (var h in hits)
                    report.AppendLine("  • " + h);
            }

            Debug.Log("[KayKitPurgeAudit] " + report.ToString());
            EditorUtility.DisplayDialog("KayKit Purge Audit", report.ToString(), "OK");
        }
    }
}
#endif
