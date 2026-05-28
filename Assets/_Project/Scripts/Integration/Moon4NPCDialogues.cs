using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-39)]
    public class Moon4NPCDialogues : MonoBehaviour
    {
        [Header("Moon 4: Desert NPCs")]
        [SerializeField] int questGiverCount = 3;
        [SerializeField] int merchantCount = 2;
        [SerializeField] int loreNPCCount = 2;
        [SerializeField] int helperNPCCount = 1;
        List<GameObject> npcs = new List<GameObject>();
        void Start() { SpawnNPCs(); }
        void SpawnNPCs()
        {
            for (int i = 0; i < questGiverCount; i++)
                CreateQuestGiver($"QuestGiver_{i}", new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f)), $"moon4_quest_{i}", new string[] { "Greetings, adventurer.", "I have a task for you." });
            for (int i = 0; i < merchantCount; i++)
                CreateMerchant($"Merchant_{i}", new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f)), $"shop4_{i}", new string[] { "Looking to trade?", "I have rare goods." });
            for (int i = 0; i < loreNPCCount; i++)
                CreateLoreNPC($"Sage_{i}", new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f)), new string[] { "This place holds ancient secrets.", "Listen and learn." });
            CreateHelper("Helper_Desert", new Vector3(0f, 0.5f, 0f), new string[] { "Need help? Follow the markers.", "Check your map." });
            Debug.Log($"💬 Moon4NPCDialogues: {npcs.Count} NPCs spawned");
        }
        GameObject CreateQuestGiver(string name, Vector3 pos, string questId, string[] dialogue) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(1f, 2f, 1f); var npc = obj.AddComponent<DialogueNPC>(); npc.npcName = name; npc.npcType = "questGiver"; npc.dialogueLines = dialogue; npc.questId = questId; npcs.Add(obj); return obj; }
        GameObject CreateMerchant(string name, Vector3 pos, string shopId, string[] dialogue) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(1f, 2f, 1f); var npc = obj.AddComponent<DialogueNPC>(); npc.npcName = name; npc.npcType = "merchant"; npc.dialogueLines = dialogue; npc.shopId = shopId; npcs.Add(obj); return obj; }
        GameObject CreateLoreNPC(string name, Vector3 pos, string[] loreDialogue) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(1f, 2f, 1f); var npc = obj.AddComponent<DialogueNPC>(); npc.npcName = name; npc.npcType = "lore"; npc.dialogueLines = loreDialogue; npcs.Add(obj); return obj; }
        GameObject CreateHelper(string name, Vector3 pos, string[] hints) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(1f, 2f, 1f); var npc = obj.AddComponent<DialogueNPC>(); npc.npcName = name; npc.npcType = "helper"; npc.dialogueLines = hints; npcs.Add(obj); return obj; }
        void OnDestroy() { foreach (var npc in npcs) if (npc != null) Destroy(npc); npcs.Clear(); }
    }
    public class DialogueNPC : MonoBehaviour, IInteractable
    {
        public string npcName;
        public string[] dialogueLines;
        public string npcType;
        public string questId;
        public string shopId;
        int dialogueIndex;
        public string GetInteractPrompt() { return $"Talk to {npcName} (E)"; }
        public void Interact(GameObject player) { if (dialogueIndex < dialogueLines.Length) { Core.GameEvents.RaiseHUDShowObjective(dialogueLines[dialogueIndex]); dialogueIndex++; if (npcType == "questGiver" && dialogueIndex >= dialogueLines.Length) { QuestManager.Instance?.ActivateQuest(questId); } } }
    }
}
