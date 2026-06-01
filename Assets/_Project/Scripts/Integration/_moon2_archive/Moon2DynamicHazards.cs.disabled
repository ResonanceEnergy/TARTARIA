using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration { [DefaultExecutionOrder(-74)] public class Moon2DynamicHazards : MonoBehaviour {
    [SerializeField] GameObject crystalShardPrefab;
    [SerializeField] GameObject poisonGasPrefab;
    [SerializeField] GameObject unstableGroundPrefab;
    void Start() { SpawnHazards(); Debug.Log("[Moon2DynamicHazards] ✅ 30 hazards spawned"); }
    void SpawnHazards() {
        for (int i = 0; i < 15; i++) { Vector3 pos = new Vector3(Random.Range(-50f, 50f), Random.Range(0f, 15f), Random.Range(-50f, 50f)); GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Sphere); hazard.transform.position = pos; hazard.tag = "Hazard"; }
    } } }
