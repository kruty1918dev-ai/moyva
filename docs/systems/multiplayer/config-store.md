# Multiplayer — MultiplayerConfig та BinaryConfigStore

← [Назад до огляду](README.md)

---

## Призначення

`MultiplayerConfig` — єдине авторитарне джерело налаштувань мультиплеєра.

`BinaryConfigStore` — завантажує і зберігає `MultiplayerConfig` у бінарний файл.  
Не залежить від `UnityEditor` — використовується як у runtime, так і в Editor-інструменті.

---

## MultiplayerConfig

**Файл:** `API/MultiplayerConfig.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Config`

### Властивості

| Властивість | Тип | Опис |
|---|---|---|
| `SchemaVersion` | `int` | Версія схеми файлу (для backward-compatibility) |
| `ProviderType` | `NetworkProviderType` | Транспорт: Relay / Mirror / Offline |
| `DefaultSessionRules` | `SessionRules` | Правила сесії за замовчуванням |
| `StrictParticipantLock` | `bool` | Тільки ті самі 4 гравці можуть продовжити world |
| `EnforceConfigConsistency` | `bool` | Перевіряти checksum конфігу при вході |
| `MatchmakingEnabled` | `bool` | Дозволений matchmaking |

### Константи

```csharp
public const int CurrentSchemaVersion = 1;
```

### Незмінність

`MultiplayerConfig` — `sealed` клас з `get`-only властивостями.  
Для зміни налаштувань потрібно створити новий екземпляр.

### Дефолт

```csharp
var config = MultiplayerConfig.Default();
// SchemaVersion = 1
// ProviderType = Offline
// DefaultSessionRules = SessionRules.Default()
// StrictParticipantLock = false
// EnforceConfigConsistency = true
// MatchmakingEnabled = false
```

---

## BinaryConfigStore

**Файл:** `Runtime/BinaryConfigStore.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Runtime`

Реалізує `IConfigStore`.

### Конструктор

```csharp
public BinaryConfigStore(string filePath = null)
```

Якщо `filePath = null`, використовується шлях за замовчуванням:  
`Application.persistentDataPath + "/multiplayer_config.dat"`

### Методи

#### `Load()`

```csharp
public MultiplayerConfig Load()
```

- Якщо файл **не існує** → повертає `MultiplayerConfig.Default()`.
- Якщо виникає помилка читання → логує Warning, повертає `Default()`.
- Ніколи не кидає виключень.

#### `Save(config)`

```csharp
public void Save(MultiplayerConfig config)
```

- Створює директорію якщо не існує.
- Записує конфіг у бінарному форматі.
- При помилці логує Error, але не кидає.

#### `Exists()`

```csharp
public bool Exists()
```

Повертає `true` якщо файл існує.

---

## Бінарний формат

Дані записуються через `BinaryWriter` у такому порядку:

```
Offset  Type    Field
─────── ─────── ─────────────────────────────────────────
0       int32   SchemaVersion
4       int32   ProviderType (cast to int)
8       bool    StrictParticipantLock
9       bool    EnforceConfigConsistency
10      bool    MatchmakingEnabled
11      int32   DefaultSessionRules.Mode (cast to int)
15      int32   DefaultSessionRules.MaxParticipants
19      int32   DefaultSessionRules.MaxHumans
23      int32   DefaultSessionRules.MaxBots
27      bool    DefaultSessionRules.AllowBotsFallbackOnLeave
28      bool    DefaultSessionRules.AllowMatchSaveForAnalysis
29      bool    DefaultSessionRules.StrictParticipantLock
─────────────────────────────────────────────────────────
Total: 30 bytes
```

### Сумісність

- Читання завжди очікує рівно цю послідовність полів.
- При зміні `SchemaVersion` потрібна міграція (зчитати `SchemaVersion`, потім застосувати правильну логіку).

---

## Використання у коді

### Runtime (гра)

```csharp
// Завантажити конфіг (зазвичай через DI)
var configStore = new BinaryConfigStore();
var config = configStore.Load();

Debug.Log($"Provider: {config.ProviderType}");
Debug.Log($"Max players: {config.DefaultSessionRules.MaxParticipants}");
```

### Editor (Config Hub)

```csharp
// Редактор знає конкретний шлях
var store = new BinaryConfigStore("Assets/Moyva/multiplayer_config.dat");

// Завантажити
var config = store.Load();

// Зберегти змінений конфіг
var newConfig = new MultiplayerConfig(
    MultiplayerConfig.CurrentSchemaVersion,
    NetworkProviderType.Relay,
    new SessionRules(SessionMode.MultiplayerHumans, 4, 4, 0, false, false, false),
    strictParticipantLock: false,
    enforceConfigConsistency: true,
    matchmakingEnabled: true);

store.Save(newConfig);
```

---

## Майбутні можливості

- Підтримка **remote config** (завантаження з сервера при наявності мережі, fallback на локальний файл).
- **Versioning** — при виявленні старого `SchemaVersion` застосовувати міграцію полів.
- **Shared config** — хост транслює свій конфіг клієнтам через `IConfigSyncService`.

---

## IConfigSyncService (carcass)

**Файл:** `API/IConfigSyncService.cs`

Дозволяє оновити локальний конфіг з хоста:

```csharp
public interface IConfigSyncService
{
    void SyncFromHost(MultiplayerConfig hostConfig);
}
```

Конкретна реалізація буде інтегрована, коли Relay/Mirror провайдери стануть активними.
