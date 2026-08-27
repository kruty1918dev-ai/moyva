using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;
using UnityEngine.UI;

#pragma warning disable CS0067
namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class MultiplayerPanelServiceTests
    {
        [TestCase(NetworkProviderType.Lan)]
        [TestCase(NetworkProviderType.Relay)]
        public void CreateRoomIntent_SetsContextAndOpensCreatePanelOnce(NetworkProviderType provider)
        {
            var view = new FakeMultiplayerViewController();
            var navigation = new FakeNavigation();
            var modeSelector = new FakeModeSelector();
            var context = new LobbyFlowContext();
            var createRoomView = new FakeCreateRoomViewController();
            var service = new MultiplayerPanelService();

            Set(service, "_viewControllers", new List<IMultiplayerViewController> { view });
            Set(service, "_navigation", navigation);
            Set(service, "_modeSelector", modeSelector);
            Set(service, "_lobbyFlowContext", context);
            Set(service, "_createRoomViewController", createRoomView);
            Set(service, "_createRoomPanelName", "CreateRoomPanel");
            Set(service, "_joinRoomPanelName", "JoinRoomPanel");

            service.Initialize();
            view.FireCreate(provider);

            Assert.That(context.FlowKind, Is.EqualTo(LobbyFlowKind.Create));
            Assert.That(context.Provider, Is.EqualTo(provider));
            Assert.That(modeSelector.CurrentMode, Is.EqualTo(provider));
            Assert.That(navigation.OpenCount, Is.EqualTo(1));
            Assert.That(navigation.LastOpened, Is.EqualTo("CreateRoomPanel"));
            Assert.That(createRoomView.LastPresentation.Title, Does.Contain(provider == NetworkProviderType.Lan ? "LAN" : "Global"));
        }

        private static void Set(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }

        private sealed class FakeMultiplayerViewController : IMultiplayerViewController
        {
            public Button ButtonCreateRoom { get; set; }
            public Button ButtonJoinToRoom { get; set; }
            public event Action<NetworkProviderType> OnCreateRoomClicked;
            public event Action<NetworkProviderType> OnJoinRoomClicked;

            public void FireCreate(NetworkProviderType provider) => OnCreateRoomClicked?.Invoke(provider);
        }

        private sealed class FakeCreateRoomViewController : ICreateRoomViewController
        {
            public string RoomName { get; set; }
            public string Password { get; set; }
            public bool IsPublic { get; set; }
            public int MaxPlayers { get; set; }
            public Button NextButton => null;
            public CreateRoomPanelPresentation LastPresentation { get; private set; }

            public event Action OnButtonNextClicked;

            public void ApplyPresentation(CreateRoomPanelPresentation presentation)
            {
                LastPresentation = presentation;
            }
        }

        private sealed class FakeNavigation : INavigation
        {
            public string CurrentMenu { get; private set; } = string.Empty;
            public int OpenCount { get; private set; }
            public string LastOpened { get; private set; }
            public event Action<NavigationChangeEventArgs> OnMenuChanged;

            public void Open(string menuName)
            {
                OpenCount++;
                LastOpened = menuName;
                CurrentMenu = menuName;
            }

            public void Close(string menuName) { }
            public void CloseForce(string menuName) { }
            public Task CloseIf(string menuName, Func<Task<bool>> condition) => Task.CompletedTask;
            public void OpenForce(string menuName) => Open(menuName);
            public void OpenLast() { }
            public void OpenLastForce() { }
            public Task OpenIfAsync(string menuName, Func<Task<bool>> condition) => Task.CompletedTask;
            public void CloseLast() { }
            public void CloseLastForce() { }
        }

        private sealed class FakeModeSelector : IMultiplayerModeSelector
        {
            public NetworkProviderType CurrentMode { get; private set; } = NetworkProviderType.Relay;
            public NetworkProviderType EffectiveMode => CurrentMode;
            public event Action<NetworkProviderType> OnModeChanged;

            public Task SetModeAsync(NetworkProviderType mode, CancellationToken ct = default)
            {
                CurrentMode = mode;
                OnModeChanged?.Invoke(mode);
                return Task.CompletedTask;
            }
        }
    }
}
#pragma warning restore CS0067
