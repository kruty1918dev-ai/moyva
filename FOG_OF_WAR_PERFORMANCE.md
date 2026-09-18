# Fog of War — Performance Notes

Budget targets: large procedural maps, mobile-class hardware, per-owner
grids for every participant.

## Incremental updates

- Tile visibility is counter-based (`FogStateGrid.IncrementVisible` /
  `DecrementVisible`); moving a unit touches only its own tile set — O(range²)
  per move, never a full-grid pass.
- Visual deltas go through `FogVisualDirtyBuffer`; only changed cells reach
  the visual updaters. `FlushVisual` is a no-op when nothing changed.
- `RecalculateAllVisibility` runs only on local-perspective owner change,
  resize, or snapshot load — O(sources × range²).

## Per-owner grids

- Each owner grid is a separate `FogStateGrid` sized `w×h`. Memory is
  `owners × w × h` cells; on a 256×256 map with 4 owners that is ~256 KiB of
  counters — acceptable.
- Owner visibility notifications are batched: `CellsBecameVisible` fires once
  per mutation batch per owner with a deduplicated `HashSet<Vector2Int>`, not
  per tile. Listeners (intel store, MP reconcile) do O(cells) work per batch.

## Intel store

- Truth indexes are flat dictionaries by unit id / cell; per-owner intel adds
  `Units`, `Buildings`, and a `UnitsByCell` index.
- Signal handlers are O(known owners) for evaluations and O(1) lookups for
  per-cell checks; reconcile iterates only the revealed cell set.
- Reader APIs return clones — allocation happens on read, never in the
  per-frame path unless a caller polls every frame (ghost presenter reads
  only when `IntelChanged` or fog `Version` bumps).
- Replicated snapshots write counts-bounded sections with hard caps
  (`MaxUnitSnapshotCount`, payload byte caps) to bound late-join cost.

## Ghost presentation

- Pooled `SpriteRenderer` markers under one root, capped at 512, zero
  per-frame allocation; rebuild only on `IntelChanged` or fog version change.
- Sprites share a single generated texture; no prefab instantiation, no
  gameplay components.

## Culling

- `FogRendererCullingEvaluator` samples grid cells under renderer bounds with
  padding; cost is O(footprint cells) per candidate renderer, evaluated only
  when fog version changes.

## Height-aware vision

- LOS/elevation work stays inside `HeightAwareVisionService` /
  `IFogVisibilityResolver`; vision-source tile sets are computed once per
  source mutation and cached in `_unitVisibleTiles` / owner `VisibleTiles`.
