# Square tiles — preserve authored FBX shape (2026-09-26)

Task: land tiles must render as the authored square FBX modules (crisp edges,
flat tops, original low-poly geometry) instead of a rounded/wavy/stretched
surface. Evidence basis: branch `improvement/moyva-production-polish`,
screenshots `034836`/`035236` showing deformed tile surfaces and edges.

## Proven cause

`ChunkTerrainMeshBuilder.ResolveVisibleMesh` called `ResolveWarpedMesh` →
`TileSurfaceHeightWarpUtility.TryCreate`, which sheared every dual-grid land
fragment's vertices onto a **bilinear field built from the four surrounding
cells' continuous `SurfaceHeight`**. `SurfaceHeight` is the rendered
continuous height — it carries per-cell noise jitter (±0.05 m) and the
stepped level value. Feeding it through a corner-height warp produced:

- wavy / melted seams between same-level neighbours (jitter sheared the
  otherwise-flat quad tops), and
- ramped, non-square tile tops wherever a corner cell sat at another level.

That warp was **redundant**: the authored dual-grid forms
(`fill`/`edge`/`corner`/`interior`/`merged` + `low` variants) already encode
biome *and* level boundaries (`ResolvedTileCompositionResolver.MatchesMain`
opens a side whenever a corner cell differs in terrain identity **or**
surface height), and `TileMeshEdgeBottoms`-driven vertical fill emits the
side walls that close each level drop. Stair passages never carried corner
heights and were never warped. The warp only ever deformed authored land
modules — it was the deformation.

`ResolveBorderClampedMesh` was checked and is **not** the deformer: it folds
only the overshoot apron vertices that cross the map rect back onto the
boundary, producing a flush rim wall (clean termination, verified on
`edge_s`/`corner_sw`). Kept.

`ExactVertexWeldMeshUtility` was checked and is **not** a positional weld —
it groups vertices on the full per-stream byte set (position + normal + uv +
tangent + …), never averages or moves a vertex, and only collapses
byte-identical duplicates / reorders indices. Kept; now covered by tests.

## Change

Removed the arbitrary corner-height slope warp entirely (not a bypass):

- `ChunkTerrainMeshBuilder.cs`: dropped the `ResolveWarpedMesh` step from
  `ResolveVisibleMesh`; removed `_warpedMeshCache`, `_warpedPassthroughCache`
  and the `ResolveWarpedMesh*` methods; corrected the weld comment.
- `TileMeshSource.cs`: removed `TileMeshCornerHeights`, the `cornerHeights`
  ctor arg, `CornerHeights`, `HasCornerHeights`.
- `TwcTileMeshSourceProvider.cs`: removed the corner-height computation and
  its threading through `TryAddAtlasDualSource` / `TryAddMeshSources` /
  `TryAddPrefabMeshSources`.
- Deleted `TileSurfaceHeightWarpUtility.cs` and `TileHeightWarpMeshKey.cs`
  (+`.meta`) — now orphaned.
- `TerrainPlanTests.cs`: removed the six warp tests; added two
  geometry-preservation tests for the kept weld
  (`ExactWeld_IdenticalAttributes_PreservesVertexData` — output verts are a
  subset of source verts, per-triangle vertex sets preserved under index
  reorder; `ExactWeld_DifferingAttributes_KeepsVerticesDistinct` — same
  position with different normal/uv does not merge).
- `WorldVisualSmoke.cs` (editor QA): added a `control_fbx` shot — spawns the
  authored `grass_fill` prefab at 4 rotations just south of the map border
  and renders it through the preview path alongside the real terrain chunk
  meshes, for a same-frame FBX-vs-generated comparison.

Whole tiles are now placed flat at their level; transitions between levels
are carried by the authored border forms + `edgeBottoms` side walls + stair
modules — no tile is stretched from hilltop to Y=0.

## Reference module

Gameplay land tiles come from `Assets/Moyva/Art/World/Tiles/AtlasV3/` —
`TileSet-1.fbx` extracted into `Generated/Meshes/<theme>_<form>.asset` and
driven by `Generated/Prefabs/<theme>_<form>.prefab`. `grass_fill` = mesh guid
`1cddbe3195674ee8233338f5aab5b76f`: a flat 1×1 quad (4 verts, LocalAABB
centre 0, extent 0.5×0×0.5), i.e. a crisp square module. Nominal module size
= 1 cell (`cellSize`), prefab scale 1.

## Verification (seed 6130, deterministic)

- `worldHash=F9314FE13D1FC4B4` — **identical** before and after (the change
  touches mesh construction only, not world generation).
- warped → unwarped: `meshes=9`, `verts 699362 → 701058`, `nanVerts=0`,
  `land=1180 water=1124 empty=0`, 0 console errors.
- Same-level tile edges are now straight/square; terrace walls clean and
  rectangular; shoreline, rivers, falls, confluence, border rim all coherent;
  no gaps / floating plates / stretched triangles observed.
- `control_fbx.png`: authored `grass_fill` row matches generated flat tiles.
- EditMode suite: **873/873 passed, 0 failed** (was 877; −6 warp tests,
  +2 weld-preservation tests).

Evidence: `docs/qa/evidence/fbx-square-seed6130/` (34+ shots incl.
`control_fbx`, `edge_s`, `corner_sw`, `detail_transition`, `shore`,
`detail_river`, `detail_waterfall`, `peak`, top-down + orbit views).

## Notes / limitations

- Same-level neighbours sit at their own continuous `SurfaceHeight`
  (±0.05 m jitter) — reads as flat square seams; no see-through gaps because
  land tiles carry a base slab and the `MatchesMain` epsilon decides coplanar
  merge vs border form.
- With the top now flat at the authored height, picking / construction /
  height queries that read `SurfaceHeight` align *better* with the rendered
  surface than under the warp (which tilted tops at the corners).
- Chunk regeneration reuses `ClearExistingMesh` + the mesh registry; vert and
  mesh counts were identical across repeated seed-6130 runs (no accumulation).
- Gameplay probe (same run): movement services resolve and the traversal
  policy correctly rejects `stone`/`water` for `infantry` (spawned unit was
  boxed in by stone → `moveTiles=1`, `move` issued). Barrack placement still
  failed because the castle's territory candidates near `(10,42)` are sparse —
  a probe cell-selection gap, not a production defect; `TryDeployReady` was
  therefore not exercised this cycle.
