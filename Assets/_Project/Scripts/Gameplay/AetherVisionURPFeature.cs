using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// URP Renderer Feature for Aether Vision. Two passes:
    ///   1) Full-screen desaturation when <see cref="AetherVisionOverlay.IsActive"/> is true.
    ///   2) Ley-line glow accumulation (additive) tinted by AetherVisionOverlay.LeyLineColor.
    ///
    /// Both passes blit through hidden shaders that the art lane authors separately
    /// (see HANDOFFS — "Aether Vision shaders"). Until those shaders ship, both passes
    /// no-op gracefully and log a one-time warning naming the missing shader id so the
    /// art lane has an exact target to author.
    ///
    /// Add this feature to <c>Assets/_Project/Settings/URP/UniversalRendererData.asset</c>
    /// via the inspector — "Add Renderer Feature → Aether Vision URP Feature".
    /// Per CLAUDE.md no-debt rule 4: no silent fallbacks — every shader miss logs once.
    /// </summary>
    public class AetherVisionURPFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("When to inject the desaturation pass. AfterRenderingPostProcessing keeps it on top of color grading.")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            [Range(0f, 1f)]
            [Tooltip("How much to desaturate when vision is active. 1 = full grayscale, 0 = none.")]
            public float desaturationStrength = 0.85f;

            [Range(0f, 4f)]
            [Tooltip("Additive multiplier for the ley-line glow pass.")]
            public float leyLineGlowIntensity = 1.4f;
        }

        [SerializeField] Settings settings = new();

        DesaturatePass _desaturatePass;
        LeyLineGlowPass _leyLinePass;

        // Logged-once flags so we don't spam every frame the shaders are missing.
        static bool s_loggedActivation;

        public override void Create()
        {
            _desaturatePass = new DesaturatePass(settings.renderPassEvent);
            _leyLinePass = new LeyLineGlowPass(settings.renderPassEvent);

            if (!s_loggedActivation)
            {
                Debug.Log("[AetherVisionURPFeature] Registered. Awaiting AetherVisionOverlay.IsActive=true.");
                s_loggedActivation = true;
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Only enqueue passes while vision is active — saves a full-screen blit per frame.
            var overlay = AetherVisionOverlay.Instance;
            if (overlay == null || !overlay.IsActive) return;

            // Skip in overlay cameras (UI) and the scene camera in Editor.
            var cam = renderingData.cameraData.cameraType;
            if (cam == CameraType.Preview || cam == CameraType.Reflection) return;

            _desaturatePass.Setup(settings.desaturationStrength);
            _leyLinePass.Setup(overlay.LeyLineColor, settings.leyLineGlowIntensity);

            renderer.EnqueuePass(_desaturatePass);
            renderer.EnqueuePass(_leyLinePass);
        }

        protected override void Dispose(bool disposing)
        {
            _desaturatePass?.Cleanup();
            _leyLinePass?.Cleanup();
        }

        // ───────────────────────────────────────────────────────────────────
        // PASS 1: Desaturation
        // ───────────────────────────────────────────────────────────────────
        class DesaturatePass : ScriptableRenderPass
        {
            const string k_ShaderId = "Hidden/Tartaria/AetherVision_Desaturate";
            static readonly int s_Strength = Shader.PropertyToID("_DesaturationStrength");

            Material _material;
            float _strength;
            static bool s_loggedMissingShader;

            public DesaturatePass(RenderPassEvent evt)
            {
                renderPassEvent = evt;
                profilingSampler = new ProfilingSampler("AetherVision.Desaturate");
            }

            public void Setup(float strength) => _strength = strength;

            Material GetMaterial()
            {
                if (_material != null) return _material;
                var shader = Shader.Find(k_ShaderId);
                if (shader == null)
                {
                    if (!s_loggedMissingShader)
                    {
                        Debug.LogWarning(
                            $"[AetherVisionURPFeature] Shader '{k_ShaderId}' not found. " +
                            "Desaturation pass will no-op until the art lane authors it. " +
                            "Author target: full-screen blit, _DesaturationStrength (float) lerp toward luminance.");
                        s_loggedMissingShader = true;
                    }
                    return null;
                }
                _material = new Material(shader) { hideFlags = HideFlags.DontSave };
                return _material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var mat = GetMaterial();
                if (mat == null) return;

                mat.SetFloat(s_Strength, _strength);

                var resourceData = frameData.Get<UniversalResourceData>();
                var source = resourceData.activeColorTexture;

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "AetherVision Desaturate", out var passData, profilingSampler))
                {
                    passData.material = mat;
                    builder.UseTexture(source, AccessFlags.ReadWrite);
                    builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }
            }

            class PassData
            {
                public Material material;
            }

            public void Cleanup()
            {
                if (_material != null)
                    Object.DestroyImmediate(_material);
            }
        }

        // ───────────────────────────────────────────────────────────────────
        // PASS 2: Ley-line glow (additive)
        // ───────────────────────────────────────────────────────────────────
        class LeyLineGlowPass : ScriptableRenderPass
        {
            const string k_ShaderId = "Hidden/Tartaria/AetherVision_LeyLineGlow";
            static readonly int s_GlowColor = Shader.PropertyToID("_LeyLineColor");
            static readonly int s_GlowIntensity = Shader.PropertyToID("_LeyLineIntensity");

            Material _material;
            Color _color = Color.yellow;
            float _intensity = 1f;
            static bool s_loggedMissingShader;

            public LeyLineGlowPass(RenderPassEvent evt)
            {
                renderPassEvent = evt;
                profilingSampler = new ProfilingSampler("AetherVision.LeyLineGlow");
            }

            public void Setup(Color color, float intensity)
            {
                _color = color;
                _intensity = intensity;
            }

            Material GetMaterial()
            {
                if (_material != null) return _material;
                var shader = Shader.Find(k_ShaderId);
                if (shader == null)
                {
                    if (!s_loggedMissingShader)
                    {
                        Debug.LogWarning(
                            $"[AetherVisionURPFeature] Shader '{k_ShaderId}' not found. " +
                            "Ley-line glow pass will no-op until the art lane authors it. " +
                            "Author target: additive full-screen blit, _LeyLineColor (Color) and _LeyLineIntensity (float).");
                        s_loggedMissingShader = true;
                    }
                    return null;
                }
                _material = new Material(shader) { hideFlags = HideFlags.DontSave };
                return _material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var mat = GetMaterial();
                if (mat == null) return;

                mat.SetColor(s_GlowColor, _color);
                mat.SetFloat(s_GlowIntensity, _intensity);

                var resourceData = frameData.Get<UniversalResourceData>();
                var source = resourceData.activeColorTexture;

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "AetherVision LeyLineGlow", out var passData, profilingSampler))
                {
                    passData.material = mat;
                    builder.UseTexture(source, AccessFlags.ReadWrite);
                    builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }
            }

            class PassData
            {
                public Material material;
            }

            public void Cleanup()
            {
                if (_material != null)
                    Object.DestroyImmediate(_material);
            }
        }
    }
}
