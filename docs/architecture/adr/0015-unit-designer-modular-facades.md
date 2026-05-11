# 0015 Unit Designer Modular Facades

- Status: Accepted
- Date: 2026-05-11

## Context

`UnitDesignerWindow` виріс до великого monolith-файлу з багатьма відповідальностями (identity, prefab, animation, preview, combat), що ускладнювало зміну окремих частин та збільшувало ризик побічних регресій.

## Decision

Ввести фасадний поділ Unit Designer на підмодулі:

1. Identity facade.
2. Prefab facade.
3. Animation facade.
4. Preview facade.
5. Combat facade.

`UnitDesignerWindow` викликає ці частини тільки через інтерфейси фасадів.

## Consequences

Positive:
- Чіткі межі відповідальності в редакторі.
- Простіший поетапний рефакторинг без великого перепису.
- Менше змішаних правок у великому файлі.

Trade-offs:
- Додатковий шар абстракції.
- Потрібно підтримувати контракти фасадів узгодженими.

## Rollback / Alternative

Rollback:
- Повернути прямі виклики секцій у `UnitDesignerWindow` без фасадного шару.

Alternative:
- Повна декомпозиція в окремі EditorWindow інструменти для кожного домену.
