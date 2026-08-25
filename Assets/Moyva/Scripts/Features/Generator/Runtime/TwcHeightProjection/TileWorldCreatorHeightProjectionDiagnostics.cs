using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionDiagnostics : ITileWorldCreatorHeightProjectionDiagnostics
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        public void LogConfigured(TileWorldCreatorHeightProjectionState state)
        {
            var root = state.TargetRoot;
        }

        public void LogWorldStart(TileWorldCreatorHeightProjectionState state)
        {
        }

        public void LogWorldEnd(TileWorldCreatorHeightProjectionState state)
        {
        }

        public void LogPass(string message)
        {
        }

        public string FormatSamples(System.Collections.Generic.List<string> samples)
        {
            return TileWorldCreatorHeightProjectionUtility.FormatSamples(samples);
        }
    }
}
