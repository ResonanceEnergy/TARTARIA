"""One-off audit: categorize Moon 1 FBX by proportion vs docs/15 spec.
Run from any cwd. Reads C:\\Users\\gripa\\AppData\\Local\\Temp\\fbx_bbox.json.
"""
import json, os, datetime, glob

bbox = json.load(open(r'C:\Users\gripa\AppData\Local\Temp\fbx_bbox.json'))

SPEC = {
    'AnastasiaPrincess':   ('NPC', 1.7,  'Anastasia ~1.7m humanoid'),
    'LiraelGuardian':      ('NPC', 1.8,  'Lirael ~1.8m humanoid'),
    'CassianCarter':       ('NPC', 1.8,  'Cassian ~1.8m humanoid'),
    'MiloBoy':             ('NPC', 0.4,  'Milo fox 40cm tall per spec'),
    'BobInnkeeper':        ('NPC', 1.75, 'Bob innkeeper humanoid'),
    'CathedralChoirSpirit':('NPC', 1.8,  'Spirit humanoid'),
    'AnastasiaRockingChair':('Prop',1.0, 'Rocking chair ~1m'),
    'EchohavenBrazier':    ('Prop', 1.5, 'Brazier ~1.2-1.5m'),
    'MercuryBallSpireHero':('Hero', 15.0, 'Spire 15m height per docs/15'),
    'BobsInn':             ('Building', 6.0, 'Inn building'),
    'Apothecary':          ('Building', 5.0, 'Apothecary single-story'),
    'TownHall':            ('Building', 12.0, 'Town hall ~12m'),
    'Watchtower':          ('Building', 15.0, 'Watchtower tall'),
    'VillageBakery':       ('Building', 6.0, 'Single-story bakery'),
    'VillageCottageA':     ('Building', 5.0, 'Cottage'),
    'VillageCottageB':     ('Building', 5.0, 'Cottage'),
    'VillageCottageC':     ('Building', 5.0, 'Cottage'),
    'VillageInn':          ('Building', 6.0, 'Inn'),
    'VillageMill':         ('Building', 8.0, 'Mill with wheel'),
    'VillageSmithy':       ('Building', 5.0, 'Smithy'),
    'VillageWell':         ('Prop', 1.8, 'Well'),
    'VillagerSignpost':    ('Prop', 2.4, 'Signpost ~2.4m'),
    'PipeOrganCathedral':  ('Prop', 2.0, 'Pipe organ ~2m'),
    'MudPoolBasin':        ('Prop', 9.1, 'Mud pool basin ~9m'),
    'MudPoolResonancePad': ('Prop', 5.0, 'Resonance pad'),
    'CarvedStoneObelisk':  ('Prop', 3.0, 'Obelisk'),
    'Bookshelf':           ('Prop', 2.0, 'Bookshelf'),
    'CandelabraTriple':    ('Prop', 1.5, 'Candelabra'),
    'ClayUrn':             ('Prop', 0.5, 'Urn'),
    'CymaticTray':         ('Prop', 0.3, 'Tray'),
    'FireplaceHearth':     ('Prop', 1.0, 'Hearth'),
    'FrequencySliderStand':('Prop', 1.3, 'Stand'),
    'GiantSkeletonKey':    ('Prop', 1.5, 'Giant key long'),
    'GrainSack':           ('Prop', 0.7, 'Sack'),
    'HangingLantern':      ('Prop', 0.5, 'Lantern'),
    'HarmonicTile_Flower': ('Prop', 0.1, 'Floor tile'),
    'HarmonicTile_Spiral': ('Prop', 0.1, 'Floor tile'),
    'HarmonicTile_Square': ('Prop', 0.1, 'Floor tile'),
    'LongBench':           ('Prop', 1.2, 'Bench'),
    'LongDiningTable':     ('Prop', 1.0, 'Table'),
    'LoreArtifactScroll':  ('Prop', 0.5, 'Scroll'),
    'MetalBucket':         ('Prop', 0.45, 'Bucket'),
    'MiloSatchelAndLantern':('Prop',0.7, 'Satchel'),
    'NightStand':          ('Prop', 0.8, 'Nightstand'),
    'PeasantChair':        ('Prop', 1.0, 'Chair'),
    'PureWaterFont':       ('Prop', 1.8, 'Font'),
    'ResonancePlate':      ('Prop', 0.5, 'Plate'),
    'RoseWindowCymatic':   ('Prop', 4.4, 'Rose window 4m'),
    'RoundTable':          ('Prop', 1.0, 'Table'),
    'RugWoven':            ('Prop', 0.05, 'Rug flat'),
    'SkeletonKeySlot':     ('Prop', 0.3, 'Keyhole'),
    'SkeletonRemains':     ('Prop', 1.6, 'Skeleton lying'),
    'StoneFireBrazier':    ('Prop', 1.0, 'Brazier'),
    'StorageChest':        ('Prop', 0.6, 'Chest'),
    'TableLantern':        ('Prop', 0.4, 'Lantern'),
    'ThreeLeggedStool':    ('Prop', 0.7, 'Stool'),
    'TorchOnPost':         ('Prop', 2.5, 'Torch post'),
    'TuningBell_High':     ('Prop', 0.5, 'Bell'),
    'TuningBell_Mid':      ('Prop', 0.5, 'Bell'),
    'TuningBell_Low':      ('Prop', 0.6, 'Bell'),
    'TuningPedestal':      ('Prop', 1.2, 'Pedestal'),
    'WallSconceIron':      ('Prop', 0.6, 'Sconce'),
    'WaveformPillar':      ('Prop', 1.4, 'Pillar'),
    'WoodenBarrel':        ('Prop', 0.8, 'Barrel'),
    'WoodenBed':           ('Prop', 0.6, 'Bed'),
    'WoodenCrate':         ('Prop', 0.7, 'Crate'),
    'WoodenLectern':       ('Prop', 1.4, 'Lectern'),
    'Aether_A3_Crystal_Amber':  ('VFX', 1.1, 'Aether crystal'),
    'Aether_D4_Crystal_PaleGreen':('VFX', 1.1, 'Aether crystal'),
    'Aether_E3_Crystal_BlueIce':('VFX', 1.1, 'Aether crystal'),
    'ResetScout':          ('NPC', 1.8, 'Reset agent humanoid'),
}

