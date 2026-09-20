import json, io, os, re, uuid

PRESETS = 'Assets/Moyva/Presets/Buildings'
GEN = 'Assets/Moyva/Resources/MoyvaConfigGenerated'
CATALOG = 'Assets/Moyva/Resources/MoyvaRuntimeAssetCatalog.prefab'
KAY = 'Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Prefabs/buildings'

# guid lookup for every kaykit prefab
prefab_guids = {}
for color in os.listdir(KAY):
    d = os.path.join(KAY, color)
    if not os.path.isdir(d):
        continue
    for f in os.listdir(d):
        if f.endswith('.prefab.meta'):
            meta = io.open(os.path.join(d, f), encoding='utf-8').read()
            prefab_guids[f[:-12]] = re.search(r'guid: ([a-f0-9]+)', meta).group(1)


def slug(n):
    return n.lower().replace('_', '-')


# 1. sync generated json copies ----------------------------------------------
meta_tpl = ('fileFormatVersion: 2\n'
            'guid: {guid}\n'
            'TextScriptImporter:\n'
            '  externalObjects: {{}}\n'
            '  userData: \n'
            '  assetBundleName: \n'
            '  assetBundleVariant: \n')

synced, created_meta = [], []
for f in sorted(os.listdir(PRESETS)):
    if not f.endswith('.json'):
        continue
    d = json.load(io.open(os.path.join(PRESETS, f), encoding='utf-8'))
    if d.get('schema') != 'moyva.building':
        continue
    gid = d['id']
    gen_path = os.path.join(GEN, 'moyva-building--{}.json'.format(gid))
    txt = json.dumps(d, indent=2, ensure_ascii=False)
    body = txt.replace('\n', '\r\n') + '\n'
    io.open(gen_path, 'w', encoding='utf-8', newline='').write(body)
    synced.append(gid)
    if not os.path.exists(gen_path + '.meta'):
        io.open(gen_path + '.meta', 'w', encoding='utf-8').write(
            meta_tpl.format(guid=uuid.uuid4().hex))
        created_meta.append(gid)

print('synced {} generated docs, {} new metas: {}'.format(
    len(synced), len(created_meta), ', '.join(created_meta)))

# 2. collect referenced game-object keys -------------------------------------
needed = set()
asset_re = re.compile(r'"\$asset":\s*"(asset\.game-object\.[a-z0-9-]+\.[a-f0-9]{8})"')
for f in sorted(os.listdir(PRESETS)):
    if not f.endswith('.json'):
        continue
    raw = io.open(os.path.join(PRESETS, f), encoding='utf-8').read()
    needed.update(asset_re.findall(raw))

cat = io.open(CATALOG, encoding='utf-8').read()
existing = set(re.findall(r'Key: (asset\.game-object\.[a-z0-9-]+\.[a-f0-9]{8})', cat))
missing = sorted(needed - existing)
print('missing catalog keys:', len(missing))
for k in missing:
    print('  +', k)

# key -> guid:  key = asset.game-object.<slug>.<guid8>; prefab name = slug parts
def guid_for_key(k):
    g8 = k.rsplit('.', 1)[1]
    for name, g in prefab_guids.items():
        if g.startswith(g8):
            return g
    return None

entries = []
for k in missing:
    g = guid_for_key(k)
    if g is None:
        print('  !! no prefab guid for', k)
        continue
    entries.append((k, g))

# insert in alphabetical order among existing '- Key:' lines
lines = cat.split('\n')
out = []
pending = sorted(entries)
for line in lines:
    m = re.match(r'\s*- Key: (\S+)', line)
    while pending and m and pending[0][0] < m.group(1):
        k, g = pending.pop(0)
        out.append('  - Key: ' + k)
        out.append('    Asset: {fileID: 919132149155446097, guid: ' + g + ', type: 3}')
    out.append(line)
for k, g in pending:
    out.append('  - Key: ' + k)
    out.append('    Asset: {fileID: 919132149155446097, guid: ' + g + ', type: 3}')
io.open(CATALOG, 'w', encoding='utf-8', newline='').write('\n'.join(out))
print('catalog updated')
