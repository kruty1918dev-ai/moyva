using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Domain;
using Kruty1918.Moyva.Calendar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Calendar
{
    // ====================================================================
    // GameCalendarServiceExtendedTests — 18 tests
    // ====================================================================
    [TestFixture]
    public sealed class GameCalendarServiceExtendedTests
    {
        private CalendarConfig MakeConfig(
            int hoursInDay = 24, int daysInMonth = 30, int monthsInYear = 12,
            int hoursPerTurn = 1, int startYear = 1054, int startMonth = 1,
            int startDay = 1, int startHour = 0,
            int dayStartHour = 6, int nightStartHour = 20,
            int dawnDuration = 2, int duskDuration = 2)
        {
            return new CalendarConfig(
                schemaVersion: CalendarConfig.CurrentSchemaVersion,
                startYear: startYear,
                startMonth: startMonth,
                startDay: startDay,
                startHour: startHour,
                monthsInYear: monthsInYear,
                daysInMonth: daysInMonth,
                hoursInDay: hoursInDay,
                dayStartHour: dayStartHour,
                nightStartHour: nightStartHour,
                dawnDurationHours: dawnDuration,
                duskDurationHours: duskDuration,
                hoursPerTurn: hoursPerTurn);
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new GameCalendarService(null));
        }

        [Test]
        public void Initial_TotalHours_IsZero()
        {
            var svc = new GameCalendarService(MakeConfig());
            Assert.AreEqual(0, svc.TotalHoursSinceEpoch);
        }

        [Test]
        public void Initial_Year_IsStartYear()
        {
            var svc = new GameCalendarService(MakeConfig(startYear: 1054));
            Assert.AreEqual(1054, svc.Current.Year);
        }

        [Test]
        public void SetByTotalHours_Negative_Throws()
        {
            var svc = new GameCalendarService(MakeConfig());
            Assert.Throws<System.ArgumentOutOfRangeException>(() => svc.SetByTotalHours(-1));
        }

        [Test]
        public void SetByTotalHours_Zero_ResetsToStart()
        {
            var svc = new GameCalendarService(MakeConfig());
            svc.AdvanceTurn();
            svc.SetByTotalHours(0);
            Assert.AreEqual(0, svc.TotalHoursSinceEpoch);
        }

        [Test]
        public void AdvanceTurn_MultipleTimes_AccumulatesCorrectly()
        {
            var svc = new GameCalendarService(MakeConfig(hoursPerTurn: 3));
            svc.AdvanceTurn();
            svc.AdvanceTurn();
            Assert.AreEqual(6, svc.TotalHoursSinceEpoch);
        }

        [Test]
        public void AdvanceTurn_24Turns_AdvancesOneDay()
        {
            var cfg = MakeConfig(hoursInDay: 24, hoursPerTurn: 1, startDay: 1);
            var svc = new GameCalendarService(cfg);
            for (int i = 0; i < 24; i++) svc.AdvanceTurn();
            Assert.AreEqual(2, svc.Current.Day);
        }

        [Test]
        public void AdvanceTurn_30Days_AdvancesOneMonth()
        {
            var cfg = MakeConfig(hoursInDay: 24, daysInMonth: 30, hoursPerTurn: 24, startMonth: 1);
            var svc = new GameCalendarService(cfg);
            for (int i = 0; i < 30; i++) svc.AdvanceTurn();
            Assert.AreEqual(2, svc.Current.Month);
        }

        [Test]
        public void AdvanceTurn_FullYear_AdvancesYear()
        {
            var cfg = MakeConfig(hoursInDay: 24, daysInMonth: 30, monthsInYear: 12, hoursPerTurn: 24, startYear: 1054);
            var svc = new GameCalendarService(cfg);
            for (int i = 0; i < 360; i++) svc.AdvanceTurn();
            Assert.AreEqual(1055, svc.Current.Year);
        }

        [Test]
        public void OnHourChanged_FiresOnEveryAdvance()
        {
            var svc = new GameCalendarService(MakeConfig());
            int count = 0;
            svc.OnHourChanged += () => count++;
            svc.AdvanceTurn();
            svc.AdvanceTurn();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void OnDayChanged_DoesNotFire_WhenSameDay()
        {
            var svc = new GameCalendarService(MakeConfig(hoursPerTurn: 1));
            int count = 0;
            svc.OnDayChanged += () => count++;
            svc.AdvanceTurn();
            Assert.AreEqual(0, count);
        }

        [Test]
        public void OnDayChanged_FiresOnDayBoundary()
        {
            var cfg = MakeConfig(hoursInDay: 24, hoursPerTurn: 24);
            var svc = new GameCalendarService(cfg);
            int count = 0;
            svc.OnDayChanged += () => count++;
            svc.AdvanceTurn();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnMonthChanged_Fires_WhenMonthRollsOver()
        {
            var cfg = MakeConfig(hoursInDay: 24, daysInMonth: 1, hoursPerTurn: 24);
            var svc = new GameCalendarService(cfg);
            int count = 0;
            svc.OnMonthChanged += () => count++;
            svc.AdvanceTurn();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnYearChanged_Fires_WhenYearRollsOver()
        {
            var cfg = MakeConfig(hoursInDay: 24, daysInMonth: 1, monthsInYear: 1, hoursPerTurn: 24);
            var svc = new GameCalendarService(cfg);
            int count = 0;
            svc.OnYearChanged += () => count++;
            svc.AdvanceTurn();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void OnDayPhaseChanged_Fires_WhenPhaseChanges()
        {
            var cfg = MakeConfig(hoursInDay: 24, hoursPerTurn: 6, startHour: 0,
                dayStartHour: 6, nightStartHour: 20, dawnDuration: 2, duskDuration: 2);
            var svc = new GameCalendarService(cfg);
            DayPhase? changed = null;
            svc.OnDayPhaseChanged += p => changed = p;
            svc.AdvanceTurn(); // 0 → 6: Night → Day (crosses dawn)
            Assert.IsNotNull(changed);
        }

        [Test]
        public void ComputeDayPhase_Day_InDayRange()
        {
            var cfg = MakeConfig(dayStartHour: 6, nightStartHour: 20, dawnDuration: 2, duskDuration: 2);
            Assert.AreEqual(DayPhase.Day, GameCalendarService.ComputeDayPhase(cfg, 10));
        }

        [Test]
        public void ComputeDayPhase_Dawn_AtBoundary()
        {
            var cfg = MakeConfig(dayStartHour: 6, dawnDuration: 2);
            Assert.AreEqual(DayPhase.Dawn, GameCalendarService.ComputeDayPhase(cfg, 4));
        }

        [Test]
        public void ComputeDateTime_LargeValue_DoesNotOverflow()
        {
            var cfg = MakeConfig();
            var dt = GameCalendarService.ComputeDateTime(cfg, 100000);
            Assert.Greater(dt.Year, 1054);
        }
    }

    // ====================================================================
    // DayPhaseEnumTests — 4 tests
    // ====================================================================
    [TestFixture]
    public sealed class DayPhaseEnumTests
    {
        [Test]
        public void Night_IsZero() => Assert.AreEqual(0, (int)DayPhase.Night);

        [Test]
        public void Dawn_IsOne() => Assert.AreEqual(1, (int)DayPhase.Dawn);

        [Test]
        public void Day_IsTwo() => Assert.AreEqual(2, (int)DayPhase.Day);

        [Test]
        public void Dusk_IsThree() => Assert.AreEqual(3, (int)DayPhase.Dusk);
    }

    // ====================================================================
    // GameDateTimeTests — 6 tests
    // ====================================================================
    [TestFixture]
    public sealed class GameDateTimeTests
    {
        [Test]
        public void Constructor_SetsAllFields()
        {
            var dt = new GameDateTime(1054, 3, 15, 8);
            Assert.AreEqual(1054, dt.Year);
            Assert.AreEqual(3, dt.Month);
            Assert.AreEqual(15, dt.Day);
            Assert.AreEqual(8, dt.Hour);
        }

        [Test]
        public void Default_AllZeros()
        {
            var dt = default(GameDateTime);
            Assert.AreEqual(0, dt.Year);
            Assert.AreEqual(0, dt.Month);
            Assert.AreEqual(0, dt.Day);
            Assert.AreEqual(0, dt.Hour);
        }

        [Test]
        public void Equals_SameValues_True()
        {
            var a = new GameDateTime(1054, 1, 1, 0);
            var b = new GameDateTime(1054, 1, 1, 0);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void NotEquals_DifferentHour()
        {
            var a = new GameDateTime(1054, 1, 1, 0);
            var b = new GameDateTime(1054, 1, 1, 1);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void NotEquals_DifferentYear()
        {
            var a = new GameDateTime(1054, 1, 1, 0);
            var b = new GameDateTime(1055, 1, 1, 0);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void ToString_ContainsYear()
        {
            var dt = new GameDateTime(1054, 1, 1, 0);
            Assert.IsTrue(dt.ToString().Contains("1054"));
        }
    }
}
