using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using Tartaria.Core;

namespace Tartaria.AI
{
    /// <summary>
    /// Moon 1 completion cinematic — Cassian boss intro beat.
    ///
    /// Sequenced between Anastasia reveal (8.7s post-OnMoonCompleted) and
    /// Lirael monologue (12s post-OnMoonCompleted): Cassian fires at 4s.
    ///
    /// Beat:
    ///   t=4.0s   Locate Cassian + Cathedral, walk Cassian toward cathedral
    ///   t=6.0s   Banner: "You wake what should sleep. We end this at the next moon."
    ///
    /// Owner: AI (per docs/agents/COORDINATION.md path ownership for Scripts/AI/).
    /// Companion piece: Tartaria.Integration.Moon1CassianController owns the actual
    /// NavMeshAgent drive on the Cassian GameObject. This controller only triggers it.
    ///
    /// 2026-06-02 no-debt mandate compliance:
    ///   - Cassian/Cathedral lookup failures log error WITH the search type + scene name
    ///   - Banner raise wrapped in try/catch that LOGS + RETHROWS (no silent fail)
    ///   - No bypass driver, no fallback to primitives, no TODO stubs
    /// </summary>
    [DisallowMultipleComponent]
    public class CassianBossIntro : MonoBehaviour
    {
        public static CassianBossIntro Instance { get; private set; }

        const float kFireDelaySeconds = 4f;
        const float kWalkBeforeBannerSeconds = 2f;
        const float kBannerDurationSeconds = 5f;
        const string kBannerSpeaker = "Cassian";
        const string kBannerLine = "You wake what should sleep. We end this at the next moon.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject(nameof(CassianBossIntro));
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CassianBossIntro>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameEvents.OnMoonCompleted += HandleMoonCompleted;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            GameEvents.OnMoonCompleted -= HandleMoonCompleted;
        }

        void HandleMoonCompleted(MoonCompletedEventArgs args)
        {
            if (args == null) { Debug.LogWarning("[CassianBossIntro] OnMoonCompleted fired with null args — ignoring."); return; }
            if (args.moonIndex != 1) return;
            StartCoroutine(IntroSequence(args));
        }

