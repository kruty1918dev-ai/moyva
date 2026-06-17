using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderGraphModule.Util.RenderGraphUtils;

namespace Kruty1918.Moyva.Visuals
{
    public sealed class BadNorthStylizedRendererFeature : ScriptableRendererFeature
    {
        private const string ShaderName = "Hidden/Moyva/BadNorth Stylized Post Process";

        public enum DebugViewMode
        {
            FinalComposite = 0,
            AOOnly = 1,
            ContactOnly = 2,
            CreaseOnly = 3,
            CombinedMask = 4
        }

        [System.Serializable]
        public sealed class StylizedSSAOSettings
        {
            public bool Enabled = true;
            [Range(0f, 1.5f)] public float Intensity = 0.48f;
            [Range(0.05f, 2f)] public float Radius = 0.55f;
            [Range(4, 12)] public int SampleCount = 10;
            [Range(0.5f, 4f)] public float Power = 1.45f;
            [Range(0.01f, 1f)] public float Thickness = 0.24f;
            [Range(0f, 6f)] public float BlurRadius = 3f;
            [Range(0.1f, 6f)] public float DepthSensitivity = 1.55f;
            [Range(0f, 4f)] public float NormalSensitivity = 0.9f;
            [Min(0f)] public float DistanceFadeStart = 45f;
            [Min(0.01f)] public float DistanceFadeEnd = 160f;
            public Color OcclusionColor = new Color(0.27f, 0.31f, 0.38f, 1f);
            public bool AffectSky;
            [Tooltip("Fullscreen passes cannot isolate transparent pixels after they have been composited. Keep the render event before transparents when this is off.")]
            public bool AffectTransparent;
        }

        [System.Serializable]
        public sealed class ContactShadowSettings
        {
            public bool Enabled = true;
            [Range(0f, 1.5f)] public float ContactStrength = 0.28f;
            [Range(0.01f, 1.5f)] public float ContactRadius = 0.28f;
            [Range(0.01f, 1f)] public float ContactFalloff = 0.22f;
            public Color ContactColor = new Color(0.32f, 0.36f, 0.30f, 1f);
            [Range(0f, 2f)] public float SmallObjectBoost = 0.55f;
            [Range(0f, 2f)] public float GrassCardBoost = 0.35f;
        }

        [System.Serializable]
        public sealed class CreaseDarkeningSettings
        {
            public bool Enabled = true;
            [Range(0f, 1.5f)] public float CreaseStrength = 0.18f;
            [Range(0.1f, 3f)] public float CreaseRadius = 1.1f;
            [Range(0.001f, 2f)] public float CreaseThreshold = 0.18f;
            [Range(0.001f, 2f)] public float CreaseSoftness = 0.45f;
            public Color CreaseColor = new Color(0.34f, 0.38f, 0.32f, 1f);
        }

        [System.Serializable]
        public sealed class PaletteGradeSettings
        {
            public bool Enabled = true;
            [Range(-100f, 100f)] public float Saturation = -8f;
            [Range(-100f, 100f)] public float Contrast = -8f;
            public Color ShadowTint = new Color(0.25f, 0.31f, 0.41f, 1f);
            public Color HighlightTint = new Color(1f, 0.95f, 0.76f, 1f);
            public Color Lift = new Color(0.02f, 0.02f, 0.025f, 0f);
            public Color Gamma = Color.white;
            public Color Gain = Color.white;
            [Range(0f, 1f)] public float Blend = 0.34f;
        }

        [System.Serializable]
        public sealed class SoftDepthFogSettings
        {
            public bool Enabled = true;
            public Color FogColor = new Color(0.75f, 0.82f, 0.90f, 1f);
            [Range(0f, 1f)] public float FogStrength = 0.10f;
            [Min(0f)] public float FogStart = 35f;
            [Min(0.01f)] public float FogEnd = 180f;
            [Range(0f, 1f)] public float HeightInfluence = 0.20f;
            [Range(0f, 1f)] public float Blend = 0.75f;
        }

        [System.Serializable]
        public sealed class SoftVignetteSettings
        {
            public bool Enabled = true;
            [Range(0f, 0.35f)] public float VignetteStrength = 0.045f;
            [Range(0.1f, 2f)] public float VignetteRadius = 1.05f;
            [Range(0.01f, 2f)] public float VignetteSoftness = 0.55f;
            public Color VignetteColor = new Color(0.13f, 0.19f, 0.25f, 1f);
        }

