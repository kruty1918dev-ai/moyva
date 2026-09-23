using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// One generated stair flight: a 1-cell-wide corridor of modules carved
    /// into the low plateau, ending flush against the high plateau edge.
    /// Modules are ordered from the entry (lowest) to the exit (highest).
    /// </summary>
    public sealed class StairFlight
    {
        /// <summary>Low-plateau cell directly before the first module.</summary>
        public Vector2Int Entrance;
        /// <summary>Cell adjacent to the high plateau; holds the last module.</summary>
        public Vector2Int ExitModule;
        /// <summary>High-plateau cell the flight exits onto.</summary>
        public Vector2Int Exit;
        /// <summary>Climb direction: 0 = +Z, 1 = +X, 2 = -Z, 3 = -X.</summary>
        public int DirectionIndex;
        /// <summary>Corridor cells, lowest module first.</summary>
        public Vector2Int[] Modules;
        /// <summary>Top height of <see cref="Modules"/>[i].</summary>
        public float[] ModuleTopY;
        public float LowSurfaceY;
        public float HighSurfaceY;
        public string ThemeId;
    }

    /// <summary>
    /// Output of the passage planner: the generated stair flights plus derived
    /// lookup data for traversal classification and mesh emission.
    /// </summary>
    public sealed class TerrainPassagePlan
    {
        public List<StairFlight> Flights = new();

        public static Vector2Int DirectionOffset(int directionIndex)
        {
            return directionIndex switch
            {
                0 => new Vector2Int(0, 1),
                1 => new Vector2Int(1, 0),
                2 => new Vector2Int(0, -1),
                3 => new Vector2Int(-1, 0),
                _ => Vector2Int.zero,
            };
        }

        public static int DirectionIndexOf(Vector2Int offset)
        {
            if (offset == new Vector2Int(0, 1)) return 0;
            if (offset == new Vector2Int(1, 0)) return 1;
            if (offset == new Vector2Int(0, -1)) return 2;
            if (offset == new Vector2Int(-1, 0)) return 3;
            return -1;
        }

        /// <summary>Module cells in climb order for a flight.</summary>
        public IEnumerable<Vector2Int> EnumerateCells(StairFlight flight)
        {
            if (flight?.Modules == null)
                yield break;
            for (int i = 0; i < flight.Modules.Length; i++)
                yield return flight.Modules[i];
        }

        /// <summary>
        /// Orthogonal stair step edges of the plan (unordered cell pairs):
        /// plateau->first module, module->module and last module->plateau.
        /// </summary>
        public IEnumerable<(Vector2Int a, Vector2Int b)> EnumerateStairSteps()
        {
            if (Flights == null)
                yield break;

            foreach (StairFlight flight in Flights)
            {
                if (flight == null || flight.Modules == null || flight.Modules.Length == 0)
                    continue;

                yield return (flight.Entrance, flight.Modules[0]);
                for (int i = 0; i + 1 < flight.Modules.Length; i++)
                    yield return (flight.Modules[i], flight.Modules[i + 1]);
                yield return (flight.Modules[flight.Modules.Length - 1], flight.Exit);
            }
        }
    }

    /// <summary>
    /// Runtime store for the active world's generated passages. The generator
    /// writes it before the chunk build; mesh providers and traversal read it
    /// through <see cref="ITerrainPassageMap"/>.
    /// </summary>
    internal sealed class TerrainPassageStore : ITerrainPassageMap
    {
        private readonly Dictionary<Vector2Int, TerrainPassageModule> _modules = new();
        private readonly HashSet<long> _stairSteps = new();
        private int _version;

        public int Version => _version;
        public bool HasPassages => _modules.Count > 0;

        public void Clear()
        {
            if (_modules.Count == 0 && _stairSteps.Count == 0)
                return;
            _modules.Clear();
            _stairSteps.Clear();
            unchecked { _version++; }
        }

        public void Replace(TerrainPassagePlan plan)
        {
            _modules.Clear();
            _stairSteps.Clear();
            if (plan?.Flights != null)
            {
                foreach (StairFlight flight in plan.Flights)
                {
                    if (flight?.Modules == null)
                        continue;
                    for (int i = 0; i < flight.Modules.Length; i++)
                    {
                        _modules[flight.Modules[i]] = new TerrainPassageModule
                        {
                            Cell = flight.Modules[i],
                            DirectionIndex = flight.DirectionIndex,
                            TopY = flight.ModuleTopY[i],
                            RiseMeters = flight.ModuleTopY[i]
                                - (i == 0 ? flight.LowSurfaceY : flight.ModuleTopY[i - 1]),
                            LowSurfaceY = flight.LowSurfaceY,
                            HighSurfaceY = flight.HighSurfaceY,
                            ThemeId = flight.ThemeId,
                        };
                    }
                }

                foreach ((Vector2Int a, Vector2Int b) in plan.EnumerateStairSteps())
                    _stairSteps.Add(PackStep(a, b));
            }
            unchecked { _version++; }
        }

        public bool TryGetModule(Vector2Int cell, out TerrainPassageModule module)
            => _modules.TryGetValue(cell, out module);

        public bool IsStairCell(Vector2Int cell)
            => _modules.ContainsKey(cell);

        public bool IsStairStep(Vector2Int from, Vector2Int to)
            => _stairSteps.Contains(PackStep(from, to));

        internal static long PackStep(Vector2Int a, Vector2Int b)
        {
            long ai = ((long)(a.x + 32768) << 21) | (uint)(a.y + 32768);
            long bi = ((long)(b.x + 32768) << 21) | (uint)(b.y + 32768);
            long lo = ai < bi ? ai : bi;
            long hi = ai < bi ? bi : ai;
            return lo * 1000003L + hi;
        }
    }
}
