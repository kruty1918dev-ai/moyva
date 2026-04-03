# ScreenToGrid — Конвертація координат

← [Назад до Construction](../construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/screen-to-grid)

---

## Призначення

`ScreenToGridConverter` реалізує `IScreenToGridConverter` — відповідає виключно за **перетворення координат** між трьома просторами:

- **Screen space** (пікселі екрану) → **World space** → **Grid space** (цілочисельні тайл-координати).

Камера є **ортографічною**, тому Z-компонента ігнорується. Камера ін'єктується через DI (а не береться через `Camera.main` статично).

---

## Публічний API

### `IScreenToGridConverter`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    public interface IScreenToGridConverter
    {
        /// <summary>
        /// Перетворити позицію на екрані (пікселі) в координати тайлу на сітці.
        /// Використовує ін'єктовану ортографічну камеру (Z ігнорується).
        /// </summary>
        Vector2Int ScreenToGrid(Vector2 screenPosition);

        /// <summary>
        /// Перетворити вже отриману world-позицію в координати тайлу.
        /// Зручно при drag, коли world-координата вже є.
        /// </summary>
        Vector2Int WorldToGrid(Vector2 worldPosition);
    }
}
```

---

## Реалізація

```csharp
internal sealed class ScreenToGridConverter : IScreenToGridConverter
{
    private readonly Camera _camera;

    [Inject]
    public ScreenToGridConverter(Camera camera)
    {
        _camera = camera;
    }

    public Vector2Int ScreenToGrid(Vector2 screenPosition)
    {
        // Ортографічна камера: Z не впливає на проекцію
        Vector3 worldPos = _camera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, 0f));
        return WorldToGrid(new Vector2(worldPos.x, worldPos.y));
    }

    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.y));
    }
}
```

---

## Реєстрація в Zenject

```csharp
// ConstructionInstaller.cs
Container.Bind<IScreenToGridConverter>()
    .To<ScreenToGridConverter>()
    .AsSingle();

// Ін'єкція Camera через тег або explicit binding:
Container.Bind<Camera>()
    .FromComponentInNewPrefab(cameraPrefab)
    .AsSingle();
// АБО якщо камера вже є в сцені:
Container.Bind<Camera>()
    .FromInstance(Camera.main)
    .AsSingle();
```

> Уникайте `Camera.main` напряму в методах — беріть камеру через DI для тестовості.

---

## Примітки

| Пункт | Деталь |
|---|---|
| Ортографічна проекція | Z = 0 при `ScreenToWorldPoint` для 2D |
| Округлення | `Mathf.RoundToInt` — стандарт для тайлових координат у проєкті |
| Ін'єкція камери | Через конструктор `[Inject]`, не `Camera.main` в коді |

---

## Пов'язані системи

- [Construction (огляд)](../construction.md)
- [wall-placement.md](wall-placement.md)
- [service.md](service.md)
