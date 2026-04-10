# Multiplayer — Логування та обробка помилок

← [Назад до огляду](README.md)

---

## Концепція

Multiplayer-система ніколи не кидає виключень у нормальному потоці.  
Усі помилки:
1. **Логуються** через `IMultiplayerLogger`.
2. **Маршрутизуються** через `IFailureHandlingPolicy`.

Це дозволяє:
- Легко замінити спосіб логування (Unity console, файл, telemetry).
- Налаштувати реакцію на помилки без зміни ядра системи.
- Тестувати систему без будь-яких side effects.

---

## IMultiplayerLogger

**Файл:** `API/IMultiplayerLogger.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

```csharp
public interface IMultiplayerLogger
{
    void Info(string message);    // інформаційне повідомлення
    void Warn(string message);    // попередження (recoverable ситуація)
    void Error(string message);   // критична помилка
    void Trace(string message);   // детальне трасування (для дебагу)
}
```

### Де використовується

| Клас | Що логує |
|---|---|
| `SessionManager` | Початок/кінець сесії, checksum mismatch, мережеві помилки |
| `ParticipantPolicyService` | Причина відхилення учасника |
| `WorldConsistencyService` | Деталі mismatch (WorldId, checksum) |
| `SimpleFailureHandlingPolicy` | Категорія та деталі помилки |

---

## UnityMultiplayerLogger

**Файл:** `Runtime/UnityMultiplayerLogger.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Runtime`

Runtime-безпечна реалізація, що делегує до `UnityEngine.Debug`.

```csharp
public sealed class UnityMultiplayerLogger : IMultiplayerLogger
{
    private const string Tag = "[Multiplayer]";

    public void Info(string message)  => Debug.Log($"{Tag} {message}");
    public void Warn(string message)  => Debug.LogWarning($"{Tag} {message}");
    public void Error(string message) => Debug.LogError($"{Tag} {message}");
    public void Trace(string message) => Debug.Log($"{Tag} TRACE {message}");
}
```

Всі повідомлення мають префікс `[Multiplayer]` для легкого фільтрування у Console.

---

## IFailureHandlingPolicy

**Файл:** `API/IFailureHandlingPolicy.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Core`

```csharp
public interface IFailureHandlingPolicy
{
    // Повертає true = спробувати ще раз, false = перервати
    bool HandleRecoverable(FailureCategory category, string details);

    // Викликається при критичній помилці
    void HandleNonRecoverable(FailureCategory category, string details);
}
```

### FailureCategory

| Значення | Коли виникає |
|---|---|
| `Unknown` | Невідома помилка |
| `NetworkDisconnect` | Мережа розірвана / з'єднання не вдалось |
| `ConfigMismatch` | Checksum конфігу не збігається |
| `WorldMismatch` | Checksum або WorldId стану світу не збігається |
| `ParticipantRejected` | Учасника відхилено ParticipantPolicyService |
| `HostMigrationFailed` | Не вдалось обрати новий хост |
| `SessionFull` | Кількість учасників вже на максимумі |
| `StrictLockViolation` | Гравець не входить у locked participant set |

---

## SimpleFailureHandlingPolicy

**Файл:** `Runtime/SimpleFailureHandlingPolicy.cs`  
**Namespace:** `Kruty1918.Moyva.Multiplayer.Runtime`

```csharp
public sealed class SimpleFailureHandlingPolicy : IFailureHandlingPolicy
{
    private readonly IMultiplayerLogger _logger;

    public SimpleFailureHandlingPolicy(IMultiplayerLogger logger)
        => _logger = logger;

    public bool HandleRecoverable(FailureCategory category, string details)
    {
        _logger.Warn($"Recoverable failure [{category}]: {details}");
        return false;  // не повторювати
    }

    public void HandleNonRecoverable(FailureCategory category, string details)
    {
        _logger.Error($"Non-recoverable failure [{category}]: {details}");
        // не кидаємо виключення
    }
}
```

---

## Як писати власні реалізації

### Кастомний logger (наприклад, у файл)

```csharp
public sealed class FileMultiplayerLogger : IMultiplayerLogger
{
    private readonly string _filePath;

    public FileMultiplayerLogger(string filePath)
        => _filePath = filePath;

    public void Info(string msg)  => Append("INFO ", msg);
    public void Warn(string msg)  => Append("WARN ", msg);
    public void Error(string msg) => Append("ERROR", msg);
    public void Trace(string msg) => Append("TRACE", msg);

    private void Append(string level, string msg)
        => File.AppendAllText(_filePath, $"[{DateTime.UtcNow:HH:mm:ss}] [{level}] [Multiplayer] {msg}\n");
}
```

### Кастомна policy (наприклад, з retry)

```csharp
public sealed class RetryOnNetworkFailurePolicy : IFailureHandlingPolicy
{
    private readonly IMultiplayerLogger _logger;
    private int _retryCount = 0;
    private const int MaxRetries = 3;

    public RetryOnNetworkFailurePolicy(IMultiplayerLogger logger)
        => _logger = logger;

    public bool HandleRecoverable(FailureCategory category, string details)
    {
        if (category == FailureCategory.NetworkDisconnect && _retryCount < MaxRetries)
        {
            _retryCount++;
            _logger.Warn($"Network failure (attempt {_retryCount}/{MaxRetries}): {details}");
            return true;  // retry
        }

        _retryCount = 0;
        _logger.Error($"Failure [{category}]: {details}");
        return false;
    }

    public void HandleNonRecoverable(FailureCategory category, string details)
        => _logger.Error($"Fatal [{category}]: {details}");
}
```

### Fake logger для тестів

```csharp
private sealed class FakeLogger : IMultiplayerLogger
{
    public List<string> Warnings { get; } = new List<string>();
    public List<string> Errors { get; } = new List<string>();

    public void Info(string msg)  { }
    public void Warn(string msg)  => Warnings.Add(msg);
    public void Error(string msg) => Errors.Add(msg);
    public void Trace(string msg) { }
}

// У тесті:
var logger = new FakeLogger();
// ... виконати операцію ...
Assert.AreEqual(1, logger.Warnings.Count);
Assert.IsTrue(logger.Warnings[0].Contains("session full"));
```

---

## Приклади логів у Unity Console

```
[Multiplayer] CreateOrJoinSession: room=room-42, create=True
[Multiplayer] Session established: room-42, host=True

[Multiplayer] WARN CanJoin rejected: session full (4/4).
[Multiplayer] WARN Recoverable failure [ParticipantRejected]: Participant p5 rejected.

[Multiplayer] ERROR Network operation failed: No session to join in offline mode.
[Multiplayer] ERROR Non-recoverable failure [NetworkDisconnect]: No session to join in offline mode.

[Multiplayer] WARN Config checksum mismatch: local=AB12CD34, remote=DEADBEEF
[Multiplayer] WARN Recoverable failure [ConfigMismatch]: Config checksums differ.
```

---

## Фільтрація у Unity Console

Щоб бачити лише multiplayer-повідомлення, введіть у пошуковий рядок консолі:

```
[Multiplayer]
```
