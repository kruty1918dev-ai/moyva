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
                new UiActionRequest(UiActionId.BuildOpen, UiActionSource.Button, "Gameplay"),
                "Gameplay",
                UiActionResult.Performed());

            UiActionJournalEntry entry = journal.GetRecent(1)[0];
            Assert.AreEqual(UiActionId.BuildOpen, entry.ActionId);
            Assert.AreEqual(UiActionResultState.Performed, entry.Result);
        }

        [Test]
        public void Journal_RecordsRejectedActionAndReason()
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(UiActionId.RecruitmentEnqueue, UiActionSource.Button, "RecruitmentPanel"),
                "RecruitmentPanel",
                UiActionResult.Rejected(UiActionReasonCode.InsufficientResources));

            UiActionJournalEntry entry = journal.GetRecent(1)[0];
            Assert.AreEqual(UiActionResultState.Rejected, entry.Result);
            Assert.AreEqual(UiActionReasonCode.InsufficientResources, entry.Reason);
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
                    new UiActionRequest($"test.{i}", UiActionSource.Programmatic),
                    "Gameplay",
                    UiActionResult.Performed());
            }

            Assert.AreEqual(UiActionJournal.DefaultCapacity, journal.Count);
            Assert.AreEqual("test.20", journal.GetRecent(UiActionJournal.DefaultCapacity)[0].ActionId);
        }

        [Test]
        public void Journal_DoesNotSpamHeldInputPerFrame()
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest("camera.pan.start", UiActionSource.Hotkey, "Gameplay"),
                "Gameplay",
                UiActionResult.Performed());

            Assert.AreEqual(1, journal.Count);
        }

        private static void AssertSource(UiActionSource source)
        {
            var journal = new UiActionJournal();

            journal.Record(
                new UiActionRequest(UiActionId.BuildOpen, source, "Gameplay"),
                "Gameplay",
                UiActionResult.Performed());

            Assert.AreEqual(source, journal.GetRecent(1)[0].Source);
        }
    }
}
