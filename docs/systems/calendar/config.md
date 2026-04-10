# Calendar — CalendarConfig та CalendarBinaryConfigStore

← [Назад до огляду](README.md)

---

## CalendarConfig

`CalendarConfig` — незмінний об'єкт конфігурації, що описує структуру ігрового календаря.

### Параметри

| Параметр | Тип | Опис |
|---|---|---|
| `SchemaVersion` | `int` | Версія бінарного формату (для forward compatibility) |
| `StartYear` | `int` | Рік початку гри (за замовчуванням: 1) |
| `StartMonth` | `int` | Місяць початку гри (1..MonthsInYear) |
| `StartDay` | `int` | День початку гри (1..DaysInMonth) |
| `StartHour` | `int` | Година початку гри (0..HoursInDay-1) |
| `MonthsInYear` | `int` | Кількість місяців у році (12 за замовчуванням) |
| `DaysInMonth` | `int` | Кількість днів у кожному місяці (фіксовано, 30 за замовчуванням) |
| `HoursInDay` | `int` | Кількість годин у добі (24 за замовчуванням) |
| `DayStartHour` | `int` | Година початку дня (6 за замовчуванням) |
| `NightStartHour` | `int` | Година початку ночі (20 за замовчуванням) |
| `DawnDurationHours` | `int` | Тривалість світанку в годинах (1 за замовчуванням) |
| `DuskDurationHours` | `int` | Тривалість сутінків в годинах (1 за замовчуванням) |
| `HoursPerTurn` | `int` | Скільки ігрових годин проходить за один хід (1 за замовчуванням) |

### Формула TotalHoursSinceEpoch → GameDateTime

```
absHours  = totalHours + StartHour
hour      = absHours % HoursInDay
totalDays = absHours / HoursInDay
day       = (totalDays % DaysInMonth) + StartDay     (simplified)
...
```

Повна реалізація: `GameCalendarService.ComputeDateTime(config, totalHours)`.

### Формула DayPhase

```
dawnStart = DayStartHour - DawnDurationHours
duskStart = NightStartHour - DuskDurationHours

Dawn   : hour ∈ [dawnStart, DayStartHour)
Day    : hour ∈ [DayStartHour, duskStart)
Dusk   : hour ∈ [duskStart, NightStartHour)
Night  : все інше
```

---

## CalendarBinaryConfigStore

**Файл:** `Runtime/CalendarBinaryConfigStore.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Runtime`  
**Реалізує:** `ICalendarConfigStore`

Зберігає та завантажує `CalendarConfig` у бінарному файлі — аналог `BinaryConfigStore` з Multiplayer.

### Шлях до файлу

За замовчуванням:

```
Application.persistentDataPath/calendar_config.dat
```

Для Editor (через `CalendarConfigEditorWindow`):

```
Assets/Moyva/calendar_config.dat
```

### Формат файлу (бінарний, в порядку запису)

| Порядок | Поле | Тип |
|---|---|---|
| 1 | SchemaVersion | int32 |
| 2 | StartYear | int32 |
| 3 | StartMonth | int32 |
| 4 | StartDay | int32 |
| 5 | StartHour | int32 |
| 6 | MonthsInYear | int32 |
| 7 | DaysInMonth | int32 |
| 8 | HoursInDay | int32 |
| 9 | DayStartHour | int32 |
| 10 | NightStartHour | int32 |
| 11 | DawnDurationHours | int32 |
| 12 | DuskDurationHours | int32 |
| 13 | HoursPerTurn | int32 |

### Поведінка при помилках

- Якщо файл не знайдено → `CalendarConfig.Default()` (попередження не виводиться).
- Якщо файл пошкоджений → `CalendarConfig.Default()` + `Debug.LogWarning(...)`.
- Якщо директорія не існує → `Directory.CreateDirectory()` під час запису.

### Використання у коді

```csharp
// Завантаження
ICalendarConfigStore store = new CalendarBinaryConfigStore(); // Application.persistentDataPath
CalendarConfig config = store.Load();

// Завантаження з конкретного шляху (Editor / тести)
ICalendarConfigStore store = new CalendarBinaryConfigStore("Assets/Moyva/calendar_config.dat");
CalendarConfig config = store.Load();

// Збереження
store.Save(new CalendarConfig(CalendarConfig.CurrentSchemaVersion, ...));
```

---

## Zenject / DI прив'язка (приклад)

```csharp
// У CalendarInstaller.cs (MonoInstaller або ScriptableObject Installer):
Container.Bind<ICalendarConfigStore>()
    .To<CalendarBinaryConfigStore>()
    .AsSingle()
    .WithArguments("Assets/Moyva/calendar_config.dat");

Container.Bind<ICalendarService>()
    .To<GameCalendarService>()
    .AsSingle();

Container.Bind<ICalendarSyncAdapter>()
    .To<CalendarSyncAdapter>()
    .AsSingle();
```
