# Multiplayer — TDD та Unit-тести

← [Назад до огляду](README.md)

---

## Принципи TDD у цьому модулі

1. **Спочатку тест** — код написаний після того, як тест описав очікувану поведінку.
2. **Мінімальна реалізація** — кожен сервіс робить рівно стільки, скільки потрібно для проходження тестів.
3. **Ізольованість** — тести не мають залежностей від Unity Engine (де можливо).
4. **Fake замість Mock** — замість Moq/NSubstitute використовуються прості sealed class фейки.

---

## Структура тестів

```
Assets/Moyva/Scripts/Tests/Multiplayer/
├── Kruty1918.Moyva.Tests.Multiplayer.asmdef   ← тест-збірка
├── SessionRulesTests.cs                        ← тести SessionRules
├── ParticipantPolicyServiceTests.cs            ← тести ParticipantPolicyService
├── WorldConsistencyServiceTests.cs             ← тести WorldConsistencyService
└── SessionManagerTests.cs                      ← інтеграційні тести SessionManager
```

---

## Assembly Definition

```json
{
    "name": "Kruty1918.Moyva.Tests.Multiplayer",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Kruty1918.Moyva.Multiplayer"
    ],
    "includePlatforms": ["Editor"],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"]
}
```

---

## SessionRulesTests

Тестує значення та дефолтні налаштування `SessionRules`.

```csharp
[Test]
public void Default_ShouldHaveMaxParticipants4()
{
    var rules = SessionRules.Default();
    Assert.AreEqual(4, rules.MaxParticipants);
}

[Test]
public void Default_ShouldNotHaveStrictLock()
{
    var rules = SessionRules.Default();
    Assert.IsFalse(rules.StrictParticipantLock);
}

[Test]
public void Constructor_SetsAllProperties()
{
    var rules = new SessionRules(
        SessionMode.MixedHumansAndBots, 4, 2, 2, true, true, true);

    Assert.AreEqual(SessionMode.MixedHumansAndBots, rules.Mode);
    Assert.AreEqual(4, rules.MaxParticipants);
    Assert.AreEqual(2, rules.MaxHumans);
    Assert.AreEqual(2, rules.MaxBots);
    Assert.IsTrue(rules.AllowBotsFallbackOnLeave);
    Assert.IsTrue(rules.StrictParticipantLock);
}
```

---

## ParticipantPolicyServiceTests

Тестує логіку допуску учасників.

### Fake залежності

```csharp
private sealed class FakeLogger : IMultiplayerLogger
{
    public void Info(string msg) { }
    public void Warn(string msg) { }
    public void Error(string msg) { }
    public void Trace(string msg) { }
}

private sealed class FakeSnapshotStore : IWorldSnapshotStore
{
    public bool Exists(string worldId) => false;
    public WorldSnapshot Load(string worldId) => null;
    public void Save(WorldSnapshot snapshot) { }
}
```

### Ключові тести

| Тест | Очікуваний результат |
|---|---|
| Порожня сесія | `CanJoin = true` |
| 4/4 учасники | `CanJoin = false` (session full) |
| Ліміт людей 2/2 | `CanJoin = false` (max humans) |
| Ліміт ботів 1/1 | `CanJoin = false` (max bots) |
| Вільний слот для бота | `CanJoin = true` |
| Загальний ліміт (3/3) | `CanJoin = false` |

---

## WorldConsistencyServiceTests

Тестує порівняння snapshot.

### Ключові тести

| Тест | Очікуваний результат |
|---|---|
| Однакові WorldId + Checksum | `Equal` |
| Різні Checksum | `WorldMismatch` |
| Різні WorldId | `WorldMismatch` |
| `null` snapshot (host або client) | `WorldMismatch` |

### Приклад

```csharp
[Test]
public void Compare_ShouldReturnEqual_WhenChecksumAndIdMatch()
{
    var host   = new WorldSnapshot("world-1", 1, 0xDEADBEEF);
    var client = new WorldSnapshot("world-1", 1, 0xDEADBEEF);

    var result = _service.Compare(host, client);

    Assert.AreEqual(ConsistencyCheckResult.Equal, result);
}

[Test]
public void Compare_ShouldReturnWorldMismatch_WhenChecksumsDiffer()
{
    var host   = new WorldSnapshot("world-1", 1, 0x11111111);
    var client = new WorldSnapshot("world-1", 1, 0x22222222);

    var result = _service.Compare(host, client);

    Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, result);
}
```

