using System.Collections.Generic;

namespace Kruty1918.Moyva.SaveSystem
{
    internal interface ISaveLoadService
    {
        bool TryLoad(int slot, IReadOnlyList<ISaveModule> modules, string requiredBlockModuleFullName, out string errorMessage);
    }
}
