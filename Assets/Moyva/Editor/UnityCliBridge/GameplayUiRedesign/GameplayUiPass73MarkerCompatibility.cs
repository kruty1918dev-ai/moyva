using UnityEngine;

#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    // The legacy authoring implementation already declares GameplayUiPass73Marker.
    // Keep this file inert when that compatibility symbol is explicitly enabled.
    internal static class GameplayUiPass73MarkerCompatibility
    {
    }
}
#else
namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    /// <summary>
    /// Serialization compatibility shim for the two Pass73 marker components that
    /// remain in Gamplay_Scene after the legacy editor bridge was compiled out by Pass82.
    /// The marker is editor-only and carries no gameplay behavior.
    /// </summary>
    internal sealed class GameplayUiPass73Marker : MonoBehaviour
    {
        [SerializeField] private string marker = "MOYVA_GAMEPLAY_UI_PASS73";
        public string Marker => marker;
    }
}
#endif
