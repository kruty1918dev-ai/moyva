using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingPlacementEvaluationResult
    {
        private readonly List<BuildingPlacementBlocker> _blockers = new List<BuildingPlacementBlocker>();
        private readonly List<string> _notes = new List<string>();
        private readonly List<Vector2Int> _footprintPositions = new List<Vector2Int>();

        public bool IsValid => !ConfigurationBlocked && !TileOccupied && !TerrainBlocked && !SpacingBlocked && !FogBlocked && !InfluenceZoneBlocked;
        public bool ConfigurationBlocked { get; internal set; }
        public bool TileOccupied { get; internal set; }
        public bool TerrainBlocked { get; internal set; }
        public bool SpacingBlocked { get; internal set; }
        public bool FogBlocked { get; internal set; }
        public bool InfluenceZoneBlocked { get; internal set; }
        public IReadOnlyList<BuildingPlacementBlocker> Blockers => _blockers;
        public IReadOnlyList<string> Notes => _notes;
        public IReadOnlyList<Vector2Int> FootprintPositions => _footprintPositions;

        internal void AddBlocker(BuildingPlacementBlocker blocker)
        {
            if (blocker != null)
                _blockers.Add(blocker);
        }

        internal void AddNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
                _notes.Add(note);
        }

        internal void AddFootprintPosition(Vector2Int position)
        {
            _footprintPositions.Add(position);
        }
    }
}
