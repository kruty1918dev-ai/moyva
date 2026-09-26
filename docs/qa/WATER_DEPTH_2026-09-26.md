# Water depth visibility — implementation report (2026-09-26)

Task: "Вода, крізь яку дно поступово зникає з віддаленням від берега" —
sand readable at the shallows, the bed fading smoothly with real water
column depth until invisible; stable across camera moves; shore
distance must not be approximated by camera distance or a "Horizontal"
property.

## Proven cause of the flat look

Measured the ACTUAL material on rendered chunk meshes at runtime
(new `watermat.txt` dump in `WorldVisualSmoke`):

- Rendered water material was **`StylizedWater3_NintendoStyle`** (the
  ThirdParty package's own .mat) — assigned via
  `materialOverride` on `Assets/Moyva/TileWater.asset`, which every
  water terrain layer in `moyvatileworldcreatoridmapping.json` uses.
- Its `_DepthHorizontal = 0.01` → the water-column attenuation
  `1−exp(−column·k)` was effectively disabled; only the view-depth term
  produced any variation → nearly uniform water over the whole body.
- The legacy `WaterLayerMaterialSettings`/`WaterLayerMaterialApplier`
  path writes a custom pixel-shader property set and is injected but
  never applied — dormant; not a water API for the active material.
- `WaterMaterial.mat` (Moyva-owned, same SW3 Standard shader) existed
  and was already referenced by `Gamplay_Scene.unity` +
  `PlanarReflectionRenderer` — the scene's intended water material.

## Mechanism (stock SW3 3.2.7, no new shader)

`water.fog = max(1−exp(−verticalDepth·_DepthHorizontal),
                1−exp(−viewDepth·_DepthVertical·0.1))`,
`baseColor = lerp(_ShallowColor, _BaseColor, water.fog)`.
`verticalDepth` = vertical distance from the water surface to the
opaque geometry below — i.e. **the real column over the continuous
seabed**, world-space, camera-independent, identical across chunks.
Requires the URP Depth Texture — already enabled on both
`Moyva_RPAsset` and `PC_RPAsset`; `StylizedWaterRenderFeature` present
on both renderers; `_FogSource=0`, `_DisableDepthTexture=0`.

## Changes

- `Assets/Moyva/TileWater.asset`: `materialOverride` →
  `WaterMaterial.mat` (guid 0c2ba448…) — Moyva-owned SW3 material;
  package files untouched.
- `WaterMaterial.mat`: `_DepthHorizontal` 6.12 → **1.0**
  (column fade now spans the real 0.05–2 m bed range instead of
  saturating within ~0.4 m), `_DepthVertical` 9.1 → **4.0**
  (view-ray term stays secondary so the game camera doesn't wash out
  the shallow band).
- `WorldVisualSmoke.cs`: `DumpWaterMaterial` (effective shader,
  keywords, all fog-relevant props, global depth-texture binding) +
  `water_transect_top/low` shots along the shore→deep gradient.

## Verification (seed 6130, same world `worldHash=87D5247B44DA008A`)

- `watermat.txt` at runtime: material=`WaterMaterial`,
  `_FogSource=0`, `_DisableDepthTexture=0`, `_DepthHorizontal=1`,
  `_DepthVertical=4`, `globalDepthTextureBound=True`, queue 3001.
- Acceptance strip: `water_transect_top.png` (ortho) /
  `water_transect_low.png` (perspective) — sandy bed readable at the
  shoreline (column ≤ ~0.3 m, fog ≲ .3, alpha ≈ .4–.55), smooth fade
  through mid (~0.5–1.2 m column), deep centres dark navy with the
  bed invisible (2 m column, fog ≈ .89–.95, alpha ≈ .9+). No hard
  band, no per-cell tiles.
- `topdown.png`: every water body shows rim→deep gradient; no
  chunk-boundary discontinuities (per-pixel world-space computation).
- `detail_shore.png`: restrained cyan shore edge — soft outline, not
  a foam blanket; subtle refraction (0.105) + waves (0.1) kept.
- `game_orbit_w.png` perspective: gradient consistent at orbit angle.
- Smoke PASS, `verts=515861`, `nanVerts=0`, `empty=0`, error log
  empty; no perf delta (material + two scalar edits only).
- `waterstack.txt` confirms the 95 `water`-id tiles are
  `Swamp|SolidTerrain` — terrain tiles, not sheets; correctly excluded
  from the underwater path.

## Regeneration / persistence

Material values are serialized on the .mat — unchanged by world
regeneration; no meshes, objects or materials are created per
generation. `PlanarReflectionRenderer` writes reflection texture +
matrix into the same material at runtime as before.

## Limitations

- No physical mobile device in this environment — the mobile-quality
  path was verified by configuration only (`Mobile_Renderer` has the
  SW3 render feature; `Moyva_RPAsset` requests Depth+Opaque textures).
- `BaseBlockPresetWater.asset` (ThirdParty) still references
  NintendoStyle — only legacy/alternative registries use it; the
  active chunk-first path resolves through `TileWater.asset`.
- Water reads fairly light/sandy over the widest shallow shelves —
  intended for the clear-water style; `_ShallowColor` alpha is the
  tuning knob if it should read wetter.
