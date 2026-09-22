// Shared ingestion protocol — used identically by the local Node server and the
// Cloudflare Worker. Zero dependencies; storage/registry are injected so the same
// logic runs on the filesystem (local) or R2+D1 (production).
//
// Server flow:
//   envelope headers → limits → checksum verify → transactional group resolve
//   → BatchId idempotency → immutable raw object write → batch register → ACK.
'use strict';

const crypto = require('crypto');

const PROTOCOL = '1';
const MAX_BODY_BYTES = 16 * 1024 * 1024;
const MAX_DECOMPRESSED_BYTES = 64 * 1024 * 1024;
const ALLOWED_COMPRESSION = new Set(['none', 'gzip']);

const REQUIRED_HEADERS = [
  'x-telemetry-batch-id',
  'x-telemetry-fingerprint',
  'x-telemetry-payload-sha256',
  'x-telemetry-event-count',
  'x-telemetry-installation-id',
];

function sha256hex(buf) {
  return crypto.createHash('sha256').update(buf).digest('hex');
}

function err(status, code, message) {
  return { status, body: JSON.stringify({ error: code, message }) };
}

function ackJson(rec, status) {
  return {
    protocol: 1,
    ackId: rec.ackId,
    batchId: rec.batchId,
    checksum: rec.checksum,
    fingerprint: rec.fingerprint,
    acceptedEvents: rec.acceptedEvents,
    groupId: rec.groupId,
    objectRef: rec.objectRef,
    status,
    validatorVersion: 'ref-1',
  };
}

/**
 * @param {object} deps
 *   - registry: { transact(fn) } — fn receives a tx exposing
 *     getBatch(batchId), getOrCreateGroup(key, make), putBatch(rec),
 *     incrementGroup(key, events). Local impl serializes via mutex;
 *     D1 impl relies on unique constraints (batch_id PK, group_key PK).
 *   - objects: { putIfAbsent(key, bytes) -> bool }
 *   - decompress(bytes, maxBytes) -> rawBytes (may be async)
 */
async function ingest(deps, headers, body) {
  if (!body || body.length > MAX_BODY_BYTES) return err(413, 'too_large', 'body');
  if (headers['x-telemetry-protocol'] !== PROTOCOL)
    return err(400, 'protocol', 'unsupported protocol');
  for (const h of REQUIRED_HEADERS)
    if (!headers[h]) return err(400, 'bad_request', 'missing ' + h);

  const project = headers['x-telemetry-project'] || 'dev';
  const env = headers['x-telemetry-environment'] || 'dev';
  const batchId = headers['x-telemetry-batch-id'];
  const fingerprint = headers['x-telemetry-fingerprint'];
  const checksum = headers['x-telemetry-payload-sha256'];
  const eventCount = parseInt(headers['x-telemetry-event-count'], 10);
  const compression = headers['x-telemetry-compression'] || 'none';
  const session = headers['x-telemetry-app-session-id'] || 'unknown';
  if (!Number.isInteger(eventCount) || eventCount < 0 || eventCount > 4_000_000)
    return err(400, 'bad_request', 'event-count');

  if (!ALLOWED_COMPRESSION.has(compression))
    return err(400, 'compression', 'unsupported');
  let raw = body;
  if (compression === 'gzip') {
    try {
      raw = await deps.decompress(body, MAX_DECOMPRESSED_BYTES);
    } catch (e) {
      return err(422, 'integrity', 'decompress: ' + e.message);
    }
  }
  if (sha256hex(raw) !== checksum)
    return err(422, 'checksum', 'payload mismatch');

  // Transactional: idempotent batch check + group resolution + immutable object
  // write + batch registration. One tx per request — concurrent same-batchId
  // uploads resolve deterministically (second sees the committed record).
  return deps.registry.transact(async (tx) => {
    const existing = await tx.getBatch(batchId);
    if (existing) {
      if (existing.checksum !== checksum)
        return err(409, 'checksum_conflict', 'batchId reused with different payload');
      return { status: 200, body: JSON.stringify(ackJson(existing, 'duplicate')) };
    }

    const groupKey = `${project}/${env}/${fingerprint}`;
    const group = await tx.getOrCreateGroup(groupKey, () => ({
      groupId: 'grp-' + crypto.randomUUID(),
      projectId: project,
      environment: env,
      fingerprint,
      batchCount: 0,
      eventCount: 0,
      firstSeenUtc: new Date().toISOString(),
    }));

    // Deterministic immutable object key — one object per BATCH, not per event.
    const day = new Date().toISOString().slice(0, 10);
    const key =
      `raw/${project}/${env}/${fingerprint}/date=${day}/session=${session}/batch=${batchId}.jsonl` +
      (compression === 'gzip' ? '.gz' : '');
    const placed = await deps.objects.putIfAbsent(key, body);
    if (!placed) return err(409, 'object_conflict', 'key exists');

    const rec = {
      batchId, checksum, fingerprint, groupId: group.groupId,
      objectRef: key, acceptedEvents: eventCount,
      status: 'accepted', ackId: 'ack-' + crypto.randomUUID(),
    };
    await tx.putBatch(rec);
    await tx.incrementGroup(groupKey, eventCount);
    return { status: 200, body: JSON.stringify(ackJson(rec, 'accepted')) };
  });
}

module.exports = { ingest, sha256hex, MAX_BODY_BYTES, MAX_DECOMPRESSED_BYTES };
