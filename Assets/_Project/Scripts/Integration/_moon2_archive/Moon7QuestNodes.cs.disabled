using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
#pragma warning disable CS0414 // Placeholder counts for planned features
    [DefaultExecutionOrder(-48)]
    public class Moon7QuestNodes : MonoBehaviour
    {
        [Header("Moon 7 Quest Configuration")]
        [SerializeField] int questStartNodeCount = 5;
        [SerializeField] int questCompletionNodeCount = 4;
        [SerializeField] int objectiveMarkerCount = 8;
        List<GameObject> questNodes = new List<GameObject>();
        void Start() { SpawnQuestNodes(); }
        void SpawnQuestNodes()
        {
            for (int i = 0; i < questStartNodeCount; i++)
                CreateQuestNode($"QuestStart_Moon7_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), "QuestStart", $"moon7_quest_{i}");
            for (int i = 0; i < questCompletionNodeCount; i++)
                CreateQuestNode($"QuestComplete_Moon7_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), "QuestComplete", $"moon7_quest_{i}");
            for (int i = 0; i < objectiveMarkerCount; i++)
                CreateQuestNode($"Objective_Moon7_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), "Objective", $"moon7_objective_{i}");
            Debug.Log($"🎯 Moon7QuestNodes spawned {questNodes.Count} quest nodes");
        }
        GameObject CreateQuestNode(string name, Vector3 position, string nodeType, string questId)
        {
            GameObject node = new GameObject(name);
            node.transform.position = position;
            SphereCollider trigger = node.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = nodeType == "Objective" ? 3f : 5f;
            QuestTriggerZone qtzone = node.AddComponent<QuestTriggerZone>();
            qtzone.questId = questId;
            qtzone.nodeType = nodeType;
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(node.transform, false);
            visual.transform.localPosition = Vector3.up * 2f;
            visual.transform.localScale = Vector3.one * 0.5f;
            Destroy(visual.GetComponent<Collider>());
            Renderer rend = visual.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                Color markerColor = nodeType switch { "QuestStart" => new Color(1f, 0.9f, 0.3f), "QuestComplete" => new Color(0.3f, 1f, 0.4f), "Objective" => new Color(0.4f, 0.7f, 1f), _ => Color.white };
                mat.color = markerColor;
                mat.SetColor("_EmissionColor", markerColor * 1.5f);
                mat.EnableKeyword("_EMISSION");
                rend.material = mat;
            }
            questNodes.Add(node);
            return node;
        }
        void OnDestroy() { foreach (GameObject node in questNodes) if (node != null) Destroy(node); questNodes.Clear(); }
    }
}