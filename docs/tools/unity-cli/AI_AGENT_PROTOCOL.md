# AI agent protocol for Moyva + Unity CLI

An agent operating Moyva through Unity CLI should follow this contract.

## Phase A — discovery
1. Confirm project connection with `unity status`.
2. Discover commands with `unity list` / `unity command`.
3. Get `moyva-project-status`.
4. Audit the relevant scene/UI before editing.
5. Inspect exact objects and their prefab/source relationships.

## Phase B — plan
Before changing anything, identify:
- objects to modify
- serialized/runtime dependencies
- whether the object is prefab-owned
- whether LayoutGroup/ContentSizeFitter will override manual RectTransform changes
- expected result
- rollback path

## Phase C — mutate
Prefer registered `moyva-*` commands for common UI changes because they:
- resolve exact hierarchy paths
- use Unity Undo
- mark the scene dirty
- do not auto-save

Use official Pipeline `eval`/`eval_file` for operations not represented by a permanent command.

## Phase D — verify
After mutations:
1. run another audit
2. inspect changed targets
3. check for missing components/references
4. verify no unintended hierarchy movement
5. run Play Mode if runtime behavior matters

## Phase E — commit to disk
Only after verification run `moyva-ui-save-scene`.

The save command creates an external scene backup first.

## Hard rules
- Never mass-delete hierarchy items without an explicit target list.
- Never save after a failed/partial mutation sequence.
- Never treat a GameObject name as unique without checking its full hierarchy path.
- Never assume a RectTransform is authoritative when a layout component controls it.
- Never expose or commit Unity CLI auth/security tokens.
- Prefer structured JSON output for agent parsing.
- If an operation is ambiguous, inspect more state rather than guessing.
