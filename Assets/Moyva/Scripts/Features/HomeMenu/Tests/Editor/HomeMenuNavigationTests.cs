using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class HomeMenuNavigationTests
    {
        [Test]
        public void Open_WhenSamePanelIsAlreadyCurrent_IsIdempotent()
        {
            var panel = new FakePanel("CreateRoomPanel");
            var navigation = new HomeMenuNavigation(new INavigationPanel[] { panel });
            var changeCount = 0;
            NavigationChangeEventArgs lastEvent = default;
            navigation.OnMenuChanged += args =>
            {
                changeCount++;
                lastEvent = args;
            };

            navigation.Open("CreateRoomPanel");
            navigation.Open("CreateRoomPanel");

            Assert.That(navigation.CurrentMenu, Is.EqualTo("CreateRoomPanel"));
            Assert.That(panel.OpenCount, Is.EqualTo(1));
            Assert.That(panel.CloseCount, Is.EqualTo(0));
            Assert.That(changeCount, Is.EqualTo(1));
            Assert.That(lastEvent.CurrentIsOpen, Is.True);
        }

        private sealed class FakePanel : INavigationPanel
        {
            public FakePanel(string menuName)
            {
                MenuName = menuName;
            }

            public string MenuName { get; }
            public int OpenCount { get; private set; }
            public int CloseCount { get; private set; }

            public void Open() => OpenCount++;
            public void Close() => CloseCount++;
        }
    }
}
