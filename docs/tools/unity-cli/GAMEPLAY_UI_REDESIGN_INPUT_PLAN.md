# What the next Gameplay UI redesign pass will derive from this audit

The next pass should not redesign the UI from screenshots alone. It will combine
four evidence layers from the Pass72 report.

## 1. Live scene structure
Determine the real Canvas roots, modes, panel ownership, anchors, layout groups,
serialized references, prefab instances and active/inactive states.

## 2. Runtime contracts
Preserve the existing controller contracts:
- Construction UI selection/action/status flow;
- GameMode enter/exit construction controls;
- Economy aggregate resource display;
- World/Building information panel;
- interaction/selection presentation;
- any local extensions visible in the serialized component audit.

## 3. Existing visual asset library
Reuse project sprites, fonts, UI prefabs and materials where they already fit.
Do not introduce a parallel visual language unless the existing assets are
insufficient.

## 4. Visual evidence
Use GameView + SceneView screenshots to judge hierarchy, spacing, visual weight,
screen occupation and conflicts that serialized data alone cannot show.

## Planned output of the later redesign pass
After analyzing the uploaded Pass72 ZIP, produce a concrete migration plan before
writing the scene:
- target HUD hierarchy;
- normal-mode layout;
- construction-mode layout;
- resources/status/info hierarchy;
- reusable prefab/style inventory;
- responsive/mobile rules;
- list of exact preserved bindings;
- exact objects to move/restyle/create/delete;
- validation checklist;
- safe CLI mutation/save sequence.
