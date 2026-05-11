# 0016 Unit Designer Safe Edit Mode

- Status: Accepted
- Date: 2026-05-11

## Context

Unit Designer містить mass-операції (масове створення/оновлення/видалення), які в legacy-потоці виконувались одразу після натискання кнопки.

Це збільшувало ризик випадкової деградації даних registry, особливо для дизайнерських сценаріїв із великою кількістю юнітів.

## Decision

Додати глобальний Safe Edit Mode для Unit Designer:

1. Увімкнений режим переводить mass-операції в preview-before-apply flow.
2. Операція виконується лише після явного підтвердження (`Застосувати`).
3. Є єдина pending preview-панель з можливістю `Скасувати`.
4. Стан режиму зберігається у `EditorPrefs`.

Покриті операції на першому етапі:
- Apply all combat presets.
- Auto-generate animation set.
- Clear all animations.

## Consequences

Positive:
- Менше випадкових руйнівних змін у registry.
- Прозорий UX для масових правок.
- Єдина точка розширення для майбутніх bulk-дiй.

Trade-offs:
- Додатковий крок підтвердження для дизайнерів.
- Потрібно підтримувати актуальність preview-описів.

## Rollback / Alternative

Rollback:
- Вимкнути Safe Edit Mode та повернути direct-apply поведінку для mass-операцій.

Alternative:
- Окремі confirm-діалоги для кожної дії без єдиного pending-механізму.
