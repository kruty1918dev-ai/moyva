# Multiplayer — INetworkProvider та провайдери

← [Назад до огляду](README.md)

---

## Концепція

`INetworkProvider` — абстракція мережевого транспорту.  
`SessionManager` залежить лише від цього інтерфейсу, а не від конкретного бекенду.

Це дозволяє:
- **Тестувати** SessionManager без жодного мережевого з'єднання (`OfflineNetworkProvider`).
- **Перемикати** бекенди (Relay ↔ WebSocket ↔ Offline) без зміни будь-якої ігрової логіки.
- **Автоматично деградувати** через `FallbackNetworkProvider` — якщо primary не відповідає, система сама переходить на fallback.
- **Розширювати** систему майбутніми транспортами без модифікацій ядра.

---

## Підтримувані провайдери

| Тип (`NetworkProviderType`) | Клас | Статус |
|---|---|---|
| `Offline` | `OfflineNetworkProvider` | ✅ Повна реалізація |
| `Relay` | `RelayNetworkProvider` | ⚙️ Потребує UGS SDK (MOYVA_UGS_RELAY define) |
| `WebSocket` | `WebSocketNetworkProvider` | ✅ Повна реалізація |

---

## Інтерфейс `INetworkProvider`

**Файл:** `API/INetworkProvider.cs`

```csharp
public interface INetworkProvider
{
    IObservable<NetworkMessage> Messages { get; }
    event Action<string> PeerConnected;
    event Action<string> PeerDisconnected;

    Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default);
    Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default);
    Task LeaveSessionAsync(CancellationToken ct = default);
    Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default);
}
```

---

## OfflineNetworkProvider

**Файл:** `Runtime/OfflineNetworkProvider.cs`

Реалізація без мережі. Призначена для offline/single-player та unit-тестів.

| Метод | Поведінка |
|---|---|
| `HostSessionAsync` | Завжди `Ok`, встановлює `_isHosting = true` |
| `JoinSessionAsync` | `Ok` якщо `_isHosting = true`, інакше `Fail` |
| `SendMessageAsync` | Loopback — повертає повідомлення всім підписникам |

---

## RelayNetworkProvider

**Файл:** `Runtime/RelayNetworkProvider.cs`

Unity Gaming Services Relay — хмарний NAT traversal без відкритих портів.

### Активація

1. Встановіть пакет: `Window → Package Manager → Unity Gaming Services → Relay`
2. Додайте scripting define: `Edit → Project Settings → Player → Scripting Define Symbols → MOYVA_UGS_RELAY`
3. Перед викликом `HostSessionAsync` / `JoinSessionAsync` виконайте автентифікацію:

```csharp
await UnityServices.InitializeAsync();
await AuthenticationService.Instance.SignInAnonymouslyAsync();
```

### Як працює

- `HostSessionAsync` → `RelayService.Instance.CreateAllocationAsync(maxConnections, region)` → повертає **Join Code** як `SessionResult.SessionId`
- `JoinSessionAsync(joinCode)` → `RelayService.Instance.JoinAllocationAsync(joinCode)`
- Без `MOYVA_UGS_RELAY` provider повертає `SessionResult.Fail(...)`, `FallbackNetworkProvider` автоматично переходить на fallback.

### Налаштування (RelayProviderSettings)

| Поле | Опис |
|---|---|
| `ProjectId` | ID проекту з Unity Dashboard → Settings |
| `Environment` | `"production"` або `"development"` |
| `Region` | Регіон, наприклад `eu-west-1`. Порожнє = автовибір |
| `MaxConnections` | Максимальна кількість підключень в алокації |

---

## WebSocketNetworkProvider

**Файл:** `Runtime/WebSocketNetworkProvider.cs`

