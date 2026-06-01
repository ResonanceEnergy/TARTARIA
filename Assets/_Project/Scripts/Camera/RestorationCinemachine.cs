using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Camera
{
    /// <summary>
    /// Restoration cinematic — 4s orbit around a restored building, then return.
    /// SUPERSEDED 2026-05-31: Moon1CinematicMoments owns the OnBuildingRestoredTyped
    /// subscription. This class is kept for debug / direct invocation but no longer
    /// auto-fires to avoid double cinematic stacks.
    /// </summary>
    public class RestorationCinemachine : MonoBehaviour
    {
        [SerializeField] float orbitSeconds = 4f;
        [SerializeField] float orbitRadius = 8f;
        [SerializeField] float orbitHeight = 4f;

        // SUPERSEDED 2026-05-31 — Moon1CinematicMoments owns this subscription. Avoid double-fire.
        // void OnEnable()  { GameEvents.OnBuildingRestoredTyped += HandleRestored; }
        // void OnDisable() { GameEvents.OnBuildingRestoredTyped -= HandleRestored; }

        public void HandleRestored(BuildingRestoredEventArgs args)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            StartCoroutine(OrbitAround(cam, args.position));
        }

        IEnumerator OrbitAround(UnityEngine.Camera cam, Vector3 center)
        {
            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float t = 0f;
            while (t < orbitSeconds)
            {
                float a = (t / orbitSeconds) * Mathf.PI * 2f;
                cam.transform.position = center + new Vector3(Mathf.Cos(a) * orbitRadius, orbitHeight, Mathf.Sin(a) * orbitRadius);
                cam.transform.LookAt(center + Vector3.up * 2f);
                t += Time.deltaTime;
                yield return null;
            }
            float r = 0f;
            Vector3 endPos = cam.transform.position;
            Quaternion endRot = cam.transform.rotation;
            while (r < 1f)
            {
                cam.transform.position = Vector3.Lerp(endPos, origPos, r);
                cam.transform.rotation = Quaternion.Slerp(endRot, origRot, r);
                r += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
        }
    }
}
