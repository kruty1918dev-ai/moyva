using System;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
        // ─── Локальні дії гравця (перехоплення) ─────────────────────────────────

        public bool TryHandleConfirmRequest()
        {
            if (_constructionService == null)
            {
                return false;
            }

            if (IsOfflineOrHost())
            {
                // Хост / офлайн: виконуємо одразу; BuildingPlacedSignal транслює результат.
                _constructionService.Confirm();
                return true;
            }

            // Клієнт: зібрати pending-розміщення, скасувати локально, надіслати запити до хоста.
            var pending = _constructionService.GetPendingPlacements();
            if (pending == null || pending.Count == 0)
            {
                _constructionService.Cancel();
                return true;
            }

            string ownerId = _constructionService.GetActiveOwner();
            var placementQuery =
                _constructionService as IConstructionPlacementQuery;
            var intentSource =
                _constructionService
                    as IConstructionPendingPlacementIntentSource;
            foreach (var kv in pending)
            {
                ConstructionPlacementCommitIntent intent =
                    intentSource != null
                    && intentSource.TryGetPendingPlacementIntent(
                        kv.Key,
                        out ConstructionPlacementCommitIntent
                            pendingIntent)
                        ? pendingIntent
                        : ConstructionPlacementCommitIntent.None;
                if (placementQuery != null)
                {
                    ConstructionPlacementQueryResult placement =
                        placementQuery.EvaluatePlacement(
                            CreateClientPlacementPreflightRequest(
                                kv.Value,
                                kv.Key,
                                ownerId,
                                intent.Rotation));
                    if (!placement.CanPreview
                        || !placement.ResourcesValid)
                    {
                        continue;
                    }
                }

                var payload = new BuildingPlacePayload(
                    GameActionMessageKind.Request,
                    kv.Value,
                    kv.Key,
                    ownerId,
                    ownerId,
                    intent.HasRelocationSource,
                    intent.RelocationSourcePosition
                        .GetValueOrDefault(),
                    intent.SatisfiedReplacementBuildingId,
                    intent.Rotation);

                _syncService.SendCommand(GameCommandType.BuildingPlace, payload.ToBytes());
            }

            // Pending previews remain until a host confirmation is received.
            // This preserves unaffordable previews and their exact deficit, and
            // also prevents a lost/rejected request from silently deleting the
            // player's placement intent.
            return true;
        }

        internal static ConstructionPlacementQueryRequest
            CreateClientPlacementPreflightRequest(
                string buildingId,
                Vector2Int position,
                string ownerId,
                ConstructionRotation rotation =
                    ConstructionRotation.Degrees0)
            => new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition: position,
                includeResources: true,
                includeDetails: true,
                ownerId: ownerId,
                includePendingPlacements: false,
                attemptSource:
                    ConstructionPlacementAttemptSource
                        .NetworkRequest,
                allowUniquePreviewRelocation: false,
                rotation: rotation);

        // ─── Хост: трансляція після локального виконання ─────────────────────────

        private void OnBuildingPlacedLocally(BuildingPlacedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new BuildingPlacePayload(
                GameActionMessageKind.Confirmed,
                signal.BuildingId,
                signal.Position,
                signal.OwnerId,
                signal.SourceFactionId,
                signal.HasRelocationSource,
                signal.RelocationSourcePosition,
                rotation: ConstructionRotationUtility.Normalize(
                    signal.RotationQuarterTurns));
            _syncService.SendCommand(GameCommandType.BuildingPlace, payload.ToBytes());
        }

        private void OnBuildingDemolishedLocally(BuildingDemolishedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new BuildingDemolishPayload(
                GameActionMessageKind.Confirmed,
                signal.Position,
                signal.OwnerId);
            _syncService.SendCommand(GameCommandType.BuildingDemolish, payload.ToBytes());
        }
        // ─── Мережеві обробники (вхідні повідомлення) ────────────────────────────

        private void OnNetworkBuildingPlace(string senderId, byte[] body)
        {
            var data = BuildingPlacePayload.FromBytes(body);

            if (_constructionService == null)
            {
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                // Лише хост обробляє запити.
                if (!IsOfflineOrHost()) return;
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        data.OwnerId,
                        data.SourceFactionId,
                        out string authorizedOwnerId,
                        out string authorizationReason))
                {
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    ConstructionPlacementCommitIntent intent =
                        data.ToCommitIntent();
                    bool placed;
                    if (_constructionService
                        is IAuthoritativeConstructionPlacementExecutor
                            authoritativeExecutor)
                    {
                        placed =
                            authoritativeExecutor
                                .TryPlaceAuthoritatively(
                                    data.BuildingId,
                                    data.Position,
                                    authorizedOwnerId,
                                    intent);
                    }
                    else if (!intent.HasRelocationSource
                             && string.IsNullOrWhiteSpace(
                                 intent
                                     .SatisfiedReplacementBuildingId))
                    {
                        placed = _constructionService.TryDirectPlace(
                            data.BuildingId,
                            data.Position,
                            authorizedOwnerId);
                    }
                    else
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot execute the requested placement intent authoritatively.");
                        placed = false;
                    }

                    if (placed)
                    {
                        // Хост вручну транслює підтвердження (BuildingPlacedSignal вже заблоковано флагом).
                        var confirmed = new BuildingPlacePayload(
                            GameActionMessageKind.Confirmed,
                            data.BuildingId,
                            data.Position,
                            authorizedOwnerId,
                            authorizedOwnerId,
                            intent.HasRelocationSource,
                            intent.RelocationSourcePosition
                                .GetValueOrDefault(),
                            intent.SatisfiedReplacementBuildingId,
                            intent.Rotation);
                        _syncService.SendCommand(GameCommandType.BuildingPlace, confirmed.ToBytes());
                    }
                    else
                    {
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
            else if (data.Kind == GameActionMessageKind.Confirmed)
            {
                // Клієнти застосовують підтверджене розміщення.
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    string confirmedOwnerId =
                        string.IsNullOrWhiteSpace(data.SourceFactionId)
                            ? data.OwnerId
                            : data.SourceFactionId;
                    ConstructionPlacementCommitIntent intent =
                        data.ToCommitIntent();
                    bool applied;
                    if (_constructionService
                        is IConfirmedConstructionPlacementIntentApplier
                            intentApplier)
                    {
                        applied =
                            intentApplier
                                .TryApplyConfirmedPlacement(
                                    data.BuildingId,
                                    data.Position,
                                    confirmedOwnerId,
                                    intent);
                    }
                    else if (!intent.HasRelocationSource
                             && string.IsNullOrWhiteSpace(
                                 intent
                                     .SatisfiedReplacementBuildingId)
                             && _constructionService
                                 is IConfirmedConstructionPlacementApplier
                                     legacyApplier)
                    {
                        applied =
                            legacyApplier.TryApplyConfirmedPlacement(
                                data.BuildingId,
                                data.Position,
                                confirmedOwnerId);
                    }
                    else
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot apply a host-confirmed placement intent without charging client resources.");
                        return;
                    }
                    if (applied
                        && _constructionService
                            .TryGetPendingBuildingIdAt(
                                data.Position,
                                out string pendingBuildingId)
                        && string.Equals(
                            pendingBuildingId,
                            data.BuildingId,
                            System.StringComparison.Ordinal))
                    {
                        _constructionService.RemovePendingAt(
                            data.Position);
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
        }

        private void OnNetworkBuildingDemolish(string senderId, byte[] body)
        {
            var data = BuildingDemolishPayload.FromBytes(body);

            if (_constructionService == null)
            {
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                if (!IsOfflineOrHost()) return;
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        data.OwnerId,
                        requestedSourceOwnerId: null,
                        out string authorizedOwnerId,
                        out string authorizationReason))
                {
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    bool demolished =
                        _constructionService.TryDemolishByFaction(
                            data.Position,
                            authorizedOwnerId);
                    if (demolished)
                    {
                        var confirmed = new BuildingDemolishPayload(
                            GameActionMessageKind.Confirmed,
                            data.Position,
                            authorizedOwnerId);
                        _syncService.SendCommand(GameCommandType.BuildingDemolish, confirmed.ToBytes());
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
            else if (data.Kind == GameActionMessageKind.Confirmed)
            {
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    if (_constructionService
                        is not IConfirmedConstructionDemolitionApplier applier)
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot apply a host-confirmed demolition without local turn authority.");
                        return;
                    }

                    if (!applier.TryApplyConfirmedDemolition(
                            data.Position,
                            data.OwnerId))
                    {
                    }
                }
                finally { _applyingNetworkEvent = false; }
            }
        }

    }
}
