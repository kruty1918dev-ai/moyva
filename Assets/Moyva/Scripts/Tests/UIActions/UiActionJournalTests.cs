using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.UIActions.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.UIActions
{
    [TestFixture]
    public sealed class UiActionJournalTests
    {
        [Test]
        public void Journal_RecordsPerformedAction()
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(UiActionIds.Construction.Open, UiActionSource.Button, "Gameplay"),
                UiActionResult.Performed());

            UiActionJournalEntry entry = journal.GetRecent(1)[0];
            Assert.AreEqual(UiActionIds.Construction.Open, entry.ActionId);
            Assert.AreEqual(UiActionStatus.Performed, entry.Status);
        }

        [Test]
        public void Journal_RecordsRejectedActionAndReason()
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(UiActionIds.Recruitment.Enqueue, UiActionSource.Button, "RecruitmentPanel"),
                UiActionResult.Rejected(UiActionReason.InsufficientResources));

            UiActionJournalEntry entry = journal.GetRecent(1)[0];
            Assert.AreEqual(UiActionStatus.Rejected, entry.Status);
            Assert.AreEqual(UiActionReason.InsufficientResources, entry.Reason);
        }

        [Test]
        public void Journal_RecordsSourceButton()
            => AssertSource(UiActionSource.Button);

        [Test]
        public void Journal_RecordsSourceHotkey()
            => AssertSource(UiActionSource.Hotkey);

        [Test]
        public void Journal_RecordsSourceEscape()
            => AssertSource(UiActionSource.Escape);

        [Test]
        public void Journal_IsBounded()
        {
            var journal = new UiActionJournal();

            for (int i = 0; i < UiActionJournal.DefaultCapacity + 20; i++)
            {
                journal.Record(
                    new UiActionRequest(new UiActionId($"test.id{i}"), UiActionSource.Programmatic, "Gameplay"),
                    UiActionResult.Performed());
            }

            Assert.AreEqual(UiActionJournal.DefaultCapacity, journal.Count);
            Assert.AreEqual(new UiActionId("test.id20"), journal.GetRecent(UiActionJournal.DefaultCapacity)[0].ActionId);
        }

        [Test]
        public void Journal_DoesNotSpamHeldInputPerFrame()
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(new UiActionId("camera.pan.start"), UiActionSource.Hotkey, "Gameplay"),
                UiActionResult.Performed());

            Assert.AreEqual(1, journal.Count);
        }

        private static void AssertSource(UiActionSource source)
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(UiActionIds.Construction.Open, source, "Gameplay"),
                UiActionResult.Performed());

            Assert.AreEqual(source, journal.GetRecent(1)[0].Source);
        }
    }
}
