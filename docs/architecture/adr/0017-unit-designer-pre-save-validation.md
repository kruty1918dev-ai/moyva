# 0017 Unit Designer Pre-Save Validation

- Status: Accepted
- Date: 2026-05-11

## Context

Unit Designer мав кілька шляхів, де зміни серіалізованих даних комітились напряму через `ApplyModifiedProperties` без єдиного pre-save gate.

У результаті критичні помилки могли потрапляти в asset раніше, ніж користувач помічав проблему.

## Decision

Додати централізований pre-save validation gate перед apply-комітом у Unit Designer:

1. Критичні інваріанти перевіряються централізовано перед комітом.
2. Explicit/bulk операції працюють у blocking-режимі (операція скасовується при помилці).
3. Auto UI apply у `OnGUI` працює в non-blocking режимі, щоб не ламати UX редагування.
4. Статус-bar показує, коли останній apply був заблокований валідацією.

## Consequences

Positive:
- Помилки в критичних полях фіксуються до збереження asset.
- Масові операції не записують пошкоджений стан.
- Єдина точка розширення валідації на майбутні поля.

Trade-offs:
- Додатковий шар коміту й валідації.
- Потрібно підтримувати актуальність критичних правил при еволюції UnitConfig.

## Rollback / Alternative

Rollback:
- Повернути прямі виклики `ApplyModifiedProperties` без централізованого pre-save gate.

Alternative:
- Повністю blocking-поведінка для всіх apply-шляхів, включно з `OnGUI` (відхилено через ризик ламання UX редагування).
