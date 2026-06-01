using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Environment Decorator — Visual polish and atmosphere details
    /// Props, vegetation, architectural details, ambient objects that bring Echohaven to life
    /// </summary>
    [DefaultExecutionOrder(-86)]
    public class Moon1EnvironmentDecorator : MonoBehaviour
    {
        [Header("Decoration Prefabs")]
        [SerializeField] GameObject[] propPrefabs;          // Furniture, debris, etc.
        [SerializeField] GameObject[] vegetationPrefabs;    // Vines, moss, dead plants
        [SerializeField] GameObject[] architecturalPrefabs; // Columns, arches, ornaments
        [SerializeField] GameObject[] candlePrefabs;        // Light sources
        
        [Header("Density Settings")]
        [SerializeField] int propsCount = 40;
        [SerializeField] int vegetationCount = 30;
        [SerializeField] int architecturalCount = 20;
        [SerializeField] int candlesCount = 25;
        
        [Header("Placement")]
        [SerializeField] float spawnRadius = 35f;
        [SerializeField] bool useRandomRotation = true;
        [SerializeField] bool snapToGround = true;
        
        readonly List<GameObject> _decorations = new();
        
        void Start()
        {
            DecorateEnvironment();
            
            Debug.Log($"[Moon1EnvironmentDecorator] ✅ Initialized - {_decorations.Count} decorations placed");
        }
        
        void DecorateEnvironment()
        {
            // Place props (furniture, barrels, crates, debris)
            PlaceDecorations(propPrefabs, propsCount, "Prop");
            
            // Place vegetation (vines, moss, dead plants)
            PlaceDecorations(vegetationPrefabs, vegetationCount, "Vegetation");
            
            // Place architectural details (columns, arches, broken statues)
            PlaceDecorations(architecturalPrefabs, architecturalCount, "Architectural");
            
            // Place candles and light sources
            PlaceCandles();
        }
        
        void PlaceDecorations(GameObject[] prefabs, int count, string category)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab == null) continue;
                
                Vector3 position = GetRandomPlacementPosition();
                Quaternion rotation = useRandomRotation ? 
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : 
                    Quaternion.identity;
                
                GameObject decoration = Instantiate(prefab, position, rotation, transform);
                decoration.name = $"{category}_{i}";
                
                // Add slight scale variation
                float scaleVariation = Random.Range(0.9f, 1.1f);
                decoration.transform.localScale *= scaleVariation;
                
                _decorations.Add(decoration);
            }
        }
        
        void PlaceCandles()
        {
            if (candlePrefabs == null || candlePrefabs.Length == 0) return;
            
            // Strategic candle placement (along walls, near entrances)
            Vector3[] candleLocations = new Vector3[]
            {
                // Cathedral interior
                new Vector3(5f, 1.5f, 10f),
                new Vector3(-5f, 1.5f, 10f),
                new Vector3(10f, 1.5f, 5f),
                new Vector3(-10f, 1.5f, 5f),
                new Vector3(0f, 1.5f, 15f),
                new Vector3(0f, 1.5f, -15f),
                
                // Corridors
                new Vector3(15f, 1.5f, 0f),
                new Vector3(-15f, 1.5f, 0f),
                new Vector3(0f, 1.5f, 20f),
                new Vector3(0f, 1.5f, -20f),
                
                // Courtyard
                new Vector3(20f, 1.5f, 15f),
                new Vector3(-20f, 1.5f, 15f),
                new Vector3(20f, 1.5f, -15f),
                new Vector3(-20f, 1.5f, -15f),
                
                // Altar area
                new Vector3(2f, 1.5f, 25f),
                new Vector3(-2f, 1.5f, 25f),
                new Vector3(4f, 1.5f, 28f),
                new Vector3(-4f, 1.5f, 28f),
                
                // Side chambers
                new Vector3(25f, 1.5f, 8f),
                new Vector3(-25f, 1.5f, 8f),
                new Vector3(25f, 1.5f, -8f),
                new Vector3(-25f, 1.5f, -8f),
                
                // Upper gallery
                new Vector3(8f, 5f, 20f),
                new Vector3(-8f, 5f, 20f),
                new Vector3(12f, 5f, 10f),
            };
            
            for (int i = 0; i < Mathf.Min(candlesCount, candleLocations.Length); i++)
            {
                GameObject candlePrefab = candlePrefabs[Random.Range(0, candlePrefabs.Length)];
                if (candlePrefab == null) continue;
                
                GameObject candle = Instantiate(candlePrefab, candleLocations[i], Quaternion.identity, transform);
                candle.name = $"Candle_{i}";
                
                // Add flickering light
                Light candleLight = candle.GetOrAddComponent<Light>();
                candleLight.type = LightType.Point;
                candleLight.color = new Color(1f, 0.7f, 0.4f);  // Warm orange
                candleLight.range = 6f;
                candleLight.intensity = Random.Range(0.6f, 0.9f);
                
                // Flickering animation
                FlickeringLight flicker = candle.AddComponent<FlickeringLight>();
                flicker.baseIntensity = candleLight.intensity;
                flicker.flickerSpeed = Random.Range(3f, 8f);
                flicker.flickerAmount = 0.2f;
                
                _decorations.Add(candle);
            }
        }
        
        Vector3 GetRandomPlacementPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 position = new Vector3(randomCircle.x, 10f, randomCircle.y);
            
            if (snapToGround)
            {
                // Raycast down to find ground
                if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 20f))
                {
                    return hit.point;
                }
            }
            
            return new Vector3(position.x, 0f, position.z);
        }
        
        void OnDestroy()
        {
            foreach (GameObject decoration in _decorations)
            {
                if (decoration != null)
                    Destroy(decoration);
            }
        }
    }
    
    /// <summary>
    /// Flickering light component for candles
    /// </summary>
    public class FlickeringLight : MonoBehaviour
    {
        public float baseIntensity = 0.8f;
        public float flickerSpeed = 5f;
        public float flickerAmount = 0.2f;
        
        Light _light;
        float _time;
        
        void Start()
        {
            _light = GetComponent<Light>();
            if (_light == null)
            {
                _light = gameObject.AddComponent<Light>();
            }
        }
        
        void Update()
        {
            if (_light == null) return;
            
            _time += Time.deltaTime * flickerSpeed;
            
            // Perlin noise for organic flicker
            float flicker = Mathf.PerlinNoise(_time, 0f) * flickerAmount;
            _light.intensity = baseIntensity + flicker;
        }
    }
}
