#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildOutNPCs — places Milo, Anastasia, Lirael, Cassian, Bob at the
    /// canonical spec positions per docs/15 §11 and docs/03 Moon 1 NPC arrivals.
    ///
    ///   Milo       at Dome ventilation shaft  (-26,  0, 26)   active     Day 1
    ///   Anastasia  at Spire                   ( 32,  0, 22)   inactive   Reveal after Dome restored
    ///   Lirael     at Fountain                (  5, 1.5, 48)  inactive   Day-25 gate (TODO event)
    ///   Cassian    wandering village          (  3,  0, 35)   active     Day 1
    ///   Bob        at Bob's Inn               ( 12,  0, -10)  active     Day 1
    ///
    /// Idempotent — re-runs reposition / re-activate without duplicating.
    /// </summary>
    public static class Moon1BuildOutNPCs
    {
        [MenuItem("Tartaria/1 Build/Build Out Moon 1 NPCs (Milo + Anastasia + Lirael + Cassian + Bob)", priority = 105)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("NPCs", "No active scene.", "OK");
                return;
            }

            var parent = GameObject.Find("Echohaven_NPCs");
            if (parent == null)
            {
                parent = new GameObject("Echohaven_NPCs");
                Undo.RegisterCreatedObjectUndo(parent, "Create NPCs parent");
            }

            int placed = 0, repositioned = 0, missing = 0;

            placed += PlaceOrUpdate(ref repositioned, ref missing, parent,
                "Milo_AtDomeShaft",
                "Assets/_Project/Prefabs/Characters/Milo.prefab",
                new Vector3(-26f, 0f, 26f),
                Quaternion.Euler(0f, 45f, 0f),
                activeAtStart: true,
                gateNote: "Day 1 — companion emerges from Dome shaft");

            placed += PlaceOrUpdate(ref repositioned, ref missing, parent,
                "Anastasia_AtSpire",
                "Assets/_Project/Prefabs/Characters/Anastasia.prefab",
                new Vector3(32f, 0f, 22f),
                Quaternion.Euler(0f, -135f, 0f),
                activeAtStart: false,
                gateNote: "Reveal via GameEvents.OnBuildingRestored('echohaven_stardome')");

            placed += PlaceOrUpdate(ref repositioned, ref missing, parent,
                "Lirael_AtFountain",
                "Assets/_Project/Prefabs/Characters/Lirael.prefab",
                new Vector3(5f, 1.5f, 48f),
                Quaternion.identity,
                activeAtStart: false,
                gateNote: "Day >= 25 (TODO: hook GameEvents.OnDayChanged when it exists)");

            placed += PlaceOrUpdate(ref repositioned, ref missing, parent,
                "Cassian_Wandering",
                "Assets/_Project/Prefabs/Characters/Cassian.prefab",
                new Vector3(3f, 0f, 35f),
                Quaternion.identity,
                activeAtStart: true,
                gateNote: "Day 1 — wanders village square");

            placed += PlaceOrUpdate(ref repositioned, ref missing, parent,
                "Bob_AtInn",
                "Assets/_Project/Prefabs/Moon1/Blender/NPCs/BobInnkeeper.prefab",
                new Vector3(12f, 0f, -10f),
                Quaternion.Euler(0f, 180f, 0f),
                activeAtStart: true,
                gateNote: "Day 1 — at Bob's Inn entrance");

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = parent;

            EditorUtility.DisplayDialog("Build Out Moon 1 NPCs",
                $"NPCs:\n  Placed new: {placed}\n  Repositioned existing: {repositioned}\n  Missing prefabs: {missing}\n\n" +
                "Per spec:\n" +
                "  Milo + Cassian + Bob start ACTIVE.\n" +
                "  Anastasia INACTIVE until Dome restoration.\n" +
                "  Lirael INACTIVE until Day 25 (TODO: OnDayChanged event).\n",
                "OK");
        }

        static int PlaceOrUpdate(ref int repositioned, ref int missing, GameObject parent,
            string id, string prefabPath, Vector3 position, Quaternion rotation,
            bool activeAtStart, string gateNote)
        {
            var existing = FindInChildrenByName(parent.transform, id);
            if (existing != null)
            {
                Undo.RecordObject(existing.transform, "Reposition NPC");
                existing.transform.position = position;
                existing.transform.rotation = rotation;
                existing.SetActive(activeAtStart);
                repositioned++;
                Debug.Log($"[Moon1BuildOutNPCs] Repositioned {id} → {position} (active={activeAtStart}). Note: {gateNote}");
                return 0;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Moon1BuildOutNPCs] Prefab missing: {prefabPath} — skipping {id}");
                missing++;
                return 0;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            instance.name = id;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(activeAtStart);
            Undo.RegisterCreatedObjectUndo(instance, "Place NPC");
            Debug.Log($"[Moon1BuildOutNPCs] Placed {id} at {position} (active={activeAtStart}). Note: {gateNote}");
            return 1;
        }

        static GameObject FindInChildrenByName(Transform t, string name)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (c.name == name) return c.gameObject;
            }
            return null;
        }
    }
}
#endif
