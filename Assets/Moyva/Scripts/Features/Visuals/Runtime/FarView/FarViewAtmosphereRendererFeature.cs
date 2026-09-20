using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>
    /// Fullscreen high-altitude atmosphere composite.
    /// Runs AFTER Fog of War (FoW injects at BeforeRenderingPostProcessing - 10,
    /// this feature defaults to BeforeRenderingPostProcessing) so the effect can
    /// only restyle pixels the player is allowed to see — it never reconstructs
    /// hidden world state. Reads only global uniforms published by
    /// FarViewAtmosphereDriver; zero-cost when FarViewWeight ≈ 0.
    /// </summary>
    public sealed class FarViewAtmosphereRendererFeature : ScriptableRendererFeature
    {
        public const string ShaderName = "Hidden/Moyva/FarViewAtmosphere";
        public const float WeightEpsilon = 0.001f;

        /// <summary>RenderPassEvent used by FogOfWarScreenSpaceRendererFeature.</summary>
        public const RenderPassEvent FogCompositeEvent =
            (RenderPassEvent)((int)RenderPassEvent.BeforeRenderingPostProcessing - 10);

        private static readonly string[] ShaderAssetPaths =
        {
            "Assets/Moyva/Art/Shaders/URP/FarViewAtmosphere.shader"
        };

        private static readonly int WeightId =
            Shader.PropertyToID("_MoyvaFarViewWeight");
        private static readonly int DebugId =
            Shader.PropertyToID("_MoyvaFarViewDebug");

        public enum DebugView
        {
            Final = 0,
            Weight = 1,
            HazeOnly = 2,
            VeilOnly = 3,
            FlattenOnly = 4,
            EyeDepth = 5
        }

        [SerializeField] private Shader _shader;

        [SerializeField]
        [Tooltip("Must stay AFTER the Fog of War composite so atmosphere can only "
               + "restyle already-visible pixels.")]
        private RenderPassEvent _renderPassEvent =
            RenderPassEvent.BeforeRenderingPostProcessing;

        [SerializeField] private bool _applyInSceneView = true;
        [SerializeField] private bool _applyInSceneViewDuringPlay;
        [SerializeField] private DebugView _debugView = DebugView.Final;

        private Material _material;
        private FarViewPass _pass;

        public override void Create()
        {
            ResolveShaderReference();
            if (_material == null
                && _shader != null
                && _shader.isSupported)
            {
                _material = CoreUtils.CreateEngineMaterial(_shader);
            }

            _pass = new FarViewPass(_material)
            {
                renderPassEvent = _renderPassEvent
            };
        }

        private void OnValidate()
        {
            ResolveShaderReference();
            if (_pass != null)
                _pass.renderPassEvent = _renderPassEvent;
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (_material == null || _pass == null)
            {
                ResolveShaderReference();
                if (_material == null
                    && _shader != null
                    && _shader.isSupported)
                {
                    _material = CoreUtils.CreateEngineMaterial(_shader);
                    _pass = new FarViewPass(_material)
                    {
                        renderPassEvent = _renderPassEvent
                    };
                }
                if (_material == null || _pass == null)
                    return;
            }

            CameraData cameraData = renderingData.cameraData;
            if (cameraData.isPreviewCamera
                || cameraData.renderType == CameraRenderType.Overlay)
                return;

            Camera camera = cameraData.camera;
            if (camera != null
                && camera.cameraType == CameraType.SceneView
                && (!_applyInSceneView
                    || (Application.isPlaying
                        && !_applyInSceneViewDuringPlay)))
                return;

            float weight = Shader.GetGlobalFloat(WeightId);
            bool debugActive = _debugView != DebugView.Final;
            if (weight <= WeightEpsilon && !debugActive)
                return;

            _material.SetFloat(DebugId, (float)_debugView);
            _pass.renderPassEvent = _renderPassEvent;
            _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(_pass);
        }

        private void ResolveShaderReference()
        {
            if (_shader != null)
                return;

            _shader = Shader.Find(ShaderName);
            if (_shader != null)
                return;

#if UNITY_EDITOR
            for (int i = 0; i < ShaderAssetPaths.Length; i++)
            {
                var resolved = AssetDatabase.LoadAssetAtPath<Shader>(
                    ShaderAssetPaths[i]);
                if (resolved != null)
                {
                    _shader = resolved;
                    return;
                }
            }
#endif
        }

        protected override void Dispose(bool disposing)
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
                _material = null;
            }
            _pass = null;
        }

        private sealed class FarViewPass : ScriptableRenderPass
        {
            private const int CompositePass = 0;
            private const int CopyPass = 1;

            private readonly ProfilingSampler _compositeSampler =
                new ProfilingSampler("Moyva FarView Composite");
            private readonly ProfilingSampler _copySampler =
                new ProfilingSampler("Moyva FarView Copy Back");

            private Material _material;

            public FarViewPass(Material material)
            {
                _material = material;
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
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
                bool depthAvailable = depth.IsValid();
                _material.SetFloat(
                    Shader.PropertyToID("_MoyvaFarViewDepthAvailable"),
                    depthAvailable ? 1f : 0f);

                TextureDesc tempDesc = renderGraph.GetTextureDesc(color);
                tempDesc.name = "_MoyvaFarViewAtmosphereColor";
                tempDesc.depthBufferBits = DepthBits.None;
                tempDesc.clearBuffer = false;
                TextureHandle composite = renderGraph.CreateTexture(tempDesc);

                AddBlitPass(
                    renderGraph,
                    "Moyva FarView Composite",
                    color,
                    composite,
                    CompositePass,
                    _compositeSampler,
                    depth,
                    depthAvailable);

                AddBlitPass(
                    renderGraph,
                    "Moyva FarView Copy Back",
                    composite,
                    color,
                    CopyPass,
                    _copySampler,
                    depth,
                    depthAvailable: false);
            }

            private void AddBlitPass(
                RenderGraph renderGraph,
                string passName,
                TextureHandle source,
                TextureHandle destination,
                int materialPass,
                ProfilingSampler sampler,
                TextureHandle depth,
                bool depthAvailable)
            {
                using var builder =
                    renderGraph.AddRasterRenderPass<BlitPassData>(
                        passName,
                        out BlitPassData passData,
                        sampler);

                passData.Source = source;
                passData.Material = _material;
                passData.PassIndex = materialPass;

                builder.UseTexture(source, AccessFlags.Read);
                if (depthAvailable && depth.IsValid())
                    builder.UseTexture(depth, AccessFlags.Read);

                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    static (BlitPassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(
                            context.cmd,
                            data.Source,
                            new Vector4(1f, 1f, 0f, 0f),
                            data.Material,
                            data.PassIndex);
                    });
            }

            private sealed class BlitPassData
            {
                public TextureHandle Source;
                public Material Material;
                public int PassIndex;
            }
        }
    }
}
