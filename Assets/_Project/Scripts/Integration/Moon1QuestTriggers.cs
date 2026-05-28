using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Quest Triggers — Sets up quest activation zones for Echohaven
    /// Triggers: Milo intro, first excavation, Cathedral discovery, Spire activation
    /// Integrates with QuestSystem to track progress
    /// </summary>
    [DefaultExecutionOrder(-81)] // After NPCs (-82)
    public class Moon1QuestTriggers : MonoBehaviour
    {
        [Header("Quest Trigger Zones")]
        [SerializeField] Vector3 miloZoneCenter = new Vector3(-40f, 0f, 20f);
        [SerializeField] Vector3 cathedralZoneCenter = new Vector3(0f, 0f, 80f);
        [SerializeField] Vector3 spireZoneCenter = new Vector3(60f, 0f, 40f);
        [SerializeField] float triggerRadius = 8f;

        [Header("Quest IDs")]
        [SerializeField] string miloQuestId = "moon1_meet_milo";
        [SerializeField] string excavationQuestId = "moon1_restore_first";
        [SerializeField] string shardQuestId = "moon1_collect_shards";

        void Start()
        {
            CreateQuestTriggers();
        }

        void CreateQuestTriggers()
        {
            Debug.Log("[Moon1QuestTriggers] Creating quest trigger zones...");

            var triggerParent = new GameObject("Quest_Triggers");
            triggerParent.transform.position = Vector3.zero;

            // Milo introduction trigger
            CreateTriggerZone(triggerParent, "Milo_Quest_Trigger", miloZoneCenter, triggerRadius,
                () => ActivateMiloQuest());

            // Cathedral discovery trigger
            CreateTriggerZone(triggerParent, "Cathedral_Quest_Trigger", cathedralZoneCenter, triggerRadius,
                () => ActivateCathedralQuest());

            // Spire activation trigger
            CreateTriggerZone(triggerParent, "Spire_Quest_Trigger", spireZoneCenter, triggerRadius,
                () => ActivateSpireQuest());

            Debug.Log("[Moon1QuestTriggers] ✅ 3 quest triggers created");
        }

        void CreateTriggerZone(GameObject parent, string name, Vector3 position, float radius, System.Action onTrigger)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent.transform);
            zone.transform.position = position;

            // Add trigger collider
            var collider = zone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = radius;

            // Add trigger component
            var trigger = zone.AddComponent<QuestZoneTrigger>();
            trigger.onPlayerEnter = onTrigger;
            trigger.triggerOnce = true;

            Debug.Log($"  ✓ {name} at {position} (radius: {radius}m)");
        }

        void ActivateMiloQuest()
        {
            Debug.Log("[Moon1QuestTriggers] Activating Milo quest...");

            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.ActivateQuest(miloQuestId);
            }

            // Show notification
            ShowQuestNotification("New Quest", "Talk to Milo the Mapmaker");
        }

        void ActivateCathedralQuest()
        {
            Debug.Log("[Moon1QuestTriggers] Activating Cathedral excavation quest...");

            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.ActivateQuest(excavationQuestId);
            }

            ShowQuestNotification("New Quest", "Restore Echohaven Cathedral");
        }

        void ActivateSpireQuest()
        {
            Debug.Log("[Moon1QuestTriggers] Activating Spire quest...");

            if (QuestSystem.Instance != null)
            {
                // Complete shard collection objective when player reaches spire
                QuestSystem.Instance.CompleteObjective(shardQuestId, 0);
            }

            ShowQuestNotification("Objective Complete", "Aether Shards collected!");
        }

        void ShowQuestNotification(string title, string message)
        {
            // TODO: Integrate with NotificationSystem when available
            Debug.Log($"[QUEST] {title}: {message}");
        }
    }

    /// <summary>
    /// Quest Zone Trigger — Simple trigger component for quest activation
    /// </summary>
    public class QuestZoneTrigger : MonoBehaviour
    {
        public System.Action onPlayerEnter;
        public bool triggerOnce = true;
        private bool hasTriggered = false;

        void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce)
                return;

            if (other.CompareTag("Player"))
            {
                Debug.Log($"[QuestZoneTrigger] {gameObject.name} activated by player");
                onPlayerEnter?.Invoke();
                hasTriggered = true;

                if (triggerOnce)
                {
                    // Disable collider after first trigger
                    GetComponent<Collider>().enabled = false;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualize trigger zone in editor
            Gizmos.color = hasTriggered ? Color.gray : Color.yellow;
            var collider = GetComponent<SphereCollider>();
            if (collider != null)
            {
                Gizmos.DrawWireSphere(transform.position, collider.radius);
            }
        }
    }
}
