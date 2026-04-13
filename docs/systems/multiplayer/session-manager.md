# SessionManager — Детальний опис

← [Назад до огляду](README.md)

---

## Призначення

`SessionManager` — центральний оркестратор мультиплеєрної сесії.  
Він не знає нічого про ігрову логіку. Його задача:

1. Завантажити конфіг і перевірити checksum.
2. Перевірити правила участі гравця.
3. Викликати мережевого провайдера для hosting/joining.
4. Керувати списком учасників протягом усієї сесії.
5. Автоматично обробляти відключення: **міграція хоста** + **бот-замінник**.
6. Логувати всі кроки через `IMultiplayerLogger`.

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
    INetworkProvider           network,
    IParticipantPolicyService  participantPolicy,
    IWorldConsistencyService   consistency,
    IWorldSnapshotStore        snapshotStore,
    IConfigStore               configStore,
    IMultiplayerLogger         logger,
    IFailureHandlingPolicy     failurePolicy,
    IHostMigrationService      hostMigration,      // нове
    IParticipantFallbackService participantFallback) // нове
```

Усі параметри обов'язкові. `ArgumentNullException` при передачі `null`.

> `IHostMigrationService` та `IParticipantFallbackService` були додані для повноцінного
> автоматичного керування учасниками при відключенні.

---

## Публічний API

### `Participants`

```csharp
public IReadOnlyList<Participant> Participants { get; }
```

Поточний список учасників сесії (read-only). Оновлюється автоматично при приєднанні та відключенні.

---

### `CreateOrJoinSessionAsync`

```csharp
public async Task<bool> CreateOrJoinSessionAsync(
    SessionConnectOptions options,
    CancellationToken ct = default)
```

**Параметри `SessionConnectOptions`:**

| Поле | Тип | Опис |
|---|---|---|
| `LocalIdentity` | `ParticipantIdentity` | Ідентичність локального гравця |
| `RoomId` | `string` | ID кімнати. Порожній рядок → автоматичний solo fallback |
| `CreateIfNotExists` | `bool` | `true` = хост, `false` = клієнт |
| `Rules` | `SessionRules` | Правила сесії (або `null` → з конфіга) |
| `ConfigChecksum` | `uint` | Checksum конфіга від хоста. `0` = не перевіряти |

**Алгоритм:**

```
1. Load MultiplayerConfig via IConfigStore
2. Normalize options (fill missing RoomId, Rules, Identity)
3. If EnforceConfigConsistency = true AND ConfigChecksum ≠ 0:
     Compare local checksum → false if mismatch
4. Load WorldSnapshot (or null) for RoomId
5. ParticipantPolicyService.CanJoin(...) → false if rejected
6. HostSessionAsync (create) or JoinSessionAsync (join) via INetworkProvider
7. If network fails → try offline solo fallback, else → false
8. Add local participant to _participants
9. Save _currentRules for use in OnPeerDisconnected
10. Return true
```

**Повертає:** `true` при успіху, `false` при будь-якій помилці.

---

### `LeaveSessionAsync`

```csharp
public async Task LeaveSessionAsync(CancellationToken ct = default)
```

- Відписується від `PeerDisconnected` (щоб уникнути зворотного виклику при виході).
- Викликає `INetworkProvider.LeaveSessionAsync`.
- Очищає `_participants`, `_currentSessionId`, `_currentRules`.

---

## Автоматична обробка відключень

`SessionManager` підписується на `INetworkProvider.PeerDisconnected` **у конструкторі**:

```csharp
_network.PeerDisconnected += OnPeerDisconnected;
```

При відключенні піра викликається `OnPeerDisconnected(string peerId)`:

```
1. Знайти учасника в _participants за PlayerId
2. Видалити учасника
3. Якщо це був хост і є інші учасники:
     IHostMigrationService.ChooseNewHost(remaining)
     Оновити запис відповідного учасника як IsHost=true
4. IParticipantFallbackService.GetFallback(leaving, remaining, rules)
     Якщо повернуто бот-замінника → додати до _participants
```

> Детальна документація: [host-migration.md](./host-migration.md)

---

## Обчислення checksum конфігу

```csharp
internal static uint ComputeConfigChecksum(MultiplayerConfig config)
```

FNV-1a 32-bit хеш над ключовими полями конфіга:

- `SchemaVersion`
- `ProviderType`
- `StrictParticipantLock`
- `EnforceConfigConsistency`
- `DefaultSessionRules.Mode`
- `DefaultSessionRules.MaxParticipants`

> Метод `internal` — доступний з тестів через `InternalsVisibleTo`.

---

## Приклади використання

### Хост створює сесію

```csharp
var options = new SessionConnectOptions(
    new ParticipantIdentity("host-001", "Host"),
    roomId: "room-42",
    createIfNotExists: true,
    rules: SessionRules.Default(),
    configChecksum: 0);

bool ok = await sessionManager.CreateOrJoinSessionAsync(options);
if (ok) Debug.Log($"Сесія створена: {sessionManager.Participants.Count} учасник(ів)");
```

### Клієнт приєднується з перевіркою checksum

```csharp
// Клієнт отримує checksum від хоста (наприклад через окремий канал)
uint hostChecksum = SessionManager.ComputeConfigChecksum(localConfig);

var options = new SessionConnectOptions(
    new ParticipantIdentity("client-001", "Guest"),
    roomId: "room-42",
    createIfNotExists: false,
    rules: SessionRules.Default(),
    configChecksum: hostChecksum);

bool ok = await sessionManager.CreateOrJoinSessionAsync(options);
```

### Вихід із сесії

```csharp
await sessionManager.LeaveSessionAsync();
```

---

## Обробка помилок

`SessionManager` **ніколи не кидає** виключень у нормальному потоці.  
Усі помилки передаються в `IFailureHandlingPolicy`:

| Ситуація | Категорія | Повернення |
|---|---|---|
| Config checksum mismatch | `ConfigMismatch` | `false` |
| Participant rejected by policy | `ParticipantRejected` | `false` |
| Network operation failed | `NetworkDisconnect` | `false` |
| Offline solo fallback | — | `true` (fallback activated) |

---

## Граф залежностей

```
SessionManager
├── INetworkProvider            → OfflineNetworkProvider / Relay / WebSocket
│     └── PeerDisconnected event ← підписаний SessionManager
├── IParticipantPolicyService   → ParticipantPolicyService
│       └── IWorldSnapshotStore
├── IWorldConsistencyService    → WorldConsistencyService
├── IWorldSnapshotStore         → InMemoryWorldSnapshotStore / DiskWorldSnapshotStore
├── IConfigStore                → BinaryConfigStore
├── IMultiplayerLogger          → UnityMultiplayerLogger
├── IFailureHandlingPolicy      → SimpleFailureHandlingPolicy
├── IHostMigrationService       → HostMigrationService      ← нове
└── IParticipantFallbackService → ParticipantFallbackService ← нове
```

---

## Тести

```
Assets/Moyva/Scripts/Tests/Multiplayer/SessionManagerTests.cs
```

| Клас тестів | Що покриває |
|---|---|
| `SessionManagerTests` | Основний flow: create, join, leave, policy, checksum, fallback |
| `SessionManagerMigrationTests` | Міграція хоста, бот-замінник, невідомий пір |
