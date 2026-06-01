using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// MoonConfig — ScriptableObject describing the per-moon shippable spec
    /// (intro narrative, building set, RS reward, capstone audio). Keeps Moon 1
    /// hand-tuned values as data instead of constants in <c>EchohavenProgressionSystem</c>.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Level Designer (moonconfig-factory-seed).
    ///
    /// Authored assets live at <c>Assets/_Project/Data/Moons/Moon{N}Config.asset</c>.
    /// Loader (<see cref="MoonConfigLoader"/>) resolves by 1-based moon index.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Moon/MoonConfig", fileName = "MoonConfig", order = 100)]
    public class MoonConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("1-based moon index (Moon 1 = Echohaven, Moon 2 = …).")]
        public int moonIndex = 1;
        public string moonName = "Echohaven";
        [TextArea(2, 5)] public string introNarrative = "The first moon stirs. Resonance is faint but reachable.";

        [Header("Restoration")]
        [Tooltip("Building IDs required to clear the moon (matches InteractableBuilding.BuildingId).")]
        public string[] buildings = new[] { "fountain", "dome", "spire" };
        [Tooltip("RS awarded on full moon completion.")]
        public int rsReward = 100;

        [Header("Audio")]
        [Tooltip("Resources path to the moon-complete capstone clip (without extension).")]
        public string capstoneStingerResourcePath = "Audio/Stingers/EchohavenAwakened";

        [Header("Capstone")]
        [Tooltip("Seconds after WinScreen before the post-credits reveal fires.")]
        public float postWinRevealDelaySeconds = 8.7f;
        [Tooltip("DialogueManager context ID for the post-credits reveal (Yarn node title).")]
        public string postWinRevealDialogueContext = "anastasia_reveal";
    }
}
