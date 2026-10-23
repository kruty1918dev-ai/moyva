# com.kruty1918.motion

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.motion?label=version&sort=semver)

Reusable motion primitives extracted from Moyva: `MotionEaseKind`/`MotionEaseEvaluator`,
`EntityMotion` (slot-based position/yaw/scale/punch/custom motions on a
MonoBehaviour), `PathTraversalMotion` (waypoint traversal).

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.motion.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.motion": "https://github.com/kruty1918dev-ai/com.kruty1918.motion.git#v0.1.0"
```

## API surface

| Type | Purpose |
|---|---|
| `MotionEaseKind` / evaluator | Named easing curves evaluated without DOTween |
| `EntityMotion` | Slot-based motion component — concurrent position/yaw/scale/punch/custom channels |
| `PathTraversalMotion` | Waypoint/path traversal for world entities |

## Model

No game-specific dependencies; compose via your own installer/DI. Slots let a
unit play "walk to tile" and "hit-punch" simultaneously without overriding
each other.

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
