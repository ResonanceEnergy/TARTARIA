using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Collectibles - Aether fragments and jungle-themed artifacts for The Verdant Labyrinth
    /// </summary>
    [DefaultExecutionOrder(-45)]
    public class Moon3Collectibles : MonoBehaviour
    {
        [Header("Moon 3: Jungle Collectibles")]
        [SerializeField] int aetherFragmentCount = 15;
        [SerializeField] int ancientRelicCount = 8;
        [SerializeField] int hiddenCacheCount = 5;

        List<GameObject> collectibles = new List<GameObject>();

        void Start()
        {
            SpawnCollectibles();
        }

        void SpawnCollectibles()
        {
            // Aether Fragments - common collectibles for RS/currency
            for (int i = 0; i < aetherFragmentCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-65f, 65f),
                    Random.Range(0.5f, 3f),
                    Random.Range(-65f, 65f)
                );
                CreateCollectible($"AetherFragment_Moon3_{i}", pos, "AetherFragment", 5f, new Color(0.3f, 0.9f, 0.4f));
            }

            // Ancient Relics - jungle-themed artifacts
            for (int i = 0; i < ancientRelicCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-65f, 65f),
                    Random.Range(0.5f, 2f),
                    Random.Range(-65f, 65f)
                );
                CreateCollectible($"JungleRelic_Moon3_{i}", pos, "JungleRelic", 15f, new Color(0.8f, 0.7f, 0.3f));
            }

            // Hidden Caches - rare hidden treasures
            for (int i = 0; i < hiddenCacheCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-70f, 70f),
                    Random.Range(0.5f, 4f),
                    Random.Range(-70f, 70f)
                );
                CreateCollectible($"HiddenCache_Moon3_{i}", pos, "HiddenCache", 30f, new Color(0.9f, 0.5f, 0.9f));
            }

            Debug.Log($"💎 Moon3Collectibles spawned {collectibles.Count} collectibles");
        }

        GameObject CreateCollectible(string name, Vector3 position, string collectibleType, float rsReward, Color glowColor)
        {
            GameObject collectible = new GameObject(name);
            collectible.transform.position = position;
            collectible.tag = "Interactable";

            // Add trigger collider
            SphereCollider trigger = collectible.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.5f;

            // Add collectible component
            CollectibleItem item = collectible.AddComponent<CollectibleItem>();
            item.itemId = $"moon3_{collectibleType.ToLower()}_{name}";
            item.collectibleType = collectibleType;
            item.rsReward = rsReward;
            item.displayName = collectibleType switch
            {
                "AetherFragment" => "Jungle Aether Fragment",
                "JungleRelic" => "Ancient Jungle Relic",
                "HiddenCache" => "Hidden Treasure Cache",
                _ => "Unknown Item"
            };

            // Visual representation
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

            // Add floating/bobbing animation
            BobAnimation bobber = collectible.AddComponent<BobAnimation>();
            bobber.bobSpeed = 1f;
            bobber.bobHeight = 0.3f;
            bobber.rotationSpeed = 45f;

            collectibles.Add(collectible);
            return collectible;
        }

        void OnDestroy()
        {
            foreach (GameObject collectible in collectibles)
            {
                if (collectible != null) Destroy(collectible);
            }
            collectibles.Clear();
        }
    }

    /// <summary>
    /// Collectible item component - handles player pickup interactions
    /// </summary>
    public class CollectibleItem : MonoBehaviour, IInteractable
    {
        public string itemId;
        public string collectibleType;
        public string displayName;
        public float rsReward;
        bool collected;

        public string GetInteractPrompt()
        {
            return collected ? "" : $"Collect {displayName} (E)";
        }

        public void Interact(GameObject player)
        {
            if (collected) return;

            collected = true;

            // Grant rewards
            if (rsReward > 0f)
            {
                Core.GameLoopController.Instance?.QueueRSReward(rsReward, "collectible");
                Core.GameEvents.FireRSChange(rsReward);
            }

            // Progress quest objectives
            QuestManager.Instance?.ProgressByType(Core.Enums.QuestObjectiveType.CollectItem, itemId);

            // Feedback
            Audio.AudioManager.Instance?.PlaySFX2D("ItemPickup");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();
            Core.GameEvents.RaiseHUDShowInteractionPrompt($"Collected: {displayName} (+{rsReward} RS)");

            Debug.Log($"[Collectible] Collected {itemId}: {displayName} (+{rsReward} RS)");

            // Destroy after brief delay
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Simple bob/rotation animation for collectibles
    /// </summary>
    public class BobAnimation : MonoBehaviour
    {
        public float bobSpeed = 1f;
        public float bobHeight = 0.3f;
        public float rotationSpeed = 45f;

        Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPos + Vector3.up * yOffset;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
