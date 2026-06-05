// File: Assets/_Project/Scripts/Integration/Moon1HeroBuildingSpawner.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Integration
{
    public class Moon1HeroBuildingSpawner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private void Initialize()
        {
            SpawnHeroBuildings();
        }

        public void SpawnHeroBuildings()
        {
            SpawnDome(Vector3.zero);
            SpawnFountain(Vector3.zero);
            SpawnSpire(Vector3.zero);
        }

        public GameObject SpawnDome(Vector3 pos)
        {
            var foundation = LoadCathedralPrefab("Foundation_16x16m.prefab");
            if (foundation == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Foundation_16x16m — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }

            var dome = Instantiate(foundation, pos, Quaternion.identity);
            dome.transform.position += new Vector3(0, -3.5f, 0);

            // Add corners
            var corner1 = Instantiate(LoadCathedralPrefab("Wall_Corner_4x4m.prefab"), pos + new Vector3(-2, 0, 0), Quaternion.identity);
            var corner2 = Instantiate(LoadCathedralPrefab("Wall_Corner_4x4m.prefab"), pos + new Vector3(2, 0, 0), Quaternion.identity);
            var corner3 = Instantiate(LoadCathedralPrefab("Wall_Corner_4x4m.prefab"), pos + new Vector3(0, -2, 0), Quaternion.identity);
            var corner4 = Instantiate(LoadCathedralPrefab("Wall_Corner_4x4m.prefab"), pos + new Vector3(0, 2, 0), Quaternion.identity);

            // Add walls
            var wall1 = Instantiate(LoadCathedralPrefab("Wall_4x4m_Stone.prefab"), pos + new Vector3(-1, 0, -1), Quaternion.identity);
            var wall2 = Instantiate(LoadCathedralPrefab("Wall_4x4m_Stone.prefab"), pos + new Vector3(1, 0, -1), Quaternion.identity);
            var wall3 = Instantiate(LoadCathedralPrefab("Wall_4x4m_Stone.prefab"), pos + new Vector3(-1, 0, 1), Quaternion.identity);
            var wall4 = Instantiate(LoadCathedralPrefab("Wall_4x4m_Stone.prefab"), pos + new Vector3(1, 0, 1), Quaternion.identity);

            // Add door
            var door = Instantiate(LoadCathedralPrefab("Door_Grand_3x6m.prefab"), pos + new Vector3(0, 3, -1), Quaternion.identity);

            // Add rose window
            var roseWindow = Instantiate(LoadCathedralPrefab("RoseWindow_4x4m.prefab"), pos + new Vector3(0, 3, 1), Quaternion.identity);

            // Add columns
            var column1 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(-2, 0, -2), Quaternion.identity);
            var column2 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(2, 0, -2), Quaternion.identity);
            var column3 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(-2, 0, 2), Quaternion.identity);
            var column4 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(2, 0, 2), Quaternion.identity);

            // Add dome segments
            for (int i = 0; i < 8; i++)
            {
                var segment = Instantiate(LoadCathedralPrefab($"Dome_Segment_{i}.prefab"), pos + new Vector3(0, 0, i * 2), Quaternion.identity);
                segment.transform.position += new Vector3(0, -3.5f, 0);
            }

            return dome;
        }

        public GameObject SpawnFountain(Vector3 pos)
        {
            var foundation = LoadCathedralPrefab("Foundation_16x16m.prefab");
            if (foundation == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Foundation_16x16m — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }

            var fountain = Instantiate(foundation, pos, Quaternion.identity);
            fountain.transform.position += new Vector3(0, -3.5f, 0);

            // Add columns
            var column1 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(-2, 0, -2), Quaternion.identity);
            var column2 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(2, 0, -2), Quaternion.identity);
            var column3 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(-2, 0, 2), Quaternion.identity);
            var column4 = Instantiate(LoadCathedralPrefab("Column_Ornate_6.5m.prefab"), pos + new Vector3(2, 0, 2), Quaternion.identity);

            // Add water font mount
            var waterFontMount = Instantiate(Resources.Load<GameObject>("WaterFontMount"), pos + new Vector3(0, 3, -1), Quaternion.identity);

            return fountain;
        }

        public GameObject SpawnSpire(Vector3 pos)
        {
            var foundation = LoadCathedralPrefab("Foundation_16x16m.prefab");
            if (foundation == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Foundation_16x16m — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }

            var spire = Instantiate(foundation, pos, Quaternion.identity);
            spire.transform.position += new Vector3(0, -3.5f, 0);

            // Add base
            var basePrefab = LoadCathedralPrefab("Spire_Base_2x2m.prefab");
            if (basePrefab == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Spire_Base_2x2m — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }
            var baseInstance = Instantiate(basePrefab, pos + new Vector3(0, 0, 0), Quaternion.identity);

            // Add mid taper
            var midTaperPrefab = LoadCathedralPrefab("Spire_Mid_Taper.prefab");
            if (midTaperPrefab == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Spire_Mid_Taper — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }
            var midTaperInstance = Instantiate(midTaperPrefab, pos + new Vector3(0, 0, 0), Quaternion.identity);

            // Add top mercury ball
            var topMercuryBallPrefab = LoadCathedralPrefab("Spire_Top_MercuryBall.prefab");
            if (topMercuryBallPrefab == null)
            {
                Debug.LogWarning($"[Moon1HeroBuildingSpawner] Missing prefab: Spire_Top_MercuryBall — using marker cube");
                return Instantiate(Resources.Load<GameObject>("MarkerCube"), pos, Quaternion.identity);
            }
            var topMercuryBallInstance = Instantiate(topMercuryBallPrefab, pos + new Vector3(0, 0, 0), Quaternion.identity);

            return spire;
        }
    }
}
