using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Calendar
{
    [TestFixture]
    public class CalendarSyncAdapterTests
    {
        private static CalendarConfig DefaultConfig() => CalendarConfig.Default();

        // ------------------------------------------------------------------ OnHostAdvanced

        [Test]
        public void NotifyTurnCompleted_FiresOnHostAdvanced()
        {
            var service = new GameCalendarService(DefaultConfig());
            var adapter = new CalendarSyncAdapter(service);

            long? received = null;
            adapter.OnHostAdvanced += hours => received = hours;

            adapter.NotifyTurnCompleted();

            Assert.IsNotNull(received);
        }

        [Test]
        public void NotifyTurnCompleted_OnHostAdvanced_CarriesCorrectTotalHours()
        {
            var cfg     = DefaultConfig();
            var service = new GameCalendarService(cfg);
            var adapter = new CalendarSyncAdapter(service);

            long received = -1;
            adapter.OnHostAdvanced += hours => received = hours;

            adapter.NotifyTurnCompleted();

            Assert.AreEqual(cfg.HoursPerTurn, received);
        }

        [Test]
        public void NotifyTurnCompleted_MultipleAdvances_AccumulatesHours()
        {
            var cfg     = DefaultConfig();
            var service = new GameCalendarService(cfg);
            var adapter = new CalendarSyncAdapter(service);

            long lastReceived = -1;
            adapter.OnHostAdvanced += hours => lastReceived = hours;

            adapter.NotifyTurnCompleted();
            adapter.NotifyTurnCompleted();
            adapter.NotifyTurnCompleted();

            Assert.AreEqual(cfg.HoursPerTurn * 3, lastReceived);
        }

        // ------------------------------------------------------------------ ApplyRemoteSnapshot

        [Test]
        public void ApplyRemoteSnapshot_UpdatesProxyState()
        {
            var cfg     = DefaultConfig();
            var service = new GameCalendarService(cfg);
            var proxy   = new ClientCalendarProxy(cfg);
            var adapter = new CalendarSyncAdapter(service, proxy);

            long hoursInYear = (long)cfg.MonthsInYear * cfg.DaysInMonth * cfg.HoursInDay;
            adapter.ApplyRemoteSnapshot(hoursInYear);

            Assert.AreEqual(cfg.StartYear + 1, proxy.Current.Year);
        }

        [Test]
        public void ApplyRemoteSnapshot_WithNullProxy_DoesNotThrow()
        {
            var service = new GameCalendarService(DefaultConfig());
            var adapter = new CalendarSyncAdapter(service, proxy: null);

            Assert.DoesNotThrow(() => adapter.ApplyRemoteSnapshot(42));
        }

        // ------------------------------------------------------------------ Default start year Easter egg

        [Test]
        public void Default_StartYear_IsPeakUkraineYear()
        {
            Assert.AreEqual(CalendarConfig.PeakUkraineYear, CalendarConfig.Default().StartYear);
        }

        [Test]
        public void PeakUkraineYear_Is1054()
        {
            Assert.AreEqual(1054, CalendarConfig.PeakUkraineYear);
        }
    }
}
