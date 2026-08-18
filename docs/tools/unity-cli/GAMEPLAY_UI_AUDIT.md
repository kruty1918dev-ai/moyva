# Gameplay UI Audit — Pass72

Target scene:

`Assets/Moyva/Scenes/Gamplay_Scene.unity`

The spelling `Gamplay_Scene` is intentional because that is the current scene path in the project build settings.

## Goal

This is the information-gathering pass before any visual redesign. It does not
save or rewrite the Gameplay scene.

The report is designed so a later UI pass can answer:

- What UI already exists and what is missing?
- Which objects are safe to reposition/re-style?
- Which objects are controlled by LayoutGroups?
- Which serialized references and Zenject/UI controllers must be preserved?
- Which sprites, fonts, materials and UI prefabs can be reused?
- Which controls are duplicated, too small, off-screen or blocking raycasts?
- Which UI is shown/hidden by Construction/GameMode/Economy systems?
- Which exact local source differs from the remote `optimaze` baseline?

## Output

One run creates an external report directory:

`~/.local/share/moyva-cli/reports/gameplay-ui/<timestamp>/`

and a convenient upload ZIP in the user's Downloads folder:

`Moyva_GameplayUI_Audit_<timestamp>.zip`

### Primary file

`gameplay_ui_audit.json`

Contains:

- Unity/project/render-pipeline/build-target state;
- exact Gameplay scene hierarchy summary;
- Canvas + CanvasScaler configuration;
- every UI RectTransform and screen-space bounds;
- Images, RawImages, TMP text, Selectables, ScrollRects;
- LayoutGroup/LayoutElement/ContentSizeFitter/AspectRatioFitter state;
- CanvasGroup state;
- serialized fields and Unity-object references for Moyva/Zenject components on UI objects;
- persistent UI event bindings;
- reusable UI prefabs under `Assets/Moyva`;
- available sprite assets and TMP fonts;
- UI materials currently in use;
- relevant source-area indexes;
- heuristic UX/layout issues.

### Visual files

`gameview.png`

Requested through `ScreenCapture.CaptureScreenshot` after focusing the Unity Game View.
The screenshot is best-effort because Unity captures the Game View asynchronously.

`sceneview.png`

Rendered from the current SceneView camera immediately when available.

### Exact local source snapshot

The report ZIP also carries the current local C# files for the Gameplay UI areas:

- Construction UI
- Economy UI
- InfoPanel
- GameMode
- WorldCreation UI
- Interactions

This prevents a later redesign from assuming that the local working tree exactly
matches GitHub.

## Safety

- No scene objects are changed by the audit.
- No scene is saved by the audit.
- If Gameplay is not open, the preparer opens it only when no currently loaded
  scene has unsaved changes.
- Report output is restricted to the external
  `~/.local/share/moyva-cli/reports/gameplay-ui` tree.
- No Git commit/reset/stash is performed.

## Run later without reinstalling

```bash
cd ~/moyva
tools/unity-cli/moyva-gameplay-ui-audit
```

## Custom Pipeline commands

- `moyva-gameplay-ui-prepare`
- `moyva-gameplay-ui-audit`
- `moyva-gameplay-ui-capture`
- `moyva-gameplay-ui-audit-status`
