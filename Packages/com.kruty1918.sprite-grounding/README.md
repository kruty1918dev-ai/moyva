# Kruty1918 Sprite Grounding

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.sprite-grounding?label=version&sort=semver)

Reusable runtime API for alpha-carded sprites and quad-card meshes:

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.sprite-grounding.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.sprite-grounding": "https://github.com/kruty1918dev-ai/com.kruty1918.sprite-grounding.git#v0.1.0"
```

## Features

- **Visible bounds** — `AlphaBoundsAnalyzer` scans alpha and returns the
  opaque pixel/UV rect (alpha-clip threshold semantics).
- **Support point** — `SupportPointResolver` locates the ground-contact
  point (bottom-centre or lowest-band centroid) in UV space.
- **Card projection** — `QuadCardProjector` maps UV-space results onto
  quad-card meshes (cross/clump cards) via bilinear UV→local mapping.
- **Trimmed representation** — `TrimmedSpriteFactory` (sprite rect/pivot)
  and `CardMeshTrimmer` (bounds-refit or UV-trimmed mesh copies; source
  assets are never mutated).
- **Grounded pose** — `GroundPlacementSolver` turns sprite metrics into a
  world-space position relative to a caller-supplied ground surface.

Pixel input is abstracted behind `IPixelSource` (`ArrayPixelSource`,
`TexturePixelSource`, `SpritePixelSource`, `RenderTexturePixelSource` for
non-readable textures). Results are pure functions of pixels + profile;
`SpriteGroundingCache` and `SpriteGroundingBatch` provide keying,
dedup and budgeted pre-pass processing.

The package has no dependencies on Zenject, TileWorldCreator or any
game-domain types.

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
