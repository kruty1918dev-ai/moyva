using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingContext
    {
        TileWorldCreatorManager Manager { get; }
        GraphAsset GraphAsset { get; }
        int EditorSeed { get; }
        bool CompileBeforeGenerate { get; }
        bool GenerateBuildLayersAfterCompile { get; }
        bool IsGenerating { get; }
        IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
        Object LogContext { get; }

        void SetLastCompiledLayers(IReadOnlyList<CompiledLayerMap> layers);
        void SetGenerating(bool value);
    }
}
