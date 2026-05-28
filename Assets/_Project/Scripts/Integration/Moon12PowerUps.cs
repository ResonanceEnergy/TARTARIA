using UnityEngine;
using System.Collections.Generic;
using Tartaria.Input;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-36)]
    public class Moon12PowerUps : MonoBehaviour
    {
        [Header("Moon 12: Shadow Power-Ups")]
        [SerializeField] int speedBoostCount = 3;
        [SerializeField] int damageBoostCount = 2;
        [SerializeField] int shieldCount = 2;
        [SerializeField] int visionCount = 2;
        [SerializeField] int aetherSurgeCount = 1;
        List<GameObject> powerups = new List<GameObject>();
        void Start() { SpawnPowerUps(); }
        void SpawnPowerUps()
        {
            for (int i = 0; i < speedBoostCount; i++)
                CreatePowerUp($"SpeedBoost_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 2f), Random.Range(-65f, 65f)), "SpeedBoost", 15f, new Color(0.3f, 0.6f, 1f));
            for (int i = 0; i < damageBoostCount; i++)
                CreatePowerUp($"DamageBoost_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 2f), Random.Range(-65f, 65f)), "DamageBoost", 10f, new Color(1f, 0.2f, 0.2f));
            for (int i = 0; i < shieldCount; i++)
                CreatePowerUp($"Shield_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 2f), Random.Range(-65f, 65f)), "Shield", 20f, new Color(0.3f, 1f, 0.3f));
            for (int i = 0; i < visionCount; i++)
                CreatePowerUp($"Vision_{i}", new Vector3(Random.Range(-65f, 65f), Random.Range(0.5f, 2f), Random.Range(-65f, 65f)), "VisionBoost", 30f, new Color(1f, 1f, 0.3f));
            CreatePowerUp("AetherSurge", new Vector3(Random.Range(-70f, 70f), Random.Range(1f, 3f), Random.Range(-70f, 70f)), "AetherSurge", 0f, new Color(0.8f, 0.3f, 1f));
            Debug.Log($"⚡ Moon12PowerUps: {powerups.Count} power-ups spawned");
        }
        GameObject CreatePowerUp(string name, Vector3 pos, string powerUpType, float duration, Color glowColor) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; obj.transform.localScale = Vector3.one * 0.5f; Destroy(obj.GetComponent<Collider>()); SphereCollider trigger = obj.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 1.5f; var pickup = obj.AddComponent<Moon12PowerUpPickup>(); pickup.powerUpType = powerUpType; pickup.duration = duration; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = glowColor; mat.SetColor("_EmissionColor", glowColor * 2f); mat.EnableKeyword("_EMISSION"); rend.material = mat; } BobAnimation bobber = obj.AddComponent<BobAnimation>(); bobber.bobSpeed = 1.5f; bobber.bobHeight = 0.4f; bobber.rotationSpeed = 60f; powerups.Add(obj); return obj; }
        void OnDestroy() { foreach (var pu in powerups) if (pu != null) Destroy(pu); powerups.Clear(); }
    }
    public class Moon12PowerUpPickup : MonoBehaviour, IInteractable
    {
        public string powerUpType;
        public float duration;
        bool collected;
        public string GetInteractPrompt() { return collected ? "" : $"Collect {powerUpType} (E)"; }
        public void Interact(GameObject player) { if (collected) return; collected = true; Integration.GameLoopController.Instance?.QueueRSReward(2f, "powerup"); Core.GameEvents.RaiseHUDShowObjective($"Power-Up: {powerUpType}"); Audio.AudioManager.Instance?.PlaySFX2D("PowerUpCollect"); Destroy(gameObject, 0.5f); }
    }
}
