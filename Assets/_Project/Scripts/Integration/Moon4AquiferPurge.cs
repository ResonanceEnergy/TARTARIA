using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 aquifer purge minigame — 6 fountains, layered corruption removal.
    /// Each fountain has 3 corruption layers that must be purged in sequence.
    /// Requires channeling resonant frequency (tuning minigame per layer).
    /// 
    /// Cross-ref: docs/13_MINI_GAMES.md §Moon 4 Aquifer Purge
    /// </summary>
    public class AquiferPurgeMinigame : MonoBehaviour
    {
        public static AquiferPurgeMinigame Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] int totalFountains = 6;
        [SerializeField] int corruptionLayers = 3;
        [SerializeField] Vector3 aquiferCenter = new Vector3(100f, -10f, 80f);
        [SerializeField] float fountainRadius = 15f;

        List<AquiferFountain> _fountains = new List<AquiferFountain>();
        int _fountainsPurged = 0;

        public int FountainsPurged => _fountainsPurged;
        public float Progress => _fountainsPurged / (float)totalFountains;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeMinigame()
        {
            Debug.Log("[AquiferPurge] Initializing aquifer purge minigame...");
            
            SpawnFountains();
        }

        void SpawnFountains()
        {
            for (int i = 0; i < totalFountains; i++)
            {
                float angle = (i / (float)totalFountains) * Mathf.PI * 2f;
                Vector3 pos = aquiferCenter + new Vector3(
                    Mathf.Cos(angle) * fountainRadius,
                    0f,
                    Mathf.Sin(angle) * fountainRadius
                );

                GameObject fountainObj = new GameObject($"AquiferFountain_{i}");
                fountainObj.transform.position = pos;

                AquiferFountain fountain = fountainObj.AddComponent<AquiferFountain>();
                fountain.fountainIndex = i;
                fountain.corruptionLayers = corruptionLayers;
                fountain.OnPurged += OnFountainPurged;

                _fountains.Add(fountain);

                // Visual: corrupted fountain (dark water)
                CreateFountainVisual(fountainObj, false);
            }

            Debug.Log($"[AquiferPurge] Spawned {totalFountains} corrupted fountains.");
        }

        void CreateFountainVisual(GameObject fountainObj, bool purified)
        {
            // Base: stone fountain
            GameObject basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.transform.SetParent(fountainObj.transform);
            basin.transform.localPosition = Vector3.zero;
            basin.transform.localScale = new Vector3(2f, 0.5f, 2f);
            
            Renderer basinRend = basin.GetComponent<Renderer>();
            basinRend.material.color = new Color(0.4f, 0.4f, 0.4f); // Stone gray

            // Water: sphere (corrupted = dark, purified = clear blue)
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            water.transform.SetParent(fountainObj.transform);
            water.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            water.transform.localScale = new Vector3(1.5f, 0.3f, 1.5f);

            Renderer waterRend = water.GetComponent<Renderer>();
            waterRend.material.color = purified 
                ? new Color(0.3f, 0.6f, 1f, 0.6f)  // Clear blue
                : new Color(0.2f, 0.15f, 0.1f, 0.8f); // Corrupted dark

            // Light (off when corrupted)
            if (purified)
            {
                Light light = fountainObj.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.5f, 0.8f, 1f);
                light.range = 8f;
                light.intensity = 2f;
            }
        }

        void OnFountainPurged(AquiferFountain fountain)
        {
            _fountainsPurged++;

            Debug.Log($"[AquiferPurge] Fountain {fountain.fountainIndex} purged! ({_fountainsPurged}/{totalFountains})");

            Audio.AudioManager.Instance?.PlaySFX2D("FountainPurge");
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();

            // Update visual to purified state
            foreach (Transform child in fountain.transform)
            {
                Destroy(child.gameObject);
            }
            CreateFountainVisual(fountain.gameObject, true);

            // Quest progress
            QuestManager.Instance?.ProgressByType(QuestObjectiveType.FindLocation, $"aquifer_fountain_{fountain.fountainIndex}");

            // Check completion
            if (_fountainsPurged >= totalFountains)
            {
                OnAllFountainsPurged();
            }
        }

        void OnAllFountainsPurged()
        {
            Debug.Log("[AquiferPurge] ALL FOUNTAINS PURGED! Aquifer restored, conductive water flows!");

            HUDController.Instance?.ShowObjective("Aquifer Restored! Pure water flows to star fort moats.");

            // Climax VFX: golden water burst from center
            GameObject vfx = new GameObject("AquiferPurgeComplete_VFX");
            vfx.transform.position = aquiferCenter;

            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 3f;
            main.startSpeed = 5f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.5f, 0.8f, 1f);
            main.loop = false;
            main.maxParticles = 500;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 500) });

            Destroy(vfx, 4f);

            // Quest completion
            QuestManager.Instance?.CompleteQuest("moon4_aquifer_purge");
        }
    }

    /// <summary>
    /// Individual aquifer fountain with layered corruption.
    /// Player must purge 3 layers via resonant frequency tuning.
    /// </summary>
    public class AquiferFountain : MonoBehaviour, IInteractable
    {
        public int fountainIndex;
        public int corruptionLayers = 3;
        public event System.Action<AquiferFountain> OnPurged;

        int _layersPurged = 0;
        bool _isPurging = false;

        readonly float[] _layerFrequencies = { 174f, 285f, 396f }; // Solfeggio healing frequencies

        public string GetInteractPrompt()
        {
            if (_layersPurged >= corruptionLayers)
                return "Fountain Purified ✓";
            
            return _isPurging 
                ? "Purging layer..." 
                : $"[E] Purge Corruption (Layer {_layersPurged + 1}/{corruptionLayers})";
        }

        public void Interact(GameObject player)
        {
            if (_layersPurged >= corruptionLayers || _isPurging) return;

            StartCoroutine(PurgeLayer());
        }

        System.Collections.IEnumerator PurgeLayer()
        {
            _isPurging = true;

            float targetFreq = _layerFrequencies[_layersPurged];
            Debug.Log($"[AquiferFountain {fountainIndex}] Purging layer {_layersPurged + 1} (tuning to {targetFreq} Hz)...");

            // Simplified tuning (instant success for beta)
            // Full version: frequency matching minigame
            yield return new WaitForSeconds(2f);

            _layersPurged++;
            _isPurging = false;

            Debug.Log($"[AquiferFountain {fountainIndex}] Layer {_layersPurged} purged!");

            // Visual feedback: corruption layer dissolves
            Audio.AudioManager.Instance?.PlaySFX2D("CorruptionPurge");

            // Check if fully purged
            if (_layersPurged >= corruptionLayers)
            {
                Debug.Log($"[AquiferFountain {fountainIndex}] Fountain fully purified!");
                OnPurged?.Invoke(this);
            }
        }
    }
}
