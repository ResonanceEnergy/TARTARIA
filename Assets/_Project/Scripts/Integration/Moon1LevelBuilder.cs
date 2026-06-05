// File: Assets/_Project/Scripts/Integration/Moon1LevelBuilder.cs
using System;
using UnityEngine;

namespace Tartaria.Integration
{
    public class Moon1LevelBuilder : MonoBehaviour
    {
        public void BuildVillage(Transform parent)
        {
            var foundationPrefab = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Foundation_16x16m.prefab");
            var wallPrefab       = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Wall_4x4m_Stone.prefab");
            var columnPrefab     = LoadPrefab("Assets/_Project/Prefabs/Moon1/Cathedral/Column_Ornate_6.5m.prefab");

            for (int i = 0; i < VILLAGE_POSITIONS.Length; i++)
            {
                var center = VILLAGE_POSITIONS[i];
                var root = new GameObject("Moon1_Village_" + i);
                root.transform.SetParent(parent, false);
                root.transform.position = center;

                if (foundationPrefab != null)
                {
                    var f = Instantiate(foundationPrefab, root.transform);
                    f.transform.localPosition = Vector3.zero;
                    f.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
                    ApplyURPStone(f);
                }

                // Two wall fragments offset to the back + side
                var wall1 = Instantiate(wallPrefab, root.transform);
                wall1.transform.localPosition = new Vector3(-2.0f, 0.0f, -2.0f);
                wall1.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
                ApplyURPStone(wall1);

                var wall2 = Instantiate(wallPrefab, root.transform);
                wall2.transform.localPosition = new Vector3(2.0f, 0.0f, 2.0f);
                wall2.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
                ApplyURPStone(wall2);

                // Broken column tilted ~15°
                var column = Instantiate(columnPrefab, root.transform);
                column.transform.localPosition = new Vector3(0.0f, -1.0f, 0.0f);
                column.transform.localScale = new Vector3(0.7f, 0.6f, 0.7f);
                column.transform.rotation = Quaternion.Euler(0.0f, 15.0f, 0.0f);
                ApplyURPStone(column);

                // Prop at the base for "lived-in" feel
                var prop = Instantiate(LoadPrefab("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/anvil.fbx"), root.transform);
                prop.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);
                prop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                prop = Instantiate(LoadPrefab("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/grindstone.fbx"), root.transform);
                prop.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);
                prop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                prop = Instantiate(LoadPrefab("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/bucket_metal.fbx"), root.transform);
                prop.transform.localPosition = new Vector3(0.0f, -2.0f, 0.0f);
                prop.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }
}
