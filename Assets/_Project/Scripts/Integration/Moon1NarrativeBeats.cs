using System;
using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(40)]
    public class Moon1NarrativeBeats : MonoBehaviour
    {
        bool _eruptionFired;
        bool _giantKeySpawned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (FindFirstObjectByType<Moon1NarrativeBeats>() != null) return;
            var go = new GameObject("__Moon1NarrativeBeats");
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1NarrativeBeats>();
        }

        void OnEnable()
        {
            try { TartarianHourCycle.OnSeventeenthHour += HandleSeventeenthHour; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(OnEnable)} failed to subscribe to TartarianHourCycle.OnSeventeenthHour: {ex.GetType().Name}: {ex.Message}\n  context: scene=Echohaven_VerticalSlice\n{ex.StackTrace}");
                // Non-fatal: 17th-hour cathedral eruption beat will not fire this session, but the rest of Moon 1 narrative continues.
            }
        }

        void OnDisable()
        {
            try { TartarianHourCycle.OnSeventeenthHour -= HandleSeventeenthHour; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(OnDisable)} failed to unsubscribe from TartarianHourCycle.OnSeventeenthHour: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak across scene changes; logged for diagnosis.
            }
        }

        void Start() { if (!_giantKeySpawned) SpawnGiantSkeletonKey(new Vector3(-40f, 1.2f, -20f)); }

        void HandleSeventeenthHour()
        {
            if (_eruptionFired) return;
            _eruptionFired = true;
            StartCoroutine(CathedralLightEruption());

            // C.L4 - Progress canonical Moon 1 "Lirael's 17th Whisper" companion side quest
            // (QuestDatabaseBuilder.cs r7_m1_lirael_calendar_echo, objective type
            // CompanionMilestone+targetId=="lirael_17th_m1"). 17th-hour beat is the canonical
            // trigger per docs/03 Days 25-28 + docs/03C Moon 1 Revelation.
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.CompanionMilestone, "lirael_17th_m1");
        }

        IEnumerator CathedralLightEruption()
        {
            var stardome = GameObject.Find("Building_echohaven_stardome");
            Vector3 center = stardome != null ? stardome.transform.position : new Vector3(0, 0, 100);
            GameObject vfx = null;
            var vfxPrefab = Resources.Load<GameObject>("VFX/Moon1/VFX_CathedralLightEruption");
            if (vfxPrefab != null) vfx = Instantiate(vfxPrefab, center + Vector3.up * 4f, Quaternion.identity);
            try { GameEvents.RaiseHUDShowObjective("Cathedral Light Eruption!"); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(CathedralLightEruption)} HUD objective raise failed: {ex.GetType().Name}: {ex.Message}\n  context: stardomePresent={(stardome != null)} center={center}\n{ex.StackTrace}");
                // Non-fatal: VFX still plays, but the player won't see the on-screen objective banner.
            }
            try { GameEvents.FireRSChange(20f); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(CathedralLightEruption)} FireRSChange(+20) failed: {ex.GetType().Name}: {ex.Message}\n  context: cathedral eruption RS reward\n{ex.StackTrace}");
                // Non-fatal: player misses the +20 RS reward; rest of beat continues.
            }
            yield return new WaitForSeconds(6f);
            if (vfx != null) Destroy(vfx, 2f);
        }

        void SpawnGiantSkeletonKey(Vector3 worldPos)
        {
            if (PlayerPrefs.GetInt("TARTARIA_GiantKeys", 0) >= 1) { _giantKeySpawned = true; return; }
            var go = new GameObject("GiantSkeletonKey_1");
            go.transform.position = worldPos;
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.5f, 1f, 2.5f);
            go.AddComponent<GiantSkeletonKeyPickup>().Init(1);
            _giantKeySpawned = true;
        }
    }

    public class GiantSkeletonKeyPickup : MonoBehaviour
    {
        const string KEYS_PREF = "TARTARIA_GiantKeys";
        int _keyNumber;
        public void Init(int n) { _keyNumber = n; }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            int current = PlayerPrefs.GetInt(KEYS_PREF, 0);
            if (current >= _keyNumber) return;
            PlayerPrefs.SetInt(KEYS_PREF, _keyNumber);
            PlayerPrefs.Save();
            try { GameEvents.RaiseHUDShowObjective($"Giant Skeleton Key #{_keyNumber} of 8 collected"); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(GiantSkeletonKeyPickup)}] {nameof(OnTriggerEnter)} HUD objective raise failed: {ex.GetType().Name}: {ex.Message}\n  context: keyNumber={_keyNumber} other={other?.name}\n{ex.StackTrace}");
                // Non-fatal: pickup is still credited to PlayerPrefs above; only the HUD banner is missed.
            }
            try { GameEvents.FireRSChange(15f); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(GiantSkeletonKeyPickup)}] {nameof(OnTriggerEnter)} FireRSChange(+15) failed: {ex.GetType().Name}: {ex.Message}\n  context: keyNumber={_keyNumber}\n{ex.StackTrace}");
                // Non-fatal: player misses the +15 RS reward; key is still counted.
            }

            // C.L4 - Progress canonical Moon 1 "Giant Skeleton Key #1" collectible quest
            // (QuestDatabaseBuilder moon1_giant_skeleton_key, CollectItem+targetId="giant_skeleton_key_1").
            // QuestManager.ProgressByType (QuestManager.cs:360) fires RaiseQuestStatusChanged on
            // completion -> QuestObjectiveTrackerUI.HandleQuestStatusChanged (UI/QuestObjectiveTrackerUI.cs:78).
            if (_keyNumber == 1)
                QuestManager.Instance?.ProgressByType(QuestObjectiveType.CollectItem, "giant_skeleton_key_1");

            Destroy(gameObject);
        }
    }
}
