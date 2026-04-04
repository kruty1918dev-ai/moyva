# Overlay

← [Утиліти](../nodes-utility.md) · [Graph System](../README.md)

**Категорія:** Utility · **Файл:** `OverlayNode.cs`

Накладає **нову карту тайлів поверх базової**, використовуючи маску для вибору зон.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🔵 | BaseMap | `string[,]` | Базова карта тайлів |
| **Input** 🔵 | OverlayMap | `string[,]` | Карта для накладання |
| **Input** 🟡 | Mask | `bool[,]` | Маска: де true → використовуємо OverlayMap |
| **Output** 🔵 | ResultMap | `string[,]` | Результат |

## Формула

```
result[x,y] = mask[x,y] ? overlayMap[x,y] : baseMap[x,y]
```

## Приклад

```
BaseMap:      OverlayMap:     Mask:              Результат:
grass grass   water water    false false         grass grass
grass grass   water water    false true      →   grass water
grass grass   water water    true  true          water water
```
