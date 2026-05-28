using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Quest Triggers — Resonance puzzle activation zones
    /// 5 quest zones corresponding to the 5 chambers
    /// Main quest: Harmonize the cavern's crystals to unlock the Harmonic Sanctum
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon2QuestTriggers : MonoBehaviour
    {
        [Header("Quest Configuration")]
        [SerializeField] float triggerRadius = 10f;
        [SerializeField] bool triggerOnce = true;

        void Start()
        {
            CreateQuestTriggers();
        }

        void CreateQuestTriggers()
        {
            Debug.Log("[Moon2QuestTriggers] Setting up quest trigger zones...");

            // 1. The Resonator (main quest giver) at Resonance Chamber entrance
            CreateQuestZone(
                "Quest_TheResonator",
                new Vector3(0f, 2f, 55f),
                8f,
                "Meet The Resonator",
                ActivateMainQuest
            );

            // 2. Echo Hall — Calibrate echo patterns
            CreateQuestZone(
                "Quest_EchoHall",
                new Vector3(-50f, -8f, 0f),
                12f,
                "Calibrate Echo Patterns",
                ActivateEchoQuest
            );

            // 3. Crystal Grotto — Tune crystal frequencies
            CreateQuestZone(
                "Quest_CrystalGrotto",
                new Vector3(60f, -13f, 20f),
                10f,
                "Tune Crystal Frequencies",
                ActivateCrystalQuest
            );

            // 4. Resonance Chamber — Activate resonance altar
            CreateQuestZone(
                "Quest_ResonanceChamber",
                new Vector3(0f, -18f, 50f),
                15f,
                "Activate Resonance Altar",
                ActivateResonanceQuest
            );

            // 5. Harmonic Sanctum — Unlock final chamber
            CreateQuestZone(
                "Quest_HarmonicSanctum",
                new Vector3(0f, -33f, 0f),
                12f,
                "Enter Harmonic Sanctum",
                ActivateSanctumQuest
            );

            Debug.Log("[Moon2QuestTriggers] ✅ 5 quest trigger zones created!");
        }

        void CreateQuestZone(string name, Vector3 position, float radius, string questTitle, System.Action onActivate)
        {
            var triggerObj = new GameObject(name);
            triggerObj.transform.position = position;
            triggerObj.layer = LayerMask.NameToLayer("Default");

            var trigger = triggerObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;

            var questTrigger = triggerObj.AddComponent<Moon2QuestZoneTrigger>();
            questTrigger.questTitle = questTitle;
            questTrigger.onPlayerEnter = onActivate;
            questTrigger.triggerOnce = triggerOnce;

            Debug.Log($"  ✓ {name}: {questTitle} ({radius}m radius)");
        }

        // Quest activation methods
        void ActivateMainQuest()
        {
            Debug.Log("🎯 [Quest] The Resonator: \"Welcome, seeker. The caverns await your harmony.\"");
            // TODO: Integrate with QuestSystem.Instance.ActivateQuest("moon2_resonator");
        }

        void ActivateEchoQuest()
        {
            Debug.Log("🎯 [Quest] Echo Hall: Sound waves bounce through the corridor...");
            // TODO: Integrate with QuestSystem.Instance.ActivateQuest("moon2_echo_hall");
        }

        void ActivateCrystalQuest()
        {
            Debug.Log("🎯 [Quest] Crystal Grotto: The crystals hum at different frequencies...");
            // TODO: Integrate with QuestSystem.Instance.ActivateQuest("moon2_crystal_grotto");
        }

        void ActivateResonanceQuest()
        {
            Debug.Log("🎯 [Quest] Resonance Chamber: The altar pulses with dormant energy...");
            // TODO: Integrate with QuestSystem.Instance.ActivateQuest("moon2_resonance_chamber");
        }

        void ActivateSanctumQuest()
        {
            Debug.Log("🎯 [Quest] Harmonic Sanctum: The final harmony must be achieved...");
            // TODO: Integrate with QuestSystem.Instance.ActivateQuest("moon2_harmonic_sanctum");
        }
    }

    /// <summary>
    /// Quest Zone Trigger — Activates quest when player enters
    /// </summary>
    public class Moon2QuestZoneTrigger : MonoBehaviour
    {
        public string questTitle;
        public System.Action onPlayerEnter;
        public bool triggerOnce = true;

        private bool hasTriggered = false;

        void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;

            Debug.Log($"[QuestZoneTrigger] Player entered: {questTitle}");
            onPlayerEnter?.Invoke();

            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }
}
