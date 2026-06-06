using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Collectibles — Crystal Fragments (resonance shards) + Cave Lore Tablets
    /// Crystalline Caverns collection system with glowing purple crystals
    /// </summary>
    [DefaultExecutionOrder(-85)]
    public class Moon2Collectibles : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] GameObject crystalFragmentPrefab;
        [SerializeField] GameObject caveLoreTabletPrefab;
        
        [Header("Counts")]
        [SerializeField] int totalCrystalFragments = 20;  // More than Moon 1
        [SerializeField] int totalCaveLoreTablets = 6;
        
        [Header("Settings")]
        [SerializeField] float autoCollectRadius = 3f;  // Slightly larger for caves
        [SerializeField] float fragmentRSReward = 3f;   // Higher than Moon 1
        [SerializeField] float tabletRSReward = 8f;
        
        readonly List<Collectible> _fragments = new();
        readonly List<Collectible> _tablets = new();
        int _fragmentsCollected;
        int _tabletsCollected;
        
        void Start()
        {
            SpawnCrystalFragments();
            SpawnCaveLoreTablets();
            LoadCollectionState();
            
            Debug.Log($"[Moon2Collectibles] ✅ Initialized - {totalCrystalFragments} fragments, {totalCaveLoreTablets} tablets");
        }
        
        void OnEnable()
        {
            SaveManager.OnBeforeSave += SaveCollectionState;
            SaveManager.OnAfterLoad += LoadCollectionState;
        }
        
        void OnDisable()
        {
            SaveManager.OnBeforeSave -= SaveCollectionState;
            SaveManager.OnAfterLoad -= LoadCollectionState;
        }
        
        void Update()
        {
            CheckAutoCollection();
        }
        
        void SpawnCrystalFragments()
        {
            if (crystalFragmentPrefab == null)
            {
                Debug.LogWarning("[Moon2Collectibles] No crystal fragment prefab assigned - using procedural");
            }
            
            // Strategic placement in crystal caverns
            Vector3[] positions = GenerateCavePositions(totalCrystalFragments, 80f);
            
            for (int i = 0; i < totalCrystalFragments; i++)
            {
                GameObject fragment = crystalFragmentPrefab != null ? 
                    Instantiate(crystalFragmentPrefab, positions[i], Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform) :
                    CreateProceduralFragment(positions[i]);
                
                fragment.name = $"CrystalFragment_{i}";
                
                Collectible collectible = new Collectible
                {
                    id = $"moon2_fragment_{i}",
                    gameObject = fragment,
                    position = positions[i],
                    rsReward = fragmentRSReward,
                    collectibleType = "CrystalFragment"
                };
                
                _fragments.Add(collectible);
                
                // Visual effects
                AddCrystalGlow(fragment, new Color(0.6f, 0.2f, 0.8f));  // Purple
            }
        }
        
        void SpawnCaveLoreTablets()
        {
            if (caveLoreTabletPrefab == null)
            {
                Debug.LogWarning("[Moon2Collectibles] No cave lore tablet prefab - using procedural");
            }
            
            // Hidden in secret cave alcoves
            Vector3[] positions = new Vector3[]
            {
                new Vector3(15f, 2f, 35f),   // Upper chamber
                new Vector3(-25f, -5f, 20f), // Deep cave
                new Vector3(40f, 1f, -10f),  // Crystal grove
                new Vector3(-15f, 8f, -30f), // High ledge
                new Vector3(0f, -10f, 45f),  // Underwater grotto
                new Vector3(30f, 15f, 15f)   // Summit cave
            };
            
            string[] loreIds = new string[]
            {
                "crystal_formation_theory",
                "cavern_acoustics",
                "dissonance_corruption_origin",
                "ancient_mining_techniques",
                "resonance_crystal_properties",
                "deep_earth_harmonics"
            };
            
            for (int i = 0; i < totalCaveLoreTablets; i++)
            {
                GameObject tablet = caveLoreTabletPrefab != null ?
                    Instantiate(caveLoreTabletPrefab, positions[i], Quaternion.identity, transform) :
                    CreateProceduralTablet(positions[i]);
                
                tablet.name = $"CaveLoreTablet_{i}";
                
                Collectible collectible = new Collectible
                {
                    id = $"moon2_tablet_{i}",
                    gameObject = tablet,
                    position = positions[i],
                    rsReward = tabletRSReward,
                    collectibleType = "CaveLoreTablet",
                    loreId = loreIds[i]
                };
                
                _tablets.Add(collectible);
                
                // Stone tablet glow (dim blue)
                AddCrystalGlow(tablet, new Color(0.3f, 0.5f, 0.8f));
            }
        }
        
        Vector3[] GenerateCavePositions(int count, float radius)
        {
            Vector3[] positions = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                // Cave distribution pattern - clusters near walls
                float angle = (i / (float)count) * 360f + Random.Range(-20f, 20f);
                float distance = Random.Range(radius * 0.4f, radius);
                float height = Random.Range(-5f, 10f);  // Vertical variation for caves
                
                positions[i] = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    height,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );
            }
            
            return positions;
        }
        
        GameObject CreateProceduralFragment(Vector3 position)
        {
            GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fragment.transform.position = position;
            fragment.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            fragment.tag = "Collectible";
            fragment.layer = LayerMask.NameToLayer("Interactable");
            
            Renderer renderer = fragment.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.6f, 0.2f, 0.8f);
            mat.SetColor("_EmissionColor", new Color(0.6f, 0.2f, 0.8f) * 2f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
            
            return fragment;
        }
        
        GameObject CreateProceduralTablet(Vector3 position)
        {
            GameObject tablet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tablet.transform.position = position;
            tablet.transform.localScale = new Vector3(0.5f, 0.7f, 0.1f);
            tablet.tag = "Collectible";
            
            Renderer renderer = tablet.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.4f, 0.5f);
            renderer.material = mat;
            
            return tablet;
        }
        
        void AddCrystalGlow(GameObject obj, Color glowColor)
        {
            Light light = obj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = glowColor;
            light.range = 6f;
            light.intensity = 1.5f;
            light.shadows = LightShadows.None;
            
            // Pulsing animation
            CrystalPulse pulse = obj.AddComponent<CrystalPulse>();
            pulse.baseIntensity = 1.5f;
            pulse.pulseSpeed = 2f;
            pulse.pulseAmount = 0.5f;
        }
        
        void CheckAutoCollection()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            
            // Check fragments
            foreach (Collectible fragment in _fragments)
            {
                if (fragment.collected || fragment.gameObject == null) continue;
                
                if (Vector3.Distance(playerPos, fragment.position) <= autoCollectRadius)
                {
                    CollectFragment(fragment);
                }
            }
            
            // Check tablets
            foreach (Collectible tablet in _tablets)
            {
                if (tablet.collected || tablet.gameObject == null) continue;
                
                if (Vector3.Distance(playerPos, tablet.position) <= autoCollectRadius)
                {
                    CollectTablet(tablet);
                }
            }
        }
        
        void CollectFragment(Collectible fragment)
        {
            fragment.collected = true;
            _fragmentsCollected++;
            
            // Rewards
            GameStateManager.Instance?.AddResonancePoints(fragment.rsReward);
            
            // Progress tracking
            float progress = (_fragmentsCollected / (float)totalCrystalFragments) * 20f;  // 20% for all fragments
            GameStateManager.Instance?.SetMoonProgress("Moon2", Mathf.Min(progress, 20f));
            
            // VFX + SFX
            PlayCollectionEffect(fragment.position, new Color(0.6f, 0.2f, 0.8f));
            
            // Destroy visual
            if (fragment.gameObject != null)
                Destroy(fragment.gameObject);
            
            // Event
            GameEvents.FireCollectibleGathered(new CollectibleEventArgs 
            { 
                collectibleType = "CrystalFragment",
                rsReward = fragment.rsReward,
                position = fragment.position
            });
            
            Debug.Log($"[Moon2Collectibles] Crystal Fragment collected: {_fragmentsCollected}/{totalCrystalFragments} (+{fragment.rsReward} RS)");
            
            // Completion bonus
            if (_fragmentsCollected == totalCrystalFragments)
            {
                GameStateManager.Instance?.AddResonancePoints(15f);
                GameStateManager.Instance?.UnlockAchievement("moon2_fragments_complete");
                Debug.Log("[Moon2Collectibles] 🏆 All Crystal Fragments collected! Bonus: +15 RS");
            }
        }
        
        void CollectTablet(Collectible tablet)
        {
            tablet.collected = true;
            _tabletsCollected++;
            
            // Rewards
            GameStateManager.Instance?.AddResonancePoints(tablet.rsReward);
            
            // Lore unlock
            if (!string.IsNullOrEmpty(tablet.loreId))
            {
                LoreManager.Instance?.UnlockLoreEntry(tablet.loreId);
            }
            
            // VFX
            PlayCollectionEffect(tablet.position, new Color(0.3f, 0.5f, 0.8f));
            
            if (tablet.gameObject != null)
                Destroy(tablet.gameObject);
            
            Debug.Log($"[Moon2Collectibles] Cave Lore Tablet collected: {_tabletsCollected}/{totalCaveLoreTablets} (+{tablet.rsReward} RS, Lore: {tablet.loreId})");
            
            // Completion bonus
            if (_tabletsCollected == totalCaveLoreTablets)
            {
                GameStateManager.Instance?.AddResonancePoints(20f);
                GameStateManager.Instance?.UnlockAchievement("moon2_lore_master");
            }
        }
        
        void PlayCollectionEffect(Vector3 position, Color color)
        {
            GameObject vfx = new GameObject("CollectionVFX");
            vfx.transform.position = position;
            
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.startSize = 0.2f;
            
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
            
            Destroy(vfx, 2f);
        }
        
        void SaveCollectionState()
        {
            SaveData save = SaveManager.Instance?.GetCurrentSaveData();
            if (save == null) return;
            
            MoonSaveData moon2 = save.GetMoonData("Moon2");
            moon2.collectiblesCollected.Clear();
            
            foreach (Collectible fragment in _fragments)
            {
                if (fragment.collected)
                    moon2.collectiblesCollected.Add(fragment.id);
            }
            
            foreach (Collectible tablet in _tablets)
            {
                if (tablet.collected)
                    moon2.collectiblesCollected.Add(tablet.id);
            }
            
            save.SetMoonData("Moon2", moon2);
        }
        
        void LoadCollectionState()
        {
            SaveData save = SaveManager.Instance?.GetCurrentSaveData();
            if (save == null) return;
            
            MoonSaveData moon2 = save.GetMoonData("Moon2");
            
            foreach (Collectible fragment in _fragments)
            {
                if (moon2.collectiblesCollected.Contains(fragment.id))
                {
                    fragment.collected = true;
                    _fragmentsCollected++;
                    if (fragment.gameObject != null)
                        fragment.gameObject.SetActive(false);
                }
            }
            
            foreach (Collectible tablet in _tablets)
            {
                if (moon2.collectiblesCollected.Contains(tablet.id))
                {
                    tablet.collected = true;
                    _tabletsCollected++;
                    if (tablet.gameObject != null)
                        tablet.gameObject.SetActive(false);
                }
            }
        }
        
        class Collectible
        {
            public string id;
            public GameObject gameObject;
            public Vector3 position;
            public float rsReward;
            public string collectibleType;
            public string loreId;
            public bool collected;
        }
    }
    
    public class CrystalPulse : MonoBehaviour
    {
        public float baseIntensity = 1.5f;
        public float pulseSpeed = 2f;
        public float pulseAmount = 0.5f;
        
        Light _light;
        float _time;
        
        void Start()
        {
            _light = GetComponent<Light>();
        }
        
        void Update()
        {
            if (_light == null) return;
            
            _time += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(_time) * pulseAmount;
            _light.intensity = baseIntensity + pulse;
        }
    }
}
