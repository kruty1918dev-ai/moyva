using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
	/// <summary>
	/// Місток між GraphAsset та TileWorldCreator: граф є джерелом правди,
	/// а TWC лише виконує згенеровані інструкції. MonoBehaviour тримає
	/// serialized state, а роботу делегує сервісам GraphBinding.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TileWorldCreatorManager))]
	[AddComponentMenu("Moyva/Generator/Moyva TWC Graph Binding")]
	public sealed class MoyvaTileWorldCreatorGraphBinding : MonoBehaviour, IMoyvaTwcGraphBindingContext
	{
		[SerializeField] private TileWorldCreatorManager _manager;
		[SerializeField] private GraphAsset _graphAsset;
		[SerializeField] private int _editorSeed = 1;
		[SerializeField] private bool _compileBeforeGenerate = true;
		[SerializeField] private bool _generateBuildLayersAfterCompile = true;
		private bool _isGenerating;
		private IMoyvaTwcGraphBindingService _service;

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
		public bool IsGenerating => _isGenerating;

		[Inject]
		private void Construct([InjectOptional] IMoyvaTwcGraphBindingService service = null)
		{
			_service = service;
		}

		public void SetGraphAsset(GraphAsset graphAsset) => _graphAsset = graphAsset;

		public void SetEditorSeed(int seed) => _editorSeed = NormalizeSeed(seed);

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
			return ResolveService().CanCompile(this, out reason);
		}

		public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration()
		{
			return ResolveService().CompileGraphToConfiguration(this);
		}

		public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(int seed)
		{
			return ResolveService().CompileGraphToConfiguration(this, seed);
		}

		public void GenerateFromGraph()
		{
			ResolveService().GenerateFromGraph(this);
		}

		public void GenerateFromGraph(int seed)
		{
			ResolveService().GenerateFromGraph(this, seed);
		}

		public IReadOnlyList<string> GetGraphLayerNames()
		{
			return ResolveService().GetGraphLayerNames(this);
		}

		public void GenerateLayerPreview(string layerName)
		{
			ResolveService().GenerateLayerPreview(this, layerName);
		}

		public void GenerateLayerPreview(string layerName, int seed)
		{
			ResolveService().GenerateLayerPreview(this, layerName, seed);
		}

		public void ClearGeneratedMap()
		{
			ResolveService().ClearGeneratedMap(this);
		}

		void IMoyvaTwcGraphBindingContext.SetLastCompiledLayers(IReadOnlyList<CompiledLayerMap> layers)
		{
			LastCompiledLayers = layers ?? Array.Empty<CompiledLayerMap>();
		}

		void IMoyvaTwcGraphBindingContext.SetGenerating(bool value)
		{
			_isGenerating = value;
		}

		UnityEngine.Object IMoyvaTwcGraphBindingContext.LogContext => this;

		private IMoyvaTwcGraphBindingService ResolveService()
		{
			return _service ??= MoyvaTwcGraphBindingComposition.Create();
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
