import json, io, os, re, uuid

PRESETS = 'Assets/Moyva/Presets/Buildings'
KAY = 'Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Prefabs/buildings'
ICONS = 'Assets/Moyva/Art/UI/Icons/Generated/Buildings'
COLORS = ['blue', 'green', 'red', 'yellow']

guids = {}
for color in os.listdir(KAY):
    d = os.path.join(KAY, color)
    if not os.path.isdir(d):
        continue
    for f in os.listdir(d):
        if f.endswith('.prefab.meta'):
            meta = io.open(os.path.join(d, f), encoding='utf-8').read()
            g = re.search(r'guid: ([a-f0-9]+)', meta).group(1)
            guids[f[:-12]] = (g, d.replace(os.sep, '/') + '/' + f[:-5])

icon_guids = {}
for f in os.listdir(ICONS):
    if f.endswith('.png.meta'):
        meta = io.open(os.path.join(ICONS, f), encoding='utf-8').read()
        g = re.search(r'guid: ([a-f0-9]+)', meta).group(1)
        icon_guids[f[:-9]] = (g, ICONS + '/' + f[:-5])


def slug(n):
    return n.lower().replace('_', '-')


def prefab_ref(n):
    g, p = guids[n]
    return {'$asset': 'asset.game-object.{}.{}'.format(slug(n), g[:8]),
            'editorPath': p, 'required': True}


def sprite_ref(n):
    g, p = icon_guids[n]
    return {'$asset': 'asset.sprite.{}.{}'.format(slug(n), g[:8]),
            'editorPath': p, 'required': True}


def mod(t, **kw):
    m = {'$type': t}
    m.update(kw)
    m['isEnabled'] = True
    m['singletonScope'] = 'PerBuilding'
    return m


def influence_req():
    return mod('settlement-influence-requirement', mergeMode='Override',
               requiresInfluence=True, blockOverlappingCenters=False,
               maximumDistanceToCenter=0)


def cost(rid, amount):
    return {'resourceId': rid, 'amount': amount}


def make(bid, model, icon, display, category, role, desc, tags,
         costs, turns, hp, armor, modules, water=False):
    variants = {'constructionPrefab': prefab_ref('building_scaffolding'),
                'prefabVariants': {c: prefab_ref(model + '_' + c) for c in COLORS}}
    return {
        '$schema': '../Schemas/building.schema.json',
        'schema': 'moyva.building',
        'version': 1,
        'id': bid,
        'model': 'building-definition',
        'placementModuleMigrationVersion': 1,
        'identity': {'id': bid, 'displayName': display, 'category': category,
                     'role': role, 'description': desc, 'tags': tags},
        'presentation': {
            'prefab': prefab_ref(model + '_blue'),
            'icon': sprite_ref(icon),
            'runtimePreview': sprite_ref(icon),
            'variants': variants,
            'uiTint': {'r': 1.0, 'g': 1.0, 'b': 1.0, 'a': 1.0},
            'visualYOffset': 0.0,
            'previewSettings': {
                'cameraOffset': {'x': 4.0, 'y': 5.0, 'z': -6.0},
                'cameraEulerAngles': {'x': 45.0, 'y': -35.0, 'z': 0.0},
                'orthographicSize': 4.0,
                'backgroundColor': {'r': 0.0, 'g': 0.0, 'b': 0.0, 'a': 0.0}}},
        'footprint': {
            'size': {'x': 1, 'y': 1}, 'anchor': 'Center',
            'customAnchor': {'x': 0, 'y': 0},
            'blocksMovement': True, 'blocksConstruction': True,
            'requiresFlatGround': True,
            'occupiedCells': [{'x': 0, 'y': 0}],
            'entranceCells': [], 'roadConnectionCells': []},
        'placement': {
            'canPlaceInFog': False, 'requiresSettlementInfluence': True,
            'createsSettlementInfluence': False,
            'blockIfSettlementCenterInRange': False,
            'influenceRadius': 0, 'minDistanceFromSettlementCenters': 0,
            'requiredTerrainIds': [], 'requiredNeighborOffsets': [],
            'requiresWaterNearby': water, 'requiresForestNearby': False,
            'requiresMountainNearby': False, 'requiresRoadNearby': False,
            'nearbyTileRequirements': []},
        'construction': {'cost': costs, 'buildTurns': turns,
                         'requiresBuilder': True, 'workRequired': 0},
        'runtimeStats': {'maxHp': hp, 'armor': armor,
                         'flags': 'BlocksPath, Selectable, Damageable',
                         'runtimeTags': []},
        'modules': modules,
        'migration': {'sourceAssetGuid': '', 'sourceAssetPath': '',
                      'sourceHash': 'agent-added-{}-2026-02'.format(bid)}}


