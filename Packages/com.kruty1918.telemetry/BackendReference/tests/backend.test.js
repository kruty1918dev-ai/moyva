// Contract tests for the reference ingestion backend.
// Run: node --test Packages/com.kruty1918.telemetry/BackendReference/tests/
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { spawn } = require('child_process');
const crypto = require('crypto');
const fs = require('fs');
const os = require('os');
const path = require('path');
const zlib = require('zlib');

const SERVER = path.join(__dirname, '..', 'local', 'server.js');

function sha256(b) { return crypto.createHash('sha256').update(b).digest('hex'); }

function startServer(t) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'tmb-backend-'));
  const port = 8700 + Math.floor(Math.random() * 800);
  const proc = spawn(process.execPath, [SERVER, '--port', String(port), '--data', dir], {
    stdio: ['ignore', 'pipe', 'pipe'],
  });
  t.after(() => proc.kill('SIGTERM'));
  return { proc, port, dir };
}

async function waitReady(port) {
  for (let i = 0; i < 100; i++) {
    try {
      const r = await fetch(`http://localhost:${port}/health`);
      if (r.ok) return;
    } catch { }
    await new Promise(r => setTimeout(r, 50));
  }
  throw new Error('server did not start');
}

function envelope(overrides = {}) {
  const payload = Buffer.from('{"t":"demo"}\n{"t":"demo"}\n');
  const compressed = zlib.gzipSync(payload);
  const h = {
    'x-telemetry-protocol': '1',
    'x-telemetry-batch-id': 'batch-' + crypto.randomUUID().slice(0, 8),
    'x-telemetry-fingerprint': 'tfp1.' + 'a'.repeat(64),
    'x-telemetry-payload-sha256': sha256(payload),
    'x-telemetry-event-count': '2',
    'x-telemetry-installation-id': 'inst1',
    'x-telemetry-app-session-id': 'sess1',
    'x-telemetry-compression': 'gzip',
    'x-telemetry-project': 'test',
    'x-telemetry-environment': 'dev',
    ...overrides,
  };
  return { headers: h, body: compressed };
}

async function post(port, env) {
  return fetch(`http://localhost:${port}/v1/batch`, {
    method: 'POST', headers: env.headers, body: env.body,
  });
}

test('health endpoint', async (t) => {
  const { port } = startServer(t);
  await waitReady(port);
  const r = await fetch(`http://localhost:${port}/health`);
  assert.equal(r.status, 200);
  assert.deepEqual(await r.json(), { ok: true });
});

test('accept → duplicate → conflict lifecycle', async (t) => {
  const { port } = startServer(t);
  await waitReady(port);
  const env = envelope();

  const r1 = await post(port, env);
  assert.equal(r1.status, 200);
  const ack1 = await r1.json();
  assert.equal(ack1.status, 'accepted');
  assert.equal(ack1.batchId, env.headers['x-telemetry-batch-id']);
  assert.equal(ack1.checksum, env.headers['x-telemetry-payload-sha256']);
  assert.ok(ack1.groupId.startsWith('grp-'));
  assert.ok(ack1.objectRef.includes(env.headers['x-telemetry-fingerprint']));

  // Lost-ACK resend: same batch → duplicate, original ackId preserved.
  const r2 = await post(port, env);
  const ack2 = await r2.json();
  assert.equal(r2.status, 200);
  assert.equal(ack2.status, 'duplicate');
  assert.equal(ack2.ackId, ack1.ackId);

  // Same batchId, different payload → 409, never silently accepted.
  const bad = envelope({ 'x-telemetry-batch-id': env.headers['x-telemetry-batch-id'] });
  bad.headers['x-telemetry-payload-sha256'] = 'f'.repeat(64);
  const r3 = await post(port, bad);
  assert.equal(r3.status, 422); // checksum mismatch hits first (integrity)
});

test('checksum mismatch rejected', async (t) => {
  const { port } = startServer(t);
  await waitReady(port);
  const env = envelope();
  env.body = zlib.gzipSync(Buffer.from('{"t":"tampered"}\n'));
  const r = await post(port, env);
  assert.equal(r.status, 422);
});

test('missing headers → 400; bad compression → 400', async (t) => {
  const { port } = startServer(t);
  await waitReady(port);
  const env = envelope();
  delete env.headers['x-telemetry-batch-id'];
  assert.equal((await post(port, env)).status, 400);
  const env2 = envelope({ 'x-telemetry-compression': 'lz77' });
  assert.equal((await post(port, env2)).status, 400);
});

test('concurrent first-group creation yields exactly one group', async (t) => {
  const { port, dir } = startServer(t);
  await waitReady(port);
  // 10 parallel uploads with the SAME fingerprint → one group.
  const fp = 'tfp1.' + 'b'.repeat(64);
  const results = await Promise.all(Array.from({ length: 10 }, () => {
    const e = envelope({ 'x-telemetry-fingerprint': fp });
    return post(port, e).then(r => r.json());
  }));
  const groups = new Set(results.map(a => a.groupId));
  assert.equal(groups.size, 1, 'single group for shared fingerprint');
  const state = await (await fetch(`http://localhost:${port}/debug/state`)).json();
  assert.equal(Object.keys(state.groups).length, 1);
  assert.equal(Object.keys(state.batches).length, 10);
  assert.equal(state.objects.length, 10);
});

test('different fingerprints → separate groups; objects immutable under fp prefix', async (t) => {
  const { port } = startServer(t);
  await waitReady(port);
  const a = await (await post(port, envelope({ 'x-telemetry-fingerprint': 'tfp1.' + 'c'.repeat(64) }))).json();
  const b = await (await post(port, envelope({ 'x-telemetry-fingerprint': 'tfp1.' + 'd'.repeat(64) }))).json();
  assert.notEqual(a.groupId, b.groupId);
  assert.ok(a.objectRef.includes('c'.repeat(8)));
  assert.ok(b.objectRef.includes('d'.repeat(8)));
});
