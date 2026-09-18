# Unit Designer Batch Operations

Мета: прискорити масове редагування контенту через групові дії над юнітами у Unit Designer.

## Покриті batch-дії

1. Масова зміна `Role`.
2. Масова зміна `Prefab` reference.
3. Масове застосування animation defaults (`MoveDurationPerTile`, `DelayOnTile`).

## Таргет групи

Batch-операції можна застосувати до:

1. Лише юнітів, які пройшли поточний фільтр (search/role/problem).
2. Усіх юнітів у registry.

## UX-флоу

1. Designer обирає, які batch-дії активні.
2. Designer обирає target group.
3. Натискає `Застосувати batch`.
4. Якщо Safe Edit Mode увімкнений — операція йде через preview-before-apply.
5. Коміт проходить через pre-save validation gate.

## Гарантії безпеки

1. Batch-коміт не обходить централізовану pre-save валідацію.
2. У safe mode масові дії показують preview рядки для перевірки перед підтвердженням.
3. У випадку критичної валідаційної помилки apply блокується до виправлення даних.

## Правило розширення

Нова batch-дія в Unit Designer має:
1. додавати preview-опис змін;
2. застосовуватись через централізований commit path;
3. бути сумісною з Safe Edit Mode.
