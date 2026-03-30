using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Tartaria.UI
{
    /// <summary>
    /// URP Renderer Feature that applies a full-screen colorblind correction pass.
    /// Configured at runtime by AccessibilityManager.
    ///
    /// Add this feature to the UniversalRendererData asset in the Editor.
    /// It reads the current mode from AccessibilityManager.Instance.
    /// </summary>
    public class ColorblindRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        [SerializeField] Settings settings = new();

        ColorblindRenderPass _pass;

        public override void Create()
        {
            _pass = new ColorblindRenderPass(settings.renderPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var mode = AccessibilityManager.Instance?.CurrentColorblindMode ?? ColorblindMode.None;
            if (mode == ColorblindMode.None) return;

            _pass.SetMode(mode);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Cleanup();
        }
    }

    class ColorblindRenderPass : ScriptableRenderPass
    {
        static readonly int s_ColorMatrix = Shader.PropertyToID("_ColorMatrix");
        Material _material;
        ColorblindMode _mode;

        // Daltonization matrices (3x3 flattened to Vector4 rows for shader)
        static readonly Matrix4x4 ProtanopiaMatrix = new(
            new Vector4(0.567f, 0.433f, 0f, 0f),
            new Vector4(0.558f, 0.442f, 0f, 0f),
            new Vector4(0f, 0.242f, 0.758f, 0f),
            new Vector4(0f, 0f, 0f, 1f));

        static readonly Matrix4x4 DeuteranopiaMatrix = new(
            new Vector4(0.625f, 0.375f, 0f, 0f),
            new Vector4(0.700f, 0.300f, 0f, 0f),
            new Vector4(0f, 0.300f, 0.700f, 0f),
            new Vector4(0f, 0f, 0f, 1f));

        static readonly Matrix4x4 TritanopiaMatrix = new(
            new Vector4(0.950f, 0.050f, 0f, 0f),
            new Vector4(0f, 0.433f, 0.567f, 0f),
            new Vector4(0f, 0.475f, 0.525f, 0f),
            new Vector4(0f, 0f, 0f, 1f));

        public ColorblindRenderPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            profilingSampler = new ProfilingSampler("ColorblindCorrection");
        }

        public void SetMode(ColorblindMode mode) => _mode = mode;

        Material GetMaterial()
        {
            if (_material != null) return _material;

            var shader = Shader.Find("Hidden/Tartaria/ColorblindCorrection");
            if (shader == null)
            {
                Debug.LogWarning("[Colorblind] Shader 'Hidden/Tartaria/ColorblindCorrection' not found.");
                return null;
            }
            _material = new Material(shader) { hideFlags = HideFlags.DontSave };
            return _material;
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var mat = GetMaterial();
            if (mat == null) return;

            Matrix4x4 correction = _mode switch
            {
                ColorblindMode.Protanopia => ProtanopiaMatrix,
                ColorblindMode.Deuteranopia => DeuteranopiaMatrix,
                ColorblindMode.Tritanopia => TritanopiaMatrix,
                _ => Matrix4x4.identity
            };

            mat.SetMatrix(s_ColorMatrix, correction);

            var cmd = CommandBufferPool.Get("ColorblindCorrection");
#pragma warning disable CS0618
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
#pragma warning restore CS0618
            Blitter.BlitCameraTexture(cmd, source, source, mat, 0);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Cleanup()
        {
            if (_material != null)
                Object.DestroyImmediate(_material);
        }
    }
}
