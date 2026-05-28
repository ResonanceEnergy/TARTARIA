using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 NPC Spawner — Cave-dwelling scholars and crystal guardians
    /// 8 NPCs: scholars studying acoustics, guardians protecting chambers
    /// </summary>
    [DefaultExecutionOrder(-82)]
    public class Moon2NPCSpawner : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] int scholarCount = 5;
        [SerializeField] int guardianCount = 3;

        private enum NPCRole
        {
            Scholar,    // Studying resonance/acoustics
            Guardian,   // Protecting chambers
            QuestGiver  // Main NPC (The Resonator)
        }

        void Start()
        {
            SpawnNPCs();
        }

        void SpawnNPCs()
        {
            Debug.Log("[Moon2NPCSpawner] Spawning cavern NPCs...");

            // Main quest giver: "The Resonator" at Resonance Chamber
            SpawnQuestGiver();

            // 5 Scholars at various chambers
            SpawnScholar("Scholar_Entrance", new Vector3(10f, 2f, -75f), "Entrance Chamber");
            SpawnScholar("Scholar_EchoHall_A", new Vector3(-45f, 0f, 10f), "Echo Hall");
            SpawnScholar("Scholar_EchoHall_B", new Vector3(-55f, 0f, -10f), "Echo Hall");
            SpawnScholar("Scholar_CrystalGrotto", new Vector3(55f, 0f, 15f), "Crystal Grotto");
            SpawnScholar("Scholar_Sanctum", new Vector3(-5f, -33f, 5f), "Harmonic Sanctum");

            // 3 Guardians at key locations
            SpawnGuardian("Guardian_Resonance", new Vector3(18f, 0f, 45f), "Resonance Chamber");
            SpawnGuardian("Guardian_Grotto", new Vector3(60f, 0f, 25f), "Crystal Grotto");
            SpawnGuardian("Guardian_Sanctum", new Vector3(0f, -33f, 0f), "Harmonic Sanctum");

            Debug.Log($"[Moon2NPCSpawner] ✅ Spawned 1 quest giver + {scholarCount} scholars + {guardianCount} guardians");
        }

        void SpawnQuestGiver()
        {
            var npc = new GameObject("NPC_TheResonator");
            npc.transform.position = new Vector3(0f, 2f, 55f); // Near Resonance Chamber entrance
            npc.tag = "NPC";

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(npc.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Larger

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.9f, 0.6f, 1f); // Purple (harmonic)
            material.SetFloat("_Metallic", 0.5f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.9f, 0.6f, 1f) * 0.5f);
            visual.GetComponent<Renderer>().material = material;

            // Collision
            npc.AddComponent<CapsuleCollider>().radius = 0.8f;

            // Add interaction trigger
            var trigger = npc.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3f;

            Debug.Log("  ✓ The Resonator (quest giver) at Resonance Chamber");
        }

        void SpawnScholar(string npcName, Vector3 position, string location)
        {
            var npc = new GameObject(npcName);
            npc.transform.position = position;
            npc.tag = "NPC";

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(npc.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.6f, 0.9f); // Blue (scholar)
            visual.GetComponent<Renderer>().material = material;

            // Collision
            npc.AddComponent<CapsuleCollider>().radius = 0.5f;

            // Simple patrol behavior
            var patroller = npc.AddComponent<SimpleNPCPatrol>();
            patroller.patrolRadius = 8f;
            patroller.moveSpeed = 1.5f;

            Debug.Log($"  ✓ {npcName} at {location}");
        }

        void SpawnGuardian(string npcName, Vector3 position, string location)
        {
            var npc = new GameObject(npcName);
            npc.transform.position = position;
            npc.tag = "NPC";

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(npc.transform);
            visual.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            visual.transform.localScale = new Vector3(1.2f, 2.4f, 1.2f); // Taller, wider

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.8f, 0.3f, 0.1f); // Orange-red (guardian)
            material.SetFloat("_Metallic", 0.8f);
            visual.GetComponent<Renderer>().material = material;

            // Collision
            var collider = npc.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 2.4f, 1.2f);

            // Stationary guardian (no patrol)
            Debug.Log($"  ✓ {npcName} at {location}");
        }
    }

    /// <summary>
    /// Simple NPC Patrol — Random patrol around spawn point
    /// </summary>
    public class SimpleNPCPatrol : MonoBehaviour
    {
        public float patrolRadius = 8f;
        public float moveSpeed = 1.5f;
        public float waypointWaitTime = 3f;

        private Vector3 spawnPoint;
        private Vector3 targetWaypoint;
        private float waitTimer;
        private bool isWaiting;

        void Start()
        {
            spawnPoint = transform.position;
            PickNewWaypoint();
        }

        void Update()
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    PickNewWaypoint();
                }
                return;
            }

            // Move toward waypoint
            Vector3 direction = (targetWaypoint - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.forward = direction;

            // Check if reached waypoint
            if (Vector3.Distance(transform.position, targetWaypoint) < 0.5f)
            {
                isWaiting = true;
                waitTimer = waypointWaitTime;
            }
        }

        void PickNewWaypoint()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            targetWaypoint = spawnPoint + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }
    }
}
