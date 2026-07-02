using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionSpawnSetupService
    {
        bool TryPrepareStartingPositions(WorldGeneratedDataSignal signal);
    }

    internal sealed class StartingPositionSpawnSetupService
        : IStartingPositionSpawnSetupService
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly IStartingPositionSelector _selector;
        private readonly IStartingPositionAssignmentFactory _assignmentFactory;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionState _startingPositionState;
        private readonly SignalBus _signalBus;

        public StartingPositionSpawnSetupService(
            IStartingPositionSelector selector,
            IStartingPositionAssignmentFactory assignmentFactory,
            IStartingPositionPolicy policy,
            IStartingPositionState startingPositionState,
            SignalBus signalBus)
        {
            _selector = selector;
            _assignmentFactory = assignmentFactory;
            _policy = policy;
            _startingPositionState = startingPositionState;
            _signalBus = signalBus;
        }

        public bool TryPrepareStartingPositions(WorldGeneratedDataSignal signal)
        {
            int participantCount = _policy.Participants?.Count ?? 0;
            int requestedPlayerCount = _policy.ResolveStartPositionCount();
            bool shouldCompute = _policy.ShouldComputeHostStartPositions();
            bool stateAlreadySet = _startingPositionState.IsSet;
            bool willCallSelector = shouldCompute && !stateAlreadySet;
            int launchExtraSlots = ResolveLaunchExtraSlotsEquivalent(participantCount);
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Debug.Log($"{DirectDiagTag} SpawnSetup.ENTER map={signal.Width}x{signal.Height}, mode={GameLaunchContext.Mode}, maxPlayers={GameLaunchContext.MaxPlayers}, participants={participantCount}, requestedPlayerCount={requestedPlayerCount}, launchExtraSlots={launchExtraSlots}, hasSelector={_selector != null}, hasFactory={_assignmentFactory != null}, hasState={_startingPositionState != null}, hasSignalBus={_signalBus != null}.");

            Debug.Log(
                $"{PolicyDiagTag} SpawnSetup ENTER map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}, mode={GameLaunchContext.Mode}, " +
                $"maxPlayers={GameLaunchContext.MaxPlayers}, launchExtraSlots={launchExtraSlots}, participants={participantCount}, " +
                $"requestedPlayerCount={requestedPlayerCount}, shouldCompute={shouldCompute}, stateAlreadySet={stateAlreadySet}, willCallSelector={willCallSelector}.");

            if (!willCallSelector)
            {
                Debug.Log($"{DirectDiagTag} SpawnSetup.EXIT result=false reason={(shouldCompute ? "state-already-set" : "selector-not-requested")}.");
                Debug.Log(
                    $"{PolicyDiagTag} SpawnSetup RESULT selected=0, assignments={_startingPositionState.SpawnAssignments.Count}, stateSet={_startingPositionState.IsSet}, " +
                    $"stateSetCalled=false, signalFired=false, skipped=true, shouldCompute={shouldCompute}, stateAlreadySet={stateAlreadySet}.");
                return false;
            }

            Debug.Log($"{DirectDiagTag} SpawnSetup.CALL Selector.SelectStartPositions count={requestedPlayerCount}, map={baseMapSize.x}x{baseMapSize.y}.");
            Debug.Log($"{WorldGenDiagTag} SpawnSetup.CALL selector frame={Time.frameCount}, requested={requestedPlayerCount}, map={baseMapSize.x}x{baseMapSize.y}");
            List<Vector2Int> startPositions = _selector.PickStartingPositions(signal, requestedPlayerCount);
            Debug.Log($"{DirectDiagTag} SpawnSetup.Selector.RESULT selected={startPositions.Count}, positions={FormatPositions(startPositions)}.");
            Vector2Int startPos = startPositions.Count > 0
                ? startPositions[0]
                : _selector.PickStartingPosition(baseMapSize);
            startPos = StartingPositionMapUtility.ClampToMap(startPos, baseMapSize.x, baseMapSize.y);
            Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic picked start count={startPositions.Count}, chosen={startPos}, map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}.");

            bool stateSetCalled = false;
            if (startPositions.Count > 0)
            {
                stateSetCalled = true;
                SpawnPositionAssignment[] assignments = _assignmentFactory.BuildSpawnAssignments(
                    startPositions,
                    _policy.Participants,
                    _policy.ResolveLocalPlayerId(),
                    GameLaunchContext.HasWorldSettings,
                    GameLaunchContext.MaxPlayers);
                Debug.Log($"{DirectDiagTag} SpawnSetup.Assignments.RESULT assignments={assignments.Length}, localPlayerId={_policy.ResolveLocalPlayerId()}, assignmentsShort={FormatAssignments(assignments)}.");
                _startingPositionState.Set(assignments);
            }
            else
            {
                stateSetCalled = true;
                _startingPositionState.Set(startPos);
            }
            Debug.Log($"{DirectDiagTag} SpawnSetup.State.SET startStateSet={_startingPositionState.IsSet}, startPosition={_startingPositionState.StartPosition}, playerStarts={_startingPositionState.PlayerStartPositions.Count}.");
            Debug.Log($"{WorldGenDiagTag} SpawnSetup.RESULT selected={startPositions.Count}, assignments={_startingPositionState.SpawnAssignments.Count}");

            bool signalFired = false;
            if (_startingPositionState.SpawnAssignments.Count > 0)
            {
                signalFired = true;
                Debug.Log($"{WorldGenDiagTag} Signal.FIRE WorldSpawnPositionsSignal source=new-game assignments={_startingPositionState.SpawnAssignments.Count}, frame={Time.frameCount}");
                Debug.Log($"{DirectDiagTag} SpawnSetup.Signal.FIRE WorldSpawnPositionsSignal assignments={_startingPositionState.SpawnAssignments.Count}.");
                _signalBus.Fire(new WorldSpawnPositionsSignal
                {
                    Assignments = _assignmentFactory.CopySpawnAssignments(_startingPositionState.SpawnAssignments),
                });
                Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldSpawnPositionsSignal source=new-game frame={Time.frameCount}");
            }

            Debug.Log(
                $"{PolicyDiagTag} SpawnSetup RESULT selected={startPositions.Count}, assignments={_startingPositionState.SpawnAssignments.Count}, " +
                $"stateSet={_startingPositionState.IsSet}, stateSetCalled={stateSetCalled}, signalFired={signalFired}, chosen={startPos}.");
            Debug.Log($"{DirectDiagTag} SpawnSetup.EXIT result=true signalFired={signalFired}.");

            return true;
        }

        private static string FormatPositions(IReadOnlyList<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return "[]";

            int count = Mathf.Min(positions.Count, 4);
            var parts = new string[count];
            for (int index = 0; index < count; index++)
                parts[index] = positions[index].ToString();

            return positions.Count > count
                ? $"[{string.Join(", ", parts)}, ...]"
                : $"[{string.Join(", ", parts)}]";
        }

        private static string FormatAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            if (assignments == null || assignments.Count == 0)
                return "[]";

            int count = Mathf.Min(assignments.Count, 4);
            var parts = new string[count];
            for (int index = 0; index < count; index++)
            {
                SpawnPositionAssignment assignment = assignments[index];
                parts[index] = $"#{assignment.SlotIndex}:{assignment.ParticipantId}@{assignment.Position}{(assignment.IsBot ? ":bot" : string.Empty)}";
            }

            return assignments.Count > count
                ? $"[{string.Join(", ", parts)}, ...]"
                : $"[{string.Join(", ", parts)}]";
        }

        private static int ResolveLaunchExtraSlotsEquivalent(int participantCount)
        {
            int normalizedParticipantCount = Mathf.Max(1, participantCount);
            return Mathf.Max(0, GameLaunchContext.MaxPlayers - normalizedParticipantCount);
        }
    }
}
