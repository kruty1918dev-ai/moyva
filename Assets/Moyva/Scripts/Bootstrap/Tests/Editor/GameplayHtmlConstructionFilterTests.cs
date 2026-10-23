using Kruty1918.Moyva.Bootstrap.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests
{
    /// <summary>
    /// Construction panel availability filter: a separate dropdown switches
    /// the building list between all options, only buildable ones and only
    /// locked ones — on top of the existing category/search/producer filters.
    /// </summary>
    public sealed class GameplayHtmlConstructionFilterTests
    {
        private const string BuildableName = "ZZZOpenForge";
        private const string LockedName = "ZZZLockedKeep";

        private GameplayHtmlState _state;
        private GameplayHtmlSnapshot _snapshot;

        [SetUp]
        public void SetUp()
        {
            _state = new GameplayHtmlState();
            _state.OpenPanel(GameplayHtmlPanel.Construction);
            _snapshot = new GameplayHtmlSnapshot
            {
                BuildingOptions = new[]
                {
                    new GameplayBuildingOptionSnapshot(
                        "open-forge", BuildableName, "Industry",
                        "ZZZ buildable description.", "Wood 10",
                        canSelect: true),
                    new GameplayBuildingOptionSnapshot(
                        "locked-keep", LockedName, "Military",
                        "ZZZ locked description.", "Gold 50",
                        canSelect: false, unavailableReason: "ZZZ requires a castle."),
                },
            };
        }

        private string Render()
            => GameplayHtmlMarkup.Build(_snapshot, _state, "test");

        [Test]
        public void Availability_All_ShowsEveryOption()
        {
            _state.SetConstructionAvailability(GameplayConstructionAvailability.All);
            string html = Render();

            StringAssert.Contains(BuildableName, html);
            StringAssert.Contains(LockedName, html);
        }

        [Test]
        public void Availability_Buildable_HidesUnavailableOptions()
        {
            _state.SetConstructionAvailability(GameplayConstructionAvailability.Buildable);
            string html = Render();

            StringAssert.Contains(BuildableName, html);
            Assert.IsFalse(html.Contains(LockedName),
                "The unavailable option must be filtered out.");
        }

        [Test]
        public void Availability_Unavailable_HidesBuildableOptions()
        {
            _state.SetConstructionAvailability(GameplayConstructionAvailability.Unavailable);
            string html = Render();

            StringAssert.Contains(LockedName, html);
            Assert.IsFalse(html.Contains(BuildableName),
                "The buildable option must be filtered out.");
        }

        [Test]
        public void ConstructionPanel_RendersAvailabilityDropdown()
        {
            string html = Render();

            StringAssert.Contains("construction-availability-select", html);
            StringAssert.Contains("SetConstructionAvailability", html);
        }

        [Test]
        public void Availability_Change_ResetsPageIndex()
        {
            _state.MoveConstructionPage(1);
            _state.SetConstructionAvailability(GameplayConstructionAvailability.Buildable);

            Assert.AreEqual(0, _state.ConstructionPageIndex);
        }
    }
}