        [Header("Injection")]
        [Tooltip("AfterRenderingSkybox keeps transparent water/UI safer. Enable SSAO/Affect Transparent before using a later event such as BeforeRenderingPostProcessing.")]
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        [SerializeField] private bool _applyInSceneView = true;
        [SerializeField] private DebugViewMode _debugView = DebugViewMode.FinalComposite;

        [Header("Stylized SSAO")]
        [SerializeField] private StylizedSSAOSettings _ssao = new();

        [Header("Contact Shadow / Grounding")]
        [SerializeField] private ContactShadowSettings _contact = new();

        [Header("Crease / Corner Darkening")]
        [SerializeField] private CreaseDarkeningSettings _crease = new();

        [Header("Bad North Palette Grade")]
        [SerializeField] private PaletteGradeSettings _palette = new();

        [Header("Soft Depth Fog")]
        [SerializeField] private SoftDepthFogSettings _fog = new();

        [Header("Very Soft Vignette")]
        [SerializeField] private SoftVignetteSettings _vignette = new();

        private Material _material;
        private BadNorthStylizedPass _pass;

        public override void Create()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find(ShaderName);
                if (shader != null)
                    _material = CoreUtils.CreateEngineMaterial(shader);
            }

            ApplyMaterialProperties();
            _pass = new BadNorthStylizedPass(_material)
            {
                renderPassEvent = ResolveRenderPassEvent()
            };
        }

        private void OnValidate()
        {
            ClampSettings();
            ApplyMaterialProperties();
            if (_pass != null)
                _pass.renderPassEvent = ResolveRenderPassEvent();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null || _pass == null || !IsAnyEffectEnabled())
                return;

            if (renderingData.cameraData.isPreviewCamera)
                return;
            if (!_applyInSceneView && renderingData.cameraData.isSceneViewCamera)
                return;
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
                return;

            ApplyMaterialProperties();
            _pass.renderPassEvent = ResolveRenderPassEvent();
            bool usesOcclusionMask = IsOcclusionMaskEnabled();
            bool usesDepth = IsDepthInputEnabled();
            _pass.Setup(
                Mathf.Max(_ssao.BlurRadius, 0f),
                Mathf.Max(_ssao.DepthSensitivity, 0.01f),
                usesOcclusionMask,
                usesOcclusionMask);

            var requirements = ScriptableRenderPassInput.None;
            if (usesDepth)
                requirements |= ScriptableRenderPassInput.Depth;
            if (usesOcclusionMask)
                requirements |= ScriptableRenderPassInput.Normal;
            _pass.ConfigureInput(requirements);

            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
                _material = null;
            }
        }

        private bool IsAnyEffectEnabled()
        {
            return (_ssao.Enabled && _ssao.Intensity > 0.001f)
                || (_contact.Enabled && _contact.ContactStrength > 0.001f)
                || (_crease.Enabled && _crease.CreaseStrength > 0.001f)
                || (_palette.Enabled && _palette.Blend > 0.001f)
                || (_fog.Enabled && _fog.FogStrength > 0.001f && _fog.Blend > 0.001f)
                || (_vignette.Enabled && _vignette.VignetteStrength > 0.001f)
                || _debugView != DebugViewMode.FinalComposite;
        }

        private bool IsOcclusionMaskEnabled()
        {
            return (_ssao.Enabled && _ssao.Intensity > 0.001f)
                || (_contact.Enabled && _contact.ContactStrength > 0.001f)
                || (_crease.Enabled && _crease.CreaseStrength > 0.001f)
                || _debugView != DebugViewMode.FinalComposite;
        }

        private bool IsDepthInputEnabled()
        {
            return IsOcclusionMaskEnabled()
                || (_palette.Enabled && _palette.Blend > 0.001f)
                || (_fog.Enabled && _fog.FogStrength > 0.001f && _fog.Blend > 0.001f)
                || (_vignette.Enabled && _vignette.VignetteStrength > 0.001f);
        }

        private RenderPassEvent ResolveRenderPassEvent()
        {
            if (!_ssao.AffectTransparent && _renderPassEvent > RenderPassEvent.AfterRenderingSkybox)
                return RenderPassEvent.AfterRenderingSkybox;

            return _renderPassEvent;
        }

        private void ClampSettings()
        {
            _ssao.DistanceFadeEnd = Mathf.Max(_ssao.DistanceFadeStart + 0.01f, _ssao.DistanceFadeEnd);
            _fog.FogEnd = Mathf.Max(_fog.FogStart + 0.01f, _fog.FogEnd);
            _palette.Gamma.r = Mathf.Max(0.001f, _palette.Gamma.r);
            _palette.Gamma.g = Mathf.Max(0.001f, _palette.Gamma.g);
            _palette.Gamma.b = Mathf.Max(0.001f, _palette.Gamma.b);
        }

        private void ApplyMaterialProperties()
        {
            if (_material == null)
                return;

            _material.SetColor("_OcclusionColor", _ssao.OcclusionColor);
            _material.SetColor("_ContactColor", _contact.ContactColor);
            _material.SetColor("_CreaseColor", _crease.CreaseColor);
            _material.SetColor("_ShadowTint", _palette.ShadowTint);
            _material.SetColor("_HighlightTint", _palette.HighlightTint);
            _material.SetColor("_Lift", _palette.Lift);
            _material.SetColor("_Gamma", _palette.Gamma);
            _material.SetColor("_Gain", _palette.Gain);
            _material.SetColor("_FogColor", _fog.FogColor);
            _material.SetColor("_VignetteColor", _vignette.VignetteColor);

            _material.SetFloat("_AOEnabled", _ssao.Enabled ? 1f : 0f);
            _material.SetFloat("_AOIntensity", _ssao.Intensity);
            _material.SetFloat("_AORadius", _ssao.Radius);
            _material.SetFloat("_AOSampleCount", _ssao.SampleCount);
            _material.SetFloat("_AOPower", _ssao.Power);
            _material.SetFloat("_AOThickness", _ssao.Thickness);
            _material.SetFloat("_AODepthSensitivity", _ssao.DepthSensitivity);
            _material.SetFloat("_AONormalSensitivity", _ssao.NormalSensitivity);
            _material.SetFloat("_AODistanceFadeStart", _ssao.DistanceFadeStart);
            _material.SetFloat("_AODistanceFadeEnd", _ssao.DistanceFadeEnd);
            _material.SetFloat("_AOAffectSky", _ssao.AffectSky ? 1f : 0f);
            _material.SetFloat("_AOAffectTransparent", _ssao.AffectTransparent ? 1f : 0f);

            _material.SetFloat("_ContactEnabled", _contact.Enabled ? 1f : 0f);
            _material.SetFloat("_ContactStrength", _contact.ContactStrength);
            _material.SetFloat("_ContactRadius", _contact.ContactRadius);
            _material.SetFloat("_ContactFalloff", _contact.ContactFalloff);
            _material.SetFloat("_SmallObjectBoost", _contact.SmallObjectBoost);
            _material.SetFloat("_GrassCardBoost", _contact.GrassCardBoost);

            _material.SetFloat("_CreaseEnabled", _crease.Enabled ? 1f : 0f);
            _material.SetFloat("_CreaseStrength", _crease.CreaseStrength);
            _material.SetFloat("_CreaseRadius", _crease.CreaseRadius);
            _material.SetFloat("_CreaseThreshold", _crease.CreaseThreshold);
            _material.SetFloat("_CreaseSoftness", _crease.CreaseSoftness);

            _material.SetFloat("_PaletteEnabled", _palette.Enabled ? 1f : 0f);
            _material.SetFloat("_Saturation", _palette.Saturation);
            _material.SetFloat("_Contrast", _palette.Contrast);
            _material.SetFloat("_PaletteBlend", _palette.Blend);

            _material.SetFloat("_FogEnabled", _fog.Enabled ? 1f : 0f);
            _material.SetFloat("_FogStrength", _fog.FogStrength);
            _material.SetFloat("_FogStart", _fog.FogStart);
            _material.SetFloat("_FogEnd", _fog.FogEnd);
            _material.SetFloat("_FogHeightInfluence", _fog.HeightInfluence);
            _material.SetFloat("_FogBlend", _fog.Blend);

            _material.SetFloat("_VignetteEnabled", _vignette.Enabled ? 1f : 0f);
            _material.SetFloat("_VignetteStrength", _vignette.VignetteStrength);
            _material.SetFloat("_VignetteRadius", _vignette.VignetteRadius);
            _material.SetFloat("_VignetteSoftness", _vignette.VignetteSoftness);
            _material.SetFloat("_DebugView", (float)_debugView);
        }

        private sealed class BadNorthStylizedPass : ScriptableRenderPass
        {
            private const int OcclusionPass = 0;
            private const int BlurPass = 1;
            private const int CompositePass = 2;
            private const int CopyPass = 3;

            private static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");
            private static readonly int OcclusionTextureId = Shader.PropertyToID("_BadNorthOcclusionTexture");
            private static readonly int BlurDirectionId = Shader.PropertyToID("_BlurDirection");
            private static readonly int TexelSizeId = Shader.PropertyToID("_BadNorth_TexelSize");
            private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
            private static readonly int BlurDepthSensitivityId = Shader.PropertyToID("_BlurDepthSensitivity");
            private static readonly int DepthAvailableId = Shader.PropertyToID("_DepthAvailable");
            private static readonly int OcclusionMaskEnabledId = Shader.PropertyToID("_OcclusionMaskEnabled");
            private static readonly MaterialPropertyBlock SharedPropertyBlock = new();

            private readonly Material _material;
            private float _blurRadius = 3f;
            private float _blurDepthSensitivity = 1.5f;
            private bool _requiresNormals = true;
            private bool _usesOcclusionMask = true;

            public BadNorthStylizedPass(Material material)
            {
                _material = material;
            }

            public void Setup(
                float blurRadius,
                float blurDepthSensitivity,
                bool requiresNormals,
                bool usesOcclusionMask)
            {
                _blurRadius = blurRadius;
                _blurDepthSensitivity = blurDepthSensitivity;
                _requiresNormals = requiresNormals;
                _usesOcclusionMask = usesOcclusionMask;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_material == null)
                    return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                    return;

                TextureHandle color = resourceData.activeColorTexture;
                if (!color.IsValid())
                    return;

                TextureHandle depth = resourceData.cameraDepthTexture;
                TextureHandle normals = resourceData.cameraNormalsTexture;
                bool depthAvailable = depth.IsValid();
                bool occlusionActive = _usesOcclusionMask && depthAvailable;

                TextureDesc colorDesc = renderGraph.GetTextureDesc(color);
                Vector4 texelSize = new Vector4(
                    1f / Mathf.Max(1, colorDesc.width),
                    1f / Mathf.Max(1, colorDesc.height),
                    colorDesc.width,
                    colorDesc.height);

                TextureDesc maskDesc = colorDesc;
                maskDesc.name = "_MoyvaBadNorthOcclusionMask";
                maskDesc.depthBufferBits = DepthBits.None;
                maskDesc.clearBuffer = true;
                maskDesc.clearColor = Color.clear;
                maskDesc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;

                TextureHandle occlusion = renderGraph.CreateTexture(maskDesc);
                TextureHandle occlusionForComposite = occlusion;

                AddOcclusionPass(renderGraph, occlusion, depth, normals, texelSize, depthAvailable, occlusionActive);
                if (occlusionActive && _blurRadius > 0.01f)
                {
                    TextureHandle blurA = renderGraph.CreateTexture(maskDesc);
                    TextureHandle blurB = renderGraph.CreateTexture(maskDesc);
                    AddBlurPass(renderGraph, occlusion, blurA, depth, texelSize, new Vector2(1f, 0f), "Moyva BadNorth AO Blur Horizontal", depthAvailable, occlusionActive);
                    AddBlurPass(renderGraph, blurA, blurB, depth, texelSize, new Vector2(0f, 1f), "Moyva BadNorth AO Blur Vertical", depthAvailable, occlusionActive);
                    occlusionForComposite = blurB;
                }

                TextureDesc finalDesc = colorDesc;
                finalDesc.name = "_MoyvaBadNorthStylizedColor";
                finalDesc.depthBufferBits = DepthBits.None;
                finalDesc.clearBuffer = false;
                TextureHandle finalColor = renderGraph.CreateTexture(finalDesc);

                AddCompositePass(renderGraph, color, occlusionForComposite, finalColor, depth, texelSize, depthAvailable, occlusionActive);

                var copyBackParams = new BlitMaterialParameters(finalColor, color, _material, CopyPass);
                renderGraph.AddBlitPass(copyBackParams, "Moyva BadNorth Copy Back");
            }

            private void AddOcclusionPass(
                RenderGraph renderGraph,
                TextureHandle destination,
                TextureHandle depth,
                TextureHandle normals,
                Vector4 texelSize,
                bool depthAvailable,
                bool occlusionActive)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Moyva BadNorth Stylized Occlusion",
                    out var passData);

                passData.material = _material;
                passData.passIndex = OcclusionPass;
                passData.texelSize = texelSize;
                passData.depth = depth;
                passData.normals = normals;
                passData.depthAvailable = depthAvailable;
                passData.occlusionMaskEnabled = occlusionActive;

                if (depth.IsValid())
                    builder.UseTexture(depth, AccessFlags.Read);
                if (_requiresNormals && normals.IsValid())
                    builder.UseTexture(normals, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => DrawMaterialPass(data, context));
            }

            private void AddBlurPass(
                RenderGraph renderGraph,
                TextureHandle source,
                TextureHandle destination,
                TextureHandle depth,
                Vector4 texelSize,
                Vector2 direction,
                string passName,
                bool depthAvailable,
                bool occlusionActive)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData);
                passData.material = _material;
                passData.passIndex = BlurPass;
                passData.source = source;
                passData.depth = depth;
                passData.texelSize = texelSize;
                passData.blurDirection = direction;
                passData.blurRadius = _blurRadius;
                passData.blurDepthSensitivity = _blurDepthSensitivity;
                passData.depthAvailable = depthAvailable;
                passData.occlusionMaskEnabled = occlusionActive;

                builder.UseTexture(source, AccessFlags.Read);
                if (depth.IsValid())
                    builder.UseTexture(depth, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => DrawMaterialPass(data, context));
            }

            private void AddCompositePass(
                RenderGraph renderGraph,
                TextureHandle color,
                TextureHandle occlusion,
                TextureHandle destination,
                TextureHandle depth,
                Vector4 texelSize,
                bool depthAvailable,
                bool occlusionActive)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Moyva BadNorth Final Composite",
                    out var passData);

                passData.material = _material;
                passData.passIndex = CompositePass;
                passData.source = color;
                passData.occlusion = occlusion;
                passData.depth = depth;
                passData.texelSize = texelSize;
                passData.depthAvailable = depthAvailable;
                passData.occlusionMaskEnabled = occlusionActive;

                builder.UseTexture(color, AccessFlags.Read);
                builder.UseTexture(occlusion, AccessFlags.Read);
                if (depth.IsValid())
                    builder.UseTexture(depth, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => DrawMaterialPass(data, context));
            }

            private static void DrawMaterialPass(PassData data, RasterGraphContext context)
            {
                SharedPropertyBlock.Clear();
                SharedPropertyBlock.SetVector(TexelSizeId, data.texelSize);
                SharedPropertyBlock.SetVector(BlurDirectionId, data.blurDirection);
                SharedPropertyBlock.SetFloat(BlurRadiusId, data.blurRadius);
                SharedPropertyBlock.SetFloat(BlurDepthSensitivityId, data.blurDepthSensitivity);
                SharedPropertyBlock.SetFloat(DepthAvailableId, data.depthAvailable ? 1f : 0f);
                SharedPropertyBlock.SetFloat(OcclusionMaskEnabledId, data.occlusionMaskEnabled ? 1f : 0f);

                if (data.source.IsValid())
                    SharedPropertyBlock.SetTexture(BlitTextureId, data.source);
                if (data.occlusion.IsValid())
                    SharedPropertyBlock.SetTexture(OcclusionTextureId, data.occlusion);

                context.cmd.DrawProcedural(
                    Matrix4x4.identity,
                    data.material,
                    data.passIndex,
                    MeshTopology.Triangles,
                    3,
                    1,
                    SharedPropertyBlock);
            }

            private sealed class PassData
            {
                public Material material;
                public int passIndex;
                public TextureHandle source;
                public TextureHandle occlusion;
                public TextureHandle depth;
                public TextureHandle normals;
                public Vector4 texelSize;
                public Vector2 blurDirection;
                public float blurRadius;
                public float blurDepthSensitivity;
                public bool depthAvailable;
                public bool occlusionMaskEnabled;
            }
        }
    }
}
