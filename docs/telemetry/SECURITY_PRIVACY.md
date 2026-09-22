# Telemetry security & privacy

## Secrets

- No tokens, keys or endpoints are committed. The client token is a public
  identifier delivered by remote config (`telemetry.remote.json` in
  `persistentDataPath`, or `MOYVA_TELEMETRY_REMOTE_CONFIG` pointing elsewhere).
- Backend auth is a boundary check (`X-Api-Key`/token header); the reference
  server treats it as non-secret and issues 401 for malformed/missing auth when
  configured.

## Data minimization

- IDs that can identify a human (network peer ids) are SHA-256 hashed at the
  edge; raw values never enter the pipeline.
- High-frequency UI noise (hover, preview, drag) is intentionally untracked.
- Installation id is a random GUID stored locally — rotatable via
  `TelemetryRuntime.PurgeAndRotateIdentity()` which also wipes the spool.

## Consent

`TelemetryConfig.Consent`: `Disabled` | `Analytics` (default) | `Research`.
Contracts declare `PrivacyClass`; Research-class events (bot decision traces,
episode metrics) are rejected below consent — enforced in `TelemetrySink`
before serialization.

## Integrity

- Payload SHA-256 computed on uncompressed bytes; verified client-side at
  spool read and server-side at ingest. Corrupt files are quarantined, never
  silently dropped.
- Batch + dataset-group registration is idempotent; replays return
  `duplicate`, never double-count.

## Transport

HTTPS recommended in production; the Unity transport uses UnityWebRequest
(platform TLS). The local reference server is dev-only HTTP.
