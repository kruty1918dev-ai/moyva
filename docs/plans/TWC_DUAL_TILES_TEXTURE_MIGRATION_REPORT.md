# TWC Dual-Tiles Texture Migration — Report

Plan: `docs/plans/TWC_DUAL_TILES_TEXTURE_MIGRATION_PLAN.md`
Source prompt: `Library/ai/packs/MOYVA_TWC_DualTiles_TextureReplacement_Pack/PROMPT_UA.md`
Status: implemented and verified.

## 1. What changed

### Textures and material

- Seven supplied base-color textures imported to
  `Assets/Moyva/Art/World/Tiles/AtlasV3/Textures/Sources/`
  (sRGB, mipmaps, original source GUIDs preserved).
- `Moyva_AlbedoAtlas.png` rebuilt **in place** (same GUID
  `1da97433e8793ac4ea48b784985d58a1`) at the original 2048×4096,
  4×8-cell layout, 16 px gutters respected. Authored mesh UVs were not
  touched, so the dual-tile half-composition is unchanged.
- Theme → texture assignment (verified by mean-colour match):

  | Theme | Base cell | Border cell composition |
  | --- | --- | --- |
  | grass | Grass | Grass lip → Earth soil → Rock face |
  | sand | Sand | Sand lip → Earth soil → Rock face |
  | dirt | Earth | Earth → Earth (darker) → Rock face |
  | stone | Rock | Rock rim → Rock face |
  | snow | Snow | Snow lip → Rock face |
  | swamp | ForestFloor | ForestFloor lip → Earth soil → Rock face |
  | rock_cliff | Rock | Rock rim → Rock face |
  | road | Rock | Earth rim → Rock face |
  | footpath | Earth | Earth rim → Rock face |

- `Moyva_Atlas.mat`: `_BumpMap`/`_MetallicGlossMap` cleared, `_NORMALMAP`
  and metallic keywords disabled, `_Smoothness` lowered to 0.12 (matte
  faceted look). `_BaseMap`/`_MainTex` → rebuilt atlas.
- Deleted `Moyva_NormalAtlas.png`, `Moyva_MetallicSmoothnessAtlas.png`,
  `Moyva_RoughnessAtlas.png` (+`.meta`) after a repo-wide reference
  check — only `Moyva_Atlas.mat` used them, and its refs were cleared.
- `MoyvaAtlasPackImporter.cs`: `CreateOrUpdateMaterial` is albedo-only
  and explicitly clears the old map slots/keywords so a re-run cannot
  resurrect them. Texture prep unchanged (sRGB, clamp, trilinear,
  aniso 4, 4096, CompressedHQ).

### Bark

- New `VegBark.mat` is produced by `MoyvaVegetationAssetBuilder`
  (decor shader + `T_MOYVA_Bark_BaseColor.png`), deterministic GUID
  `a2e485432f1f54a8d13f8f964c56f589`.
- `veg-log`, `veg-twig-a`, `veg-twig-b` prefabs use it; the builder now
  emits real UVs for those meshes (`MB.PrismXBark` — seam-duplicated
  wrap around the circumference, v along the length; `MB.StripUv` for
  the forked twig) so the bark grain is visible instead of a single
  palette texel. Verified by decoding `veg_log.asset` vertex data.
- Regeneration is durable: the builder creates the material itself, so
  re-running `Moyva → Vegetation → Rebuild Generated Assets` keeps the
  bark wiring.

### Smoothing removal

- New `FacetNormalsMeshUtility.Apply(mesh)` writes per-face geometric
  normals — no averaging, no `RecalculateNormals`.
- Call sites migrated:
  - `ChunkTerrainMeshBuilder` — border-clamped copies keep authored
    normals (`RecalculateNormals` removed); combine fallback uses
    `FacetNormalsMeshUtility` only when the mesh has no normal attribute.
  - `TileVerticalFillMeshUtility` — boundary skirt, UV-remap fallback,
    axis-aligned closure skirt all use facet normals.
  - `TileWorldCreatorTerrainSideWallMeshBuilder` — generated side walls
    use facet normals (tangents still recalculated afterwards).
- Authored pack normals were already faceted (verified vs face normals
  during the audit); generated geometry is now faceted by construction.

### World-generation wiring

- All nine atlas themes resolve through
  `Presets/Generator/atlas-tile-set/atlas-tile-set.json` →
  `IAtlasTileSetCatalog` → generated `atlas-<theme>` presets.
