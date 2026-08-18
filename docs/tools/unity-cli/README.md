# Moyva Unity CLI / Pipeline Bridge

This subsystem gives terminal and agent tooling a stable, project-specific interface to the running Unity Editor.

## Architecture

```text
terminal / AI agent
        |
        | Unity CLI
        v
com.unity.pipeline (local Editor API)
        |
        +--> built-in eval / eval_file
        |
        +--> Moyva [CliCommand] bridge
                |
                +--> project / scene status
                +--> hierarchy inspection
                +--> UI audit
                +--> safe UI mutations with Undo
                +--> explicit scene save with external backup
```

The bridge is installed at:

`Assets/Moyva/Editor/UnityCliBridge/`

The terminal helper is:

`tools/unity-cli/moyva-unity`

## First verification

With the Moyva project open:

```bash
cd ~/moyva
unity --version
unity auth status
unity pipeline list
unity status --format json
unity command --project-path="$PWD" --format json
```

You should see commands whose names start with `moyva-`.

## Recommended terminal helper

```bash
cd ~/moyva

tools/unity-cli/moyva-unity status
tools/unity-cli/moyva-unity project
tools/unity-cli/moyva-unity scene
tools/unity-cli/moyva-unity ui
```

## Arbitrary live C#

For tasks not covered by the registered bridge:

```bash
tools/unity-cli/moyva-unity eval 'return Application.unityVersion;'
tools/unity-cli/moyva-unity eval-file tools/unity-cli/eval/scene-summary.cs
```

`eval`/`eval_file` run inside the connected Editor through Unity Pipeline. Use them for inspection and carefully scoped edits that do not justify adding another permanent command.

## Editor window

Open:

`Moyva -> Tools -> Unity CLI Bridge`

It provides local read-only audits, quick-start commands, and links to this documentation.

## Important safety model

Read first, mutate second, inspect again, save last.

Most mutation commands only modify the in-memory scene and mark it dirty. They deliberately do **not** save automatically.

Use:

```bash
tools/unity-cli/moyva-unity save
```

only after verifying the result. The save command writes an external backup of the previous scene file to:

`~/.local/share/moyva-cli/backups/scenes/...`

No auth tokens are stored in the Unity project.