buildings = [
    make('watchtower', 'building_watchtower', 'castle-01', 'Watchtower',
         'Military', 'Defense',
         'A tall lookout tower that spots enemies from afar and shoots at attackers.',
         ['military', 'defense', 'vision'],
         [cost('walnut-wood-materials-resources', 30),
          cost('stone-materials-resources', 20)],
         2, 120, 1,
         [mod('defense', armor=1, garrisonCapacity=0, attackRange=3,
              attackDamage=4, visionRevealBonus=2),
          mod('fog-reveal', revealRadius=4, shape='PixelCircle',
              revealOnBuilt=True, revealWhileActive=True,
              onlyAfterConstructionComplete=True),
          influence_req()]),
    make('well', 'building_well', 'house-01', 'Well',
         'Civilian', 'Housing',
         'A settlement well that supports a few extra residents.',
         ['civilian', 'housing', 'utility'],
         [cost('stone-materials-resources', 10),
          cost('walnut-wood-materials-resources', 10)],
         1, 60, 0,
         [mod('housing', capacity=2, isGarrisonCapable=False),
          influence_req()]),
    make('watermill', 'building_watermill', 'windmill-01', 'Watermill',
         'Industrial', 'Production',
         'A river mill that grinds grain into food. Must be built next to water.',
         ['industry', 'production', 'food', 'water'],
         [cost('walnut-wood-materials-resources', 40),
          cost('stone-materials-resources', 20)],
         3, 90, 0,
         [mod('workforce', workersRequired=1, priority=410, workerTypeId=''),
          mod('production', resourceId='wheat-bundle-food-resources',
              workersRequired=1, priority=410,
              recipes=[{'recipeId': 'watermill:mill-food', 'inputs': [],
                        'outputs': [{'resourceId': 'wheat-bundle-food-resources',
                                     'amount': 10}],
                        'turnsPerCycle': 1, 'requiresWorkers': True,
                        'requiresStorageSpace': False}]),
          influence_req()],
         water=True),
    make('church', 'building_church', 'castle-01', 'Church',
         'Civilian', 'Housing',
         'A stone church that anchors the community and shelters residents.',
         ['civilian', 'housing', 'landmark'],
         [cost('walnut-wood-materials-resources', 30),
          cost('stone-materials-resources', 40)],
         3, 140, 1,
         [mod('housing', capacity=3, isGarrisonCapable=False),
          mod('fog-reveal', revealRadius=2, shape='PixelCircle',
              revealOnBuilt=True, revealWhileActive=True,
              onlyAfterConstructionComplete=True),
          influence_req()]),
    make('tavern', 'building_tavern', 'house-01', 'Tavern',
         'Civilian', 'Support',
         'A lively tavern that earns a steady trickle of gold coins.',
         ['civilian', 'economy', 'gold'],
         [cost('walnut-wood-materials-resources', 50),
          cost('stone-materials-resources', 20)],
         3, 80, 0,
         [mod('workerless'),
          mod('housing', capacity=2, isGarrisonCapable=False),
          mod('production', resourceId='coin-gold-materials-resources',
              workersRequired=0, priority=300,
              recipes=[{'recipeId': 'tavern:earn-gold', 'inputs': [],
                        'outputs': [{'resourceId': 'coin-gold-materials-resources',
                                     'amount': 2}],
                        'turnsPerCycle': 1, 'requiresWorkers': False,
                        'requiresStorageSpace': False}]),
          influence_req()]),
]

meta_tpl = ('fileFormatVersion: 2\n'
            'guid: {guid}\n'
            'TextScriptImporter:\n'
            '  externalObjects: {{}}\n'
            '  userData: \n'
            '  assetBundleName: \n'
            '  assetBundleVariant: \n')

for b in buildings:
    fp = os.path.join(PRESETS, b['id'] + '.json')
    txt = json.dumps(b, indent=2, ensure_ascii=False)
    io.open(fp, 'w', encoding='utf-8', newline='\r\n').write(txt + '\r\n')
    io.open(fp + '.meta', 'w', encoding='utf-8').write(
        meta_tpl.format(guid=uuid.uuid4().hex))
    print('created', fp)
