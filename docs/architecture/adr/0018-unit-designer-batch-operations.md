# 0018 Unit Designer Batch Operations

- Status: Accepted
- Date: 2026-05-11

## Context

Команда контенту часто виконує однотипні дії для групи юнітів (роль, prefab reference, animation defaults). Поштучне редагування сповільнює production flow і підвищує ризик ручних пропусків.

## Decision

Додати в Unit Designer вбудований batch-інструмент для масових змін групи юнітів.

1. Дії: Role, Prefab reference, Animation defaults.
2. Target group: filtered set або весь registry.
3. Batch-застосування підтримує Safe Edit preview-before-apply.
4. Batch-коміт виконується лише через pre-save validation gate.

## Consequences

Positive:
- Значно швидше масове редагування контенту.
- Менше ручних помилок при синхронізації параметрів між юнітами.
- Єдина безпечна модель apply для одиночних і масових змін.

Trade-offs:
- Додано новий UI-блок і стан batch-операцій.
- Потрібно підтримувати зрозумілі preview-рядки для нових batch-дій.

## Rollback / Alternative

Rollback:
- Прибрати batch-block з Unit Designer і повернутись до поштучних змін.

Alternative:
- Винести batch-операції у окреме EditorWindow (відхилено на цьому етапі через вищий поріг входу для дизайнерів).
