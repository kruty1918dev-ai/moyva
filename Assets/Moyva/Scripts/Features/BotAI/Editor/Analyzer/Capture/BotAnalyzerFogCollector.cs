using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerFogCollector
    {
        private readonly BotAnalyzerServices _services;
        private string _cachedOwner = string.Empty;
        private double _nextSampleTime;
        private BotAnalyzerFogState _cached = new();

        public BotAnalyzerFogCollector(BotAnalyzerServices services)
        {
            _services = services;
        }

        public BotAnalyzerFogState Collect(
            string ownerId,
            double now,
            BotAnalyzerSettings settings,
            bool force = false)
        {
            if (!force &&
                string.Equals(_cachedOwner, ownerId, StringComparison.Ordinal) &&
                now < _nextSampleTime)
            {
                return Clone(_cached);
            }

            _cachedOwner = ownerId ?? string.Empty;
            _nextSampleTime = now + Math.Max(0.20f, settings?.FogSampleInterval ?? 0.50f);

            if (_services?.Grid == null || string.IsNullOrWhiteSpace(ownerId))
            {
                _cached = new BotAnalyzerFogState();
                return Clone(_cached);
            }

            if (_services.Perception != null)
            {
                _cached = new BotAnalyzerFogState
                {
                    GridWidth = _services.Grid.GridWidth,
                    GridHeight = _services.Grid.GridHeight,
                    VisibleCells = new List<Vector2Int>(_services.Perception.GetVisibleCells(ownerId)),
                    ExploredCells = new List<Vector2Int>(_services.Perception.GetExploredCells(ownerId)),
                };
                return Clone(_cached);
            }

            if (_services.FogRegistry == null ||
                !_services.FogRegistry.TryGetFor(ownerId, out IFogOfWarService fog) ||
                fog == null)
            {
                _cached = new BotAnalyzerFogState
                {
                    GridWidth = _services.Grid.GridWidth,
                    GridHeight = _services.Grid.GridHeight,
                };
                return Clone(_cached);
            }

            _cached = CollectFromReader(_services.Grid, fog);
            return Clone(_cached);
        }

        internal static BotAnalyzerFogState CollectFromReader(
            IGridService grid,
            IFogStateReader reader)
        {
            var result = new BotAnalyzerFogState
            {
                GridWidth = Math.Max(0, grid?.GridWidth ?? 0),
                GridHeight = Math.Max(0, grid?.GridHeight ?? 0),
            };

            if (grid == null || reader == null)
                return result;

            for (int y = 0; y < result.GridHeight; y++)
            {
                for (int x = 0; x < result.GridWidth; x++)
                {
                    var cell = new Vector2Int(x, y);
                    FogStateType state = reader.GetFogState(cell);
                    if (state == FogStateType.Visible)
                        result.VisibleCells.Add(cell);
                    else if (state == FogStateType.Explored)
                        result.ExploredCells.Add(cell);
                }
            }

            return result;
        }

        private static BotAnalyzerFogState Clone(BotAnalyzerFogState source)
        {
            if (source == null)
                return new BotAnalyzerFogState();

            return new BotAnalyzerFogState
            {
                GridWidth = source.GridWidth,
                GridHeight = source.GridHeight,
                VisibleCells = new List<Vector2Int>(source.VisibleCells ?? new List<Vector2Int>()),
                ExploredCells = new List<Vector2Int>(source.ExploredCells ?? new List<Vector2Int>()),
            };
        }
    }
}
