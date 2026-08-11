#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Kruty1918.Moyva.FogOfWar.API;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    [CustomEditor(typeof(FogOfWarSettings))]
    public sealed class FogOfWarSettingsEditor : OdinEditor
    {
    }
}

#endif
