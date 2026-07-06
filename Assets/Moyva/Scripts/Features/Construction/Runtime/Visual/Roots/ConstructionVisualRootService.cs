using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualRootService : IConstructionVisualRootService
    {
        private readonly IConstructionSceneSettingsProvider _sceneSettingsProvider;
        private readonly IConstructionVisualSettingsProvider _visualSettingsProvider;

        public Transform PreviewRoot { get; private set; }
        public Transform PlacedRoot { get; private set; }
        public Transform RadiusRoot { get; private set; }

        [Inject]
        public ConstructionVisualRootService(
            [InjectOptional] IConstructionSceneSettingsProvider sceneSettingsProvider = null,
            [InjectOptional] IConstructionVisualSettingsProvider visualSettingsProvider = null)
        {
            _sceneSettingsProvider = sceneSettingsProvider;
            _visualSettingsProvider = visualSettingsProvider;
        }

        public void EnsureRoots()
        {
            _sceneSettingsProvider?.EnsureSceneRoots();
            PreviewRoot = _sceneSettingsProvider?.ResolvePreviewRoot()
                ?? EnsureRoot(ResolvePreviewRootName());
            PlacedRoot = _sceneSettingsProvider?.ResolvePlacedRoot()
                ?? EnsureRoot(ResolvePlacedRootName());
            RadiusRoot = _sceneSettingsProvider?.ResolveRadiusRoot()
                ?? EnsureRoot(ResolveRadiusRootName());
        }

        private static Transform EnsureRoot(string rootName)
        {
            GameObject existing = GameObject.Find(rootName);
            if (existing != null)
                return existing.transform;

            return new GameObject(rootName).transform;
        }

        private string ResolvePreviewRootName()
            => _visualSettingsProvider?.PreviewRootName ?? "ConstructionPreviewRoot";

        private string ResolvePlacedRootName()
            => _visualSettingsProvider?.PlacedRootName ?? "PlayerBuildingsRoot";

        private string ResolveRadiusRootName()
            => _visualSettingsProvider?.RadiusRootName ?? "ConstructionRadiusRoot";
    }
}
