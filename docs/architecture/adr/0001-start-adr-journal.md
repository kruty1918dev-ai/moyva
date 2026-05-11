# 0001 Start ADR Journal

- Status: Accepted
- Date: 2026-05-11
- Decision Makers: Core Team

## Context

Архітектурні рішення зберігались фрагментарно: у чаті, PR-коментарях або в коді без пояснення причин. Нові розробники повторювали старі помилки через відсутність компактного журналу рішень.

## Decision

Вести ADR-журнал у `docs/architecture/adr/` і фіксувати ключові рішення коротким форматом: Context, Decision, Consequences, Rollback.

## Consequences

- Positive:
  - Швидше онбординг нових розробників.
  - Менше повторних дискусій по вже закритих питаннях.
  - Прозора історія "чому так".
- Negative / Trade-offs:
  - Додаткова дисципліна підтримки ADR при важливих змінах.
- Risks:
  - Якщо ADR не оновлюється, журнал застаріває.

## Rollback / Alternative

Альтернатива: зберігати рішення лише в PR. Відхилено через низьку discoverability.

## Links

- `docs/architecture/adr/README.md`
- `docs/architecture/adr/0000-template.md`
