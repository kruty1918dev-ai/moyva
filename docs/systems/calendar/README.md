# Calendar — Огляд системи

← [Назад до README](../../../README.md)

---

## Що це таке

**Calendar** — автономний доменний сервіс у проєкті Moyva, що відслідковує ігровий час:
роки, місяці, дні та години (без хвилин і секунд).

Він не рендерить нічого і не надсилає мережеві пакети напряму.  
Натомість він є **джерелом правди** про поточний ігровий час і сповіщає інші системи через події.

---

## Що вирішує

| Проблема | Рішення |
|---|---|
| Потрібен єдиний ігровий час для всіх систем | `ICalendarService` — єдина точка читання часу |
| День/ніч залежить від ігрового часу | `DayPhase` обчислюється з поточної години |
| Час у мультиплеєрі має бути синхронізований | `TotalHoursSinceEpoch` — канонічний індекс для реплікації |
| Дизайнери хочуть налаштовувати часову структуру | `CalendarConfig` + `CalendarConfigEditorWindow` |

---

## Де знаходиться код

```
Assets/Moyva/Scripts/Features/Calendar/
  Kruty1918.Moyva.Calendar.asmdef   ← runtime-збірка
  API/                              ← публічні контракти, моделі, enum-и
  Runtime/                          ← конкретні реалізації
  Editor/                           ← редактор конфігурації (лише Editor)

Assets/Moyva/Scripts/Tests/Calendar/
  Kruty1918.Moyva.Tests.Calendar.asmdef
  *.cs                              ← unit-тести (NUnit)
```

---

## Namespaces

| Namespace | Призначення |
|---|---|
| `Kruty1918.Moyva.Calendar.Domain` | `GameDateTime`, `DayPhase` |
| `Kruty1918.Moyva.Calendar.Config` | `CalendarConfig`, `ICalendarConfigStore` |
| `Kruty1918.Moyva.Calendar.Core` | `ICalendarService` |
| `Kruty1918.Moyva.Calendar.Multiplayer` | `ICalendarSyncAdapter` |
| `Kruty1918.Moyva.Calendar.Runtime` | `GameCalendarService`, `ClientCalendarProxy`, `CalendarBinaryConfigStore`, `CalendarSyncAdapter` |
| `Kruty1918.Moyva.Calendar.Editor` | `CalendarConfigEditorWindow` |

---

## Ключові принципи

- **Автономність** — Calendar не залежить від рендер-систем, мережі або UI.
- **Детермінованість** — той самий `TotalHoursSinceEpoch` завжди дає той самий `GameDateTime` і `DayPhase`.
- **Хост-авторитарність** — `GameCalendarService` крутиться на сервері; клієнти отримують `TotalHoursSinceEpoch` у знімку світу.
- **Незмінні моделі** — `CalendarConfig` і `GameDateTime` — sealed/readonly.
- **SOLID/TDD** — кожен клас залежить від абстракцій; є unit-тести.

---

## Документація

| Файл | Зміст |
|---|---|
| [architecture.md](architecture.md) | Збірки, класи, потік даних |
| [interfaces.md](interfaces.md) | Довідник усіх інтерфейсів і типів |
| [config.md](config.md) | `CalendarConfig`, зберігання, завантаження |
| [editor-guide.md](editor-guide.md) | Посібник користувача: календар, day/night візуал, пресети, тюнінг темної ночі |
| [quickstart.md](quickstart.md) | Покрокове увімкнення календаря в новій сцені |
| [multiplayer-integration.md](multiplayer-integration.md) | Інтеграція з мультиплеєром |
| [usage-examples.md](usage-examples.md) | Приклади використання в інших системах |
| [testing.md](testing.md) | Стратегія тестування |

---

## Схема взаємодії

```
┌────────────────────────────────────────────┐
│             GameCalendarService            │  ← авторитет (сервер)
│  Config: CalendarConfig                    │
│  State:  TotalHoursSinceEpoch (long)       │
│  Raise:  OnHourChanged / OnDayPhaseChanged │
└────────────┬───────────────────────────────┘
             │  ICalendarSyncAdapter.NotifyTurnCompleted()
             │
     ┌───────▼──────────┐
     │  Session / Turn  │  ← мультиплеєрний шар
     └───────┬──────────┘
             │  ApplyRemoteSnapshot(totalHours)
             │
┌────────────▼───────────────────────────────┐
│             ClientCalendarProxy            │  ← клієнт
│  Receive snapshot, raise events            │
└────────────┬───────────────────────────────┘
             │  subscribe
    ┌────────▼────────┐   ┌──────────────────┐
    │ DayNight Shader │   │  AI / Economy /  │
    │  (Visuals)      │   │  Quests / UI     │
    └─────────────────┘   └──────────────────┘
```

---

## Швидкий старт

→ Дивись [quickstart.md](quickstart.md)

---

## Редактор конфігурації

→ Дивись [editor-guide.md](editor-guide.md)

Меню Unity: `Moyva → Calendar → Config Hub`
