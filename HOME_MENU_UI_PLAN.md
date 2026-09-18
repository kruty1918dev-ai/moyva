# Home Menu UI/UX Overhaul — Plan

Base: `game-process @ 387a07ef84996dbbc7f9c09038a1be193b27b8f4`
Work branch: `ui/home-menu-overhaul` (worktree `../moyva-home-menu-ui`)

## Existing architecture

- `HomeMenuInstaller` (`UseDynamicMoyvaUi = true`) binds `HomeMenuMoyvaUiViewController`
  as the single implementation of ~14 UI interfaces. Services drive it; it never
  renders UGUI itself.
- `HomeMenuMoyvaUiPresenter` (ITickable) builds full markup via
  `HomeMenuMoyvaUiMarkup.Build(state, view)` and mounts through `IUnityHtmlHost`.
  `UnityHtmlDocumentTree.Update` **reconciles** nodes by tag + `id`/`data-key`:
  existing UGUI objects are preserved, only attribute/text/child diffs apply.
- `HomeMenuMoyvaUiBridge` translates `onClick`/input callbacks from the HTML
  runtime into navigation/settings/lobby actions.
- `HomeMenuMoyvaUiState` tracks route, settings section, play flow, dirty flag.
- `UnityHtmlMotionBridge` (DOTween) already implements declarative
  `data-motion` presets (fade, slide-*, scale, pulse) played after document
  update, with dedupe by signature, `SetLink(KillOnDestroy)`, unscaled time.
  GameplayHUD markup already uses it — it is the canonical motion layer.
- Multiplayer: `MultiplayerPanelService` (mode select, create/join entry),
  `CreateRoomPanelService` (draft + validation), `WorldSetupPanelService`
  (world draft), `JoinRoomPanelService` (refresh/join pipeline with
  rate-limit + idempotency + cancellation), `LobbyPanelService` (lobby +
  start game), `KickPlayerPanelService`, `PasswordPanelService`,
  `GameStartListenerService`, `ILobbyHostMigrationService`,
  `RoomCapabilityFlags` on `LobbyRoom`.
- Overlays: `IOverlayLoader` → `OverlayLoaderResult` (static `Current` +
  `CurrentChanged`) rendered as modal by the view controller.
- Legacy UGUI panel set still exists; only needs to stay compilable.

## Findings — technical problems

1. **Host controls unreachable**: nothing navigates to `KickPlayerPanel`.
   Kick/transfer-host backends exist (`KickPlayerPanelService`,
   `ILobbyHostMigrationService`, `RoomCapabilityFlags`) but have no UI path.
2. **No ESC/back key** in the menu: `IUiEscapeRouter` is only consumed by
   gameplay. `bridge.Back()` exists but is only reachable via on-screen BACK.
3. **Overlay shows fake "%" progress** (`0%`, `100%`) — no stage text.
   `OverlayLoaderResult` has no status field.
4. **Join entry blocks navigation**: `MultiplayerPanelService.OnJoinRoomClicked`
   awaits provider switch + full room query behind a "%" overlay *before*
   opening the join panel — feels like a hang (up to ~5s timeout).
5. **No inline validation feedback**: create-room invalid state only disables
   NEXT silently; seed input silently ignores malformed values.
6. **Lobby panel is minimal**: invite code + `P1..Pn` + START/LEAVE. No room
   name, host/YOU badges, provider/privacy, world summary, readiness reason,
   copy-invite action.
7. `LobbyUserInfo` carries only name+index id — no host/local identity.
8. `CreateRoomPanelService.ApplySelectedProviderInBackground` is `async void`
   fire-and-forget without busy/error surfacing on the UI.
9. `Presenter.Tick` calls `anchor.ApplyViewportLayoutNow()` every frame
   unconditionally (canvas scaler + safe-rect math); the anchor has its own
   signature guard only in `Update`.
10. Fallback path disables MoyvaUI *and* legacy UI → blank screen if mount
    fails (rare but leaves no usable UI).
11. `HomeMenuMoyvaUiViewController` is ~1050 lines implementing 14 interfaces —
    needs a partial-class split for maintainability (low risk, same identity).
12. Motions: declared `data-motion` gives enter animations for free (new
    elements animate on creation), but there is **no exit transition** — old
    subtree is destroyed on reconcile. Needs a coordinated fade-out.

## Findings — UX problems

- Main menu has no profile/nickname element; nickname is buried in
  Settings→General.
- Busy states are a bare spinner with a meaningless percent.
- Room browser: no loading/empty/error distinction inside the panel; refresh
  blocks entry; long names unbounded.
