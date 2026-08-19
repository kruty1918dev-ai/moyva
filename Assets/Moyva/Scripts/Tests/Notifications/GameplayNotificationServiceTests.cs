using System;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.Notifications.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Notifications
{
    [TestFixture]
    public sealed class GameplayNotificationServiceTests
    {
        private sealed class FakePresenter : IGameplayNotificationPresenter
        {
            private Action _completed;

            public int PresentCount { get; private set; }
            public int ResetCount { get; private set; }
            public GameplayNotificationRequest LastRequest { get; private set; }

            public void Present(
                GameplayNotificationRequest request,
                float holdDuration,
                Action completed)
            {
                PresentCount++;
                LastRequest = request;
                _completed = completed;
            }

            public void ResetPresentation()
            {
                ResetCount++;
                _completed = null;
            }

            public void Complete()
            {
                Action completed = _completed;
                _completed = null;
                completed?.Invoke();
            }
        }

        [Test]
        public void Notification_Show_EnqueuesMessage()
        {
            GameplayNotificationService service = CreateService(out _);

            service.Show("Hello");

            Assert.AreEqual(1, service.PendingCount);
            Assert.AreEqual("Hello", service.ActiveMessage);
        }

        [Test]
        public void Notification_FirstMessageBecomesActive()
        {
            GameplayNotificationService service = CreateService(out FakePresenter presenter);

            service.Show("First");

            Assert.IsTrue(service.HasActive);
            Assert.AreEqual(1, presenter.PresentCount);
            Assert.AreEqual("First", presenter.LastRequest.Message);
        }

        [Test]
        public void Notification_SecondMessageWaitsForFirst()
        {
            GameplayNotificationService service = CreateService(out FakePresenter presenter);

            service.Show("First");
            service.Show("Second");

            Assert.AreEqual("First", service.ActiveMessage);
            Assert.AreEqual(1, service.QueuedCount);
            Assert.AreEqual(1, presenter.PresentCount);
        }

        [Test]
        public void Notification_DuplicateDedupKey_IsNotQueuedTwice()
        {
            GameplayNotificationService service = CreateService(out _);

            service.Show("First", dedupKey: "same");
            service.Show("Second", dedupKey: "same");

            Assert.AreEqual(1, service.PendingCount);
            Assert.AreEqual(0, service.QueuedCount);
        }

        [Test]
        public void Notification_AfterExit_ShowsNext()
        {
            GameplayNotificationService service = CreateService(out FakePresenter presenter);
            service.Show("First");
            service.Show("Second");

            presenter.Complete();

            Assert.IsTrue(service.HasActive);
            Assert.AreEqual("Second", service.ActiveMessage);
            Assert.AreEqual(2, presenter.PresentCount);
            Assert.AreEqual(0, service.QueuedCount);
        }

        [Test]
        public void Notification_QueueLimit_IsRespected()
        {
            var settings = new GameplayNotificationSettings { MaxQueueSize = 8 };
            var presenter = new FakePresenter();
            var service = new GameplayNotificationService(settings, presenter);

            for (int i = 0; i < 12; i++)
                service.Show($"Message {i}");

            Assert.AreEqual(8, service.PendingCount);
            Assert.AreEqual(7, service.QueuedCount);
        }

        [Test]
        public void Notification_DefaultDurations_AreValid()
        {
            var settings = new GameplayNotificationSettings();
            settings.Normalize();

            Assert.AreEqual(0.25f, settings.EnterDuration);
            Assert.AreEqual(1.8f, settings.HoldDuration);
            Assert.AreEqual(0.22f, settings.ExitDuration);
            Assert.AreEqual(70f, settings.HiddenOffsetY);
            Assert.AreEqual(-70f, settings.VisibleOffsetY);
            Assert.AreEqual(8, settings.MaxQueueSize);
            Assert.Greater(settings.EnterDuration, 0f);
            Assert.Greater(settings.HoldDuration, 0f);
            Assert.Greater(settings.ExitDuration, 0f);
        }

        private static GameplayNotificationService CreateService(out FakePresenter presenter)
        {
            presenter = new FakePresenter();
            return new GameplayNotificationService(
                new GameplayNotificationSettings(),
                presenter);
        }
    }
}
