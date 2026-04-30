using UnityEngine;

namespace Tartaria.Core.Data
{
    /// <summary>
    /// Visual data profile for a character (Player, NPC). Decouples the spawner code
    /// from any specific mesh/animator/audio setup. Swap profiles to upgrade visuals
    /// from capsule placeholder → low-poly → AAA without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Visual/Character Visual Profile", fileName = "Profile_Character")]
    public class CharacterVisualProfile : ScriptableObject
    {
        [Header("Identity")]
        public string characterId = "elara";
        public string displayName = "Elara Voss";

        [Header("Visual")]
        [Tooltip("Mesh prefab applied as a child of the spawned Player root. " +
                 "If null, the spawner keeps whatever the Player prefab already has " +
                 "(useful for the procedural capsule placeholder).")]
        public GameObject meshPrefab;

        [Tooltip("Animator controller applied to the spawned Player Animator. " +
                 "If null, the spawner keeps whatever the Player prefab already references.")]
        public RuntimeAnimatorController animatorController;

        [Tooltip("Optional avatar (Humanoid). Required when swapping to a humanoid mesh.")]
        public Avatar avatar;

        [Tooltip("Optional Y offset applied to the spawned mesh prefab " +
                 "(e.g. to align Mixamo root with capsule baseline).")]
        public float meshYOffset = 0f;

        [Header("Audio")]
        public AudioClip[] footstepClips;
        [Range(0f, 1f)] public float footstepVolume = 0.6f;

        [Header("VFX")]
        public GameObject footstepVFX;
        public GameObject jumpVFX;
        public GameObject hitVFX;

        public bool HasMesh => meshPrefab != null;
        public bool HasAnimator => animatorController != null;
    }
}
