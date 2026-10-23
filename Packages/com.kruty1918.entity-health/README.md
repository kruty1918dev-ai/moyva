# com.kruty1918.entity-health

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.entity-health?label=version&sort=semver)

Reusable entity-health primitives extracted from Moyva: `IHealth` (hp/destroyed/events
contract), `HealthComponent` (MonoBehaviour impl), `IHealthRegistry`/`HealthRegistry`
(entityId keyed lookup). Damage formulas, death/capture rules and network
authority stay host-side.

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.entity-health.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.entity-health": "https://github.com/kruty1918dev-ai/com.kruty1918.entity-health.git#v0.1.0"
```

## API surface

| Type | Purpose |
|---|---|
| `IHealth` | Hit-points contract: current/max, damaged/healed/destroyed events |
| `HealthComponent` | `IHealth` MonoBehaviour implementation for scene entities |
| `IHealthRegistry` / `HealthRegistry` | Keyed `entityId → IHealth` lookup |

## Model

The package deliberately stops at primitives: damage formulas, death/capture
rules and network authority are host-game concerns.

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
