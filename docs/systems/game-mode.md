# GameMode — Система ігрових режимів

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/game-mode)

---

## Призначення

Система **GameMode** керує поточним режимом гри. Вона надає єдине джерело правди про те, **який режим активний зараз**, і транслює зміну режиму через `SignalBus`. Інші системи (Interactions, Construction тощо) самостійно підписуються на сигнал і регулюють свою активність.

> Це дозволяє легко додавати нові режими без зміни наявних систем — кожна система знає лише про існування сигналу, а не про конкретних відправників.

---

## Режими гри

| Режим | Значення | Опис |
|---|---|---|
| `Normal` | `0` | Стандартний ігровий режим: керування юнітами, навігація по карті |
| `Construction` | `1` | Режим будівництва: Interactions вимкнено, активний ConstructionService |

---

## Як працює внутрішньо

1. `GameModeService` зберігає `CurrentMode` типу `GameModeType`.
2. При виклику `SetMode(newMode)`:
   - якщо `newMode == CurrentMode` → ніякої дії;
   - інакше → `CurrentMode = newMode` і `SignalBus.Fire(GameModeChangedSignal { NewMode })`.
3. `TileInteractionService` підписується на `GameModeChangedSignal` і вимикає обробку кліків у режимі `Construction`.
4. `ConstructionService` підписується на той самий сигнал і активується лише в режимі `Construction`.
5. `GameModePanelController` підписується на `GameModeChangedSignal` і показує/приховує всі зареєстровані `IGameModePanel`.

```
UI викликає IGameModeService.SetMode(Construction)
    │
    ▼
GameModeService: CurrentMode ← Construction
    │
    ▼
SignalBus.Fire(GameModeChangedSignal { NewMode = Construction })
    │
    ├─► TileInteractionService.OnGameModeChanged()  → _isActive = false
    ├─► ConstructionService.OnGameModeChanged()     → _isActive = true
    └─► GameModePanelController.OnGameModeChanged()
            │
            ├─► panel.TargetMode == Construction → panel.Show()
            └─► panel.TargetMode != Construction → panel.Hide()
```

---

## Публічний API

### `IGameModeService`

```csharp
namespace Kruty1918.Moyva.GameMode.API
{
    public interface IGameModeService
    {
        /// <summary>Поточний активний режим гри.</summary>
        GameModeType CurrentMode { get; }

        /// <summary>
        /// Перемикає режим гри. Якщо newMode == CurrentMode — нічого не відбувається.
        /// При зміні надсилає GameModeChangedSignal.
        /// </summary>
        void SetMode(GameModeType newMode);
    }
}
```

### `GameModeType`

```csharp
namespace Kruty1918.Moyva.GameMode.API
{
    public enum GameModeType
    {
        Normal       = 0,
        Construction = 1
    }
}
```

### `IGameModePanel`

Контракт для UI-панелей, видимість яких залежить від активного режиму гри.
`GameModePanelController` автоматично показує/приховує всі зареєстровані реалізації.

```csharp
namespace Kruty1918.Moyva.GameMode.API
{
    public interface IGameModePanel
    {
        /// <summary>Режим гри, при якому ця панель має бути видима.</summary>
        GameModeType TargetMode { get; }

        void Show();
        void Hide();
    }
}
```

**Як додати нову UI-панель для свого режиму:**

1. Реалізуйте `IGameModePanel` у своєму MonoBehaviour або plain-класі.
2. Вкажіть `TargetMode`.
3. Зареєструйте в інсталері фічі:

```csharp
Container.BindInterfacesTo<MyFeaturePanel>().AsSingle();
```

Більше нічого не потрібно — `GameModePanelController` отримає панель через Zenject
і керуватиме нею автоматично.

---

## Компоненти

### `GameModeChangeRequestRouter`

Посередник між UI-підсистемами та `IGameModeService`.
UI-контролери не викликають `SetMode()` напряму — вони надсилають `GameModeChangeRequestedSignal`,
а роутер вирішує, чи дозволити зміну.

Цей паттерн дозволяє у майбутньому додати валідацію переходів (наприклад, заборона виходу з будівництва під час анімації).

