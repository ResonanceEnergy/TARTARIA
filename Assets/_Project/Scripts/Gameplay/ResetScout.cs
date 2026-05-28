using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Reset Scout - Victorian-costumed enemies with clipboards and jackhammers
    /// First tutorial combat encounter in Moon 1
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ResetScout : MonoBehaviour
    {
        [Header("Combat")]
        public float health = 100f;
        public float maxHealth = 100f;
        public float attackDamage = 15f;
        public float attackRange = 2f;
        public float attackCooldown = 2f;
        
        [Header("Movement")]
        public float patrolSpeed = 2f;
        public float chaseSpeed = 4f;
        public float detectionRange = 10f;
        
        [Header("Victorian Props")]
        public GameObject clipboard;
        public GameObject jackhammer;
        public bool hasJackhammer = true;
        
        private Transform player;
        private CharacterController controller;
        private float lastAttackTime;
        private bool isChasing = false;
        
        void Start()
        {
            controller = GetComponent<CharacterController>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            // Equip props
            if (hasJackhammer && jackhammer)
            {
                jackhammer.SetActive(true);
                if (clipboard) clipboard.SetActive(false);
            }
            else if (clipboard)
            {
                clipboard.SetActive(true);
            }
        }
        
        void Update()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Detect player
            if (distanceToPlayer <= detectionRange)
            {
                isChasing = true;
                ChasePlayer(distanceToPlayer);
            }
            else
            {
                isChasing = false;
                Patrol();
            }
        }
        
        void ChasePlayer(float distance)
        {
            if (distance > attackRange)
            {
                // Move toward player
                Vector3 direction = (player.position - transform.position).normalized;
                controller.SimpleMove(direction * chaseSpeed);
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            }
            else
            {
                // Attack if in range
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                    lastAttackTime = Time.time;
                }
            }
        }
        
        void Patrol()
        {
            // Simple patrol logic (can be expanded)
            controller.SimpleMove(transform.forward * patrolSpeed);
            
            // Random direction change
            if (Random.value < 0.01f)
            {
                transform.Rotate(0, Random.Range(-90f, 90f), 0);
            }
        }
        
        void Attack()
        {
            Debug.Log($"[ResetScout] Jackhammer attack! ({attackDamage} damage)");
            // TODO: Apply damage to player
            // TODO: Play jackhammer animation + sound
        }
        
        public void TakeDamage(float damage)
        {
            health -= damage;
            Debug.Log($"[ResetScout] Took {damage} damage. Health: {health}/{maxHealth}");
            
            if (health <= 0)
            {
                Die();
            }
        }
        
        void Die()
        {
            Debug.Log("[ResetScout] Defeated! Dropping Victorian clipboard...");
            // TODO: Drop loot (clipboard collectible)
            Destroy(gameObject, 0.5f);
        }
    }
}