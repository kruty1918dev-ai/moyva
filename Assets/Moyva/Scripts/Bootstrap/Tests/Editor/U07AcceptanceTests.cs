using Kruty1918.Moyva.Bootstrap.Runtime;
using NUnit.Framework;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// U07: the Kingdom regions rail must reflect the live settlement list with
    /// the correct owner and active state, survive empty/one/many sets and long
    /// localized names, stay keyboard-selectable, and mark the current region.
    /// Selection must never imply an ownership change — rows carry no action.
    /// </summary>
    [TestFixture]
    internal sealed class U07AcceptanceTests
    {
        private static string Overlay(GameplayHtmlSnapshot snapshot, GameplayHtmlState state)
        {
            IReadOnlyDictionary<string, string> regions = GameplayHtmlMarkup.BuildRegions(snapshot, state);
            return regions["gameplay-overlay-region"];
        }

        private static GameplayHtmlState KingdomOpen()
        {
            var state = new GameplayHtmlState();
            state.OpenPanel(GameplayHtmlPanel.Kingdom);
            return state;
        }

        [Test]
        public void Regions_ShowOwnerAndActiveStatePerSettlement()
        {
            var state = KingdomOpen();
            var snapshot = new GameplayHtmlSnapshot
            {
                KingdomName = "Testland",
                Settlements = new[]
                {
                    new GameplaySettlementViewSnapshot("set-1", "Alpha", 12, 3, null),
                    new GameplaySettlementViewSnapshot("set-2", "Beta", 4, 1, null),
                },
            };

            string html = Overlay(snapshot, state);

            StringAssert.Contains("id=\"kingdom-regions\"", html);
            StringAssert.Contains("data-key=\"region-set-1\"", html);
            StringAssert.Contains("data-key=\"region-set-2\"", html);
            StringAssert.Contains("Testland", html);
            StringAssert.Contains("Active", html);
            StringAssert.Contains("Alpha", html);
            StringAssert.Contains("Beta", html);
        }

        [Test]
        public void Regions_Empty_ShowsHonestEmptyState()
        {
            string html = Overlay(new GameplayHtmlSnapshot { KingdomName = "Testland" }, KingdomOpen());

            StringAssert.Contains("id=\"kingdom-regions\"", html);
            StringAssert.Contains("No regions yet", html);
            StringAssert.DoesNotContain("data-key=\"region-", html);
        }

        [Test]
        public void Regions_Hidden_WhenKingdomPanelClosed()
        {
            var snapshot = new GameplayHtmlSnapshot
            {
                Settlements = new[] { new GameplaySettlementViewSnapshot("set-1", "Alpha", 1, 1, null) },
            };

            string html = Overlay(snapshot, new GameplayHtmlState());

            StringAssert.DoesNotContain("kingdom-regions", html);
        }

        [Test]
        public void Regions_LongLocalizedName_IsEscapedAndAvailableAsTooltip()
        {
            string longName = "Новобудо́ва Довга <незламна> Назва Поселення №17-а";
            var snapshot = new GameplayHtmlSnapshot
            {
                KingdomName = "Testland",
                Settlements = new[] { new GameplaySettlementViewSnapshot("set-long", longName, 2, 1, null) },
            };

            string html = Overlay(snapshot, KingdomOpen());

            StringAssert.DoesNotContain("<незламна>", html);
            StringAssert.Contains("&lt;", html);
            StringAssert.Contains("data-tooltip=", html);
            StringAssert.Contains("data-key=\"region-set-long\"", html);
        }

        [Test]
        public void Regions_CurrentRegion_AutofocusedOnFirstRowOnly()
        {
            var snapshot = new GameplayHtmlSnapshot
            {
                Settlements = new[]
                {
                    new GameplaySettlementViewSnapshot("set-a", "Alpha", 1, 1, null),
                    new GameplaySettlementViewSnapshot("set-b", "Beta", 1, 1, null),
                },
            };

            string html = Overlay(snapshot, KingdomOpen());
            int layer = html.IndexOf("kingdom-regions-layer", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(layer, 0);
            string rail = html.Substring(layer);

            Assert.AreEqual(1, CountOccurrences(rail, "data-autofocus=\"true\""));
            int rowA = rail.IndexOf("region-set-a", System.StringComparison.Ordinal);
            int focus = rail.IndexOf("data-autofocus", System.StringComparison.Ordinal);
            int rowB = rail.IndexOf("region-set-b", System.StringComparison.Ordinal);
            Assert.Greater(focus, rowA, "autofocus lands after the first row opens");
            Assert.Less(focus, rowB, "autofocus belongs to the first row, not a later one");
        }

        [Test]
        public void Regions_ManySettlements_RenderUniqueStableKeys()
        {
            var list = new GameplaySettlementViewSnapshot[40];
            for (int i = 0; i < list.Length; i++)
                list[i] = new GameplaySettlementViewSnapshot($"set-{i:00}", $"Region {i:00}", i, i / 2, null);
            var snapshot = new GameplayHtmlSnapshot { Settlements = list };

            string html = Overlay(snapshot, KingdomOpen());

            var seen = new HashSet<string>();
            for (int i = 0; i < list.Length; i++)
            {
                string key = $"data-key=\"region-set-{i:00}\"";
                StringAssert.Contains(key, html);
                Assert.IsTrue(seen.Add(key));
            }
        }

        [Test]
        public void Regions_RowSelection_CannotMutateOwnership()
        {
            var snapshot = new GameplayHtmlSnapshot
            {
                Settlements = new[] { new GameplaySettlementViewSnapshot("set-1", "Alpha", 1, 1, null) },
            };

            string html = Overlay(snapshot, KingdomOpen());
            int row = html.IndexOf("data-key=\"region-set-1\"", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(row, 0);
            int rowEnd = html.IndexOf("</button>", row, System.StringComparison.Ordinal);
            Assert.Greater(rowEnd, row, "region row must be a focusable button");
            string markup = html.Substring(row, rowEnd - row);

            StringAssert.DoesNotContain("onClick", markup);
            StringAssert.DoesNotContain("onSubmit", markup);
        }

        [Test]
        public void Regions_CaptureAndDeactivation_FollowSnapshot()
        {
            var state = KingdomOpen();
            var withBoth = new GameplayHtmlSnapshot
            {
                Settlements = new[]
                {
                    new GameplaySettlementViewSnapshot("set-a", "Alpha", 1, 1, null),
                    new GameplaySettlementViewSnapshot("set-b", "Captured Keep", 5, 2, null),
                },
            };
            string captured = Overlay(withBoth, KingdomOpen());
            StringAssert.Contains("data-key=\"region-set-b\"", captured);

            var withoutB = new GameplayHtmlSnapshot
            {
                Settlements = new[] { new GameplaySettlementViewSnapshot("set-a", "Alpha", 1, 1, null) },
            };
            string deactivated = Overlay(withoutB, KingdomOpen());
            StringAssert.DoesNotContain("region-set-b", deactivated);
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0, index = 0;
            while ((index = haystack.IndexOf(needle, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }
            return count;
        }
    }
}
