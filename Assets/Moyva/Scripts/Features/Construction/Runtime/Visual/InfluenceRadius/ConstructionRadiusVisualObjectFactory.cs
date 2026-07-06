using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionRadiusVisualObjectFactory : IConstructionRadiusVisualObjectFactory
    {
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;

        [Inject]
        public ConstructionRadiusVisualObjectFactory(
            IConstructionVisualRootService roots,
            IGridService gridService,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _gridService = gridService;
            _gridProjection = gridProjection;
            _settingsProvider = settingsProvider;
        }

        public ConstructionRadiusVisualHandle Create(string name, int sortingOffset, Mesh mesh)
        {
            Transform existing = _roots.RadiusRoot.Find(name);
            if (existing != null)
                Object.Destroy(existing.gameObject);

            GameObject go = new GameObject(name);
            go.transform.SetParent(_roots.RadiusRoot, false);
            MeshRenderer renderer = CreateRenderer(go, sortingOffset);

            if (!Uses3DWorldPlane())
                go.AddComponent<MeshFilter>().sharedMesh = mesh;

            Material material = CreateMaterial(name);
            renderer.sharedMaterial = material;
            renderer.enabled = false;
            return new ConstructionRadiusVisualHandle(go, renderer, material);
        }

        private MeshRenderer CreateRenderer(GameObject go, int sortingOffset)
        {
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = ResolveSortingOrder() + sortingOffset;
            return renderer;
        }

        private Material CreateMaterial(string objectName)
        {
            string shaderName = ResolveShaderName();
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionVisual] Shader '{shaderName}' not found. Influence radius overlay is disabled.");
                return null;
            }

            Material material = new Material(shader) { name = $"{objectName}_Material" };
            material.SetVector("_MapRect", new Vector4(-0.5f, -0.5f, _gridService.GridWidth - 0.5f, _gridService.GridHeight - 0.5f));
            return material;
        }

        private bool Uses3DWorldPlane() => GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection);
        private int ResolveSortingOrder() => _settingsProvider?.BuildingLayerMinSortingOrder ?? 5;
        private string ResolveShaderName() => Uses3DWorldPlane()
            ? _settingsProvider?.InfluenceRadiusShaderName3D ?? "Moyva/3D/InfluenceRadiusExistingMeshOverlay"
            : _settingsProvider?.InfluenceRadiusShaderName2D ?? "Moyva/2D/InfluenceRadius";
    }
}
