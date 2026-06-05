using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration { [DefaultExecutionOrder(-75)] public class Moon2PowerUps : MonoBehaviour {
    [SerializeField] GameObject rsBoostPrefab;
    [SerializeField] GameObject combatBoostPrefab;
    [SerializeField] GameObject healingOrbPrefab;
    void Start() { SpawnInitialPowerUps(); Debug.Log("[Moon2PowerUps] ✅ Power-ups spawned"); }
    void SpawnInitialPowerUps() { for (int i = 0; i < 10; i++) { Vector3 pos = new Vector3(Random.Range(-40f, 40f), Random.Range(0f, 10f), Random.Range(-40f, 40f)); GameObject pickup = rsBoostPrefab != null ? Instantiate(rsBoostPrefab, pos, Quaternion.identity, transform) : GameObject.CreatePrimitive(PrimitiveType.Sphere); pickup.transform.position = pos; pickup.transform.localScale = Vector3.one * 0.5f; } } } }
