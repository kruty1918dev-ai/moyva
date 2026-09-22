# Telemetry migration / adoption notes

## For Moyva gameplay code

Nothing changes. Telemetry observes `SignalBus`, `ITurnParticipant`,
`BotTelemetryHub` and `INetworkProvider` — producers are untouched.
To emit a *new* event type:

1. Add an `EventContract` in `MoyvaTelemetryContracts.CreateRegistry`
   (ContractId == EventType, Version=1).
2. Add a typed struct implementing `ITelemetryEvent` in `MoyvaTelemetryEvents*`.
3. Subscribe to the signal in `MoyvaSignalTelemetryAdapter` (or fire directly
   via `ITelemetrySink.Track` from a service that legitimately owns the fact).

Changing a payload's semantics ⇒ bump `Version` — the dataset fingerprint
changes, creating a new server-side dataset group. Incidental metadata
(session ids, timestamps) never affects the fingerprint.

## For other products reusing the package

- Copy the adapter pattern: product assembly references the package, owns its
  contracts + `FingerprintInput`, and binds `ITelemetrySink`.
- Bootstrap in one call: `UnityTelemetryBootstrap.Create(config, registry, fp)`
  or compose `TelemetryRuntime` headlessly.
- Backend: `node BackendReference/local/server.js` locally; `worker/` is the
  Cloudflare reference (same shared ingestion core).

## From BotTelemetryHub buffers

Existing in-memory trace/metrics buffers are unchanged and still authoritative
for debug tooling. The new `TraceRecorded`/`EpisodeRecorded` events let the
pipeline observe them without duplicating or consuming the buffers.
