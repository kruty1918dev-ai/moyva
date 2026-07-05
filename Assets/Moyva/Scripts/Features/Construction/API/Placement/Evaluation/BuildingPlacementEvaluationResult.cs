using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingPlacementEvaluationResult
    {
        private readonly List<BuildingPlacementBlocker> _blockers = new List<BuildingPlacementBlocker>();
        private readonly List<string> _notes = new List<string>();

        public bool IsValid => !TileOccupied && !SpacingBlocked && !FogBlocked && !InfluenceZoneBlocked;
        public bool TileOccupied { get; internal set; }
        public bool SpacingBlocked { get; internal set; }
        public bool FogBlocked { get; internal set; }
        public bool InfluenceZoneBlocked { get; internal set; }
        public IReadOnlyList<BuildingPlacementBlocker> Blockers => _blockers;
        public IReadOnlyList<string> Notes => _notes;

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
    }
}
