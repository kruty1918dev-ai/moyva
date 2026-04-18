# Economy API Files Reference

Нижче коротко пояснено призначення кожного файла в Assets/Moyva/Scripts/Features/Economy/API.

## EconomyAiRuleProfile.cs

- EconomyResourceThreshold: поріг для конкретного ресурсу (id + мін/макс).
- EconomyAiRuleProfile: профіль AI-правил (пороги нестачі/надлишку, авто-поведінка).

## EconomyCaravanTemplate.cs

- Шаблон каравану: дефолтні параметри караванної логіки і вантажності.

## EconomyDatabaseSO.cs

- Центральний каталог economy-ассетів: ресурси, поселення, склади, production, каравани, AI-профілі, rules.
- Фактична точка входу runtime/інструментів у дані.

## EconomyEnums.cs

- Базові enum-и домену:
- EconomyResourceCategory;
- EconomySettlementType;
- EconomyWarehouseType.

## EconomyProductionProfile.cs

- Конфіг виробничого циклу будівлі: recipe/input/output, тривалість, активність за замовчуванням.

## EconomyResourceDefinition.cs

- Опис ресурсу: id, назва, категорія, візуальні/службові метадані.

## EconomyRuleParameter.cs

- EconomyRuleParameter: універсальна модель параметра правила (id, тип, межі, дефолт, overridable).
- EconomyRuleCategory: категорії правил (Settlement, Population, Market тощо).
- EconomyRuleParameterType: типи значень (int/float/bool/string).

## EconomyRulesConfigSO.cs

- Структурований runtime-конфіг правил по групах.
- Містить класи груп:
- EconomySettlementRules
- EconomyPopulationRules
- EconomyWorkforceRules
- EconomyProductionRules
- EconomyStorageRules
- EconomyCaravanRules
- EconomyMarketRules
- EconomyConsumptionRules
- EconomyMortalityRules
- EconomyBuildingRules
- EconomyAiExtensibilityRules

## EconomyRulesConfiguration.cs

- Централізований шаблон параметрів Economy Hub.
- Містить список EconomyRuleParameter і дефолтну ініціалізацію параметрів.
- Містить параметри форматування UI:
- ui-summary-materials-format
- ui-summary-food-format

## EconomySettlementDefinition.cs

- Конфіг поселення: ідентифікатор, тип, прив'язка до центральної будівлі, радіус та інші властивості.

## EconomyWarehousePolicy.cs

- EconomyWarehousePolicyEntry: правило для окремого ресурсу в складі (резерв, пріоритет, доступність).
- EconomyWarehousePolicy: набір правил для типу складу/поселення.

## WorldEconomyOverride.cs

- Runtime/world-рівень overrides для параметрів, які дозволено змінювати перед сесією.
- ParameterOverride: значення конкретного override-параметра.

## Як читати API пакет правильно

1. EconomyDatabaseSO + EconomyRulesConfigSO = основа runtime-читання.
2. EconomyRulesConfiguration = редакторний шаблон параметрів і метаданих.
3. Решта ScriptableObject-файлів = доменні каталоги, які лінкуються в EconomyDatabaseSO.
