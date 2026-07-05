using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionSceneSettingsProvider
    {
        ConstructionSystemProfileSO SystemProfile { get; }
        Transform ResolvePreviewRoot();
        Transform ResolvePlacedRoot();
        Transform ResolveRadiusRoot();
        Transform ResolveUiRoot();
        Transform ResolveDebugRoot();
        void EnsureSceneRoots();
    }
}
