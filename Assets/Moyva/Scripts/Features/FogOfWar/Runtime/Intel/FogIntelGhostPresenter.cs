using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Renders last-known intel as lightweight ghost markers for the local
    /// perspective owner. Ghosts are pure presentation: pooled sprite markers,
    /// never gameplay entities, never selectable, never fed back into fog or
    /// simulation state.
    ///
    /// A ghost is shown only while its cell is Explored-but-not-Visible for
    /// the local owner — once the cell is re-observed the real entity (or its
    /// absence) takes over.
    /// </summary>
    internal sealed class FogIntelGhostPresenter
        : IInitializable
        , ITickable
        , IDisposable
    {
        private const int MaxGhosts = 512;
        private const string RootName = "FogIntelGhosts";
        private const int GhostSortingOrder = 40;

        private readonly IFogIntelReader _intel;
        private readonly IFogLocalPerspective _perspective;
        private readonly IFogOwnerStateReader _ownerFog;
        private readonly IGridProjection _gridProjection;
        private readonly FogOfWarService _fogService;
        private readonly FogOfWarSettings _settings;

        private readonly List<SpriteRenderer> _ghostPool =
            new List<SpriteRenderer>(64);
        private Transform _root;
        private Sprite _unitSprite;
        private Sprite _buildingSprite;
        private bool _dirty = true;
        private int _lastFogVersion = -1;

        public FogIntelGhostPresenter(
            IFogIntelReader intel,
            IFogLocalPerspective perspective,
            IFogOwnerStateReader ownerFog,
            IGridProjection gridProjection,
            [InjectOptional] FogOfWarService fogService = null,
            [InjectOptional] FogOfWarSettings settings = null)
        {
            _intel = intel;
            _perspective = perspective;
            _ownerFog = ownerFog;
            _gridProjection = gridProjection;
            _fogService = fogService;
            _settings = settings;
        }

        public void Initialize()
        {
            if (_intel != null)
                _intel.IntelChanged += OnIntelChanged;
        }

        public void Dispose()
        {
            if (_intel != null)
                _intel.IntelChanged -= OnIntelChanged;
            ReleaseAll();
            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
                _root = null;
            }
            if (_unitSprite != null)
            {
                Object.Destroy(_unitSprite);
                _unitSprite = null;
            }
            if (_buildingSprite != null && _buildingSprite != _unitSprite)
                Object.Destroy(_buildingSprite);
            _buildingSprite = null;
        }

        private void OnIntelChanged(string ownerId)
        {
            if (IsLocalOwner(ownerId))
                _dirty = true;
        }

        public void Tick()
        {
            // Fog transitions (visible -> explored) also affect ghost display.
            if (_fogService != null && _fogService.Version != _lastFogVersion)
            {
                _lastFogVersion = _fogService.Version;
                _dirty = true;
            }

            if (!_dirty)
                return;

            _dirty = false;
            Rebuild();
        }

        private void Rebuild()
        {
            string ownerId = _perspective?.LocalPerspectiveOwnerId;
            if (_intel == null
                || _ownerFog == null
                || _gridProjection == null
                || string.IsNullOrWhiteSpace(ownerId))
            {
                ReleaseAll();
                return;
            }
            ownerId = ownerId.Trim();

            int used = 0;
            used = RebuildBuildings(ownerId, used);
            used = RebuildUnits(ownerId, used);

            for (int index = used; index < _ghostPool.Count; index++)
            {
                if (_ghostPool[index] != null)
                    _ghostPool[index].gameObject.SetActive(false);
            }
        }

        private int RebuildBuildings(string ownerId, int used)
        {
            var buildings = _intel.GetRememberedBuildings(ownerId);
            if (buildings == null)
                return used;

            foreach (FogIntelBuildingRecord record in buildings)
            {
                if (used >= MaxGhosts || record == null)
                    break;
                if (!IsExploredNotVisible(ownerId, record.Position))
                    continue;

                SpriteRenderer ghost = AcquireGhost(used++);
                if (ghost == null)
                    continue;

                PlaceGhost(ghost, record.Position, GetBuildingSprite(), 1f);
            }

            return used;
        }

        private int RebuildUnits(string ownerId, int used)
        {
            var units = _intel.GetRememberedUnits(ownerId);
            if (units == null)
                return used;

            foreach (FogIntelUnitRecord record in units)
            {
                if (used >= MaxGhosts || record == null)
                    break;
                if (!IsExploredNotVisible(ownerId, record.LastKnownPosition))
                    continue;

                SpriteRenderer ghost = AcquireGhost(used++);
                if (ghost == null)
                    continue;

                PlaceGhost(ghost, record.LastKnownPosition, GetUnitSprite(), 0.6f);
            }

            return used;
        }

        private bool IsExploredNotVisible(string ownerId, Vector2Int cell)
            => _ownerFog.GetFogState(ownerId, cell) == FogStateType.Explored;

        private void PlaceGhost(
            SpriteRenderer ghost,
            Vector2Int cell,
            Sprite sprite,
            float scaleFactor)
        {
            ghost.sprite = sprite;
            Vector3 world = _gridProjection.GridToWorld(cell);
            ghost.transform.position = world;
            ghost.transform.rotation =
                _gridProjection.WorldPlane == GridWorldPlane.XZ
                    ? Quaternion.Euler(90f, 0f, 0f)
                    : Quaternion.identity;

            float cellSize = ResolveCellSize(cell);
            ghost.transform.localScale = Vector3.one * cellSize * scaleFactor;
            ghost.gameObject.SetActive(true);
        }

        private float ResolveCellSize(Vector2Int cell)
        {
            float step = _gridProjection.GetStepDistance(
                cell,
                cell + new Vector2Int(1, 0));
            return step > 0.0001f ? step : 1f;
        }

        private SpriteRenderer AcquireGhost(int index)
        {
            while (_ghostPool.Count <= index)
            {
                EnsureRoot();
                var go = new GameObject($"Ghost_{_ghostPool.Count}");
                go.transform.SetParent(_root, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = GhostSortingOrder;
                renderer.color = ResolveGhostColor();
                _ghostPool.Add(renderer);
            }

            return _ghostPool[index];
        }

        private void EnsureRoot()
        {
            if (_root != null)
                return;
            var existing = GameObject.Find(RootName);
            _root = existing != null
                ? existing.transform
                : new GameObject(RootName).transform;
        }

        private Color ResolveGhostColor()
        {
            if (_settings == null)
                return new Color(0.85f, 0.85f, 0.85f, 0.55f);
            Color color = _settings.ExploredColor;
            color.a = Mathf.Clamp01(color.a + 0.25f);
            return color;
        }

        private Sprite GetUnitSprite()
            => _unitSprite ??= ResolveIconSprite() ?? CreateFallbackSprite();

        private Sprite GetBuildingSprite()
            => _buildingSprite ??= ResolveIconSprite() ?? CreateFallbackSprite();

        private Sprite ResolveIconSprite()
        {
            Sprite[] icons = _settings?.FogIconSprites;
            return icons != null && icons.Length > 0 ? icons[0] : null;
        }

        private static Sprite CreateFallbackSprite()
            => Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f),
                4f);

        private void ReleaseAll()
        {
            for (int index = 0; index < _ghostPool.Count; index++)
            {
                if (_ghostPool[index] != null)
                    _ghostPool[index].gameObject.SetActive(false);
            }
        }

        private bool IsLocalOwner(string ownerId)
        {
            string local = _perspective?.LocalPerspectiveOwnerId;
            return !string.IsNullOrWhiteSpace(local)
                   && !string.IsNullOrWhiteSpace(ownerId)
                   && string.Equals(
                       local.Trim(),
                       ownerId.Trim(),
                       System.StringComparison.Ordinal);
        }
    }
}
