using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(WaterContourGeneratorNode))]
    public sealed class WaterContourGeneratorNodeEditor : ContourGeneratorNodeEditorBase
    {
        protected override string TileArrayPropertyName => "_contourTiles";
        protected override string CenterCellLabel       => "shore\ntarget";
    }
}
