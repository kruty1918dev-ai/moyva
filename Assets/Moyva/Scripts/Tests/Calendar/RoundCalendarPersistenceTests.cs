using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Calendar
{
    public sealed class RoundCalendarPersistenceTests
    {
        [Test]
        public void SetByTotalHours_ReapplyingCurrentValue_DoesNotEmitDuplicateHourEvent()
        {
            var calendar = new GameCalendarService(CalendarConfig.Default());
            int hoursChanged = 0;
            calendar.OnHourChanged += () => hoursChanged++;

            calendar.SetByTotalHours(calendar.Config.HoursPerTurn);
            calendar.SetByTotalHours(calendar.Config.HoursPerTurn);

            Assert.That(hoursChanged, Is.EqualTo(1));
        }

        [Test]
        public void RestoreByTotalHours_ChangesCanonicalStateWithoutPublishingGameplayEvents()
        {
            var calendar = new GameCalendarService(CalendarConfig.Default());
            int hoursChanged = 0;
            int daysChanged = 0;
            calendar.OnHourChanged += () => hoursChanged++;
            calendar.OnDayChanged += () => daysChanged++;

            long restoredHours = calendar.Config.HoursPerTurn * 5L;
            ((ICalendarStateRestorer)calendar).RestoreByTotalHours(restoredHours);

            Assert.That(calendar.TotalHoursSinceEpoch, Is.EqualTo(restoredHours));
            Assert.That(hoursChanged, Is.Zero);
            Assert.That(daysChanged, Is.Zero);
        }

        [Test]
        public void AdvanceTurn_AfterSilentRestore_PublishesExactlyOneLiveHourEvent()
        {
            var calendar = new GameCalendarService(CalendarConfig.Default());
            ((ICalendarStateRestorer)calendar).RestoreByTotalHours(calendar.Config.HoursPerTurn * 2L);

            int hoursChanged = 0;
            calendar.OnHourChanged += () => hoursChanged++;
            long before = calendar.TotalHoursSinceEpoch;

            calendar.AdvanceTurn();

            Assert.That(calendar.TotalHoursSinceEpoch, Is.EqualTo(before + calendar.Config.HoursPerTurn));
            Assert.That(hoursChanged, Is.EqualTo(1));
        }

        [Test]
        public void RestoreByTotalHours_ReapplyingSameValue_IsSilentAndIdempotent()
        {
            var calendar = new GameCalendarService(CalendarConfig.Default());
            long restoredHours = calendar.Config.HoursPerTurn * 3L;
            ((ICalendarStateRestorer)calendar).RestoreByTotalHours(restoredHours);

            int hoursChanged = 0;
            calendar.OnHourChanged += () => hoursChanged++;
            ((ICalendarStateRestorer)calendar).RestoreByTotalHours(restoredHours);

            Assert.That(calendar.TotalHoursSinceEpoch, Is.EqualTo(restoredHours));
            Assert.That(hoursChanged, Is.Zero);
        }
    }
}
