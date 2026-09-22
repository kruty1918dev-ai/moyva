# com.kruty1918.telemetry

High-integrity telemetry and data-acquisition layer for Unity — built for
ML/AI dataset pipelines, not just analytics.

## Pipeline

```
typed events → contract validation → quality classification → deterministic
dataset fingerprint → append-only local spool (atomic, crash-safe) →
batching + compression → retrying upload → server checksum + idempotent
registration → verified ACK → safe local cleanup
```

## Layers

| Folder | Contents |
|---|---|
| `Runtime/` | Engine-free core: contracts, canonical JSON, fingerprint, validation, spool, upload coordinator, transports |
| `Unity/` | `TelemetryHostBehaviour`, `UnityWebRequestTransport`, `UnityTelemetryBootstrap`, network status, remote config |
| `Editor/` | `TelemetryDashboardWindow` (Tools → Telemetry → Dashboard) |
| `BackendReference/` | Node local ingestion server + Cloudflare Worker adapter + HTTP tests |
| `Tests/Editor/` | EditMode tests |

## Quick start (Unity host)

```csharp
var registry = new ContractRegistry();
StandardContracts.RegisterAll(registry);
registry.Register(new EventContract("my.score", 1, "my.score", "com.example")
    .Add(new ContractField("value", ContractFieldType.Int, true)));

var handle = UnityTelemetryBootstrap.Create(config, registry, fingerprintInput);
handle.Runtime.Sink.Track(new MyScoreEvent { Value = 42 });
```

Product layers own: contract registry, fingerprint input, adapter bindings.
The package knows nothing about the product.

## Contracts

- `ContractId == EventType`, one contract per event type, `Version` bumps on
  semantic change. `EventContract.EventType` may end in `.*` to govern a family.
- Field types: string/int/long/float/bool (+ nested maps, capped depth).
- `Privacy` (Analytics/Research) is enforced against consent level.
- `Priority` drives storage-pressure eviction order (Critical never evicted).

## Fingerprint

SHA-256 over canonical semantic inputs: registered contracts, semantic
parameters, pipeline stage versions. Incidental metadata (session ids,
timestamps, app version) never enters the fingerprint — that is the dataset
grouping key; identical data → identical group server-side.

## Storage

Append-only spool under `persistentDataPath/telemetry/spool`:
`pending/`, `inflight/`, `quarantine/`. Atomic temp+rename writes, recovery of
in-flight/orphaned files on start, corrupt files quarantined, storage pressure
evicts lowest-priority first and refuses when only Critical remain.

## Upload

Batch header carries `payloadSha256` (of uncompressed bytes), lineage
(producer, client ids), contracts used, quality report. Coordinator handles
offline/no-endpoint (batches stay pending), exponential backoff, HTTP 429,
lost-ACK resend, and deletes local copies only after a verified ACK.

## Backend

`BackendReference/local/server.js` — `node server.js [--port N]`.
Protocol: `POST /ingest` with `X-Batch-Id`, `X-Fingerprint`,
`X-Payload-Sha256`, `X-Compression`, `X-Protocol` headers and the batch body.
Server verifies checksum, creates exactly one dataset group per fingerprint
(transactional, race-safe), registers batches idempotently, stores raw objects
immutably, returns signed ACK. `worker/` is the same core on Cloudflare D1+R2.

## Remote config

`ITelemetryConfigProvider` → `TelemetryRemoteConfig` JSON (endpoint, project,
token, sampling rates, expiry). Missing/expired = local-only spool mode.
File provider: point at any JSON path. Endpoint/token never baked in code.

## Privacy

Consent levels: Disabled / Analytics (default) / Research. Research-class
contracts are rejected below consent. Peer ids and similar are hashed at the
edge before recording. `PurgeAndRotateIdentity()` wipes spool + rotates the
installation id for consent withdrawal.

## Verification

- `tools/telemetry/harness` — 23 scenario/unit tests over the core on plain .NET.
- `BackendReference/tests/backend.test.js` — 6 tests over real HTTP.
- `tools/telemetry/unitycheck` — compile-check of Unity+adapter layers against
  the project's ScriptAssemblies (no editor needed).
