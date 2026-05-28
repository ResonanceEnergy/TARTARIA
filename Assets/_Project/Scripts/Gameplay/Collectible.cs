using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Collectible Item - Base class for all secrets/treasures
    /// Handles pickup, VFX, UI notifications, save persistence
    /// </summary>
    public enum CollectibleType
    {
        GiantSkeletonKey,    // 8 total (unlock post-game secret)
        SpireFragment,       // Crossover to Moon 5
        AirshipComponent,    // Crossover to Moon 8
        ProphecyFragment,    // Lore reveals about Dissonant One
        ResonanceCrystal,    // Currency for Milo'"'"'s shop
        AetherBattery,       // Energy storage
        VictorianClipboard   // Loot from Reset Scouts
    }
    
    public class Collectible : MonoBehaviour
    {
        [Header("Collectible Data")]
        public CollectibleType type;
        public string itemName;
        [TextArea(2, 4)] public string description;
        public int quantity = 1;
        
        [Header("VFX")]
        public ParticleSystem glowVFX;
        public Color glowColor = Color.cyan;
        public float rotationSpeed = 50f;
        public float bobHeight = 0.5f;
        public float bobSpeed = 2f;
        
        [Header("Audio")]
        public AudioClip pickupSound;
        
        private Vector3 startPos;
        private bool isCollected = false;
        
        void Start()
        {
            startPos = transform.position;
            
            // Apply glow color
            if (glowVFX)
            {
                var main = glowVFX.main;
                main.startColor = glowColor;
            }
        }
        
        void Update()
        {
            if (isCollected) return;
            
            // Rotate + bob animation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPos + new Vector3(0, bob, 0);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            if (!other.CompareTag("Player")) return;
            
            Collect(other.gameObject);
        }
        
        void Collect(GameObject player)
        {
            isCollected = true;
            
            Debug.Log($"[Collectible] ✅ Collected: {itemName} ({type})");
            
            // Add to player inventory
            // TODO: Connect to inventory system
            
            // Play VFX + audio
            if (pickupSound)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Show UI notification
            ShowNotification();
            
            // Save to persistent data
            SaveCollectionState();
            
            // Destroy object
            Destroy(gameObject, 0.5f);
        }
        
        void ShowNotification()
        {
            string message = type switch
            {
                CollectibleType.GiantSkeletonKey => $"Giant Skeleton Key {GetKeyNumber()}/8 Found!",
                CollectibleType.SpireFragment => "Spire Fragment - Moon 5 crossover unlocked!",
                CollectibleType.AirshipComponent => "Airship Component - Moon 8 crossover unlocked!",
                CollectibleType.ProphecyFragment => "Prophecy Fragment - New lore revealed!",
                _ => $"{itemName} collected!"
            };
            
            Debug.Log($"[UI] {message}");
            // TODO: Show UI toast notification
        }
        
        void SaveCollectionState()
        {
            string saveKey = $"Collectible_{type}_{transform.position}";
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }
        
        int GetKeyNumber()
        {
            // Count how many keys player has
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                string key = $"Collectible_GiantSkeletonKey_{i}";
                if (PlayerPrefs.GetInt(key, 0) == 1) count++;
            }
            return count;
        }
    }
}