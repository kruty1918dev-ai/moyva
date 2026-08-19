using Kruty1918.Moyva.Notifications.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Tests.Notifications
{
    [TestFixture]
    public sealed class GameplayNotificationViewTests
    {
        [Test]
        public void Notification_View_DoesNotBlockRaycasts()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameplayNotificationView view = null;

            try
            {
                view = GameplayNotificationView.Create(canvasObject.transform);

                Assert.NotNull(view.CanvasGroup);
                Assert.NotNull(view.MessageText);
                Assert.IsFalse(view.CanvasGroup.blocksRaycasts);
                Assert.IsFalse(view.CanvasGroup.interactable);
                Assert.IsFalse(view.MessageText.raycastTarget);
            }
            finally
            {
                Object.DestroyImmediate(canvasObject);
            }
        }

        [Test]
        public void Notification_VisualSequence_MatchesRuntimeTiming()
        {
            var settings = new GameplayNotificationSettings();
            float hold = settings.HoldDuration;

            GameplayNotificationVisualState start = GameplayNotificationAnimation.Evaluate(settings, 0f, hold);
            GameplayNotificationVisualState entered = GameplayNotificationAnimation.Evaluate(settings, settings.EnterDuration, hold);
            GameplayNotificationVisualState held = GameplayNotificationAnimation.Evaluate(settings, settings.EnterDuration + hold * 0.5f, hold);
            GameplayNotificationVisualState exiting = GameplayNotificationAnimation.Evaluate(settings, settings.EnterDuration + hold + settings.ExitDuration * 0.5f, hold);
            GameplayNotificationVisualState end = GameplayNotificationAnimation.Evaluate(settings, settings.EnterDuration + hold + settings.ExitDuration, hold);

            Assert.AreEqual(0f, start.Alpha);
            Assert.AreEqual(settings.HiddenOffsetY, start.AnchoredPositionY);
            Assert.AreEqual(1f, entered.Alpha);
            Assert.AreEqual(settings.VisibleOffsetY, entered.AnchoredPositionY);
            Assert.AreEqual(1f, held.Alpha);
            Assert.AreEqual(settings.VisibleOffsetY, held.AnchoredPositionY);
            Assert.That(exiting.Alpha, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(exiting.AnchoredPositionY, Is.GreaterThan(settings.VisibleOffsetY).And.LessThan(settings.HiddenOffsetY));
            Assert.IsFalse(end.IsActive);
            Assert.AreEqual(0f, end.Alpha);
            Assert.AreEqual(settings.HiddenOffsetY, end.AnchoredPositionY);
        }
    }
}
