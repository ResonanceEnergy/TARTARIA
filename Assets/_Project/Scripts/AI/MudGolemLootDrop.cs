using UnityEngine;
using Tartaria.Core;

namespace Tartaria.AI
{
    public class MudGolemLootDrop : MonoBehaviour
    {
        private Vector3 cachedPosition;

        void OnEnable()
        {
            cachedPosition = transform.position;
        }

        public void DropLoot(GameObject killer = null)
        {
            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                GameObject shard = 
GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.transform.localScale = new Vector3(0.25f, 0.18f, 
0.25f);
                shard.transform.position = cachedPosition + Vector3.up * 
0.5f;
                shard.transform.rotation = Quaternion.Euler(0, 
Random.Range(0, 360), 0);

                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    var mat = new Material(urpLit);
                    mat.SetColor("_BaseColor", new Color(0.32f, 0.22f, 
0.14f));
                    foreach (var r in 
shard.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
                }

                shard.AddComponent<Rigidbody>().mass = 0.4f;
                shard.GetComponent<Rigidbody>().AddForce(new 
Vector3(Random.Range(-2, 2), Random.Range(2, 4), Random.Range(-2, 2)));
                shard.AddComponent<SphereCollider>().radius = 0.4f;
                Destroy(shard, 8f);
            }

            GameObject rsCoin = 
GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rsCoin.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            rsCoin.transform.position = cachedPosition + Vector3.up * 
0.5f;
            rsCoin.AddComponent<_LootRSCoin>();

            var urpLit2 = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit2 != null)
            {
                var mat = new Material(urpLit2);
                mat.SetColor("_BaseColor", new Color(1.0f, 0.86f, 0.30f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 
0.20f) * 1.4f);
                foreach (var r in 
rsCoin.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
            }

            rsCoin.AddComponent<Rigidbody>().mass = 0.3f;
            rsCoin.AddComponent<SphereCollider>().radius = 0.6f;
        }
    }

    public class _LootShard : MonoBehaviour { /* no-op; just for tagging 
the shard GO */ }
    public class _LootRSCoin : MonoBehaviour
    {
        void Update() { transform.Rotate(0f, 60f * Time.deltaTime, 0f, 
Space.World); }
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Tartaria.Core.GameEvents.FireRSChange(8f);
            /* PlaySFX: ServiceLocator.Audio not in scope */
            Destroy(gameObject);
        }
    }
}
