using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapDiagnosticsService : IGraphLogicalTileMapDiagnosticsService
    {
        private readonly IGraphLogicalTileMapSnapshotFactory _snapshotFactory;
        private readonly IGraphLogicalTileMapReportFormatter _formatter;
        private GraphLogicalTileMapSnapshot _lastPreview;
        private GraphLogicalTileMapSnapshot _lastScene;

        public GraphLogicalTileMapDiagnosticsService(
            IGraphLogicalTileMapSnapshotFactory snapshotFactory,
            IGraphLogicalTileMapReportFormatter formatter)
        {
            _snapshotFactory = snapshotFactory;
            _formatter = formatter;
        }

        public void EmitAndCompare(
            string source,
            GraphAsset graph,
            int seed,
            GraphLogicalTileMap map,
            UObject context = null)
        {
            if (map == null)
                return;

            var snapshot = _snapshotFactory.Create(source, graph, seed, map);

            Emit(_formatter.BuildSummary(snapshot), context, warning: false);

            var other = IsPreviewSource(source) ? _lastScene : _lastPreview;

            if (other != null)
            {
                string comparison = _formatter.BuildComparison(snapshot, other, out int mismatchCount);
                Emit(comparison, context, mismatchCount != 0);
            }

            if (IsPreviewSource(source))
                _lastPreview = snapshot;
            else
                _lastScene = snapshot;
        }

        private static bool IsPreviewSource(string source)
        {
            return !string.IsNullOrEmpty(source)
                   && source.IndexOf("preview", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void Emit(string message, UObject context, bool warning)
        {
            if (warning)
            {
                if (context != null)
                    Debug.LogWarning(message, context);
                else
                    Debug.LogWarning(message);

                return;
            }

            if (context != null)
                Debug.Log(message, context);
            else
                Debug.Log(message);
        }
    }
}