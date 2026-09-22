# Telemetry — dependency decisions

Direction is strictly one-way:

```
Packages/com.kruty1918.telemetry      (generic, reusable — zero Moyva knowledge)
        ↑
Assets/Moyva/Scripts/Features/Telemetry  (Moyva adapter — only layer that knows both)
```

## Rules enforced

- `com.kruty1918.telemetry` references nothing from `Kruty1918.Moyva.*`,
  `Moyva.*`, Zenject, or gameplay assemblies. Verified: its asmdefs list only
  themselves; `dotnet` core harness compiles it with no Unity/game refs at all.
- The Moyva adapter (`Kruty1918.Moyva.Telemetry`) references the package plus
  Signals / Turns / AI.Bot.Core / Multiplayer / Zenject — adaptation point.
- `Kruty1918.Moyva.Bootstrap` gained one reference + one `Install()` call line.

## Why SignalBus + ITurnParticipant + BotTelemetryHub (and not gameplay code)

- `SignalBus` is already the cross-feature event surface; subscribing needs zero
  gameplay changes — telemetry stays observational, no second authority.
- Turns emit no signals; `ITurnParticipant` is the canonical turn boundary.
  `TelemetryTurnParticipant` (order 10_000, runs last) reads committed state.
- `BotTelemetryHub` is the single authority for bot decisions — it gained a
  `TraceRecorded`/`EpisodeRecorded` event (additive) instead of a parallel system.
- `INetworkProvider.PeerConnected/Disconnected` covers multiplayer session edges.

## Rejected alternatives

- New script events / UnityEvents per producer → invasive, duplicate plumbing.
- Reflection-driven event capture → violates contract discipline and IL2CPP safety.
- Telemetry types inside Moyva gameplay assemblies → breaks reuse, inverts deps.