---

## SessionManagerTests

Інтеграційні тести через `OfflineNetworkProvider`.  
Тестують повний flow `CreateOrJoinSessionAsync` + `LeaveSessionAsync`.

### Fakes у тесті

```csharp
private sealed class FakeConfigStore : IConfigStore
{
    public MultiplayerConfig Config { get; set; } = MultiplayerConfig.Default();
    public MultiplayerConfig Load() => Config;
    public void Save(MultiplayerConfig config) => Config = config;
    public bool Exists() => true;
}

private sealed class FakeSnapshotStore : IWorldSnapshotStore
{
    private readonly Dictionary<string, WorldSnapshot> _store = new();
    public bool Exists(string id) => _store.ContainsKey(id);
    public WorldSnapshot Load(string id) => _store.GetValueOrDefault(id);
    public void Save(WorldSnapshot s) => _store[s.WorldId] = s;
}
```

### Ключові тести

| Тест | Що перевіряє |
|---|---|
| Create offline session | Повертає `true`, 1 учасник, IsHost = true |
| Join after create | Другий гравець приєднується успішно |
| Leave clears participants | Після Leave список порожній |
| Config checksum mismatch | Повертає `false` при невідповідному checksum |
| Session full (1/1) | Другий гравець отримує відмову |

### Приклад helper-методу

```csharp
private SessionManager BuildManager(
    INetworkProvider network = null,
    IConfigStore configStore = null,
    IWorldSnapshotStore snapshotStore = null)
{
    var logger = new FakeLogger();
    var failPolicy = new FakeFailurePolicy();
    var snapStore = snapshotStore ?? new FakeSnapshotStore();
    var cfgStore = configStore ?? new FakeConfigStore();
    var policy = new ParticipantPolicyService(logger, snapStore);
    var consistency = new WorldConsistencyService(logger);
    var net = network ?? new OfflineNetworkProvider();

    return new SessionManager(net, policy, consistency, snapStore, cfgStore, logger, failPolicy);
}
```

---

## Як додати нові тести

### 1. Тест для нового сервісу

Якщо ти реалізуєш `IHostMigrationService`:

```csharp
[TestFixture]
public class HostMigrationServiceTests
{
    private IHostMigrationService _service;

    [SetUp]
    public void SetUp()
    {
        _service = new HostMigrationService(new FakeLogger());
    }

    [Test]
    public void ChooseNewHost_ShouldReturnFirstNonBot_WhenAvailable()
    {
        var remaining = new List<Participant>
        {
            new Participant(new ParticipantIdentity("BOT_1", "Bot"), true, false),
            new Participant(new ParticipantIdentity("p2", "Player2"), false, false),
        };

        var newHost = _service.ChooseNewHost(remaining);

        Assert.NotNull(newHost);
        Assert.IsFalse(newHost.IsBot);
        Assert.AreEqual("p2", newHost.Identity.PlayerId);
    }

    [Test]
    public void ChooseNewHost_ShouldReturnNull_WhenNoParticipantsLeft()
    {
        var newHost = _service.ChooseNewHost(new List<Participant>());
        Assert.IsNull(newHost);
    }
}
```

### 2. Тест через TestRunner

1. Відкрити `Window → General → Test Runner`.
2. Перейти на вкладку `EditMode`.
3. Знайти `Kruty1918.Moyva.Tests.Multiplayer`.
4. Запустити окремий тест або всі.

---

## InternalsVisibleTo

Деякі методи (наприклад, `SessionManager.ComputeConfigChecksum`) — `internal`.  
Тести мають доступ через `InternalsVisibleTo`:

```csharp
// Assets/Moyva/Scripts/Features/Multiplayer/Runtime/AssemblyInfo.cs
[assembly: InternalsVisibleTo("Kruty1918.Moyva.Tests.Multiplayer")]
```

---

## Подальше покриття тестами

> **Статус**: більшість пунктів нижче покрито у `MultiplayerExtendedTests.cs` (80 тестів).
> Перевірити, що наступні сценарії також покриті:

- `BinaryConfigStore.WriteConfig` / `ReadConfig` — round-trip тест
- `OfflineNetworkProvider` — підписка на `Messages`, loopback
- `SimpleFailureHandlingPolicy` — перевірка виклику logger
- `SessionManager.ComputeConfigChecksum` — детермінованість і різні конфіги
