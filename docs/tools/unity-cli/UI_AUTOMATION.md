# UI automation workflow

This is the preferred workflow when modifying the Gameplay UI through Unity CLI.

## 1. Observe

```bash
tools/unity-cli/moyva-unity project
tools/unity-cli/moyva-unity ui > /tmp/moyva-ui-before.json
```

Do not begin by moving objects blindly.

## 2. Inspect exact targets

```bash
tools/unity-cli/moyva-unity object 'Canvas/TopBar'
tools/unity-cli/moyva-unity object 'Canvas/BuildMenu'
```

Record anchors, pivot, size, current components, sprite references, and prefab origin.

## 3. Make small in-memory changes

Examples:

```bash
tools/unity-cli/moyva-unity set-text 'Canvas/TopBar/TurnText' 'Turn 4'

tools/unity-cli/moyva-unity set-rect \
  'Canvas/BuildMenu' \
  0 -380 820 150
```

For unsupported changes, use `eval` or `eval-file`.

## 4. Re-audit before saving

```bash
tools/unity-cli/moyva-unity ui > /tmp/moyva-ui-after.json
```

Compare the exact target objects.

## 5. Use Unity Undo if needed

```bash
tools/unity-cli/moyva-unity undo
```

## 6. Save explicitly

```bash
tools/unity-cli/moyva-unity save
```

Saving creates an external backup before overwriting the scene asset.

## 7. Verify runtime behavior

Enter Play Mode manually or through an appropriate `eval` command, then re-read the hierarchy/state and inspect the Editor console.

## Design principle

The CLI bridge is an execution surface, not the source of truth for UI architecture. Runtime scripts, serialized references, prefab ownership, and existing layout components must be inspected before restructuring a hierarchy.
