using Kruty1918.Moyva.Diagnostics.API;
using UnityEngine;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    internal sealed class UnityDiagnosticClock : IDiagnosticClock
    {
        public double NowMilliseconds => Time.realtimeSinceStartupAsDouble * 1000d;
    }
}
