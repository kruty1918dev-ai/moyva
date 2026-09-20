# Tech Debt Register

Правило: записи відсортовані за спаданням Priority.

Priority formula:

Priority = Impact * Risk + Urgency - Effort

## Items

| ID | Title | Area | Impact | Risk | Effort | Urgency | Priority | Status | Owner | Target | Link |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| TD-001 | Migrate legacy gameplay subscribers to DomainEvents — resolved by deletion: subscribers moved to canonical signals | Signals | 5 | 4 | 3 | 3 | 20 | Done | Core Team | Iteration 22 | ADR-0013 (superseded) |
| TD-002 | Remove transitional SignalDomainEventBridge after full migration — bridge and DomainEvents layer deleted | Signals | 4 | 4 | 2 | 2 | 16 | Done | Core Team | Iteration 23 | ADR-0013 (superseded) |
| TD-003 | Split oversized UnitDesignerWindow into focused editors/services | Units/Editor | 4 | 3 | 4 | 2 | 10 | Planned | Tools Team | Iteration 23 | UnitDesignerWindow |
