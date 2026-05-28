using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Audio Zones — Spatial audio triggers for Echohaven
    /// Defines areas with unique soundscapes (cathedral interior, courtyard, catacombs, etc.)
    /// Triggers ambient audio changes and music transitions based on player location
    /// </summary>
    [DefaultExecutionOrder(-84)]
    public class Moon1AudioZones : MonoBehaviour
    {
        [Header("Audio Zone Configuration")]
        [SerializeField] AudioZone[] audioZones;
        
        [Header("Transition Settings")]
        [SerializeField] float crossfadeDuration = 2f;
        [SerializeField] float zoneCheckInterval = 0.5f;  // Check player zone every 0.5s
        
        readonly List<AudioSource> _zoneAudioSources = new();
        AudioZone _currentZone;
        float _nextZoneCheck;
        GameObject _player;
        
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            SetupAudioZones();
            
            Debug.Log($"[Moon1AudioZones] ✅ Initialized - {audioZones?.Length ?? 0} zones configured");
        }
        
        void SetupAudioZones()
        {
            if (audioZones == null) return;
            
            foreach (AudioZone zone in audioZones)
            {
                // Create audio source for each zone
                GameObject sourceObj = new GameObject($"AudioZone_{zone.zoneName}");
                sourceObj.transform.SetParent(transform);
                sourceObj.transform.position = zone.center;
                
                AudioSource source = sourceObj.AddComponent<AudioSource>();
                source.clip = zone.ambientClip;
                source.loop = true;
                source.spatialBlend = zone.is3D ? 1f : 0f;
                source.volume = 0f;  // Start silent
                source.maxDistance = zone.radius * 1.5f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.playOnAwake = false;
                
                zone.audioSource = source;
                _zoneAudioSources.Add(source);
                
                // Add trigger collider for zone detection
                GameObject triggerObj = new GameObject($"ZoneTrigger_{zone.zoneName}");
                triggerObj.transform.SetParent(sourceObj.transform);
                triggerObj.transform.position = zone.center;
                triggerObj.layer = LayerMask.NameToLayer("Trigger");
                
                SphereCollider trigger = triggerObj.AddComponent<SphereCollider>();
                trigger.radius = zone.radius;
                trigger.isTrigger = true;
                
                AudioZoneTrigger triggerScript = triggerObj.AddComponent<AudioZoneTrigger>();
                triggerScript.zone = zone;
                triggerScript.onEnterZone += EnterZone;
                triggerScript.onExitZone += ExitZone;
            }
        }
        
        void Update()
        {
            // Periodic zone check (fallback if triggers fail)
            if (Time.time >= _nextZoneCheck)
            {
                _nextZoneCheck = Time.time + zoneCheckInterval;
                CheckPlayerZone();
            }
        }
        
        void CheckPlayerZone()
        {
            if (_player == null || audioZones == null) return;
            
            Vector3 playerPos = _player.transform.position;
            
            // Find zone player is in
            foreach (AudioZone zone in audioZones)
            {
                float distance = Vector3.Distance(playerPos, zone.center);
                if (distance <= zone.radius)
                {
                    if (_currentZone != zone)
                    {
                        EnterZone(zone);
                    }
                    return;
                }
            }
            
            // Player not in any zone - exit current
            if (_currentZone != null)
            {
                ExitZone(_currentZone);
            }
        }
        
        void EnterZone(AudioZone zone)
        {
            if (zone == _currentZone) return;
            
            // Exit previous zone
            if (_currentZone != null)
            {
                ExitZone(_currentZone);
            }
            
            _currentZone = zone;
            
            // Fade in zone audio
            if (zone.audioSource != null)
            {
                zone.audioSource.Play();
                
                LeanTween.value(gameObject, 0f, zone.volume, crossfadeDuration)
                    .setOnUpdate((float val) => zone.audioSource.volume = val);
            }
            
            // Fire event for other systems
            GameEvents.FirePlayerEnteredZone(zone.zoneName);
            
            Debug.Log($"[Moon1AudioZones] Entered zone: {zone.zoneName}");
        }
        
        void ExitZone(AudioZone zone)
        {
            if (zone == null) return;
            
            // Fade out zone audio
            if (zone.audioSource != null)
            {
                LeanTween.value(gameObject, zone.audioSource.volume, 0f, crossfadeDuration)
                    .setOnUpdate((float val) => zone.audioSource.volume = val)
                    .setOnComplete(() =>
                    {
                        if (zone.audioSource != null)
                            zone.audioSource.Stop();
                    });
            }
            
            if (_currentZone == zone)
            {
                _currentZone = null;
            }
            
            Debug.Log($"[Moon1AudioZones] Exited zone: {zone.zoneName}");
        }
        
        void OnDestroy()
        {
            foreach (var source in _zoneAudioSources)
            {
                if (source != null)
                    Destroy(source.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Defines an audio zone with spatial properties
    /// </summary>
    [System.Serializable]
    public class AudioZone
    {
        public string zoneName;
        public Vector3 center;
        public float radius = 20f;
        public AudioClip ambientClip;
        [Range(0f, 1f)] public float volume = 0.5f;
        public bool is3D = true;
        [HideInInspector] public AudioSource audioSource;
    }
    
    /// <summary>
    /// Trigger component for audio zone detection
    /// </summary>
    public class AudioZoneTrigger : MonoBehaviour
    {
        public AudioZone zone;
        public System.Action<AudioZone> onEnterZone;
        public System.Action<AudioZone> onExitZone;
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                onEnterZone?.Invoke(zone);
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                onExitZone?.Invoke(zone);
            }
        }
    }
}
