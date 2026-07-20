# Construction placement diagnostics and unique castle relocation

## Problem

A castle is configured as one building per player, but its modules use `PerBuilding`
scope rather than a global singleton scope. The old preview flow therefore treated the
first pending castle as a normal placement. A second click was validated as a second
copy and rejected by the per-player limit instead of moving the preview.

The build-grid query and the authoritative click path could also use different pending
preview assumptions, producing a green grid cell that rejected the click.

## Runtime behaviour

`BuildingDefinitionCapabilities.GetPlacementUniquenessScope` now separates:

- `Global`: existing global singleton modules.
- `PerOwner`: a castle or town hall whose per-player limit is exactly one.
- `None`: normal buildings, including generic buildings that merely have a one-copy cap.

Only explicit preview/grid requests may enable automatic unique relocation. Direct
placement, network-authoritative placement and unrelated queries remain strict.

When a movable unique building is selected:

1. An existing pending preview is moved to the newly clicked cell.
2. Otherwise, the current owner's placed instance becomes a relocation preview.
3. Another owner's castle is never selected as the relocation source.
4. Confirmation still revalidates occupancy, terrain, fog, influence and ownership.

## Diagnostics

Action-level placement validation emits a stable one-line record:

```text
[MoyvaPlacementAttempt] id=17 source=PointerClick result=blocked building='castle-01' owner='player_0' origin=(12, 8) context=(12, 8) code='terrain' reason='Footprint contains a terrain cell blocked for construction.' tile='grass' terrainLevel=1 terrainReason='edge terrain tile' fog=Visible limit=1 existing=0 pending=0
```

The record contains:

- caller/source and monotonically increasing attempt ID;
- building, owner, requested origin and actual blocker cell;
- stable reason code and human-readable reason;
- tile ID, terrain level, concrete terrain rule and fog state;
- per-player limit counts;
- ignored pending/occupied origins used for relocation;
- all available placement blockers.

High-frequency whole-map grid filtering does not allocate full diagnostic snapshots.
Pointer clicks, moves, confirmation and direct placement do.

## Regression coverage

The tests verify:

- a second castle click moves the only pending castle preview;
- the movable grid query ignores that preview while a strict query still rejects a
  second copy;
- per-owner relocation cannot move another faction's castle;
- direct placement remains strict;
- diagnostic formatting preserves stable reason codes and blocker context.
