# Moyva UnityHTML

UnityHTML is Moyva's thin runtime wrapper around ReactUnity UGUI. It mounts HTML and CSS stored as Unity `TextAsset` files into an existing `RectTransform`.

CSS presentation assets should be authored as plain CSS text with a Unity text extension, for example `HomeMenuShell.css.txt`. This keeps the file readable while ensuring Unity imports it as a populated `TextAsset`.

## Runtime Lifecycle

Create a `UnityHtmlDocument` from HTML and optional CSS text, then mount it through `IUnityHtmlHost`. The host creates a ReactUnity `UGUIContext`, injects a `GlobalRecord`, selects QuickJS, inserts CSS before `Start()`, calculates layout, and disposes the old context on remount, `Unmount()`, `Dispose()`, mount failure, or scene unload through the owning presenter.

```csharp
var document = UnityHtmlDocument.FromTextAssets(htmlAsset, cssAsset, "HomeMenuShell");
var result = host.Mount(root, document, new Dictionary<string, object>
{
    ["moyvaMenu"] = bridge
});
```

## Globals

Globals are an explicit allow-list of C# objects exposed to QuickJS. Keep bridge objects narrow and route actions through existing application services. Do not expose gameplay stores or mutable domain services directly to HTML.

## Motion

Runtime motion is declared in HTML and executed by DOTween after layout. Every animated element needs a stable `id`:

```html
<view id="inventory" data-motion="slide-left" data-motion-duration="0.16" data-motion-ease="out-cubic"></view>
```

Supported presets are `fade`, `fade-out`, `slide-left`, `slide-right`, `slide-up`, `slide-down`, `scale`, and `pulse`. Optional attributes are `data-motion-delay`, `data-motion-distance`, and `data-motion-ease`. Imperative feedback can call `Globals.motion.Play('inventory', 'pulse', 0.12, 0)` or `Globals.motion.Stop('inventory')`. Motion changes only opacity, anchored position, or scale and never triggers an HTML render.

### Motion roles

Prefer `data-motion-role` over hand-tuned attributes — it applies the shared policy so new UI animates consistently without copying values:

```html
<view id="side-panel" data-motion-role="panel"></view>
```

Roles: `panel`, `dialog`, `scrim`, `toast`, `edge-top`, `edge-bottom`, `none`. Each role owns an entry preset, an exit preset, and duration/distance/easing tokens (`UnityHtmlMotionPolicy`). Any `data-motion`, `data-motion-duration`, `data-motion-delay`, `data-motion-distance`, or `data-motion-ease` attribute overrides the role locally. `data-motion="exit"` plays the role's exit preset — closable surfaces stay mounted with that attribute until their close window elapses. `data-motion="none"` or `data-motion-role="none"` disables motion for the element.

`IUnityHtmlMotion.ExitFinished` fires exactly once per declarative exit — whether the tween completes, is cancelled by a replacement motion, or is torn down mid-exit — so a close flow can settle on the callback instead of guessing a fixed duration.

## Scrolling

`<scroll>` mounts `MoyvaSmoothScrollRect`, which completes any in-flight wheel animation to its target before applying the next delta — continuous wheel streams accumulate instead of collapsing to one step, and abrupt reversals move in the new direction immediately rather than drifting against the input.

`IUnityHtmlHost.ScrollSettings` (`UnityHtmlScrollSettings`) carries the per-host scroll configuration: `WheelSensitivity` (multiplier on the control's own sensitivity baseline), `Inertia`, `DecelerationRate`, `Smoothness` (seconds of easing per wheel step), and `ReducedMotion` (snaps to the final position — no easing, no inertia — and settles an in-flight animation). Settings apply to every mounted scroll control on mount, on regional updates, and when the property is reassigned; scroll position and velocity stay per control and are preserved only for controls that survive the update.

## Requirements

ReactUnity Core and QuickJS stay as commit-pinned UPM git dependencies. Unity must be launched from an environment where `git` is on `PATH`, otherwise Package Manager cannot resolve `com.reactunity.core` or `com.reactunity.quickjs`. Node, npm, and TypeScript are not required for runtime HTML/CSS assets.
