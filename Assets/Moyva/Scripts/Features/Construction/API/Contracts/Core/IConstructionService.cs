using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionService
    {
        /// <summary>Поточний стан сесії будівництва.</summary>
        BuildingPlacementState State { get; }

        /// <summary>True коли активний режим знесення (замість розміщення).</summary>
        bool IsDemolishMode { get; }

        /// <summary>
        /// Вибрати будівлю для розміщення. Перемикає State → Placing.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>Поточний вибраний buildingId або null якщо нічого не вибрано.</summary>
        string GetSelectedBuildingId();

        /// <summary>
        /// Встановити owner для наступних операцій Confirm/RestoreFromSave.
        /// Використовується для мультиплеєра/ботів (ізоляція подій по власнику).
        /// </summary>
        void SetActiveOwner(string ownerId);

        /// <summary>
        /// Поточний owner, який буде записаний у BuildingPlacedSignal / BuildingDemolishedSignal.
        /// </summary>
        string GetActiveOwner();

        /// <summary>
        /// Спробувати розмістити preview будівлі на тайлі.
        /// Надсилає BuildingPreviewChangedSignal з актуальним BuildingPreviewState.
        /// Повертає true для просторово валідних станів Valid та Unaffordable;
        /// false — для Blocked або коли State != Placing.
        /// </summary>
        bool TryPreviewAt(Vector2Int position);

        /// <summary>
        /// Повертає true, якщо на тайлі є непідтверджене preview-розміщення в поточній сесії.
        /// </summary>
        bool HasPendingPlacementAt(Vector2Int position);

        /// <summary>
        /// Повертає buildingId непідтвердженого preview-розміщення на тайлі.
        /// </summary>
        bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId);

        /// <summary>
        /// Повертає snapshot усіх непідтверджених preview-розміщень поточної сесії.
        /// Ключ — позиція тайлу, значення — buildingId.
        /// </summary>
        IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements();

        /// <summary>
        /// Перемістити непідтверджену будівлю з одного тайлу на інший.
        /// Повертає true при успіху, false якщо нова позиція заблокована або preview не знайдено.
        /// </summary>
        bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition);

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Реєструє кожне в ObjectsMapService, надсилає BuildingPlacedSignal.
        /// У режимі знесення — підтверджує всі позначені до знесення об'єкти.
        /// Після Confirm дія незворотна.
        /// </summary>
        void Confirm();

        /// <summary>
        /// Скасувати всю сесію будівництва.
        /// Видаляє всі pending, очищує Redo-стек, надсилає BuildingCancelledSignal.
        /// </summary>
        void Cancel();

        /// <summary>Відмінити останнє розміщення (Ctrl+Z / кнопка Undo).</summary>
        void UndoLast();

        /// <summary>Повернути скасоване розміщення (Ctrl+Y / кнопка Redo).</summary>
        void RedoLast();

        /// <summary>
        /// Увімкнути або вимкнути режим знесення.
        /// В режимі знесення <see cref="TryDemolishAt"/> замість розміщення видаляє будівлю.
        /// </summary>
        void ToggleDemolishMode();

        /// <summary>
        /// Позначити будівлю на позиції до знесення, якщо вона була поставлена гравцем у цій сесії гри.
        /// Надсилає BuildingPreviewChangedSignal(Valid) для preview-підсвітки.
        /// Фактичне знесення і BuildingDemolishedSignal відбуваються під час Confirm().
        /// Будівлі, що існували до початку гри або розміщені не гравцем — не знищуються.
        /// </summary>
        bool TryDemolishAt(Vector2Int position);

        /// <summary>
        /// Повертає словник усіх будівель, підтверджених гравцем у цій сесії.
        /// Ключ — позиція тайлу, значення — buildingId.
        /// </summary>
        IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings();

        /// <summary>
        /// Відновлює будівлю з saved data: реєструє в ObjectsMap, додає до
        /// playerPlacedBuildings і стріляє BuildingPlacedSignal для візуалів.
        /// Не потребує активного режиму будівництва.
        /// </summary>
        void RestoreFromSave(Vector2Int position, string buildingId);

        /// <summary>
        /// Видалити конкретне pending-розміщення за позицією.
        /// Використовується для перебудови шляху стін при drag.
        /// Повертає true якщо pending було успішно видалено.
        /// </summary>
        bool RemovePendingAt(Vector2Int position);

        /// <summary>
        /// Безпосередньо розміщує будівлю від імені фракції, минаючи UI flow.
        /// Не потребує активного режиму будівництва.
        /// Повертає true якщо розміщення успішне (тайл вільний і проходить правило радіусу ратуші/замку).
        /// </summary>
        bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId);

        /// <summary>
        /// Повертає affordability-стан для конкретного pending preview.
        /// </summary>
        bool TryGetPendingPlacementStatus(Vector2Int position, out ConstructionPendingPlacementStatus status);

        /// <summary>
        /// Повертає проєкцію ресурсів поселення для позиції прев'ю з урахуванням усіх pending будівель.
        /// </summary>
        ConstructionResourceProjection GetResourceProjection(Vector2Int position);

        /// <summary>
        /// Останнє детальне повідомлення про дію сервісу будівництва.
        /// </summary>
        string GetLastActionMessage();

        /// <summary>
        /// Знести будівлю від імені фракції (для AI/мережевих команд).
        /// Фракція повинна бути власником будівлі.
        /// Повертає true якщо успішно.
        /// </summary>
        bool TryDemolishByFaction(Vector2Int position, string factionId);

        /// <summary>
        /// Повертає true, якщо будівлю з вказаним ID вже розміщено.
        /// Якщо ownerId задано, перевірка виконується для конкретного власника.
        /// </summary>
        bool HasPlacedBuilding(string buildingId, string ownerId = null);
    }

    /// <summary>
    /// Optional high-frequency mutation batching contract used by interactive
    /// construction gestures. A batch stores at most one Undo snapshot.
    /// </summary>
    public interface IConstructionPendingUndoBatch
    {
        void BeginPendingUndoBatch(string reason = null);
        void EndPendingUndoBatch();
    }

    /// <summary>
    /// Optional PC placement rotation contract. Keeping it separate preserves
    /// compatibility for integrations that only implement IConstructionService.
    /// </summary>
    public interface IConstructionRotationService
    {
        ConstructionRotation SelectedRotation { get; }
        bool RotateSelectedClockwise();
        bool TryGetPendingRotation(
            Vector2Int position,
            out ConstructionRotation rotation);
    }

    /// <summary>
    /// Optional bootstrap query used by construction UI and runtime guards.
    /// </summary>
    public interface IConstructionBootstrapQuery
    {
        bool RequiresInitialCastle(
            string ownerId,
            out string castleBuildingId);

        bool IsCastleBuilding(string buildingId);
    }

    public readonly struct ConstructionSavedPlacement
    {
        public ConstructionSavedPlacement(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            Position = position;
            BuildingId = buildingId;
            OwnerId = ownerId;
            Rotation = rotation;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
        public string OwnerId { get; }
        public ConstructionRotation Rotation { get; }
    }

    public interface IConstructionSaveSnapshotSource
    {
        IReadOnlyList<ConstructionSavedPlacement>
            GetSavedPlacements();
    }

    public interface IConstructionSaveRestorer
    {
        void RestoreFromSave(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0);
    }

    public interface IConstructionModuleStatePersistence
    {
        string StateKey { get; }
        byte[] CaptureState();
        void RestoreState(byte[] payload);
    }

    public interface IConstructionPlacedBuildingDestruction
    {
        bool TryDestroyPlacedBuilding(
            Vector2Int position,
            string cause = null);
    }

    public interface IConstructionRuntimeAuthorityQuery
    {
        bool IsAuthoritativeRuntime { get; }
    }

    public interface IConstructionBuildingOwnershipQuery
    {
        bool TryGetPlacedBuildingOwner(
            Vector2Int position,
            out string ownerId);
    }

    public interface IConstructionGateStateService
    {
        bool IsGateOpen(Vector2Int position);

        bool TrySetGateOpen(
            Vector2Int position,
            bool isOpen,
            out float transitionSeconds,
            out string reason);

        bool CanUnitPassGate(
            Vector2Int position,
            string unitOwnerId,
            out string reason);

        bool TryEnsureOpenForUnit(
            Vector2Int position,
            string unitOwnerId,
            out string reason);
    }

    public interface IConstructionUnitTraversalQuery
    {
        bool CanTraverseOccupiedConstructionCell(
            string unitId,
            Vector2Int position,
            bool openGateIfNeeded,
            out string reason);
    }

    public interface IConstructionUnitGarrisonRuntime
    {
        bool TryEnterGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason);

        bool TryExitGarrison(
            string unitId,
            Vector2Int targetPosition,
            out string reason);

        bool TryRestoreGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason);

        bool TryExitGarrisonNear(
            string unitId,
            Vector2Int origin,
            int maxRadius,
            out Vector2Int targetPosition,
            out string reason);

        bool IsGarrisoned(string unitId);
        string GetUnitOwnerId(string unitId);
    }

    public interface IBuildingGarrisonService
    {
        bool TryGarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            out string reason);

        bool TryUngarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            Vector2Int targetPosition,
            out string reason);

        IReadOnlyList<string> GetGarrisonedUnits(
            Vector2Int buildingPosition);

        bool TryGetGarrisonStatus(
            Vector2Int buildingPosition,
            out int occupied,
            out int capacity);
    }

    /// <summary>
    /// Applies an already host-authorized placement to a replica.
    /// This path never validates affordability or consumes resources and is
    /// idempotent for the same building, owner and origin.
    /// </summary>
    public interface IConfirmedConstructionPlacementApplier
    {
        bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId);
    }

    /// <summary>
    /// Applies an already host-authorized demolition to a replica.
    /// This is a state-convergence path and intentionally does not require the
    /// replica's local owner to be the active faction.
    /// </summary>
    public interface IConfirmedConstructionDemolitionApplier
    {
        bool TryApplyConfirmedDemolition(
            Vector2Int position,
            string ownerId);
    }

    /// <summary>
    /// Immutable placement metadata that must survive an authoritative
    /// client-host-client round trip. Relocation and transactional pending
    /// replacement are placement intent, not properties that may be inferred
    /// from the target cell.
    /// </summary>
    public readonly struct ConstructionPlacementCommitIntent
    {
        public ConstructionPlacementCommitIntent(
            Vector2Int? relocationSourcePosition = null,
            string satisfiedReplacementBuildingId = null,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            RelocationSourcePosition = relocationSourcePosition;
            SatisfiedReplacementBuildingId =
                satisfiedReplacementBuildingId;
            Rotation = ConstructionRotationUtility.Normalize(
                (int)rotation);
        }

        public Vector2Int? RelocationSourcePosition { get; }
        public bool HasRelocationSource =>
            RelocationSourcePosition.HasValue;
        public string SatisfiedReplacementBuildingId { get; }
        public ConstructionRotation Rotation { get; }

        public static ConstructionPlacementCommitIntent None =>
            new ConstructionPlacementCommitIntent();
    }

    /// <summary>
    /// Exposes the authoritative intent attached to a local pending preview
    /// without leaking ConstructionService's internal pending representation.
    /// </summary>
    public interface IConstructionPendingPlacementIntentSource
    {
        bool TryGetPendingPlacementIntent(
            Vector2Int position,
            out ConstructionPlacementCommitIntent intent);
    }

    /// <summary>
    /// Executes a host-authoritative placement. Unlike direct AI placement,
    /// this entry point accepts the exact intent supplied by the requesting
    /// client and validates it against persistent host state.
    /// </summary>
    public interface IAuthoritativeConstructionPlacementExecutor
    {
        bool TryPlaceAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent);
    }

    /// <summary>
    /// Applies a host-confirmed placement intent to a replica without charging
    /// resources. The legacy three-argument applier remains available for
    /// integrations that only support ordinary placements.
    /// </summary>
    public interface IConfirmedConstructionPlacementIntentApplier
    {
        bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent);
    }
}
