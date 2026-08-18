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
    /// 3. Dilate + Erode — лише діагностичний closed-state preview.
    /// 4. Composite — point-stable fog mask, surface-locked
    ///    grid edge, main grey unexplored fog.
    /// 5. CopyBack — повертає результат у camera color.
    ///
    /// Legacy world curtain у цьому режимі не використовується.
    /// </summary>
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
        private bool _logPipelineDiagnostics = true;

        private Material _screenSpaceMaterial;
        private Material _surfaceDepthMaterial;
        private FogScreenSpacePass _pass;
        private bool _loggedReady;
        private bool _loggedBackBufferWarning;
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

            _loggedReady = false;
            _loggedBackBufferWarning = false;
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

            if (!_applyInSceneView
                && camera != null
                && camera.cameraType
                    == CameraType.SceneView)
            {
                return;
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
                effectiveStateScale,
                HandleBackBufferWarning);

            renderer.EnqueuePass(
                _pass);

            if (_logPipelineDiagnostics
                && !_loggedReady)
            {
                _loggedReady = true;

                Debug.Log(
                    "[MOYVA_FOG_PRESENTATION] " +
                    "pipeline=DepthAwareScreenSpace " +
                    "surfaceDepth=DedicatedOverridePass " +
                    "stateMorphology=DebugOnly " +
                    "edgeMode=SurfaceLockedGridEdge " +
                    "cornerJoin=RadialSoftUnion " +
                    "diagonalCornerCaps=True " +
                    "edgeSide=UnexploredCellOnly " +
                    "cameraExtrusion=False " +
                    "screenNeighbourSampling=False " +
                    "fallbackPlane=False " +
                    "depthOcclusion=False " +
                    "legacyCurtain=False " +
                    "stateScale=" +
                    (
                        _forceFullResolutionState
                            ? 1f
                            : Mathf.Clamp(
                                _screenStateScale,
                                0.5f,
                                1f)
                    ).ToString("F2") +
                    " zoomStable=True " +
                    "depthSource=ImmutableSurfaceEyeDepth " +
                    "maskDepthDecoupled=True " +
                    "presentationSafety=SurfaceLocked " +
                    "finalMask=RawPointState " +
                    "finalEvent=" +
                    (int)FogRenderPassEvent +
                    " layerMask=" +
                    _fogSurfaceLayerMask.value);
            }
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

#if UNITY_EDITOR
            if (_screenSpaceShader != null
                || _surfaceDepthShader != null)
            {
                EditorUtility.SetDirty(this);
            }
#endif
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
            if (!_logPipelineDiagnostics
                || _loggedUnavailable)
            {
                return;
            }

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

        private void HandleBackBufferWarning()
        {
            if (!_logPipelineDiagnostics
                || _loggedBackBufferWarning)
            {
                return;
            }

            _loggedBackBufferWarning = true;

            Debug.LogWarning(
                "[MOYVA_FOG_PRESENTATION] " +
                "activeColor is BackBuffer and cannot be sampled. " +
                "The feature requests Color input, but the renderer " +
                "still did not create an intermediate color texture. " +
                "Set Intermediate Texture to Auto/Always in URP Renderer.");
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
            private const int DilateStatePass = 1;
            private const int ErodeStatePass = 2;
            private const int CompositePass = 3;
            private const int CopyBackPass = 4;

            private static readonly int SurfaceEyeDepthTextureId =
                Shader.PropertyToID(
                    "_MoyvaFogSurfaceEyeDepthTexture");

            private static readonly int RawStateTextureId =
                Shader.PropertyToID(
                    "_MoyvaFogScreenStateRawTexture");

            private static readonly int ClosedStateTextureId =
                Shader.PropertyToID(
                    "_MoyvaFogScreenStateTexture");

            private static readonly int StateTexelSizeId =
                Shader.PropertyToID(
                    "_MoyvaFogScreenStateTexelSize");

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

            private readonly ProfilingSampler _dilateSampler =
                new ProfilingSampler(
                    "Moyva Fog State Dilate");

            private readonly ProfilingSampler _erodeSampler =
                new ProfilingSampler(
                    "Moyva Fog State Erode");

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
            private System.Action _onBackBuffer;

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
                float stateScale,
                System.Action onBackBuffer)
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

                _onBackBuffer =
                    onBackBuffer;
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
                    _onBackBuffer?.Invoke();
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

                _screenSpaceMaterial.SetVector(
                    StateTexelSizeId,
                    new Vector4(
                        1f / Mathf.Max(
                            1,
                            stateDescriptor.width),
                        1f / Mathf.Max(
                            1,
                            stateDescriptor.height),
                        stateDescriptor.width,
                        stateDescriptor.height));

                TextureHandle rawState =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        stateDescriptor,
                        "Moyva Fog Raw Screen State",
                        false);

                TextureHandle dilatedState =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        stateDescriptor,
                        "Moyva Fog Dilated Screen State",
                        false);

                TextureHandle closedState =
                    UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        stateDescriptor,
                        "Moyva Fog Closed Screen State",
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

                AddBlitPass(
                    renderGraph,
                    "Moyva Fog Dilate State",
                    rawState,
                    dilatedState,
                    _screenSpaceMaterial,
                    DilateStatePass,
                    _dilateSampler,
                    0);

                AddBlitPass(
                    renderGraph,
                    "Moyva Fog Erode State",
                    dilatedState,
                    closedState,
                    _screenSpaceMaterial,
                    ErodeStatePass,
                    _erodeSampler,
                    ClosedStateTextureId);

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

                    builder.UseGlobalTexture(
                        ClosedStateTextureId,
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
                int globalTextureAfterPass)
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
