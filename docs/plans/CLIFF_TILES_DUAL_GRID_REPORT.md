# Cliff tile set — dual-grid migration report

Replaces the procedurally built atlas tile meshes with the KayKit
`TileSet-1.fbx` Cliff tile set, normalized to the project's dual-grid
contract. All nine themes share the same ten geometry meshes; only the
material/texture changes per theme.

## Source

`Assets/Moyva/Art/World/Tiles/AtlasV3/Models/TileSet-1.fbx`
(Blender 5.2 export, 19 Cliff_* models, KayKit `hexagons_medieval`
material — texture reference stripped; project T_MOYVA_* textures used
instead).

| form | source models | chosen yaw |
|------|---------------|-----------|
| corner | Cliff_Corner_Tile ×4 | 90 |
| edge | Cliff_Edge_Tile ×4 | 0 |
| interior | Cliff_Int_Corner_Tile ×2 | 180 |
| merged | Cliff_Double_Corner_Tile ×3 | 90 |
| fill | Cliff_Fill_Tile ×6 | — |

Canonical yaw is auto-derived per form from source top-face quadrant
coverage matched against `AtlasDualGridShapes` masks (corner = SW,
edge = S half, interior = all-but-SW, merged = SW+NE). Offsets in the
generated presets remain zero — orientation is baked into vertices.

## Normalization

`MoyvaCliffTileAssetBuilder.Build()` (Editor):

- bakes FBX model rotation (−90°X) and scale (×100) into vertices —
  imported meshes carry identity-friendly data;
- scales authored 2×2-unit footprint → 1×1 project cell (fill quad is
  the scale reference);
- recentering per tile: XZ stays quad-centered, top surface → Y = 0;
- emits high (−0.5) and low (−0.25) wall-depth variants;
- rebuilds UVs (planar projection on tops, box-projected walls), emits
  unshared vertices for faceted normals — no `RecalculateNormals`;
- writes 10 shared meshes under `Generated/Meshes/Cliff/`;
- writes 9 theme materials under `Generated/Materials/Cliff/`
  (URP/Lit, `_BaseMap` = T_MOYVA_* per theme, smoothness 0.12,
  glossy reflections off — same fields as `Moyva_Atlas.mat`);
- rewrites the 90 existing `{theme}_{form}[_low]` prefabs **in place**
  via `LoadPrefabContents` → mutate → save, preserving root fileIDs so
  `TilePreset.DUALGRD_*` references keep resolving.

Hooked into `MoyvaAtlasPackImporter.Import()` after
`NormalizeDeterministicGuids()` — the official regeneration path stays
complete.

## Bounds verification (generated meshes)

- `cliff_corner`: X∈[−0.5,0], Z∈[−0.5,0], Y∈[−0.5,0] — canonical SW
  quadrant, plateau top at 0, walls to −0.5.
- `cliff_edge`: S-half footprint.
- `cliff_corner_low`: −0.25 drop.
- `cliff_fill`: full-tile footprint, ~flat.
- Deterministic GUIDs; preset → prefab fileID chain verified intact.

## Test / smoke results

- Compile: 0 errors (smoke compile, sources=1753).
- Focused Generator EditMode: 135/135.
- Visual smoke, recipe seed (48×48):
  - run 1: `worldHash=2F557635C172624E`, verts=749329, nanVerts=0,
    0 errors;
  - rerun: identical hash/verts — deterministic.
  - Evidence: `docs/qa/evidence/cliff-tiles-seed777/` and
    `...-seed777-rerun/`.
- Land/water counts identical to the pre-swap seed-777 world
  (land=1351, water=953) — generation logic untouched, meshes only.

## Visual notes / known artifacts

- KayKit chunky slab aesthetic: visible seams between adjacent tiles
  and rounded slab edges are the pack's authored look, not stitching
  defects.
- Dark wall faces on the shadow side: T_MOYVA_* albedos are dark and
  wall UVs are box-projected; normals verified outward (lit faces
  correct in `corner_sw.png`).
- Iridescent/moiré specular on water and dark flats: pre-existing —
  identical in `twc-migration-seed777` captures; material keywords
  match the previous atlas material (`_GlossyReflections: 0`).
- Waterfall strips still use the opaque water material (no waterfall
  theme in the atlas) — unchanged from previous state.

## Changed/added paths

- `Assets/Moyva/Art/World/Tiles/AtlasV3/Models/TileSet-1.fbx` (new)
- `Generated/Meshes/Cliff/` — 10 shared meshes (new)
- `Generated/Materials/Cliff/` — 9 theme materials (new)
- `Generated/Prefabs/{theme}_{form}[_low].prefab` — 90 rewritten in
  place (mesh + material refs only)
- `Generated/Materials/Moyva_Atlas.mat` — albedo-only (normal/metallic
  packed maps removed; the unused atlas PNGs were deleted earlier)
- `MoyvaCliffTileAssetBuilder.cs` (new), `MoyvaAtlasPackImporter.cs`
  (hook + helpers made internal)