```csharp
public sealed class GameModeChangeRequestRouter : IInitializable, IDisposable
{
    [Inject]
    public GameModeChangeRequestRouter(SignalBus signalBus, IGameModeService gameModeService) { ... }

    // Підписується на GameModeChangeRequestedSignal → делегує до SetMode()
}
```

**Потік запиту:**

```
UI натискає кнопку «Будівництво»
    │
    ▼
SignalBus.Fire(GameModeChangeRequestedSignal { RequestedMode = Construction })
    │
    ▼
GameModeChangeRequestRouter.OnModeChangeRequested()
    │
    ▼
IGameModeService.SetMode(Construction)
    │
    ▼
SignalBus.Fire(GameModeChangedSignal { NewMode = Construction })
```

---

### `GameModeUIController`

MonoBehaviour, який керує кнопками входу/виходу з режиму будівництва.
Інжектується Zenject через `QueueForInject` + `FromInstance`.

```csharp
public class GameModeUIController : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private GameObject enterConstructionButton;
    [SerializeField] private GameObject exitConstructionButton;

    // Кнопка Enter → Fire(GameModeChangeRequestedSignal { Construction })
    // Кнопка Exit → Fire(GameModeChangeRequestedSignal { Normal })
    // Підписується на GameModeChangedSignal → ховає/показує кнопки
}
```

**Як підключити в Unity:**
1. Додай `GameModeUIController` до GameObject з кнопками режимів.
2. Перетягни `enterConstructionButton` та `exitConstructionButton` у Inspector.
3. `GameModeInstaller` знайде контролер через `FindObjectOfType` і зареєструє автоматично.

---

## Сигнали

### `GameModeChangedSignal`

Надсилається: `GameModeService.SetMode()`
Отримується: `TileInteractionService`, `ConstructionService`, `GameModePanelController`, `GameModeUIController`, `ConstructionUIController`

```csharp
public struct GameModeChangedSignal
{
    public GameModeType NewMode;
}
```

### `GameModeChangeRequestedSignal`

Надсилається: `ConstructionUIController.EnterConstructionMode()`, `GameModeUIController`
Отримується: `GameModeChangeRequestRouter`

```csharp
public struct GameModeChangeRequestedSignal
{
    public GameModeType RequestedMode;
}
```

---

## Реєстрація в Zenject (`GameModeInstaller`)

```csharp
public sealed class GameModeInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IGameModeService>()
            .To<GameModeService>()
            .AsSingle()
            .NonLazy();

        Container.BindInterfacesAndSelfTo<GameModePanelController>()
            .AsSingle()
            .NonLazy();

        Container.BindInterfacesAndSelfTo<GameModeChangeRequestRouter>()
            .AsSingle()
            .NonLazy();

        // GameModeUIController — знаходиться на сцені через FindObjectOfType
        var gameModeUiController = Object.FindObjectOfType<GameModeUIController>(true);
        if (gameModeUiController != null)
        {
            Container.QueueForInject(gameModeUiController);
            Container.BindInterfacesAndSelfTo<GameModeUIController>()
                .FromInstance(gameModeUiController)
                .AsSingle()
                .NonLazy();
        }

        Container.BindExecutionOrder<GameModeChangeRequestRouter>(-10);
        Container.BindExecutionOrder<GameModePanelController>(-10);
        Container.BindExecutionOrder<GameModeUIController>(-5);
    }
}
```

Додайте `GameModeInstaller` до SceneContext у Unity Inspector.

---

## Залежності

| Залежність | Причина |
|---|---|
| [`SignalBus`](signals/README.md) | Надсилання `GameModeChangedSignal` і `GameModeChangeRequestedSignal` |

---

## Пов'язані системи

- [Signals](signals/README.md) — `GameModeChangedSignal`, `GameModeChangeRequestedSignal`
- [Interactions](interactions.md) — вимикається в режимі `Construction`
- [Construction](construction.md) — активується в режимі `Construction`
- [Construction UI](construction/ui.md) — надсилає `GameModeChangeRequestedSignal` для входу в будівництво
