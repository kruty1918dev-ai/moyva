# Moyva Control Center V4 — multi-monitor training preview

Visual training stays a normal desktop window rather than exclusive fullscreen.

On Linux/X11 the Control Center:

1. reads connected displays with `xrandr`;
2. determines which display currently contains the terminal/Control Center;
3. selects another display when one is available;
4. starts a small watcher before the Unity training player appears;
5. moves the `MoyvaTraining` player to the selected display and maximizes it there;
6. leaves normal OS window controls enabled so it can be minimized, restored and resized.

If only one display exists, that display is used. If automatic placement tools are unavailable, Unity still opens as a normal resizable window and the UI explains that placement could not be automated.

Recommended Linux/X11 helpers:

```bash
sudo apt install -y wmctrl xdotool
```

The training-player build is configured with `PlayerSettings.resizableWindow = true` and `PlayerSettings.runInBackground = true` only for the duration of the dedicated training-player build. The original project PlayerSettings values are restored after the build.

After installing V4, rebuild the training player once:

```bash
./moyva build training --rebuild
```
