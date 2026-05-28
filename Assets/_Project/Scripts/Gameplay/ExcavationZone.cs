using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Excavation System - Swipe to reveal buried architecture
    /// Satisfying mud removal reveals precision-cut Tartarian stone
    /// </summary>
    public class ExcavationZone : MonoBehaviour
    {
        [Header("Excavation Settings")]
        public int totalMudLayers = 100;
        public int currentLayersRemoved = 0;
        public float excavationRate = 5f; // Layers per second
        
        [Header("Reveal Settings")]
        public GameObject mudCover;
        public GameObject[] revealStages; // 0%→25%→50%→75%→100%
        public Transform cathedralStructure;
        
        [Header("Loot")]
        public List<GameObject> buriedTreasures = new List<GameObject>();
        public float treasureSpawnChance = 0.1f;
        
        [Header("VFX")]
        public ParticleSystem mudDustVFX;
        public AudioClip excavationSound;
        
        private bool isExcavating = false;
        private AudioSource audioSource;
        
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = excavationSound;
            audioSource.loop = true;
            audioSource.spatialBlend = 0.8f;
            
            // Hide all reveal stages initially
            foreach (var stage in revealStages)
            {
                if (stage) stage.SetActive(false);
            }
            
            if (mudCover) mudCover.SetActive(true);
        }
        
        void Update()
        {
            if (isExcavating)
            {
                Excavate(excavationRate * Time.deltaTime);
            }
        }
        
        public void StartExcavation()
        {
            isExcavating = true;
            
            if (mudDustVFX) mudDustVFX.Play();
            if (audioSource && excavationSound) audioSource.Play();
            
            Debug.Log("[Excavation] Started digging...");
        }
        
        public void StopExcavation()
        {
            isExcavating = false;
            
            if (mudDustVFX) mudDustVFX.Stop();
            if (audioSource) audioSource.Stop();
        }
        
        void Excavate(float amount)
        {
            currentLayersRemoved += Mathf.RoundToInt(amount);
            currentLayersRemoved = Mathf.Clamp(currentLayersRemoved, 0, totalMudLayers);
            
            float progress = (float)currentLayersRemoved / totalMudLayers;
            
            // Update reveal stages
            UpdateRevealStages(progress);
            
            // Random treasure spawns
            if (Random.value < treasureSpawnChance * Time.deltaTime)
            {
                SpawnTreasure();
            }
            
            if (progress >= 1f)
            {
                OnExcavationComplete();
            }
        }
        
        void UpdateRevealStages(float progress)
        {
            // Show different cathedral reveal stages
            for (int i = 0; i < revealStages.Length; i++)
            {
                float threshold = (i + 1) / (float)revealStages.Length;
                if (revealStages[i])
                {
                    revealStages[i].SetActive(progress >= threshold);
                }
            }
            
            // Hide mud cover when fully revealed
            if (mudCover && progress >= 0.75f)
            {
                Color mudColor = mudCover.GetComponent<Renderer>()?.material.color ?? Color.gray;
                mudColor.a = 1f - ((progress - 0.75f) / 0.25f);
                if (mudCover.GetComponent<Renderer>())
                {
                    mudCover.GetComponent<Renderer>().material.color = mudColor;
                }
            }
        }
        
        void SpawnTreasure()
        {
            if (buriedTreasures.Count == 0) return;
            
            GameObject treasure = buriedTreasures[Random.Range(0, buriedTreasures.Count)];
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 5f;
            spawnPos.y = transform.position.y;
            
            Instantiate(treasure, spawnPos, Quaternion.identity);
            Debug.Log($"[Excavation] Found treasure: {treasure.name}!");
        }
        
        void OnExcavationComplete()
        {
            isExcavating = false;
            StopExcavation();
            
            if (mudCover) mudCover.SetActive(false);
            if (cathedralStructure) cathedralStructure.gameObject.SetActive(true);
            
            Debug.Log("[Excavation] ✅ COMPLETE! Cathedral fully revealed!");
            // TODO: Trigger Milo comment
            // TODO: Unlock next quest objective
        }
        
        public float GetProgress()
        {
            return (float)currentLayersRemoved / totalMudLayers;
        }
    }
}