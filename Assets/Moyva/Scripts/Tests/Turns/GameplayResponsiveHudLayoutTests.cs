using Kruty1918.Moyva.Bootstrap.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Turns
{
    public sealed class GameplayResponsiveHudLayoutTests
    {
        [Test]
        public void WideLandscape_TurnPanelStaysLeftOfTopCenterReservation()
        {
            var safe = new GameplayResponsiveHudLayout.SafeInsets(0f, 0f, 0f, 0f);
            Rect reservation = new(720f, 1010f, 480f, 50f);

            GameplayResponsiveHudLayout.LayoutPlan plan = GameplayResponsiveHudLayout.CalculateLayout(
                new Vector2(1920f, 1080f),
                safe,
                reservation);

            Assert.LessOrEqual(plan.TurnRect.xMax, reservation.xMin - 15.9f);
            Assert.IsFalse(plan.TurnMovedBelowTopReservation);
            Assert.IsFalse(plan.RecruitmentBottomSheet);
        }

        [Test]
        public void NarrowLandscape_WhenTopReservationBlocksLeftSpace_TurnPanelMovesBelowIt()
        {
            var safe = new GameplayResponsiveHudLayout.SafeInsets(0f, 0f, 0f, 0f);
            Rect reservation = new(250f, 650f, 460f, 50f);

            GameplayResponsiveHudLayout.LayoutPlan plan = GameplayResponsiveHudLayout.CalculateLayout(
                new Vector2(960f, 720f),
                safe,
                reservation);

            Assert.IsTrue(plan.TurnMovedBelowTopReservation);
            Assert.LessOrEqual(plan.TurnRect.yMax, reservation.yMin - 15.9f);
            Assert.GreaterOrEqual(plan.TurnRect.xMin, 20f);
        }

        [Test]
        public void Portrait_RecruitmentBecomesBottomSheetInsideSafeArea()
        {
            var safe = new GameplayResponsiveHudLayout.SafeInsets(12f, 18f, 24f, 30f);

            GameplayResponsiveHudLayout.LayoutPlan plan = GameplayResponsiveHudLayout.CalculateLayout(
                new Vector2(900f, 1600f),
                safe,
                Rect.zero);

            Assert.IsTrue(plan.RecruitmentBottomSheet);
            Assert.AreEqual(32f, plan.RecruitmentRect.x, 0.01f);
            Assert.AreEqual(50f, plan.RecruitmentRect.y, 0.01f);
            Assert.LessOrEqual(plan.RecruitmentRect.xMax, 900f - 18f - 20f + 0.01f);
        }
    }
}
