-- D1 metadata schema for the telemetry ingestion reference backend.
-- One dataset group per (project, environment, fingerprint); one batch per batchId.

CREATE TABLE IF NOT EXISTS dataset_groups (
  group_key      TEXT PRIMARY KEY,            -- project/env/fingerprint
  group_id       TEXT NOT NULL,
  project_id     TEXT NOT NULL,
  environment    TEXT NOT NULL,
  fingerprint    TEXT NOT NULL,
  batch_count    INTEGER NOT NULL DEFAULT 0,
  event_count    INTEGER NOT NULL DEFAULT 0,
  first_seen_utc TEXT NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_groups_fingerprint
  ON dataset_groups (project_id, environment, fingerprint);

CREATE TABLE IF NOT EXISTS batches (
  batch_id        TEXT PRIMARY KEY,
  checksum        TEXT NOT NULL,
  fingerprint     TEXT NOT NULL,
  group_id        TEXT NOT NULL,
  object_ref      TEXT NOT NULL,
  accepted_events INTEGER NOT NULL,
  status          TEXT NOT NULL,              -- accepted | quarantined
  ack_id          TEXT NOT NULL,
  created_utc     TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS rate_limits (
  k TEXT PRIMARY KEY,
  n INTEGER NOT NULL
);
