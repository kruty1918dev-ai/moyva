using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Observes, but never drives, the first playable bot vertical slice:
    /// Castle -> economy -> recruitment -> ready unit -> deployed unit.
    ///
    /// This is diagnostics only. It cannot build, recruit or deploy anything.
    /// </summary>
    internal sealed class BotVerticalSliceDiagnostics
    {
        [Flags]
        private enum Stage
        {
            None = 0,
            Castle = 1 << 0,
            Warehouse = 1 << 1,
            Production = 1 << 2,
            Recruitment = 1 << 3,
            RecruitReady = 1 << 4,
            UnitDeployed = 1 << 5,
        }

        private readonly IBuildingRegistry _buildings;
        private readonly IBotReasoningTrace _reasoning;

        private readonly Dictionary<string, Stage> _seen =
            new(StringComparer.Ordinal);

        [Inject]
        public BotVerticalSliceDiagnostics(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IBotReasoningTrace reasoning = null)
        {
            _buildings = buildings;
            _reasoning = reasoning;
        }

        public void Observe(
            BotWorldSnapshot snapshot)
        {
            if (snapshot == null ||
                string.IsNullOrWhiteSpace(snapshot.OwnerId))
            {
                return;
            }

            Stage current =
                ResolveStage(snapshot);

            _seen.TryGetValue(
                snapshot.OwnerId,
                out Stage previous);

            Stage newlyReached =
                current & ~previous;

            if (newlyReached == Stage.None)
                return;

            _seen[snapshot.OwnerId] =
                previous | current;

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.Castle,
                "Vertical Slice: Castle visible in snapshot",
                $"OwnBuildings={snapshot.OwnBuildings.Count}. " +
                "The capital mutation is now observable by BotWorldSnapshot; " +
                "Opening must no longer behave as 'no infrastructure'.");

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.Warehouse,
                "Vertical Slice: storage capability established",
                $"OwnBuildings={snapshot.OwnBuildings.Count}.");

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.Production,
                "Vertical Slice: industrial production established",
                $"OwnBuildings={snapshot.OwnBuildings.Count}.");

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.Recruitment,
                "Vertical Slice: recruitment capability established",
                $"OwnBuildings={snapshot.OwnBuildings.Count}.");

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.RecruitReady,
                "Vertical Slice: recruited unit ready",
                $"ReadyRecruitmentItems={snapshot.ReadyRecruitmentItems.Count}.");

            LogIfReached(
                snapshot,
                newlyReached,
                Stage.UnitDeployed,
                "Vertical Slice: army deployed",
                $"OwnUnits={snapshot.OwnUnits.Count}. " +
                "Castle -> economy/recruitment -> recruit -> deploy is observable.");
        }

        private Stage ResolveStage(
            BotWorldSnapshot snapshot)
        {
            Stage stage = Stage.None;

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BotBuildingSnapshot building =
                    snapshot.OwnBuildings[i];

                BuildingDefinition definition =
                    _buildings?.GetById(
                        building.BuildingId);

                if (definition == null)
                    continue;

                if (BuildingDefinitionCapabilities
                    .IsCastle(definition))
                {
                    stage |= Stage.Castle;
                }

                if (BuildingDefinitionCapabilities
                    .IsWarehouse(definition))
                {
                    stage |= Stage.Warehouse;
                }

                if (!string.IsNullOrWhiteSpace(
                        BuildingDefinitionCapabilities
                            .GetIndustrialResourceId(
                                definition)))
                {
                    stage |= Stage.Production;
                }

                if (BuildingDefinitionCapabilities
                    .HasEnabledModule<
                        UnitRecruitmentBuildingModule>(
                            definition))
                {
                    stage |= Stage.Recruitment;
                }
            }

            if (snapshot.ReadyRecruitmentItems.Count > 0)
                stage |= Stage.RecruitReady;

            if (snapshot.OwnUnits.Count > 0)
                stage |= Stage.UnitDeployed;

            return stage;
        }

        private void LogIfReached(
            BotWorldSnapshot snapshot,
            Stage newlyReached,
            Stage expected,
            string headline,
            string narrative)
        {
            if ((newlyReached & expected) == 0)
                return;

            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.Observation,
                headline,
                narrative);
        }
    }
}
