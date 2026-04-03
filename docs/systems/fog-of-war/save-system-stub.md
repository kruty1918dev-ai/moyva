# Fog of War — інтеграція з SaveSystem

← [README](README.md)

---

> **Статус (2026):** інтеграція виконана.
> Fog of War зберігається через `FogOfWarSaveModule` (`ISaveModule`) у `.mvs` блок SaveSystem.
> Деталі системи: [docs/systems/save-system.md](../save-system.md).

---

## Що реалізовано

Fog of War зберігає explored state карти в окремому блоці SaveSystem:

- `FogOfWarSaveModule.OnSave(...)`
    - читає `bool[,]` snapshot через `IFogOfWarService.GetExploredSnapshot()`
    - записує `width`, `height`, усі клітинки `explored`

- `FogOfWarSaveModule.OnLoad(...)`
    - читає `width`, `height`, масив explored
    - викликає `IFogOfWarService.LoadFromSnapshot(...)`

Binding в installer:

```csharp
Container.Bind<ISaveModule>()
        .To<FogOfWarSaveModule>()
        .AsSingle();
```

---

## Що зберігається

`bool[,] _exploredTiles` — масив розміром `mapWidth × mapHeight`. Кожен елемент `true` означає, що цей тайл колись бачили.

**Що НЕ зберігається:**
- `int[,] _visibilityCounters` — при завантаженні гри юніти не розставлені, тому лічильники завжди стартують з 0
- Позиції юнітів — це відповідальність системи юнітів

---

## Розмір даних

Для карти 100×100 = 10 000 булів = ~1.25 KB (якщо упакований як bits) або ~10 KB (якщо byte per bool). Прийнятно.

---

## Важливий сценарій ініціалізації

У проекті можливий ранній `Load` (до ініціалізації карти FogOfWar).

Щоб дані не губились, `FogOfWarService` має pending snapshot буфер:

1. `LoadFromSnapshot(...)` до `Initialize(width,height)` → дані кладуться у pending
2. Під час `Initialize(width,height)` pending snapshot застосовується автоматично

Це дозволяє коректно відновлювати fog state навіть у bootstrap-порядку, де save може завантажитись раніше за map/fog runtime init.
