# com.kruty1918.connectivity

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.connectivity?label=version&sort=semver)

Reusable connectivity monitoring: internet reachability probes, online/offline
status events, and wait-for-connectivity helpers.

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.connectivity.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.connectivity": "https://github.com/kruty1918dev-ai/com.kruty1918.connectivity.git#v0.1.0"
```

## API surface

| Type | Purpose |
|---|---|
| `IConnectivityService` | Reachability probes, online/offline status events, `await`-able wait-for-connectivity helpers |

## Model

Reusable UPM package extracted from Moyva. No game-specific dependencies;
compose via your own installer/DI. Useful for gating online features
(lobby/relay flows, telemetry upload) on real connectivity rather than
`Application.internetReachability` alone.

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
