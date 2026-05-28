using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Pure Water Fountain - Ionized mist, repels golems, restores player
    /// </summary>
    public class PureWaterFountain : MonoBehaviour
    {
        [Header("Fountain Settings")]
        public bool isRestored = false;
        public float mistRadius = 10f;
        public float healRate = 5f; // HP per second
        public float golemRepelForce = 20f;
        
        [Header("VFX")]
        public ParticleSystem mistParticles;
        public Light fountainLight;
        public AudioClip fountainAmbience;
        
        private AudioSource audioSource;
        private Collider[] nearbyEnemies = new Collider[20];
        
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = fountainAmbience;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
            
            if (isRestored)
            {
                ActivateFountain();
            }
        }
        
        void Update()
        {
            if (!isRestored) return;
            
            // Repel golems in radius
            int count = Physics.OverlapSphereNonAlloc(transform.position, mistRadius, nearbyEnemies);
            for (int i = 0; i < count; i++)
            {
                if (nearbyEnemies[i].CompareTag("Enemy") || nearbyEnemies[i].GetComponent<ResetScout>())
                {
                    RepelEnemy(nearbyEnemies[i].transform);
                }
            }
        }
        
        public void RestoreFountain()
        {
            isRestored = true;
            ActivateFountain();
            Debug.Log("[Fountain] ✅ Pure water restored! Ionized mist now repels golems.");
        }
        
        void ActivateFountain()
        {
            if (mistParticles)
            {
                mistParticles.Play();
            }
            
            if (fountainLight)
            {
                fountainLight.enabled = true;
                fountainLight.color = new Color(0.7f, 0.9f, 1f);
                fountainLight.intensity = 2f;
            }
            
            if (audioSource && fountainAmbience)
            {
                audioSource.Play();
            }
        }
        
        void RepelEnemy(Transform enemy)
        {
            Vector3 repelDirection = (enemy.position - transform.position).normalized;
            
            // Apply force (if enemy has Rigidbody)
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(repelDirection * golemRepelForce, ForceMode.Force);
            }
            
            // Damage over time
            ResetScout scout = enemy.GetComponent<ResetScout>();
            if (scout)
            {
                scout.TakeDamage(healRate * Time.deltaTime);
            }
        }
        
        void OnTriggerStay(Collider other)
        {
            // Heal player
            if (other.CompareTag("Player"))
            {
                // TODO: Apply healing to player health component
                Debug.Log($"[Fountain] Healing player: +{healRate * Time.deltaTime} HP");
            }
        }
    }
}