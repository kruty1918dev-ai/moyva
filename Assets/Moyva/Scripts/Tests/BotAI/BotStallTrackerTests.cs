using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotStallTrackerTests
    {
        [Test]
        public void ThreeCompletedZeroMutationTurns_TriggersStall()
        {
            var tracker =
                new BotStallTracker(
                    BotPlanningProfile.Normal());

            for (long turn = 1; turn <= 3; turn++)
            {
                tracker.BeginTurn("bot", turn);
                tracker.RecordActionResult(
                    "bot",
                    turn,
                    "build:castle",
                    succeeded: false,
                    mutated: false,
                    failureReason: "blocked");

                tracker.CompleteTurn("bot", turn);
            }

            BotStallStatus status =
                tracker.GetStatus("bot");

            Assert.That(status.IsStalled, Is.True);
            Assert.That(
                status.ConsecutiveZeroMutationTurns,
                Is.EqualTo(3));

            Assert.That(
                status.StallReason,
                Does.Contain("0 authoritative mutations"));
        }

        [Test]
        public void SuccessfulMutation_ResetsZeroMutationStreak()
        {
            var tracker =
                new BotStallTracker(
                    BotPlanningProfile.Normal());

            for (long turn = 1; turn <= 2; turn++)
            {
                tracker.BeginTurn("bot", turn);
                tracker.CompleteTurn("bot", turn);
            }

            tracker.BeginTurn("bot", 3);
            tracker.RecordActionResult(
                "bot",
                3,
                "build:castle",
                succeeded: true,
                mutated: true,
                failureReason: null);

            BotStallStatus status =
                tracker.CompleteTurn("bot", 3);

            Assert.That(status.IsStalled, Is.False);
            Assert.That(
                status.ConsecutiveZeroMutationTurns,
                Is.Zero);
        }
    }
}
