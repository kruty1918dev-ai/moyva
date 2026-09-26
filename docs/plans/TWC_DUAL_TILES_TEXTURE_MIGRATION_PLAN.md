# TWC Dual-Tiles Texture Migration Plan

Source prompt: `Library/ai/packs/MOYVA_TWC_DualTiles_TextureReplacement_Pack/PROMPT_UA.md`.
Goal: remove tile-mesh smoothing, keep the real TWC dual-grid mechanism,
replace the old atlas textures with the 7 supplied `T_MOYVA_*_BaseColor` textures,
preserve dual-tile half-texture composition, verify visually and in tests.

## 1. Audit — canonical TWC values (from implementation, not assumptions)

Source of truth: `Assets/Moyva/Art/World/Tiles/AtlasV3/tile_manifest.json`,
`GENERATION_DEFAULTS.json`, `AtlasDualGridShapes.cs`, `TwcTileMeshSourceProvider.cs`.

| Property | Value |
| --- | --- |
| Logical cell | 1.0 m square, XZ plane, Y-up |
| Pivot | cell centre on the top surface, Y = 0 |
| Height quantum | 0.25 m (`heightQuantumMeters`) |
| High border bottom | −0.5 m |
| Low border bottom | −0.25 m (`lowBorderDropMaxMeters` = 0.25) |
| Stair module | 1×1 m footprint, 0.25 m rise, mid drop 0.125 m |
| Corner bits | SW=1, SE=2, NE=4, NW=8 |
| Canonical masks | corner=1, edge=3, interior=14, merged=5, fill=15 |
| Rotation | +90° yaw about Y maps SW→NW→NE→SE→SW |

### Tile creation path

1. `TwcTileMeshSourceProvider.CollectDualGridSources` emits the four
   half-offset dual fragments per logical cell (`±0.5, ±0.5` offsets).
   `ShouldCurrentOwnDualFragment` makes each physical fragment emit once.
2. `TryAddDualGridSource` builds the corner-match mask from
   `ResolvedTileComposition.*Matches`, resolves occluded sides, edge
   bottoms and corner heights.
3. `TryAddAtlasDualSource` resolves the theme via `IAtlasTileSetCatalog`
   (JSON `Presets/Generator/atlas-tile-set/atlas-tile-set.json`),
   `AtlasDualGridShapes.TryResolve` → form + yaw, `ShouldDemoteToFill`
   demotes occluded seams, `ResolveMaxOpenDrop` picks low vs high variant.
4. `AtlasTileTheme.ResolveForm` returns the prefab
   (`AtlasV3/Generated/Prefabs/<theme>_<form>[_low].prefab`).
5. `ChunkTerrainMeshBuilder` warps fragments onto corner heights
   (`TileSurfaceHeightWarpUtility`), builds vertical fill skirts
   (`TileVerticalFillMeshUtility`), clamps map-border protrusions,
   combines per material and exact-welds vertices.

### Dual-tile half-texture mechanism

Each fragment's mesh covers only the quad portion matching the emitting
cell's identity (corner form ≈ one quarter, edge ≈ half, interior ≈ ¾,
merged = two diagonal quarters, fill = whole quad). Two different-theme
cells emit complementary partial fragments into the same quad → one tile
made of halves of two textures. This is structural; preserving it requires
keeping the authored UV scheme intact.

UV scheme (verified from `Unity/MoyvaMeshData.json`):

- Base atlas cell: top surface. Fill samples u,v ∈ [0.032, 0.968];
  edge forms sample one half (v ≤ ~0.52), corners one quarter
  (u,v ≤ ~0.516), interior ¾, merged the two diagonal quarters.
- Edge/border cell: used only by border forms — upper band is the
  neutral dirt rim on the sloped apron, lower band is the rock-cliff
  side face. Skirt faces UV into the lower part.

### Atlas layout (2048×4096, 4×8 cells of 512×512, row 7 = top)

| Theme (tileTypeId) | Base cell | Border cell |
| --- | --- | --- |
| grass | (0,7) | (1,7) |
| sand | (2,7) | (3,7) |
| dirt | (0,6) | (1,6) |
| stone | (2,6) | (3,6) |
| snow | (0,5) | (1,5) |
| swamp | (2,5) | (3,5) |
| rock_cliff | (2,4) | (3,4) |
| road | (0,3) | (1,3) |
| footpath | (2,3) | (3,3) |

Cells below row 3 are unused (dark brown).

### Materials / textures

- `Moyva_Atlas.mat` (URP Lit): `_BaseMap`/`_MainTex` → `Moyva_AlbedoAtlas.png`
  (guid 1da97433…), `_BumpMap` → `Moyva_NormalAtlas.png` (bcf7a6ca…),
  `_MetallicGlossMap` → `Moyva_MetallicSmoothnessAtlas.png` (12103a27…).
  `_SmoothnessTextureChannel` = 0 → smoothness came from the metallic map
  alpha; without the map the scalar `_Smoothness` (=1) applies and must be
  lowered.
