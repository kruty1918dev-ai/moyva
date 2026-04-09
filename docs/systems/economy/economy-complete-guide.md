# Economy Complete Guide

## 1. Загальна архітектура

Економічний модуль має 3 шари:
- API: типи даних і ScriptableObject-активи;
- Runtime: версія схеми даних;
- Editor: інструмент налаштування, валідація, автоправки, simulation preview, міграція.

Основна точка входу даних: `EconomyDatabaseSO`.

## 2. Швидка карта файлів

### API
- Assets/Moyva/Scripts/Features/Economy/API/EconomyEnums.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyResourceDefinition.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomySettlementDefinition.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyWarehousePolicy.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyProductionProfile.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyCaravanTemplate.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyAiRuleProfile.cs
- Assets/Moyva/Scripts/Features/Economy/API/EconomyDatabaseSO.cs

### Runtime
- Assets/Moyva/Scripts/Features/Economy/Runtime/EconomySchema.cs

### Editor
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomyDesignerWindow.cs
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomyValidationIssue.cs
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomyValidationService.cs
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomyAutoFixService.cs
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomySimulationService.cs
- Assets/Moyva/Scripts/Features/Economy/Editor/EconomyDataMigrationService.cs

## 3. Дані та контракти (повний розбір)

### 3.1 EconomyEnums

#### EconomyResourceCategory
- None: ресурс не категоризований.
- Food: харчові ресурси.
- Materials: виробничі/будівельні ресурси.

#### EconomySettlementType
- Village
- Castle

#### EconomyWarehouseType
- FoodWarehouse
- MaterialsWarehouse

Контракт:
- `None` дозволений у даних, але валідація позначає це warning.

### 3.2 EconomyResourceDefinition

Публічні властивості:
- `Id`: стабільний ідентифікатор ресурсу.
- `DisplayName`: назва для UI/дизайну.
- `Category`: категорія ресурсу.
- `Icon`: візуальне представлення.
- `StackLimit`: ліміт стеку (editor-конфіг, runtime-поведінка поки не підключена).

Контракт:
- `Id` має бути унікальним серед усіх ресурсів;
- `Category` бажано не `None`;
- рядки бажано без пробілів по краях (автофікс це виправляє).

### 3.3 EconomySettlementDefinition

Публічні властивості:
- `SettlementId`: унікальний ідентифікатор поселення.
- `SettlementType`: Village/Castle.
- `CenterBuildingId`: ідентифікатор головної будівлі.
- `BuildRadius`: радіус будівництва.

Контракт:
- `SettlementId` унікальний;
- `CenterBuildingId` не порожній;
- `BuildRadius > 0`.

### 3.4 EconomyWarehousePolicyEntry

Публічні властивості:
- `ResourceId`
- `ConsumptionAllowed`
- `Priority`
- `ReserveAmount`

Контракт:
- `ResourceId` має посилатися на існуючий ресурс;
- `Priority` рекомендовано >= 1;
- `ReserveAmount` має бути логічно узгоджений з вашими балансними цілями.

### 3.5 EconomyWarehousePolicy

Публічні властивості:
- `WarehouseType`
- `Entries` (read-only колекція для зовнішнього споживача)

Контракт:
- `Entries` не повинні містити порожні `ResourceId`;
- кожен `ResourceId` має бути відомим у `EconomyDatabaseSO.Resources`.

### 3.6 EconomyProductionProfile

Публічні властивості:
- `BuildingId`
- `IsActiveByDefault`
- `RecipeId`
- `CycleDurationSeconds`
- `OutputAmountPerCycle`

Контракт:
- `BuildingId` обов'язковий;
- `RecipeId` обов'язковий;
- `CycleDurationSeconds > 0`;
- `OutputAmountPerCycle > 0`.

Нотатка:
- у simulation preview `RecipeId` інтерпретується як ключ ресурсу output.

### 3.7 EconomyCaravanTemplate

