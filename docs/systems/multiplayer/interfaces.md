# Multiplayer — Довідник інтерфейсів

← [Назад до огляду](README.md)

---

## Огляд

Усі інтерфейси знаходяться у папці `API/`.  
Runtime-код залежить **лише** від інтерфейсів — це дозволяє підміняти реалізації без зміни SessionManager або тестів.

---

## `INetworkProvider`

**Файл:** `API/INetworkProvider.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Networking`

Абстракція мережевого транспорту. Реалізації: `OfflineNetworkProvider`, `RelayNetworkProvider` (заглушка), `MirrorNetworkProvider` (заглушка).

```csharp
public interface INetworkProvider
{
    IObservable<NetworkMessage> Messages { get; }   // стрім вхідних повідомлень
    event Action<string> PeerConnected;             // peer підключився
    event Action<string> PeerDisconnected;          // peer відключився

    Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default);
    Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default);
    Task LeaveSessionAsync(CancellationToken ct = default);
    Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default);
}
```

**DTOs:**

```csharp
// Результат операцій Host/Join
public sealed class SessionResult
{
    public bool Success { get; }
    public string SessionId { get; }
    public string ErrorMessage { get; }

    public static SessionResult Ok(string sessionId);
    public static SessionResult Fail(string error);
}

// Повідомлення між пірами
public sealed class NetworkMessage
{
    public string SenderId { get; }
    public byte[] Payload { get; }
}
```

---

## `ISessionManager`

**Файл:** `API/ISessionManager.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Головний оркестратор сесії.

```csharp
public interface ISessionManager
{
    Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default);
    Task LeaveSessionAsync(CancellationToken ct = default);
}
```

Конкретна реалізація: [`SessionManager`](session-manager.md).

---

## `IConfigStore`

**Файл:** `API/IConfigStore.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Config`

Runtime-friendly абстракція для завантаження/збереження `MultiplayerConfig`.  
Не залежить від `UnityEditor`.

```csharp
public interface IConfigStore
{
    MultiplayerConfig Load();
    void Save(MultiplayerConfig config);
    bool Exists();
}
```

Конкретна реалізація: [`BinaryConfigStore`](config-store.md).

---

## `IWorldSnapshotStore`

**Файл:** `API/IWorldSnapshotStore.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

Міст між multiplayer-системою і існуючою save-системою.  
Дозволяє зберігати/завантажувати метадані snapshot без повторної реалізації SaveService.

```csharp
public interface IWorldSnapshotStore
{
    bool Exists(string worldId);
    WorldSnapshot Load(string worldId);
    void Save(WorldSnapshot snapshot);
}
```

---

## `IWorldConsistencyService`

**Файл:** `API/IWorldConsistencyService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Порівнює snapshot клієнта зі snapshot хоста.

```csharp
public interface IWorldConsistencyService
{
    ConsistencyCheckResult Compare(WorldSnapshot host, WorldSnapshot client);
}
```

Конкретна реалізація: [`WorldConsistencyService`](world-consistency.md).

---

## `IParticipantPolicyService`

**Файл:** `API/IParticipantPolicyService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Перевіряє, чи може учасник приєднатись до сесії.

```csharp
public interface IParticipantPolicyService
{
    bool CanJoin(
        ParticipantIdentity candidate,
        IReadOnlyList<Participant> currentParticipants,
        SessionRules rules,
        WorldSnapshot worldSnapshot);   // null якщо world не існує
}
```

Перевіряє:
- ліміт загальної кількості учасників,
- ліміт людей і ботів окремо,
- strict 4-player world lock.

Конкретна реалізація: [`ParticipantPolicyService`](participant-policy.md).

---

## `IMultiplayerLogger`

**Файл:** `API/IMultiplayerLogger.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Абстракція логування для multiplayer-підсистеми.

```csharp
public interface IMultiplayerLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Trace(string message);
}
```

Конкретна реалізація: [`UnityMultiplayerLogger`](logging-and-errors.md).

---

## `IFailureHandlingPolicy`

**Файл:** `API/IFailureHandlingPolicy.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Визначає, що робити при recoverable та non-recoverable помилках.

```csharp
public interface IFailureHandlingPolicy
{
    // Повертає true = retry, false = abort
    bool HandleRecoverable(FailureCategory category, string details);

    void HandleNonRecoverable(FailureCategory category, string details);
}
```

Конкретна реалізація: [`SimpleFailureHandlingPolicy`](logging-and-errors.md).

---

## `IWorldCloneService`

**Файл:** `API/IWorldCloneService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

Клонує світ із новими правилами і перемапленими слотами.

```csharp
public interface IWorldCloneService
{
    string CloneWorld(string sourceWorldId, SessionRules newRules, SlotMapping mapping);
}
```

> Реалізація: carcass — заглушка, конкретна логіка реалізовується пізніше.

---

## `IParticipantFallbackService`

**Файл:** `API/IParticipantFallbackService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Що робити, коли учасник виходить (наприклад, замінити ботом).

```csharp
public interface IParticipantFallbackService
{
    Participant GetFallback(
        ParticipantIdentity leavingParticipant,
        IReadOnlyList<Participant> remaining,
        SessionRules rules);
}
```

> Реалізація: carcass — заглушка.

---

## `IHostMigrationService`

**Файл:** `API/IHostMigrationService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Обирає нового хоста серед учасників, що залишились.

```csharp
public interface IHostMigrationService
{
    Participant ChooseNewHost(IReadOnlyList<Participant> remaining);
}
```

> Реалізація: carcass — заглушка. Повертає null якщо немає кандидатів.

---

## `IConfigSyncService`

**Файл:** `API/IConfigSyncService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Config`

Оновлює локальний конфіг з авторитарного конфігу хоста.

```csharp
public interface IConfigSyncService
{
    void SyncFromHost(MultiplayerConfig hostConfig);
}
```

> Реалізація: carcass — заглушка.

---

## Таблиця реалізацій

| Інтерфейс | Конкретна реалізація | Статус |
|---|---|---|
| `INetworkProvider` | `OfflineNetworkProvider` | ✅ Реалізовано |
| `INetworkProvider` | `RelayNetworkProvider` | 🔲 Заглушка |
| `INetworkProvider` | `MirrorNetworkProvider` | 🔲 Заглушка |
| `ISessionManager` | `SessionManager` | ✅ Реалізовано |
| `IConfigStore` | `BinaryConfigStore` | ✅ Реалізовано |
| `IWorldSnapshotStore` | _(вбудована save-система)_ | 🔲 Потребує обгортки |
| `IWorldConsistencyService` | `WorldConsistencyService` | ✅ Реалізовано |
| `IParticipantPolicyService` | `ParticipantPolicyService` | ✅ Реалізовано |
| `IMultiplayerLogger` | `UnityMultiplayerLogger` | ✅ Реалізовано |
| `IFailureHandlingPolicy` | `SimpleFailureHandlingPolicy` | ✅ Реалізовано |
| `IWorldCloneService` | _(заглушка)_ | 🔲 Carcass |
| `IParticipantFallbackService` | _(заглушка)_ | 🔲 Carcass |
| `IHostMigrationService` | _(заглушка)_ | 🔲 Carcass |
| `IConfigSyncService` | _(заглушка)_ | 🔲 Carcass |
