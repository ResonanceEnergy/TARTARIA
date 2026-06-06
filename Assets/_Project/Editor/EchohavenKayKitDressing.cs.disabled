using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Drops KayKit character + prop prefabs into the Echohaven vertical-slice
    /// scene so the imported assets are actually visible in the play session.
    /// Idempotent — uses a single root marker "KayKit_Dressing" that is
    /// regenerated each pass. Silently no-ops when the KayKit prefab folders
    /// don't exist yet.
    /// </summary>
    public static class EchohavenKayKitDressing
    {
        const string ScenePath        = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string AdventurersDir   = "Assets/_Project/Prefabs/Characters/KayKit";
        const string SkeletonsCharDir = "Assets/_Project/Prefabs/Characters/KayKit/Skeletons";
        const string ForestDir        = "Assets/_Project/Prefabs/Props/KayKit/Forest";
        const string ToolsDir         = "Assets/_Project/Prefabs/Props/KayKit/Tools";
        const string RootName         = "KayKit_Dressing";

        [MenuItem("TARTARIA/Integration/Dress Echohaven with KayKit Assets")]
        public static void DressMenu() => Dress(ScenePath);

        [MenuItem("TARTARIA/Integration/Dress ALL Moon Scenes with KayKit Assets")]
        public static void DressAllMenu() => DressAllMoons();

        /// <summary>
        /// Dresses every moon scene returned by <see cref="MoonScenesFactory"/>
        /// that exists on disk. Skips missing files silently.
        /// </summary>
        public static int DressAllMoons()
        {
            int total = 0;
            foreach (var moon in MoonScenesFactory.Moons)
            {
                string path = MoonScenesFactory.ScenePathFor(moon);
                total += Dress(path, moon);
            }
            Debug.Log($"[KayKitDressing] Dressed {MoonScenesFactory.Moons.Length} moons "
                      + $"({total} total instances placed).");
            return total;
        }

        public static int Dress(string scenePath)
        {
            // Default theme: behave exactly like the old Echohaven dressing.
            var defaultMoon = new MoonScenesFactory.MoonInfo {
                number = 1, sceneName = Path.GetFileNameWithoutExtension(scenePath),
            };
            return Dress(scenePath, defaultMoon);
        }

        public static int Dress(string scenePath, MoonScenesFactory.MoonInfo moon)
        {
            if (!File.Exists(scenePath))
            {
                Debug.Log($"[KayKitDressing] Scene not found: {scenePath} — skipping.");
                return 0;
            }

            // Open the scene if it isn't already active.
            var active = SceneManager.GetActiveScene();
            bool opened = active.path != scenePath;
            Scene scene = opened
                ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)
                : active;

            // Wipe + recreate root.
            var existing = GameObject.Find(RootName);
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject(RootName);
            EditorSceneManager.MoveGameObjectToScene(root, scene);

            // Per-moon density: forest moons get more foliage, observatory/nexus less.
            int forestCount = moon.number switch {
                8 => 110,            // Verdant Canopy — forest biome
                3 or 11 => 40,       // Highlands / Tidal — sparse
                12 => 20,            // Observatory — minimal
                _ => 60,
            };
            int toolsCount = moon.number switch {
                7 or 10 => 24,       // Clockwork / Forge — workshop dense
                12 or 13 => 6,       // Observatory / Nexus — sparse
                _ => 14,
            };

            int placed = 0;
            placed += PlaceCharacters(root.transform, moon);
            placed += PlaceProps(root.transform, ForestDir, $"Forest_M{moon.number:D2}", radius: 28f, count: forestCount);
            placed += PlaceProps(root.transform, ToolsDir,  $"Tools_M{moon.number:D2}",  radius: 6f,  count: toolsCount);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[KayKitDressing] Moon {moon.number:D2} ({moon.sceneName}): placed {placed} instances.");
            return placed;
        }

        // ─────────────────────────────────────────────────────────────────────

        static int PlaceCharacters(Transform parent, MoonScenesFactory.MoonInfo moon)
        {
            var charsRoot = new GameObject("Characters").transform;
            charsRoot.SetParent(parent, false);

            int placed = 0;

            // Adventurer party (always present — your companions).
            string[] adv = { "Char_Knight", "Char_Mage", "Char_Rogue", "Char_Ranger", "Char_Barbarian" };
            for (int i = 0; i < adv.Length; i++)
            {
                var p = LoadPrefab($"{AdventurersDir}/{adv[i]}.prefab");
                if (p == null) continue;
                float a = (-60f + i * 30f) * Mathf.Deg2Rad;
                var pos = moon.spawnPos + new Vector3(Mathf.Sin(a) * 4f, 0f, Mathf.Cos(a) * 4f + 2f);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(p, charsRoot);
                instance.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(-(pos - moon.spawnPos).normalized, Vector3.up));
                placed++;
            }

            // Skeleton density scales with moon difficulty (later moons = more enemies).
            int skeletonRingCount = Mathf.Clamp(moon.number, 4, 9);
            string[] skel = { "Char_Skeleton_Warrior", "Char_Skeleton_Mage", "Char_Skeleton_Rogue", "Char_Skeleton_Minion" };
            for (int i = 0; i < skeletonRingCount; i++)
            {
                var p = LoadPrefab($"{SkeletonsCharDir}/{skel[i % skel.Length]}.prefab");
                if (p == null) continue;
                float a = (i / (float)skeletonRingCount) * Mathf.PI * 2f;
                var pos = moon.spawnPos + new Vector3(Mathf.Sin(a) * 16f, 0f, Mathf.Cos(a) * 16f);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(p, charsRoot);
                instance.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(-(pos - moon.spawnPos).normalized, Vector3.up));
                placed++;
            }

            return placed;
        }

        static int PlaceProps(Transform parent, string dir, string label, float radius, int count)
        {
            if (!AssetDatabase.IsValidFolder(dir)) return 0;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { dir });
            var prefabs = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(g => g != null)
                .ToArray();
            if (prefabs.Length == 0) return 0;

            var group = new GameObject(label).transform;
            group.SetParent(parent, false);

            // Deterministic so the layout is identical across runs.
            var rng = new System.Random(label.GetHashCode());
            int placed = 0;
            for (int i = 0; i < count; i++)
            {
                var prefab = prefabs[rng.Next(prefabs.Length)];
                float a = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float r = radius * (0.5f + (float)rng.NextDouble() * 0.5f);
                var pos = new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
                float yaw = (float)(rng.NextDouble() * 360f);
                float scl = 0.85f + (float)rng.NextDouble() * 0.5f;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group);
                instance.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
                instance.transform.localScale = Vector3.one * scl;
                placed++;
            }
            return placed;
        }

        static GameObject LoadPrefab(string path)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (p == null) Debug.LogWarning($"[KayKitDressing] Missing prefab: {path}");
            return p;
        }
    }
}
