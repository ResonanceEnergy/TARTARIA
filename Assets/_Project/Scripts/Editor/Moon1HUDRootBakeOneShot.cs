// Moon1HUDRootBakeOneShot.cs — 2026-06-04 NIGHT LOCK-DOWN
//
// Auto-fires once on next Unity Editor launch to bake a SKELETON HUD_Root.prefab to
//   Assets/_Project/Resources/Prefabs/UI/HUD_Root.prefab
// so that RuntimeHUDBuilder.Bootstrap()'s prefab-first path (Resources.Load<GameObject>
// at line ~83 of RuntimeHUDBuilder.cs, Wave 6) succeeds and skips the 64-GameObject
// runtime fabrication.
//
// IMPORTANT CAVEAT — this is a SKELETON prefab, not a 1:1 RuntimeHUDBuilder replacement.
// RuntimeHUDBuilder.BuildCanvas() spawns ~64 panels (RSGauge, AetherBar, BossHealthPanel,
// WaveCounterPanel, AchievementToast, MoonTrophyPanel, ObjectivePanel, FrequencyWheel,
// GiantMeter, AccessibilityHint, BossTargetFreq, 3x AbilityCooldown indicators, PauseMenu,
// QuestLog, TuningOverlay, DiscoveryFlash, WorldMap, SkillTree, AetherVisionOverlay,
// Archive, ControlsHint, QuestToast, DialoguePanel, MissionBriefing, DamageNumberPool).
// Replicating all of those + their SerializeField wiring to HUDController / QuestLogUI /
// QuestToastNotification / UIManager / MissionBriefingDismisser would be a full migration.
//
// This skeleton creates:
//   - Canvas (ScreenSpaceOverlay, sortingOrder 100) + CanvasScaler + GraphicRaycaster
//   - 7 major-section RectTransform anchors (TopBar / BottomBar / LeftPanel / RightPanel
//     / CenterReticle / InteractionPrompt / BannerLayer)
//   - Named child GameObjects (RSText / AetherText / RSDisplayText / PauseMenu /
//     TuningOverlay) that match RuntimeHUDBuilder.RecacheLiveDataRefsFromPrefab()'s
//     name-based lookups, so OnRSChanged / OnAetherEnergyChanged still drive visible text
//     when the prefab-first path activates.
//   - HUDController MonoBehaviour attached at root (reflection lookup, asmdef-safe)
//
// Once the prefab exists on disk, RuntimeHUDBuilder's Bootstrap() sees it and short-circuits
// runtime fabrication. Full RuntimeHUDBuilder→prefab migration of the remaining 50+ panels
// is left as a separate task.
//
// Reset via Tartaria/8 Fix/Reset HUD Root Bake OneShot.
// Re-run via Tartaria/8 Fix/Run HUD Root Bake NOW.

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace Tartaria.Editor.OneShots
{
    [InitializeOnLoad]
    public static class Moon1HUDRootBakeOneShot
    {
        const string PREF_KEY = "Tartaria.OneShot.HUDRootBake.2026-06-04";
        const string PREFAB_PATH = "Assets/_Project/Resources/Prefabs/UI/HUD_Root.prefab";

        static Moon1HUDRootBakeOneShot()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false)) return;
            EditorApplication.delayCall += Run;
        }

        [MenuItem("Tartaria/8 Fix/Reset HUD Root Bake OneShot", priority = 997)]
        static void ResetFlag()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("[HUDRootBakeOneShot] Flag cleared. Will fire again on next domain reload.");
        }

        [MenuItem("Tartaria/8 Fix/Run HUD Root Bake NOW", priority = 996)]
        static void RunNow() => Run();

        static void Run()
        {
            if (EditorPrefs.GetBool(PREF_KEY, false))
            {
                Debug.Log("[HUDRootBakeOneShot] Already ran. Skip.");
                return;
            }

            // Skip if prefab already exists — someone authored it manually or a prior run succeeded.
            if (File.Exists(PREFAB_PATH))
            {
                Debug.Log($"[HUDRootBakeOneShot] Prefab already exists at {PREFAB_PATH}. Setting flag and skipping.");
                EditorPrefs.SetBool(PREF_KEY, true);
                return;
            }

            // Build the HUD root tree under a temp GameObject, then save as prefab + destroy temp.
            var root = new GameObject("HUD_Root");
            try
            {
                // Canvas
                var canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                var scaler = root.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;

                root.AddComponent<GraphicRaycaster>();

                // Major sections — anchored to mirror RuntimeHUDBuilder's primary panel layout.
                BuildSection(root, "TopBar",            new Vector2(0f, 1f),    new Vector2(1f, 1f),    new Vector2(0f, -40f), new Vector2(0f, 80f));
                BuildSection(root, "BottomBar",         new Vector2(0f, 0f),    new Vector2(1f, 0f),    new Vector2(0f, 40f),  new Vector2(0f, 80f));
                BuildSection(root, "LeftPanel",         new Vector2(0f, 0.5f),  new Vector2(0f, 0.5f),  new Vector2(40f, 0f),  new Vector2(240f, 200f));
                BuildSection(root, "RightPanel",        new Vector2(1f, 0.5f),  new Vector2(1f, 0.5f),  new Vector2(-40f, 0f), new Vector2(240f, 200f));
                BuildSection(root, "CenterReticle",     new Vector2(0.5f, 0.5f),new Vector2(0.5f, 0.5f),Vector2.zero,          new Vector2(40f, 40f));
                BuildSection(root, "InteractionPrompt", new Vector2(0.5f, 0.2f),new Vector2(0.5f, 0.2f),Vector2.zero,          new Vector2(400f, 60f));
                BuildSection(root, "BannerLayer",       new Vector2(0.5f, 0.7f),new Vector2(0.5f, 0.7f),Vector2.zero,          new Vector2(800f, 100f));

                // Named placeholders matching RuntimeHUDBuilder.RecacheLiveDataRefsFromPrefab() lookups
                // (FindDescendantByName). When the prefab-first path runs, these get their TMP_Text
                // components found and bound to OnRSChanged / OnAetherEnergyChanged subscribers.
                CreateNamed(root, "RSText");
                CreateNamed(root, "AetherText");
                CreateNamed(root, "RSDisplayText");
                CreateNamed(root, "PauseMenu", active: false);
                CreateNamed(root, "TuningOverlay", active: false);

                // Attach HUDController via reflection (asmdef-boundary safe — Tartaria.UI may not be
                // referenced by Tartaria.Editor). If the type isn't resolvable, skip — RuntimeHUDBuilder
                // will still own its own HUDController spawn under the host GameObject in fallback paths.
                var hudT = ResolveType("Tartaria.UI.HUDController");
                if (hudT != null)
                {
                    root.AddComponent(hudT);
                    Debug.Log("[HUDRootBakeOneShot] + HUDController attached at root.");
                }
                else
                {
                    Debug.LogWarning("[HUDRootBakeOneShot] Tartaria.UI.HUDController type not resolvable from Editor asmdef — skipped attach. RuntimeHUDBuilder fallback path will still create one.");
                }

                EnsureDir(PREFAB_PATH);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
                if (saved == null)
                {
                    Debug.LogError($"[HUDRootBakeOneShot] SaveAsPrefabAsset returned null for {PREFAB_PATH}. Aborting — flag NOT set so it will retry.");
                    return;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorPrefs.SetBool(PREF_KEY, true);
                Debug.Log($"[HUDRootBakeOneShot] OK — baked skeleton {PREFAB_PATH}. RuntimeHUDBuilder prefab-first path will now activate on next Play. Full panel migration still pending.");
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
            }
        }

        static void BuildSection(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        static void CreateNamed(GameObject parent, string name, bool active = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            go.SetActive(active);
        }

        static System.Type ResolveType(string fullName)
        {
            var t = System.Type.GetType(fullName);
            if (t != null) return t;
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        static void EnsureDir(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(dir)) return;

            var parts = dir.Split('/');
            var build = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = build + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(build, parts[i]);
                }
                build = next;
            }
        }
    }
}
