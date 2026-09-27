using System;
using Kruty1918.Calendar.Config;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests
{
    /// <summary>
    /// C11 — sandbox progress clock advances in measured units: accumulated
    /// deltaTime × speed crosses SandboxRoundSeconds per tick, large frames
    /// emit multiple ticks, timeScale=0 freezes, and TurnBased mode never
    /// advances the calendar on its own.
    /// </summary>
    public sealed class C11AcceptanceTests
    {
        private sealed class FakeCalendar : ICalendarService
        {
            public int AdvanceCount;
            public long TotalHours;

            public GameDateTime Current => default;
            public long TotalHoursSinceEpoch => TotalHours;
            public DayPhase CurrentDayPhase => default;
            public CalendarConfig Config { get; } = new CalendarConfig(
                schemaVersion: 1,
                startYear: 1, startMonth: 1, startDay: 1, startHour: 0,
                monthsInYear: 12, daysInMonth: 30, hoursInDay: 24,
                dayStartHour: 6, nightStartHour: 20,
                dawnDurationHours: 1, duskDurationHours: 1,
                hoursPerTurn: 1);

            public event Action OnHourChanged { add { } remove { } }
            public event Action OnDayChanged { add { } remove { } }
            public event Action OnMonthChanged { add { } remove { } }
            public event Action OnYearChanged { add { } remove { } }
            public event Action<DayPhase> OnDayPhaseChanged { add { } remove { } }

            public void AdvanceTurn()
            {
                AdvanceCount++;
                TotalHours += Config.HoursPerTurn;
            }

            public void SetByTotalHours(long totalHours)
                => TotalHours = totalHours;
        }

        private static GameplayProgressClock Clock(
            FakeCalendar calendar, float roundSeconds = 10f, float speed = 1f)
        {
            var clock = new GameplayProgressClock(calendar);
            clock.Configure(
                GameplayProgressMode.SandboxRealtime,
                roundSeconds,
                speed);
            return clock;
        }

        [Test]
        public void OneX_TicksEverySandboxRoundSecond()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar, roundSeconds: 10f, speed: 1f);

            clock.AdvanceRealtime(9.9f);
            Assert.AreEqual(0, calendar.AdvanceCount);

            clock.AdvanceRealtime(0.2f);
            Assert.AreEqual(
                1, calendar.AdvanceCount,
                "10.1 accumulated seconds crosses one 10s round");
        }

        [Test]
        public void TwoX_TicksEveryHalfRound()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar, roundSeconds: 10f, speed: 2f);

            clock.AdvanceRealtime(5f);

            Assert.AreEqual(
                1, calendar.AdvanceCount,
                "2X speed turns a 10s round into a 5s tick");
        }

        [Test]
        public void LargeFrame_EmitsOneTickPerCrossedRound()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar, roundSeconds: 10f, speed: 1f);

            clock.AdvanceRealtime(35f);

            Assert.AreEqual(
                3, calendar.AdvanceCount,
                "35s at 1X crosses exactly three 10s rounds");
            Assert.LessOrEqual(
                clock.SecondsUntilNextProgress, 10f,
                "The remainder stays buffered for the next round");
        }

        [Test]
        public void ProgressedEvent_CarriesSequenceAndMode()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar, roundSeconds: 10f, speed: 1f);
            int events = 0;
            GameplayProgressTick last = default;
            clock.Progressed += tick => { events++; last = tick; };

            clock.AdvanceRealtime(25f);

            Assert.AreEqual(2, events);
            Assert.AreEqual(GameplayProgressMode.SandboxRealtime, last.Mode);
            Assert.Greater(last.Sequence, 0);
        }

        [Test]
        public void TurnBasedMode_NeverSelfAdvancesCalendar()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar);
            clock.Configure(GameplayProgressMode.TurnBased, 10f, 1f);

            clock.AdvanceRealtime(600f);

            Assert.AreEqual(
                0, calendar.AdvanceCount,
                "In turn-based mode the authoritative turn loop, not the " +
                "clock, owns calendar advancement");
        }

        [Test]
        public void Configure_ResetsAccumulator()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar, roundSeconds: 10f, speed: 1f);
            clock.AdvanceRealtime(9f);

            clock.Configure(GameplayProgressMode.SandboxRealtime, 10f, 1f);
            clock.AdvanceRealtime(2f);

            Assert.AreEqual(
                0, calendar.AdvanceCount,
                "Reconfiguration must not carry stale partial progress");
        }

        [Test]
        public void SetSpeed_Clamped_ToSupportedRange()
        {
            var calendar = new FakeCalendar();
            var clock = Clock(calendar);

            clock.SetSpeed(0f);
            Assert.AreEqual(0.1f, clock.Speed, 0.0001f);
            clock.SetSpeed(100f);
            Assert.AreEqual(8f, clock.Speed, 0.0001f);
        }
    }
}
