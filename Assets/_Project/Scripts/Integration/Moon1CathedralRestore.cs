using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration {
    [DisallowMultipleComponent]
    public class Moon1CathedralRestore : MonoBehaviour {
        [Header("Piecewise restore — assign in Inspector by Cowork")]
        [SerializeField] GameObject foundation;
        [SerializeField] GameObject walls;
        [SerializeField] GameObject roof;
        [SerializeField] GameObject buttresses;
        [SerializeField] GameObject spire;
        [SerializeField] float gapSeconds = 1.5f;

        public static Moon1CathedralRestore Instance { get; private set; }

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDestroy() {
            if (Instance == this) Instance = null;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void HandleBuildingRestored(string buildingId) {
            if (string.IsNullOrEmpty(buildingId)) return;
            if (!buildingId.ToLowerInvariant().Contains("cathedral")) return;
            StopAllCoroutines();
            StartCoroutine(PiecewiseRise());
        }

        IEnumerator PiecewiseRise() {
            yield return RisePiece(foundation, "Foundation");
            yield return new WaitForSeconds(gapSeconds);
            yield return RisePiece(walls, "Walls");
            yield return new WaitForSeconds(gapSeconds);
            yield return RisePiece(roof, "Roof");
            yield return new WaitForSeconds(gapSeconds);
            yield return RisePiece(buttresses, "Buttresses");
            yield return new WaitForSeconds(gapSeconds);
            yield return RisePiece(spire, "Spire");
            Debug.Log("[Moon1CathedralRestore] All 5 pieces risen — cathedral complete.");
        }

        IEnumerator RisePiece(GameObject piece, string label) {
            if (piece == null) { Debug.LogWarning($"[Moon1CathedralRestore] {label} not assigned in Inspector — skipping."); yield break; }
            piece.SetActive(true);
            Vector3 start = piece.transform.localPosition + new Vector3(0f, -4f, 0f);
            Vector3 end = piece.transform.localPosition;
            piece.transform.localPosition = start;
            float t = 0f;
            const float dur = 1.0f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                piece.transform.localPosition = Vector3.Lerp(start, end, k);
                yield return null;
            }
            piece.transform.localPosition = end;
            Debug.Log($"[Moon1CathedralRestore] {label} risen.");
        }
    }
}
