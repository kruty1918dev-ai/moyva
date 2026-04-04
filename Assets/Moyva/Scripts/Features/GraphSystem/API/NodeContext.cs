using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class NodeContext
    {
        public int Seed { get; }
        public CancellationToken Cancellation { get; }
        public IProgress<float> Progress { get; }
        public Vector2Int MapSize { get; set; }

        private readonly Dictionary<Type, object> _services = new();

        public NodeContext(int seed, CancellationToken cancellation = default,
            IProgress<float> progress = null)
        {
            Seed = seed;
            Cancellation = cancellation;
            Progress = progress;
        }

        public void RegisterService<T>(T service) =>
            _services[typeof(T)] = service;

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

        public System.Random CreateRandom() => new(Seed);
        public System.Random CreateRandom(int salt) => new(Seed ^ salt);
    }
}
