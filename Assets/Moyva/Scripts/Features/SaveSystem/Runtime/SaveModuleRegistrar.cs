using System;
using Zenject;

namespace Kruty1918.Moyva.SaveSystem
{
    public sealed class SaveModuleRegistrar<TModule> : IInitializable, IDisposable
        where TModule : ISaveModule
    {
        private readonly ISaveModuleRegistry _registry;
        private readonly TModule _module;

        public SaveModuleRegistrar(ISaveModuleRegistry registry, TModule module)
        {
            _registry = registry;
            _module = module;
        }

        public void Initialize()
        {
            _registry.Register(_module);
        }

        public void Dispose()
        {
            _registry.Unregister(_module);
        }
    }
}