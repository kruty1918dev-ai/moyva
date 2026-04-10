# Multiplayer — Доменні моделі та enum-и

← [Назад до огляду](README.md)

---

## Enum-и

### `SessionMode`

**Файл:** `API/SessionMode.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Визначає режим гри у сесії.

| Значення | Опис |
|---|---|
| `PeacefulSolo` | Одиночна гра без ботів. Немає мережевих з'єднань. |
| `SoloWithBots` | Одиночна гра з ботами. Мережа не потрібна. |
| `MultiplayerHumans` | Мережева гра — тільки люди. |
| `MixedHumansAndBots` | Мережева гра: люди + боти займають вільні слоти. |

---

### `NetworkProviderType`

**Файл:** `API/NetworkProviderType.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Networking`

Визначає транспортний бекенд.

| Значення | Опис |
|---|---|
| `Relay` | Unity Relay (хмарне з'єднання без відкритих портів) |
| `Mirror` | Mirror Networking (LAN / direct) |
| `Offline` | Без мережі, лише локальна симуляція |

---

### `ConsistencyCheckResult`

**Файл:** `API/ConsistencyCheckResult.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Результат порівняння snapshot клієнта і хоста.

| Значення | Опис |
|---|---|
| `Equal` | Дані збігаються — можна продовжувати гру |
| `ConfigMismatch` | Конфігурація різна — потрібна синхронізація або відмова |
| `WorldMismatch` | Дані світу (checksum або worldId) відрізняються |

---

### `FailureCategory`

**Файл:** `API/FailureCategory.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Категорія помилки для маршрутизації в `IFailureHandlingPolicy`.

| Значення | Опис |
|---|---|
| `Unknown` | Невідома помилка |
| `NetworkDisconnect` | Мережеве з'єднання розірване |
| `ConfigMismatch` | Несумісна конфігурація |
| `WorldMismatch` | Стан світу не збігається |
| `ParticipantRejected` | Учасника відхилено (правила/ліміт) |
| `HostMigrationFailed` | Не вдалося перенести хост |
| `SessionFull` | Сесія заповнена |
| `StrictLockViolation` | Порушення strict 4-player world lock |

---

## Моделі

### `SessionRules`

**Файл:** `API/SessionRules.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Config`

Незмінний об'єкт правил сесії.

```csharp
public sealed class SessionRules
{
    public SessionMode Mode { get; }
    public int MaxParticipants { get; }   // максимум учасників (люди + боти), hard cap = 4
    public int MaxHumans { get; }          // максимум гравців-людей
    public int MaxBots { get; }            // максимум ботів
    public bool AllowBotsFallbackOnLeave { get; }     // замінити людину ботом при виході
    public bool AllowMatchSaveForAnalysis { get; }    // дозволити зберегти матч для аналізу
    public bool StrictParticipantLock { get; }        // lock 4-гравців: лише ті самі 4
}
```

**Дефолт** (`SessionRules.Default()`):

| Поле | Значення |
|---|---|
| Mode | `MultiplayerHumans` |
| MaxParticipants | `4` |
| MaxHumans | `4` |
| MaxBots | `0` |
| AllowBotsFallbackOnLeave | `false` |
| AllowMatchSaveForAnalysis | `false` |
| StrictParticipantLock | `false` |

**Приклад:**

```csharp
var rules = new SessionRules(
    SessionMode.MixedHumansAndBots,
    maxParticipants: 4,
    maxHumans: 2,
    maxBots: 2,
    allowBotsFallbackOnLeave: true,
    allowMatchSaveForAnalysis: false,
    strictParticipantLock: false);
```

---

### `MultiplayerConfig`

**Файл:** `API/MultiplayerConfig.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Config`

Авторитарна конфігурація мультиплеєра. Зберігається у бінарний файл.

```csharp
public sealed class MultiplayerConfig
{
    public int SchemaVersion { get; }                 // версія схеми файлу
    public NetworkProviderType ProviderType { get; }  // тип транспорту
    public SessionRules DefaultSessionRules { get; }  // правила за замовчуванням
    public bool StrictParticipantLock { get; }        // глобальний lock 4 гравців
    public bool EnforceConfigConsistency { get; }     // перевіряти checksum конфігу при вході
    public bool MatchmakingEnabled { get; }           // чи увімкнений matchmaking
}
```

**Дефолт** (`MultiplayerConfig.Default()`):

| Поле | Значення |
|---|---|
| SchemaVersion | `1` |
| ProviderType | `Offline` |
| DefaultSessionRules | `SessionRules.Default()` |
| StrictParticipantLock | `false` |
| EnforceConfigConsistency | `true` |
| MatchmakingEnabled | `false` |

---

### `ParticipantIdentity`

**Файл:** `API/ParticipantIdentity.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Незмінна ідентичність учасника. Використовується для strict world lock.

