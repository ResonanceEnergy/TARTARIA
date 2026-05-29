using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// AnastasiaController - Scholar companion (4th character - COMPLETE!).
    /// Lore expert, building analysis, research insights.
    /// </summary>
    public class AnastasiaController : MonoBehaviour
    {

        private readonly string[] INTRO_LINES = new[]
        {
            "Fascinating! These resonance patterns are unlike anything in the historical records.",
            "I''ve spent years researching the Tartarian civilization. This is unprecedented.",
            "Let me analyze this structure. There may be clues in the architecture."
        };

        private readonly string[] DISCOVERY_LINES = new[]
        {
            "Look at this! Golden ratio proportions everywhere!",
            "The crystalline lattice structure... it''s designed to amplify specific frequencies!",
            "This wasn''t built with tools. This was *sung* into existence.",
            "According to my research, this aligns with the 432 Hz theory."
        };

        private readonly string[] ANALYSIS_LINES = new[]
        {
            "The degradation pattern suggests a sudden, catastrophic disruption.",
            "Notice the mud composition? This isn''t natural sediment.",
            "The electromagnetic readings here are off the charts.",
            "My hypothesis: this was intentionally buried, not naturally covered."
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            GameEvents.OnBuildingDiscovered += OnBuildingDiscovered;
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            Debug.Log($"[AnastasiaController] ✅ {characterName} initialized");
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingDiscovered -= OnBuildingDiscovered;
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
        }

        void OnBuildingDiscovered(string buildingName, Vector3 position)
        {
            SayRandomLine(DISCOVERY_LINES);
            DiscoverLorePiece();
        }

        void OnBuildingRestored(string buildingId)
        {
            SayRandomLine(ANALYSIS_LINES);
            researchLevel = Mathf.Min(100, researchLevel + 10);
        }

        void DiscoverLorePiece()
        {
            lorePiecesDiscovered++;
            Debug.Log($"[Anastasia] Lore pieces: {lorePiecesDiscovered}");
        }

        public void ProvideResearchInsight(string topic)
        {
            Say($"About {topic}: According to my research, the Tartarians used cymatics to manipulate matter at the molecular level.");
        }

        void Say(string line)
        {
            Debug.Log($"[Anastasia]: {line}");
            DialogueManager.Instance?.ShowDialogue(characterName, line, 5f);
        }

        void SayRandomLine(string[] lines)
        {
            if (lines.Length > 0)
                Say(lines[Random.Range(0, lines.Length)]);
        }

        public int GetResearchLevel() => researchLevel;
        public int GetLorePiecesDiscovered() => lorePiecesDiscovered;
    }
}
