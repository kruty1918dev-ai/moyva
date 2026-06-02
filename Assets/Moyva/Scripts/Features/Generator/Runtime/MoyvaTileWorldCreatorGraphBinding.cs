using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Місток між GraphAsset та TileWorldCreator: граф є джерелом правди,
    /// а TWC лише виконує згенеровані інструкції. Логіка розділена на partial-файли:
    /// ядро (цей файл), компіляція графа та превʼю окремих шарів.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileWorldCreatorManager))]
    [AddComponentMenu("Moyva/Generator/Moyva TWC Graph Binding")]
    public sealed partial class MoyvaTileWorldCreatorGraphBinding : MonoBehaviour
    {
        [SerializeField] private TileWorldCreatorManager _manager;
        [SerializeField] private GraphAsset _graphAsset;
        [SerializeField] private int _editorSeed = 1;
        [SerializeField] private bool _compileBeforeGenerate = true;
        [SerializeField] private bool _generateBuildLayersAfterCompile = true;

        public TileWorldCreatorManager Manager
        {
            get
            {
                if (_manager == null)
                    _manager = GetComponent<TileWorldCreatorManager>();
                return _manager;
            }
        }

        public GraphAsset GraphAsset => _graphAsset;
        public int EditorSeed => NormalizeSeed(_editorSeed);
        public bool CompileBeforeGenerate => _compileBeforeGenerate;
        public bool GenerateBuildLayersAfterCompile => _generateBuildLayersAfterCompile;
        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; } = Array.Empty<CompiledLayerMap>();

        public void SetGraphAsset(GraphAsset graphAsset)
        {
            _graphAsset = graphAsset;
        }

        public void SetEditorSeed(int seed)
        {
            _editorSeed = NormalizeSeed(seed);
        }

        public void SetCompileBeforeGenerate(bool value)
        {
            _compileBeforeGenerate = value;
        }

        public void SetGenerateBuildLayersAfterCompile(bool value)
        {
            _generateBuildLayersAfterCompile = value;
        }

        public bool CanCompile(out string reason)
        {
            if (Manager == null)
            {
                reason = "TileWorldCreatorManager відсутній.";
                return false;
            }

            if (Manager.configuration == null)
            {
                reason = "TWC Configuration не задано.";
                return false;
            }

            if (_graphAsset == null)
            {
                reason = "GraphAsset не задано.";
                return false;
            }

            reason = null;
            return true;
        }

        private void Reset()
        {
            _manager = GetComponent<TileWorldCreatorManager>();
            _editorSeed = NormalizeSeed(_editorSeed);
        }

        private void OnValidate()
        {
            if (_manager == null)
                _manager = GetComponent<TileWorldCreatorManager>();
            _editorSeed = NormalizeSeed(_editorSeed);
        }

        private static int NormalizeSeed(int seed)
        {
            return seed == 0 ? 1 : seed;
        }
    }
}
