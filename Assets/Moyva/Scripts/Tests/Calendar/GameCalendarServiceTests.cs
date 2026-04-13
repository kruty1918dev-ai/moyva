using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Domain;
using Kruty1918.Moyva.Calendar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Calendar
{
    [TestFixture]
    public class GameCalendarServiceTests
    {
        private static CalendarConfig DefaultConfig() => CalendarConfig.Default();

        // ------------------------------------------------------------------ ComputeDateTime

        [Test]
        public void ComputeDateTime_AtEpoch_ReturnsStartDate()
        {
            var cfg = DefaultConfig();
            var dt  = GameCalendarService.ComputeDateTime(cfg, 0);

            Assert.AreEqual(cfg.StartYear,  dt.Year);
            Assert.AreEqual(cfg.StartMonth, dt.Month);
            Assert.AreEqual(cfg.StartDay,   dt.Day);
            Assert.AreEqual(cfg.StartHour,  dt.Hour);
        }

        [Test]
        public void ComputeDateTime_After24Hours_AdvancesOneDay()
        {
            var cfg = DefaultConfig();            // hoursInDay = 24, startHour = 6
            var dt  = GameCalendarService.ComputeDateTime(cfg, 24);

            // After 24 hours from startHour=6: hour = (24+6) % 24 = 6; day +1
            Assert.AreEqual(cfg.StartHour, dt.Hour);
            Assert.AreEqual(cfg.StartDay + 1, dt.Day);
        }

        [Test]
        public void ComputeDateTime_AfterFullMonth_AdvancesMonth()
        {
            var cfg = DefaultConfig();            // 30 days * 24 h = 720 hours
            long hoursInMonth = cfg.DaysInMonth * cfg.HoursInDay;
            var dt = GameCalendarService.ComputeDateTime(cfg, hoursInMonth);

            Assert.AreEqual(cfg.StartMonth + 1, dt.Month);
        }

        [Test]
        public void ComputeDateTime_AfterFullYear_AdvancesYear()
        {
            var cfg = DefaultConfig();
            long hoursInYear = (long)cfg.MonthsInYear * cfg.DaysInMonth * cfg.HoursInDay;
            var dt = GameCalendarService.ComputeDateTime(cfg, hoursInYear);

            Assert.AreEqual(cfg.StartYear + 1, dt.Year);
        }

        // ------------------------------------------------------------------ ComputeDayPhase

        [Test]
        public void ComputeDayPhase_AtMidnight_IsNight()
        {
            var cfg   = DefaultConfig();     // DayStart=6, NightStart=20, Dawn=1, Dusk=1
            var phase = GameCalendarService.ComputeDayPhase(cfg, 0);
            Assert.AreEqual(DayPhase.Night, phase);
        }

        [Test]
        public void ComputeDayPhase_AtDawnStart_IsDawn()
        {
            var cfg   = DefaultConfig();     // dawnStart = 6-1 = 5
            var phase = GameCalendarService.ComputeDayPhase(cfg, 5);
            Assert.AreEqual(DayPhase.Dawn, phase);
        }

        [Test]
        public void ComputeDayPhase_AtDayStart_IsDay()
        {
            var cfg   = DefaultConfig();
            var phase = GameCalendarService.ComputeDayPhase(cfg, cfg.DayStartHour);
            Assert.AreEqual(DayPhase.Day, phase);
        }

        [Test]
        public void ComputeDayPhase_AtDusk_IsDusk()
        {
            var cfg   = DefaultConfig();     // duskStart = 20-1 = 19
            var phase = GameCalendarService.ComputeDayPhase(cfg, 19);
            Assert.AreEqual(DayPhase.Dusk, phase);
        }

        [Test]
        public void ComputeDayPhase_AtNightStart_IsNight()
        {
            var cfg   = DefaultConfig();
            var phase = GameCalendarService.ComputeDayPhase(cfg, cfg.NightStartHour);
            Assert.AreEqual(DayPhase.Night, phase);
        }

        // ------------------------------------------------------------------ Service lifecycle

        [Test]
        public void AdvanceTurn_IncrementsTotalHours_ByHoursPerTurn()
        {
            var svc = new GameCalendarService(DefaultConfig());
            svc.AdvanceTurn();

            Assert.AreEqual(DefaultConfig().HoursPerTurn, svc.TotalHoursSinceEpoch);
        }

        [Test]
        public void AdvanceTurn_FiresOnHourChanged()
        {
            var svc   = new GameCalendarService(DefaultConfig());
            bool fired = false;
            svc.OnHourChanged += () => fired = true;

            svc.AdvanceTurn();

            Assert.IsTrue(fired);
        }

        [Test]
        public void AdvanceTurn_FiresOnDayChanged_AfterFullDay()
        {
            var cfg = DefaultConfig();
            var svc = new GameCalendarService(cfg);
            bool dayFired = false;
            svc.OnDayChanged += () => dayFired = true;

            // Advance enough turns to cross a day boundary
            long turnsNeeded = cfg.HoursInDay / cfg.HoursPerTurn;
            for (int i = 0; i < turnsNeeded; i++)
                svc.AdvanceTurn();

            Assert.IsTrue(dayFired);
        }

        [Test]
        public void SetByTotalHours_UpdatesCurrentDateTime()
        {
            var cfg = DefaultConfig();
            var svc = new GameCalendarService(cfg);

            long hoursInYear = (long)cfg.MonthsInYear * cfg.DaysInMonth * cfg.HoursInDay;
            svc.SetByTotalHours(hoursInYear);

            Assert.AreEqual(cfg.StartYear + 1, svc.Current.Year);
        }

        [Test]
        public void SetByTotalHours_FiresPhaseChangedEvent_WhenPhaseChanges()
        {
            var cfg   = DefaultConfig();
            var svc   = new GameCalendarService(cfg);
            DayPhase? lastPhase = null;
            svc.OnDayPhaseChanged += p => lastPhase = p;

            // Initial phase at epoch (startHour=6) is Day.
            // 13 hours in: hour = (13+6)%24 = 19 → Dusk (different from Day).
            svc.SetByTotalHours(13);

            Assert.IsNotNull(lastPhase);
        }
    }
}
