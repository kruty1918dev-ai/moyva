# SaveSystem — Система збереження

← [Назад до README](../README.md)

---

## Призначення

**SaveSystem** — незалежний модуль для збереження та завантаження стану гри у бінарний файл формату `.mvs`.  
Модуль спілкується з рештою систем виключно через **SignalBus** і контракт `ISaveModule`.  
Він нічого не знає про конкретні ігрові системи — лише про байти, файли, CRC і виключення.

---

## Залежності

| Залежність | Причина |
|---|---|
| `Zenject` | DI-контейнер, `IInitializable`, `IDisposable`, `SignalBus` |
| `Kruty1918.Moyva.Signals` | Сигнали `SaveRequestedSignal`, `LoadRequestedSignal`, `SaveCompletedSignal` |

Інших залежностей немає.

---

## Архітектура

```
Assets/Moyva/Scripts/Features/SaveSystem/
  Kruty1918.Moyva.SaveSystem.asmdef          ← збірка (refs: Zenject, Kruty1918.Moyva.Signals)
  API/
    ISaveService.cs      ← публічний контракт системи (Save/Load/HasSave/Delete/GetSlotInfo)
    ISaveModule.cs       ← контракт для участі у save/load циклі (OnSave/OnLoad)
    ISaveContext.cs      ← контекст запису/читання (BinaryWriter / BinaryReader)
    SaveSlotInfo.cs      ← метадані слоту (exists, size, timestamp)
  Runtime/
    SaveService.cs          ← internal sealed, реалізує ISaveService + IInitializable + IDisposable
    SaveContext.cs          ← internal sealed, реалізує ISaveContext
    SaveFileCodec.cs        ← internal static, кодування/декодування .mvs формату
    Crc32.cs                ← internal static, CRC-32 (IEEE 802.3 polynomial)
    SaveSystemInstaller.cs  ← public sealed MonoInstaller
```

---

## Бінарний формат `.mvs`

```
┌─────────────────────────────────────────────────────┐
│  HEADER                                             │
│    magic      : "MVSA"  (4 bytes)                   │
│    version    : ushort  (2 bytes) = 1               │
│    blockCount : uint    (4 bytes)                   │
├─────────────────────────────────────────────────────┤
│  BLOCK × N  (per ISaveModule)                       │
│    blockId  : uint   (4 bytes)  FNV-1a хеш типу    │
│    blockSize: uint   (4 bytes)                      │
│    blockCrc : uint   (4 bytes)  CRC32 payload       │
│    payload  : byte[] (blockSize bytes)              │
├─────────────────────────────────────────────────────┤
│  FOOTER                                             │
│    globalCrc: uint (4 bytes)  CRC32 усіх попередніх│
└─────────────────────────────────────────────────────┘
```

### BlockId

Детермінований FNV-1a 32-bit хеш `Type.FullName` модуля.  
Якщо модуль перейменований — blockId змінюється, і при завантаженні блок буде **пропущений** (forward compatibility).

---

## Сигнали

| Сигнал | Поля | Хто надсилає | Хто слухає |
|---|---|---|---|
| `SaveRequestedSignal` | `int Slot` | Будь-який компонент (UI, hotkey) | `SaveService` |
| `LoadRequestedSignal` | `int Slot` | Будь-який компонент | `SaveService` |
| `SaveCompletedSignal` | `int Slot`, `bool Success`, `string ErrorMessage` | `SaveService` | UI, аналітика |

---

## Рівні валідації

### Рівень 1 — Вхідні перевірки
- Слот у межах `[0, 99]`
- Директорія збережень доступна для запису

### Рівень 2 — Per-block sandbox (запис)
- Модуль не `null`
- `OnSave()` не кинув виняток (ізольований `MemoryStream`)
- Payload > 0 байт
- Payload ≤ 10 MB

### Рівень 3 — Верифікація зібраного буфера
- Розмір ≥ `MinFileSize` (14 bytes)
- Magic bytes коректні

### Рівень 4 — Атомарний запис
- Записати у `.tmp`
- Верифікувати `.tmp` (розмір + magic)
- Існуючий файл → `.bak`
- `.tmp` → final (з відновленням з `.bak` при помилці)

### Рівень 5 — Валідація при читанні
- Файл існує і читабельний
- Розмір ≥ `MinFileSize`
- Magic bytes
- Версія сумісна (`MinVersion ≤ v ≤ CurrentVersion`)
- Глобальна CRC32
- Per-block CRC32 (блоки з невалідним CRC пропускаються)
- Невідомий blockId → LogWarning + skip (forward compatibility)
- `OnLoad()` не кинув виняток (ізольований `MemoryStream`)

### Рівень 6 — Fallback
- При критичній помилці → спроба завантажити `.bak`

---

## Як інтегрувати свою систему

### Крок 1 — Реалізуйте `ISaveModule`

