using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Mud Dissolution Animator — listens for GameEvents.OnBuildingRestoredTyped and animates
    /// the building's mud-shader property from 0 -> 1 over 5 seconds so the restored building
    /// "emerges" through dissolving mud instead of just popping in.
    ///
    /// Why a separate component: per Fix 8 (2026-05-31) the dissolution shader was wired but
    /// never animated — `_Dissolution` (and the legacy `_Dissolve` / `_DissolveProgress`)
    /// stayed at whatever the prefab shipped with. This component runs in the Integration
    /// assembly (same place we already drive emergence/restoration FX) and self-bootstraps
    /// so designers don't have to drop it into every scene.
    ///
    /// Bootstrap order: RuntimeInitializeOnLoadMethod after scene load so the listener is
    /// live before BuildingSpawner / CathedralRestorationSystem fire restoration events.
    /// </summary>
    [DefaultExecutionOrder(-65)]
    public class MudDissolutionAnimator : MonoBehaviour
    {
        const float DissolveDurationSeconds = 5f;

        // Spec primary, then the two legacy property names actually present on existing
        // dissolution shaders in this project (InteractableBuilding uses _DissolveProgress,
        // CathedralRestorationSystem uses _Dissolve). We write whichever ones the material
        // declares so this component is robust to mixed shader sources.
        static readonly int DissolutionId      = Shader.PropertyToID("_Dissolution");
        static readonly int DissolveId         = Shader.PropertyToID("_Dissolve");
        static readonly int DissolveProgressId = Shader.PropertyToID("_DissolveProgress");

        static MudDissolutionAnimator _instance;
        readonly HashSet<int> _inFlightBuildingIds = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("MudDissolutionAnimator");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MudDissolutionAnimator>();
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestoredTyped += HandleBuildingRestored;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestoredTyped -= HandleBuildingRestored;
        }

        void HandleBuildingRestored(BuildingRestoredEventArgs args)
        {
            if (args == null) return;
            var building = args.Building;
            if (building == null)
            {
                // Fall back to a name-based lookup if the payload didn't include the GO ref.
                if (!string.IsNullOrEmpty(args.buildingId))
                {
                    var found = GameObject.Find(args.buildingId);
                    if (found != null) building = found;
                }
            }
            if (building == null)
            {
                Debug.Log($"[MudDissolutionAnimator] No GameObject for buildingId='{args.buildingId}'; skipping dissolve anim.");
                return;
            }

            int key = building.GetInstanceID();
            if (_inFlightBuildingIds.Contains(key)) return;
            _inFlightBuildingIds.Add(key);

            StartCoroutine(AnimateDissolve(building, key));
        }

        IEnumerator AnimateDissolve(GameObject building, int key)
        {
            var renderers = building.GetComponentsInChildren<MeshRenderer>(includeInactive: false);
            if (renderers == null || renderers.Length == 0)
            {
                _inFlightBuildingIds.Remove(key);
                yield break;
            }

            // Instance-per-renderer the shared materials so we don't mutate project assets,
            // and seed _Dissolution = 0 so the building starts fully shrouded in mud.
            var allMaterials = new List<Material>();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                // Reading .materials returns instanced copies — cache them.
                foreach (var m in r.materials)
                {
                    if (m == null) continue;
                    SetDissolveValue(m, 0f);
                    allMaterials.Add(m);
                }
            }

            if (allMaterials.Count == 0)
            {
                _inFlightBuildingIds.Remove(key);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < DissolveDurationSeconds)
            {
                // Guard against the building being destroyed mid-anim.
                if (building == null) break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / DissolveDurationSeconds);
                // Smoothstep gives a slightly more cinematic dissolve curve.
                float v = Mathf.SmoothStep(0f, 1f, t);
                for (int i = 0; i < allMaterials.Count; i++)
                {
                    var m = allMaterials[i];
                    if (m == null) continue;
                    SetDissolveValue(m, v);
                }
                yield return null;
            }

            // Snap to fully dissolved (mud cleared) at end.
            for (int i = 0; i < allMaterials.Count; i++)
            {
                var m = allMaterials[i];
                if (m == null) continue;
                SetDissolveValue(m, 1f);
            }

            _inFlightBuildingIds.Remove(key);
        }

        static void SetDissolveValue(Material m, float v)
        {
            if (m == null) return;
            if (m.HasProperty(DissolutionId))      m.SetFloat(DissolutionId, v);
            if (m.HasProperty(DissolveId))         m.SetFloat(DissolveId, v);
            if (m.HasProperty(DissolveProgressId)) m.SetFloat(DissolveProgressId, v);
        }
    }
}
