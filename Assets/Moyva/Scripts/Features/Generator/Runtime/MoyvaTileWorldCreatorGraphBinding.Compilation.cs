using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Компіляція графа у TileWorldCreator <see cref="Configuration"/> та повна генерація мапи.
    /// </summary>
    public sealed partial class MoyvaTileWorldCreatorGraphBinding
    {
        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration()
        {
            return CompileGraphToConfiguration(EditorSeed);
        }

        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(int seed)
        {
            if (!CanCompile(out string reason))
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] Неможливо скомпілювати граф: {reason}", this);
                LastCompiledLayers = Array.Empty<CompiledLayerMap>();
                return LastCompiledLayers;
            }

            int normalizedSeed = NormalizeSeed(seed);
            LastCompiledLayers = GraphToConfigurationCompiler.Compile(_graphAsset, Manager, normalizedSeed);
            return LastCompiledLayers;
        }

        public void GenerateFromGraph()
        {
            GenerateFromGraph(EditorSeed);
        }

        public void GenerateFromGraph(int seed)
        {
            if (_compileBeforeGenerate)
                CompileGraphToConfiguration(seed);

            if (Manager == null || Manager.configuration == null)
                return;

            if (_generateBuildLayersAfterCompile)
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(Manager);
            else
                Manager.ExecuteBlueprintLayers();
        }
    }
}
