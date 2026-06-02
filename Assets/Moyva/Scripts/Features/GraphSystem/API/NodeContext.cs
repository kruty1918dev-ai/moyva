using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class NodeContext
    {
        public int Seed { get; }
        public CancellationToken Cancellation { get; }
        public IProgress<float> Progress { get; }
        public Vector2Int MapSize { get; set; }
        public GridTopology GridTopology { get; private set; } = GridTopology.Orthogonal;
        public GridProjectionMode ProjectionMode { get; private set; } = GridProjectionMode.Orthographic3D;
        public GridRenderMode RenderMode { get; private set; } = GridRenderMode.Mesh3D;
        public GridNeighborhoodMode NeighborhoodMode { get; private set; } = GridNeighborhoodMode.Moore8;

        private readonly Dictionary<Type, object> _services = new();
        private long _nodeIterations;

        public NodeContext(int seed, CancellationToken cancellation = default,
            IProgress<float> progress = null)
        {
            Seed = seed;
            GlobalSeed.Set(seed);
            Cancellation = cancellation;
            Progress = progress;
        }

        public void RegisterService<T>(T service) =>
            _services[typeof(T)] = service;

        public void ApplySharedSettings(GraphSharedSettings settings)
        {
            if (settings == null)
                return;

            GridTopology = settings.GridTopology;
            ProjectionMode = settings.ProjectionMode;
            RenderMode = settings.RenderMode;
            NeighborhoodMode = settings.ResolveNeighborhoodMode();
        }

        public void ApplyProjectSettings(MoyvaProjectSettingsSO settings)
        {
            if (settings == null)
                return;

            if (!settings.UseAdvancedProjectSettings)
            {
                if (settings.ResolveProjectVisualMode() == MoyvaProjectVisualMode.Full3D)
                {
                    GridTopology = GridTopology.Layered;
                    ProjectionMode = GridProjectionMode.Orthographic3D;
                    RenderMode = GridRenderMode.Mesh3D;
                    NeighborhoodMode = ResolveNeighborhoodMode(GridTopology, ProjectionMode, GridNeighborhoodMode.Auto);
                    return;
                }

                GridTopology = GridTopology.Orthogonal;
                ProjectionMode = GridProjectionMode.Orthographic3D;
                RenderMode = GridRenderMode.Mesh3D;
                NeighborhoodMode = ResolveNeighborhoodMode(GridTopology, ProjectionMode, GridNeighborhoodMode.Auto);
                return;
            }

            GridTopology = settings.DefaultGridTopology;
            ProjectionMode = settings.DefaultProjectionMode;
            RenderMode = settings.DefaultRenderMode;
            NeighborhoodMode = ResolveNeighborhoodMode(GridTopology, ProjectionMode, settings.DefaultNeighborhood);
        }

        private static GridNeighborhoodMode ResolveNeighborhoodMode(
            GridTopology topology,
            GridProjectionMode projectionMode,
            GridNeighborhoodMode neighborhoodMode)
        {
            if (neighborhoodMode != GridNeighborhoodMode.Auto)
                return neighborhoodMode;

            if (topology == GridTopology.HexAxial)
                return GridNeighborhoodMode.HexAxial6;

            return projectionMode == GridProjectionMode.Isometric3DPreview
                ? GridNeighborhoodMode.VonNeumann4
                : GridNeighborhoodMode.Moore8;
        }

        public T GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException(
                $"Service {typeof(T).Name} not registered in NodeContext.");
        }

        public bool TryGetService<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var raw))
            {
                service = (T)raw;
                return true;
            }
            service = default;
            return false;
        }

        public void CountIteration(int amount = 1)
        {
            if (amount <= 0) return;
            _nodeIterations += amount;
        }

        public void ResetNodeProfiling()
        {
            _nodeIterations = 0;
        }

        public long ConsumeNodeIterations()
        {
            long value = _nodeIterations;
            _nodeIterations = 0;
            return value;
        }

        public void CopyServicesTo(NodeContext other)
        {
            other.GridTopology = GridTopology;
            other.ProjectionMode = ProjectionMode;
            other.RenderMode = RenderMode;
            other.NeighborhoodMode = NeighborhoodMode;

            foreach (var kv in _services)
                other._services[kv.Key] = kv.Value;
        }

        public System.Random CreateRandom() => new(Seed);
        public System.Random CreateRandom(int salt) => new(GlobalSeed.Combine(Seed, salt));
        public System.Random CreateRandom(string salt) => new(GlobalSeed.Combine(Seed, GlobalSeed.StableHash(salt)));
    }
}
