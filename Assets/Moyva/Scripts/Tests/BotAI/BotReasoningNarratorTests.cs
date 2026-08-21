using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotReasoningNarratorTests
    {
        [Test]
        public void DescribeCandidate_UsesRealCandidateScoreReasonAndTarget()
        {
            var candidate = new BotActionCandidate(
                "build:castle:4,5",
                BotActionKind.Build,
                default,
                new BotActionScore(2100, "weighted site"),
                targetCell: new Vector2Int(4, 5),
                definitionId: "castle",
                reason: "castle-site-weighted-evaluation");

            string text = BotReasoningNarrator.DescribeCandidate(
                candidate,
                1,
                3);

            Assert.That(text, Does.Contain("Build"));
            Assert.That(text, Does.Contain("2100"));
            Assert.That(text, Does.Contain("castle-site-weighted-evaluation"));
            Assert.That(text, Does.Contain("(4, 5)"));
        }
    }
}
