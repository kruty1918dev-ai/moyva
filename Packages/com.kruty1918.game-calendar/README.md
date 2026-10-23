# Kruty1918 Game Calendar

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.game-calendar?label=version&sort=semver)

Reusable in-game calendar for Unity: immutable calendar config, day phases,
`GameDateTime` arithmetic, a load → validate → freeze lifecycle, and pluggable
config-store / sync-adapter contracts. Games supply their own default config
and persistence store.

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.game-calendar.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.game-calendar": "https://github.com/kruty1918dev-ai/com.kruty1918.game-calendar.git#v0.1.0"
```

## Layout

| Folder | Contents |
|---|---|
| `Runtime/` | Calendar service, date arithmetic, config lifecycle, contracts |

## API surface

| Type | Purpose |
|---|---|
| `ICalendarService` | Current in-game date/time, day-phase queries, day advancement |
| `ICalendarConfigStore` | Host persistence for the calendar config |
| `ICalendarStateRestorer` | Restores calendar state on load |
| `ICalendarSyncAdapter` | Propagates calendar state (e.g. multiplayer host → clients) |
| `DayPhase` | Named phase of a day (dawn/day/dusk/night-style splits) |
| `GameDateTime` | Value type with calendar arithmetic |

## Model

- Config is immutable after the load → validate → freeze step; runtime code
  reads only the frozen snapshot.
- Host owns where the config comes from (JSON, assets, code) via
  `ICalendarConfigStore` — the package has no asset-type dependency.

## Requirements

Unity 6.x. No external package dependencies.

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