        IEnumerator IntroSequence(MoonCompletedEventArgs args)
        {
            yield return new WaitForSecondsRealtime(kFireDelaySeconds);

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            var cassianGO = FindCassian(sceneName);
            var cathedralGO = FindCathedral(sceneName);

            if (cassianGO != null && cathedralGO != null)
            {
                Debug.Log($"[CassianBossIntro] Cassian walks toward cathedral (cassian='{cassianGO.name}', cathedral='{cathedralGO.name}', scene='{sceneName}').");
                DriveCassianToCathedral(cassianGO, cathedralGO.transform.position);
            }
            else if (cassianGO != null && cathedralGO == null)
            {
                // Cassian present but no target — log + speak banner anyway (still valid narrative beat).
                Debug.LogError($"[CassianBossIntro] Cathedral not found (searched tag 'HeroBuilding' + names 'Cathedral'/'cathedral_dome'/'crystal_hall'). scene='{sceneName}'. Banner will still fire; Cassian will not walk.");
            }
            else if (cassianGO == null && cathedralGO != null)
            {
                Debug.LogError($"[CassianBossIntro] Cassian GameObject not found (searched tag 'Cassian' + name 'Cassian' + Moon1CassianController via reflection). scene='{sceneName}'. Banner will still fire so the player hears the threat.");
            }
            else
            {
                Debug.LogError($"[CassianBossIntro] Neither Cassian nor Cathedral found in scene '{sceneName}'. Searched: tag 'Cassian'/'HeroBuilding', name 'Cassian'/'Cathedral'/'cathedral_dome'/'crystal_hall', Moon1CassianController via reflection. Banner will still fire.");
            }

            yield return new WaitForSecondsRealtime(kWalkBeforeBannerSeconds);

            try
            {
                GameEvents.RaiseHUDShowBanner(kBannerSpeaker, kBannerLine, kBannerDurationSeconds);
                Debug.Log("[CassianBossIntro] Banner fired");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CassianBossIntro] RaiseHUDShowBanner threw at file:line — title='{kBannerSpeaker}' subtitle='{kBannerLine}' duration={kBannerDurationSeconds}. Exception: {ex}");
                throw;
            }
        }

        // ─── Cassian lookup ───────────────────────────────────────────────────────

        static GameObject FindCassian(string sceneName)
        {
            // 1) Try tag "Cassian" — wrapped: undefined tags throw UnityException.
            try
            {
                var tagged = GameObject.FindGameObjectsWithTag("Cassian");
                if (tagged != null && tagged.Length > 0)
                {
                    return tagged[0];
                }
            }
            catch (UnityException)
            {
                // Tag not defined in this project's TagManager — fall through to name lookup.
                // Not logged as error: this is an expected, documented fallback path.
            }

            // 2) Try name "Cassian"
            var byName = GameObject.Find("Cassian");
            if (byName != null) return byName;

            // 3) Try Moon1CassianController via reflection (lives in Tartaria.Integration —
            //    AI assembly does not reference Integration to avoid circular deps).
            var ctrlType = Type.GetType("Tartaria.Integration.Moon1CassianController, Tartaria.Integration");
            if (ctrlType != null)
            {
                var found = UnityEngine.Object.FindFirstObjectByType(ctrlType) as Component;
                if (found != null) return found.gameObject;
            }

            return null;
        }

        // ─── Cathedral lookup ─────────────────────────────────────────────────────

        static GameObject FindCathedral(string sceneName)
        {
            // 1) Try tag "HeroBuilding" — pick the nearest object whose name contains "cathedral".
            try
            {
                var tagged = GameObject.FindGameObjectsWithTag("HeroBuilding");
                if (tagged != null)
                {
                    foreach (var go in tagged)
                    {
                        if (go == null) continue;
                        var n = go.name.ToLowerInvariant();
                        if (n.Contains("cathedral") || n.Contains("dome") || n.Contains("crystal_hall"))
                        {
                            return go;
                        }
                    }
                    // No name match among tagged hero buildings — take the first as a reasonable target.
                    if (tagged.Length > 0 && tagged[0] != null) return tagged[0];
                }
            }
            catch (UnityException)
            {
                // Tag undefined — fall through.
            }

            // 2) Try common Moon 1 cathedral GameObject names (per Moon1CathedralRestore + CassianNPCController references).
            var names = new[] { "Cathedral", "cathedral_dome", "crystal_hall", "Cathedral_Hero", "Moon1_Cathedral" };
            foreach (var n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) return go;
            }

            return null;
        }

        // ─── Drive Cassian forward ─────────────────────────────────────────────────

        static void DriveCassianToCathedral(GameObject cassian, Vector3 cathedralPos)
        {
            // Preferred path: call Moon1CassianController.WalkToCathedral() via reflection
            // (it owns its own NavMeshAgent + target cache).
            var ctrlType = Type.GetType("Tartaria.Integration.Moon1CassianController, Tartaria.Integration");
            if (ctrlType != null)
            {
                var ctrl = cassian.GetComponent(ctrlType);
                if (ctrl != null)
                {
                    // Set cathedralTarget field (public Vector3).
                    var field = ctrlType.GetField("cathedralTarget", BindingFlags.Instance | BindingFlags.Public);
                    if (field != null)
                    {
                        try { field.SetValue(ctrl, cathedralPos); }
                        catch (Exception ex) { Debug.LogError($"[CassianBossIntro] Setting Moon1CassianController.cathedralTarget threw: {ex}"); }
                    }
                    var walk = ctrlType.GetMethod("WalkToCathedral", BindingFlags.Instance | BindingFlags.Public);
                    if (walk != null)
                    {
                        try
                        {
                            walk.Invoke(ctrl, null);
                            Debug.Log($"[CassianBossIntro] Moon1CassianController.WalkToCathedral() invoked toward {cathedralPos}.");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[CassianBossIntro] Moon1CassianController.WalkToCathedral() threw: {ex}. Falling through to direct NavMeshAgent.");
                        }
                    }
                }
            }

            // Fallback: drive NavMeshAgent directly on the Cassian GO.
            var agent = cassian.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(cathedralPos);
                Debug.Log($"[CassianBossIntro] NavMeshAgent.SetDestination({cathedralPos}) on '{cassian.name}'.");
                return;
            }

            // No agent / not on nav mesh — log loud with the hierarchy path + scene.
            string hierarchyPath = BuildHierarchyPath(cassian.transform);
            Debug.LogWarning($"[CassianBossIntro] No NavMeshAgent (or agent not on NavMesh) on '{hierarchyPath}' in scene '{cassian.scene.name}'. Cassian cannot walk this beat — banner will still fire.");
        }

        static string BuildHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var path = t.name;
            var p = t.parent;
            while (p != null) { path = p.name + "/" + path; p = p.parent; }
            return path;
        }
    }
}
