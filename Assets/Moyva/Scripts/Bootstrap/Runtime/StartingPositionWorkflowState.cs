using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionWorkflowState
    {
        bool StartLogicApplied { get; set; }
        bool StartRevealApplied { get; set; }
        bool StartupCameraTeleported { get; set; }
        Vector2Int AppliedStartRevealCenter { get; set; }
        int AppliedStartRevealWidth { get; set; }
        int AppliedStartRevealHeight { get; set; }
        bool HasPendingWorldGeneratedSignal { get; set; }
        WorldGeneratedDataSignal PendingWorldGeneratedSignal { get; set; }
    }

    internal sealed class StartingPositionWorkflowState
        : IStartingPositionWorkflowState
    {
        public bool StartLogicApplied { get; set; }
        public bool StartRevealApplied { get; set; }
        public bool StartupCameraTeleported { get; set; }
        public Vector2Int AppliedStartRevealCenter { get; set; }
        public int AppliedStartRevealWidth { get; set; }
        public int AppliedStartRevealHeight { get; set; }
        public bool HasPendingWorldGeneratedSignal { get; set; }
        public WorldGeneratedDataSignal PendingWorldGeneratedSignal { get; set; }
    }
}
