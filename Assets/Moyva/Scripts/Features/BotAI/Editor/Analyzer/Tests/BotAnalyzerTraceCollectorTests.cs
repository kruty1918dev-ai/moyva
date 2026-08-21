using Kruty1918.Moyva.BotAI.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerTraceCollectorTests
    {
        [Test]
        public void Map_PreservesRealTraceFieldsAndRanking()
        {
            var input = new[]
            {
                new BotDecisionTraceEntry(9, BotStrategicPosture.Pressure, "attack:a", BotActionKind.Attack, 900, "visible target"),
                new BotDecisionTraceEntry(9, BotStrategicPosture.Pressure, "move:b", BotActionKind.Move, 500, "support"),
            };

            var result = BotAnalyzerTraceCollector.Map(input);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Rank, Is.EqualTo(1));
            Assert.That(result[0].CandidateId, Is.EqualTo("attack:a"));
            Assert.That(result[0].Kind, Is.EqualTo("Attack"));
            Assert.That(result[0].Score, Is.EqualTo(900));
            Assert.That(result[0].Explanation, Is.EqualTo("visible target"));
            Assert.That(result[0].ActorId, Is.Empty);
            Assert.That(BotAnalyzerTraceCollector.Fingerprint(result), Does.Contain("attack:a"));
        }
    }
}
