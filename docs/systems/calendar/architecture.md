# Calendar — Архітектура

← [Назад до огляду](README.md)

---

## Збірки (Assembly Definitions)

| Збірка | Шлях | Роль |
|---|---|---|
| `Kruty1918.Moyva.Calendar` | `Assets/Moyva/Scripts/Features/Calendar/` | Runtime: API + сервіси |
| `Kruty1918.Moyva.Calendar.Editor` | `Assets/Moyva/Scripts/Features/Calendar/Editor/` | Editor: вікно конфігурації |
| `Kruty1918.Moyva.Tests.Calendar` | `Assets/Moyva/Scripts/Tests/Calendar/` | Unit-тести (NUnit) |

**Залежності:**

```
Kruty1918.Moyva.Calendar.Editor  →  Kruty1918.Moyva.Calendar
Kruty1918.Moyva.Tests.Calendar   →  Kruty1918.Moyva.Calendar
```

Основна збірка `Kruty1918.Moyva.Calendar` **не залежить** від інших Moyva-збірок.  
Це дозволяє підключати Calendar до будь-якого модуля без ризику циклічних посилань.

---

## Класи та файли

### API (публічні контракти)

| Файл | Тип | Namespace | Призначення |
|---|---|---|---|
| `API/DayPhase.cs` | `enum DayPhase` | `Kruty1918.Moyva.Calendar.Domain` | Night / Dawn / Day / Dusk |
| `API/GameDateTime.cs` | `readonly struct GameDateTime` | `Kruty1918.Moyva.Calendar.Domain` | Незмінна мітка часу: Year/Month/Day/Hour |
| `API/CalendarConfig.cs` | `sealed class CalendarConfig` | `Kruty1918.Moyva.Calendar.Config` | Незмінна конфігурація календаря |
| `API/ICalendarConfigStore.cs` | `interface ICalendarConfigStore` | `Kruty1918.Moyva.Calendar.Config` | Load / Save / Exists |
| `API/ICalendarService.cs` | `interface ICalendarService` | `Kruty1918.Moyva.Calendar.Core` | Основний контракт сервісу |
| `API/ICalendarSyncAdapter.cs` | `interface ICalendarSyncAdapter` | `Kruty1918.Moyva.Calendar.Multiplayer` | Мультиплеєрна точка інтеграції |

### Runtime (реалізації)

| Файл | Тип | Призначення |
|---|---|---|
| `Runtime/GameCalendarService.cs` | `sealed class GameCalendarService` | Авторитативна реалізація (сервер) |
| `Runtime/ClientCalendarProxy.cs` | `sealed class ClientCalendarProxy` | Клієнтський проксі (read-only) |
| `Runtime/CalendarBinaryConfigStore.cs` | `sealed class CalendarBinaryConfigStore` | Зберігання конфігу у бінарному файлі |
| `Runtime/CalendarSyncAdapter.cs` | `sealed class CalendarSyncAdapter` | Реалізація `ICalendarSyncAdapter` |
| `Runtime/AssemblyInfo.cs` | — | `InternalsVisibleTo("Kruty1918.Moyva.Tests.Calendar")` |

### Editor

| Файл | Тип | Призначення |
|---|---|---|
| `Editor/CalendarConfigEditorWindow.cs` | `sealed class CalendarConfigEditorWindow : EditorWindow` | Unity EditorWindow для налаштування конфігу |

### Tests

| Файл | Тип | Призначення |
|---|---|---|
| `Tests/Calendar/GameCalendarServiceTests.cs` | `[TestFixture]` | Тести дати, фаз, подій |
| `Tests/Calendar/CalendarConfigTests.cs` | `[TestFixture]` | Тести конфігу та бінарного serialization |

---

## Потік даних

```
1. Розробник відкриває Moyva → Calendar → Config Hub
2. Зберігає CalendarConfig у Assets/Moyva/calendar_config.dat
   (через CalendarBinaryConfigStore)

3. На старті сесії:
   - CalendarBinaryConfigStore.Load() → CalendarConfig
   - new GameCalendarService(config)

4. Після кожного завершеного ходу (сервер):
   - ICalendarSyncAdapter.NotifyTurnCompleted()
     → GameCalendarService.AdvanceTurn()
     → TotalHoursSinceEpoch += HoursPerTurn
     → Recalculate GameDateTime + DayPhase
     → Fire events

5. Реплікація на клієнти:
   - Snapshot містить TotalHoursSinceEpoch
   - Клієнт отримує snapshot
   - ICalendarSyncAdapter.ApplyRemoteSnapshot(totalHours)
     → ClientCalendarProxy.ApplySnapshot(totalHours)
     → Recalculate + Fire events

6. Підписники (DayNight shader, AI, UI):
   - Підписались на ICalendarService.OnDayPhaseChanged
   - Отримують нову фазу і реагують
```

---

## Інтеграція з іншими системами

### Multiplayer

Детальніше: [multiplayer-integration.md](multiplayer-integration.md)

- `ICalendarSyncAdapter` є точкою підключення до `SessionManager` або `TurnManager`.
- `WorldSnapshot` (або аналог) має містити `TotalHoursSinceEpoch` для реплікації.
- Ніхто з гравців не бачить, скільки ходів зробив інший гравець — вони бачать лише `GameDateTime`.

### Save System

- `CalendarBinaryConfigStore` використовує той самий підхід, що й `BinaryConfigStore` Multiplayer.
- Поточний `TotalHoursSinceEpoch` може зберігатися разом із загальним збереженням гри.

### Visuals / DayNight Shader

- Немає прямої залежності — підписник сам слухає `OnDayPhaseChanged` або читає `CurrentDayPhase`.

### GameMode / Bootstrap

- `GameCalendarService` та `CalendarBinaryConfigStore` підключаються через Zenject Installer у сцені.
- Рекомендований порядок ініціалізації: Config → CalendarService → SessionManager.

---

## Відсутні залежності (no Unity в тестах)

`GameCalendarService` і `CalendarBinaryConfigStore` — чисті C# класи.  
`Application.persistentDataPath` у `CalendarBinaryConfigStore` викликається лише при `new CalendarBinaryConfigStore()` без аргументів — тести передають власний шлях.
