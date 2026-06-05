// File: Assets/_Project/Scripts/Editor/Moon2BuildOutCavern.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    [MenuItem("Tartaria/Moon 2/Build Out Crystalline Cavern")]
    public static void Run()
    {
        var existing = GameObject.Find("Moon2_Cavern_Root");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Cavern Exists",
                "Moon2_Cavern_Root already in scene. Rebuild from scratch?",
                "Rebuild", "Cancel")) return;
            Object.DestroyImmediate(existing);
        }

        var root = new GameObject("Moon2_Cavern_Root");
        root.transform.position = new Vector3(-80f, 0f, 0f);

        BuildEntryPortal(root);     // 1 Cathedral Archway prefab + 2 columns
        BuildCavernFloor(root);     // a flat plane primitive, dark stone color
        BuildStalactites(root, 12); // 12 hanging spikes from a ceiling at Y=12
        Build7Crystals(root);       // 7 DissonanceCrystal MonoBehaviours scattered

        // Mark dirty so the Editor saves
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Moon2BuildOutCavern] Built Moon 2 cavern at (-80, 0, 0)");
    }

    private static void BuildEntryPortal(GameObject root)
    {
        var archway = LoadCathedral("Archway_4x7m.prefab");
        archway.transform.SetParent(root.transform, false);
        archway.transform.localPosition = new Vector3(-50f, 0f, 0f);

        var column1 = LoadCathedral("Column_Ornate.prefab");
        column1.transform.SetParent(archway.transform, false);
        column1.transform.localPosition = new Vector3(-25f, 0f, 0f);

        var column2 = LoadCathedral("Column_Ornate.prefab");
        column2.transform.SetParent(archway.transform, false);
        column2.transform.localPosition = new Vector3(25f, 0f, 0f);
    }

    private static void BuildCavernFloor(GameObject root)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.transform.SetParent(root.transform, false);
        floor.transform.localPosition = new Vector3(-15f, 0f, -15f);
        floor.transform.localScale = new Vector3(30f, 1f, 30f);
        floor.AddComponent<MeshRenderer>();
        floor.GetComponent<MeshRenderer>().material.color = Color.black;
    }

    private static void BuildStalactites(GameObject root, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.transform.SetParent(root.transform, false);
            spike.transform.localPosition = new Vector3(Random.Range(-15f, 15f), 12f, Random.Range(-15f, 15f));
            spike.transform.localScale = new Vector3(0.5f, 4f + Random.Range(-1f, 1.5f), 0.5f);
            spike.AddComponent<MeshRenderer>();
            spike.GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
    }

    private static void Build7Crystals(GameObject root)
    {
        for (int i = 0; i < 7; i++)
        {
            var crystal = new GameObject("Moon2_DissonanceCrystal_" + i);
            crystal.transform.SetParent(root.transform, false);
            crystal.transform.localPosition = CRYSTAL_LOCAL_POS[i];
            crystal.AddComponent<Tartaria.Gameplay.DissonanceCrystal>();
        }
    }
}
