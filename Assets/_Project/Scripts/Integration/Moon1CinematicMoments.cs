using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1CinematicMoments — canonical cinematic camera owner for Moon 1.
    /// On OnBuildingRestoredTyped: 4-second restoration dolly.
    /// On TartarianHourCycle.OnSeventeenthHour: 6-second wide pan over the cathedral
    /// AND instantiates VFX_SeventeenthHourBeam from Resources/VFX/Moon1/.
    /// </summary>
    public class Moon1CinematicMoments : MonoBehaviour
    {
        const float DOLLY_DURATION = 4f;
        const float SEVENTEENTH_HOUR_PAN = 6f;
        bool _cinematicActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (FindFirstObjectByType<Moon1CinematicMoments>() != null) return;
            var go = new GameObject("__Moon1CinematicMoments");
            DontDestroyOnLoad(go);
            go.AddComponent<Moon1CinematicMoments>();
        }

        void OnEnable()
        {
            try { GameEvents.OnBuildingRestoredTyped += HandleRestored; } catch { }
            try { TartarianHourCycle.OnSeventeenthHour += HandleSeventeenthHour; } catch { }
        }

        void OnDisable()
        {
            try { GameEvents.OnBuildingRestoredTyped -= HandleRestored; } catch { }
            try { TartarianHourCycle.OnSeventeenthHour -= HandleSeventeenthHour; } catch { }
        }

        void HandleRestored(BuildingRestoredEventArgs args)
        {
            if (_cinematicActive) return;
            StartCoroutine(RestorationDolly(args.position));
        }

        void HandleSeventeenthHour()
        {
            if (_cinematicActive) return;
            var stardome = GameObject.Find("Building_echohaven_stardome");
            Vector3 target = stardome != null ? stardome.transform.position : new Vector3(0, 0, 100);
            StartCoroutine(SeventeenthHourPan(target));
        }

        IEnumerator RestorationDolly(Vector3 center)
        {
            _cinematicActive = true;
            var cam = UnityEngine.Camera.main;
            if (cam == null) { _cinematicActive = false; yield break; }
            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float t = 0f;
            while (t < DOLLY_DURATION)
            {
                float rad = (t / DOLLY_DURATION) * Mathf.PI * 2f;
                cam.transform.position = center + new Vector3(Mathf.Cos(rad) * 8f, 4f, Mathf.Sin(rad) * 8f);
                cam.transform.LookAt(center + Vector3.up * 2f);
                t += Time.deltaTime;
                yield return null;
            }
            float backDur = 1.4f;
            float bt = 0f;
            Vector3 fromPos = cam.transform.position;
            Quaternion fromRot = cam.transform.rotation;
            while (bt < backDur)
            {
                cam.transform.position = Vector3.Lerp(fromPos, origPos, bt / backDur);
                cam.transform.rotation = Quaternion.Slerp(fromRot, origRot, bt / backDur);
                bt += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            _cinematicActive = false;
        }

        IEnumerator SeventeenthHourPan(Vector3 target)
        {
            _cinematicActive = true;
            var cam = UnityEngine.Camera.main;
            if (cam == null) { _cinematicActive = false; yield break; }
            GameObject beamFx = null;
            var beamPrefab = Resources.Load<GameObject>("VFX/Moon1/VFX_SeventeenthHourBeam");
            if (beamPrefab != null) beamFx = Instantiate(beamPrefab, target, Quaternion.identity);
            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            Vector3 panStart = target + new Vector3(-30f, 12f, -30f);
            Vector3 panEnd   = target + new Vector3( 30f, 12f, -30f);
            float t = 0f;
            while (t < SEVENTEENTH_HOUR_PAN)
            {
                cam.transform.position = Vector3.Lerp(panStart, panEnd, t / SEVENTEENTH_HOUR_PAN);
                cam.transform.LookAt(target + Vector3.up * 6f);
                t += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            if (beamFx != null) Destroy(beamFx, 2f);
            _cinematicActive = false;
        }
    }
}
