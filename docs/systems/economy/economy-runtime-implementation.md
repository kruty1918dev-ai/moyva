# Economy Runtime Implementation

## Огляд
Після затвердження економічного плану реалізовано базовий runtime-шар і розширено `Economy Designer` до формату `Economy Hub`.

Цілі реалізації:
- прибрати критичний хардкод економічних формул;
- зібрати ключові правила в єдиному конфіг-асеті;
- дати runtime-сервісам стабільні точки входу для розрахунків;
- покрити базову логіку тестами EditMode.

## Що реалізовано

### 1. Єдиний конфіг Economy Hub
Додано `EconomyRulesConfigSO` з групами налаштувань:
- Settlement
- Population
- Workforce
- Production
- Storage
- Caravan
- Market
- Consumption
- Mortality
- Building
- AI Extensibility

Також додано reference на rules-конфіг у `EconomyDatabaseSO`.

### 2. Runtime сервіси
Додані runtime-класи:
- `EconomyMarketPricingService`
- `EconomyComfortAndMortalityService`
- `EconomyConsumptionService`
- `EconomySettlementLifecycleService`

Додані runtime-моделі:
- `EconomyResidentState`
- `EconomyNeedSnapshot`
- `EconomyComfortInput`
- `EconomyConsumptionProfile`

### 3. Формули з плану як конфігуровані дефолти
У rules asset закладені стартові пресети:
- `MinTownHallDistance = 25`
- `NewResidentsArrivalIntervalTurns = 10`
- ринкові коефіцієнти (`stockExponent`, `volumeExponent`, min/max multiplier)
- базові ціни ресурсів (Wood, Stone, Grain, Meat, Berries, Water, Coal, Clothing, Planks, Tools)
- age-based consumption (Child/Adult/Elder)
- mortality tiers і ваги ризиків
- comfort weights

## Economy Hub (Editor)
`EconomyDesignerWindow` отримав нову вкладку `Rules Hub`:
- призначення/створення `EconomyRulesConfigSO` прямо з вікна;
- інспектування всіх rule-груп з одного місця;
- збереження конфіга у базі `EconomyDatabaseSO`.

Валідація і автофікси розширені для rules-конфіга:
- перевірки критичних діапазонів (інтервали, ліміти, multipliers);
- safe-clamp автоправки для основних числових полів.

## Тести
Додано EditMode тести:
- `EconomyMarketPricingServiceTests`
- `EconomyComfortAndMortalityServiceTests`
- `EconomyConsumptionServiceTests`
- `EconomySettlementLifecycleServiceTests`

Поточні тести перевіряють:
- реакцію ціни на дефіцит запасу;
- коректність формули комфорту;
- миттєву смертність при колапсі будинку;
- вибір вікового профілю споживання;
- деактивацію поселення при 0 населення;
- ліміт створення поселень.

## Обмеження поточної реалізації
- Повна бойова поведінка перехоплення караванів поки залишається заглушкою на рівні правил.
- Інтеграція з фактичними world-сервісами (unit movement, combat resolution, save modules) потребує наступного етапу.
- Калібрування дефолтних коефіцієнтів очікує перших плейтестів.

## Рекомендований наступний етап
1. Підключити runtime-сервіси до game loop (tick update).
2. Додати save/load модуль для стану runtime-економіки.
3. Дотягнути UI-шари (household list, resident card, event feed).
4. Провести плейтести і підкрутити пресети в Economy Hub.
