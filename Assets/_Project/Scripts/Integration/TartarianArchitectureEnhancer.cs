// File: Assets/_Project/Scripts/Integration/TartarianArchitectureEnhancer.cs
using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Integration
{
    public class TartarianArchitectureEnhancer : MonoBehaviour
    {
        public void EnhanceBuilding(GameObject building, string buildingId)
        {
            AddCapital(building, new Vector3(0, 1.5f, 0));
            AddFrieze(building);
            AddFinial(building);
            AddRoseRing(building);
            AddButtress(building);
        }

        public void EnhanceAll()
        {
            var buildings = FindObjectsOfType<InteractableBuilding>();
            foreach (var building in buildings)
            {
                EnhanceBuilding(building.gameObject, building.BuildingId);
            }
            Debug.Log($"Enhanced {buildings.Length} buildings.");
        }

        private void AddCapital(GameObject parent, Vector3 localPos)
        {
            var prefab = LoadKit("Spire_Top_MercuryBall.prefab");
            GameObject capital;
            if (prefab != null)
            {
                capital = Instantiate(prefab, parent.transform);
            }
            else
            {
                capital = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                capital.transform.SetParent(parent.transform);
                ApplyURPStone(capital);
                Debug.LogWarning("rchEnhancer] Spire_Top_MercuryBall.prefab missing — fallback sphere capital");
            }
            capital.transform.localPosition = localPos;
            capital.transform.localScale = Vector3.one * 0.25f;
        }

        private void AddFrieze(GameObject parent)
        {
            var prefab = LoadKit("Archway_4x7m.prefab");
            GameObject frieze;
            if (prefab != null)
            {
                frieze = Instantiate(prefab, parent.transform);
            }
            else
            {
                frieze = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                frieze.transform.SetParent(parent.transform);
                ApplyURPStone(frieze);
                Debug.LogWarning("rchEnhancer] Archway_4x7m.prefab missing — fallback sphere frieze");
            }
            frieze.transform.localPosition = new Vector3(0, 1.5f, 0);
            frieze.transform.localScale = new Vector3(4, 0.15f, 4);
        }

        private void AddFinial(GameObject parent)
        {
            var prefab = LoadKit("Spire_Top_MercuryBall.prefab");
            GameObject finial;
            if (prefab != null)
            {
                finial = Instantiate(prefab, parent.transform);
            }
            else
            {
                finial = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                finial.transform.SetParent(parent.transform);
                ApplyURPStone(finial);
                Debug.LogWarning("rchEnhancer] Spire_Top_MercuryBall.prefab missing — fallback sphere finial");
            }
            finial.transform.localPosition = new Vector3(0, 1.5f, 0);
            finial.transform.localScale = Vector3.one * 0.4f;
        }

        private void AddRoseRing(GameObject parent)
        {
            var prefab = LoadKit("RoseWindow_4x4m.prefab");
            GameObject roseRing;
            if (prefab != null)
            {
                roseRing = Instantiate(prefab, parent.transform);
            }
            else
            {
                roseRing = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                roseRing.transform.SetParent(parent.transform);
                ApplyURPStone(roseRing);
                Debug.LogWarning("rchEnhancer] RoseWindow_4x4m.prefab missing — fallback sphere rose ring");
            }
            roseRing.transform.localPosition = new Vector3(0, 1.5f, 0);
            roseRing.transform.localScale = new Vector3(2.5f, 0.5f, 2.5f);
        }

        private void AddButtress(GameObject parent)
        {
            var prefab = LoadKit("Column_Ornate_6.5m.prefab");
            GameObject buttress;
            if (prefab != null)
            {
                buttress = Instantiate(prefab, parent.transform);
            }
            else
            {
                buttress = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                buttress.transform.SetParent(parent.transform);
                ApplyURPStone(buttress);
                Debug.LogWarning("rchEnhancer] Column_Ornate_6.5m.prefab missing — fallback sphere buttress");
            }
            buttress.transform.localPosition = new Vector3(0, 1.5f, 0);
            buttress.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
            buttress.transform.Rotate(Vector3.up, 8f); // 8° tilt outward
        }

        private void ApplyURPStone(GameObject obj)
        {
            var material = Resources.Load<Material>("Assets/_Project/Models/Blender/Moon1/Stone");
            if (material != null)
            {
                obj.GetComponent<Renderer>().material = material;
            }
        }
    }
}
