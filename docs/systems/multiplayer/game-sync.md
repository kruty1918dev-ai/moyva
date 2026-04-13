# Синхронізація команд (`GameCommandSyncService`)

## Огляд

`GameCommandSyncService` реалізує `IGameCommandSyncService` та відповідає за:
- Надсилання ігрових команд усім пірам (broadcast)
- Отримання вхідних команд і їх диспетчеризацію до обробників

## Типи команд (`GameCommandType`)

| Команда | Значення | Опис |
|---|---|---|
| `UnitMove` | 1 | Переміщення юніта |
| `BuildingPlace` | 2 | Розміщення будівлі |
| `BuildingDemolish` | 3 | Знесення будівлі |
| `UnitSpawn` | 4 | Появлення юніта |
| `GameStateChange` | 5 | Зміна стану гри |

## Протокол пакету

```
[ 1 байт: GameCommandType ] [ N байт: payload ]
```

Перший байт — тип команди. Решта — довільні дані команди.

## API

```csharp
public interface IGameCommandSyncService
{
    void SendCommand(GameCommandType type, byte[] payload);
    void RegisterHandler(GameCommandType type, Action<string, byte[]> handler);
}
```

## Використання

### Надсилання команди

```csharp
[Inject] private IGameCommandSyncService _commandSync;

public void MoveUnit(string unitId, Vector2Int destination)
{
    byte[] payload = SerializeUnitMove(unitId, destination);
    _commandSync.SendCommand(GameCommandType.UnitMove, payload);
}
```

### Реєстрація обробника

```csharp
[Inject] private IGameCommandSyncService _commandSync;

public void Initialize()
{
    _commandSync.RegisterHandler(GameCommandType.UnitMove, OnUnitMoveReceived);
}

private void OnUnitMoveReceived(string senderId, byte[] payload)
{
    var (unitId, dest) = DeserializeUnitMove(payload);
    _unitMovementService.MoveUnit(unitId, dest);
}
```

## Реєстрація в Zenject

Реєструється у `MultiplayerInstaller`:

```csharp
Container.Bind<IGameCommandSyncService>()
    .To<GameCommandSyncService>()
    .AsSingle();
```

`GameCommandSyncService` автоматично підписується на `INetworkProvider.Messages` при ініціалізації та відписується при `Dispose()`.
