using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    /// <summary>
    /// Moon 3: The Verdant Labyrinth - Ambient Creatures
    /// Execution order: -63 (after InteractiveObjects -65)
    /// Spawns jungle-themed ambient creatures: birds, butterflies, tree frogs, lizards
    /// Non-interactable animated creatures that patrol and add life to the environment
    /// </summary>
    [DefaultExecutionOrder(-63)]
    public class Moon3AmbientCreatures : MonoBehaviour
    {
        [Header("Jungle Creatures")]
        [SerializeField] int birdCount = 15;
        [SerializeField] int butterflyCount = 20;
        [SerializeField] int treeFrogCount = 12;
        [SerializeField] int lizardCount = 10;

        List<GameObject> creatures = new List<GameObject>();

        void Start()
        {
            SpawnCreatures();
        }

        void SpawnCreatures()
        {
            // Birds (flying high)
            for (int i = 0; i < birdCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(10f, 25f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Jungle_Bird_{i}", pos, new Vector3(0.5f, 0.3f, 0.8f), new Color(0.2f, 0.6f, 0.3f), 5f);
            }

            // Butterflies (mid-level flying)
            for (int i = 0; i < butterflyCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(1f, 8f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Butterfly_{i}", pos, new Vector3(0.4f, 0.1f, 0.4f), new Color(0.9f, 0.7f, 0.3f), 2f);
            }

            // Tree frogs (on elevated positions)
            for (int i = 0; i < treeFrogCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    Random.Range(2f, 6f),
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Tree_Frog_{i}", pos, new Vector3(0.3f, 0.3f, 0.3f), new Color(0.1f, 0.7f, 0.2f), 1f);
            }

            // Lizards (ground level)
            for (int i = 0; i < lizardCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-160f, 160f),
                    0.2f,
                    Random.Range(-160f, 160f)
                );
                CreateCreature($"Lizard_{i}", pos, new Vector3(0.4f, 0.2f, 0.6f), new Color(0.3f, 0.5f, 0.2f), 3f);
            }

            Debug.Log($"🦜 JUNGLE CREATURES: {creatures.Count} ambient creatures adding life to the jungle");
        }

        void CreateCreature(string name, Vector3 position, Vector3 scale, Color color, float speed)
        {
            GameObject creature = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            creature.name = name;
            creature.transform.position = position;
            creature.transform.localScale = scale;
            creature.transform.parent = transform;
            creature.tag = "Creature";

            Renderer renderer = creature.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.6f);
            renderer.material = mat;

            // Add simple patrol behavior
            CreaturePatrol patrol = creature.AddComponent<CreaturePatrol>();
            patrol.patrolSpeed = speed;
            patrol.patrolRadius = Random.Range(10f, 30f);
            patrol.startPosition = position;

            Destroy(creature.GetComponent<SphereCollider>()); // Non-interactive

            creatures.Add(creature);
        }

        void OnDestroy()
        {
            foreach (var creature in creatures)
            {
                if (creature != null) Destroy(creature);
            }
            creatures.Clear();
        }
    }

    /// <summary>
    /// Simple patrol behavior for ambient creatures
    /// </summary>
    public class CreaturePatrol : MonoBehaviour
    {
        public float patrolSpeed = 3f;
        public float patrolRadius = 20f;
        public Vector3 startPosition;

        Vector3 targetPosition;
        float nextMoveTime;

        void Start()
        {
            SetNewTarget();
        }

        void Update()
        {
            if (Time.time >= nextMoveTime)
            {
                SetNewTarget();
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                SetNewTarget();
            }
        }

        void SetNewTarget()
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                Random.Range(-5f, 5f),
                Random.Range(-patrolRadius, patrolRadius)
            );
            targetPosition = startPosition + randomOffset;
            nextMoveTime = Time.time + Random.Range(3f, 8f);
        }
    }
}
