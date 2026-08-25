using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Registers the live scene construction service with an optional
    /// project-scoped authority endpoint and removes it when the scene exits.
    /// </summary>
    internal sealed class ConstructionAuthorityEndpointRegistration :
        IInitializable,
        IDisposable
    {
        private readonly IConstructionService _constructionService;
        private readonly IConstructionAuthorityEndpointRegistry _registry;
        private bool _attached;

        public ConstructionAuthorityEndpointRegistration(
            IConstructionService constructionService,
            [InjectOptional]
            IConstructionAuthorityEndpointRegistry registry = null)
        {
            _constructionService = constructionService;
            _registry = registry;
        }

        public void Initialize()
        {
            if (_attached || _registry == null)
                return;

            _registry.Attach(_constructionService);
            _attached = true;
        }

        public void Dispose()
        {
            if (!_attached)
                return;

            _registry.Detach(_constructionService);
            _attached = false;
        }
    }

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

        }
    }
}