Підключається до власного WebSocket-сервера (ws:// або wss://).  
Використовує `System.Net.WebSockets.ClientWebSocket` — доступний на всіх платформах Unity, крім WebGL.

### Протокол

**Контрольні повідомлення** (текстові фрейми, UTF-8):

| Напрямок | Формат | Опис |
|---|---|---|
| → сервер | `HOST:<sessionId>:<peerId>` | Стати хостом кімнати |
| → сервер | `JOIN:<sessionId>:<peerId>` | Приєднатись до кімнати |
| ← сервер | `OK:<sessionId>` | Підтвердження |
| ← сервер | `ERR:<reason>` | Відмова |
| ← сервер | `PEER_CONNECTED:<peerId>` | Новий учасник |
| ← сервер | `PEER_DISCONNECTED:<peerId>` | Учасник вийшов |

**Дані** (бінарні фрейми):

```
[16 байт senderId (ASCII, доповнений нулями або пробілами)] [payload bytes]
```

### Налаштування (WebSocketProviderSettings)

| Поле | Опис |
|---|---|
| `ServerUrl` | URL сервера, наприклад `ws://localhost` або `wss://example.com` |
| `Port` | Порт, що додається до URL |
| `AuthToken` | Bearer-токен для Authorization header (необов'язково) |
| `ReconnectAttempts` | Кількість спроб перепідключення (0 = без перепідключення) |
| `ReconnectDelaySeconds` | Затримка між спробами перепідключення |

### Реконект

При розриві з'єднання `WebSocketNetworkProvider` автоматично:
1. Чекає `ReconnectDelaySeconds`
2. Повторно підключається та надсилає `JOIN:<sessionId>:<peerId>`
3. Якщо `ReconnectAttempts` вичерпані — повертає помилку (→ `FallbackNetworkProvider` переходить на fallback)

---

## FallbackNetworkProvider

**Файл:** `Runtime/FallbackNetworkProvider.cs`

Обгортка над двома провайдерами — primary і fallback.

```
HostSessionAsync / JoinSessionAsync
        │
        ▼ primary.HostSessionAsync()
    ┌───────────┐
    │  Success? │──── Так ──→ повертає результат
    └───────────┘
         │ Ні (Fail або Exception)
         ▼
    _usingFallback = true
    SubscribeTo(fallback)
         │
         ▼ fallback.HostSessionAsync()
    повертає результат
```

- Після першого перемикання всі наступні виклики йдуть напряму до fallback.
- Виклик `Reset()` повертає систему до primary (якщо він знову доступний).
- Підписники `Messages` прозоро отримують повідомлення від поточного активного провайдера.

---

## NetworkProviderFactory

**Файл:** `Runtime/NetworkProviderFactory.cs`

```csharp
// Автоматично будує ланцюг з конфігу:
INetworkProvider provider = NetworkProviderFactory.Create(config, logger);

// Якщо ProviderType != FallbackProviderType і ProviderType != Offline:
//   → повертає FallbackNetworkProvider(primary, fallback)
// Інакше:
//   → повертає одиночний провайдер без обгортки
```

---

## Вибір провайдера через Config Hub

Відкрийте `Moyva → Multiplayer → Config Hub` в Unity Editor:

1. Виберіть **Primary Provider** (Relay або WebSocket або Offline)
2. Виберіть **Fallback Provider** (зазвичай Offline або другий провайдер)
3. Заповніть поля **Unity Relay Settings** або **WebSocket Settings** — вони з'являються автоматично залежно від вибору
4. Натисніть **Save Config** — конфіг зберігається в `Assets/Moyva/multiplayer_config.dat`

---

## Як додати власний провайдер

1. Створіть клас у `Runtime/` і реалізуйте `INetworkProvider`.
2. Додайте значення до `NetworkProviderType` (не змінюйте існуючі значення — вони збережені у бінарному конфізі).
3. Додайте новий тип у `NetworkProviderFactory.CreateSingle()`.
4. За потреби додайте `ProviderSettings`-клас і поля в `MultiplayerConfig` / `BinaryConfigStore`.
