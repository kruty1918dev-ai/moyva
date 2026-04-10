# Calendar — Інтеграція з Multiplayer

← [Назад до огляду](README.md)

---

## Принцип

Календар є частиною стану світу і підпорядковується загальному принципу **хост-авторитарності** Moyva Multiplayer:

- `GameCalendarService` існує лише на **сервері/хості**.
- `ClientCalendarProxy` існує на кожному **клієнті**.
- Єдиний синхронізований стан — `TotalHoursSinceEpoch` (тип `long`).
- Гравці ніколи не бачать, скільки ходів зробив інший гравець — вони бачать лише поточний `GameDateTime`.

---

## ICalendarSyncAdapter

```csharp
public interface ICalendarSyncAdapter
{
    void NotifyTurnCompleted();              // хост → просуває час
    void ApplyRemoteSnapshot(long totalHours); // клієнт → отримує час
}
```

**`CalendarSyncAdapter`** — стандартна реалізація, що зв'язує `GameCalendarService` і `ClientCalendarProxy`.

---

## Де підключати в SessionManager / TurnManager

Оскільки Moyva Multiplayer — це carcass-архітектура без реального транспорту, точні місця підключення залежать від вашої реалізації TurnManager. Нижче — рекомендована схема.

### Серверна сторона (хост)

```csharp
// Після того, як усі гравці/боти виконали свій хід:
calendarSyncAdapter.NotifyTurnCompleted();
// → внутрішньо: GameCalendarService.AdvanceTurn()

// Потім зібрати snapshot і відправити клієнтам:
long totalHours = calendarService.TotalHoursSinceEpoch;
SendWorldSnapshot(new WorldSnapshotWithCalendar(worldId, version, checksum, totalHours));
```

### Клієнтська сторона

```csharp
// Отримати snapshot від хоста:
void OnSnapshotReceived(WorldSnapshotWithCalendar snapshot)
{
    worldConsistency.Compare(...);
    calendarSyncAdapter.ApplyRemoteSnapshot(snapshot.CalendarTotalHours);
    // → ClientCalendarProxy.ApplySnapshot(totalHours)
    // → OnHourChanged / OnDayPhaseChanged спрацьовують
}
```

---

## Розширення WorldSnapshot для календаря

Поточний `WorldSnapshot` (в Multiplayer API) зберігає лише `WorldId`, `Version`, `Checksum`.  
Для повноцінної інтеграції потрібно розширити або обгорнути його:

```csharp
// Варіант 1: новий DTO
public sealed class WorldSnapshotWithCalendar
{
    public WorldSnapshot Base        { get; }
    public long CalendarTotalHours   { get; }

    public WorldSnapshotWithCalendar(WorldSnapshot @base, long calendarTotalHours)
    {
        Base               = @base;
        CalendarTotalHours = calendarTotalHours;
    }
}

// Варіант 2: включити як окремий payload у мережеве повідомлення
// (залежить від вашого INetworkProvider)
```

> Поточна реалізація `WorldSnapshot` є незмінною (sealed, без сеттерів),  
> тому рекомендується Варіант 1 — обгортка.

---

## Ініціалізація Calendar при старті сесії

```csharp
// У SessionManager.CreateOrJoinSessionAsync (або після нього):
CalendarConfig calendarConfig = calendarConfigStore.Load();
var calendarService = new GameCalendarService(calendarConfig);

// Якщо відновлюємо збережену гру — відновити TotalHoursSinceEpoch:
if (savedGame != null)
    calendarService.SetByTotalHours(savedGame.CalendarTotalHours);
```

---

## Захист балансу: гравці не бачать ходи один одного

Схема "ніхто не знає про хід іншого":

1. Кожен гравець виконує хід локально та надсилає серверу лише **підсумок** (без часової мітки власного ходу).
2. Сервер чекає, поки **всі** гравці/боти відправлять хід.
3. Тільки тоді сервер викликає `NotifyTurnCompleted()` і broadcastить `TotalHoursSinceEpoch`.
4. Клієнти отримують **тільки час** — не порядок і не час відправлення ходів.

Таким чином, **`GameDateTime` та `TotalHoursSinceEpoch` є єдиними зовнішньо видимими часовими значеннями** і не розкривають поведінку інших гравців.

---

## Зв'язок з ParticipantPolicyService

Multiplayer `ParticipantPolicyService.CanJoin()` перевіряє `WorldSnapshot`.  
Якщо `WorldSnapshotWithCalendar.CalendarTotalHours` відрізняється між хостом і клієнтом, це є підставою для відхилення у `WorldConsistencyService.Compare()`.

Рекомендується включити `CalendarTotalHours` до обчислення `Checksum` в `WorldSnapshot`:

```csharp
// У вашій реалізації WorldSnapshotStore:
uint checksum = ComputeChecksum(worldData, calendarService.TotalHoursSinceEpoch);
```

---

## Діаграма взаємодії

```
[Player 1 turn done] ──┐
[Player 2 turn done] ──┼──→ TurnManager (server)
[Bot    1 turn done] ──┘        │
                                ▼
                    calendarSyncAdapter.NotifyTurnCompleted()
                                │
                    GameCalendarService.AdvanceTurn()
                    TotalHoursSinceEpoch++
                                │
                    BroadcastSnapshot(totalHours)
                                │
                    ┌───────────▼──────────────┐
                    │  ClientCalendarProxy     │  (per client)
                    │  ApplySnapshot(hours)    │
                    │  → OnDayPhaseChanged     │
                    └──────────────────────────┘
```
