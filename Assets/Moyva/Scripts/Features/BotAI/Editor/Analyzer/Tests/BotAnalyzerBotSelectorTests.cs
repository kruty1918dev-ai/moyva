using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerBotSelectorTests
    {
        [Test]
        public void GetBotIds_ReturnsOnlyBotsSortedAndUnique()
        {
            var factions = new[]
            {
                new TurnFaction("human", false, Vector2Int.zero),
                new TurnFaction("bot_b", true, Vector2Int.right),
                new TurnFaction("bot_a", true, Vector2Int.up),
                new TurnFaction("bot_b", true, Vector2Int.one),
            };

            var bots = BotAnalyzerBotSelector.GetBotIds(factions);

            Assert.That(bots, Is.EqualTo(new[] { "bot_a", "bot_b" }));
        }
    }
}
