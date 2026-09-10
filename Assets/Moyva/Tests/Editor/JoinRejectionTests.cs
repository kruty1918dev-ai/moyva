using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Startup
{
    public sealed class JoinRejectionTests
    {
        [Test]
        public async Task ConfigurationRejectionDoesNotStartAliasDiscovery()
        {
            var type = typeof(JoinRoomTarget).Assembly.GetType("Kruty1918.Moyva.HomeMenu.Runtime.JoinRoomPanelService");
            var service = Activator.CreateInstance(type, true);
            var lobby = new RejectedLobby();
            type.GetField("_lobbyService", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(service, lobby);
            var task = (Task)type.GetMethod("JoinTargetResultAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(service, new object[] { new JoinRoomTarget(JoinRoomTargetKind.JoinCode, "ABC123"), null, CancellationToken.None });
            await task;
            object result = task.GetType().GetProperty("Result").GetValue(task);
            Assert.That(result.GetType().GetProperty("IsFailure").GetValue(result), Is.True);
            Assert.That(lobby.JoinCalls, Is.EqualTo(1));
            Assert.That(lobby.QueryCalls, Is.Zero, "Rejected configuration must not become an alias-search timeout.");
        }

        [TestCase("ResolvingTarget", LobbyState.Closed, false)]
        [TestCase("JoiningLobby", LobbyState.Closed, false)]
        [TestCase("JoiningLobby", LobbyState.Started, false)]
        [TestCase("ConnectingTransport", LobbyState.Closed, true)]
        [TestCase("ConnectingTransport", LobbyState.Started, false)]
        [TestCase("Failed", LobbyState.Closed, false)]
        public void OnlyClosingTheJoinedRoomCancelsTransport(string phase, LobbyState state, bool cancelled)
        {
            var type = typeof(JoinRoomTarget).Assembly.GetType("Kruty1918.Moyva.HomeMenu.Runtime.JoinRoomPanelService");
            var service = Activator.CreateInstance(type, true);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            using var cancellation = new CancellationTokenSource();
            type.GetField("_joinCts", fields).SetValue(service, cancellation);
            type.GetField("_isJoining", fields).SetValue(service, true);
            var phaseField = type.GetField("_joinState", fields);
            phaseField.SetValue(service, Enum.Parse(phaseField.FieldType, phase));
            type.GetMethod("OnLobbyStateChanged", fields).Invoke(service, new object[] { state });
            Assert.That(cancellation.IsCancellationRequested, Is.EqualTo(cancelled));
        }

        private sealed class RejectedLobby : ILobbyService
        {
            public int JoinCalls;
            public int QueryCalls;
            public LobbyRoom Current => null;
            public LobbyState State => LobbyState.Open;
            public event Action<LobbyRoom> LobbyUpdated { add { } remove { } }
            public event Action<string> KickedFromLobby { add { } remove { } }
            public event Action<LobbyState> StateChanged { add { } remove { } }
            public Task<LobbyRoom> JoinByCodeAsync(string code, string name, CancellationToken ct = default)
            {
                JoinCalls++;
                return Task.FromException<LobbyRoom>(new RoomConfigMismatchException("local", "host"));
            }
            public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
            {
                QueryCalls++;
                return Task.FromException<IReadOnlyList<LobbyRoom>>(new OperationCanceledException("Discovery timed out"));
            }
            public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default) => throw new NotSupportedException();
            public Task<LobbyRoom> JoinByIdAsync(string id, string name, CancellationToken ct = default) => throw new NotSupportedException();
            public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string code, string name, string password, CancellationToken ct = default) => throw new NotSupportedException();
            public Task LeaveAsync(CancellationToken ct = default) => Task.CompletedTask;
            public Task KickAsync(string id, CancellationToken ct = default) => throw new NotSupportedException();
            public Task SetRelayJoinCodeAsync(string code, CancellationToken ct = default) => throw new NotSupportedException();
            public Task LockAsync(bool locked, byte[] world = null, CancellationToken ct = default) => throw new NotSupportedException();
        }
    }
}