```csharp
// У вашому модулі (наприклад, FogOfWar)
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogSaveModule : ISaveModule
    {
        private readonly IFogOfWarService _fog;

        public FogSaveModule(IFogOfWarService fog) { _fog = fog; }

        public void OnSave(ISaveContext ctx)
        {
            bool[,] explored = _fog.GetExploredSnapshot();
            ctx.Writer.Write(explored.GetLength(0));
            ctx.Writer.Write(explored.GetLength(1));
            foreach (bool b in explored)
                ctx.Writer.Write(b);
        }

        public void OnLoad(ISaveContext ctx)
        {
            int w = ctx.Reader.ReadInt32();
            int h = ctx.Reader.ReadInt32();
            var data = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    data[x, y] = ctx.Reader.ReadBoolean();
            _fog.LoadFromSnapshot(data);
        }
    }
}
```

### Крок 2 — Зареєструйте у вашому Installer

```csharp
// У FogOfWarInstaller.cs (або окремому installer)
Container.Bind<ISaveModule>().To<FogSaveModule>().AsSingle();
```

Zenject автоматично зібере всі `ISaveModule` у `List<ISaveModule>` для `SaveService`.

### Крок 3 — Додайте `SaveSystemInstaller` до SceneContext

У Unity Inspector → Scene Context → Mono Installers додайте `SaveSystemInstaller` після `SignalBusInstaller`.

### Крок 4 — Виклик збереження

```csharp
// Через SignalBus (рекомендовано):
_signalBus.Fire(new SaveRequestedSignal { Slot = 0 });

// Або напряму через ISaveService (якщо інжектовано):
_saveService.Save(0);
```

---

## Файли збережень

Зберігаються у `Application.persistentDataPath/saves/`:

| Файл | Призначення |
|---|---|
| `slot00.mvs` | Основний файл збереження (слот 0) |
| `slot00.mvs.bak` | Резервна копія попереднього збереження |
| `slot00.mvs.tmp` | Тимчасовий файл під час атомарного запису |

---

## Installer і ExecutionOrder

```csharp
// SaveSystemInstaller.cs
public sealed class SaveSystemInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<SaveService>()
            .AsSingle()
            .NonLazy();

        Container.BindExecutionOrder<SaveService>(-8); // ← до інших сервісів (за замовчуванням 0)
    }
}
```

---

---

## ConfigService — глобальний конфіг

**ConfigService** — другий сервіс SaveSystem. Зберігає один файл `config.mvs` (не слоти) і призначений для **налаштувань, локалізації, модів** та іншого глобального конфігу.

### Відмінності від SaveService

| | `SaveService` | `ConfigService` |
|---|---|---|
| Файлів | до 100 (slot00–slot99.mvs) | 1 (`config.mvs`) |
| Сигнали | `SaveRequestedSignal`, `LoadRequestedSignal` | немає — виклик явний |
| API | `Save(int slot)`, `Load(int slot)` | `SaveConfig(modules)`, `LoadConfig(modules)` |
| Призначення | ігровий прогрес, стан сесії | налаштування, локалізація, mod-конфіг |

### `IConfigService`

```csharp
public interface IConfigService : IInitializable, IDisposable
{
    /// <summary>Зберегти конфіг з модулями. Виконується атомарно.</summary>
    void SaveConfig(List<ISaveModule> modules);

    /// <summary>Завантажити конфіг у модулі.</summary>
    void LoadConfig(List<ISaveModule> modules);

    /// <summary>Перевірити, чи існує config.mvs.</summary>
    bool HasConfig();

    /// <summary>Видалити config.mvs.</summary>
    void DeleteConfig();

    /// <summary>Отримати інформацію про config файл (розмір, дата).</summary>
    SaveSlotInfo GetConfigInfo();
}
```

### Файл конфігу

Зберігається у `Application.persistentDataPath/saves/config.mvs`.

| Файл | Призначення |
|---|---|
| `config.mvs` | Основний файл конфігу |
| `config.mvs.bak` | Резервна копія |
| `config.mvs.tmp` | Тимчасовий файл під час атомарного запису |

Формат ідентичний слотам: `MVSA` magic, version, blocks, global CRC32.

### Як використовувати

```csharp
// Збереження налаштувань
[Inject] private IConfigService _configService;

var modules = new List<ISaveModule> { audioModule, graphicsModule };
_configService.SaveConfig(modules);

// Завантаження
_configService.LoadConfig(modules);
```

---

## Пов'язані документи

- [Signals](../signals.md) — усі сигнали проекту
- [Порядок ініціалізації](../initialization-order.md) — місце SaveSystem у порядку запуску
- [FogOfWar → Save stub](../fog-of-war/save-system-stub.md) — поточний заглушковий стан FogOfWar
- [TDD Standard](../../standarts/TDD.md) — архітектурні правила модульності
