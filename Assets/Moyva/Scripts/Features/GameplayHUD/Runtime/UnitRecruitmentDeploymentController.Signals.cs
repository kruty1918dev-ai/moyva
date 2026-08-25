using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class UnitRecruitmentDeploymentController
    {
        private void OnTurnStateChanged()
        {
            if (_session == null)
                return;

            if (!_turns.CanOwnerAct(_session.OwnerId, out _))
                CancelSession();
        }

        private void OnRecruitmentQueueChanged(
            UnitRecruitmentQueueChangedSignal signal)
        {
            if (_session == null
                || !string.Equals(
                    NormalizeId(signal.OwnerId),
                    _session.OwnerId,
                    StringComparison.Ordinal)
                || signal.BuildingPosition != _session.RecruitingBuildingPosition)
            {
                return;
            }

            if (!_recruitment.TryPeekReady(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != _session.QueueId)
            {
                EndSession(destroyPreview: true);
                return;
            }

            RefreshDeploymentTiles();
        }


        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            if (_session != null && signal.NewMode != GameModeType.Normal)
                CancelSession();
        }

        private static bool TryResolveRendererBounds(
            GameObject root,
            out Bounds bounds)
        {
            bounds = default;
            if (root == null)
                return false;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || !renderer.enabled)
                    continue;

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return found;
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void DestroyMaterial(Material material)
        {
            if (material != null)
                Object.Destroy(material);
        }

        private static void DestroyRuntimeRoot(Transform root)
        {
            if (root != null)
                Object.Destroy(root.gameObject);
        }

        private sealed class DeploymentSession
        {
            public readonly string OwnerId;
            public readonly long QueueId;
            public readonly string UnitTypeId;
            public readonly Vector2Int RecruitingBuildingPosition;
            public readonly List<UnitRecruitmentDeploymentTileSnapshot> Tiles = new();
            public readonly HashSet<Vector2Int> ValidTiles = new();
            public readonly Dictionary<Vector2Int, string> InvalidReasons = new();
            public IDisposable InputBlock;
            public Vector2Int? SelectedTile;
            public bool OverlayAcquired;

            public DeploymentSession(
                string ownerId,
                long queueId,
                string unitTypeId,
                Vector2Int recruitingBuildingPosition)
            {
                OwnerId = ownerId;
                QueueId = queueId;
                UnitTypeId = unitTypeId ?? string.Empty;
                RecruitingBuildingPosition = recruitingBuildingPosition;
            }

            public bool Matches(string ownerId, long queueId)
                => QueueId == queueId
                   && string.Equals(
                       OwnerId,
                       NormalizeId(ownerId),
                       StringComparison.Ordinal);

            public void SetTiles(
                IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles)
            {
                Tiles.Clear();
                ValidTiles.Clear();
                InvalidReasons.Clear();

                if (tiles == null)
                    return;

                for (int index = 0; index < tiles.Count; index++)
                {
                    UnitRecruitmentDeploymentTileSnapshot tile = tiles[index];
                    Tiles.Add(tile);
                    if (tile.IsValid)
                        ValidTiles.Add(tile.Position);
                    else
                        InvalidReasons[tile.Position] = tile.Reason;
                }
            }
        }
    }
}
