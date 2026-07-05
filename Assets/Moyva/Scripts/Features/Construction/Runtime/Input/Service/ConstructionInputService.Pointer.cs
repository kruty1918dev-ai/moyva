using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private ConstructionPointerSnapshot ReadPointerSnapshot() => _pointerInputSource.ReadPointerSnapshot();
    }
}
