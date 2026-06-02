// DamagePopupPrefabBuilder.cs
// Sprint 6 Lane 5 — agent/anim/combat-hit-feedback
//
// Editor utility that authors the DamagePopup prefab from code so the prefab is
// reproducible and version-controllable. Run from Tartaria/Combat/Build Damage Popup Prefab.
//
// The prefab is saved at Assets/_Project/Resources/Combat/DamagePopup.prefab so
// HitFeedback.cs can resolve it via Resources.Load<GameObject>("Combat/DamagePopup")
// at runtime when the inline serialized prefab field is null.

using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;
using Tartaria.Gameplay.Combat;

namespace Tartaria.Editor.Combat
{
    public static class DamagePopupPrefabBuilder
    {
        private const string ResourcesRoot = "Assets/_Project/Resources";
        private const string CombatFolder = "Combat";
        private const string PrefabRelative = "Combat/DamagePopup.prefab";
        private const string PrefabFullPath = "Assets/_Project/Resources/Combat/DamagePopup.prefab";

        [MenuItem("Tartaria/Combat/Build Damage Popup Prefab")]
        public static void BuildDamagePopupPrefab()
        {
            EnsureFolders();

            // ----------------------------------------------------------------
            // Root GameObject — holds the DamagePopup behaviour + Animator.
            // ----------------------------------------------------------------
            var root = new GameObject("DamagePopup");
            var popup = root.AddComponent<DamagePopup>();
            var animator = root.AddComponent<Animator>();
            // No controller assigned — DamagePopup drives motion/alpha via coroutine.
            // Animator is added per lane spec ("authors the prefab + TextMeshPro + Animator").
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            // ----------------------------------------------------------------
            // TextMeshPro child — the floating number itself, world-space.
            // ----------------------------------------------------------------
            var tmpGO = new GameObject("Label");
            tmpGO.transform.SetParent(root.transform, worldPositionStays: false);
            var tmp = tmpGO.AddComponent<TextMeshPro>();
            tmp.text = "0";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color32(0, 0, 0, 255);

            // Bind the TMP into the DamagePopup serialized field so it doesn't need to
            // hunt at runtime. We use SerializedObject so the field saves into the prefab.
            var so = new SerializedObject(popup);
            var labelProp = so.FindProperty("_label");
            if (labelProp != null)
            {
                labelProp.objectReferenceValue = tmp;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("[DamagePopupPrefabBuilder] Could not find '_label' serialized field on DamagePopup — runtime auto-resolve will run instead.");
            }

            // ----------------------------------------------------------------
            // Save as prefab into Resources so HitFeedback can Resources.Load it.
            // ----------------------------------------------------------------
            string fullDir = Path.Combine(ResourcesRoot, CombatFolder).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(fullDir))
            {
                Debug.LogError($"[DamagePopupPrefabBuilder] Combat folder missing at '{fullDir}' after EnsureFolders — aborting.");
                Object.DestroyImmediate(root);
                return;
            }

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabFullPath, out bool success);
            Object.DestroyImmediate(root);

            if (!success || saved == null)
            {
                Debug.LogError($"[DamagePopupPrefabBuilder] PrefabUtility.SaveAsPrefabAsset failed for '{PrefabFullPath}'. Check that the path's parent folder exists and the project is writable.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DamagePopupPrefabBuilder] DamagePopup prefab written to '{PrefabFullPath}'. HitFeedback will load it via Resources.Load<GameObject>(\"Combat/DamagePopup\").");
            EditorGUIUtility.PingObject(saved);
            Selection.activeObject = saved;
        }

        private static void EnsureFolders()
        {
            string assets = "Assets";
            string project = "Assets/_Project";
            string resources = ResourcesRoot;
            string combat = ResourcesRoot + "/" + CombatFolder;

            if (!AssetDatabase.IsValidFolder(project))
            {
                AssetDatabase.CreateFolder(assets, "_Project");
            }
            if (!AssetDatabase.IsValidFolder(resources))
            {
                AssetDatabase.CreateFolder(project, "Resources");
            }
            if (!AssetDatabase.IsValidFolder(combat))
            {
                AssetDatabase.CreateFolder(resources, CombatFolder);
            }
        }
    }
}
