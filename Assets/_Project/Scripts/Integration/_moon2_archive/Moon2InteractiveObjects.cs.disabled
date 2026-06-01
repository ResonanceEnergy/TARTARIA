using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Interactive Objects — 12 Dissonance Crystals puzzle system
    /// Players must harmonize corrupted crystals to restore cavern resonance
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon2InteractiveObjects : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] GameObject dissonanceCrystalPrefab;
        [SerializeField] GameObject resonanceBarrierPrefab;
        
        [Header("Settings")]
        [SerializeField] int totalDissonanceCrystals = 12;
        [SerializeField] float interactionRange = 3f;
        [SerializeField] float harmonizationTime = 2f;  // Hold E for 2 seconds
        
        readonly List<DissonanceCrystal> _crystals = new();
        readonly List<ResonanceBarrier> _barriers = new();
        int _crystalsHarmonized;
        DissonanceCrystal _currentCrystal;
        float _harmonizationProgress;
        
        void Start()
        {
            SpawnDissonanceCrystals();
            SpawnResonanceBarriers();
            
            Debug.Log($"[Moon2InteractiveObjects] ✅ Initialized - {totalDissonanceCrystals} Dissonance Crystals puzzle");
        }
        
        void Update()
        {
            CheckCrystalInteraction();
            UpdateHarmonization();
        }
        
        void SpawnDissonanceCrystals()
        {
            // 12 crystals in strategic cave locations
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0f, 1f, 25f),     // Central chamber
                new Vector3(15f, 1f, 20f),    // East tunnel
                new Vector3(-15f, 1f, 20f),   // West tunnel
                new Vector3(25f, 3f, 10f),    // Upper east
                new Vector3(-25f, 3f, 10f),   // Upper west
                new Vector3(10f, -2f, 30f),   // Deep east
                new Vector3(-10f, -2f, 30f),  // Deep west
                new Vector3(0f, 5f, 15f),     // Crystal dome
                new Vector3(20f, 1f, -10f),   // South east
                new Vector3(-20f, 1f, -10f),  // South west
                new Vector3(0f, -5f, 40f),    // Deep core
                new Vector3(0f, 10f, 0f)      // Summit crystal
            };
            
            for (int i = 0; i < totalDissonanceCrystals; i++)
            {
                GameObject crystalObj = dissonanceCrystalPrefab != null ?
                    Instantiate(dissonanceCrystalPrefab, positions[i], Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform) :
                    CreateProceduralCrystal(positions[i]);
                
                crystalObj.name = $"DissonanceCrystal_{i}";
                
                DissonanceCrystal crystal = new DissonanceCrystal
                {
                    gameObject = crystalObj,
                    position = positions[i],
                    id = $"moon2_crystal_{i}",
                    harmonized = false
                };
                
                _crystals.Add(crystal);
                
                // Visual setup
                SetupCrystalVisuals(crystalObj, false);
            }
        }
        
        void SpawnResonanceBarriers()
        {
            // 3 barriers that unlock as crystals harmonize
            Vector3[] barrierPositions = new Vector3[]
            {
                new Vector3(0f, 2f, 35f),     // Main path (unlock at 4 crystals)
                new Vector3(30f, 2f, 15f),    // East chamber (unlock at 8 crystals)
                new Vector3(-30f, 2f, 15f)    // West chamber (unlock at 12 crystals - all)
            };
            
            int[] unlockThresholds = { 4, 8, 12 };
            
            for (int i = 0; i < barrierPositions.Length; i++)
            {
                GameObject barrierObj = resonanceBarrierPrefab != null ?
                    Instantiate(resonanceBarrierPrefab, barrierPositions[i], Quaternion.identity, transform) :
                    CreateProceduralBarrier(barrierPositions[i]);
                
                barrierObj.name = $"ResonanceBarrier_{i}";
                
                ResonanceBarrier barrier = new ResonanceBarrier
                {
                    gameObject = barrierObj,
                    position = barrierPositions[i],
                    unlockThreshold = unlockThresholds[i],
                    active = true
                };
                
                _barriers.Add(barrier);
                
                // Visual: purple energy wall
                SetupBarrierVisuals(barrierObj, true);
            }
        }
        
        GameObject CreateProceduralCrystal(Vector3 position)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.transform.position = position;
            crystal.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
            crystal.transform.rotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            crystal.tag = "Interactive";
            crystal.layer = LayerMask.NameToLayer("Interactable");
            
            return crystal;
        }
        
        GameObject CreateProceduralBarrier(Vector3 position)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.transform.position = position;
            barrier.transform.localScale = new Vector3(5f, 6f, 0.5f);
            barrier.tag = "Barrier";
            
            return barrier;
        }
        
        void SetupCrystalVisuals(GameObject crystal, bool harmonized)
        {
            // Light component
            Light light = crystal.GetOrAddComponent<Light>();
            light.type = LightType.Point;
            light.color = harmonized ? new Color(0f, 1f, 1f) : new Color(0.8f, 0f, 0.3f);  // Cyan vs red
            light.range = 8f;
            light.intensity = harmonized ? 3f : 1.5f;
            
            // Material
            Renderer renderer = crystal.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                Color baseColor = harmonized ? new Color(0.2f, 0.8f, 0.9f) : new Color(0.7f, 0.1f, 0.3f);
                mat.color = baseColor;
                mat.SetColor("_EmissionColor", baseColor * 2f);
                mat.EnableKeyword("_EMISSION");
            }
            
            // Particle system
            ParticleSystem ps = crystal.GetOrAddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = harmonized ? new Color(0f, 1f, 1f) : new Color(0.8f, 0f, 0.3f);
            main.startLifetime = 1.5f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            
            var emission = ps.emission;
            emission.rateOverTime = 5f;
        }
        
        void SetupBarrierVisuals(GameObject barrier, bool active)
        {
            Renderer renderer = barrier.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                mat.color = active ? new Color(0.6f, 0f, 0.4f, 0.6f) : new Color(0f, 0.8f, 0.8f, 0.2f);
                mat.EnableKeyword("_EMISSION");
            }
            
            // Collider blocks when active
            Collider collider = barrier.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = !active;
            }
            
            barrier.SetActive(active);
        }
        
        void CheckCrystalInteraction()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            _currentCrystal = null;
            
            // Find nearest unharmonized crystal
            float closestDist = interactionRange;
            
            foreach (DissonanceCrystal crystal in _crystals)
            {
                if (crystal.harmonized) continue;
                
                float dist = Vector3.Distance(playerPos, crystal.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    _currentCrystal = crystal;
                }
            }
            
            // Show interaction prompt if near crystal
            if (_currentCrystal != null)
            {
                UIManager.Instance?.ShowInteractionPrompt("Hold [E] to Harmonize Crystal");
            }
            else
            {
                UIManager.Instance?.HideInteractionPrompt();
                _harmonizationProgress = 0f;
            }
        }
        
        void UpdateHarmonization()
        {
            if (_currentCrystal == null) return;
            
            // Check E key hold
            if (Input.GetKey(KeyCode.E))
            {
                _harmonizationProgress += Time.deltaTime;
                
                // Progress bar
                float progress = _harmonizationProgress / harmonizationTime;
                UIManager.Instance?.UpdateInteractionProgress(progress);
                
                // Complete harmonization
                if (_harmonizationProgress >= harmonizationTime)
                {
                    HarmonizeCrystal(_currentCrystal);
                    _harmonizationProgress = 0f;
                }
            }
            else
            {
                // Reset if released early
                _harmonizationProgress = 0f;
                UIManager.Instance?.UpdateInteractionProgress(0f);
            }
        }
        
        void HarmonizeCrystal(DissonanceCrystal crystal)
        {
            crystal.harmonized = true;
            _crystalsHarmonized++;
            
            // Visual update
            SetupCrystalVisuals(crystal.gameObject, true);
            
            // Play harmonization effect
            PlayHarmonizationEffect(crystal.position);
            
            // Progress tracking
            float progress = (_crystalsHarmonized / (float)totalDissonanceCrystals) * 30f;  // 30% total
            GameStateManager.Instance?.SetMoonProgress("Moon2", progress);
            
            // Check barrier unlocks
            CheckBarrierUnlocks();
            
            // Event
            GameEvents.FireTuningNodeActivated(new TuningNodeEventArgs 
            { 
                nodeId = crystal.id,
                position = crystal.position,
                nodesActivated = _crystalsHarmonized,
                totalNodes = totalDissonanceCrystals
            });
            
            Debug.Log($"[Moon2InteractiveObjects] Crystal harmonized: {_crystalsHarmonized}/{totalDissonanceCrystals}");
            
            // Completion bonus
            if (_crystalsHarmonized == totalDissonanceCrystals)
            {
                GameStateManager.Instance?.AddResonancePoints(30f);
                GameStateManager.Instance?.UnlockAchievement("moon2_crystals_harmonized");
                Debug.Log("[Moon2InteractiveObjects] 🏆 All Dissonance Crystals harmonized! Cavern restored!");
            }
        }
        
        void CheckBarrierUnlocks()
        {
            foreach (ResonanceBarrier barrier in _barriers)
            {
                if (!barrier.active) continue;
                
                if (_crystalsHarmonized >= barrier.unlockThreshold)
                {
                    UnlockBarrier(barrier);
                }
            }
        }
        
        void UnlockBarrier(ResonanceBarrier barrier)
        {
            barrier.active = false;
            SetupBarrierVisuals(barrier.gameObject, false);
            
            // Dissolve effect
            PlayBarrierDissolveEffect(barrier.position);
            
            Debug.Log($"[Moon2InteractiveObjects] Barrier unlocked at {barrier.unlockThreshold} crystals!");
        }
        
        void PlayHarmonizationEffect(Vector3 position)
        {
            GameObject vfx = new GameObject("HarmonizationVFX");
            vfx.transform.position = position;
            
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0f, 1f, 1f);
            main.startLifetime = 1f;
            main.startSpeed = 8f;
            main.startSize = 0.3f;
            main.maxParticles = 50;
            
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 2f;
            
            Destroy(vfx, 2f);
        }
        
        void PlayBarrierDissolveEffect(Vector3 position)
        {
            GameObject vfx = new GameObject("BarrierDissolveVFX");
            vfx.transform.position = position;
            
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.6f, 0f, 0.4f);
            main.startLifetime = 2f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.maxParticles = 100;
            
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });
            
            Destroy(vfx, 3f);
        }
        
        class DissonanceCrystal
        {
            public GameObject gameObject;
            public Vector3 position;
            public string id;
            public bool harmonized;
        }
        
        class ResonanceBarrier
        {
            public GameObject gameObject;
            public Vector3 position;
            public int unlockThreshold;
            public bool active;
        }
    }
}
