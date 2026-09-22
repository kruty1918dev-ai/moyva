// Local mock ingestion backend — zero npm deps. Filesystem registry + object store
// with an in-process async mutex for transactional group/batch resolution.
//
//   node server.js [--port 8787] [--data ./telemetry-backend-data]
//
// Endpoints:
//   GET  /health           → {"ok":true}
//   POST /v1/batch         → ingest (envelope headers + binary body)
//   GET  /v1/config        → sample TelemetryRemoteConfig
//   GET  /debug/state      → {groups, batches, objects} for acceptance tests
'use strict';

const http = require('http');
const fs = require('fs');
const fsp = require('fs/promises');
const path = require('path');
const zlib = require('zlib');
const { ingest } = require('../shared/ingest-core');

// ---- tiny async mutex: serialize registry transactions in-process ----
let tail = Promise.resolve();
function mutex(fn) {
  const run = tail.then(fn);
  tail = run.catch(() => {});
  return run;
}

class FileRegistry {
  constructor(dir) {
    this.dir = dir;
    this.batchesPath = path.join(dir, 'batches.json');
    this.groupsPath = path.join(dir, 'groups.json');
  }
  async _read(p) {
    try { return JSON.parse(await fsp.readFile(p, 'utf8')); }
    catch { return {}; }
  }
  async _write(p, obj) {
    const tmp = p + '.tmp';
    await fsp.writeFile(tmp, JSON.stringify(obj, null, 2));
    await fsp.rename(tmp, p); // atomic commit
  }
  /// One mutex-guarded metadata transaction per request: batch lookup,
  /// group resolution and registration are atomic within it.
  transact(fn) {
    return mutex(async () => {
      const tx = {
        getBatch: async (batchId) => (await this._read(this.batchesPath))[batchId] || null,
        putBatch: async (rec) => {
          const all = await this._read(this.batchesPath);
          all[rec.batchId] = rec;
          await this._write(this.batchesPath, all);
        },
        getOrCreateGroup: async (key, make) => {
          const all = await this._read(this.groupsPath);
          if (!all[key]) { all[key] = make(); await this._write(this.groupsPath, all); }
          return all[key];
        },
        incrementGroup: async (key, events) => {
          const all = await this._read(this.groupsPath);
          if (all[key]) {
            all[key].batchCount++;
            all[key].eventCount += events;
            await this._write(this.groupsPath, all);
          }
        },
      };
      return fn(tx);
    });
  }
  async state() {
    return {
      groups: await this._read(this.groupsPath),
      batches: await this._read(this.batchesPath),
    };
  }
}

class FileObjectStore {
  constructor(dir) { this.dir = dir; }
  async putIfAbsent(key, bytes) {
    const safe = key.replace(/\.\./g, '').replace(/^\/+/, '');
    const p = path.join(this.dir, safe);
    await fsp.mkdir(path.dirname(p), { recursive: true });
    try {
      const fd = await fsp.open(p, 'wx'); // fail if exists → immutability
      try { await fd.write(bytes); } finally { await fd.close(); }
      return true;
    } catch (e) {
      if (e.code === 'EEXIST') return false;
      throw e;
    }
  }
  async list() {
    const out = [];
    const walk = async (d) => {
      for (const e of await fsp.readdir(d, { withFileTypes: true })) {
        const p = path.join(d, e.name);
        if (e.isDirectory()) await walk(p);
        else out.push(path.relative(this.dir, p).replace(/\\/g, '/'));
      }
    };
    try { await walk(this.dir); } catch { }
    return out;
  }
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    let size = 0;
    req.on('data', (c) => {
      size += c.length;
      if (size > 64 * 1024 * 1024) { reject(new Error('too_large')); req.destroy(); return; }
      chunks.push(c);
    });
    req.on('end', () => resolve(Buffer.concat(chunks)));
    req.on('error', reject);
  });
}

async function main() {
  const args = process.argv.slice(2);
  const port = +(args[args.indexOf('--port') + 1] || 8787);
  const dataDir = path.resolve(args[args.indexOf('--data') + 1] || './telemetry-backend-data');
  await fsp.mkdir(dataDir, { recursive: true });
  const registry = new FileRegistry(path.join(dataDir, 'meta'));
  const objects = new FileObjectStore(path.join(dataDir, 'objects'));
  await fsp.mkdir(path.join(dataDir, 'meta'), { recursive: true });
  await fsp.mkdir(path.join(dataDir, 'objects'), { recursive: true });

  const deps = {
    registry,
    objects,
    decompress: (buf, max) => {
      const out = zlib.gunzipSync(buf);
      if (out.length > max) throw new Error('decompression-size-limit');
      return out;
    },
  };

  const server = http.createServer(async (req, res) => {
    try {
      const url = new URL(req.url, 'http://x');
      if (req.method === 'GET' && url.pathname === '/health') {
        res.writeHead(200, { 'content-type': 'application/json' });
        return res.end('{"ok":true}');
      }
      if (req.method === 'GET' && url.pathname === '/v1/config') {
        res.writeHead(200, { 'content-type': 'application/json' });
        return res.end(JSON.stringify({
          configVersion: 1, enabled: true,
          endpoint: `http://localhost:${port}/v1/batch`,
          projectId: 'local', environment: 'dev',
          supportedProtocols: [1], compressionModes: ['gzip', 'none'],
        }));
      }
      if (req.method === 'GET' && url.pathname === '/debug/state') {
        const state = await registry.state();
        state.objects = await objects.list();
        res.writeHead(200, { 'content-type': 'application/json' });
        return res.end(JSON.stringify(state));
      }
      if (req.method === 'POST' && url.pathname === '/v1/batch') {
        const body = await readBody(req);
        const headers = {};
        for (const [k, v] of Object.entries(req.headers)) headers[k.toLowerCase()] = v;
        const result = await ingest(deps, headers, body);
        res.writeHead(result.status, { 'content-type': 'application/json' });
        return res.end(result.body);
      }
      res.writeHead(404); res.end('{"error":"not_found"}');
    } catch (e) {
      res.writeHead(500); res.end(JSON.stringify({ error: 'internal', message: e.message }));
    }
  });
  server.listen(port, () => console.log(`telemetry backend on :${port} data=${dataDir}`));
}

if (require.main === module) main();
module.exports = { FileRegistry, FileObjectStore };