- `Moyva_RoughnessAtlas.png` (79e1c41c…): referenced by nothing — orphaned.
- The three maps are referenced only by `Moyva_Atlas.mat`.
- Water renders through separate `SurfaceOnly` sheets + water material;
  shoreline fragments use the `sand` theme (`TwcTileMeshSourceProvider` ~L95)
  — this already keeps sand localised; no change needed.
- Trees are sprite billboards (`m_Sprite`) — bark has no 3-D trunk target
  there; `veg-log`, `veg-twig-a/b` prefabs use `VegFlat.mat` and are the
  valid bark wiring target.
- `MoyvaAtlasPackImporter` (Editor) loads the 3 maps that will be deleted —
  must be updated so re-running does not resurrect them.

### Smoothing inventory

Authored mesh normals are already faceted (verified: `grass_edge` vertex
normals ≈ face normals). No subdivision/bevel/weighted-normal geometry is
authored in the pack. Actual smoothing sites are runtime `RecalculateNormals`
calls:

| Site | Context | Effect |
| --- | --- | --- |
| `ChunkTerrainMeshBuilder.cs:~745` | border-clamped copy | **destroys authored faceted normals** — real defect |
| `ChunkTerrainMeshBuilder.cs:~920` | combine fallback when mesh has no normal attribute | smooths entire chunk (fallback path) |
| `TileVerticalFillMeshUtility.cs:~332` | boundary-edge skirt (verts unshared per quad) | already faceted; call is cosmetic but flagged by audit |
| `TileVerticalFillMeshUtility.cs:~1245` | UV-remap result when source lacks normals | smooth normals on generated geometry |
| `TileVerticalFillMeshUtility.cs:~1347` | axis-aligned closure skirt (unshared verts) | same as above |
| `TileWorldCreatorTerrainSideWallMeshBuilder.cs:~40` | generated side walls (4 unshared verts/edge) | same as above |

## 2. Texture mapping (old theme → new texture)

| Theme | Base cell | Border cell composition |
| --- | --- | --- |
| grass | Grass | Grass lip → Earth soil → Rock face |
| sand | Sand | Sand lip → Earth soil → Rock face |
| dirt | Earth | Earth → Earth(darker) → Rock face |
| stone | Rock | Rock rim → Rock face |
| snow | Snow | Snow → Rock face (old pack had no dirt band) |
| swamp | ForestFloor | ForestFloor lip → Earth soil → Rock face |
| rock_cliff | Rock | Rock rim → Rock face |
| road | Rock (closest to paving) | Earth rim → Rock face |
| footpath | Earth (exposed-soil path) | Earth rim → Rock face |
| bark | — | new `Moyva_Bark.mat` for veg-log / veg-twig trunks |

New atlas is written **in place** into `Moyva_AlbedoAtlas.png`, preserving
the GUID so every existing material/importer reference keeps working.
Source PNGs are imported under `AtlasV3/Textures/Sources/` for provenance.
Normal / metallic-smoothness / roughness atlases: references removed from
the material, files deleted (roughness already orphaned).

## 3. Implementation steps

1. Copy `T_MOYVA_*.png` → `Assets/Moyva/Art/World/Tiles/AtlasV3/Textures/Sources/`
   with import metas (sRGB, mips, Repeat wrap, bilinear+, aniso 2, no R/W).
2. Python rebuild of `Moyva_AlbedoAtlas.png` (in place): base cells =
   resized theme texture; border cells = banded composite with soft row
   blends; unused cells keep the old dark-brown fill.
3. `Moyva_Atlas.mat`: drop `_BumpMap`/`_MetallicGlossMap`, `_Smoothness`
   → matte (~0.12), clear `_NORMALMAP`-style keywords.
4. Delete the 3 orphan atlas maps + metas; update `MoyvaAtlasPackImporter`
   to build the material without them (albedo-only).
5. Smoothing removal: small `FacetNormals` helper; border clamp copies
   authored normals instead of recalculating; skirts/closure/side-wall
   builders assign per-face normals; combine fallback facet-normal fill.
6. `Moyva_Bark.mat` (URP Lit + bark texture) wired into
   `veg-log.prefab`, `veg-twig-a/b.prefab`.
7. Verification: `smoke_compile.py`, focused + full EditMode suites,
   seeded smoke (42/777/12345), visual screenshots (flat tile, 4-tile
   junction, mixed tiles, shoreline, height step, chunk border, near/far
   cameras) via `WorldVisualSmoke`.
8. Self-review + `TWC_DUAL_TILES_TEXTURE_MIGRATION_REPORT.md` +
   `WORK_STATE.md` update.

## 4. Acceptance checklist

- [x] No `RecalculateNormals` on tile/generated meshes remains (facet path only).
- [x] Chunk mesh normals preserve authored faceted shading after clamp/combine.
- [x] All 9 themes sample the new textures; UV layout unchanged.
- [x] Mixed-theme quads still split complementary fragments (half textures).
- [x] No pink materials, stretched UVs, bleeding, or z-fighting.
- [x] Deterministic seeds reproduce identical chunk borders.
- [x] Importer re-run produces no deleted-map references.

Status: complete — see
`docs/plans/TWC_DUAL_TILES_TEXTURE_MIGRATION_REPORT.md`.
