# Economy Tutorials

## Швидкий стартовий туторіал

- Детальний сценарій додавання нового ресурсу: [Tutorial: Як додати новий ресурс](tutorial-add-resource.md)

## Tutorial 1: Перший запуск Economy Designer з нуля

Ціль: отримати валідну базу економіки без помилок.

Кроки:
1. Відкрийте `Moyva/Tools/Economy Designer`.
2. У полі `Economy Database` натисніть `Create Database Asset`.
3. Перейдіть у вкладку `Resources`.
4. Натисніть `Create` і створіть мінімум 4 ресурси: food, wood, stone, iron.
5. Для кожного ресурсу заповніть `Id`, `DisplayName`, `Category`.
6. Перейдіть у `Settlements`, створіть 2 assets: village_01, castle_01.
7. Заповніть `CenterBuildingId` і `BuildRadius` (>0).
8. У `Warehouses` створіть policy для FoodWarehouse і MaterialsWarehouse.
9. Додайте policy entries на ресурси з пріоритетами.
10. У `Production` створіть профілі для 2-3 будівель.
11. Натисніть `Validation -> Run Validation`.
12. Виправте всі Error, потім за потреби `Fix Common Issues`.

Результат:
- валідна база даних для наступних кроків балансу.

## Tutorial 2: Налаштування складів і ручних обмежень споживання

Ціль: зробити запас критичних ресурсів недоторканим.

Кроки:
1. Вкладка `Warehouses` -> оберіть MaterialsWarehouse policy.
2. Для дерева/каменю виставте:
- `ConsumptionAllowed = false` для аварійного резерву;
- `ReserveAmount` під ваші будівельні потреби.
3. Для ресурсів поточного споживання (напр., food у FoodWarehouse):
- `ConsumptionAllowed = true`.
4. Запустіть `Run Validation`.

Порада:
- для сценаріїв облоги створюйте окремий policy asset.

## Tutorial 3: Базовий production sanity-check через Simulation

Ціль: оцінити приріст ресурсів за 10 хв без запуску runtime.

Кроки:
1. Вкладка `Simulation`.
2. Оберіть settlement (опційно, для контексту звіту).
3. Встановіть `Duration (minutes)` = 10.
4. Натисніть `Select Defaults`.
5. Додайте вручну профілі, якщо потрібно.
6. Натисніть `Run Deterministic Preview`.
7. Перевірте `Estimated Resource Totals` і `Log`.

Як інтерпретувати:
- якщо ресурс не зростає, перевірте `RecipeId`, `CycleDurationSeconds`, `OutputAmountPerCycle`.

## Tutorial 4: Міграція схеми перед релізом

Ціль: уніфікувати версію економічних даних.

Кроки:
1. Вкладка `Validation`.
2. Натисніть `Run Migration`.
3. Перегляньте блок `Schema` і `Migration steps`.
4. Перезапустіть `Run Validation`.
5. Збережіть assets і закомітьте зміни.

## Tutorial 5: Розширення моделі даних без зламу пайплайна

Ціль: додати нове поле у профіль без втрати сумісності.

Кроки:
1. Додайте поле в потрібний ScriptableObject.
2. Підвищіть `EconomySchema.CurrentVersion`.
3. Реалізуйте migration-step у `EconomyDataMigrationService`.
4. Розширте `EconomyValidationService` під нове правило.
5. За потреби додайте auto-fix у `EconomyAutoFixService`.
6. Оновіть Simulation (якщо поле впливає на підрахунок).
7. Додайте/оновіть EditMode тести.
8. Оновіть документацію.

## Checklist перед PR

1. Всі Error у Validation усунуті.
2. Simulation дає очікувані результати.
3. Schema/Migration актуальні.
4. Документація оновлена.
5. Коміт не містить сторонніх, нерелевантних файлів.
