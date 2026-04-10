# Calendar — Швидкий старт

← [Назад до огляду](README.md)

---

## Мета цього документа

Показати покрокову процедуру: від нуля до працюючого ігрового календаря в новій сцені.

---

## Крок 1 — Створити конфіг

1. Відкрийте **Moyva → Calendar → Config Hub**.
2. Налаштуйте структуру під свій геймдизайн (або залиште дефолт: 12 місяців × 30 днів × 24 год.).
3. Натисніть **Save Config**.  
   → Файл `Assets/Moyva/calendar_config.dat` створено.

---

## Крок 2 — Завантажити конфіг у runtime

```csharp
ICalendarConfigStore store = new CalendarBinaryConfigStore("Assets/Moyva/calendar_config.dat");
CalendarConfig config = store.Load();
```

Або через Zenject:

```csharp
// CalendarInstaller.cs
Container.Bind<ICalendarConfigStore>()
    .To<CalendarBinaryConfigStore>()
    .AsSingle()
    .WithArguments("Assets/Moyva/calendar_config.dat");
```

---

## Крок 3 — Створити GameCalendarService

```csharp
// Ручне створення (без DI):
ICalendarConfigStore store = new CalendarBinaryConfigStore();
CalendarConfig config = store.Load();
ICalendarService calendar = new GameCalendarService(config);

// Через Zenject:
Container.Bind<ICalendarService>()
    .To<GameCalendarService>()
    .AsSingle();
```

---

## Крок 4 — Підписатись на події

```csharp
// Приклад: MonoBehaviour що слухає зміну фази
public class DayNightController : MonoBehaviour
{
    [Inject] private ICalendarService _calendar;

    private void Start()
    {
        _calendar.OnDayPhaseChanged += HandlePhaseChanged;
    }

    private void OnDestroy()
    {
        _calendar.OnDayPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(DayPhase phase)
    {
        Debug.Log($"[DayNight] Phase changed to {phase}");
        // → тут оновити шейдер, освітлення тощо
    }
}
```

---

## Крок 5 — Просувати час

### Одиночна гра / тест

```csharp
// Після кожного ходу гравця:
calendar.AdvanceTurn();   // +1 година (або HoursPerTurn)
```

### Мультиплеєр (через ICalendarSyncAdapter)

```csharp
ICalendarSyncAdapter adapter = new CalendarSyncAdapter(calendar);

// На сервері після завершення ходу:
adapter.NotifyTurnCompleted();

// На клієнті після отримання snapshot:
adapter.ApplyRemoteSnapshot(snapshotTotalHours);
```

---

## Крок 6 — Перевірити поточний стан

```csharp
GameDateTime now   = calendar.Current;
DayPhase     phase = calendar.CurrentDayPhase;
long         total = calendar.TotalHoursSinceEpoch;

Debug.Log($"[Calendar] {now} | {phase} | total={total}h");
```

---

## Повний приклад Zenject Installer

```csharp
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Multiplayer;
using Kruty1918.Moyva.Calendar.Runtime;
using Zenject;

public sealed class CalendarInstaller : MonoInstaller
{
    private const string ConfigPath = "Assets/Moyva/calendar_config.dat";

    public override void InstallBindings()
    {
        Container.Bind<ICalendarConfigStore>()
            .To<CalendarBinaryConfigStore>()
            .AsSingle()
            .WithArguments(ConfigPath);

        Container.Bind<ICalendarService>()
            .To<GameCalendarService>()
            .AsSingle();

        Container.Bind<ICalendarSyncAdapter>()
            .To<CalendarSyncAdapter>()
            .AsSingle();
    }
}
```

> **Ручний крок:** Додайте `CalendarInstaller` як компонент до `GameContext` GameObject у вашій сцені.

---

## Що далі?

- [multiplayer-integration.md](multiplayer-integration.md) — синхронізація часу у мультиплеєрі.
- [usage-examples.md](usage-examples.md) — приклади використання в шейдерах, AI, UI.
- [testing.md](testing.md) — як тестувати календар.