mtimes = {}
for f in glob.glob(r'C:\dev\TARTARIA_new\Assets\_Project\Models\Blender\Moon1\*.fbx'):
    mtimes[os.path.basename(f)] = datetime.datetime.fromtimestamp(os.path.getmtime(f))

healthy=[]; proportion_off=[]; broken=[]; unknown=[]

for fbx, bb in bbox.items():
    name = fbx.replace('.fbx','')
    spec = SPEC.get(name)
    mt = mtimes.get(fbx)
    post_fix = mt and mt.strftime('%Y-%m-%d %H:%M') >= '2026-06-04 11:00'

    if bb is None:
        broken.append((fbx, 'LFS_POINTER_130B', spec, mt))
        continue
    if 'err' in bb:
        broken.append((fbx, 'PARSE_ERR_'+bb['err'], spec, mt))
        continue

    h = bb['sy']; w = bb['sx']; d = bb['sz']
    max_dim = max(w, h, d)

    if spec is None:
        unknown.append((fbx, w, h, d, mt))
        continue

    cat, target_h, desc = spec
    if abs(h - target_h) / max(target_h, 0.1) <= 0.25:
        healthy.append((fbx, cat, target_h, w, h, d, mt, post_fix, 'h_match'))
    elif abs(max_dim - target_h) / max(target_h, 0.1) <= 0.25 and post_fix:
        healthy.append((fbx, cat, target_h, w, h, d, mt, post_fix, 'max_match_postfix'))
    else:
        proportion_off.append((fbx, cat, target_h, w, h, d, mt, post_fix))

print("=" * 110)
print(f"HEALTHY ({len(healthy)})")
print("=" * 110)
for x in sorted(healthy):
    fbx, cat, t, w, h, d, mt, post, why = x
    star = "*" if post else " "
    print(f"  {star} {fbx:38s} [{cat:8s}] tH={t:5.2f}  WxHxD={w:6.2f}x{h:6.2f}x{d:6.2f}  via={why}")

print("=" * 110)
print(f"PROPORTION OFF ({len(proportion_off)})")
print("=" * 110)
for x in sorted(proportion_off):
    fbx, cat, t, w, h, d, mt, post = x
    star = "POSTFIX-OFF" if post else "PREFIX-STALE"
    print(f"  {star:12s} {fbx:38s} [{cat:8s}] tH={t:5.2f}  WxHxD={w:6.2f}x{h:6.2f}x{d:6.2f}")

print("=" * 110)
print(f"BROKEN ({len(broken)})")
print("=" * 110)
for x in broken:
    fbx, why, spec, mt = x
    print(f"  {fbx:38s} {why}")

print("=" * 110)
print(f"UNKNOWN/NO_SPEC ({len(unknown)})")
print("=" * 110)
for x in sorted(unknown):
    fbx, w, h, d, mt = x
    print(f"  {fbx:38s} WxHxD={w:6.2f}x{h:6.2f}x{d:6.2f}")

print()
print(f"TOTALS: healthy={len(healthy)} off={len(proportion_off)} broken={len(broken)} unknown={len(unknown)}")
