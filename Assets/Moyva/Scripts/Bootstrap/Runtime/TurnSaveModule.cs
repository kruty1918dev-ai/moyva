using System;
using System.IO;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class TurnSaveModule : ISaveModule
    {
        private const int Magic = unchecked((int)0x5455524E);
        private const int Version = 2;
        private const int LegacyVersion = 1;

        private readonly ITurnService _turns;
        private readonly ITurnStateRestorer _restorer;
        private readonly ICalendarService _calendar;
        private readonly ICalendarStateRestorer _calendarRestorer;

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
        }

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(Magic);
            context.Writer.Write(Version);
            context.Writer.Write(_turns.Round);
            context.Writer.Write(_turns.GlobalTurn);
            context.Writer.Write(_turns.ActiveOwnerId ?? string.Empty);
            context.Writer.Write(_turns.ActionsThisTurn);
            context.Writer.Write(_calendar.TotalHoursSinceEpoch);
        }

        public void OnLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != Magic)
                throw new InvalidDataException("Unsupported turn-state save block magic.");

            int version = context.Reader.ReadInt32();
            if (version != LegacyVersion && version != Version)
                throw new InvalidDataException($"Unsupported turn-state save version {version}.");

            int round = context.Reader.ReadInt32();
            long globalTurn = context.Reader.ReadInt64();
            string activeOwner = context.Reader.ReadString();
            int actions = context.Reader.ReadInt32();

            long calendarHours = version >= Version
                ? context.Reader.ReadInt64()
                : DeriveLegacyCalendarHours(round);

            // Calendar restoration is deliberately silent. Economy listens to live calendar
            // hour changes, so publishing here would replay a full economy tick during load.
            _calendarRestorer.RestoreByTotalHours(calendarHours);

            // TurnService owns world/faction restore ordering and resumes without replaying
            // participant OnTurnStarted side effects.
            _restorer.Restore(round, globalTurn, activeOwner, actions);
        }

        private long DeriveLegacyCalendarHours(int round)
        {
            int completedRounds = Math.Max(0, round - 1);
            return checked((long)completedRounds * _calendar.Config.HoursPerTurn);
        }
    }
}
