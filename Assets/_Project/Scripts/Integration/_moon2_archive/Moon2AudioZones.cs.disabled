using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration { [DefaultExecutionOrder(-80)] public class Moon2AudioZones : MonoBehaviour {
    [System.Serializable] public class AudioZone { public string zoneName; public Vector3 center; public float radius; public AudioClip ambientClip; public float volume = 0.5f; public bool is3D = true; }
    [SerializeField] AudioZone[] audioZones;
    AudioSource _zoneAudio;
    string _currentZone;
    void Start() { _zoneAudio = gameObject.AddComponent<AudioSource>(); _zoneAudio.loop = true; SetupAudioZones(); Debug.Log($"[Moon2AudioZones] ✅ {audioZones?.Length ?? 0} zones configured"); }
    void SetupAudioZones() { if (audioZones == null || audioZones.Length == 0) { audioZones = new AudioZone[] {
        new AudioZone { zoneName = "MainCavern", center = Vector3.zero, radius = 30f, ambientClip = null, volume = 0.5f, is3D = false },
        new AudioZone { zoneName = "CrystalGrove", center = new Vector3(25f, 0f, 25f), radius = 20f, ambientClip = null, volume = 0.6f, is3D = true },
        new AudioZone { zoneName = "DeepChasm", center = new Vector3(0f, -10f, 40f), radius = 15f, ambientClip = null, volume = 0.4f, is3D = true }
    }; } }
    void Update() { GameObject player = GameObject.FindGameObjectWithTag("Player"); if (player != null) CheckZoneTransition(player.transform.position); }
    void CheckZoneTransition(Vector3 playerPos) { foreach (var zone in audioZones) { if (Vector3.Distance(playerPos, zone.center) <= zone.radius) { if (_currentZone != zone.zoneName) EnterZone(zone); return; } } }
    void EnterZone(AudioZone zone) { _currentZone = zone.zoneName; if (zone.ambientClip != null) { LeanTween.value(gameObject, _zoneAudio.volume, 0f, 1f).setOnUpdate((float v) => _zoneAudio.volume = v).setOnComplete(() => { _zoneAudio.clip = zone.ambientClip; _zoneAudio.volume = zone.volume; _zoneAudio.spatialBlend = zone.is3D ? 1f : 0f; _zoneAudio.Play(); }); } GameEvents.FirePlayerEnteredZone(new ZoneEventArgs { zoneName = zone.zoneName, position = zone.center }); } } }
