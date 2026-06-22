using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Kruty1918.Moyva.Visuals
{
    public sealed class DecorSharedStylizedRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private bool _renderContactShadows = true;
        [SerializeField] private bool _renderMeshOutlines = true;
        [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        [SerializeField] private LayerMask _layerMask = -1;

        private DecorPass _contactShadowPass;
        private DecorPass _outlinePass;

        public override void Create()
        {
            _contactShadowPass = new DecorPass("Decor Contact Shadow", "DecorContactShadow")
            {
                renderPassEvent = _renderPassEvent
            };
            _outlinePass = new DecorPass("Decor Mesh Outline", "DecorOutline")
            {
                renderPassEvent = _renderPassEvent
            };
        }

        private void OnValidate()
        {
            if (_contactShadowPass != null)
                _contactShadowPass.renderPassEvent = _renderPassEvent;
            if (_outlinePass != null)
                _outlinePass.renderPassEvent = _renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;
            if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
                return;

            if (_renderContactShadows && _contactShadowPass != null)
            {
                _contactShadowPass.Setup(_layerMask);
                renderer.EnqueuePass(_contactShadowPass);
            }

            if (_renderMeshOutlines && _outlinePass != null)
            {
                _outlinePass.Setup(_layerMask);
                renderer.EnqueuePass(_outlinePass);
            }
        }

        private sealed class DecorPass : ScriptableRenderPass
        {
            private readonly string _passName;
            private readonly ShaderTagId _shaderTagId;
            private LayerMask _layerMask = -1;

            public DecorPass(string passName, string shaderTag)
            {
                _passName = passName;
                _shaderTagId = new ShaderTagId(shaderTag);
                profilingSampler = new ProfilingSampler(passName);
            }

            public void Setup(LayerMask layerMask)
            {
                _layerMask = layerMask;
            }

#if URP_COMPATIBILITY_MODE
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var command = CommandBufferPool.Get(_passName);
                using (new ProfilingScope(command, profilingSampler))
                {
                    context.ExecuteCommandBuffer(command);
                    command.Clear();

                    UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
                    var sortingCriteria = cameraData.defaultOpaqueSortFlags;
                    var drawingSettings = RenderingUtils.CreateDrawingSettings(_shaderTagId, ref renderingData, sortingCriteria);
                    var filteringSettings = new FilteringSettings(RenderQueueRange.all, _layerMask);
                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                }

                context.ExecuteCommandBuffer(command);
                CommandBufferPool.Release(command);
            }
#endif

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(_passName, out var passData, profilingSampler))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    InitRendererList(frameData, renderGraph, ref passData);

                    if (!passData.RendererList.IsValid())
                        return;

                    builder.UseRendererList(passData.RendererList);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
                    builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                    {
                        context.cmd.DrawRendererList(data.RendererList);
                    });
                }
            }

            private void InitRendererList(ContextContainer frameData, RenderGraph renderGraph, ref PassData passData)
            {
                UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                var sortingCriteria = cameraData.defaultOpaqueSortFlags;
                var drawingSettings = RenderingUtils.CreateDrawingSettings(_shaderTagId, universalRenderingData, cameraData, lightData, sortingCriteria);
                var filteringSettings = new FilteringSettings(RenderQueueRange.all, _layerMask);
                var rendererListParams = new RendererListParams(universalRenderingData.cullResults, drawingSettings, filteringSettings);
                passData.RendererList = renderGraph.CreateRendererList(rendererListParams);
            }

            private sealed class PassData
            {
                public RendererListHandle RendererList;
            }
        }
    }
}
