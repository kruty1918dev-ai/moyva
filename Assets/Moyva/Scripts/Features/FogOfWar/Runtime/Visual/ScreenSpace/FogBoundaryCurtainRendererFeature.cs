using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Legacy world-space boundary curtain. У новому режимі
    /// DepthAwareScreenSpace дані не реєструються і pass не enqueue-иться.
    ///
    /// Curtain навмисно НЕ має MeshRenderer:
    /// інші Renderer Features, зокрема Flat Kit Outline,
    /// більше не можуть повторно намалювати його override-матеріалом.
    ///
    /// Порядок:
    /// opaque scene -> transparent water/effects
    /// -> direct curtain -> main grey fullscreen fog.
    /// </summary>
    public sealed class FogBoundaryCurtainRendererFeature
        : ScriptableRendererFeature
    {
        private static readonly RenderPassEvent
            CurtainRenderPassEvent =
                (RenderPassEvent)(
                    (int)RenderPassEvent
                        .AfterRenderingTransparents
                    + 20);

        private static Mesh _registeredMesh;
        private static Material _registeredMaterial;

        private static Matrix4x4 _registeredLocalToWorld =
            Matrix4x4.identity;

        private static bool _registeredVisible;

        [SerializeField]
        private bool _applyInSceneView = true;

        private CurtainPass _pass;

        internal static void SetDrawData(
            Mesh mesh,
            Material material,
            Matrix4x4 localToWorld,
            bool visible)
        {
            _registeredMesh = mesh;
            _registeredMaterial = material;
            _registeredLocalToWorld = localToWorld;
            _registeredVisible =
                visible
                && mesh != null
                && material != null;
        }

        internal static void ClearDrawData(
            Mesh expectedMesh)
        {
            if (expectedMesh != null
                && _registeredMesh != expectedMesh)
            {
                return;
            }

            _registeredMesh = null;
            _registeredMaterial = null;
            _registeredLocalToWorld =
                Matrix4x4.identity;
            _registeredVisible = false;
        }

        private static bool TryGetDrawData(
            out Mesh mesh,
            out Material material,
            out Matrix4x4 localToWorld)
        {
            mesh = _registeredMesh;
            material = _registeredMaterial;
            localToWorld =
                _registeredLocalToWorld;

            return _registeredVisible
                   && mesh != null
                   && material != null
                   && mesh.vertexCount > 0
                   && material.passCount > 0;
        }

        public override void Create()
        {
            _pass =
                new CurtainPass
                {
                    renderPassEvent =
                        CurtainRenderPassEvent
                };
        }

        private void OnValidate()
        {
            if (_pass != null)
            {
                _pass.renderPassEvent =
                    CurtainRenderPassEvent;
            }
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
                _pass = null;
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (_pass == null)
                return;

            CameraData cameraData =
                renderingData.cameraData;

            if (cameraData.isPreviewCamera)
                return;

            if (cameraData.renderType
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

            if (!TryGetDrawData(
                    out Mesh mesh,
                    out Material material,
                    out Matrix4x4 localToWorld))
            {
                return;
            }

            _pass.Setup(
                mesh,
                material,
                localToWorld);

            renderer.EnqueuePass(
                _pass);
        }

        private sealed class CurtainPass
            : ScriptableRenderPass
        {
            private Mesh _mesh;
            private Material _material;

            private Matrix4x4 _localToWorld =
                Matrix4x4.identity;

            public CurtainPass()
            {
                profilingSampler =
                    new ProfilingSampler(
                        "Moyva Fog Boundary Curtain");

            }

            public void Setup(
                Mesh mesh,
                Material material,
                Matrix4x4 localToWorld)
            {
                _mesh = mesh;
                _material = material;
                _localToWorld = localToWorld;
            }

#if URP_COMPATIBILITY_MODE
            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                if (_mesh == null
                    || _material == null
                    || _mesh.vertexCount <= 0)
                {
                    return;
                }

                CommandBuffer command =
                    CommandBufferPool.Get(
                        "Moyva Fog Boundary Curtain");

                using (new ProfilingScope(
                           command,
                           profilingSampler))
                {
                    command.DrawMesh(
                        _mesh,
                        _localToWorld,
                        _material,
                        0,
                        0);
                }

                context.ExecuteCommandBuffer(
                    command);

                CommandBufferPool.Release(
                    command);
            }
#endif

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                if (_mesh == null
                    || _material == null
                    || _mesh.vertexCount <= 0)
                {
                    return;
                }

                using (var builder =
                    renderGraph.AddRasterRenderPass<
                        PassData>(
                        "Moyva Fog Boundary Curtain",
                        out PassData passData,
                        profilingSampler))
                {
                    UniversalResourceData resourceData =
                        frameData.Get<
                            UniversalResourceData>();

                    passData.Mesh = _mesh;
                    passData.Material = _material;
                    passData.LocalToWorld =
                        _localToWorld;

                    builder.SetRenderAttachment(
                        resourceData.activeColorTexture,
                        0);

                    /*
                     * Curtain є прямим overlay-pass.
                     * Scene depth навмисно не читається і не записується.
                     * Коректна висота визначається під час побудови mesh.
                     */

                    builder.AllowPassCulling(
                        false);

                    builder.SetRenderFunc(
                        static (
                            PassData data,
                            RasterGraphContext context) =>
                        {
                            if (data.Mesh == null
                                || data.Material == null)
                            {
                                return;
                            }

                            context.cmd.DrawMesh(
                                data.Mesh,
                                data.LocalToWorld,
                                data.Material,
                                0,
                                0);
                        });
                }
            }

            private sealed class PassData
            {
                public Mesh Mesh;
                public Material Material;

                public Matrix4x4 LocalToWorld =
                    Matrix4x4.identity;
            }
        }
    }
}
