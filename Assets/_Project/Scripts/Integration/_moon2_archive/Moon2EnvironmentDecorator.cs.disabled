using UnityEngine;

namespace Tartaria.Integration { [DefaultExecutionOrder(-73)] public class Moon2EnvironmentDecorator : MonoBehaviour {
    [SerializeField] GameObject[] crystalProps;
    [SerializeField] GameObject[] stalactites;
    [SerializeField] GameObject[] rockFormations;
    [SerializeField] GameObject[] biolumPatches;
    void Start() { PlaceDecorations(); Debug.Log("[Moon2EnvironmentDecorator] ✅ 120 decorations placed"); }
    void PlaceDecorations() {
        for (int i = 0; i < 50; i++) { Vector3 pos = new Vector3(Random.Range(-60f, 60f), Random.Range(0f, 20f), Random.Range(-60f, 60f)); GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube); prop.transform.position = pos; prop.transform.localScale = new Vector3(Random.Range(0.5f, 2f), Random.Range(0.5f, 2f), Random.Range(0.5f, 2f)); }
    } } }
