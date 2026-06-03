using System;
using System.Collections;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(40)]
    public class Moon1NarrativeBeats : MonoBehaviour
    {
        // PlayerPrefs key matching the GiantKeys pattern (line 98 of GiantSkeletonKeyPickup).
        // Tracks first-prophecy-fragment unlock state so the beat is idempotent across save loads.
        const string PROPHECY_FRAGMENT_PREF = "TARTARIA_M1_ProphecyFragment1";
        // Cue ids registered by Moon1PopulateAudioCueLibrary.cs:65-67.
        const string CUE_SKELETON_HUM = "moon1.skeleton.hum_prophecy";

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

            // H2.L5 - First prophecy fragment unlock. PlayerPrefs flag mirrors the
            // GiantKeys pattern (Moon1NarrativeBeats.cs:98) so the beat is idempotent
            // across save loads.
            UnlockFirstProphecyFragment();
        }

        /// <summary>
        /// First prophecy fragment unlock — sets the persistent state flag and surfaces a HUD banner.
        /// Idempotent: skips if already unlocked. Per docs/15 + docs/03 Moon 1 climactic moment.
        /// </summary>
        void UnlockFirstProphecyFragment()
        {
            if (PlayerPrefs.GetInt(PROPHECY_FRAGMENT_PREF, 0) >= 1) return;
            PlayerPrefs.SetInt(PROPHECY_FRAGMENT_PREF, 1);
            PlayerPrefs.Save();
            try { GameEvents.RaiseHUDShowObjective("Prophecy Fragment unlocked — The Skeleton Hum reveals the first verse"); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(UnlockFirstProphecyFragment)} HUD banner failed: {ex.GetType().Name}: {ex.Message}\n  context: prophecy fragment 1 still unlocked in PlayerPrefs\n{ex.StackTrace}");
                // Non-fatal: PlayerPrefs flag is already set above; only the banner is missed.
            }
        }

        IEnumerator CathedralLightEruption()
        {
            var stardome = GameObject.Find("Building_echohaven_stardome");
            Vector3 center = stardome != null ? stardome.transform.position : new Vector3(0, 0, 100);
            GameObject vfx = null;
            var vfxPrefab = Resources.Load<GameObject>("VFX/Moon1/VFX_CathedralLightEruption");
            if (vfxPrefab != null) vfx = Instantiate(vfxPrefab, center + Vector3.up * 4f, Quaternion.identity);

            // H2.L5 - Skeleton hum prophecy audio cue. The cue id is registered by
            // Moon1PopulateAudioCueLibrary.cs:65-67 -> Skeleton_Hum_Prophecy.wav.
            // 3D-positioned at the cathedral center so it lines up with the eruption VFX.
            try { AudioManager.Instance?.PlayCue(CUE_SKELETON_HUM, center + Vector3.up * 2f); }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(Moon1NarrativeBeats)}] {nameof(CathedralLightEruption)} skeleton hum PlayCue failed: {ex.GetType().Name}: {ex.Message}\n  context: cueId={CUE_SKELETON_HUM} center={center}\n{ex.StackTrace}");
                // Non-fatal: VFX + objective banner still fire; only the prophecy hum is missed.
            }

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
