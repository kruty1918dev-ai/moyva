using System;
using System.Reflection;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Startup
{
    // IsReconnectAllowed must measure the time since disconnect in BOTH clock
    // domains (player-local ticks and host-UTC ticks). The previous delta
    // comparison only verified the two machines' clocks were in sync, which
    // allowed reconnects indefinitely whenever clocks happened to agree.
    public sealed class ReconnectWindowTests
    {
        private const float ToleranceSeconds = 120f;

        private static readonly MethodInfo IsReconnectAllowed = FindMethod();

        private static MethodInfo FindMethod()
        {
            // The lifecycle class lives in the HomeMenu assembly (internal),
            // not in LobbyRoom's Multiplayer assembly — search loaded domains.
            foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
            {
                var type = assembly.GetType(
                    "Kruty1918.Moyva.HomeMenu.Runtime.Services.MultiplayerRoomLifecycle");
                var method = type?.GetMethod(
                    "IsReconnectAllowed", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                    return method;
            }
            return null;
        }

        private static LobbyRoom RoomWithRecord(string name, long playerLocalTicks, long hostUtcTicks)
        {
            return new LobbyRoom(
                "lobby-1",
                "ABCD",
                "room",
                4,
                false,
                "host-1",
                "relay-code",
                null,
                state: LobbyState.Started,
                reconnectRecords: new[]
                {
                    new LobbyReconnectRecord(name, playerLocalTicks, hostUtcTicks),
                });
        }

        private static bool Allowed(LobbyRoom room, string name)
        {
            Assert.NotNull(IsReconnectAllowed, "MultiplayerRoomLifecycle.IsReconnectAllowed not found.");
            return (bool)IsReconnectAllowed.Invoke(null, new object[] { room, name, ToleranceSeconds });
        }

        [Test]
        public void FreshDisconnect_AllowsReconnect()
        {
            var room = RoomWithRecord(
                "player",
                DateTime.Now.AddSeconds(-30).Ticks,
                DateTime.UtcNow.AddSeconds(-30).Ticks);
            Assert.IsTrue(Allowed(room, "player"));
        }

        [Test]
        public void StaleDisconnect_RejectsReconnect()
        {
            var room = RoomWithRecord(
                "player",
                DateTime.Now.AddSeconds(-300).Ticks,
                DateTime.UtcNow.AddSeconds(-300).Ticks);
            Assert.IsFalse(Allowed(room, "player"));
        }

        [Test]
        public void SynchronizedClocks_DoNotExtendReconnectForever()
        {
            // Regression: the old check compared |playerElapsed - hostElapsed|,
            // so a disconnected player whose clocks agree with the host could
            // reconnect hours later. Both elapsed measures must now fit the
            // tolerance window independently.
            var room = RoomWithRecord(
                "player",
                DateTime.Now.AddHours(-1).Ticks,
                DateTime.UtcNow.AddHours(-1).Ticks);
            Assert.IsFalse(Allowed(room, "player"));
        }

        [Test]
        public void FutureDisconnectTimestamp_RejectsReconnect()
        {
            var room = RoomWithRecord(
                "player",
                DateTime.Now.AddMinutes(10).Ticks,
                DateTime.UtcNow.AddMinutes(10).Ticks);
            Assert.IsFalse(Allowed(room, "player"));
        }

        [Test]
        public void UnknownPlayer_RejectsReconnect()
        {
            var room = RoomWithRecord(
                "player",
                DateTime.Now.AddSeconds(-10).Ticks,
                DateTime.UtcNow.AddSeconds(-10).Ticks);
            Assert.IsFalse(Allowed(room, "someone-else"));
            Assert.IsFalse(Allowed(room, " "));
        }

        [Test]
        public void NoReconnectRecords_RejectsReconnect()
        {
            var room = new LobbyRoom(
                "lobby-1", "ABCD", "room", 4, false, "host-1", "relay-code",
                null, state: LobbyState.Started);
            Assert.IsFalse(Allowed(room, "player"));
            Assert.IsFalse(Allowed(null, "player"));
        }
    }
}
