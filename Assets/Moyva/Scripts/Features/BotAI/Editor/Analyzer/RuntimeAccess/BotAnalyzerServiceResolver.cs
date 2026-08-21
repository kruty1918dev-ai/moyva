using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerServices
    {
        public SceneContext Context;
        public DiContainer Container;

        public ITurnService Turns;
        public IBotWorldSnapshotBuilder SnapshotBuilder;
        public IBotGoalStore Goals;
        public IBotDecisionTrace Trace;
        public IBotStrategicStateStore StrategicState;
        public IBotReasoningTrace Reasoning;
        public IBotPerceptionService Perception;

        public IFogOfWarServiceRegistry FogRegistry;
        public IGridService Grid;
        public IGridProjection GridProjection;

        public IEconomyInfoMediator Economy;

        public IUnitService Units;
        public IUnitOwnershipQuery Ownership;
        public IUnitRecruitmentService Recruitment;
        public IUnitRecruitmentStateStore RecruitmentState;
        public IUnitClassConfig UnitConfigs;
        public IBotUnitRoleResolver RoleResolver;

        public IConstructionService Construction;
        public IConstructionSaveSnapshotSource ConstructionSnapshot;
        public IConstructionLifecycle ConstructionLifecycle;

        public bool CoreReady =>
            Context != null &&
            Container != null &&
            Turns != null &&
            SnapshotBuilder != null;

        public string BuildAvailabilitySummary()
        {
            var missing = new List<string>();
            AddIfNull(missing, Turns, nameof(ITurnService));
            AddIfNull(missing, SnapshotBuilder, nameof(IBotWorldSnapshotBuilder));
            AddIfNull(missing, Goals, nameof(IBotGoalStore));
            AddIfNull(missing, Trace, nameof(IBotDecisionTrace));
            AddIfNull(missing, StrategicState, nameof(IBotStrategicStateStore));
            AddIfNull(missing, Reasoning, nameof(IBotReasoningTrace));
            AddIfNull(missing, Perception, nameof(IBotPerceptionService));
            AddIfNull(missing, FogRegistry, nameof(IFogOfWarServiceRegistry));
            AddIfNull(missing, Grid, nameof(IGridService));
            AddIfNull(missing, GridProjection, nameof(IGridProjection));
            AddIfNull(missing, Economy, nameof(IEconomyInfoMediator));
            AddIfNull(missing, Units, nameof(IUnitService));
            AddIfNull(missing, Ownership, nameof(IUnitOwnershipQuery));
            AddIfNull(missing, Recruitment, nameof(IUnitRecruitmentService));
            AddIfNull(missing, RecruitmentState, nameof(IUnitRecruitmentStateStore));
            AddIfNull(missing, ConstructionSnapshot, nameof(IConstructionSaveSnapshotSource));
            AddIfNull(missing, ConstructionLifecycle, nameof(IConstructionLifecycle));

            return missing.Count == 0
                ? "All analyzer read services resolved."
                : "Optional/read services unavailable: " + string.Join(", ", missing);
        }

        private static void AddIfNull<T>(List<string> target, T value, string name) where T : class
        {
            if (value == null)
                target.Add(name);
        }
    }

    internal sealed class BotAnalyzerServiceResolver
    {
        public bool TryResolve(out BotAnalyzerServices services, out string reason)
        {
            services = null;
            reason = null;

            SceneContext[] contexts = Resources.FindObjectsOfTypeAll<SceneContext>();
            if (contexts == null || contexts.Length == 0)
            {
                reason = "No active Zenject SceneContext was found.";
                return false;
            }

            Array.Sort(contexts, CompareContext);

            for (int i = 0; i < contexts.Length; i++)
            {
                SceneContext context = contexts[i];
                if (!IsUsable(context))
                    continue;

                DiContainer container = null;
                try
                {
                    container = context.Container;
                }
                catch
                {
                    continue;
                }

                if (container == null)
                    continue;

                ITurnService turns = TryResolve<ITurnService>(container);
                IBotWorldSnapshotBuilder snapshots = TryResolve<IBotWorldSnapshotBuilder>(container);
                if (turns == null || snapshots == null)
                    continue;

                services = Build(context, container, turns, snapshots);
                reason = null;
                return true;
            }

            reason = "SceneContext exists, but no loaded context contains ITurnService + IBotWorldSnapshotBuilder.";
            return false;
        }

        private static BotAnalyzerServices Build(
            SceneContext context,
            DiContainer container,
            ITurnService turns,
            IBotWorldSnapshotBuilder snapshots)
        {
            IConstructionService construction = TryResolve<IConstructionService>(container);

            return new BotAnalyzerServices
            {
                Context = context,
                Container = container,

                Turns = turns,
                SnapshotBuilder = snapshots,
                Goals = TryResolve<IBotGoalStore>(container),
                Trace = TryResolve<IBotDecisionTrace>(container),
                StrategicState = TryResolve<IBotStrategicStateStore>(container),
                Reasoning = TryResolve<IBotReasoningTrace>(container),
                Perception = TryResolve<IBotPerceptionService>(container),

                FogRegistry = TryResolve<IFogOfWarServiceRegistry>(container),
                Grid = TryResolve<IGridService>(container),
                GridProjection = TryResolve<IGridProjection>(container),

                Economy = TryResolve<IEconomyInfoMediator>(container),

                Units = TryResolve<IUnitService>(container),
                Ownership = TryResolve<IUnitOwnershipQuery>(container),
                Recruitment = TryResolve<IUnitRecruitmentService>(container),
                RecruitmentState = TryResolve<IUnitRecruitmentStateStore>(container),
                UnitConfigs = TryResolve<IUnitClassConfig>(container),
                RoleResolver = TryResolve<IBotUnitRoleResolver>(container),

                Construction = construction,
                ConstructionSnapshot =
                    construction as IConstructionSaveSnapshotSource ??
                    TryResolve<IConstructionSaveSnapshotSource>(container),
                ConstructionLifecycle = TryResolve<IConstructionLifecycle>(container),
            };
        }

        private static T TryResolve<T>(DiContainer container) where T : class
        {
            if (container == null)
                return null;

            try
            {
                return container.TryResolve<T>();
            }
            catch
            {
                return null;
            }
        }

        private static bool IsUsable(SceneContext context)
        {
            if (context == null || context.gameObject == null)
                return false;

            var scene = context.gameObject.scene;
            return scene.IsValid() && scene.isLoaded;
        }

        private static int CompareContext(SceneContext left, SceneContext right)
        {
            bool leftActive = left != null && left.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            bool rightActive = right != null && right.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (leftActive != rightActive)
                return leftActive ? -1 : 1;

            string leftName = left != null ? left.gameObject.scene.name : string.Empty;
            string rightName = right != null ? right.gameObject.scene.name : string.Empty;
            return string.CompareOrdinal(leftName, rightName);
        }
    }
}