```csharp
public sealed class ParticipantIdentity : IEquatable<ParticipantIdentity>
{
    public const string BotIdPrefix = "BOT_";  // ботів ідентифікуємо за префіксом PlayerId
    public string PlayerId { get; }             // унікальний ID гравця
    public string Nickname { get; }             // нік для відображення
}
```

> **Рівність** визначається виключно за `PlayerId` (ordinal comparison).

**Приклад:**

```csharp
var human = new ParticipantIdentity("player-001", "Kruty");
var bot   = new ParticipantIdentity("BOT_alpha",  "Bot α");

bool isBot = bot.PlayerId.StartsWith(ParticipantIdentity.BotIdPrefix); // true
```

---

### `Participant`

**Файл:** `API/Participant.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Учасник сесії (людина або бот).

```csharp
public sealed class Participant
{
    public ParticipantIdentity Identity { get; }
    public bool IsBot { get; }
    public bool IsHost { get; }
}
```

Метод `AsHost()` повертає новий екземпляр з `IsHost = true`.

---

### `ParticipantSlot`

**Файл:** `API/ParticipantSlot.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

Іменований/кольоровий слот у сесії (без ігрових даних).

```csharp
public sealed class ParticipantSlot
{
    public int SlotIndex { get; }
    public string ColorName { get; }    // наприклад, "Red", "Blue"
    public string DisplayName { get; }  // відображуване ім'я
    public bool IsOccupied { get; }
}
```

---

### `WorldSnapshot`

**Файл:** `API/WorldSnapshot.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

Легкий об'єкт метаданих стану світу. Фактичні дані зберігаються в існуючій save-системі.

```csharp
public sealed class WorldSnapshot
{
    public string WorldId { get; }    // унікальний ID світу
    public int Version { get; }       // версія стану (монотонно зростає)
    public uint Checksum { get; }     // CRC32 або аналогічний хеш
}
```

Призначення:
- Порівняння станів двох клієнтів при вході у світ.
- Перевірка цілісності при відновленні сесії.
- Strict 4-player world lock (через WorldId).

---

### `SessionConnectOptions`

**Файл:** `API/SessionConnectOptions.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

DTO для `CreateOrJoinSessionAsync`.

```csharp
public sealed class SessionConnectOptions
{
    public ParticipantIdentity LocalIdentity { get; }   // ідентичність локального гравця
    public string RoomId { get; }                        // ID кімнати/сесії
    public bool CreateIfNotExists { get; }               // true = хост, false = клієнт
    public SessionRules Rules { get; }                   // правила, які хоче застосувати
    public uint ConfigChecksum { get; }                  // checksum конфігу від remote-хоста (0 = skip)
}
```

---

### `SlotMapping`

**Файл:** `API/IWorldCloneService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

Описує перемаплення слотів при клонуванні світу.

```csharp
public sealed class SlotMapping
{
    public int[] OldToNewSlotIndices { get; }
}
```

---

## Незмінність моделей

Усі моделі (`SessionRules`, `MultiplayerConfig`, `WorldSnapshot`) — `sealed` класи з `get`-only властивостями.  
Оновити конфігурацію — означає створити новий екземпляр, а не мутувати існуючий.

```csharp
// Правильно — новий екземпляр
var newConfig = new MultiplayerConfig(
    config.SchemaVersion,
    NetworkProviderType.Relay,  // змінили лише провайдера
    config.DefaultSessionRules,
    config.StrictParticipantLock,
    config.EnforceConfigConsistency,
    config.MatchmakingEnabled);

// Неправильно — компілятор заборонить
// config.ProviderType = NetworkProviderType.Relay; // помилка компіляції
```