- Repointed tile-type JSONs whose variants still referenced third-party
  presets, closing the last old-texture path on canonical config:
  - `hill` → `atlas-stone` (it renders via the Stone/RockCliff recipe
    layers; id stays a biome/gameplay id).
  - `mountain` → `atlas-rock_cliff`.
  - `lowland`, `forest-dense`, `forest-sparse` → `atlas-grass`.
  - `representativePrefab` fields removed to match atlas convention.
- Legacy `Presets/Grid/tile-registry/*.json` and
  `tile-world-creator-id-mapping` keep their third-party references —
  they feed only the disabled MapVisual fallback path and the legacy
  single-grid registries, which no longer drive the chunk-first renderer
  (the three `TilesBuildLayer`s in the TWC configuration point at
  blueprint GUIDs nothing references — verified dead).

## 2. Dual-tile contract — audit findings

- 99 pack meshes (9 themes × 5 high + 5 low forms + 1 stair) match the
  TWC contract already: 1 m grid, cell-centre pivot at Y=0, high bottom
  −0.5 m, low bottom −0.25 m, stair rise 0.25 m (`tile_manifest.json`,
  `QA/geometry_report.json`, `QA/socket_checks.json`). No mesh edits were
  required; nothing is stretched for height changes.
- Half-texture composition is structural (each cell emits only its own
  quad portion); preserving authored UVs keeps mixed-theme quads working.
- Canonical masks (corner 1 / edge 3 / interior 14 / merged 5 / fill 15,
  SW=1/SE=2/NE=4/NW=8) and 90° yaw rotation live in
  `AtlasDualGridShapes.cs` — unchanged and covered by tests.
- Water/waterfalls/shores unchanged (separate `SurfaceOnly` sheets;
  `sand` theme under water beds). Stair uses each theme's authored module.
- Open stair sides: the supplied pack provides no authored side closures;
  generated skirt/closure geometry continues to cover them.

## 3. Verification

- `tools/ai/smoke_compile.py`: 0 errors (1752 sources).
- Focused EditMode `Kruty1918.Moyva.Generator`: 135/135 pass.
- Full EditMode suite: **877/877 pass** (`Kruty1918.Moyva`, EditMode).
- Visual smoke, seed 42 (run twice — deterministic):
  - 48×48 world, 9 meshes, 841 929 verts, **0 NaN verts**,
    land 1155 / water 1149 / empty 0,
    `worldHash=5D8E6C1024CA46E8` — identical across both runs.
- Visual smoke, seed 777: 48×48, 838 091 verts, 0 NaN,
  land 1351 / water 953, `worldHash=5187D641E927D132` (different seed →
  different deterministic world; no console errors in
  `Library/ai/visual-smoke-errors.log`).
- Evidence: `docs/qa/evidence/twc-migration-seed42`,
  `…-seed42-rerun`, `…-seed777`.

Visual acceptance (screenshots inspected, not just captured):

- Flat fill tiles sample the new theme textures; faceted shading visible.
- Four-tile junctions share one material, seams welded — no gaps or
  z-fighting seen in `detail_*` shots.
- Mixed-theme quads keep complementary halves (grass/earth adjacent
  fragments in `detail_transition.png`/`transition.png`).
- Shorelines carry the banded rim (theme lip → Earth → Rock) and sand is
  a real biome layer, not water-adjacency flooding (`sandstack.txt`:
  `tileId=sand`, own height/layer).
- Height steps use high vs low border forms; cliff bands face outward on
  `edge_s.png`/`side_*.png`; world edge is flush.
- Rivers, waterfalls and water sheets render unchanged; construction
  grid stays aligned to tiles.
- The world is darker than the old look because the supplied textures
  are intrinsically mid-dark (measured means above) — authored content,
  not a rendering defect.

## 4. Known non-issues / leftovers

- `oob.txt` reports a few vegetation props overhanging map-edge cells —
  pre-existing placement quirk, unrelated to the terrain migration.
- Third-party `Tiles URP` packs remain in `Assets/ThirdParty/` for the
  legacy registries/TWC fallback; nothing on the active path loads them
  (tile-type JSONs are now all atlas).
- The prompt asked for `Moyva_Bark.mat`; implemented as `VegBark.mat`
  under `Assets/Moyva/Generated/Vegetation/` so the vegetation builder
  owns it end-to-end.
- Edit-mode note for the future: a mistyped `-testFilter` left a Unity
  batch process holding `Temp/UnityLockfile`, which made follow-up runs
  exit instantly — killed the stale process to unblock.
