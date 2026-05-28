using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Quest Nodes - Quest triggers and completion zones for The Verdant Labyrinth
    /// </summary>
    [DefaultExecutionOrder(-48)]
    public class Moon3QuestNodes : MonoBehaviour
    {
        [Header("Moon 3: Jungle Quest Configuration")]
        [SerializeField] int questStartNodeCount = 5;
        [SerializeField] int questCompletionNodeCount = 4;
        [SerializeField] int objectiveMarkerCount = 8;

        List<GameObject> questNodes = new List<GameObject>();

        void Start()
        {
            SpawnQuestNodes();
        }

        void SpawnQuestNodes()
        {
            // Quest Start Nodes - NPCs or markers that activate quests
            for (int i = 0; i < questStartNodeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0.5f,
                    Random.Range(-60f, 60f)
                );
                CreateQuestNode($"QuestStart_Moon3_{i}", pos, "QuestStart", $"moon3_quest_{i}");
            }

            // Quest Completion Nodes - Turn-in locations
            for (int i = 0; i < questCompletionNodeCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0.5f,
                    Random.Range(-60f, 60f)
                );
                CreateQuestNode($"QuestComplete_Moon3_{i}", pos, "QuestComplete", $"moon3_quest_{i}");
            }

            // Objective Markers - Interact points for quest objectives
            for (int i = 0; i < objectiveMarkerCount; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-60f, 60f),
                    0.5f,
                    Random.Range(-60f, 60f)
                );
                CreateQuestNode($"Objective_Moon3_{i}", pos, "Objective", $"moon3_objective_{i}");
            }

            Debug.Log($"🎯 Moon3QuestNodes spawned {questNodes.Count} quest nodes");
        }

        GameObject CreateQuestNode(string name, Vector3 position, string nodeType, string questId)
        {
            GameObject node = new GameObject(name);
            node.transform.position = position;

            // Add trigger collider
            SphereCollider trigger = node.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = nodeType == "Objective" ? 3f : 5f;

            // Add quest trigger component
            QuestTriggerZone qtzone = node.AddComponent<QuestTriggerZone>();
            qtzone.questId = questId;
            qtzone.nodeType = nodeType;

            // Visual marker (glowing sphere)
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(node.transform, false);
            visual.transform.localPosition = Vector3.up * 2f;
            visual.transform.localScale = Vector3.one * 0.5f;
            Destroy(visual.GetComponent<Collider>());

            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                Color markerColor = nodeType switch
                {
                    "QuestStart" => new Color(1f, 0.9f, 0.3f), // Gold
                    "QuestComplete" => new Color(0.3f, 1f, 0.4f), // Green
                    "Objective" => new Color(0.4f, 0.7f, 1f), // Blue
                    _ => Color.white
                };
                mat.color = markerColor;
                mat.SetColor("_EmissionColor", markerColor * 1.5f);
                mat.EnableKeyword("_EMISSION");
                rend.material = mat;
            }

            questNodes.Add(node);
            return node;
        }

        void OnDestroy()
        {
            foreach (GameObject node in questNodes)
            {
                if (node != null) Destroy(node);
            }
            questNodes.Clear();
        }
    }

    /// <summary>
    /// Quest trigger zone component - handles player interactions with quest nodes
    /// </summary>
    public class QuestTriggerZone : MonoBehaviour
    {
        public string questId;
        public string nodeType;
        bool triggered;

        void OnTriggerEnter(Collider other)
        {
            if (triggered || !other.CompareTag("Player")) return;

            if (nodeType == "QuestStart")
            {
                QuestManager.Instance?.ActivateQuest(questId);
                Core.GameEvents.RaiseHUDShowObjective($"Quest Started: {questId}");
                Debug.Log($"[QuestTrigger] Activated quest: {questId}");
                triggered = true;
            }
            else if (nodeType == "QuestComplete")
            {
                if (QuestManager.Instance?.IsQuestActive(questId) == true)
                {
                    QuestManager.Instance?.CompleteQuest(questId);
                    Debug.Log($"[QuestTrigger] Completed quest: {questId}");
                    triggered = true;
                }
            }
            else if (nodeType == "Objective")
            {
                QuestManager.Instance?.ProgressByType(Core.Enums.QuestObjectiveType.ReachLocation, questId);
                Debug.Log($"[QuestTrigger] Reached objective: {questId}");
                triggered = true;
            }
        }
    }
}
