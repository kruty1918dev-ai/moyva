using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionConfirmRequestRouter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly List<IConstructionConfirmRequestExecutor> _executors;

        public ConstructionConfirmRequestRouter(
            SignalBus signalBus,
            List<IConstructionConfirmRequestExecutor> executors)
        {
            _signalBus = signalBus;
            _executors = executors ?? new List<IConstructionConfirmRequestExecutor>();
        }

        public void Initialize()
        {
            _executors.Sort((left, right) => right.Priority.CompareTo(left.Priority));
            _signalBus.Subscribe<PlaceBuildingConfirmRequestSignal>(OnConfirmRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<PlaceBuildingConfirmRequestSignal>(OnConfirmRequested);
        }

        private void OnConfirmRequested(PlaceBuildingConfirmRequestSignal _)
        {
            for (int i = 0; i < _executors.Count; i++)
            {
                if (_executors[i] != null && _executors[i].TryHandleConfirmRequest())
                    return;
            }

            UnityEngine.Debug.LogWarning("[Construction] PlaceBuildingConfirmRequestSignal received, but no executor handled it.");
        }
    }
}
