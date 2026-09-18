using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Повний depth-aware screen-space FogOfWar pipeline.
    ///
    /// Послідовність одного RenderGraph pass:
    /// 1. FogSurfaceDepth — повторно малює fog-relevant world renderers
    ///    простим override shader і отримує найближчу реальну поверхню,
    ///    включно з transparent water.
    /// 2. RawScreenState — переводить world fog mask у screen-space.
    /// 3. Composite — point-stable fog mask, surface-locked
    ///    grid edge, main grey unexplored fog.
    /// 4. CopyBack — повертає результат у camera color.
    ///
    /// Legacy world curtain у цьому режимі не використовується.
    /// </summary>
    [System.Serializable]
    public sealed class FogOfWarScreenSpaceRendererFeature
        : ScriptableRendererFeature
    {
        private const string ScreenShaderName =
            "Moyva/FogOfWar/ScreenSpace";

        private const string SurfaceDepthShaderName =
            "Moyva/FogOfWar/SurfaceDepth";

        private static readonly string[] ScreenShaderAssetPaths =
        {
            "Assets/Moyva/Scripts/Features/FogOfWar/Runtime/Visual/ScreenSpace/FogOfWarScreenSpace.shader",
            "Assets/Moyva/Shaders/FogOfWar/FogOfWarScreenSpace.shader"
        };

        private static readonly string[] SurfaceDepthShaderAssetPaths =
        {
            "Assets/Moyva/Scripts/Features/FogOfWar/Runtime/Visual/ScreenSpace/FogSurfaceDepth.shader",
            "Assets/Moyva/Shaders/FogOfWar/FogSurfaceDepth.shader"
        };

        private static readonly RenderPassEvent FogRenderPassEvent =
            (RenderPassEvent)(
                (int)RenderPassEvent.BeforeRenderingPostProcessing
                - 10);

        [SerializeField]
        private Shader _screenSpaceShader;

        [SerializeField]
        private Shader _surfaceDepthShader;

        [SerializeField]
        [Tooltip(
            "Renderers, які формують fog surface depth. " +
            "За замовчуванням Everything. Виключи UI, Clouds та " +
            "інші суто декоративні шари, якщо вони мають MeshRenderer.")]
        private LayerMask _fogSurfaceLayerMask = -1;

        [SerializeField]
        [Range(0.5f, 1f)]
        [Tooltip(
            "Роздільна здатність screen-state/morphology texture. " +
            "Для стабільного краю при zoom рекомендовано 1.0.")]
        private float _screenStateScale = 1f;

        [SerializeField]
        [Tooltip(
            "Примусово використовує full-resolution screen state. " +
            "Прибирає ступінчастість, плаваючу товщину та нерівності " +
            "контуру під час віддалення камери.")]
        private bool _forceFullResolutionState = true;

        [SerializeField]
        private bool _applyInSceneView = true;

        [SerializeField]
        [Tooltip(
            "Якщо вимкнено, під час Play Mode Fog of War рендериться " +
            "тільки для Game camera, а не дублюється у Scene View.")]
        private bool _applyInSceneViewDuringPlay;

        private Material _screenSpaceMaterial;
        private Material _surfaceDepthMaterial;
        private FogScreenSpacePass _pass;
        private bool _loggedUnavailable;

        public override void Create()
        {
            DisposeMaterials();

            ResolveShaderReferences();
            CreateMaterials();

            _pass =
                new FogScreenSpacePass
                {
                    renderPassEvent =
                        FogRenderPassEvent
                };

            _loggedUnavailable = false;
        }

        private void OnValidate()
        {
            _screenStateScale =
                Mathf.Clamp(
                    _screenStateScale,
                    0.5f,
                    1f);

            ResolveShaderReferences();

            if (_pass != null)
            {
                _pass.renderPassEvent =
                    FogRenderPassEvent;
            }
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (_pass == null
                || _screenSpaceMaterial == null
                || _surfaceDepthMaterial == null)
            {
                /*
                 * Renderer Feature asset can survive a domain reload while
                 * shader imports finish slightly later. Retry once here.
                 */
                ResolveShaderReferences();
                CreateMaterials();

                if (_pass == null
                    || _screenSpaceMaterial == null
                    || _surfaceDepthMaterial == null)
                {
                    LogUnavailableOnce();
                    return;
                }
            }

            CameraData cameraData =
                renderingData.cameraData;

            if (cameraData.isPreviewCamera
                || cameraData.renderType
                    == CameraRenderType.Overlay)
            {
                return;
            }

            Camera camera =
                cameraData.camera;

            if (camera != null
                && camera.cameraType
                    == CameraType.SceneView)
            {
                if (!_applyInSceneView
                    || (Application.isPlaying
                        && !_applyInSceneViewDuringPlay))
                {
                    return;
                }
            }

            float effectiveStateScale =
                _forceFullResolutionState
                    ? 1f
                    : Mathf.Clamp(
                        _screenStateScale,
                        0.5f,
                        1f);

            _pass.Setup(
                _screenSpaceMaterial,
                _surfaceDepthMaterial,
                _fogSurfaceLayerMask,
                effectiveStateScale);

            renderer.EnqueuePass(
                _pass);
        }

        private void ResolveShaderReferences()
        {
            _screenSpaceShader =
                ResolveShaderReference(
                    _screenSpaceShader,
                    ScreenShaderName,
                    ScreenShaderAssetPaths);

            _surfaceDepthShader =
                ResolveShaderReference(
                    _surfaceDepthShader,
                    SurfaceDepthShaderName,
                    SurfaceDepthShaderAssetPaths);

        }

        private static Shader ResolveShaderReference(
            Shader current,
            string shaderName,
            string[] candidateAssetPaths)
        {
            if (current != null)
                return current;

            Shader resolved =
                Shader.Find(shaderName);

            if (resolved != null)
                return resolved;

#if UNITY_EDITOR
            if (candidateAssetPaths != null)
            {
                for (int i = 0;
                     i < candidateAssetPaths.Length;
                     i++)
                {
                    string path =
                        candidateAssetPaths[i];

                    if (string.IsNullOrWhiteSpace(path))
                        continue;

                    resolved =
                        AssetDatabase.LoadAssetAtPath<Shader>(
                            path);

                    if (resolved != null)
                        return resolved;
                }
            }

            string[] shaderGuids =
                AssetDatabase.FindAssets(
                    "t:Shader");

            for (int i = 0;
                 i < shaderGuids.Length;
                 i++)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(
                        shaderGuids[i]);

                Shader candidate =
                    AssetDatabase.LoadAssetAtPath<Shader>(
                        assetPath);

                if (candidate != null
                    && candidate.name == shaderName)
                {
                    return candidate;
                }
            }
#endif

            return null;
        }

        private void CreateMaterials()
        {
            if (_screenSpaceMaterial == null
                && _screenSpaceShader != null
                && _screenSpaceShader.isSupported)
            {
                _screenSpaceMaterial =
                    CoreUtils.CreateEngineMaterial(
                        _screenSpaceShader);
            }

            if (_surfaceDepthMaterial == null
                && _surfaceDepthShader != null
                && _surfaceDepthShader.isSupported)
            {
                _surfaceDepthMaterial =
                    CoreUtils.CreateEngineMaterial(
                        _surfaceDepthShader);
            }
        }

        private void LogUnavailableOnce()
        {
            if (_loggedUnavailable)
                return;

            _loggedUnavailable = true;

            Debug.LogError(
                "[MOYVA_FOG_PRESENTATION] " +
                "DepthAware pipeline is unavailable. " +
                "screenShader=" +
                DescribeShader(_screenSpaceShader) +
                " surfaceDepthShader=" +
                DescribeShader(_surfaceDepthShader) +
                ". Expected shader names: '" +
                ScreenShaderName +
                "' and '" +
                SurfaceDepthShaderName +
                "'. Expected files: " +
                string.Join(
                    " | ",
                    ScreenShaderAssetPaths) +
                " ; " +
                string.Join(
                    " | ",
                    SurfaceDepthShaderAssetPaths) +
                ". Reimport the shader files or assign both shader " +
                "references directly in the Renderer Feature inspector.");
        }

        private static string DescribeShader(
            Shader shader)
        {
            if (shader == null)
                return "Missing";

            return shader.name
                   + "(supported="
                   + shader.isSupported
                   + ")";
        }

        protected override void Dispose(
            bool disposing)
        {
            if (!disposing)
                return;

            _pass = null;
            DisposeMaterials();
        }

        private void DisposeMaterials()
        {
            CoreUtils.Destroy(
                _screenSpaceMaterial);

            CoreUtils.Destroy(
                _surfaceDepthMaterial);

            _screenSpaceMaterial = null;
            _surfaceDepthMaterial = null;
        }

        private sealed class FogScreenSpacePass
            : ScriptableRenderPass
        {
            private const int BuildScreenStatePass = 0;
            private const int CompositePass = 1;
            private const int CopyBackPass = 2;

            private static readonly int SurfaceEyeDepthTextureId =
                Shader.PropertyToID(
                    "_MoyvaFogSurfaceEyeDepthTexture");

            private static readonly int RawStateTextureId =
                Shader.PropertyToID(
                    "_MoyvaFogScreenStateRawTexture");

            private static readonly ShaderTagId UniversalForwardTag =
                new ShaderTagId("UniversalForward");

            private static readonly ShaderTagId UniversalForwardOnlyTag =
                new ShaderTagId("UniversalForwardOnly");

            private static readonly ShaderTagId SrpDefaultUnlitTag =
                new ShaderTagId("SRPDefaultUnlit");

            private static readonly ShaderTagId UniversalGBufferTag =
                new ShaderTagId("UniversalGBuffer");

            private readonly ProfilingSampler _surfaceDepthSampler =
                new ProfilingSampler(
                    "Moyva Fog Surface Depth");

            private readonly ProfilingSampler _stateSampler =
                new ProfilingSampler(
                    "Moyva Fog Screen State");

            private readonly ProfilingSampler _compositeSampler =
                new ProfilingSampler(
                    "Moyva Fog Final Composite");

            private readonly ProfilingSampler _copySampler =
                new ProfilingSampler(
                    "Moyva Fog Copy Back");

            private Material _screenSpaceMaterial;
            private Material _surfaceDepthMaterial;
            private LayerMask _layerMask;
            private float _stateScale = 1f;

            public FogScreenSpacePass()
            {
                /*
                 * Color input forces a sampleable intermediate camera
                 * texture before the final fullscreen composite.
                 */
                ConfigureInput(
                    ScriptableRenderPassInput.Color);
            }

            public void Setup(
                Material screenSpaceMaterial,
                Material surfaceDepthMaterial,
                LayerMask layerMask,
                float stateScale)
            {
                _screenSpaceMaterial =
                    screenSpaceMaterial;

                _surfaceDepthMaterial =
                    surfaceDepthMaterial;

                _layerMask =
                    layerMask;

                _stateScale =
                    Mathf.Clamp(
                        stateScale,
                        0.5f,
                        1f);
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                if (_screenSpaceMaterial == null
                    || _surfaceDepthMaterial == null)
                {
                    return;
                }

                UniversalResourceData resourceData =
                    frameData.Get<UniversalResourceData>();

                UniversalCameraData cameraData =
                    frameData.Get<UniversalCameraData>();

                UniversalRenderingData renderingData =
                    frameData.Get<UniversalRenderingData>();

                UniversalLightData lightData =
                    frameData.Get<UniversalLightData>();

                if (resourceData.isActiveTargetBackBuffer)
                {
                    return;
                }

                TextureHandle cameraColor =
                    resourceData.activeColorTexture;

                RenderTextureDescriptor cameraDescriptor =
                    cameraData.cameraTargetDescriptor;

                TextureHandle surfaceEyeDepth =
                    CreateSurfaceEyeDepthTexture(
                        renderGraph,
                        cameraDescriptor);

                TextureHandle surfaceDepthAttachment =
                    CreateSurfaceDepthAttachment(
                        renderGraph,
                        cameraDescriptor);

                RendererListHandle surfaceRendererList =
                    CreateSurfaceRendererList(
                        renderGraph,
                        renderingData,
                        cameraData,
                        lightData);

                AddSurfaceDepthPass(
                    renderGraph,
                    surfaceRendererList,
                    surfaceEyeDepth,
                    surfaceDepthAttachment);

                RenderTextureDescriptor stateDescriptor =
                    cameraDescriptor;

                stateDescriptor.width =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            stateDescriptor.width
                            * _stateScale));

                stateDescriptor.height =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(
                            stateDescriptor.height
                            * _stateScale));

                stateDescriptor.msaaSamples = 1;
                stateDescriptor.depthBufferBits = 0;
                stateDescriptor.depthStencilFormat =
                    GraphicsFormat.None;
                stateDescriptor.graphicsFormat =
                    GraphicsFormat.R16G16B16A16_SFloat;

                TextureHandle rawState =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        stateDescriptor,
                        "Moyva Fog Raw Screen State",
                        false);

                AddBlitPass(
                    renderGraph,
                    "Moyva Fog Build Screen State",
                    surfaceEyeDepth,
                    rawState,
                    _screenSpaceMaterial,
                    BuildScreenStatePass,
                    _stateSampler,
                    RawStateTextureId);

                RenderTextureDescriptor compositeDescriptor =
                    cameraDescriptor;

                compositeDescriptor.msaaSamples = 1;
                compositeDescriptor.depthBufferBits = 0;
                compositeDescriptor.depthStencilFormat =
                    GraphicsFormat.None;

                TextureHandle compositeTexture =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        compositeDescriptor,
                        "Moyva Fog Composite",
                        false);

                AddCompositePass(
                    renderGraph,
                    cameraColor,
                    compositeTexture);

                AddBlitPass(
                    renderGraph,
                    "Moyva Fog Copy Back",
                    compositeTexture,
                    cameraColor,
                    _screenSpaceMaterial,
                    CopyBackPass,
                    _copySampler,
                    0);
            }

            private RendererListHandle CreateSurfaceRendererList(
                RenderGraph renderGraph,
                UniversalRenderingData renderingData,
                UniversalCameraData cameraData,
                UniversalLightData lightData)
            {
                DrawingSettings drawSettings =
                    RenderingUtils.CreateDrawingSettings(
                        UniversalForwardTag,
                        renderingData,
                        cameraData,
                        lightData,
                        cameraData.defaultOpaqueSortFlags);

                drawSettings.SetShaderPassName(
                    1,
                    UniversalForwardOnlyTag);

                drawSettings.SetShaderPassName(
                    2,
                    SrpDefaultUnlitTag);

                drawSettings.SetShaderPassName(
                    3,
                    UniversalGBufferTag);

                drawSettings.overrideMaterial =
                    _surfaceDepthMaterial;

                drawSettings.overrideMaterialPassIndex = 0;

                FilteringSettings filteringSettings =
                    new FilteringSettings(
                        RenderQueueRange.all,
                        _layerMask.value);

                RendererListParams rendererListParams =
                    new RendererListParams(
                        renderingData.cullResults,
                        drawSettings,
                        filteringSettings);

                return renderGraph.CreateRendererList(
                    rendererListParams);
            }

            private void AddSurfaceDepthPass(
                RenderGraph renderGraph,
                RendererListHandle rendererList,
                TextureHandle colorTarget,
                TextureHandle depthTarget)
            {
                using (var builder =
                    renderGraph.AddRasterRenderPass<SurfacePassData>(
                        "Moyva Fog Surface Depth",
                        out SurfacePassData passData,
                        _surfaceDepthSampler))
                {
                    passData.RendererList =
                        rendererList;

                    passData.DepthClearValue =
                        SystemInfo.usesReversedZBuffer
                            ? 0f
                            : 1f;

                    builder.UseRendererList(
                        rendererList);

                    builder.SetRenderAttachment(
                        colorTarget,
                        0);

                    /*
                     * Full-resolution immutable geometry source.
                     * Screen-state morphology may modify only fog coverage.
                     */
                    builder.SetGlobalTextureAfterPass(
                        colorTarget,
                        SurfaceEyeDepthTextureId);

                    builder.SetRenderAttachmentDepth(
                        depthTarget,
                        AccessFlags.Write);

                    builder.AllowPassCulling(
                        false);

                    builder.SetRenderFunc(
                        static (
                            SurfacePassData data,
                            RasterGraphContext context) =>
                        {
                            context.cmd.ClearRenderTarget(
                                true,
                                true,
                                Color.clear,
                                data.DepthClearValue);

                            context.cmd.DrawRendererList(
                                data.RendererList);
                        });
                }
            }

            private void AddCompositePass(
                RenderGraph renderGraph,
                TextureHandle source,
                TextureHandle destination)
            {
                using (var builder =
                    renderGraph.AddRasterRenderPass<BlitPassData>(
                        "Moyva Fog Final Composite",
                        out BlitPassData passData,
                        _compositeSampler))
                {
                    passData.Source = source;
                    passData.Material =
                        _screenSpaceMaterial;
                    passData.PassIndex =
                        CompositePass;

                    builder.UseTexture(
                        source,
                        AccessFlags.Read);

                    builder.UseGlobalTexture(
                        SurfaceEyeDepthTextureId,
                        AccessFlags.Read);

                    builder.UseGlobalTexture(
                        RawStateTextureId,
                        AccessFlags.Read);

                    builder.SetRenderAttachment(
                        destination,
                        0);

                    builder.AllowPassCulling(
                        false);

                    builder.SetRenderFunc(
                        static (
                            BlitPassData data,
                            RasterGraphContext context) =>
                        {
                            Blitter.BlitTexture(
                                context.cmd,
                                data.Source,
                                new Vector4(
                                    1f,
                                    1f,
                                    0f,
                                    0f),
                                data.Material,
                                data.PassIndex);
                        });
                }
            }

            private static void AddBlitPass(
                RenderGraph renderGraph,
                string passName,
                TextureHandle source,
                TextureHandle destination,
                Material material,
                int materialPass,
                ProfilingSampler sampler,
                int globalTextureAfterPass,
                int secondaryGlobalTextureAfterPass = 0)
            {
                using (var builder =
                    renderGraph.AddRasterRenderPass<BlitPassData>(
                        passName,
                        out BlitPassData passData,
                        sampler))
                {
                    passData.Source = source;
                    passData.Material = material;
                    passData.PassIndex =
                        materialPass;

                    builder.UseTexture(
                        source,
                        AccessFlags.Read);

                    builder.SetRenderAttachment(
                        destination,
                        0);

                    if (globalTextureAfterPass != 0)
                    {
                        builder.SetGlobalTextureAfterPass(
                            destination,
                            globalTextureAfterPass);
                    }

                    if (secondaryGlobalTextureAfterPass != 0)
                    {
                        builder.SetGlobalTextureAfterPass(
                            destination,
                            secondaryGlobalTextureAfterPass);
                    }

                    builder.AllowPassCulling(
                        false);

                    builder.SetRenderFunc(
                        static (
                            BlitPassData data,
                            RasterGraphContext context) =>
                        {
                            Blitter.BlitTexture(
                                context.cmd,
                                data.Source,
                                new Vector4(
                                    1f,
                                    1f,
                                    0f,
                                    0f),
                                data.Material,
                                data.PassIndex);
                        });
                }
            }

            private static TextureHandle CreateSurfaceEyeDepthTexture(
                RenderGraph renderGraph,
                RenderTextureDescriptor cameraDescriptor)
            {
                RenderTextureDescriptor descriptor =
                    cameraDescriptor;

                descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = 0;
                descriptor.depthStencilFormat =
                    GraphicsFormat.None;
                descriptor.graphicsFormat =
                    GraphicsFormat.R32_SFloat;

                return UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    descriptor,
                    "Moyva Fog Surface Eye Depth",
                    false);
            }

            private static TextureHandle CreateSurfaceDepthAttachment(
                RenderGraph renderGraph,
                RenderTextureDescriptor cameraDescriptor)
            {
                RenderTextureDescriptor descriptor =
                    cameraDescriptor;

                descriptor.msaaSamples = 1;
                descriptor.graphicsFormat =
                    GraphicsFormat.None;
                descriptor.depthBufferBits = 32;
                descriptor.depthStencilFormat =
                    GraphicsFormat.D32_SFloat;

                return UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    descriptor,
                    "Moyva Fog Surface Depth Attachment",
                    false);
            }

            private sealed class SurfacePassData
            {
                public RendererListHandle RendererList;
                public float DepthClearValue;
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
