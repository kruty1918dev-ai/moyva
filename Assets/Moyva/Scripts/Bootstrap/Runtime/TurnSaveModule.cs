using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Bootstrap.Runtime.TurnSaveModule")]
    internal sealed class TurnSaveModule : IStagedSaveModule
    {
        private const int Magic = unchecked((int)0x5455524E);
        private const int Version = 3;
        private const int LegacyVersion = 1;
        private const int MaxParticipants = 16;

        private readonly ITurnService _turns;
        private readonly ITurnStateRestorer _restorer;
        private readonly ICalendarService _calendar;
        private readonly ICalendarStateRestorer _calendarRestorer;
        private readonly ITurnHistoryQuery _history;
        private readonly ITurnHistoryRestorer _historyRestorer;

        public TurnSaveModule(
            ITurnService turns,
            ITurnStateRestorer restorer,
            ICalendarService calendar,
            [InjectOptional] ICalendarStateRestorer calendarRestorer = null)
        {
            _turns = turns ?? throw new ArgumentNullException(nameof(turns));
            _restorer = restorer ?? throw new ArgumentNullException(nameof(restorer));
            _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
            _calendarRestorer = calendarRestorer
                ?? (_calendar as ICalendarStateRestorer)
                ?? throw new InvalidOperationException(
                    $"Calendar service '{_calendar.GetType().FullName}' must implement {nameof(ICalendarStateRestorer)} for save/load restoration.");
            _history = turns as ITurnHistoryQuery
                ?? throw new InvalidOperationException("Turn service must expose participant history.");
            _historyRestorer = restorer as ITurnHistoryRestorer
                ?? throw new InvalidOperationException("Turn service must restore participant history.");
        }

        public void OnSave(ISaveContext context)
        {
            var history = _history.GetParticipantHistory();
            ValidateState(_turns.Round, _turns.GlobalTurn, _turns.ActiveOwnerId,
                _turns.ActionsThisTurn, _calendar.TotalHoursSinceEpoch);
            ValidateHistory(history, _turns.GlobalTurn, _turns.ActiveOwnerId);
            context.Writer.Write(Magic);
            context.Writer.Write(Version);
            context.Writer.Write(_turns.Round);
            context.Writer.Write(_turns.GlobalTurn);
            context.Writer.Write(_turns.ActiveOwnerId ?? string.Empty);
            context.Writer.Write(_turns.ActionsThisTurn);
            context.Writer.Write(_calendar.TotalHoursSinceEpoch);
            context.Writer.Write(history.Count);
            foreach (var participant in history)
            {
                context.Writer.Write(participant.OwnerId);
                context.Writer.Write(participant.CompletedTurns);
                context.Writer.Write(participant.IsEliminated);
            }
        }

        public void OnLoad(ISaveContext context)
            => PrepareLoad(context)();

        public Action PrepareLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != Magic)
                throw new InvalidDataException("Unsupported turn-state save block magic.");

            int version = context.Reader.ReadInt32();
            if (version < LegacyVersion || version > Version)
                throw new InvalidDataException($"Unsupported turn-state save version {version}.");

            int round = context.Reader.ReadInt32();
            long globalTurn = context.Reader.ReadInt64();
            string activeOwner = context.Reader.ReadString();
            int actions = context.Reader.ReadInt32();

            long calendarHours = version >= 2
                ? context.Reader.ReadInt64()
                : DeriveLegacyCalendarHours(round);

            ValidateState(round, globalTurn, activeOwner, actions, calendarHours);
            List<TurnParticipantHistorySnapshot> history = null;
            if (version >= 3)
            {
                int count = context.Reader.ReadInt32();
                if (count < 1 || count > MaxParticipants)
                    throw new InvalidDataException("Invalid turn participant count.");
                history = new List<TurnParticipantHistorySnapshot>(count);
                for (int index = 0; index < count; index++)
                    history.Add(new TurnParticipantHistorySnapshot(
                        context.Reader.ReadString(), context.Reader.ReadInt64(),
                        false, false, context.Reader.ReadBoolean()));
                ValidateHistory(history, globalTurn, activeOwner);
            }

            if (context.Reader.BaseStream.Position != context.Reader.BaseStream.Length)
                throw new InvalidDataException("Unexpected trailing turn-state save data.");

            return () =>
            {
                // Restore silently so loading does not run another economy tick.
                _calendarRestorer.RestoreByTotalHours(calendarHours);
                _historyRestorer.RestoreHistory(history);
                _restorer.Restore(round, globalTurn, activeOwner, actions);
            };
        }

        public Action PrepareMissingData()
            => () => _historyRestorer.RestoreHistory(null);

        private static void ValidateState(int round, long globalTurn, string owner, int actions, long hours)
        {
            if (round < 1 || globalTurn < 1 || actions < 0 || hours < 0
                || string.IsNullOrWhiteSpace(owner) || owner.Length > 256 || owner != owner.Trim())
                throw new InvalidDataException("Invalid saved turn state.");
        }

        private static void ValidateHistory(
            IReadOnlyList<TurnParticipantHistorySnapshot> history, long globalTurn, string activeOwner)
        {
            if (history.Count < 1 || history.Count > MaxParticipants)
                throw new InvalidDataException("Invalid turn participant count.");
            var owners = new HashSet<string>(StringComparer.Ordinal);
            long total = 0;
            foreach (var item in history)
            {
                if (string.IsNullOrWhiteSpace(item.OwnerId) || item.OwnerId.Length > 256
                    || item.OwnerId != item.OwnerId.Trim() || !owners.Add(item.OwnerId)
                    || item.CompletedTurns < 0 || item.CompletedTurns > globalTurn - 1 - total)
                    throw new InvalidDataException("Invalid saved participant history.");
                total += item.CompletedTurns;
            }
            if (!owners.Contains(activeOwner) || total != globalTurn - 1)
                throw new InvalidDataException("Participant history does not match the saved turn.");
        }

        private long DeriveLegacyCalendarHours(int round)
        {
            int completedRounds = Math.Max(0, round - 1);
            return checked((long)completedRounds * _calendar.Config.HoursPerTurn);
        }
    }
}
