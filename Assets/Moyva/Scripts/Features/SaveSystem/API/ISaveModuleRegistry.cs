using System.Collections.Generic;

namespace Kruty1918.Moyva.SaveSystem
{
    public interface ISaveModuleRegistry
    {
        void Register(ISaveModule module);
        void Unregister(ISaveModule module);
        void AppendRegisteredModules(List<ISaveModule> target);
    }
}