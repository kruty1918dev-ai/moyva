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
        ISaveInspectorService.cs ← read-only перевірка наявності конкретного save-блоку в слоті
    ISaveModule.cs       ← контракт для участі у save/load циклі (OnSave/OnLoad)
    ISaveContext.cs      ← контекст запису/читання (BinaryWriter / BinaryReader)
    SaveSlotInfo.cs      ← метадані слоту (exists, size, timestamp)
  Runtime/
    SaveService.cs          ← internal sealed, реалізує ISaveService + IInitializable + IDisposable
        SaveInspectorService.cs ← internal sealed, перевіряє чи є block конкретного модуля в слоті
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
| `WorldBuiltSignal` | немає | `MapVisualInstantiator` | bootstrap / відкладені loaders |

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

## Дерево документації SaveSystem

- [SaveSystem Designer Tool Guide](save-system-designer-tool.md)

---

## Нові підсистеми (Bootstrap + Units)

Після розширення SaveSystem з'явились додаткові підсистеми, які працюють разом із `SaveService`:

### 1) UnitsSaveModule

Призначення:
- серіалізація списку юнітів у save-блок
- відновлення юнітів при `Load`
- підтримка двох форматів юніт-блоку: старий (без stamina) і новий (зі stamina)

Що зберігається на 1 юніт:
- `typeId` (наприклад, `warrior`)
- `position` (`x`, `y`)
- `stamina` (для нового формату)

Особливість сумісності:
- loader спочатку пробує новий формат
- якщо структура не сходиться, автоматично робить fallback на legacy-формат
- це запобігає сценаріям `EndOfStreamException` і "космічної" stamina при читанні старого файлу новим кодом

### 2) GameExitSaver

Призначення:
- автоматично викликати `Save(0)` при `Application.quitting`

Перевага:
- дизайнеру не потрібно вручну натискати save під час швидких перевірок сцени

### 3) TestUnitSpawner + завантаження зі слота

Поточна логіка bootstrap:
- bootstrap спочатку перевіряє, чи є у слоті block генератора карти
- якщо block генератора є: виконується `Load(0)`
- якщо save є, але block генератора відсутній: це вважається новою грою
- якщо сейву немає: теж нова гра

Важлива деталь по порядку:
- `UnitsSaveModule` більше не спавнить юнітів одразу під час раннього `Load`
- юніти буферизуються і відновлюються тільки після `WorldBuiltSignal`
- це захищає статичні об'єкти карти від пропуску через ранні колізії в `ObjectsMap`

Це дає "resume" поведінку в editor-потоку тестування.

### 4) GeneratedWorldSaveModule

Призначення:
- збереження повного результату генерації карти в один save-блок
- відновлення цієї карти без повторної процедурної генерації

Що зберігається:
1. `width`
2. `height`
3. `biomeMap[x,y]`
4. `objectMap[x,y]`
5. `heightMap[x,y]`

Що це означає для дизайнера:
- у слоті зберігається не лише юніт або fog, а вся конкретна розкладка світу
- той самий `objectId` на тих самих координатах відновлюється 1:1
- якщо block генератора відсутній, SaveSystem не намагається вважати слот повноцінним continuation-save

---

## SaveSystem Designer Tool (Editor)

Додано окреме editor-вікно для дизайнерів:
- меню: `Moyva/Save System/Designer Tool`
- функції: читання файлів, перегляд блоків, редагування payload, видалення блоків, видалення файлів/слотів, робота з backup, відкриття директорії файлу, показ розміру файлу, розумний перегляд блока генератора

Детальний посібник (по кожному полю та кнопці):
- [Save System Designer Tool Guide](save-system-designer-tool.md)

---

## Інтеграція Fog of War

Тумн війни тепер інтегрований у SaveSystem через окремий `ISaveModule`:

- `FogOfWarSaveModule` (runtime, FogOfWar)
- зберігає `bool[,] exploredTiles` у власний save-блок
- завантажує explored state назад у `FogOfWarService`

Що саме записується в payload:
1. `width` (int)
2. `height` (int)
3. `width * height` булевих значень `explored[x,y]`

Підключення:
- у `FogOfWarInstaller` доданий binding:
    `Container.Bind<ISaveModule>().To<FogOfWarSaveModule>().AsSingle();`

Важливий edge case:
- якщо `Load` викликано до `FogOfWarService.Initialize(width,height)`,
    snapshot зберігається у pending-буфері та застосовується після ініціалізації карти.
    Це захищає від втрати даних при ранньому завантаженні в bootstrap-потоці.
