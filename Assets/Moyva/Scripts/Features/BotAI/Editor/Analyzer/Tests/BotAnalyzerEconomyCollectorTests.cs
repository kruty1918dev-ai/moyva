using System.Collections.Generic;
using NUnit.Framework;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer.Tests
{
    public sealed class BotAnalyzerEconomyCollectorTests
    {
        [Test]
        public void FromTotals_SortsIdsAndUsesDisplayNames()
        {
            IReadOnlyDictionary<string, float> totals = new Dictionary<string, float>
            {
                ["wood"] = 12,
                ["stone"] = 7,
            };

            var rows = BotAnalyzerEconomyCollector.FromTotals(totals, id => id.ToUpperInvariant());

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0].ResourceId, Is.EqualTo("stone"));
            Assert.That(rows[0].DisplayName, Is.EqualTo("STONE"));
            Assert.That(rows[1].ResourceId, Is.EqualTo("wood"));
            Assert.That(rows[1].Amount, Is.EqualTo(12));
        }
    }
}
