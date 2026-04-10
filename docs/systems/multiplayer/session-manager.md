# SessionManager — Детальний опис

← [Назад до огляду](README.md)

---

## Призначення

`SessionManager` — центральний оркестратор мультиплеєрної сесії.  
Він не знає нічого про ігрову логіку. Його задача:

1. Завантажити конфіг.
2. Перевірити сумісність конфігу (checksum).
3. Перевірити, чи може учасник приєднатись.
4. Викликати мережевого провайдера для hosting/joining.
5. Додати учасника до внутрішнього стану.
6. Логувати всі кроки.

---

## Розташування

```
Assets/Moyva/Scripts/Features/Multiplayer/Runtime/SessionManager.cs
```

**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

---

## Сигнатура класу

```csharp
public sealed class SessionManager : ISessionManager
```

---

## Конструктор (Dependency Injection)

```csharp
public SessionManager(
    INetworkProvider network,
    IParticipantPolicyService participantPolicy,
    IWorldConsistencyService consistency,
    IWorldSnapshotStore snapshotStore,
    IConfigStore configStore,
    IMultiplayerLogger logger,
    IFailureHandlingPolicy failurePolicy)
```

Усі параметри обов'язкові. `ArgumentNullException` при передачі `null`.

---

## Публічний API

### `CreateOrJoinSessionAsync`

```csharp
public async Task<bool> CreateOrJoinSessionAsync(
    SessionConnectOptions options,
    CancellationToken ct = default)
```

**Параметри:**

| Параметр | Опис |
|---|---|
| `options.LocalIdentity` | Ідентичність локального гравця |
| `options.RoomId` | ID кімнати/сесії |
| `options.CreateIfNotExists` | `true` = хост, `false` = клієнт |
| `options.Rules` | Правила сесії |
| `options.ConfigChecksum` | Checksum конфігу від хоста (`0` = не перевіряти) |

**Повертає:** `true` при успіху, `false` при будь-якій помилці.

**Алгоритм:**

```
1. Завантажити MultiplayerConfig через IConfigStore
2. Якщо EnforceConfigConsistency = true:
     обчислити localChecksum = ComputeConfigChecksum(config)
     якщо options.ConfigChecksum ≠ 0 і ≠ localChecksum → false (ConfigMismatch)
3. Завантажити WorldSnapshot для options.RoomId (або null якщо не існує)
4. Перевірити CanJoin через IParticipantPolicyService → false якщо rejected
5. Якщо CreateIfNotExists:
     викликати INetworkProvider.HostSessionAsync
   Інакше:
     викликати INetworkProvider.JoinSessionAsync
6. Якщо network.Success = false → false (NetworkDisconnect)
7. Додати учасника до _participants
8. Повернути true
```

---

### `LeaveSessionAsync`

```csharp
public async Task LeaveSessionAsync(CancellationToken ct = default)
```

- Викликає `INetworkProvider.LeaveSessionAsync`.
- Очищає список учасників.
- Скидає `_currentSessionId` до `null`.

---

### `Participants`

```csharp
public IReadOnlyList<Participant> Participants { get; }
```

Список учасників поточної сесії (read-only).

---

## Обчислення checksum конфігу

`SessionManager` реалізує внутрішній FNV-1a хеш для перевірки сумісності конфіга між клієнтами:

```csharp
internal static uint ComputeConfigChecksum(MultiplayerConfig config)
```

Хешуються поля:
- `SchemaVersion`
- `ProviderType`
- `StrictParticipantLock`
- `EnforceConfigConsistency`
- `DefaultSessionRules.Mode`
- `DefaultSessionRules.MaxParticipants`

> Цей метод `internal` — доступний з тестового проєкту через `InternalsVisibleTo`.

---

## Приклад використання

### Хост створює сесію

```csharp
var options = new SessionConnectOptions(
    new ParticipantIdentity("host-001", "Host"),
    roomId: "room-42",
    createIfNotExists: true,
    rules: SessionRules.Default(),
    configChecksum: 0);

bool ok = await sessionManager.CreateOrJoinSessionAsync(options);
```

### Клієнт приєднується

```csharp
// Клієнт розраховує checksum хоста (або отримує по іншому каналу)
uint hostChecksum = SessionManager.ComputeConfigChecksum(localConfig);

var options = new SessionConnectOptions(
    new ParticipantIdentity("client-001", "Guest"),
    roomId: "room-42",
    createIfNotExists: false,
    rules: SessionRules.Default(),
    configChecksum: hostChecksum);

bool ok = await sessionManager.CreateOrJoinSessionAsync(options);
```

### Вихід

```csharp
await sessionManager.LeaveSessionAsync();
```

---

## Обробка помилок

`SessionManager` ніколи не кидає виключень у нормальному потоці.  
Усі помилки передаються в `IFailureHandlingPolicy`:

| Ситуація | Категорія | Повернення |
|---|---|---|
| Config checksum mismatch | `ConfigMismatch` | `false` |
| Participant rejected by policy | `ParticipantRejected` | `false` |
| Network operation failed | `NetworkDisconnect` | `false` |

Логування відбувається через `IMultiplayerLogger` на кожному кроці.

---

## Розширення та наступні кроки

- Додати підписку на `INetworkProvider.PeerDisconnected` для хост-міграції
- Реалізувати `IHostMigrationService` і підключити до `SessionManager`
- Додати механізм надсилання checksum клієнтам при host-з'єднанні
- Інтегрувати `IWorldConsistencyService` при приєднанні до існуючого світу

---

## Залежності (граф)

```
SessionManager
├── INetworkProvider          → OfflineNetworkProvider (або Relay/Mirror)
├── IParticipantPolicyService → ParticipantPolicyService
│       └── IWorldSnapshotStore
├── IWorldConsistencyService  → WorldConsistencyService
├── IWorldSnapshotStore       → (реалізація на стороні save-системи)
├── IConfigStore              → BinaryConfigStore
├── IMultiplayerLogger        → UnityMultiplayerLogger
└── IFailureHandlingPolicy    → SimpleFailureHandlingPolicy
```
