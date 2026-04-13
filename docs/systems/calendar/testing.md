# Calendar — Тестування

← [Назад до огляду](README.md)

---

## Розташування тестів

```
Assets/Moyva/Scripts/Tests/Calendar/
  Kruty1918.Moyva.Tests.Calendar.asmdef
  GameCalendarServiceTests.cs
  CalendarConfigTests.cs
```

---

## Запуск тестів

Unity: **Window → General → Test Runner → EditMode → Run All**

Або через CLI:
```bash
Unity -runTests -projectPath . -testResults results.xml -testPlatform EditMode
```

---

## GameCalendarServiceTests

Покриває `GameCalendarService` та статичні методи `ComputeDateTime` / `ComputeDayPhase`.

### Тести дати

| Тест | Що перевіряє |
|---|---|
| `ComputeDateTime_AtEpoch_ReturnsStartDate` | TotalHours=0 повертає StartDate з конфігу |
| `ComputeDateTime_After24Hours_AdvancesOneDay` | +24 год → +1 день |
| `ComputeDateTime_AfterFullMonth_AdvancesMonth` | +720 год → +1 місяць |
| `ComputeDateTime_AfterFullYear_AdvancesYear` | +8640 год → +1 рік |

### Тести фаз дня

| Тест | Що перевіряє |
|---|---|
| `ComputeDayPhase_AtMidnight_IsNight` | Година 0 → Night |
| `ComputeDayPhase_AtDawnStart_IsDawn` | Година 5 → Dawn |
| `ComputeDayPhase_AtDayStart_IsDay` | Година 6 → Day |
| `ComputeDayPhase_AtDusk_IsDusk` | Година 19 → Dusk |
| `ComputeDayPhase_AtNightStart_IsNight` | Година 20 → Night |

### Тести сервісу

| Тест | Що перевіряє |
|---|---|
| `AdvanceTurn_IncrementsTotalHours_ByHoursPerTurn` | AdvanceTurn → TotalHours += HoursPerTurn |
| `AdvanceTurn_FiresOnHourChanged` | Подія OnHourChanged спрацьовує |
| `AdvanceTurn_FiresOnDayChanged_AfterFullDay` | Після 24 ходів OnDayChanged спрацьовує |
| `SetByTotalHours_UpdatesCurrentDateTime` | SetByTotalHours(8640) → рік +1 |
| `SetByTotalHours_FiresPhaseChangedEvent_WhenPhaseChanges` | Подія OnDayPhaseChanged спрацьовує |

---

## CalendarConfigTests

Покриває `CalendarConfig` та `CalendarBinaryConfigStore`.

| Тест | Що перевіряє |
|---|---|
| `Default_ReturnsNonNullConfig` | CalendarConfig.Default() ≠ null |
| `Default_HasExpectedHoursInDay` | Default().HoursInDay == 24 |
| `Default_HoursPerTurn_IsOne` | Default().HoursPerTurn == 1 |
| `BinaryRoundtrip_PreservesAllFields` | WriteConfig + ReadConfig = ідентичний об'єкт |
| `Store_LoadsDefault_WhenFileDoesNotExist` | Несуттєвий файл → Default повертається |
| `Store_SaveAndLoad_RoundTrips` | Save + Load = ідентичний конфіг |

---

## Стратегія тестування

### Unit-тести (чистий C#)

- `ComputeDateTime` і `ComputeDayPhase` — `internal static` методи, видимі тестам через `InternalsVisibleTo`.
- `CalendarBinaryConfigStore.WriteConfig` / `ReadConfig` — `internal static`, тестуються в пам'яті через `MemoryStream`.
- Немає залежності від Unity engine (немає `MonoBehaviour`, `Application.persistentDataPath` передається через конструктор).

### Інтеграційні тести (мультиплеєр)

Для тестування `CalendarSyncAdapter` + `ClientCalendarProxy` використовуйте той самий підхід, що й у `SessionManagerTests.cs`:

```csharp
[Test]
public void SyncAdapter_ApplySnapshot_UpdatesProxy()
{
    var config  = CalendarConfig.Default();
    var service = new GameCalendarService(config);
    var proxy   = new ClientCalendarProxy(config);
    var adapter = new CalendarSyncAdapter(service, proxy);

    adapter.NotifyTurnCompleted();              // сервер просуває час
    long serverHours = service.TotalHoursSinceEpoch;

    adapter.ApplyRemoteSnapshot(serverHours);   // клієнт отримує snapshot

    Assert.AreEqual(service.TotalHoursSinceEpoch, proxy.TotalHoursSinceEpoch);
    Assert.AreEqual(service.CurrentDayPhase, proxy.CurrentDayPhase);
}
```

### Helpers / Fakes

Якщо потрібно замінити `ICalendarService` у тестах інших систем:

```csharp
public sealed class FakeCalendarService : ICalendarService
{
    public GameDateTime   Current              { get; set; } = new GameDateTime(1,1,1,6);
    public long           TotalHoursSinceEpoch { get; set; } = 0;
    public DayPhase       CurrentDayPhase      { get; set; } = DayPhase.Day;
    public CalendarConfig Config               { get; } = CalendarConfig.Default();

    public event Action            OnHourChanged;
    public event Action            OnDayChanged;
    public event Action            OnMonthChanged;
    public event Action            OnYearChanged;
    public event Action<DayPhase>  OnDayPhaseChanged;

    public void AdvanceTurn()       => TotalHoursSinceEpoch += Config.HoursPerTurn;
    public void SetByTotalHours(long h) => TotalHoursSinceEpoch = h;

    // Simulate events in tests:
    public void RaiseHourChanged()      => OnHourChanged?.Invoke();
    public void RaisePhaseChanged(DayPhase p) => OnDayPhaseChanged?.Invoke(p);
}
```

---

## Покриття подій

| Подія | Покриття |
|---|---|
| `OnHourChanged` | ✅ `AdvanceTurn_FiresOnHourChanged` |
| `OnDayChanged` | ✅ `AdvanceTurn_FiresOnDayChanged_AfterFullDay` |
| `OnMonthChanged` | 🔲 Можна додати за аналогією |
| `OnYearChanged` | 🔲 Можна додати за аналогією |
| `OnDayPhaseChanged` | ✅ `SetByTotalHours_FiresPhaseChangedEvent_WhenPhaseChanges` |