Публічні властивості:
- `TemplateId`
- `AllowedResourceIds`
- `Capacity`
- `DefaultPriority`
- `UseLoopDelivery`

Контракт:
- `TemplateId` обов'язковий;
- `Capacity > 0`;
- `DefaultPriority` рекомендовано >= 1.

### 3.8 EconomyResourceThreshold

Публічні властивості:
- `ResourceId`
- `ShortageThreshold`
- `ExcessThreshold`

Семантика:
- `ShortageThreshold`: нижній поріг дефіциту;
- `ExcessThreshold`: верхній поріг надлишку.

### 3.9 EconomyAiRuleProfile

Публічні властивості:
- `ProfileId`
- `ResourceThresholds`
- `UseConservativeSpending`
- `PrioritizeFoodSecurity`

Контракт:
- `ProfileId` має бути стабільним і ненульовим;
- `ResourceThresholds` рекомендовано задавати для ключових ресурсів.

### 3.10 EconomyDatabaseSO

Публічні властивості:
- `SchemaVersion` (get/set)
- `Resources`
- `Settlements`
- `WarehousePolicies`
- `ProductionProfiles`
- `CaravanTemplates`
- `AiRuleProfiles`

Контракт:
- це єдина агрегуюча точка для інструмента;
- `SchemaVersion` має бути сумісний з `EconomySchema.CurrentVersion`.

### 3.11 EconomySchema

Константи:
- `InitialVersion = 1`
- `CurrentVersion = 2`

Призначення:
- централізація версії схеми для міграцій.

## 4. Editor Services: методи і призначення

### 4.1 EconomyValidationService

Метод:
- `Validate(EconomyDatabaseSO database) -> IReadOnlyList<EconomyValidationIssue>`

Що робить:
- перевіряє обов'язкові ID;
- перевіряє дублікатні IDs;
- перевіряє посилання на невідомі ресурси;
- перевіряє числові межі (радіус, цикл, output, capacity).

Результат:
- список warning/error з контекстним Unity Object.

### 4.2 EconomyAutoFixService

Метод:
- `FixCommonIssues(EconomyDatabaseSO database) -> int`

Що робить:
- trim для строкових полів (`Id`, `DisplayName`, `TemplateId`, тощо);
- clamp для числових мінімумів (`BuildRadius`, `Priority`, `Capacity`, `CycleDurationSeconds`, `OutputAmountPerCycle`).

Результат:
- повертає число застосованих правок.

### 4.3 EconomySimulationService

Моделі:
- `EconomySimulationInput`
- `EconomySimulationResult`

Метод:
- `Simulate(EconomySimulationInput input) -> EconomySimulationResult`

Алгоритм:
1. Нормалізує тривалість (`DurationMinutes >= 0`).
2. Відбирає не-null профілі.
3. Сортує профілі детерміновано (BuildingId, RecipeId).
4. Для кожного профілю рахує `cycleCount = floor(durationSeconds / cycleDurationSeconds)`.
5. Агрегує `delta = cycleCount * outputAmount` у словник ресурсів.

Результат:
- таблиця сумарних дельт ресурсів;
- детальний лог внеску кожного профілю.

### 4.4 EconomyDataMigrationService

Модель:
- `EconomyMigrationReport`:
  - `FromVersion`
  - `ToVersion`
  - `Changed`
  - `Steps`

Метод:
- `Migrate(EconomyDatabaseSO database) -> EconomyMigrationReport`

Правила:
- null database: крок "migration skipped";
- невалідна версія (`<=0`): нормалізується до `InitialVersion`;
- підтриманий перехід `1 -> 2`: no-op міграція з оновленням маркера;
- якщо версія менша за current без шляху: форс до current з повідомленням.

## 5. Economy Designer Window: інтерфейс і поведінка

Клас: `EconomyDesignerWindow`

Меню:
- `Moyva/Tools/Economy Designer`

