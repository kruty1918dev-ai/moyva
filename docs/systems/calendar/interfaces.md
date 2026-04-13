# Calendar — Довідник інтерфейсів і типів

← [Назад до огляду](README.md)

---

## `DayPhase` (enum)

**Файл:** `API/DayPhase.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Domain`

```csharp
public enum DayPhase
{
    Night,   // ніч
    Dawn,    // світанок (перехід ніч→день)
    Day,     // день
    Dusk     // сутінки (перехід день→ніч)
}
```

Порядок фаз протягом доби (за дефолтним конфігом, `HoursInDay = 24`):

```
0h ──── 5h (Night)
5h ──── 6h (Dawn)
6h ──── 19h (Day)
19h ──── 20h (Dusk)
20h ──── 24h/0h (Night)
```

---

## `GameDateTime` (struct)

**Файл:** `API/GameDateTime.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Domain`

Незмінна мітка ігрового часу. Гранулярність — одна година.

```csharp
public readonly struct GameDateTime : IEquatable<GameDateTime>
{
    public int Year  { get; }
    public int Month { get; }
    public int Day   { get; }
    public int Hour  { get; }

    public GameDateTime(int year, int month, int day, int hour);

    public bool Equals(GameDateTime other);
    public override string ToString();   // "Year 1, Month 01, Day 01, 06:00"
    // == and != operators
}
```

**Важливо:** `GameDateTime` — value object. Порівнюйте через `==` або `.Equals()`.

---

## `CalendarConfig` (sealed class)

**Файл:** `API/CalendarConfig.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Config`

```csharp
public sealed class CalendarConfig
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion     { get; }   // версія бінарного формату

    // Стартова дата/час
    public int StartYear         { get; }
    public int StartMonth        { get; }
    public int StartDay          { get; }
    public int StartHour         { get; }

    // Структура календаря
    public int MonthsInYear      { get; }   // кількість місяців у році
    public int DaysInMonth       { get; }   // днів у кожному місяці (фіксоване)
    public int HoursInDay        { get; }   // годин у добі (зазвичай 24)

    // Межі дня/ночі
    public int DayStartHour      { get; }   // година початку дня
    public int NightStartHour    { get; }   // година початку ночі
    public int DawnDurationHours { get; }   // тривалість світанку (год.)
    public int DuskDurationHours { get; }   // тривалість сутінків (год.)

    // Мультиплеєр
    public int HoursPerTurn      { get; }   // ігрових годин за один хід (default: 1)

    public CalendarConfig(int schemaVersion, int startYear, ... int hoursPerTurn);

    public static CalendarConfig Default();   // стандартний конфіг
}
```

**Дефолтні значення:**

| Поле | Значення |
|---|---|
| MonthsInYear | 12 |
| DaysInMonth | 30 |
| HoursInDay | 24 |
| DayStartHour | 6 |
| NightStartHour | 20 |
| DawnDurationHours | 1 |
| DuskDurationHours | 1 |
| HoursPerTurn | 1 |
| StartYear / Month / Day / Hour | 1 / 1 / 1 / 6 |

---

## `ICalendarConfigStore`

**Файл:** `API/ICalendarConfigStore.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Config`

```csharp
public interface ICalendarConfigStore
{
    CalendarConfig Load();
    void Save(CalendarConfig config);
    bool Exists();
}
```

Конкретна реалізація: [`CalendarBinaryConfigStore`](config.md).

---

## `ICalendarService`

**Файл:** `API/ICalendarService.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Core`

Основний контракт сервісу.

```csharp
public interface ICalendarService
{
    // --- Стан ---
    GameDateTime   Current              { get; }   // поточний ігровий час
    long           TotalHoursSinceEpoch { get; }   // годин від початку епохи
    DayPhase       CurrentDayPhase      { get; }   // поточна фаза доби
    CalendarConfig Config               { get; }   // конфіг, з яким ініціалізовано

    // --- Події ---
    event Action            OnHourChanged;
    event Action            OnDayChanged;
    event Action            OnMonthChanged;
    event Action            OnYearChanged;
    event Action<DayPhase>  OnDayPhaseChanged;

    // --- Зміни стану ---
    void AdvanceTurn();                           // +HoursPerTurn (тільки сервер)
    void SetByTotalHours(long totalHours);        // синхронізація зі snapshots
}
```

**Реалізації:**

| Клас | Роль |
|---|---|
| `GameCalendarService` | Авторитативна (сервер) — дозволяє `AdvanceTurn` |
| `ClientCalendarProxy` | Клієнтський проксі — `AdvanceTurn` кидає `InvalidOperationException` |

---

## `ICalendarSyncAdapter`

**Файл:** `API/ICalendarSyncAdapter.cs`  
**Namespace:** `Kruty1918.Moyva.Calendar.Multiplayer`

```csharp
public interface ICalendarSyncAdapter
{
    // Хост / сервер викликає після кожного завершеного ходу
    void NotifyTurnCompleted();

    // Клієнт викликає після отримання snapshot від сервера
    void ApplyRemoteSnapshot(long totalHoursSinceEpoch);
}
```

Конкретна реалізація: `CalendarSyncAdapter`.

---

## Таблиця реалізацій

| Інтерфейс | Конкретна реалізація | Статус |
|---|---|---|
| `ICalendarService` | `GameCalendarService` | ✅ Реалізовано |
| `ICalendarService` | `ClientCalendarProxy` | ✅ Реалізовано |
| `ICalendarConfigStore` | `CalendarBinaryConfigStore` | ✅ Реалізовано |
| `ICalendarSyncAdapter` | `CalendarSyncAdapter` | ✅ Реалізовано |
