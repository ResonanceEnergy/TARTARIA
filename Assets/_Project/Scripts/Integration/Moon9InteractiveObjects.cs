using UnityEngine;
using System.Collections.Generic;
using Tartaria.Input;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-42)]
    public class Moon9InteractiveObjects : MonoBehaviour
    {
        [Header("Moon 9: Corruption Interactive Objects")]
        [SerializeField] int doorCount = 5;
        [SerializeField] int leverCount = 4;
        [SerializeField] int pressurePlateCount = 3;
        [SerializeField] int puzzleCount = 3;
        [SerializeField] int breakableCount = 5;
        List<GameObject> interactives = new List<GameObject>();
        void Start() { SpawnInteractiveObjects(); }
        void SpawnInteractiveObjects()
        {
            for (int i = 0; i < doorCount; i++)
                CreateDoor($"VoidDoor_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), i < 3, "Key_{i}");
            for (int i = 0; i < leverCount; i++)
                CreateLever($"Lever_{i}", new Vector3(Random.Range(-60f, 60f), 1.5f, Random.Range(-60f, 60f)), $"Target_{i}");
            for (int i = 0; i < pressurePlateCount; i++)
                CreatePressurePlate($"Plate_{i}", new Vector3(Random.Range(-60f, 60f), 0.1f, Random.Range(-60f, 60f)), 50f);
            for (int i = 0; i < puzzleCount; i++)
                CreatePuzzleElement($"Puzzle_{i}", new Vector3(Random.Range(-60f, 60f), 1f, Random.Range(-60f, 60f)), "PuzzleType");
            for (int i = 0; i < breakableCount; i++)
                CreateBreakable($"Breakable_{i}", new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)), 30f);
            Debug.Log($"🔧 Moon9InteractiveObjects: {interactives.Count} objects spawned");
        }
        GameObject CreateDoor(string name, Vector3 pos, bool locked, string key) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(3f, 4f, 0.3f); var io = obj.AddComponent<Moon9InteractableObject>(); io.interactType = "door"; io.isLocked = locked; io.requiredKey = key; interactives.Add(obj); return obj; }
        GameObject CreateLever(string name, Vector3 pos, string targetId) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f); var io = obj.AddComponent<Moon9InteractableObject>(); io.interactType = "lever"; io.targetId = targetId; interactives.Add(obj); return obj; }
        GameObject CreatePressurePlate(string name, Vector3 pos, float weight) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(2f, 0.1f, 2f); var io = obj.AddComponent<Moon9InteractableObject>(); io.interactType = "pressurePlate"; io.requiredWeight = weight; interactives.Add(obj); return obj; }
        GameObject CreatePuzzleElement(string name, Vector3 pos, string puzzleType) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; var io = obj.AddComponent<Moon9InteractableObject>(); io.interactType = "puzzle"; io.puzzleType = puzzleType; interactives.Add(obj); return obj; }
        GameObject CreateBreakable(string name, Vector3 pos, float health) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = pos; var io = obj.AddComponent<Moon9InteractableObject>(); io.interactType = "breakable"; io.health = health; interactives.Add(obj); return obj; }
        void OnDestroy() { foreach (var obj in interactives) if (obj != null) Destroy(obj); interactives.Clear(); }
    }
    public class Moon9InteractableObject : MonoBehaviour, IInteractable
    {
        public string interactType;
        public bool isLocked;
        public string requiredKey;
        public string targetId;
        public float requiredWeight;
        public string puzzleType;
        public float health;
        bool activated;
        public string GetInteractPrompt() { if (activated) return ""; return interactType switch { "door" => isLocked ? $"Unlock Door (Need {requiredKey})" : "Open Door (E)", "lever" => "Pull Lever (E)", "pressurePlate" => "Step On Plate", "puzzle" => "Solve Puzzle (E)", "breakable" => "Break Object (E)", _ => "Interact (E)" }; }
        public void Interact(GameObject player) { if (activated) return; activated = true; if (interactType == "door" && !isLocked) { Integration.GameLoopController.Instance?.QueueRSReward(10f, "door"); Debug.Log($"Door opened: {name}"); } else if (interactType == "lever") { Integration.GameLoopController.Instance?.QueueRSReward(5f, "lever"); Debug.Log($"Lever pulled: {targetId}"); } else if (interactType == "puzzle") { QuestManager.Instance?.ProgressByType(Core.QuestObjectiveType.CompleteTuning, puzzleType); Debug.Log($"Puzzle solved: {puzzleType}"); } else if (interactType == "breakable") { Integration.GameLoopController.Instance?.QueueRSReward(3f, "break"); Destroy(gameObject, 0.2f); } }
    }
}