Вкладки:
- Settlements
- Resources
- Warehouses
- Production
- Caravans
- AI Rules
- Validation
- Simulation

### 5.1 Шапка
- Пояснює призначення тулзи;
- Працює як onboarding для нових учасників.

### 5.2 Блок Economy Database
- ObjectField для `EconomyDatabaseSO`;
- `Auto Find`: шукає перший актив за типом;
- `Create Database Asset`: створює новий database asset;
- `Ping`: швидка навігація до активу.

### 5.3 Entity Tabs (спільний патерн)
Кожна вкладка сутностей має:
- ліву панель списку + пошук;
- кнопки `Create`, `Add Selected`, `Remove`;
- праву панель інспектора вибраного asset.

### 5.4 Validation Tab
Кнопки:
- `Run Validation`
- `Fix Common Issues`
- `Run Migration`

Вивід:
- список issues з severity і кнопкою `Ping` на контекст.

Schema panel:
- поточна підтримувана версія;
- версія бази;
- кроки останньої міграції.

### 5.5 Simulation Tab
Керування:
- вибір settlement;
- duration у хвилинах;
- кнопки `Select Defaults`, `Select All`, `Clear`;
- чекліст production profiles.

Запуск:
- `Run Deterministic Preview`.

Вивід:
- settlement/duration;
- підсумок ресурсів;
- покроковий лог.

## 6. ASMDEF і межі модуля

- `Kruty1918.Moyva.Economy.asmdef`
- `Kruty1918.Moyva.Economy.Editor.asmdef`
- `Kruty1918.Moyva.Tests.Economy.asmdef`

Рекомендації:
- API-моделі тримати незалежними від Editor;
- Editor-сервіси не тягнути в Runtime;
- нові runtime-сервіси економіки додавати в `Runtime` або окремі feature-підмодулі.

## 7. Повна послідовність роботи (операційний стандарт)

1. Створити/вибрати `EconomyDatabaseSO`.
2. Заповнити Resources.
3. Заповнити Settlements.
4. Налаштувати Warehouses.
5. Налаштувати Production.
6. Налаштувати Caravans.
7. Налаштувати AI Rules.
8. Запустити Validation.
9. За потреби виконати Fix Common Issues.
10. Запустити Migration (після змін схеми або міграційного оновлення).
11. Запустити Simulation Preview для sanity-check.
12. Комітити лише релевантні assets + документацію.

## 8. Поширені помилки і діагностика

1. Помилка: `EconomyDatabase is not assigned`.
- Причина: не вибрано database asset.
- Рішення: призначити asset через ObjectField або натиснути Auto Find.

2. Дублікат `Id` у Resources/Settlements.
- Причина: ручне копіювання assets без перейменування ідентифікатора.
- Рішення: унікалізувати `Id`, повторно прогнати Validation.

3. Unknown resource у WarehousePolicy.
- Причина: `ResourceId` відсутній у `Resources`.
- Рішення: створити ресурс або виправити посилання.

4. Simulation не показує output.
- Причина: нульова тривалість або цикл/output <= 0 або порожній `RecipeId`.
- Рішення: перевірити профілі, duration, і прогнати Validation.

## 9. План безпечного розширення

1. Додати нову версію схеми в `EconomySchema`.
2. Додати migration-step в `EconomyDataMigrationService`.
3. Оновити валідацію під нові поля.
4. Оновити auto-fix лише для безпечних, детермінованих правок.
5. Додати EditMode тести для нового контракту.
6. Оновити документацію цього розділу до merge.

## 10. Межі поточної реалізації

Що є:
- централізований editor-конфігуратор;
- валідація/автофікс;
- preview-симуляція;
- schema migration stub.

Чого немає (ще):
- runtime-споживання/виробництво у грі;
- runtime-рух караванів;
- інтеграція AI rule profile у бойовий/gameplay AI runtime.
