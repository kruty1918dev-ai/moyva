// Cloudflare Worker ingestion endpoint — reference implementation.
// Storage: R2 bucket (raw immutable objects) + D1 (transactional metadata).
// Deploy: `wrangler deploy` — see wrangler.toml. Never ship account secrets in
// the Unity client; the client only needs the public ingest URL + client token.
import { ingest } from '../../shared/ingest-core.js';

const RATE_LIMIT_PER_MINUTE = 120;

class D1Registry {
  constructor(db) { this.db = db; }
  // D1 has no interactive transactions over this API; uniqueness is enforced by
  // PK/UNIQUE constraints, so concurrent first-writes resolve deterministically.
  transact(fn) {
    const db = this.db;
    const tx = {
      getBatch: async (batchId) => {
        const row = await db.prepare('SELECT * FROM batches WHERE batch_id = ?1')
          .bind(batchId).first();
        if (!row) return null;
        return {
          batchId: row.batch_id, checksum: row.checksum, fingerprint: row.fingerprint,
          groupId: row.group_id, objectRef: row.object_ref,
          acceptedEvents: row.accepted_events, status: row.status, ackId: row.ack_id,
        };
      },
      putBatch: async (rec) => {
        await db.prepare(
          'INSERT INTO batches (batch_id, checksum, fingerprint, group_id, object_ref, accepted_events, status, ack_id, created_utc) ' +
          'VALUES (?1,?2,?3,?4,?5,?6,?7,?8,?9)')
          .bind(rec.batchId, rec.checksum, rec.fingerprint, rec.groupId, rec.objectRef,
            rec.acceptedEvents, rec.status, rec.ackId, new Date().toISOString())
          .run();
      },
      getOrCreateGroup: async (key, make) => {
        // INSERT OR IGNORE + unique group_key ⇒ race-safe concurrent first create.
        const [project, env, fp] = key.split('/');
        const made = make();
        await db.prepare(
          'INSERT OR IGNORE INTO dataset_groups (group_key, group_id, project_id, environment, fingerprint, batch_count, event_count, first_seen_utc) ' +
          'VALUES (?1,?2,?3,?4,?5,0,0,?6)')
          .bind(key, made.groupId, project, env, fp, made.firstSeenUtc).run();
        const r = await db.prepare('SELECT * FROM dataset_groups WHERE group_key = ?1')
          .bind(key).first();
        return {
          groupId: r.group_id, projectId: r.project_id, environment: r.environment,
          fingerprint: r.fingerprint, batchCount: r.batch_count, eventCount: r.event_count,
        };
      },
      incrementGroup: async (key, events) => {
        await db.prepare(
          'UPDATE dataset_groups SET batch_count = batch_count + 1, event_count = event_count + ?2 WHERE group_key = ?1')
          .bind(key, events).run();
      },
    };
    return fn(tx);
  }
}

class R2Objects {
  constructor(bucket) { this.bucket = bucket; }
  async putIfAbsent(key, bytes) {
    // R2 has no conditional put; key contains unique batchId so collision means
    // a retried/conflicting write — check existence first.
    if (await this.bucket.head(key)) return false;
    await this.bucket.put(key, bytes, { customMetadata: { immutable: '1' } });
    return true;
  }
}

async function rateLimit(env, ip) {
  // Fixed-window per-IP counter in D1 — coarse throttle, tune for your traffic.
  const window = Math.floor(Date.now() / 60000);
  const key = `rl:${ip}:${window}`;
  const row = await env.TELEMETRY_DB
    .prepare('INSERT INTO rate_limits (k, n) VALUES (?1,1) ON CONFLICT(k) DO UPDATE SET n = n + 1 RETURNING n')
    .bind(key).first();
  return row && row.n <= RATE_LIMIT_PER_MINUTE;
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    if (request.method === 'GET' && url.pathname === '/health')
      return Response.json({ ok: true });
    if (request.method !== 'POST' || url.pathname !== '/v1/batch')
      return new Response('{"error":"not_found"}', { status: 404 });

    const ip = request.headers.get('cf-connecting-ip') || 'unknown';
    if (!(await rateLimit(env, ip)))
      return new Response('{"error":"rate_limited"}', { status: 429 });

    const deps = {
      registry: new D1Registry(env.TELEMETRY_DB),
      objects: new R2Objects(env.TELEMETRY_RAW),
      decompress: async (buf, max) => {
        const ds = new DecompressionStream('gzip');
        const stream = new Response(new Blob([buf]).stream().pipeThrough(ds)).arrayBuffer();
        const out = new Uint8Array(await stream);
        if (out.byteLength > max) throw new Error('decompression-size-limit');
        return out;
      },
    };
    const headers = {};
    request.headers.forEach((v, k) => { headers[k.toLowerCase()] = v; });
    const body = new Uint8Array(await request.arrayBuffer());
    const result = await ingest(deps, headers, body);
    return new Response(result.body, {
      status: result.status,
      headers: { 'content-type': 'application/json' },
    });
  },
};
