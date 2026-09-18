# Unit Designer Pre-Save Validation

Мета: ловити критичні помилки до `ApplyModifiedProperties`, щоб не зберігати пошкоджений стан registry.

## Що перевіряється перед apply

Критичний pre-save gate перевіряє:

1. `TypeId` не порожній.
2. `TypeId` не містить `_`.
3. `TypeId` унікальний в межах `UnitRegistrySO`.
4. `BaseStamina > 0`.
5. `VisionRange >= 1`.
6. Combat-блок валідний (`HP`, `BaseLevel`, базові бойові інваріанти).

## Режими застосування

1. Blocking mode:
- Використовується для explicit операцій (safe edit apply, batch apply, delete/duplicate/create prefab).
- Якщо перевірка не пройдена, операція скасовується і показується причина.

2. Non-blocking mode:
- Використовується для auto UI apply в `OnGUI`.
- Валідація виконується, але без modal-переривання потоку редагування.
- Статус-bar показує `validation: BLOCKED` до усунення проблеми.

## Правило для нових bulk-операцій

Кожна нова масова або руйнівна дія в Unit Designer має комітити зміни тільки через централізований метод pre-save валідації, а не через прямий виклик `ApplyModifiedProperties`.
