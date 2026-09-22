# com.kruty1918.audio

Reusable audio runtime extracted from Moyva.

- `IAudioService`/`AudioService`: key-driven playback over an AudioMixer with buses, named channels, ducking, pooling, per-scene overrides. Catalog arrives via `IAudioCatalog` + `IAudioSceneOverrides` (implemented by host config assets).
- `IMusicService`/`MusicService`: scene-matched music profiles (`IMusicSceneProfile`) with crossfade.
- Contracts (`AudioBus`, `AudioSoundDefinition`, `AudioPlayOptions`, `AudioHandle`, …) are game-agnostic.

Lifecycle (`Initialize`/`Tick`/`Dispose`) is driven by the host composition root.
