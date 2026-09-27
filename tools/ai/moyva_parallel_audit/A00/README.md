# moyva_parallel_audit/A00 — reproducible audit bench

Shared base, ownership and measurement contract for the parallel audit waves on
`improvement/moyva-production-polish`. Verified against base
`98d86e11f1fd905d7e90b6b7a4e8c30dc6b20e38`. These files are the shared contract
proposal: changes here must be agreed with I01/I02 before dependent work.

## Contents

| File | Purpose |
|---|---|
| `run-header.schema.json` | Machine-readable run header: SHA, seed, resolved archetype, dimensions, mode, fps/frameTime, clockConfig, snapshot sequence, wall/game time, RNG fixture, owner |
| `event-trace.schema.json` | JSONL row schema for the actual delta/event trace (SignalBus + command results) |
| `audit-seeds.json` | Three RNG fixtures with statically precomputed expected resolution |
| `canonical-queries.md` | Read-only query surface + APIs forbidden inside scans (no `TryPreviewAt`) |
| `evidence-standards.md` | What counts as proof: snapshot identity, pause vs stop, live vs staged, input provenance, concurrency |
| `collect_run_header.py` | Emits a schema-conforming run header (git/env probe; merges runtime probe) |
| `capture_runtime_snapshot.cs` | Editor `eval_file` probe: live fields + optional SignalBus JSONL trace |
| `validate_scope.py` | Scope validator: changed paths vs the task whitelist + tree-SHA report |

## Usage

```bash
# scope check (run inside the task worktree)
python tools/ai/moyva_parallel_audit/A00/validate_scope.py \
    --base 98d86e11f1fd905d7e90b6b7a4e8c30dc6b20e38 --task-id A00

# environment probe header (no Unity required)
python tools/ai/moyva_parallel_audit/A00/collect_run_header.py \
    --seed-id audit-s01-default --sequence 0 \
    --out Temp/ai/moyva-parallel/A00/run-header-s01.json

# live session (requires the serving Editor for THIS worktree)
tools/unity-cli/moyva-unity eval-file \
    tools/ai/moyva_parallel_audit/A00/capture_runtime_snapshot.cs
python tools/ai/moyva_parallel_audit/A00/collect_run_header.py \
    --seed-id audit-s01-default --sequence 0 --probe-editor \
    --runtime-json Temp/ai/moyva-parallel/A00/runtime-snapshot.json \
    --out Temp/ai/moyva-parallel/A00/run-header-s01.json
```

Set `MOYVA_AUDIT_TRACE=1` in the Editor's environment before eval to also arm the
`event-trace.jsonl` SignalBus trace for the play session.

## Resolution rules implemented (verified against base SHA)

- Seed: `GameLaunchContext.TryGetSeed` -> `GeneratorMapRecipe.Seed` ->
  `GlobalSeed.DefaultSeed (42)`; normalized `0 -> 1`; fed via
  `GlobalSeed.InitializeDeterministic` in `MapGenerationPipeline.Generate`.
- Dimensions: `GameLaunchContext` (explicit or `Size -> 32/64/128`) ->
  `recipe.SharedSettings` -> requested; then `MapChunkSizePolicy.CropAxis`
  (`<16 -> 16`, else floor to multiple of 16).
- Archetype: **not resolved in production at this base SHA** — the recipe is the
  generator profile; `WorldArchetypeResolver`/`WorldGeographyEngine` are only
  exercised by tests. Headers must emit `resolvedArchetype="not-resolved"`.
- Pause: `GameStateService.PauseGame` writes `timeScale=0` only when
  `IGamePauseModePolicy.IsMultiplayerSessionActive` is false; `GamePausedSignal`
  fires either way.