- Settings sections are dense tables without explanations or current-value
  emphasis; destructive action (Delete Saves) not visually separated enough.
- Controls screen shows everything at once (~30+ equal-weight controls):
  device picker, visual keyboard/mouse/gamepad/phone, bindings, sensitivity —
  cognitive overload. Needs progressive disclosure: profile summary →
  essential bindings → device editor → advanced.
- Lobby start button: no reason shown when disabled.
- Modal/scrim/list/button transitions absent.

## Performance risks

- Full-markup `Build` (≈20–30 KB string) + `XmlDocument` parse on every dirty
  state. Acceptable per interaction, but redundant `MarkDirty` calls
  (InvokeButton, RefreshUserList) trigger extra parses — tighten dirty calls.
- `ApplyViewportLayoutNow` every frame — gate on layout signature.
- No `FindObjects` in hot paths observed; `PlayerName` fallback does
  `FindObjectsByType<AuthenticationService>` only when playerNameService is
  null — leave as is.

## Target UX

- Route containers, modals, lists animate via `data-motion` + a coordinated
  short fade-out on route exit (presenter-owned, unscaled, cancellable,
  skipped under Reduced Motion).
- Every async action: IDLE → BUSY (stage text + indeterminate indicator) →
  SUCCESS/ERROR with the UI restored interactable.
- Main menu: compact profile chip (avatar initial + nickname + quick edit
  inline input or jump to General).
- Create flow: inline validation, stage messages, summary in world setup.
- Join flow: panel opens instantly; list area shows Fetching/Empty/Error/
  Joining states; invite-code path preserved.
- Lobby: dashboard with room meta, player cards (HOST/YOU), invite copy with
  feedback, world summary, readiness reason, HOST CONTROLS section
  (Manage Players → kick panel, Transfer Host where capability-backed).
- Settings: section tabs kept; per-row descriptions; value readouts; danger
  zone for delete-saves; Reduced Motion toggle.
- Controls: progressive disclosure — active profile card, essential bindings
  table, then device-specific editor + advanced options.
- ESC: unfocus input → cancel capture → navigate back.

## Patch sequence

1. Motion layer: extend `UnityHtmlMotionBridge` (fade-out preset, looped
   spinner preset), `data-motion` attributes in markup, exit-transition
   coordination + ESC handling + viewport guard in presenter. Reduced-motion
   support in state/settings.
2. Overlay status: `OverlayLoaderResult.Status`/`SetStatus`, markup redesign
   (title + stage + spinner), stage texts in init/create/join/start services.
3. Main menu + profile chip + settings-section polish (labels, hints, values,
   danger zone, Reduced Motion).
4. Controls markup restructure (progressive disclosure, no feature loss).
5. Create/join UX: inline validation, `IRoomListStatusView` status surface,
   immediate navigation on JOIN, stage texts, invite-code preservation.
6. Lobby dashboard + host controls + copy invite + readiness reason +
   transfer-host where supported; wire KickPlayerPanel navigation.
7. `HomeMenuMoyvaUiViewController` partial-class split (Modals/Settings/Lobby).
8. CSS polish + responsive pass (vp-compact/vp-tiny/vp-portrait preserved).
9. Tests: join/create/lobby-state/status/transition unit tests; compile;
   EditMode suite; diff review.

## Tests

- Existing: `JoinRejectionTests`, `RelayLobbyPresentationTests`,
  `HumanVsBotTests`, `MultiplayerStartupBarrierTests`,
  `StartingPositionSelectorTests`.
- New targeted EditMode tests (plain C# seams, no Unity scene):
  overlay status propagation, room-list status transitions, lobby view model
  badges/readiness reason, invite-copy feedback flag, transition reducer
  (reduced-motion zero durations), markup smoke (Build produces valid XML for
  every route/section) — validates markup without a scene.
- Runner: `tools/ai/unity-editmode-tests-quiet.sh` with `UNITY_BIN` override.

## Definition of Done

- Compile clean; focused + broader EditMode tests pass (baseline failures
  recorded separately).
- All user journeys reachable on-screen incl. kick/host controls; ESC works;
  busy/error states everywhere; no duplicate ops on rapid clicks; async
  callbacks safe across navigation/dispose.
- Animations use the central DOTween bridge; no leaks/stacking; unscaled;
  Reduced Motion honored.
- Responsive classes intact; long strings truncated safely.
- Diff contains only Home Menu scope; branch pushed; PR → `game-process`,
  not merged.
