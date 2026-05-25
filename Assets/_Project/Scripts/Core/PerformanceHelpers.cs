using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// AGENT 28: LOD Helper
    /// Utilities for managing LOD groups and distance-based optimizations
    /// </summary>
    public static class LODHelper
    {
        /// <summary>
        /// Add LOD group to GameObject with standard presets
        /// </summary>
        public static LODGroup AddStandardLOD(GameObject go, Renderer[] renderers)
        {
            if (go == null || renderers == null || renderers.Length == 0)
                return null;

            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null)
                lodGroup = go.AddComponent<LODGroup>();

            // Standard LOD distances for TARTARIA
            // LOD0: 0-30m (100% quality)
            // LOD1: 30-60m (60% quality)
            // LOD2: 60-120m (30% quality)
            // Cull: >120m
            var lods = new LOD[3];
            lods[0] = new LOD(0.25f, renderers); // 25% screen height = ~30m
            lods[1] = new LOD(0.125f, renderers); // 12.5% screen height = ~60m
            lods[2] = new LOD(0.05f, renderers); // 5% screen height = ~120m

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            return lodGroup;
        }

        /// <summary>
        /// Add LOD group with custom distances
        /// </summary>
        public static LODGroup AddCustomLOD(GameObject go, Renderer[] renderers, float[] screenHeights)
        {
            if (go == null || renderers == null || renderers.Length == 0 || screenHeights == null)
                return null;

            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null)
                lodGroup = go.AddComponent<LODGroup>();

            var lods = new LOD[screenHeights.Length];
            for (int i = 0; i < screenHeights.Length; i++)
            {
                lods[i] = new LOD(screenHeights[i], renderers);
            }

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            return lodGroup;
        }

        /// <summary>
        /// Enable LOD crossfade for smooth transitions
        /// </summary>
        public static void EnableCrossfade(LODGroup lodGroup, float fadeTime = 0.5f)
        {
            if (lodGroup == null) return;
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
        }

        /// <summary>
        /// Disable renderers beyond a certain distance (manual culling)
        /// </summary>
        public static void SetCullingDistance(Renderer renderer, Transform camera, float maxDistance)
        {
            if (renderer == null || camera == null) return;

            float distance = Vector3.Distance(renderer.transform.position, camera.position);
            renderer.enabled = distance <= maxDistance;
        }
    }

    /// <summary>
    /// AGENT 28: Draw Call Analyzer
    /// Runtime analysis of draw calls and batching efficiency
    /// </summary>
    public class DrawCallAnalyzer : MonoBehaviour
    {
        [Header("Analysis")]
        [SerializeField] bool analyzeOnStart = false;
        [SerializeField] bool continuousAnalysis = false;
        [SerializeField] float analysisInterval = 5f;

        [Header("Results")]
        [SerializeField, ReadOnly] int totalRenderers;
        [SerializeField, ReadOnly] int staticBatchedRenderers;
        [SerializeField, ReadOnly] int dynamicBatchableRenderers;
        [SerializeField, ReadOnly] int instancedRenderers;
        [SerializeField, ReadOnly] int uniqueMaterials;

        float _analysisTimer;

        void Start()
        {
            if (analyzeOnStart)
                Analyze();
        }

        void Update()
        {
            if (continuousAnalysis)
            {
                _analysisTimer += Time.deltaTime;
                if (_analysisTimer >= analysisInterval)
                {
                    _analysisTimer = 0f;
                    Analyze();
                }
            }
        }

        public void Analyze()
        {
            var allRenderers = FindObjectsOfType<Renderer>();
            totalRenderers = allRenderers.Length;

            staticBatchedRenderers = 0;
            dynamicBatchableRenderers = 0;
            instancedRenderers = 0;

            var materialSet = new System.Collections.Generic.HashSet<Material>();

            foreach (var renderer in allRenderers)
            {
                if (renderer.gameObject.isStatic)
                    staticBatchedRenderers++;

                if (renderer.sharedMaterials != null)
                {
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            materialSet.Add(mat);
                            if (mat.enableInstancing)
                                instancedRenderers++;
                        }
                    }
                }
            }

            uniqueMaterials = materialSet.Count;

            Debug.Log($"[DrawCallAnalyzer] Renderers: {totalRenderers}, Static: {staticBatchedRenderers}, " +
                     $"Instanced: {instancedRenderers}, Unique Materials: {uniqueMaterials}");
        }

        [System.AttributeUsage(System.AttributeTargets.Field)]
        class ReadOnlyAttribute : PropertyAttribute { }
    }
}
