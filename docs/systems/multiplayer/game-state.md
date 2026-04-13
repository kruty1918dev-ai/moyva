# Ігровий цикл (`GameStateService`)

## Огляд

`GameStateService` керує станом ігрового циклу: старт гри, пауза, відновлення та завершення.

## Стани (`GameStateType`)

| Стан | Опис |
|---|---|
| `Idle` | Гра ще не розпочалася |
| `Playing` | Гра активна |
| `Paused` | Гра на паузі |
| `GameOver` | Гра завершена |

## Сигнали

| Сигнал | Коли надсилається |
|---|---|
| `GameStartedSignal` | При виклику `StartGame()` |
| `GamePausedSignal` (`IsPaused = true`) | При виклику `PauseGame()` |
| `GamePausedSignal` (`IsPaused = false`) | При виклику `ResumeGame()` |
| `GameEndedSignal` | При виклику `EndGame(winnerId)` |

## API

```csharp
public interface IGameStateService
{
    GameStateType CurrentState { get; }
    void StartGame();
    void PauseGame();
    void ResumeGame();
    void EndGame(string winnerId);
}
```

## Використання

```csharp
public class MyController
{
    [Inject] private IGameStateService _gameState;

    public void OnStartButtonClicked()
    {
        _gameState.StartGame();
    }

    public void OnPauseButtonClicked()
    {
        if (_gameState.CurrentState == GameStateType.Playing)
            _gameState.PauseGame();
        else if (_gameState.CurrentState == GameStateType.Paused)
            _gameState.ResumeGame();
    }
}
```

## Підписка на сигнали

```csharp
[Inject] private SignalBus _signalBus;

private void Start()
{
    _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
    _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
}

private void OnGameStarted() => Debug.Log("Гра розпочата!");
private void OnGameEnded(GameEndedSignal signal)
    => Debug.Log($"Гра завершена. Переможець: {signal.WinnerId ?? "нічия"}");
```

## Реєстрація в Zenject

`IGameStateService` → `GameStateService` реєструється у `GameModeInstaller`:

```csharp
Container.Bind<IGameStateService>()
    .To<GameStateService>()
    .AsSingle();
```

Сигнали оголошуються у `SignalBusInstaller`:

```csharp
Container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
Container.DeclareSignal<GameEndedSignal>().OptionalSubscriber();
Container.DeclareSignal<GamePausedSignal>().OptionalSubscriber();
```
