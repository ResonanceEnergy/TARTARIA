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

        void OnEnable() { try { TartarianHourCycle.OnSeventeenthHour += HandleSeventeenthHour; } catch { } }
        void OnDisable() { try { TartarianHourCycle.OnSeventeenthHour -= HandleSeventeenthHour; } catch { } }

        void Start() { if (!_giantKeySpawned) SpawnGiantSkeletonKey(new Vector3(-40f, 1.2f, -20f)); }

        void HandleSeventeenthHour()
        {
            if (_eruptionFired) return;
            _eruptionFired = true;
            StartCoroutine(CathedralLightEruption());
        }

        IEnumerator CathedralLightEruption()
        {
            var stardome = GameObject.Find("Building_echohaven_stardome");
            Vector3 center = stardome != null ? stardome.transform.position : new Vector3(0, 0, 100);
            GameObject vfx = null;
            var vfxPrefab = Resources.Load<GameObject>("VFX/Moon1/VFX_CathedralLightEruption");
            if (vfxPrefab != null) vfx = Instantiate(vfxPrefab, center + Vector3.up * 4f, Quaternion.identity);
            try { GameEvents.RaiseHUDShowObjective("Cathedral Light Eruption!"); } catch { }
            try { GameEvents.FireRSChange(20f); } catch { }
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
            try { GameEvents.RaiseHUDShowObjective($"Giant Skeleton Key #{_keyNumber} of 8 collected"); } catch { }
            try { GameEvents.FireRSChange(15f); } catch { }
            Destroy(gameObject);
        }
    }
}
