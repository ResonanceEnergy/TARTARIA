using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Anastasia narrative beat — rocking chair outside Cathedral, humming 432Hz refrain.
    /// First emotional anchor per docs/03_CAMPAIGN_13_MOONS.md Moon 1.
    ///
    /// H.L5 (Sprint 11 L8 50ff78ea): the procedural primitive build was deleted.
    /// Visual + audio authoring lives in the text-mode prefab baked by
    /// Tartaria.Editor.Moon1AnastasiaRockerBake to
    /// Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab. This component now
    /// only instantiates that prefab, places it at the authored ChairPos, and
    /// keeps the runtime proximity-greeting + dialogue surface unchanged.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1AnastasiaRocker : MonoBehaviour
    {
        // Asset paths must match Moon1AnastasiaRockerBake.PrefabPath / Resources layout.
        const string PrefabAssetPath = "Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab";
        const string PrefabResourcePath = "Moon1/AnastasiaRocker";

        static Moon1AnastasiaRocker _instance;

        GameObject _rockerInstance;
        bool _hasGreetedPlayer;

        // Anastasia sits just outside Cathedral entrance: Cathedral at (0,_,30), so chair at (3, 0, 22)
        // Kept as world position because Bootstrap creates the host GameObject at origin.
        static readonly Vector3 ChairPos = new Vector3(3f, 0f, 22f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1AnastasiaRocker");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1AnastasiaRocker>();
        }

        void Start()
        {
            var prefab = LoadRockerPrefab();
            if (prefab == null)
            {
                Debug.LogError(
                    "[Moon1AnastasiaRocker] AnastasiaRocker prefab missing at " + PrefabAssetPath +
                    " — run Tartaria/6 Bake/Bake Anastasia Rocker Prefab.");
                return;
            }

            _rockerInstance = Object.Instantiate(prefab, transform);
            _rockerInstance.name = "AnastasiaRocker_BG_AtCathedral";
            _rockerInstance.transform.position = ChairPos;

            WireProximityListener(_rockerInstance);
            Debug.Log("[Moon1AnastasiaRocker] Anastasia seated, rocking, humming at 432 Hz.");
        }

        static GameObject LoadRockerPrefab()
        {
            var prefab = Resources.Load<GameObject>(PrefabResourcePath);
#if UNITY_EDITOR
            if (prefab == null)
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
#endif
            return prefab;
        }

        void WireProximityListener(GameObject root)
        {
            // The prefab ships the trigger + listener; bind listener.parent at runtime
            // because the prefab can't serialize a scene-only Moon1AnastasiaRocker ref.
            var listeners = root.GetComponentsInChildren<Moon1AnastasiaProximityListener>(true);
            for (int i = 0; i < listeners.Length; i++)
                listeners[i].parent = this;
        }

        public void NotifyPlayerNearby()
        {
            if (_hasGreetedPlayer) return;
            _hasGreetedPlayer = true;
            ServiceLocator.HUD?.ShowBanner("Anastasia", "The buildings remember. Listen — they hum at 432.", 7f);
            // After 8s — second line
            StartCoroutine(QueueLine("Anastasia", "I'm not who I was. None of us are. Tune them anyway.", 8f, 7f));

            // C.L4 - Progress the canonical Moon 1 "Anastasia's Lullaby" side quest.
            // QuestManager.ProgressByType matches CompanionMilestone+targetId=="anastasia_rocker_m1"
            // (QuestManager.cs:360 - fires RaiseQuestStatusChanged on completion). Specific targetId
            // avoids colliding with CompanionManager trust-milestone progression once Anastasia
            // is unlocked in Moon 7 (CompanionManager.cs:96).
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompanionMilestone, "anastasia_rocker_m1");
        }

        System.Collections.IEnumerator QueueLine(string speaker, string line, float delay, float showFor)
        {
            yield return new WaitForSeconds(delay);
            ServiceLocator.HUD?.ShowBanner(speaker, line, showFor);
        }
    }

    // Moon1ChairRockAnimator and Moon1AnastasiaProximityListener were split into their
    // own files (2026-06-06): MonoBehaviours sharing a file get no MonoScript asset and
    // can never be serialized into prefabs — the cause of the m_Script {fileID: 0} bug
    // in AnastasiaRocker.prefab.
}
