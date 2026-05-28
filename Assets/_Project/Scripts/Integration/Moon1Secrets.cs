using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Secrets — Hidden lore areas and bonus content in Echohaven
    /// 5 secret areas with unique rewards, lore discoveries, and achievement tracking
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1Secrets : MonoBehaviour
    {
        [Header("Secret Areas")]
        [SerializeField] SecretArea[] secretAreas;
        
        [Header("Discovery Settings")]
        [SerializeField] float discoveryRadius = 3f;
        [SerializeField] GameObject secretMarkerPrefab;
        
        readonly Dictionary<string, SecretArea> _secrets = new();
        readonly HashSet<string> _discoveredSecrets = new();
        GameObject _player;
        
        public int SecretsDiscovered => _discoveredSecrets.Count;
        public int TotalSecrets => secretAreas?.Length ?? 0;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            InitializeSecrets();
            LoadDiscoveryState();
            
            Debug.Log($"[Moon1Secrets] ✅ Initialized - {TotalSecrets} secrets hidden in Echohaven");
        }
        
        void InitializeSecrets()
        {
            secretAreas = new SecretArea[]
            {
                new SecretArea
                {
                    secretID = "hidden_catacombs",
                    secretName = "Hidden Catacombs",
                    description = "Ancient burial chambers beneath the cathedral. Contains the Architect's Journal.",
                    location = new Vector3(-25f, -5f, -15f),
                    rsReward = 25f,
                    loreUnlock = "architect_journal_1"
                },
                
                new SecretArea
                {
                    secretID = "bell_tower_top",
                    secretName = "Bell Tower Summit",
                    description = "The highest point in Echohaven. A perfect view of the dissonance fog below.",
                    location = new Vector3(0f, 45f, 40f),
                    rsReward = 20f,
                    loreUnlock = "resonance_theory_1"
                },
                
                new SecretArea
                {
                    secretID = "secret_garden",
                    secretName = "Secret Garden",
                    description = "A hidden courtyard where resonant flowers still bloom despite the mud.",
                    location = new Vector3(30f, 1f, -20f),
                    rsReward = 15f,
                    loreUnlock = "harmonic_flora"
                },
                
                new SecretArea
                {
                    secretID = "forgotten_library",
                    secretName = "Forgotten Library",
                    description = "A sealed wing containing pre-Flood manuscripts and tuning diagrams.",
                    location = new Vector3(-18f, 8f, 25f),
                    rsReward = 30f,
                    loreUnlock = "tuning_diagrams"
                },
                
                new SecretArea
                {
                    secretID = "resonance_vault",
                    secretName = "Resonance Vault",
                    description = "The original storage for Aether Shards. Still hums with latent energy.",
                    location = new Vector3(12f, -3f, -30f),
                    rsReward = 35f,
                    loreUnlock = "aether_origins"
                }
            };
            
            foreach (SecretArea secret in secretAreas)
            {
                _secrets[secret.secretID] = secret;
                
                // Place discovery marker (hidden until player gets close)
                if (secretMarkerPrefab != null)
                {
                    GameObject marker = Instantiate(secretMarkerPrefab, secret.location, Quaternion.identity, transform);
                    marker.name = $"SecretMarker_{secret.secretID}";
                    marker.SetActive(false);  // Hidden initially
                    secret.markerObject = marker;
                }
            }
        }
        
        void Update()
        {
            if (_player == null) return;
            
            CheckSecretDiscovery();
        }
        
        void CheckSecretDiscovery()
        {
            Vector3 playerPos = _player.transform.position;
            
            foreach (SecretArea secret in secretAreas)
            {
                if (_discoveredSecrets.Contains(secret.secretID)) continue;
                
                float distance = Vector3.Distance(playerPos, secret.location);
                
                // Discovery range
                if (distance <= discoveryRadius)
                {
                    DiscoverSecret(secret.secretID);
                }
                // Show hint marker when getting close
                else if (distance <= discoveryRadius * 3f && secret.markerObject != null)
                {
                    if (!secret.markerObject.activeSelf)
                    {
                        secret.markerObject.SetActive(true);
                        
                        // Pulse animation
                        LeanTween.scale(secret.markerObject, Vector3.one * 1.2f, 0.8f)
                            .setEaseInOutSine()
                            .setLoopPingPong();
                    }
                }
            }
        }
        
        void DiscoverSecret(string secretID)
        {
            if (_discoveredSecrets.Contains(secretID)) return;
            
            SecretArea secret = _secrets[secretID];
            _discoveredSecrets.Add(secretID);
            secret.isDiscovered = true;
            
            // Grant rewards
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.AddResonancePoints(secret.rsReward);
            }
            
            // Unlock lore entry
            if (LoreManager.Instance != null && !string.IsNullOrEmpty(secret.loreUnlock))
            {
                LoreManager.Instance.UnlockLoreEntry(secret.loreUnlock);
            }
            
            // Show discovery notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSecretDiscovered(secret.secretName, secret.description, secret.rsReward);
            }
            
            // VFX
            if (secret.markerObject != null)
            {
                // Discovery burst effect
                ParticleSystem particles = secret.markerObject.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Play();
                }
                
                // Fade out marker
                LeanTween.alpha(secret.markerObject, 0f, 2f).setOnComplete(() =>
                {
                    if (secret.markerObject != null)
                        secret.markerObject.SetActive(false);
                });
            }
            
            Debug.Log($"[Moon1Secrets] ✨ SECRET DISCOVERED: {secret.secretName} (+{secret.rsReward} RS)");
            
            // Check all secrets achievement
            if (_discoveredSecrets.Count >= secretAreas.Length)
            {
                GameEvents.FireAchievementUnlocked("echohaven_all_secrets");
                Debug.Log("[Moon1Secrets] 🏆 ALL SECRETS DISCOVERED!");
            }
        }
        
        void LoadDiscoveryState()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;
            
            SaveData save = SaveManager.Instance.CurrentSave;
            string discoveredIDs = save.GetMoonData(1, "discoveredSecrets", "");
            
            if (!string.IsNullOrEmpty(discoveredIDs))
            {
                foreach (string id in discoveredIDs.Split(','))
                {
                    if (_secrets.ContainsKey(id))
                    {
                        _discoveredSecrets.Add(id);
                        _secrets[id].isDiscovered = true;
                        
                        // Hide marker for already-discovered secrets
                        if (_secrets[id].markerObject != null)
                        {
                            _secrets[id].markerObject.SetActive(false);
                        }
                    }
                }
            }
        }
        
        void OnDestroy()
        {
            // Save discovery state
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                string discoveredIDs = string.Join(",", _discoveredSecrets);
                SaveManager.Instance.CurrentSave.SetMoonData(1, "discoveredSecrets", discoveredIDs);
            }
        }
        
        public bool IsSecretDiscovered(string secretID)
        {
            return _discoveredSecrets.Contains(secretID);
        }
    }
    
    [System.Serializable]
    public class SecretArea
    {
        public string secretID;
        public string secretName;
        public string description;
        public Vector3 location;
        public float rsReward;
        public string loreUnlock;
        public bool isDiscovered;
        [HideInInspector] public GameObject markerObject;
    }
}
