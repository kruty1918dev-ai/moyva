using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotReasoningTraceStoreTests
    {
        [Test]
        public void Record_PreservesOwnerTurnTargetAndFactors()
        {
            var store = new BotReasoningTraceStore();
            var factors = new[]
            {
                new BotSiteScoreFactor(
                    "height",
                    "Висота",
                    2,
                    30,
                    60,
                    "High ground"),
            };

            BotReasoningEntry entry = store.Record(
                "bot-01",
                3,
                BotReasoningStage.SiteEvaluation,
                "Castle site",
                "Weighted local evaluation.",
                750,
                new Vector2Int(4, 5),
                "castle",
                factors);

            Assert.That(entry.Sequence, Is.GreaterThan(0));
            Assert.That(entry.OwnerId, Is.EqualTo("bot-01"));
            Assert.That(entry.GlobalTurn, Is.EqualTo(3));
            Assert.That(entry.TargetCell, Is.EqualTo(new Vector2Int(4, 5)));
            Assert.That(entry.Factors.Count, Is.EqualTo(1));

            var captured = store.GetEntries("bot-01");
            Assert.That(captured.Count, Is.EqualTo(1));
            Assert.That(captured[0].Score, Is.EqualTo(750));
        }

        [Test]
        public void GetEntriesSince_ReturnsOnlyNewerSequence()
        {
            var store = new BotReasoningTraceStore();
            BotReasoningEntry first = store.Record(
                "bot",
                1,
                BotReasoningStage.Session,
                "one",
                "one");

            store.Record(
                "bot",
                1,
                BotReasoningStage.Strategy,
                "two",
                "two");

            var result = store.GetEntriesSince("bot", first.Sequence);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Headline, Is.EqualTo("two"));
        }
    }
}
