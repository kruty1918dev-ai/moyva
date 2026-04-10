# Multiplayer — INetworkProvider та провайдери

← [Назад до огляду](README.md)

---

## Концепція

`INetworkProvider` — абстракція мережевого транспорту.  
`SessionManager` залежить лише від цього інтерфейсу, а не від конкретного бекенду.

Це дозволяє:
- **Тестувати** SessionManager без жодного мережевого з'єднання (`OfflineNetworkProvider`).
- **Перемикати** бекенди (Relay ↔ Mirror ↔ Offline) без зміни будь-якої ігрової логіки.
- **Розширювати** систему майбутніми транспортами без модифікацій ядра.

---

## Інтерфейс `INetworkProvider`

**Файл:** `API/INetworkProvider.cs`

```csharp
public interface INetworkProvider
{
    // Спостерігач за вхідними повідомленнями (IObservable)
    IObservable<NetworkMessage> Messages { get; }

    // Події підключення/відключення однорангових вузлів
    event Action<string> PeerConnected;
    event Action<string> PeerDisconnected;

    // Стати хостом сесії
    Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default);

    // Приєднатись до існуючої сесії
    Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default);

    // Покинути сесію
    Task LeaveSessionAsync(CancellationToken ct = default);

    // Надіслати повідомлення конкретному пірові
    Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default);
}
```

---

## DTOs

### `SessionResult`

Повертається з `HostSessionAsync` і `JoinSessionAsync`.

```csharp
public sealed class SessionResult
{
    public bool Success { get; }
    public string SessionId { get; }
    public string ErrorMessage { get; }

    // Фабричні методи
    public static SessionResult Ok(string sessionId);
    public static SessionResult Fail(string error);
}
```

### `NetworkMessage`

Вхідне повідомлення від іншого піра.

```csharp
public sealed class NetworkMessage
{
    public string SenderId { get; }   // ID відправника
    public byte[] Payload { get; }    // сирі байти повідомлення
}
```

---

## OfflineNetworkProvider

**Файл:** `Runtime/OfflineNetworkProvider.cs`

Реалізація без реального мережевого з'єднання. Призначена для:
- **offline/single-player** режиму (та сама API, що і мережева),
- **unit- та integration-тестів** SessionManager.

### Поведінка

| Метод | Поведінка |
|---|---|
| `HostSessionAsync` | Завжди `SessionResult.Ok(sessionId)`, встановлює внутрішній прапор `_isHosting = true` |
| `JoinSessionAsync` | `Ok` якщо `_isHosting = true`, `Fail` якщо немає активної сесії |
| `LeaveSessionAsync` | Скидає `_isHosting = false` |
| `SendMessageAsync` | Loopback — надсилає повідомлення назад всім підписникам `Messages` |

### Приклад

```csharp
var network = new OfflineNetworkProvider();

// Підписатись на повідомлення
network.Messages.Subscribe(msg => Debug.Log($"Msg from {msg.SenderId}"));

// Хост відкриває сесію
var result = await network.HostSessionAsync("my-room");
// result.Success == true

// Клієнт приєднується
var join = await network.JoinSessionAsync("my-room");
// join.Success == true (бо _isHosting = true)

// Надіслати повідомлення (loopback)
await network.SendMessageAsync("peer-1", new byte[] { 1, 2, 3 });
// підписники Messages отримають повідомлення від "local"
```

---

## RelayNetworkProvider (заглушка)

> **Статус:** Carcass — тільки типи визначені, конкретна реалізація буде пізніше.

Unity Relay — хмарний транспорт Unity Gaming Services:
- Не потребує відкритих портів (NAT traversal).
- Гравці підключаються через relay-сервер, не напряму.
- Генерує **Join Code** для запрошення інших.

**Що потрібно реалізувати:**
- Ініціалізацію через `Unity.Services.Core.UnityServices.InitializeAsync()`.
- `HostSessionAsync` → `RelayService.Instance.CreateAllocationAsync()` + `JoinAllocationAsync`.
- `JoinSessionAsync` → `RelayService.Instance.JoinAllocationAsync(joinCode)`.
- Передавати Join Code через `SessionResult.SessionId`.

---

## MirrorNetworkProvider (заглушка)

> **Статус:** Carcass — тільки типи визначені.

Mirror — open-source networking framework для Unity:
- Підтримує TCP/UDP/WebSockets.
- Host-authoritative модель.
- Підходить для LAN або direct-connect.

**Що потрібно реалізувати:**
- `HostSessionAsync` → `NetworkManager.StartHost()`.
- `JoinSessionAsync` → `NetworkManager.StartClient()` з IP/port.
- Перетворити Mirror callbacks (`OnServerConnect`, `OnClientConnect`) на `PeerConnected` event.

---

## Як додати власний провайдер

1. Створіть клас у `Runtime/` і реалізуйте `INetworkProvider`.
2. Зареєструйте через DI або передайте напряму у `SessionManager`.
3. Додайте значення до `NetworkProviderType` enum.
4. Оновіть `BinaryConfigStore` (новий int-маппінг автоматичний).
5. Виберіть тип у `Config Hub`.

```csharp
public sealed class CustomNetworkProvider : INetworkProvider
{
    public IObservable<NetworkMessage> Messages => /* your observable */;
    public event Action<string> PeerConnected;
    public event Action<string> PeerDisconnected;

    public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct)
    {
        // Ваша логіка
        return Task.FromResult(SessionResult.Ok(sessionId));
    }

    // ... інші методи
}
```

---

## Вибір провайдера на основі конфігу

```csharp
var config = configStore.Load();

INetworkProvider provider = config.ProviderType switch
{
    NetworkProviderType.Offline => new OfflineNetworkProvider(),
    NetworkProviderType.Relay   => new RelayNetworkProvider(),   // TODO
    NetworkProviderType.Mirror  => new MirrorNetworkProvider(),  // TODO
    _                           => new OfflineNetworkProvider()
};

var manager = new SessionManager(provider, /* ... */);
```
