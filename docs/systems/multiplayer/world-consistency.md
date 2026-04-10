# Multiplayer — WorldConsistencyService та World Snapshots

← [Назад до огляду](README.md)

---

## Призначення

Коли два клієнти підключаються до одного і того ж збереженого світу, хост повинен перевірити:  
**чи мають обидва клієнти однакову копію цього світу?**

`WorldConsistencyService` порівнює `WorldSnapshot` клієнта з `WorldSnapshot` хоста і повертає результат.

---

## WorldSnapshot — що це

**Файл:** `API/WorldSnapshot.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

```csharp
public sealed class WorldSnapshot
{
    public string WorldId { get; }   // унікальний ID світу
    public int Version { get; }      // версія збереженого стану
    public uint Checksum { get; }    // CRC32 або аналогічний хеш даних
}
```

`WorldSnapshot` — це **легкий** об'єкт. Він не містить фактичних ігрових даних.  
Фактичний стан зберігається в існуючій save-системі (`ISaveService`).

### Роль Checksum

`Checksum` — це унікальний відбиток стану світу.  
Якщо хоча б один байт у збереженому світі відрізняється, checksum буде іншим.

---

## WorldConsistencyService

**Файл:** `Runtime/WorldConsistencyService.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

### Метод `Compare`

```csharp
public ConsistencyCheckResult Compare(WorldSnapshot host, WorldSnapshot client)
```

### Алгоритм

```
1. Якщо host == null або client == null → WorldMismatch (з попередженням у лог)

2. Якщо host.WorldId ≠ client.WorldId → WorldMismatch
   (Клієнт намагається зайти у інший світ, ніж той що у хоста)

3. Якщо host.Checksum ≠ client.Checksum → WorldMismatch
   (Дані світу відрізняються — потрібна міграція або відмова)

4. Повернути Equal
```

### Повернені значення

| Результат | Опис |
|---|---|
| `Equal` | Stани ідентичні — можна продовжувати гру |
| `WorldMismatch` | WorldId або Checksum відрізняється |
| `ConfigMismatch` | Зарезервовано — для несумісного конфігу (перевіряється в SessionManager) |

---

## Сценарії порівняння

### Успішне підключення

```
Host snapshot:   WorldId="world-001", Version=5, Checksum=0xDEADBEEF
Client snapshot: WorldId="world-001", Version=5, Checksum=0xDEADBEEF

Result: Equal → підключення дозволено
```

### Різні checksum (хтось грав офлайн і змінив стан)

```
Host snapshot:   WorldId="world-001", Version=5, Checksum=0x11111111
Client snapshot: WorldId="world-001", Version=5, Checksum=0x22222222

Result: WorldMismatch → потрібна міграція або синхронізація
```

### Різні world ID (гравець намагається зайти у невірний світ)

```
Host snapshot:   WorldId="world-A", Version=3, Checksum=0xABCD1234
Client snapshot: WorldId="world-B", Version=3, Checksum=0xABCD1234

Result: WorldMismatch → відмова у підключенні
```

---

## IWorldSnapshotStore

**Файл:** `API/IWorldSnapshotStore.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Persistence`

Міст між мультиплеєром і існуючою save-системою.

```csharp
public interface IWorldSnapshotStore
{
    bool Exists(string worldId);
    WorldSnapshot Load(string worldId);
    void Save(WorldSnapshot snapshot);
}
```

### Як реалізувати

Wrap існуючого `ISaveService`:

```csharp
public sealed class SaveSystemSnapshotStore : IWorldSnapshotStore
{
    private readonly ISaveService _saveService;

    public SaveSystemSnapshotStore(ISaveService saveService)
        => _saveService = saveService;

    public bool Exists(string worldId)
        => _saveService.HasSave(WorldIdToSlot(worldId));

    public WorldSnapshot Load(string worldId)
    {
        int slot = WorldIdToSlot(worldId);
        // Завантажити метадані (WorldId, Version, Checksum) з бінарного блоку
        // ...
    }

    public void Save(WorldSnapshot snapshot)
    {
        // Зберегти метадані у відповідний блок через ISaveModule
        // ...
    }

    private int WorldIdToSlot(string worldId) { /* логіка маппінгу */ }
}
```

> **Примітка:** Конкретна реалізація залежить від структури блоків у save-файлах `.mvs`.  
> Для carcass використовується `FakeSnapshotStore` (in-memory) у тестах.

---

## IWorldCloneService (carcass)

**Файл:** `API/IWorldCloneService.cs`

Клонує існуючий світ з новими правилами.

```csharp
public interface IWorldCloneService
{
    // Повертає worldId нового клону
    string CloneWorld(string sourceWorldId, SessionRules newRules, SlotMapping mapping);
}
```

**Зв'язок із strict world lock:**  
Якщо у World-001 є strict lock на 4 гравців, адміністратор може зробити клон World-001-copy з relaxed rules, і тоді інші гравці зможуть грати у цьому клоні.

---

## Міграція при WorldMismatch

Якщо `WorldConsistencyService.Compare` повертає `WorldMismatch`, існують стратегії:

| Стратегія | Опис |
|---|---|
| **Host wins** | Клієнт синхронізується зі станом хоста (найпростіше) |
| **Latest valid** | Якщо у клієнта новіша версія (`Version > host.Version`), його стан може бути прийнятий |
| **Merge** | Об'єднати певні частини стану (складно, потребує ігрової логіки) |
| **Conflict fallback** | Якщо стани критично різні — створити нову гілку (`CloneWorld`) |

> У поточному carcass при `WorldMismatch` — `SessionManager` повертає `false` і логує помилку.  
> Конкретна стратегія реалізовується пізніше через `IFailureHandlingPolicy`.

---

## Покроковий приклад (повний флоу)

```
1. Клієнт А і Клієнт Б намагаються підключитись до World-007

2. Кожен надсилає хосту свій WorldSnapshot:
   ClientA.snapshot = { WorldId="world-007", Version=8, Checksum=0xABC123 }
   ClientB.snapshot = { WorldId="world-007", Version=8, Checksum=0xABC123 }

3. Хост завантажує свій snapshot:
   Host.snapshot   = { WorldId="world-007", Version=8, Checksum=0xABC123 }

4. WorldConsistencyService.Compare(host, clientA) → Equal ✅
   WorldConsistencyService.Compare(host, clientB) → Equal ✅

5. Обидва клієнти допущені до гри.

─────────────────────────────────────────────────────────────

1. Клієнт В має іншу версію (грав офлайн):
   ClientC.snapshot = { WorldId="world-007", Version=8, Checksum=0xDEFABC }

2. WorldConsistencyService.Compare(host, clientC) → WorldMismatch ❌

3. IFailureHandlingPolicy.HandleRecoverable(WorldMismatch, "...") → false

4. Клієнт В отримує відмову.
```
