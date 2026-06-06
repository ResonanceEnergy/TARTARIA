"""Map each gen_*.py to the FBX names it exports."""
import re, glob, os, json

results = {}
for fp in sorted(glob.glob('gen_*.py')):
    s = open(fp, encoding='utf-8', errors='ignore').read()
    exports = re.findall(r"""export_(?:fbx|current_as)\(\s*["']([^"']+)["']""", s)
    has_transform_apply = 'transform_apply' in s
    has_legacy_scale = 'FBX_SCALE_NONE' in s and 'previously' not in s.split('FBX_SCALE_NONE')[0][-200:]
    uses_helper = ('from _common' in s) or ('import _common' in s)
    results[fp] = {
        'exports': exports,
        'count': len(exports),
        'uses_helper': uses_helper,
        'has_legacy_scale': has_legacy_scale,
    }
    print(f"{fp:42s} exports={len(exports):3d}  helper={uses_helper}  legacy_scale={has_legacy_scale}")
    for e in exports:
        print(f"    -> {e}")

json.dump(results, open(r'C:\Users\gripa\AppData\Local\Temp\gen_exports.json','w'), indent=1)
