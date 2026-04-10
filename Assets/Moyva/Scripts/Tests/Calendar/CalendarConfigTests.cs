using System.IO;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Calendar
{
    [TestFixture]
    public class CalendarConfigTests
    {
        [Test]
        public void Default_ReturnsNonNullConfig()
        {
            var cfg = CalendarConfig.Default();
            Assert.IsNotNull(cfg);
        }

        [Test]
        public void Default_HasExpectedHoursInDay()
        {
            Assert.AreEqual(24, CalendarConfig.Default().HoursInDay);
        }

        [Test]
        public void Default_HoursPerTurn_IsOne()
        {
            Assert.AreEqual(1, CalendarConfig.Default().HoursPerTurn);
        }

        [Test]
        public void BinaryRoundtrip_PreservesAllFields()
        {
            var original = CalendarConfig.Default();

            using var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
                CalendarBinaryConfigStore.WriteConfig(bw, original);

            ms.Position = 0;
            using var br = new BinaryReader(ms);
            var loaded = CalendarBinaryConfigStore.ReadConfig(br);

            Assert.AreEqual(original.SchemaVersion,     loaded.SchemaVersion);
            Assert.AreEqual(original.StartYear,         loaded.StartYear);
            Assert.AreEqual(original.StartMonth,        loaded.StartMonth);
            Assert.AreEqual(original.StartDay,          loaded.StartDay);
            Assert.AreEqual(original.StartHour,         loaded.StartHour);
            Assert.AreEqual(original.MonthsInYear,      loaded.MonthsInYear);
            Assert.AreEqual(original.DaysInMonth,       loaded.DaysInMonth);
            Assert.AreEqual(original.HoursInDay,        loaded.HoursInDay);
            Assert.AreEqual(original.DayStartHour,      loaded.DayStartHour);
            Assert.AreEqual(original.NightStartHour,    loaded.NightStartHour);
            Assert.AreEqual(original.DawnDurationHours, loaded.DawnDurationHours);
            Assert.AreEqual(original.DuskDurationHours, loaded.DuskDurationHours);
            Assert.AreEqual(original.HoursPerTurn,      loaded.HoursPerTurn);
        }

        [Test]
        public void Store_LoadsDefault_WhenFileDoesNotExist()
        {
            var store = new CalendarBinaryConfigStore("/tmp/nonexistent_calendar.dat");
            var cfg   = store.Load();
            Assert.IsNotNull(cfg);
            Assert.AreEqual(CalendarConfig.CurrentSchemaVersion, cfg.SchemaVersion);
        }

        [Test]
        public void Store_SaveAndLoad_RoundTrips()
        {
            string path   = Path.Combine(Path.GetTempPath(), "test_calendar_config.dat");
            var    store  = new CalendarBinaryConfigStore(path);
            var    config = CalendarConfig.Default();

            store.Save(config);
            var loaded = store.Load();

            Assert.AreEqual(config.HoursInDay,   loaded.HoursInDay);
            Assert.AreEqual(config.HoursPerTurn, loaded.HoursPerTurn);

            if (File.Exists(path)) File.Delete(path);
        }
    }
}
