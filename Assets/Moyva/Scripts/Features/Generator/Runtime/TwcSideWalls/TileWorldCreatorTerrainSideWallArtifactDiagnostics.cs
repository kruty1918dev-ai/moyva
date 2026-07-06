using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal struct TileWorldCreatorTerrainSideWallArtifactDiagnostics
    {
        private readonly float _expectedWallLength;
        private readonly int _heightStep;
        private float _largestArea;
        private string _largestAreaSample;

        public TileWorldCreatorTerrainSideWallArtifactDiagnostics(float expectedWallLength, int heightStep)
        {
            _expectedWallLength = Mathf.Max(0.0001f, expectedWallLength);
            _heightStep = Mathf.Max(1, heightStep);
            _largestArea = 0f;
            _largestAreaSample = "<none>";
            BoundaryEdgeChecks = 0;
            BoundaryWallCandidates = 0;
            SkippedBoundaryWallCandidates = 0;
            GeneratedBoundaryWalls = 0;
            GeneratedInteriorWalls = 0;
            SuspiciousLengthWalls = 0;
            TallWalls = 0;
        }

        public int BoundaryEdgeChecks { get; set; }
        public int BoundaryWallCandidates { get; set; }
        public int SkippedBoundaryWallCandidates { get; set; }
        public int GeneratedBoundaryWalls { get; set; }
        public int GeneratedInteriorWalls { get; set; }
        public int SuspiciousLengthWalls { get; set; }
        public int TallWalls { get; set; }

        public bool IsSuspicious(float wallLength, float wallHeight)
            => wallLength > _expectedWallLength * 1.05f || wallHeight > _heightStep * 3.05f;

        public void Observe(TileWorldCreatorTerrainSideWallEdge edge, int currentLevel, int neighbourLevel, bool hasNeighbour, float wallLength, float wallHeight)
        {
            if (wallLength > _expectedWallLength * 1.05f)
                SuspiciousLengthWalls++;
            if (wallHeight > _heightStep * 3.05f)
                TallWalls++;

            float area = wallLength * wallHeight;
            if (area <= _largestArea)
                return;

            _largestArea = area;
            _largestAreaSample = $"cell=({edge.CellX},{edge.CellY}) dir={edge.Direction} neighbour=({edge.NeighbourX},{edge.NeighbourY}) hasNeighbour={hasNeighbour} levels={neighbourLevel}->{currentLevel} length={TileWorldCreatorTerrainSideWallFormat.Number(wallLength)} height={TileWorldCreatorTerrainSideWallFormat.Number(wallHeight)} area={TileWorldCreatorTerrainSideWallFormat.Number(area)} edge=({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.Start)})->({TileWorldCreatorTerrainSideWallFormat.Vector3(edge.End)})";
        }

        public string Format()
            => $"boundaryEdgeChecks={BoundaryEdgeChecks}, boundaryWallCandidates={BoundaryWallCandidates}, skippedBoundaryWallCandidates={SkippedBoundaryWallCandidates}, generatedBoundaryWalls={GeneratedBoundaryWalls}, generatedInteriorWalls={GeneratedInteriorWalls}, suspiciousLengthWalls={SuspiciousLengthWalls}, tallWalls={TallWalls}, largestWall={_largestAreaSample}";
    }
}
