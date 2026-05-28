using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Visual Landmarks — Iconic cathedral structures in Echohaven
    /// Places and activates key visual elements: bell towers, stained glass, statues, resonance nodes
    /// Some landmarks evolve as player progresses (restoration visual feedback)
    /// </summary>
    [DefaultExecutionOrder(-77)]
    public class Moon1VisualLandmarks : MonoBehaviour
    {
        [Header("Landmark Prefabs")]
        [SerializeField] GameObject bellTowerPrefab;
        [SerializeField] GameObject stainedGlassWindowPrefab;
        [SerializeField] GameObject ancientStatuePrefab;
        [SerializeField] GameObject resonanceObeliskPrefab;
        [SerializeField] GameObject cathedralDomePrefab;
        
        [Header("Landmark Positions")]
        [SerializeField] Vector3 bellTowerPosition = new Vector3(0f, 25f, 40f);
        [SerializeField] Vector3[] stainedGlassPositions;
        [SerializeField] Vector3[] statuePositions;
        [SerializeField] Vector3[] obeliskPositions;
        [SerializeField] Vector3 domePosition = new Vector3(0f, 30f, 0f);
        
        [Header("Restoration Settings")]
        [SerializeField] bool enableProgressive Restoration = true;
        [SerializeField] float restorationProgressThreshold = 0.5f;  // 50% progress to restore landmarks
        
        readonly List<GameObject> _landmarks = new();
        readonly List<RestoredLandmark> _restorableLandmarks = new();
        bool _landmarksRestored;
        
        void Start()
        {
            SpawnLandmarks();
            
            Debug.Log($"[Moon1VisualLandmarks] ✅ Initialized - {_landmarks.Count} landmarks placed");
        }
        
        void SpawnLandmarks()
        {
            // Bell Tower (central landmark)
            if (bellTowerPrefab != null)
            {
                GameObject bellTower = Instantiate(bellTowerPrefab, bellTowerPosition, Quaternion.identity, transform);
                bellTower.name = "BellTower_Central";
                _landmarks.Add(bellTower);
                
                // Add restoration component
                if (enableProgressiveRestoration)
                {
                    RestoredLandmark restored = bellTower.AddComponent<RestoredLandmark>();
                    restored.landmarkName = "Bell Tower";
                    restored.restoredVersion = bellTower;  // TODO: Assign restored mesh/material variant
                    _restorableLandmarks.Add(restored);
                }
            }
            
            // Stained Glass Windows
            if (stainedGlassWindowPrefab != null && stainedGlassPositions != null)
            {
                for (int i = 0; i < stainedGlassPositions.Length; i++)
                {
                    GameObject window = Instantiate(stainedGlassWindowPrefab, stainedGlassPositions[i], Quaternion.identity, transform);
                    window.name = $"StainedGlass_{i}";
                    _landmarks.Add(window);
                }
            }
            
            // Ancient Statues
            if (ancientStatuePrefab != null && statuePositions != null)
            {
                for (int i = 0; i < statuePositions.Length; i++)
                {
                    GameObject statue = Instantiate(ancientStatuePrefab, statuePositions[i], Quaternion.identity, transform);
                    statue.name = $"AncientStatue_{i}";
                    _landmarks.Add(statue);
                    
                    // Add restoration
                    if (enableProgressiveRestoration)
                    {
                        RestoredLandmark restored = statue.AddComponent<RestoredLandmark>();
                        restored.landmarkName = $"Statue {i + 1}";
                        _restorableLandmarks.Add(restored);
                    }
                }
            }
            
            // Resonance Obelisks
            if (resonanceObeliskPrefab != null && obeliskPositions != null)
            {
                for (int i = 0; i < obeliskPositions.Length; i++)
                {
                    GameObject obelisk = Instantiate(resonanceObeliskPrefab, obeliskPositions[i], Quaternion.identity, transform);
                    obelisk.name = $"ResonanceObelisk_{i}";
                    _landmarks.Add(obelisk);
                    
                    // Add glow effect that intensifies with progress
                    ObeliskGlow glow = obelisk.AddComponent<ObeliskGlow>();
                    glow.glowColor = new Color(0.3f, 0.8f, 1f);  // Cyan
                }
            }
            
            // Cathedral Dome (central structure)
            if (cathedralDomePrefab != null)
            {
                GameObject dome = Instantiate(cathedralDomePrefab, domePosition, Quaternion.identity, transform);
                dome.name = "Cathedral_Dome";
                _landmarks.Add(dome);
            }
        }
        
        void Update()
        {
            if (!enableProgressiveRestoration || _landmarksRestored) return;
            
            // Check if player has made enough progress to restore landmarks
            if (GameStateManager.Instance != null)
            {
                float progress = GameStateManager.Instance.GetMoonProgress(1);
                
                if (progress >= restorationProgressThreshold)
                {
                    RestoreLandmarks();
                }
            }
        }
        
        void RestoreLandmarks()
        {
            _landmarksRestored = true;
            
            foreach (RestoredLandmark landmark in _restorableLandmarks)
            {
                if (landmark != null)
                {
                    landmark.Restore();
                }
            }
            
            Debug.Log("[Moon1VisualLandmarks] ✨ Landmarks restored to former glory!");
            
            // Achievement
            GameEvents.FireAchievementUnlocked("echohaven_landmarks_restored");
        }
        
        void OnDestroy()
        {
            foreach (GameObject landmark in _landmarks)
            {
                if (landmark != null)
                    Destroy(landmark);
            }
        }
    }
    
    /// <summary>
    /// Component for landmarks that can be visually restored as player progresses
    /// </summary>
    public class RestoredLandmark : MonoBehaviour
    {
        public string landmarkName;
        public GameObject restoredVersion;  // Swap to this mesh when restored
        
        bool _restored;
        MeshRenderer _renderer;
        Material _originalMaterial;
        Material _restoredMaterial;
        
        void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }
        
        public void Restore()
        {
            if (_restored) return;
            
            _restored = true;
            
            // Visual restoration effect
            if (_renderer != null)
            {
                // Brighten materials (remove dust/damage)
                Color originalColor = _originalMaterial.color;
                Color restoredColor = originalColor * 1.5f;  // Brighter
                
                LeanTween.value(gameObject, originalColor, restoredColor, 3f)
                    .setOnUpdate((Color col) =>
                    {
                        if (_renderer != null && _renderer.material != null)
                            _renderer.material.color = col;
                    });
                
                // Add emission (resonance glow)
                if (_renderer.material.HasProperty("_EmissionColor"))
                {
                    _renderer.material.EnableKeyword("_EMISSION");
                    LeanTween.value(gameObject, 0f, 1f, 3f)
                        .setOnUpdate((float intensity) =>
                        {
                            if (_renderer != null && _renderer.material != null)
                                _renderer.material.SetColor("_EmissionColor", new Color(0.3f, 0.8f, 1f) * intensity);
                        });
                }
            }
            
            Debug.Log($"[RestoredLandmark] {landmarkName} restored!");
        }
    }
    
    /// <summary>
    /// Obelisk glow effect that intensifies with Moon progress
    /// </summary>
    public class ObeliskGlow : MonoBehaviour
    {
        public Color glowColor = Color.cyan;
        
        Light _light;
        MeshRenderer _renderer;
        float _baseIntensity = 0.5f;
        
        void Start()
        {
            // Add point light
            _light = gameObject.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = glowColor;
            _light.range = 10f;
            _light.intensity = 0f;  // Start dim
            
            _renderer = GetComponent<MeshRenderer>();
        }
        
        void Update()
        {
            // Glow intensity based on Moon progress
            float progress = 0f;
            if (GameStateManager.Instance != null)
            {
                progress = GameStateManager.Instance.GetMoonProgress(1);
            }
            
            float targetIntensity = _baseIntensity + (progress * 2f);  // 0.5 → 2.5 intensity
            
            if (_light != null)
            {
                _light.intensity = Mathf.Lerp(_light.intensity, targetIntensity, Time.deltaTime * 0.5f);
            }
            
            // Pulsing glow
            float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 1f;
            if (_light != null)
            {
                _light.intensity *= pulse;
            }
            
            // Emission on mesh
            if (_renderer != null && _renderer.material.HasProperty("_EmissionColor"))
            {
                _renderer.material.EnableKeyword("_EMISSION");
                _renderer.material.SetColor("_EmissionColor", glowColor * progress * 2f * pulse);
            }
        }
    }
}
