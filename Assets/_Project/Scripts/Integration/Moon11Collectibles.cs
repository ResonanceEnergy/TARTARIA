using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-45)]
    public class Moon11Collectibles : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Collectibles")]
        [SerializeField] int aetherFragmentCount = 15;
        [SerializeField] int ancientRelicCount = 8;
        [SerializeField] int hiddenCacheCount = 5;
        List<GameObject> collectibles = new List<GameObject>();
        void Start() { SpawnCollectibles(); }
        void SpawnCollectibles()
        {
            for (int i = 0; i < aetherFragmentCount; i++)
                CreateCollectible($"AetherFragment_Moon11_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 3f), Random.Range(-65f, 65f)), "AetherFragment", 5f, new Color(1f, 0.6f, 1f));
            for (int i = 0; i < ancientRelicCount; i++)
                CreateCollectible($"SpectrumRelic_Moon11_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 2f), Random.Range(-65f, 65f)), "SpectrumRelic", 15f, new Color(0.9f, 0.5f, 0.9f));
            for (int i = 0; i < hiddenCacheCount; i++)
                CreateCollectible($"HiddenCache_Moon11_{i}", new Vector3(Random.Range(-70f, 70f), Random.Range(0.5f, 4f), Random.Range(-70f, 70f)), "HiddenCache", 30f, new Color(0.9f, 0.5f, 0.9f));
            Debug.Log($"💎 Moon11Collectibles spawned {collectibles.Count} collectibles");
        }
        GameObject CreateCollectible(string name, Vector3 position, string collectibleType, float rsReward, Color glowColor)
        {
            GameObject collectible = new GameObject(name);
            collectible.transform.position = position;
            collectible.tag = "Interactable";
            SphereCollider trigger = collectible.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.5f;
            CollectibleItem item = collectible.AddComponent<CollectibleItem>();
            item.itemId = $"moon11_{collectibleType.ToLower()}_{name}";
            item.collectibleType = collectibleType;
            item.rsReward = rsReward;
            item.displayName = collectibleType switch { "AetherFragment" => "Prismatic Aether Fragment", "SpectrumRelic" => "Ancient Prismatic Relic", "HiddenCache" => "Hidden RainbowCache", _ => "Unknown Item" };
            GameObject visual = GameObject.CreatePrimitive(collectibleType == "HiddenCache" ? PrimitiveType.Cube : PrimitiveType.Sphere);
            visual.transform.SetParent(collectible.transform, false);
            visual.transform.localPosition = Vector3.zero;
            float scale = collectibleType == "HiddenCache" ? 0.8f : 0.4f;
            visual.transform.localScale = Vector3.one * scale;
            Destroy(visual.GetComponent<Collider>());
            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = glowColor;
                mat.SetColor("_EmissionColor", glowColor * 2f);
                mat.EnableKeyword("_EMISSION");
                rend.material = mat;
            }
            BobAnimation bobber = collectible.AddComponent<BobAnimation>();
            bobber.bobSpeed = 1f;
            bobber.bobHeight = 0.3f;
            bobber.rotationSpeed = 45f;
            collectibles.Add(collectible);
            return collectible;
        }
        void OnDestroy() { foreach (GameObject collectible in collectibles) if (collectible != null) Destroy(collectible); collectibles.Clear(); }
    }
}