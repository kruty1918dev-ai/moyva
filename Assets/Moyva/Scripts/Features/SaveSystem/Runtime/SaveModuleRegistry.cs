using System.Collections.Generic;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveModuleRegistry : ISaveModuleRegistry
    {
        private readonly List<ISaveModule> _modules = new List<ISaveModule>();

        public void Register(ISaveModule module)
        {
            if (module == null || _modules.Contains(module))
                return;

            _modules.Add(module);
        }

        public void Unregister(ISaveModule module)
        {
            if (module == null)
                return;

            _modules.Remove(module);
        }

        public void AppendRegisteredModules(List<ISaveModule> target)
        {
            if (target == null)
                return;

            for (int index = 0; index < _modules.Count; index++)
            {
                var module = _modules[index];
                if (module == null || target.Contains(module))
                    continue;

                target.Add(module);
            }
        }
    }
}