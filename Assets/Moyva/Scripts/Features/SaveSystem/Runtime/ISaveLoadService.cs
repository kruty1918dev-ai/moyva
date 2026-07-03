using System.Collections.Generic;
using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.SaveSystem
{
    internal interface ISaveLoadService
    {
        bool TryLoad(
            int slot,
            IReadOnlyList<ISaveModule> modules,
            string requiredBlockModuleFullName,
            IDiagnosticFlow flow,
            out string errorMessage);
    }
}
