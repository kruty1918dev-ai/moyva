# Designer Preset System

## Мета

Єдина preset-система для дизайнерських вікон повинна зменшити ручне дублювання налаштувань між юнітами, будівлями, fog і economy.

## Базовий підхід

- Джерело preset-ів: `DesignerPresetLibrarySO`.
- Спільні типи preset-ів:
  - `UnitDesignerPreset`
  - `BuildingDesignerPreset`
  - `FogDesignerPreset`
  - `EconomyDesignerPreset`
- Застосування preset-ів виконується через `DesignerPresetApplier`.

## Правила застосування

- Unit preset:
  - ціль: вибраний `UnitClassConfig` у `UnitDesignerWindow`;
  - `TypeId` зберігається, щоб не ламати посилання та інстанси.
- Building preset:
  - ціль: вибраний `BuildingDefinition` у `BuildingDesignerWindow`;
  - `Id` зберігається як стабільний ключ registry.
- Fog preset:
  - ціль: поточний `FogOfWarSettings` у `FogVisionTuningWindow`;
  - копіюються всі серіалізовані поля з template asset.
- Economy preset:
  - ціль: поточний `EconomyDatabaseSO` у `EconomyDesignerWindow`;
  - копіюються всі серіалізовані поля з template asset.

## UX-вимоги

- Кожне дизайнерське вікно має мати блок `Designer Presets`.
- Якщо preset library не призначена або порожня, користувач отримує явний message.
- Перед застосуванням має використовуватися `Undo.RecordObject`.

## Обмеження MVP

- Preset-и застосовуються до однієї поточної цілі у вікні.
- Для Unit/Building збережено стабільні ID-поля (`TypeId`, `Id`) навіть при повному копіюванні полів.
