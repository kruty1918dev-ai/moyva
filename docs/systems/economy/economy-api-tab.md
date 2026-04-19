# Economy API Tab

## Призначення
Ця сторінка є короткою API-вкладкою для розробника: куди звертатися, щоб отримати дані з Economy, і який контракт за що відповідає.

## Головні Точки Входу
1. `IEconomyRuntimeApi`:
- агрегати ресурсів по owner;
- агрегати ресурсів по settlement;
- форматовані рядки для UI (Food/Materials).

2. `IEconomyInfoMediator`:
- контекст поселення/будівлі за `Vector2Int` позицією;
- ресурси складу за позицією;
- ресурси поселення або owner для інформаційних панелей.

## Коли Який API Використовувати
1. Потрібно показати суму ресурсів гравця в HUD:
- використовуйте `IEconomyRuntimeApi.GetFormattedOwnerCategoryTotals(ownerId)`.

2. Потрібно відкрити інфо-панель по тайлу/будівлі:
- використовуйте `IEconomyInfoMediator.TryGetSettlementContext(position, out context)`;
- і `IEconomyInfoMediator.TryGetBuildingContext(position, out buildingId, out ownerId)`.

3. Потрібен raw-перелік ресурсів без форматування:
- `IEconomyRuntimeApi.GetOwnerResourceTotals(ownerId)`;
- `IEconomyRuntimeApi.GetSettlementResourceTotals(settlementId)`.

## Мінімальні Приклади
```csharp
// HUD: агреговані значення для поточного owner
var totals = economyApi.GetFormattedOwnerCategoryTotals(ownerId);
materialsText.text = totals.MaterialsText;
foodText.text = totals.FoodText;
```

```csharp
// Інфо-панель: визначення контексту по клітинці
if (economyInfo.TryGetSettlementContext(position, out var ctx))
{
    var resources = economyInfo.GetSettlementResourceTotals(ctx.SettlementId);
    // побудувати список ресурсів у UI
}
```

## Де Біндиться В DI
- `EconomyInstaller` біндить `IEconomyRuntimeApi` і `IEconomyInfoMediator` як `AsSingle`.

## Пов'язана Документація
- [Каталог Інтерфейсів Economy](economy-interface-catalog.md)
- [Economy API Files Reference](economy-api-files-reference.md)
- [Economy Handbook](economy-handbook.md)
