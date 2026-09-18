# Moyva Marketing Content Studio

Autonomous marketing-content authoring and capture environment inside the
Unity project. Lives on the long-lived tooling branch
`tooling/marketing-content-studio` — **never merged into `game-process`**,
periodically synced *from* it.

## Checkout

```bash
git fetch origin
git checkout tooling/marketing-content-studio
```

## Syncing new game content

The branch intentionally stays separate. To pull in new gameplay work:

```bash
git fetch origin
git checkout tooling/marketing-content-studio
git merge origin/game-process
```

Resolve any conflicts, then open the project. The Studio re-indexes content
automatically — a fingerprint over scanned presets/GUIDs detects added or
removed units, buildings, audio, and animations. You can also rebuild manually
from the Studio window ("Rebuild Content Index") or:

```
Unity -batchmode -projectPath <repo> \
  -executeMethod Kruty1918.Moyva.Marketing.EditorTools.MarketingCli.BuildContentIndex -quit
```

The Studio never runs git operations itself.

## Opening the Studio

- **Window:** Unity menu → `Moyva → Marketing Content Studio`
- **Scene:** `Assets/Moyva/Scenes/MarketingStudio.unity`
  (auto-created/repaired by `Moyva → Marketing → Create or Repair Studio Scene`)

The scene is a thin capture environment. On Play it:

1. loads the recipe (or the request handed off by the window/CLI),
2. configures `GameLaunchContext` with the recipe's `worldSeed`,
3. loads `Gamplay_Scene` additively — the world is built by the canonical
   bootstrap, so every frame shows a valid game state,
4. scans the world for subjects, optionally stages real units via
   `IUnitFactory` (never fake transforms, never impossible state),
5. plans shots (`ShotPlanner` + `TrailerBeatLibrary`),
6. suspends the gameplay camera, applies the lighting style, sets UI
   visibility per recipe,
7. captures via render-to-texture (stills) or Unity Recorder (video /
   image sequence / audio),
8. writes output + `manifest.json`, then exits play mode when driven by the
   window or CLI.

`Auto Start Capture` on the `MarketingStudio` object (or in the window):
ON = full pipeline on Play; OFF = the world is staged for manual inspection
only.

## Generating content

Window buttons:

- `GENERATE` — run the selected recipe.
- `GENERATE STEAM SCREENSHOTS / TRAILER (60s) / TEASER (15s) / SHORTS (30s)` —
  one-click built-ins.
- `GENERATE SOCIAL PACK` — shorts + instagram post + hero stills.
- `GENERATE CAMPAIGN PACK` — steam + gameplay screenshots + hero stills +
  60s/30s trailers + teaser + shorts + thumbnail, sequentially.
- `CANCEL GENERATION` — safe stop (Recorder stopped, cameras/lighting/UI
  restored, completed files kept, cancelled manifest written). ESC works in
  play mode too.

CLI / CI:

```bash
Unity -projectPath <repo> \
  -executeMethod Kruty1918.Moyva.Marketing.EditorTools.MarketingCli.GenerateRecipe \
  -moyvaMarketingRecipe steam-screenshots [-moyvaMarketingOutput <dir>] -quit
```

(`-batchmode` without `-nographics` so the GameView/camera input exists for
Recorder.)

## Output

```
MarketingOutput/<yyyy-MM-dd_HHmmss>_<recipe>/
├── manifest.json                  # seeds, commits, files, validation, shots
├── Master/                        # clean masters (video / stills / sequence)
├── <Platform>/                    # per-platform variants (crop applied)
├── Metadata/{shots.json, …}
├── .temp/                         # cleaned on success
└── ContactSheet.png               # review sheet
```

`MarketingOutput/` is git-ignored. The manifest records
`sourceGameProcessSha` and `toolBranchSha` so every asset is traceable to the
game content it was captured from. Fixed seeds in a recipe reproduce a run.

## Recipes

`Assets/Moyva/Presets/Marketing/recipes/*.json` — content type, platform
profile, output kind, resolution/fps/duration/shot count, UI visibility,
logo/text/SFX/music flags, camera + lighting style, scenario strategy,
staging flag, language, seeds, naming. Built-ins:

| Recipe | Purpose |
|---|---|
| `steam-screenshots` | 10 gameplay shots, UI on, text/logo OFF, staging OFF (Steam rules) |
| `gameplay-screenshots` | 8 clean gameplay shots, UI hidden |
| `hero-stills` | 5 cinematic hero frames, golden hour, text variants |
| `trailer-60` / `trailer-30` | narrated-structure trailers w/ audio |
| `teaser-15` | 15s hook→action→end-card |
| `shorts-30` | 9:16 vertical, portrait-safe framing |
| `youtube-thumbnail` | 3 composed thumbnail stills |
| `master-sequence` | 20s PNG image sequence (editorial-grade) |

## Platform profiles

`Presets/Marketing/platforms/*.json`: `generic`, `steam-screenshot`,
`steam-capsule`, `youtube-16x9`, `youtube-shorts-9x16`, `tiktok-9x16`,
`instagram-reel-9x16`, `instagram-post-1x1`, `instagram-post-4x5`,
`appstore`, `google-play`. Compliance is enforced before export — e.g. a
Steam gameplay screenshot can never carry marketing text or a logo; a
non-compliant recipe is corrected into a compliant clean variant instead of
exporting garbage.

## Content auto-discovery

`MarketingContentIndexBuilder` scans `Presets/Units`, `Presets/Buildings`,
`Presets/Tiles`, `Presets/Generator/map-object-registry` and the audio
registry, probes prefabs (renderers, materials, animator/clips, bounds),
classifies marketing value/size, applies manual overrides from
`Presets/Marketing/metadata/*.json` (`pin`/`ban`/`prefer`/`avoid`, tier,
biome, shot suitability, scoreBias), and writes a snapshot + fingerprint to
`MarketingOutput/.cache/content-index.json`. New content from merged
`game-process` work is picked up automatically on the next build.

## Art direction & copy

- Copy comes only from `Presets/Marketing/claims.json` claims whose
  `requires` predicates match detected game features (combat, ranged, naval,
  economy, defense, settlement). Cliché phrases are hard-banned.
  Localizations: `en`, `uk`, `de`.
- Trailer structure follows the HOOK→WORLD→BUILD→EXPAND→TENSION→ACTION→
  CLIMAX→END arc, scaled to duration; restraint rules keep cuts ≥ ~2s and
  transitions mostly plain cuts.
- Lighting presets: Dawn/Morning/Day/GoldenHour/Dusk/Overcast — render
  quality is raised, game assets are never swapped.

## Troubleshooting

- **"recorder-unavailable" in manifest** — the run was started by pressing
  Play directly in the scene; the Recorder backend only exists for runs
  launched from the Studio window or CLI. Still capture still works.
- **World build timeout** — check the Console for gameplay bootstrap errors;
  the studio waits ≤ `worldBuildTimeout` for grid + units.
- **Stale index warning** — run "Rebuild Content Index" or let the next run
  rebuild it (the fingerprint check is automatic on window runs).
- **Batch mode video** — do not pass `-nographics`; Recorder needs a render
  target.

## Files

- Runtime: `Assets/Moyva/Scripts/Features/Marketing/`
- Editor: `Assets/Moyva/Editor/MarketingStudio/`
- Scene: `Assets/Moyva/Scenes/MarketingStudio.unity`
- Presets: `Assets/Moyva/Presets/Marketing/`
- Tests: `Assets/Moyva/Tests/EditMode/Marketing/`
- Plan: `MARKETING_CONTENT_STUDIO_PLAN.md`
