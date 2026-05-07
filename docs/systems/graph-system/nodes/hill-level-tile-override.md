# Hill Level Tile Override

← [Терейн](../nodes-terrain.md) · [Graph System](../README.md)

**Категорія:** Terrain · **Файл:** `HillLevelTileOverrideNode.cs`

Нода для пост-обробки `HillLevelData`: бере конкретний рівень із `Hill Generator` і перевизначає саме ті hill-тайли, які генератор поставив на цьому рівні.

---

## Порти

| Напрямок | Назва | Тип | Опис |
|---|---|---|---|
| **Input** 🟣 | HillLevelData | `HillLevelDataMap` | Дані від `Hill Generator` |
| **Output** 🟣 | HillLevelData | `HillLevelDataMap` | Оновлені дані після заміни |

## Параметри

| Параметр | Тип | Опис |
|---|---|---|
| **Target Level** | `int` | Який рівень у `HillLevelData.Level` обробляти |
| **Only Modified Tiles** | `bool` | Чіпати тільки клітинки, де `Hill Generator` реально поставив hill-тайл |
| **Replacement Tiles** | `HillTileEntry[]` | Заміна по напрямках (`North`, `South`, `CornerNE`, `InnerCornerSW` тощо) |
| **Changed Highlight Color** | `Color` | Колір підсвітки змінених клітинок у preview |

## Важлива деталь

Нода не вгадує напрямок по `TileId`. Вона використовує `DirectionId`, який записав `Hill Generator` у `HillLevelData`. Тому заміна залишається сумісною з логікою генератора і працює саме "по алгоритму hill generator", а не через простий текстовий replace.

## Preview

- Рівні відображаються базовою палітрою `Hill Generator`.
- Клітинки, які ця нода змінила, накриваються окремим highlight-кольором.

## Приклад сценарію

1. `Hill Generator` формує краї пагорбів і виводить `HillLevelData`.
2. `Hill Level Tile Override` приймає цей вихід.
3. Ви ставите `Target Level = 2`.
4. У `Replacement Tiles` задаєте новий набір тайлів для `North`, `South`, `CornerNE` тощо.
5. На виході отримуєте новий `HillLevelDataMap`, де тільки тайли рівня 2 замінені.

## Коли використовувати

- Коли геометрія hill-країв уже правильна, але для окремого рівня потрібен інший візуальний сет.
- Коли треба стилізувати лише один рівень схилів без повторного запуску всього `Hill Generator`.
- Коли потрібно мати кілька послідовних override-кроків поверх одного `HillLevelData`.