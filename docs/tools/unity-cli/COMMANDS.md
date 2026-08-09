# Moyva Unity CLI command reference

All registered commands are discoverable with:

```bash
unity command --project-path="$PWD"
```

For machine-readable schemas:

```bash
unity list --project-path="$PWD" --format json
```

## Read-only commands

### `moyva-project-status`
Returns project path, Unity version, play/compile state, and active scene.

### `moyva-scene-tree`
Returns the active scene hierarchy with component and UI metadata.

### `moyva-ui-audit`
Returns UI-related hierarchy entries and key properties:
- hierarchy path
- active state
- component types
- RectTransform anchors, pivot, position, size
- Canvas render mode and sorting order
- Image sprite/path/color/raycast
- TMP text/font/size/color
- Button state and persistent listener count
- LayoutGroup / ContentSizeFitter
- prefab source path

### `moyva-ui-object --path <path>`
Inspects one exact object.

## Mutation commands

These leave the scene dirty and do not save automatically.

- `moyva-ui-select --path <path>`
- `moyva-ui-set-active --path <path> --active true|false`
- `moyva-ui-set-text --path <path> --text <value>`
- `moyva-ui-set-sprite --path <path> --asset-path Assets/...`
- `moyva-ui-set-rect --path <path> --x X --y Y --width W --height H`
- `moyva-ui-set-anchors --path <path> --min-x ... --min-y ... --max-x ... --max-y ... --pivot-x ... --pivot-y ...`
- `moyva-ui-reparent --path <path> --parent-path <path>`
- `moyva-ui-create-panel --parent-path <path> --name <name>`
- `moyva-ui-create-text --parent-path <path> --name <name> --text <text>`
- `moyva-ui-delete --path <path> --confirm DELETE`
- `moyva-ui-undo`

## Explicit save

`moyva-ui-save-scene`

Before saving, it backs up the current on-disk `.unity` file and `.meta` outside the project.

## Built-in Unity Pipeline tools

The bridge does not replace the official built-ins.

Use `eval` for fast one-off C#:

```bash
unity command eval 'return UnityEditor.EditorApplication.isPlaying;' --project-path="$PWD"
```

Use `eval_file` for longer scripts:

```bash
unity command eval_file tools/unity-cli/eval/scene-summary.cs --project-path="$PWD"
```

Use `unity --help`, `unity command --help`, `unity pipeline --help`, and `unity mcp --help` as the authoritative syntax for the installed experimental CLI version.
