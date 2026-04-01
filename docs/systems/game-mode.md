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

```
UI/GameManager викликає IGameModeService.SetMode(Construction)
    │
    ▼
GameModeService: CurrentMode ← Construction
    │
    ▼
SignalBus.Fire(GameModeChangedSignal { NewMode = Construction })
    │
    ├─► TileInteractionService.OnGameModeChanged() → _isActive = false
    └─► ConstructionService.OnGameModeChanged()    → _isActive = true
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

---

## Сигнали

### `GameModeChangedSignal`

Надсилається: `GameModeService.SetMode()`
Отримується: `TileInteractionService`, `ConstructionService`

```csharp
public struct GameModeChangedSignal
{
    public GameModeType NewMode;
}
```

---

## Реєстрація в Zenject (`GameModeInstaller`)

```csharp
internal sealed class GameModeInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IGameModeService>()
            .To<GameModeService>()
            .AsSingle();
    }
}
```

Додайте `GameModeInstaller` до SceneContext у Unity Inspector.

---

## Залежності

| Залежність | Причина |
|---|---|
| [`SignalBus`](signals.md) | Надсилання `GameModeChangedSignal` при зміні режиму |

---

## Пов'язані системи

- [Signals](signals.md) — `GameModeChangedSignal`
- [Interactions](interactions.md) — вимикається в режимі `Construction`
- [Construction](construction.md) — активується в режимі `Construction`
