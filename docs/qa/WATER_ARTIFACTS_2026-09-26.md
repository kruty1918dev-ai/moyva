# Water / underwater-material / geometry-seam artifacts — 2026-09-26

Branch `improvement/moyva-production-polish`. Task `06_Water_Rendering_Artifacts.md`.
Seed 6130 (same world as the seabed/waterfall tasks, `worldHash=87D5247B44DA008A`).

## Method

One seed, factor-by-factor classification:

- `material-audit.csv` — every renderer's `sharedMaterials` vs `subMeshCount`,
  per-slot material name + render queue + shader.
- `waterfall-vfx.csv` — every spawned `wfall_*` instance: position, transform
  scale, **emitter `shape.scale.x`**, particle cap.
- `clay_iso` / `clay_wall` — all material slots on all terrain renderers swapped
  to one opaque grey URP/Lit: removes transparency, refraction, foam and texture
  in a single pass. Whatever remains is geometry; whatever vanishes is shading.
- `wall_closeup` — tallest land-to-land wall (1.5 m at (12,35)→(12,36)).
- `border_water` — water sheet at the map rim.
- `waterfall-vfx` + prior-run `side_n/edge_s/peak` re-shots.

## Symptom → confirmed cause → fix → evidence

| Symptom (ref frame) | Confirmed cause | Disposition | Evidence |
|---|---|---|---|
| Wide wavy sandy bands + turquoise slits (035236) | Per-cell underwater bed columns with open water sides | **Already fixed** — continuous sloping seabed (`c3a14b9a`) | `seabed_detail_deep.png`, `water_transect_*.png` |
| Rectangular patches on vertical water, odd top edge (035314) | Stretched unit quads on the lake sheet material per lower neighbour | **Already fixed** — SW3 curtain fronts (`94babbd3`) | `waterfall_closeup*.png` |
| Dark slits/stripes on tall walls, "stretched surfaces" (034848/034904) | Authored chamfer seams between per-cell cliff slabs + silhouettes of veg cards on tops. Clay pass: no see-through voids, no UV-tear; grooves are authored seams | **Kept** (task rule: preserve authored cracks/seams) | `clay_wall.png`, `clay_iso.png`, `wall_closeup.png` |
| Cyan teardrop/blob at a cliff lip; cyan spray on walls near falls | `WaterfallVfxSpawner` transform-X-squashed the SW3 emitters: Edge shape is 8 m wide in `shape.scale.x`, so `x=0.0875` collapsed it to a blob; Splash shape is 16 m wide and sprayed walls on 1-cell fronts | **Fixed**: transform stays uniform (`vfxScale`), real front width written into `shape.scale.x` (`frontWidth/uniformScale`) | `waterfall-vfx.csv` — uniform `scaleX=0.70`, `shapeScaleX`=1.43/2.86 for 1/2-cell fronts |
| Bright cyan seam tracing the waterline on shore blocks | SW3 intersection foam where the sheet meets geometry | Intended SW3 shore effect — kept | `wall_closeup.png` |
| Cyan specks/plates scattered on dark terrain | Elevated SW3 water sheets over dark pits + plateau rivers — correct rendering, stark palette | Verified not curtains/backfaces; noted as aesthetic limitation | `side_n.png`, `peak.png` |
| Red/green cell contours on some frames | Construction-grid overlays — absent from clean smoke shots | Legit overlays, not corrupted textures | smoke shots |
| Water sheet open side at the map border | Seabed `borderSkirt` brown band seals the bed; sheet terminates at the boundary | Verified correct | `border_water.png` |
| Small props sitting on the water surface | `environment-decoration` rules: `waterAffinity: require/prefer` — aquatic props (waterlily/waterplant) are designed to float | Designed — kept | `border_water.png`, config lines 66-68/214 |
| Transparent water + opaque terrain in ONE combined chunk mesh (task §3) | Slots == submeshes on all 9 renderers. Slot order puts `WaterMaterial` (q3001) **before** `StylizedWater3_Waterfall` (q3000) — the pool sheet overdraws the submerged curtain base, which is the physically correct blend. No sorting defect observed | Verified — no separation needed | `material-audit.csv` |
| Transparent veg cards sorting / opaque-texture | `DecorSharedStylized.shader`: `TransparentCutout`, `Queue=AlphaTest+40`, `ZWrite`, `clip()` — proper opaque alpha-clip path, lands in the opaque texture | Verified correct | shader lines 83-95/327/377 |
| Coplanar flicker between sheet and curtain crest | Crest lip sits +0.02 m above the sheet — no coplanar surfaces | Verified by construction | `waterfall_closeup.png` |
| Rebuild accumulation | `vfxSystems=13` identical across consecutive smokes; per-chunk `Waterfalls` roots cleared on `Spawn()`; identical `worldHash` | Verified | `waterfall-active.txt` |

## Actual code change

`WaterfallVfxSpawner.cs`:
- `SpawnOne` keeps the prefab transform **uniformly** scaled and writes the
  real front width into `shape.scale.x` (compensating the uniform scale), for
  edge foam, impact splashes and mist. Splash/mist no longer scale the
  transform with front width — shape width derives from the pour.
- Front width = `WidthCells * cellSize`; mist shape floored at 2 m.

`WorldVisualSmoke.cs` (editor diagnostics only):
- `DumpArtifacts`: material audit, VFX audit, tallest-wall closeup, border
  shot, clay pass with material restore.

## Checks performed

- Visual smoke `PASS`, `worldHash=87D5247B44DA008A`, `nanVerts=0`,
  `meshes=9`, error log empty.
- `WaterfallFieldPlannerTests` 10/10 after the spawner change.
- Material audit: 9/9 chunk renderers aligned; 3 waterfall-slot chunks.
- Same world, same seed as seabed/waterfall tasks → direct A/B.

## Limits

- VFX are `ParticleSystem`s — PRU shots draw MeshFilters only, so particles
  never appear in `ShotAt` images; VFX verification is the placement/width/cap
  audit + stock-prefab behaviour. An in-engine scene-camera closeup attempt
  rendered empty (preview context) and was removed.
- Elevated water sheets read as bright cyan plates on dark terrain from long
  range — physically correct, but a palette/art-direction note rather than a
  rendering defect.
- No on-device mobile check in this environment; `Mobile_Renderer` retains
  SW3 feature + depth texture (verified earlier).

## Evidence

`docs/qa/evidence/artifacts-seed6130/` — clay pass, wall closeup, border shot,
material/VFX audits, side/edge/peak sweeps, waterfall closeups, transect.
