# Animations — Система анімацій руху

← [Назад до README](../README.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/docs/#systems/animations)

---

## Призначення

Система **Animations** відповідає за плавне візуальне переміщення ігрових об'єктів (юнітів) вздовж заданого шляху. Вона є чистою анімаційною службою — не знає нічого про юнітів або сітку, лише переміщує `Transform` між точками.

---

## Як працює внутрішньо

1. Отримує `Transform` цільового об'єкта та список `Vector2Int` — координат шляху.
2. Перебирає шлях по одному кроку. Перед кожним кроком:
   - Викликає делегат `CanPerformStep(nextPos)` — якщо повертає `false`, рух зупиняється (наприклад, не вистачає стаміни).
3. Для кожного кроку виконує лінійну інтерполяцію (`Lerp`) позиції протягом `MoveDurationPerTile` секунд через `await Task.Yield()`.
4. Після завершення кроку: викликає `OnStepCompleted(nextPos)` (там надсилається `UnitMovedSignal`).
5. Між кроками — `Task.Delay(DelayOnTile)` для природного ритму.
6. Весь цикл реагує на `CancellationToken` — гравець або сервіс може перервати рух у будь-який момент.

---

## Публічний API

### Інтерфейс `IMovementAnimationService`

```csharp
namespace Kruty1918.Moyva.Animations.API
{
    public interface IMovementAnimationService
    {
        // Асинхронно переміщує target по шляху path з заданими налаштуваннями.
        Task MoveAlongPathAsync(
            Transform                  target,
            IReadOnlyList<Vector2Int>  path,
            PathAnimationSettings      settings,
            CancellationToken          cancellationToken = default);
    }
}
```

### Структура `PathAnimationSettings`

```csharp
[Serializable]
public struct PathAnimationSettings
{
    // Час (секунди) руху між двома сусідніми тайлами
    public float MoveDurationPerTile;

    // Затримка (секунди) після досягнення тайлу перед наступним кроком
    public float DelayOnTile;

    // Викликається після завершення кожного кроку (передає позицію нового тайлу)
    public Action<Vector2Int> OnStepCompleted;

    // Перевірка перед кроком: false → зупиняємо рух
    public Func<Vector2Int, bool> CanPerformStep;

    // Зручний конструктор за замовчуванням
    public static PathAnimationSettings Default => new PathAnimationSettings
    {
        MoveDurationPerTile = 0.3f,
        DelayOnTile         = 0.05f
    };
}
```

---

## Вхід / Вихід

| Метод | Вхід | Вихід |
|---|---|---|
| `MoveAlongPathAsync` | `Transform`, `IReadOnlyList<Vector2Int>`, `PathAnimationSettings`, `CancellationToken` | `Task` (завершується після останнього кроку або відміни) |

---

## Залежності

Система **не має зовнішніх залежностей** від інших модулів Moyva. Вона працює лише з вбудованими типами Unity (`Transform`, `Vector2Int`, `Time.deltaTime`).

---

## Реєстрація в Zenject (`AnimationsInstaller`)

```csharp
public class AnimationsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IMovementAnimationService>()
            .To<MovementAnimationService>()
            .AsSingle();
    }
}
```

---

## Приклади використання

### Базове використання

```csharp
[Inject] private IMovementAnimationService _animationService;

var settings = new PathAnimationSettings
{
    MoveDurationPerTile = 0.25f,
    DelayOnTile         = 0.05f,
    OnStepCompleted     = pos => Debug.Log($"Крок завершено: {pos}"),
    CanPerformStep      = pos => true  // завжди дозволяємо
};

await _animationService.MoveAlongPathAsync(unitTransform, path, settings, cancellationToken);
```

### Реальний приклад — `UnitMovementService.cs`

```csharp
var settings = config.AnimationSettings; // PathAnimationSettings з UnitClassConfig SO

// Делегат: перед кожним кроком перевіряємо, чи тайл не зайнятий
settings.CanPerformStep = nextPos =>
    !_objectsMapService.IsOccupied(nextPos) || nextPos == targetPosition;

// Делегат: після кожного кроку надсилаємо сигнал для списання стаміни
settings.OnStepCompleted = nextPos =>
{
    float cost = _tileSettings.GetTileWeight(_gridService.GetTileData(nextPos).TileTypeId);
    _signalBus.Fire(new UnitMovedSignal
    {
        UnitId      = unitId,
        NewPosition = nextPos,
        Cost        = cost
    });
};

await _animationService.MoveAlongPathAsync(
    unitObj.transform, path, settings, linkedCts.Token);
```

### Зупинка анімації ззовні

```csharp
var cts = new CancellationTokenSource();

// Запускаємо анімацію
var task = _animationService.MoveAlongPathAsync(t, path, settings, cts.Token);

// Зупиняємо через 1 секунду
await Task.Delay(1000);
cts.Cancel();
```

---

## Пов'язані системи

- [Units](units.md) — `UnitMovementService` викликає анімацію
- [Signals](signals.md) — `UnitMovedSignal` надсилається з `OnStepCompleted`
- [Pathfinding](pathfinding.md) — будує шлях, який передається до анімації
