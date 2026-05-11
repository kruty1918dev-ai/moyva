# 0019 - Designer Preset System

- Status: Accepted
- Date: 2026-05-10

## Context

Після safe edit mode, pre-save validation і batch operations, дизайнерам бракувало повторно використовуваних шаблонів між різними редакторами. Налаштування юнітів, будівель, fog і economy часто дублювались вручну.

## Decision

Запроваджено спільну preset-систему на базі `DesignerPresetLibrarySO` з інтеграцією в ключові дизайнерські вікна:

- `UnitDesignerWindow`
- `BuildingDesignerWindow`
- `FogVisionTuningWindow`
- `EconomyDesignerWindow`

Для застосування використано централізований `DesignerPresetApplier`.

Ключові гарантії:

- для юнітів зберігається `TypeId`;
- для будівель зберігається `Id`;
- застосування вбудоване в `Undo`-пайплайн.

## Consequences

Плюси:

- швидший дизайн-цикл;
- консистентніші налаштування між доменами;
- один формат керування preset-ами для всіх дизайнерських вікон.

Мінуси:

- economy/fog preset-и в MVP копіюють asset цілком, що вимагає уважності при виборі template;
- потрібне підтримання актуальності preset library.

## Alternatives Considered

- Роздільні preset-системи в кожному вікні.
- Відхилено: дублювання логіки, вищі витрати на підтримку і неузгоджений UX.
