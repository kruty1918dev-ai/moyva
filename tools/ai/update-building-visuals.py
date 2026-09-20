import json, io, os, re

PRESETS = 'Assets/Moyva/Presets/Buildings'
KAY = 'Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Prefabs/buildings'

guids = {}
for color in os.listdir(KAY):
    d = os.path.join(KAY, color)
    if not os.path.isdir(d):
        continue
    for f in os.listdir(d):
        if f.endswith('.prefab.meta'):
            meta = io.open(os.path.join(d, f), encoding='utf-8').read()
            g = re.search(r'guid: ([a-f0-9]+)', meta).group(1)
            path = d.replace(os.sep, '/') + '/' + f[:-5]
            guids[f[:-12]] = (g, path)


def slug(n):
    return n.lower().replace('_', '-')


def key(n):
    g, _ = guids[n]
    return 'asset.game-object.{}.{}'.format(slug(n), g[:8])


def ref(n, required=True):
    g, p = guids[n]
    return {'$asset': key(n), 'editorPath': p, 'required': required}


mapping = {
    'townhall':        ('building_townhall', True),
    'castle-01':       ('building_castle', True),
    'house-01':        ('building_home_B', True),
    'barn':            ('building_home_A', True),
    'farm':            ('building_grain', False),
    'storage':         ('building_market', True),
    'caravan-depot':   ('building_docks', True),
    'barrack':         ('building_barracks', True),
    'stable':          ('building_stables', True),
    'weapon-workshop': ('building_workshop', True),
    'armor-workshop':  ('building_archeryrange', True),
    'smelter':         ('building_blacksmith', True),
    'engineers-guild': ('building_tower_catapult', True),
    'iron-mine':       ('building_mine', True),
    'stone-quarry':    ('building_dirt', False),
    'sawmill':         ('building_lumbermill', True),
    'wood-camp':       ('building_tent', True),
    'windmill-01':     ('building_windmill', True),
}
COLORS = ['blue', 'green', 'red', 'yellow']

for bid, (base, colored) in mapping.items():
    fp = os.path.join(PRESETS, bid + '.json')
    d = json.load(io.open(fp, encoding='utf-8'))
    pres = d['presentation']
    if colored:
        pres['prefab'] = ref(base + '_blue')
        variants = pres.setdefault('variants', {})
        variants['prefabVariants'] = {c: ref(base + '_' + c) for c in COLORS}
    else:
        pres['prefab'] = ref(base)
        if 'variants' in pres:
            pres['variants'].pop('prefabVariants', None)
    txt = json.dumps(d, indent=2, ensure_ascii=False)
    io.open(fp, 'w', encoding='utf-8', newline='\r\n').write(txt + '\r\n')
    print('{:>16} -> {} ({})'.format(bid, base, '4 colors' if colored else 'neutral'))
