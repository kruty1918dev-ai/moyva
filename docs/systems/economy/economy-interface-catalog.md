# Каталог Інтерфейсів Economy

## Призначення
Стисла карта інтерфейсів, через які модуль Economy взаємодіє з іншими модулями або надає дані назовні.

## Публічні Інтерфейси Economy (Для Інтеграції)

## 1) IEconomyRuntimeApi
- Де: `Assets/Moyva/Scripts/Features/Economy/Runtime/IEconomyRuntimeApi.cs`
- Хто реалізує: `EconomyRuntimeApi`
- Задачі:
- повертати settlement-списки для owner;
- повертати сумарні ресурси owner/settlement;
- повертати форматовані рядки для HUD/UI.
- Коли використовувати:
- HUD, summary-блоки, bot/multiplayer агрегації, аналітичні панелі.

## 2) IEconomyInfoMediator
- Де: `Assets/Moyva/Scripts/Features/Signals/API/IEconomyInfoMediator.cs`
- Хто реалізує: `EconomyInfoMediator`
- Задачі:
- отримати settlement context по позиції;
- отримати building context по позиції;
- отримати ресурси складу, поселення або owner.
- Коли використовувати:
- інфо-панелі по клітинці/об'єкту, контекстні підказки, дебаг-інструменти.

## Зовнішні Інтерфейси, Які Economy Споживає

## 3) ICalendarService
- Де: `Assets/Moyva/Scripts/Features/Calendar/...`
- Хто споживає: `EconomyManager`
- Задачі:
- дати tick-тригер через `OnHourChanged`;
- забезпечити ритм симуляції економіки.

## 4) IBuildingRegistry
- Де: `Assets/Moyva/Scripts/Features/Construction/API/...`
- Хто споживає: `EconomyManager`
- Задачі:
- шукати `BuildingDefinition` за `buildingId`;
- класифікувати будівлі (TownHall/Housing/Warehouse/Production).

## 5) IConstructionService (опційно)
- Де: `Assets/Moyva/Scripts/Features/Construction/API/...`
- Хто споживає: `EconomyPlayerResourceSummaryUIController`
- Задачі:
- визначати активного owner (`GetActiveOwner`) для owner-scoped UI.

## Подієва Шина (Не Інтерфейс, Але Ключовий Контракт)
- `SignalBus`:
- Economy підписується на `BuildingPlacedSignal` і `BuildingDemolishedSignal`;
- Economy публікує `EconomyTickCompletedSignal`, `SettlementCreatedSignal`, `SettlementDeactivatedSignal`, `SettlementResourceChangedSignal`, `ResourceDeficitSignal`.

## Коротка Мапа Взаємодії
1. `ICalendarService` -> `EconomyManager` -> tick.
2. `BuildingPlacedSignal/BuildingDemolishedSignal` -> `EconomyManager` -> стан поселень.
3. `EconomyManager` -> `IEconomyRuntimeApi`/`IEconomyInfoMediator` -> UI/інші системи.
4. `IConstructionService` -> UI-контролер Economy -> owner scope.

## Пов'язані Сторінки
- [Economy API Tab](economy-api-tab.md)
- [Top 100 Питань По Economy](economy-top-100-qa.md)
