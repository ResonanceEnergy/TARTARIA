using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 NPC Spawner — Places NPCs around Echohaven with patrol routes
    /// NPCs: Milo (Mapmaker), excavation workers, scholars, merchants
    /// Each has idle/patrol behavior and dialogue triggers
    /// </summary>
    [DefaultExecutionOrder(-82)] // After paths (-83)
    public class Moon1NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] GameObject npcPrefab; // Generic NPC with simple model
        [SerializeField] int excavationWorkers = 5;
        [SerializeField] int scholars = 3;
        [SerializeField] int merchants = 2;

        [Header("Spawn Locations")]
        [SerializeField] Vector3 miloSpawnPoint = new Vector3(-40f, 0f, 20f); // Near fountain
        [SerializeField] Vector3 cathedralEntrance = new Vector3(0f, 0f, 72f);
        [SerializeField] Vector3 marketArea = new Vector3(30f, 0f, 30f);

        [Header("Patrol Routes")]
        [SerializeField] float patrolRadius = 15f;
        [SerializeField] int waypointsPerRoute = 4;

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log("[Moon1NPCSpawner] Spawning NPCs...");

            var npcParent = new GameObject("NPCs");
            npcParent.transform.position = Vector3.zero;

            // Spawn Milo the Mapmaker (quest giver)
            SpawnMilo(npcParent);

            // Spawn excavation workers near buildings
            for (int i = 0; i < excavationWorkers; i++)
            {
                Vector3 position = GetRandomPositionNear(Vector3.zero, 60f);
                SpawnNPC(npcParent, $"Excavation_Worker_{i + 1}", position, NPCRole.Worker);
            }

            // Spawn scholars near cathedral
            for (int i = 0; i < scholars; i++)
            {
                Vector3 position = GetRandomPositionNear(cathedralEntrance, 20f);
                SpawnNPC(npcParent, $"Scholar_{i + 1}", position, NPCRole.Scholar);
            }

            // Spawn merchants in market area
            for (int i = 0; i < merchants; i++)
            {
                Vector3 position = GetRandomPositionNear(marketArea, 10f);
                SpawnNPC(npcParent, $"Merchant_{i + 1}", position, NPCRole.Merchant);
            }

            Debug.Log($"[Moon1NPCSpawner] ✅ Spawned {1 + excavationWorkers + scholars + merchants} NPCs");
        }

        void SpawnMilo(GameObject parent)
        {
            var milo = new GameObject("Milo_Mapmaker");
            milo.transform.SetParent(parent.transform);
            milo.transform.position = miloSpawnPoint;

            // Visual representation (simple capsule for now)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(milo.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.3f, 0.5f, 0.8f); // Blue scholar robes
            visual.GetComponent<Renderer>().material = material;

            // Add NPC controller
            var controller = milo.AddComponent<SimpleNPCController>();
            controller.npcName = "Milo";
            controller.role = NPCRole.QuestGiver;
            controller.isStationary = true; // Stays at mapmaking table

            // Add interaction sphere
            var trigger = milo.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;
            trigger.center = new Vector3(0f, 1f, 0f);

            Debug.Log($"  ✓ Milo spawned at {miloSpawnPoint}");
        }

        void SpawnNPC(GameObject parent, string npcName, Vector3 position, NPCRole role)
        {
            var npc = new GameObject(npcName);
            npc.transform.SetParent(parent.transform);
            npc.transform.position = position;

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(npc.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(0.6f, 1f, 0.6f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = GetRoleColor(role);
            visual.GetComponent<Renderer>().material = material;

            // Add controller
            var controller = npc.AddComponent<SimpleNPCController>();
            controller.npcName = npcName.Replace("_", " ");
            controller.role = role;
            controller.isStationary = false;

            // Generate patrol route
            controller.patrolWaypoints = GeneratePatrolRoute(position, patrolRadius, waypointsPerRoute);

            // Add interaction trigger
            var trigger = npc.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2f;
            trigger.center = new Vector3(0f, 1f, 0f);
        }

        Vector3 GetRandomPositionNear(Vector3 center, float radius)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(0f, radius);
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(distance, 0f, 0f);
            return center + offset;
        }

        Vector3[] GeneratePatrolRoute(Vector3 center, float radius, int waypointCount)
        {
            var waypoints = new List<Vector3>();
            float angleStep = 360f / waypointCount;

            for (int i = 0; i < waypointCount; i++)
            {
                float angle = i * angleStep + Random.Range(-30f, 30f);
                float dist = radius * Random.Range(0.5f, 1f);
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(dist, 0f, 0f);
                waypoints.Add(center + offset);
            }

            return waypoints.ToArray();
        }

        Color GetRoleColor(NPCRole role)
        {
            return role switch
            {
                NPCRole.Worker => new Color(0.6f, 0.4f, 0.2f), // Brown work clothes
                NPCRole.Scholar => new Color(0.3f, 0.3f, 0.6f), // Blue scholar robes
                NPCRole.Merchant => new Color(0.6f, 0.2f, 0.4f), // Purple merchant garb
                _ => Color.gray
            };
        }
    }

    public enum NPCRole
    {
        Worker,
        Scholar,
        Merchant,
        QuestGiver
    }

    /// <summary>
    /// Simple NPC controller with basic patrol and idle behavior
    /// </summary>
    public class SimpleNPCController : MonoBehaviour
    {
        public string npcName = "NPC";
        public NPCRole role = NPCRole.Worker;
        public bool isStationary = false;
        public Vector3[] patrolWaypoints = new Vector3[0];

        [Header("Movement")]
        public float moveSpeed = 2f;
        public float rotationSpeed = 120f;
        public float waypointReachDistance = 1f;
        public float idleTimeAtWaypoint = 3f;

        private int currentWaypointIndex = 0;
        private float idleTimer = 0f;
        private bool isIdle = false;

        void Update()
        {
            if (isStationary || patrolWaypoints.Length == 0)
            {
                // Face random direction occasionally
                if (Random.value < 0.01f)
                {
                    transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }
                return;
            }

            if (isIdle)
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    isIdle = false;
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                }
            }
            else
            {
                MoveToWaypoint();
            }
        }

        void MoveToWaypoint()
        {
            Vector3 target = patrolWaypoints[currentWaypointIndex];
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0f; // Keep on ground

            if (direction.magnitude > 0.01f)
            {
                // Rotate towards target
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Move forward
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }

            // Check if reached waypoint
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), 
                                              new Vector3(target.x, 0f, target.z));
            if (distance < waypointReachDistance)
            {
                isIdle = true;
                idleTimer = idleTimeAtWaypoint;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[{npcName}] Player approached - ready for interaction");
                // TODO: Show interaction prompt
            }
        }
    }
}
