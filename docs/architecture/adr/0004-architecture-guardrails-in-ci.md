# 0004 Architecture Guardrails in CI

- Status: Accepted
- Date: 2026-05-11
- Decision Makers: Core Team

## Context

Архітектурні правила (розмір файлів, залежності модулів, неймінг) існували як домовленості і не були формально перевірені автоматикою.

## Decision

Додати CI guardrails:

- file-size policy;
- feature dependency map + cycle check;
- naming policy check.

Використовується debt-підхід: legacy порушення не блокують, нові порушення блокують у strict-сценаріях.

## Consequences

- Positive:
  - Правила стають enforceable.
  - Менше регресій архітектурної якості.
- Negative / Trade-offs:
  - Потрібно підтримувати точність правил і винятків.
- Risks:
  - Надто жорсткі перевірки можуть давати false positives.

## Rollback / Alternative

Альтернатива: ручний code review без автоматичних перевірок. Відхилено як недостатньо надійна для масштабу проєкту.

## Links

- `tools/quality/check-file-length-limits.sh`
- `tools/quality/feature_dependency_map.py`
- `tools/quality/check-naming-policy.py`
