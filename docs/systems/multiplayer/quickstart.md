# Multiplayer — Швидкий старт

← [Назад до огляду](README.md)

---

## Мета

За 10 хвилин ти зрозумієш, як:
1. Відкрити та налаштувати мультиплеєр через редактор.
2. Запустити offline-сесію у коді.
3. Написати перший тест для своєї логіки.

---

## Крок 1 — Відкрити Multiplayer Config Hub

У Unity Editor:

```
Moyva → Multiplayer → Config Hub
```

Якщо конфіг-файл ще не існує, відкриється вікно з **дефолтними налаштуваннями**.

---

## Крок 2 — Налаштувати базові параметри

У вікні Config Hub:

1. **Provider Type** → обери `Offline` для локальних тестів або `Relay` для мережі.
2. **Default Session Mode** → обери `MultiplayerHumans`.
3. **Max Participants** → встанови `4` (максимум).
4. **Max Humans** → `4`, **Max Bots** → `0`.
5. Натисни **Save Config**.

Файл збережеться у: `Assets/Moyva/multiplayer_config.dat`

---

## Крок 3 — Запуск offline-сесії у коді

```csharp
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;

// 1. Підготувати залежності
var logger = new UnityMultiplayerLogger();
var failPolicy = new SimpleFailureHandlingPolicy(logger);
var configStore = new BinaryConfigStore("Assets/Moyva/multiplayer_config.dat");
var snapshotStore = new InMemorySnapshotStore();         // своя реалізація або фейк
var participantPolicy = new ParticipantPolicyService(logger, snapshotStore);
var consistencyService = new WorldConsistencyService(logger);
var network = new OfflineNetworkProvider();

// 2. Створити SessionManager
var manager = new SessionManager(
    network, participantPolicy, consistencyService,
    snapshotStore, configStore, logger, failPolicy);

// 3. Визначити ідентичність гравця
var identity = new ParticipantIdentity("player-001", "Kruty");
var rules = SessionRules.Default();

// 4. Створити сесію (хост)
var options = new SessionConnectOptions(
    localIdentity: identity,
    roomId: "my-room-01",
    createIfNotExists: true,
    rules: rules,
    configChecksum: 0);

bool ok = await manager.CreateOrJoinSessionAsync(options);

if (ok)
    Debug.Log($"Сесія створена! Учасників: {manager.Participants.Count}");
```

---

## Крок 4 — Приєднатись до існуючої сесії

```csharp
// Інший клієнт (або той самий у тесті) приєднується до кімнати
var joiner = new ParticipantIdentity("player-002", "Guest");
var joinOptions = new SessionConnectOptions(
    joiner, "my-room-01", createIfNotExists: false, rules, 0);

bool joined = await manager.CreateOrJoinSessionAsync(joinOptions);
Debug.Log($"Joined: {joined}");
```

> **Примітка:** у `OfflineNetworkProvider` приєднання можливе лише до вже відкритої сесії (`HostSessionAsync` був викликаний раніше).

---

## Крок 5 — Покинути сесію

```csharp
await manager.LeaveSessionAsync();
Debug.Log("Вийшли з сесії.");
```

---

## Крок 6 — Написати перший тест

```csharp
[Test]
public async Task MyFirstMultiplayerTest()
{
    var logger = new FakeLogger();
    var snapshotStore = new FakeSnapshotStore();
    var configStore = new FakeConfigStore();
    var policy = new ParticipantPolicyService(logger, snapshotStore);
    var consistency = new WorldConsistencyService(logger);
    var network = new OfflineNetworkProvider();
    var failPolicy = new FakeFailurePolicy();

    var manager = new SessionManager(
        network, policy, consistency, snapshotStore, configStore, logger, failPolicy);

    var identity = new ParticipantIdentity("p1", "TestPlayer");
    var options = new SessionConnectOptions(identity, "room-x", true, SessionRules.Default(), 0);

    bool result = await manager.CreateOrJoinSessionAsync(options);

    Assert.IsTrue(result);
    Assert.AreEqual(1, manager.Participants.Count);
    Assert.IsTrue(manager.Participants[0].IsHost);
}
```

---

## Що далі

| Тема | Посилання |
|---|---|
| Розуміння архітектури | [architecture.md](architecture.md) |
| Усі інтерфейси | [interfaces.md](interfaces.md) |
| Правило 4 гравців | [participant-policy.md](participant-policy.md) |
| Порівняння світів | [world-consistency.md](world-consistency.md) |
| Config Hub редактор | [config-hub-guide.md](config-hub-guide.md) |
| TDD та тести | [testing.md](testing.md) |
