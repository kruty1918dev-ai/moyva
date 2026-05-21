using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class MenuWorldPreviewKingdomPlacer
    {
        public static MenuWorldPreviewKingdomPlacementReport Apply(MenuWorldPreviewData previewData, MenuPreviewKingdomPlacementSettings settings)
        {
            var report = new MenuWorldPreviewKingdomPlacementReport();

            if (previewData == null)
            {
                report.Warning = "Preview data is null.";
                return report;
            }

            if (settings == null || !settings.Enabled)
                return report;

            settings.ClampAndNormalize();

            var state = new PlacementState(previewData, settings, report);
            state.Run();
            return report;
        }

        private sealed class PlacementState
        {
            private readonly MenuWorldPreviewData _data;
            private readonly MenuPreviewKingdomPlacementSettings _settings;
            private readonly MenuWorldPreviewKingdomPlacementReport _report;
            private readonly HashSet<Vector2Int> _newPlacements = new HashSet<Vector2Int>();
            private readonly HashSet<string> _forbiddenTiles;
            private readonly System.Random _random;

            public PlacementState(
                MenuWorldPreviewData data,
                MenuPreviewKingdomPlacementSettings settings,
                MenuWorldPreviewKingdomPlacementReport report)
            {
                _data = data;
                _settings = settings;
                _report = report;
                _forbiddenTiles = BuildForbiddenTileSet(settings);
                _random = new System.Random(unchecked(_data.Seed ^ 0x41D1A53B));
            }

            public void Run()
            {
                EnsureBuildingMapExists();

                Vector2Int? castleA = TryPlaceInZone(_settings.KingdomAZone, _settings.CastleBuildingId, null, _settings.CastleMinDistance);
                if (!castleA.HasValue)
                    _report.FailedCastles++;

                Vector2Int? castleB = TryPlaceInZone(_settings.KingdomBZone, _settings.CastleBuildingId, castleA, _settings.CastleMinDistance);
                if (!castleB.HasValue)
                    _report.FailedCastles++;

                if (castleA.HasValue)
                    PlaceKingdomAroundCastle(castleA.Value);
                if (castleB.HasValue)
                    PlaceKingdomAroundCastle(castleB.Value);

                PlaceSmallTowns();

                if (castleA.HasValue && castleB.HasValue)
                    _report.CastleDistance = Manhattan(castleA.Value, castleB.Value);
            }

            private void EnsureBuildingMapExists()
            {
                if (_data.BuildingMap != null)
                    return;

                _report.Warning = "BuildingMap is null and cannot be modified.";
            }

            private void PlaceKingdomAroundCastle(Vector2Int castlePos)
            {
                for (int i = 0; i < _settings.WarehousesPerKingdom; i++)
                {
                    if (TryPlaceNear(castlePos, _settings.KingdomSettlementRadius, _settings.WarehouseBuildingId))
                        _report.PlacedWarehouses++;
                    else
                        _report.FailedWarehouses++;
                }

                for (int i = 0; i < _settings.KingdomLocalSettlementCount; i++)
                {
                    if (TryPlaceNear(castlePos, _settings.KingdomSettlementRadius, _settings.LocalSettlementBuildingId))
                        _report.PlacedLocalSettlements++;
                    else
                        _report.FailedLocalSettlements++;
                }
            }

            private void PlaceSmallTowns()
            {
                int maxAttempts = Mathf.Max(_settings.MaxAttemptsPerPlacement, _settings.SmallTownCount * 24);
                int attempts = 0;
                while (_report.PlacedSmallTowns < _settings.SmallTownCount && attempts < maxAttempts)
                {
                    attempts++;
                    var candidate = new Vector2Int(_random.Next(0, _data.Width), _random.Next(0, _data.Height));
                    if (!IsValid(candidate, _settings.TownHallBuildingId, null, _settings.MinSettlementDistance))
                        continue;

                    Place(candidate, _settings.TownHallBuildingId);
                    _report.PlacedSmallTowns++;
                }

                _report.FailedSmallTowns += Mathf.Max(0, _settings.SmallTownCount - _report.PlacedSmallTowns);
            }

            private Vector2Int? TryPlaceInZone(RectInt zone, string buildingId, Vector2Int? mustBeFarFrom, int minDistance)
            {
                if (string.IsNullOrWhiteSpace(buildingId) || _data.BuildingMap == null)
                    return null;

                var clamped = ClampZoneToMap(zone);
                if (clamped.width <= 0 || clamped.height <= 0)
                    return null;

                int maxAttempts = Mathf.Max(8, _settings.MaxAttemptsPerPlacement);
                for (int i = 0; i < maxAttempts; i++)
                {
                    var candidate = new Vector2Int(
                        _random.Next(clamped.xMin, clamped.xMax),
                        _random.Next(clamped.yMin, clamped.yMax));

                    if (!IsValid(candidate, buildingId, mustBeFarFrom, minDistance))
                        continue;

                    Place(candidate, buildingId);
                    if (string.Equals(buildingId, _settings.CastleBuildingId, StringComparison.OrdinalIgnoreCase))
                        _report.PlacedCastles++;
                    return candidate;
                }

                return null;
            }

            private bool TryPlaceNear(Vector2Int center, int radius, string buildingId)
            {
                if (string.IsNullOrWhiteSpace(buildingId) || _data.BuildingMap == null)
                    return false;

                int maxAttempts = Mathf.Max(8, _settings.MaxAttemptsPerPlacement);
                int safeRadius = Mathf.Max(1, radius);

                for (int i = 0; i < maxAttempts; i++)
                {
                    int dx = _random.Next(-safeRadius, safeRadius + 1);
                    int dy = _random.Next(-safeRadius, safeRadius + 1);
                    if (dx == 0 && dy == 0)
                        continue;

                    var candidate = new Vector2Int(center.x + dx, center.y + dy);
                    if (!IsInBounds(candidate))
                        continue;

                    if (!IsValid(candidate, buildingId, null, _settings.MinSettlementDistance))
                        continue;

                    Place(candidate, buildingId);
                    return true;
                }

                return false;
            }

            private bool IsValid(Vector2Int pos, string buildingId, Vector2Int? farFrom, int minDistance)
            {
                if (string.IsNullOrWhiteSpace(buildingId))
                    return false;
                if (!IsInBounds(pos))
                    return false;
                if (_data.BuildingMap == null)
                    return false;

                if (!string.IsNullOrWhiteSpace(_data.BuildingMap[pos.x, pos.y]))
                    return false;

                if (!IsHeightAllowed(pos))
                    return false;

                if (IsForbiddenTile(pos))
                    return false;

                if (farFrom.HasValue && Manhattan(pos, farFrom.Value) < Mathf.Max(1, minDistance))
                    return false;

                foreach (var placed in _newPlacements)
                {
                    if (Manhattan(placed, pos) < Mathf.Max(1, _settings.MinSettlementDistance))
                        return false;
                }

                return true;
            }

            private bool IsHeightAllowed(Vector2Int pos)
            {
                if (_data.HeightMap == null)
                    return true;

                float h = _data.HeightMap[pos.x, pos.y];
                return h >= _settings.MinHeight && h <= _settings.MaxHeight;
            }

            private bool IsForbiddenTile(Vector2Int pos)
            {
                if (_data.BiomeMap == null || _forbiddenTiles.Count == 0)
                    return false;

                string biome = _data.BiomeMap[pos.x, pos.y];
                if (string.IsNullOrWhiteSpace(biome))
                    return false;

                if (_forbiddenTiles.Contains(biome))
                    return true;

                if (!_settings.MatchBaseTileType)
                    return false;

                int sepIndex = biome.IndexOf(_settings.TileSeparator);
                if (sepIndex <= 0)
                    return false;

                string baseBiome = biome.Substring(0, sepIndex);
                return _forbiddenTiles.Contains(baseBiome);
            }

            private static HashSet<string> BuildForbiddenTileSet(MenuPreviewKingdomPlacementSettings settings)
            {
                var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (settings.ForbiddenBiomeTileIds == null)
                    return result;

                foreach (var tileId in settings.ForbiddenBiomeTileIds)
                {
                    if (!string.IsNullOrWhiteSpace(tileId))
                        result.Add(tileId.Trim());
                }

                return result;
            }

            private RectInt ClampZoneToMap(RectInt zone)
            {
                int xMin = Mathf.Clamp(zone.xMin, 0, _data.Width);
                int yMin = Mathf.Clamp(zone.yMin, 0, _data.Height);
                int xMax = Mathf.Clamp(zone.xMax, 0, _data.Width);
                int yMax = Mathf.Clamp(zone.yMax, 0, _data.Height);
                int width = Mathf.Max(0, xMax - xMin);
                int height = Mathf.Max(0, yMax - yMin);
                return new RectInt(xMin, yMin, width, height);
            }

            private void Place(Vector2Int pos, string buildingId)
            {
                _data.BuildingMap[pos.x, pos.y] = buildingId;
                _newPlacements.Add(pos);
            }

            private bool IsInBounds(Vector2Int pos)
            {
                return pos.x >= 0 && pos.y >= 0 && pos.x < _data.Width && pos.y < _data.Height;
            }

            private static int Manhattan(Vector2Int a, Vector2Int b)
            {
                return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            }
        }
    }

    public sealed class MenuWorldPreviewKingdomPlacementReport
    {
        public int PlacedCastles;
        public int FailedCastles;
        public int PlacedWarehouses;
        public int FailedWarehouses;
        public int PlacedLocalSettlements;
        public int FailedLocalSettlements;
        public int PlacedSmallTowns;
        public int FailedSmallTowns;
        public int CastleDistance;
        public string Warning;

        public override string ToString()
        {
            return $"castles={PlacedCastles}/{PlacedCastles + FailedCastles}, "
                   + $"warehouses={PlacedWarehouses}/{PlacedWarehouses + FailedWarehouses}, "
                   + $"local={PlacedLocalSettlements}/{PlacedLocalSettlements + FailedLocalSettlements}, "
                   + $"smallTowns={PlacedSmallTowns}/{PlacedSmallTowns + FailedSmallTowns}, "
                   + $"castleDistance={CastleDistance}";
        }
    }
}