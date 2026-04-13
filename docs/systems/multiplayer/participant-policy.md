# Multiplayer — ParticipantPolicyService та правило 4 гравців

← [Назад до огляду](README.md)

---

## Призначення

`ParticipantPolicyService` перевіряє, чи може новий учасник приєднатись до сесії.

Перевіряються три речі:
1. **Загальний ліміт** — не перевищено `MaxParticipants`.
2. **Ліміт людей/ботів** — не перевищено `MaxHumans` / `MaxBots`.
3. **Strict world lock** — учасник входить у зафіксований список гравців для цього світу.

---

## Розташування

```
Assets/Moyva/Scripts/Features/Multiplayer/Runtime/ParticipantPolicyService.cs
```

**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`  
**Реалізує:** `IParticipantPolicyService`

---

## Метод `CanJoin`

```csharp
public bool CanJoin(
    ParticipantIdentity candidate,
    IReadOnlyList<Participant> currentParticipants,
    SessionRules rules,
    WorldSnapshot worldSnapshot)  // null якщо world не існує або новий
```

### Алгоритм

```
1. total = currentParticipants.Count
   if total >= rules.MaxParticipants → false (session full)

2. humanCount = учасники де IsBot = false
   botCount = учасники де IsBot = true

3. if candidate.PlayerId НЕ починається з "BOT_":
       if humanCount >= rules.MaxHumans → false (max humans)
   else:
       if botCount >= rules.MaxBots → false (max bots)

4. if rules.StrictParticipantLock AND worldSnapshot ≠ null:
       if NOT IsInLockedSet(candidate, worldId) → false (strict lock)

5. return true
```

---

## Ідентифікація ботів

Бот ідентифікується за префіксом `PlayerId`:

```csharp
// Константа у ParticipantIdentity
public const string BotIdPrefix = "BOT_";

// Перевірка у PolicyService
bool isBot = candidate.PlayerId.StartsWith(ParticipantIdentity.BotIdPrefix);
```

**Приклади:**
- `"player-001"` → людина
- `"BOT_alpha"` → бот
- `"BOT_007"` → бот

---

## Strict 4-player world lock

Це механізм, який гарантує: **якщо у певному світі грали 4 конкретні гравці, лише вони можуть продовжити цей самий світ**.

### Поточний стан (carcass)

```csharp
private bool IsInLockedSet(ParticipantIdentity candidate, string worldId)
{
    // TODO: Implement strict lock validation once the world snapshot stores participant sets.
    // Для carcass повертаємо true (пропускаємо всіх)
    return true;
}
```

### Як буде працювати (план)

При збереженні сесії з 4 гравцями у `WorldSnapshot` зберігається набір `ParticipantIdentity`:

```
WorldSnapshot
  └── LockedParticipants: HashSet<ParticipantIdentity>  (додати до моделі)
```

При вході:
```
worldSnapshot.LockedParticipants.Contains(candidate) → true = дозволено
```

### Налаштування через Config Hub

- **Strict 4-Player World Lock** → `true` у вікні Config Hub.
- Або через `SessionRules.StrictParticipantLock = true`.

### Приклад поведінки (після реалізації)

```
World-001 graven by: [Alice, Bob, Charlie, Dave]

Alice tries to join World-001 → ✅ Allowed (in locked set)
Eve tries to join World-001   → ❌ Rejected (not in locked set)

Clone of World-001 (World-001-copy) → ✅ Eve can join (clone has relaxed rules)
```

---

## Приклади CanJoin

### Сесія ще порожня — дозволити

```csharp
var identity = new ParticipantIdentity("p1", "Player1");
var rules = SessionRules.Default();  // MaxParticipants = 4, MaxHumans = 4, MaxBots = 0

bool result = policyService.CanJoin(identity, new List<Participant>(), rules, null);
// result = true
```

### Сесія заповнена (4/4) — відхилити

```csharp
var participants = new List<Participant>
{
    new Participant(new ParticipantIdentity("p1", "P1"), false, true),
    new Participant(new ParticipantIdentity("p2", "P2"), false, false),
    new Participant(new ParticipantIdentity("p3", "P3"), false, false),
    new Participant(new ParticipantIdentity("p4", "P4"), false, false),
};

bool result = policyService.CanJoin(new ParticipantIdentity("p5", "P5"), participants, rules, null);
// result = false (session full)
```

### Досягнуто ліміт людей (2/2)

```csharp
var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 2, false, false, false);
var participants = new List<Participant>
{
    new Participant(new ParticipantIdentity("p1", "P1"), false, true),
    new Participant(new ParticipantIdentity("p2", "P2"), false, false),
};

bool result = policyService.CanJoin(new ParticipantIdentity("p3", "P3"), participants, rules, null);
// result = false (max humans reached: 2/2)
```

### Бот може приєднатись (є вільний слот)

```csharp
var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 2, false, false, false);
var participants = new List<Participant>
{
    new Participant(new ParticipantIdentity("BOT_1", "Bot1"), true, false),
};

bool result = policyService.CanJoin(new ParticipantIdentity("BOT_2", "Bot2"), participants, rules, null);
// result = true (botCount = 1/2)
```

---

## Залежності

```csharp
public ParticipantPolicyService(
    IMultiplayerLogger logger,
    IWorldSnapshotStore snapshotStore)
```

- `IMultiplayerLogger` — логує причину відхилення.
- `IWorldSnapshotStore` — зарезервовано для майбутнього завантаження locked participant set.

---

## IParticipantFallbackService (carcass)

**Файл:** `API/IParticipantFallbackService.cs`

Описує, що робити коли учасник виходить (наприклад, замінити ботом):

```csharp
public interface IParticipantFallbackService
{
    Participant GetFallback(
        ParticipantIdentity leavingParticipant,
        IReadOnlyList<Participant> remaining,
        SessionRules rules);
}
```

Якщо `AllowBotsFallbackOnLeave = true` у правилах, цей сервіс повинен повернути Participant з `IsBot = true`.  
Реалізовано у `ParticipantFallbackService`. Деталі: [host-migration.md](host-migration.md).

---

## IHostMigrationService (carcass)

**Файл:** `API/IHostMigrationService.cs`

Обирає нового хоста при відключенні поточного:

```csharp
public interface IHostMigrationService
{
    Participant ChooseNewHost(IReadOnlyList<Participant> remaining);
}
```

Реалізовано у `HostMigrationService` (пріоритет — перший живий учасник-людина). Деталі: [host-migration.md](host-migration.md).  
Повертає `null` якщо немає людських кандидатів.
