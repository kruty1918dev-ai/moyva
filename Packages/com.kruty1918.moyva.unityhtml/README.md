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

## Requirements

ReactUnity Core and QuickJS stay as commit-pinned UPM git dependencies. Unity must be launched from an environment where `git` is on `PATH`, otherwise Package Manager cannot resolve `com.reactunity.core` or `com.reactunity.quickjs`. Node, npm, and TypeScript are not required for runtime HTML/CSS assets.
