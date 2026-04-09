# WallPlacement — Сервіс розміщення стін

← [Назад до Construction](../construction.md) · [Переглянути на сайті →](https://kruty1918dev-ai.github.io/moyva/#systems/wall-placement)

---

## Призначення

`WallPlacementService` реалізує `IWallPlacementService` — окремий сервіс для побудови **стін**. Він надає два способи взаємодії:

1. **8 ручок** — після розміщення стіни навколо неї з'являються 8 точок-кнопок (по сторонах і діагоналях). Гравець натискає → стіна ставиться поряд.
2. **Drag** — гравець тримає одну з ручок і тягне. Стіни автоматично вибудовуються вздовж шляху за алгоритмом **Bresenham**.

`WallPlacementService` **не знає** про UI, він лише надає сигнали та делегує фактичне розміщення через `IConstructionService.TryPreviewAt()`.

---

## Публічний API

### `IWallPlacementService`

```csharp
namespace Kruty1918.Moyva.Construction.API
{
    public interface IWallPlacementService
    {
        /// <summary>
        /// Показати 8 ручок-кнопок навколо вже розміщеної стіни.
        /// Надсилає ShowWallHandlesSignal — UI підписується і малює кнопки.
        /// </summary>
        void ShowWallHandles(Vector2Int wallPosition);

        /// <summary>
        /// Drag від стартової позиції до поточної позиції дотику в світових координатах.
        /// Будує лінію стін за алгоритмом Bresenham між startPosition і
        /// відповідним grid-тайлом touchWorldPosition.
        /// Кожен тайл на шляху: IConstructionService.TryPreviewAt(tile).
        /// </summary>
        void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition);

        /// <summary>
        /// Завершити drag. Ручки зникають, pending-розміщення залишаються.
        /// </summary>
        void EndDrag();
    }
}
```

---

## Алгоритм Bresenham (drag)

```
DragWall(start, touchWorldPos):
  1. gridEnd = IScreenToGridConverter.WorldToGrid(touchWorldPos)
  2. Побудувати лінію Bresenham від start до gridEnd:
     while (x, y) != gridEnd:
         step x або y до gridEnd (за крутістю нахилу)
         IConstructionService.TryPreviewAt((x, y))
  3. Зафіксувати останні _dragTiles для EndDrag()
```

> Bresenham гарантує, що стіна тягнеться рівно вздовж шляху без прогалин і без повторень.

---

## Сигнал `ShowWallHandlesSignal`

```csharp
public struct ShowWallHandlesSignal
{
    public Vector2Int Center;    // Центральна позиція розміщеної стіни
    public bool Hide;            // true — приховати ручки (EndDrag або Cancel)
}
```

UI підписується на цей сигнал і малює / прибирає 8 кнопок навколо `Center`.

---

## Розташування 8 ручок

```
[(-1,+1)] [(0,+1)] [(+1,+1)]
[(-1, 0)] [ WALL ] [(+1, 0)]
[(-1,-1)] [(0,-1)] [(+1,-1)]
```

Кожна ручка — окрема кнопка в UI (або точка дотику). При натисканні передає відповідний offset до `IConstructionService.TryPreviewAt(wallPosition + offset)`.

---

## Залежності

| Залежність | Причина |
|---|---|
| [`IConstructionService`](service.md) | Делегує `TryPreviewAt()` для кожного тайлу на шляху |
| [`IScreenToGridConverter`](screen-to-grid.md) | Конвертація touchWorldPosition → grid-координати |
| [`SignalBus`](signals.md) | Надсилання `ShowWallHandlesSignal` |

---

## Пов'язані системи

- [Construction (огляд)](../construction.md)
- [service.md](service.md)
- [screen-to-grid.md](screen-to-grid.md)
- [resolver-editor.md](resolver-editor.md)
- [Signals](../signals.md)
