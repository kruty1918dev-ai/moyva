using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.GraphSystem.API
{
[System.Serializable]
public sealed partial class GraphAsset : MoyvaJsonConfigObject
    {
        [HideInInspector, SerializeField] private List<NodeBase> _nodes = new();
        [HideInInspector, SerializeField] private List<Connection> _connections = new();
        [HideInInspector, SerializeField] private int _version = 2;

        [Sirenix.OdinInspector.TitleGroup("Graph Settings")]
        [Sirenix.OdinInspector.InlineProperty]
        [SerializeField] private GraphSharedSettings _sharedSettings = new();
        [HideInInspector, SerializeField] private LayerGraphState _globalGraphState = new(string.Empty);

        [Header("Layers")]
        [Tooltip("Шари генератора. Кожен шар має власний підграф і компілюється в blueprint-шар TileWorldCreator.")]
        [Sirenix.OdinInspector.TitleGroup("Layers")]
        [Sirenix.OdinInspector.TableList(AlwaysExpanded = true, DrawScrollView = false)]
        [SerializeField] private List<GeneratorLayerDefinition> _layers = new();
        [HideInInspector, SerializeField] private List<LayerGraphState> _layerGraphStates = new();

        [Header("Tile Registry")]
        [Tooltip("Реєстр тайлів цього графа.")]
        [Sirenix.OdinInspector.TitleGroup("Tile Registry")]
        [SerializeField] private TileRegistrySO _tileRegistry;

        public IReadOnlyList<NodeBase> Nodes => _nodes;
        public IReadOnlyList<Connection> Connections => _connections;
        public int Version => _version;
        public IReadOnlyList<GeneratorLayerDefinition> Layers => _layers;
        public IReadOnlyList<LayerGraphState> LayerGraphStates => _layerGraphStates;
        public LayerGraphState GlobalGraphState => _globalGraphState ??= new LayerGraphState(string.Empty);

        /// <summary>
        /// Спільні налаштування графа (розмір мапи тощо).
        /// Змінюються прямо в інспекторі GraphAsset — без потреби в окремому ноді.
        /// </summary>
        public GraphSharedSettings SharedSettings => _sharedSettings ??= new GraphSharedSettings();
        public TileRegistrySO TileRegistry => _tileRegistry;

        public NodeBase GetNodeById(string nodeId)
        {
            EnsureLayerGraphStates();
            for (int i = 0; i < _nodes.Count; i++)
                if (_nodes[i] != null && _nodes[i].NodeId == nodeId)
                    return _nodes[i];
            return null;
        }

        public GeneratorLayerDefinition GetLayerById(string layerId)
        {
            if (string.IsNullOrEmpty(layerId))
                return null;
            for (int i = 0; i < _layers.Count; i++)
                if (_layers[i] != null && _layers[i].Id == layerId)
                    return _layers[i];
            return null;
        }

        /// <summary>
        /// Повертає вузли, що належать вказаному шару. Якщо <paramref name="layerId"/>
        /// порожній — повертає вузли без призначеного шару (глобальні).
        /// </summary>
        public List<NodeBase> GetNodesForLayer(string layerId)
        {
            EnsureLayerGraphStates();
            var result = new List<NodeBase>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node == null) continue;

                if (string.IsNullOrEmpty(layerId))
                {
                    if (string.IsNullOrEmpty(node.LayerId))
                        result.Add(node);
                }
                else if (node.LayerId == layerId)
                {
                    result.Add(node);
                }
            }
            return result;
        }

        public LayerGraphState GetLayerGraphState(string layerId)
        {
            EnsureLayerGraphStates();

            if (string.IsNullOrEmpty(layerId))
                return GlobalGraphState;

            for (int i = 0; i < _layerGraphStates.Count; i++)
            {
                var state = _layerGraphStates[i];
                if (state != null && state.LayerId == layerId)
                    return state;
            }

            return null;
        }

        public List<Connection> GetConnectionsForLayer(string layerId, bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            var result = new List<Connection>();
            var state = GetLayerGraphState(layerId);
            AddConnectionsFromState(state, result);

            if (includeGlobal && !string.IsNullOrEmpty(layerId))
                AddConnectionsFromState(GlobalGraphState, result);

            return result;
        }

        public GraphExecutionScope CreateExecutionScope(string layerId, bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            if (string.IsNullOrEmpty(layerId))
            {
                return new GraphExecutionScope(
                    this,
                    null,
                    GlobalGraphState.GraphId,
                    new List<NodeBase>(_nodes),
                    new List<Connection>(_connections));
            }

            var state = GetLayerGraphState(layerId);
            var nodes = new List<NodeBase>();
            var connections = new List<Connection>();

            if (includeGlobal)
                AddNodesFromState(GlobalGraphState, nodes);
            AddNodesFromState(state, nodes);

            if (includeGlobal)
                AddConnectionsFromState(GlobalGraphState, connections);
            AddConnectionsFromState(state, connections);

            return new GraphExecutionScope(
                this,
                layerId,
                state?.GraphId,
                nodes,
                connections);
        }

        public List<GraphExecutionScope> CreateEnabledLayerExecutionScopes(bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            var result = new List<GraphExecutionScope>();
            var orderedLayers = _layers
                .Where(layer => layer != null && layer.Enabled)
                .OrderBy(layer => layer.SortingOrder)
                .ThenBy(layer => layer.Name, StringComparer.Ordinal)
                .ToList();

            for (int i = 0; i < orderedLayers.Count; i++)
            {
                var layer = orderedLayers[i];
                result.Add(CreateExecutionScope(layer.Id, includeGlobal));
            }

            return result;
        }

    }
}
