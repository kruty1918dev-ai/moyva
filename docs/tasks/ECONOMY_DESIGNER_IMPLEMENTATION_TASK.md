# Task: Implement Economy Designer (Unity Editor Tool)

## Goal
Create a dedicated Unity Editor tool named `EconomyDesignerWindow` for configuring economy-related game data in one place.

This tool must make setup and future changes to the economy system easy, clear, and safe for developers/designers.

## Context
Project uses feature-based structure under `Assets/Moyva/Scripts/Features/*` and already has custom editor patterns (for example `RegistryHubWindow`).

We need a focused economy setup tool, not scattered inspectors.

## Scope (MVP)
Implement editor-side data model + editor window + validation + basic simulation preview.
Do NOT implement full runtime economy gameplay in this task.

## Required Deliverables

### 1) New Economy Data Assets (ScriptableObject)
Create a dedicated folder structure under:
- `Assets/Moyva/Scripts/Features/Economy/API/`
- `Assets/Moyva/Scripts/Features/Economy/Runtime/`
- `Assets/Moyva/Scripts/Features/Economy/Editor/`

Create ScriptableObject definitions for:
1. `EconomyResourceDefinition`
- `Id` (string)
- `DisplayName` (string)
- `Category` enum: `Food`, `Materials`
- `Icon` (Sprite)
- `StackLimit` (int, optional behavior)

2. `EconomySettlementDefinition`
- `SettlementId` (string)
- `SettlementType` enum: `Village`, `Castle`
- `CenterBuildingId` (string)
- `BuildRadius` (int)

3. `EconomyWarehousePolicy`
- Warehouse type enum: `FoodWarehouse`, `MaterialsWarehouse`
- Per-resource policy entries:
  - `ResourceId` (string)
  - `ConsumptionAllowed` (bool)
  - `Priority` (int)
  - `ReserveAmount` (int)

4. `EconomyProductionProfile`
- `BuildingId` (string)
- `IsActiveByDefault` (bool)
- `RecipeId` (string)
- `CycleDurationSeconds` (float)
- `OutputAmountPerCycle` (int)

5. `EconomyCaravanTemplate`
- `TemplateId` (string)
- `AllowedResourceIds` (list)
- `Capacity` (int)
- `DefaultPriority` (int)
- `UseLoopDelivery` (bool; for future automation)

6. `EconomyAiRuleProfile`
- `ProfileId` (string)
- shortage/excess thresholds per resource
- simple booleans for conservative spending

7. `EconomyDatabaseSO`
- References/lists to all economy entities above
- `SchemaVersion` (int)

### 2) Economy Designer Window
Create `EconomyDesignerWindow` under Editor.
Add menu item similar to existing tools (for example under `Moyva/Tools`).

Window must include tabs:
1. Settlements
2. Resources
3. Warehouses
4. Production
5. Caravans
6. AI Rules
7. Validation
8. Simulation

### 3) UX Requirements
- Left side: list/tree of entities for active tab
- Center: editable inspector-like form
- Search/filter on each tab
- Clear section headers and concise hints
- Undo/Redo support via SerializedObject flows
- Avoid modifying unrelated assets/files

### 4) Validation Center
Implement checks and report list with severity (Error/Warning):
- missing IDs
- duplicate IDs
- resource without category
- settlement without center or invalid radius
- warehouse policy references unknown resource
- production profile missing building/recipe/output values
- caravan template with invalid capacity

Add action button:
- `Fix Common Issues` (safe auto-fixes only, e.g. trim spaces, fill default priority/radius if zero)

### 5) Simulation Preview (Editor-only lightweight)
Implement a simple deterministic preview (no runtime world dependencies):
- Input: selected settlement + selected production profiles + duration (minutes)
- Output: estimated resource deltas over time
- Show table/log for resource totals at end

Keep simulation intentionally simple and documented as approximation.

### 6) Versioning and Migration Stub
- Add `SchemaVersion` handling in `EconomyDatabaseSO`
- Add `EconomyDataMigrationService` (editor utility)
- Implement at least migration path from version 1 to current version (even if no-op with logging)

### 7) Documentation
Create documentation file:
- `docs/systems/economy/economy-designer.md`

Must include:
- What the tool configures
- Tab-by-tab usage
- Validation rules
- Simulation limitations
- How to add new resource/settlement safely

### 8) Tests (EditMode)
Add focused tests for editor/domain logic (where feasible):
- validation catches duplicates/missing refs
- simulation produces expected deterministic totals for fixed input
- migration service handles schema version transitions

## Non-Goals
- No full gameplay runtime economy implementation
- No caravan pathfinding runtime behavior
- No AI runtime logic integration

## Code Quality Constraints
- Keep style consistent with existing project conventions
- Use concise comments only where logic is non-obvious
- Preserve asmdef boundaries; add new asmdefs only if required
- Do not touch unrelated existing files

## Acceptance Criteria
- Tool opens and is usable for all 8 tabs
- Data can be created and edited without manual asset hunting
- Validation catches major config issues with actionable messages
- Simulation tab gives deterministic preview output
- Docs and tests added and pass (at least EditMode tests)

## Suggested Commit Message
`feat(economy-editor): add Economy Designer window with validation and simulation preview`
