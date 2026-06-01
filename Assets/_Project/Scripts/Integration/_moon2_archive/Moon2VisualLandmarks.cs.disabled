using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration { [DefaultExecutionOrder(-79)] public class Moon2VisualLandmarks : MonoBehaviour {
    [SerializeField] GameObject crystalFormationPrefab;
    [SerializeField] GameObject ancientMiningEquipmentPrefab;
    [SerializeField] GameObject stalagmiteClusterPrefab;
    readonly List<GameObject> _landmarks = new();
    void Start() { SpawnLandmarks(); Debug.Log($"[Moon2VisualLandmarks] ✅ {_landmarks.Count} landmarks placed"); }
    void SpawnLandmarks() {
        Vector3[] positions = { new Vector3(0f, 5f, 30f), new Vector3(-20f, 3f, 20f), new Vector3(20f, 3f, 20f), new Vector3(0f, 0f, 50f) };
        foreach (var pos in positions) {
            GameObject landmark = crystalFormationPrefab != null ? Instantiate(crystalFormationPrefab, pos, Quaternion.identity, transform) : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landmark.transform.position = pos; landmark.transform.localScale = new Vector3(2f, 8f, 2f); _landmarks.Add(landmark);
        }
    } } }
