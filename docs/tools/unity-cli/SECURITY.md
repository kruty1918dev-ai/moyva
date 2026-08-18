# Security and operational notes

Unity CLI and `com.unity.pipeline` are experimental tooling.

The Pipeline connection is intended for local automation. `eval` can execute arbitrary C# inside the running Editor, so it must be treated as code execution with the same authority as the Unity Editor process.

Rules for Moyva:
- Do not commit CLI credentials or security tokens.
- Do not copy authentication state into `Assets`, `Packages`, `ProjectSettings`, `docs`, or `tools`.
- Keep externally generated backups under the user profile, not inside the Git repository.
- Use read-only commands before any mutation.
- Keep destructive commands confirmation-gated.
- Use the official `unity --help` / `unity mcp --help` output when experimental command syntax changes.

External scene backups:
`~/.local/share/moyva-cli/backups/scenes/`
