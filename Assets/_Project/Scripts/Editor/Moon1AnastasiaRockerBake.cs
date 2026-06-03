// H.L5 — Bake the AnastasiaRocker background prop as an authored text-mode prefab.
//
// Sprint 11 L8 50ff78ea found Moon1AnastasiaRocker.cs:32-166 building Anastasia,
// her rocking chair, the proximity trigger and the HumSource procedurally every
// Play, with primitives + procedural sine clip. Per the NO-DEBT mandate this
// authoring belongs in an asset, not a Start() hook.
//
// This editor menu loads the now-real FBX assets:
//   - Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx
//   - Assets/_Project/Models/Blender/Moon1/AnastasiaRockingChair.fbx
// the existing Building_Hum.wav (Assets/_Project/Audio/Building_Hum.wav),
// and writes a text-mode authored prefab to:
//   Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab
//
// The runtime Moon1AnastasiaRocker.cs then instantiates that prefab via
// AssetDatabase (Editor) / Resources (runtime) — zero procedural primitives.
//
// Invoke from the editor menu: Tartaria/6 Bake/Bake Anastasia Rocker Prefab.
// Headless invoke:
//   Unity.exe -batchmode -nographics -quit -projectPath <worktree>
//             -executeMethod Tartaria.Editor.Moon1AnastasiaRockerBake.BakeFromCli

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    public static class Moon1AnastasiaRockerBake
    {
        // Authored placement matches the legacy procedural code: outside the
        // Cathedral entrance, facing the village square.
        public const string PrefabPath = "Assets/_Project/Prefabs/Moon1/AnastasiaRocker.prefab";

        const string AnastasiaFbxPath = "Assets/_Project/Models/Blender/Moon1/AnastasiaPrincess.fbx";
        const string RockingChairFbxPath = "Assets/_Project/Models/Blender/Moon1/AnastasiaRockingChair.fbx";
        const string HumClipPath = "Assets/_Project/Audio/Building_Hum.wav";

        static readonly Vector3 ChairLocalPosition = new Vector3(3f, 0f, 22f);
        static readonly Quaternion ChairLocalRotation = Quaternion.Euler(0f, 195f, 0f);
        static readonly Vector3 AnastasiaSeatLocalPosition = new Vector3(0f, 0.55f, 0f);
        static readonly Vector3 AnastasiaSeatLocalScale = Vector3.one * 0.9f;
        const float TriggerRadius = 5f;
        const float HumVolume = 0.35f;
        const float HumMinDistance = 2f;
        const float HumMaxDistance = 14f;

        [MenuItem("Tartaria/6 Bake/Bake Anastasia Rocker Prefab", priority = 630)]
        public static void BakeFromMenu()
        {
            var path = Bake();
            if (!string.IsNullOrEmpty(path))
                EditorUtility.DisplayDialog(
                    "Anastasia Rocker Prefab",
                    "Baked text-mode prefab:\n" + path,
                    "OK");
        }

        public static void BakeFromCli()
        {
            Bake();
        }

        /// <summary>
        /// Bake the AnastasiaRocker prefab. Returns the asset path on success,
        /// null on failure. Throws on missing source assets so the CLI exits non-zero.
        /// </summary>
        public static string Bake()
        {
            EnsureForceText();
            EnsurePrefabDirectory();

            var anastasiaFbx = AssetDatabase.LoadAssetAtPath<GameObject>(AnastasiaFbxPath);
            if (anastasiaFbx == null)
                throw new FileNotFoundException("Missing FBX: " + AnastasiaFbxPath);

            var chairFbx = AssetDatabase.LoadAssetAtPath<GameObject>(RockingChairFbxPath);
            if (chairFbx == null)
                throw new FileNotFoundException("Missing FBX: " + RockingChairFbxPath);

            var humClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HumClipPath);
            if (humClip == null)
                throw new FileNotFoundException("Missing AudioClip: " + HumClipPath);

            var root = new GameObject("AnastasiaRocker_BG_AtCathedral");
            try
            {
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;

                // Rocking chair child — instantiate from FBX, parent under root.
                var chair = (GameObject)PrefabUtility.InstantiatePrefab(chairFbx);
                chair.name = "AnastasiaRockingChair";
                chair.transform.SetParent(root.transform, false);
                chair.transform.localPosition = ChairLocalPosition;
                chair.transform.localRotation = ChairLocalRotation;
                var anim = chair.AddComponent<Moon1ChairRockAnimator>();
                anim.amplitudeDeg = 6f;
                anim.speed = 1.2f;

                // Anastasia child — instantiate from FBX, seated on chair.
                var anastasia = (GameObject)PrefabUtility.InstantiatePrefab(anastasiaFbx);
                anastasia.name = "Anastasia_OnChair";
                anastasia.transform.SetParent(chair.transform, false);
                anastasia.transform.localPosition = AnastasiaSeatLocalPosition;
                anastasia.transform.localRotation = Quaternion.identity;
                anastasia.transform.localScale = AnastasiaSeatLocalScale;

                // Proximity trigger — BoxCollider per mission, sized to legacy 5m sphere
                // radius so the greeting volume covers the seating area + a meter walkway.
                var triggerGO = new GameObject("AnastasiaProximityTrigger");
                triggerGO.transform.SetParent(chair.transform, false);
                triggerGO.transform.localPosition = Vector3.zero;
                triggerGO.transform.localRotation = Quaternion.identity;
                var trigger = triggerGO.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = Vector3.zero;
                trigger.size = new Vector3(TriggerRadius * 2f, 2f, TriggerRadius * 2f);
                var listener = triggerGO.AddComponent<Moon1AnastasiaProximityListener>();
                // listener.parent is resolved at runtime from the prefab root's
                // Moon1AnastasiaRocker — set by Moon1AnastasiaRocker.Start().

                // HumSource — real Building_Hum.wav, looping, 3D spatial.
                var humGO = new GameObject("HumSource");
                humGO.transform.SetParent(chair.transform, false);
                humGO.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                var hum = humGO.AddComponent<AudioSource>();
                hum.clip = humClip;
                hum.loop = true;
                hum.playOnAwake = true;
                hum.spatialBlend = 1f;
                hum.rolloffMode = AudioRolloffMode.Linear;
                hum.minDistance = HumMinDistance;
                hum.maxDistance = HumMaxDistance;
                hum.volume = HumVolume;

                if (File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
                var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out var success);
                if (!success || saved == null)
                    throw new System.Exception("Failed to save prefab: " + PrefabPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Moon1AnastasiaRockerBake] Baked text-mode prefab: " + PrefabPath);
                return PrefabPath;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void EnsurePrefabDirectory()
        {
            var dir = Path.GetDirectoryName(PrefabPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        // Per CLAUDE.md text-mode prefab mandate — make sure ForceText is on
        // before we serialize, so the .prefab on disk is human-diffable YAML.
        static void EnsureForceText()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
                EditorSettings.serializationMode = SerializationMode.ForceText;
        }
    }
}
#endif
