using System.Collections.Generic;
using UnityEngine;

namespace Tartaria.Animation
{
    /// <summary>
    /// Runtime bootstrapper: after a scene loads, find every Animator in the scene
    /// whose owning GameObject is tagged as an NPC/villager/named hero and attach
    /// an <see cref="NPCWalkAnimator"/> component if it doesn't already have one.
    ///
    /// This avoids touching prefabs or scene files — wiring happens at Play time
    /// and respects the Animation agent's path ownership (scripts only).
    /// </summary>
    public static class NPCWalkAnimatorAutoAttach
    {
        // Tags that mark a GameObject as an animatable NPC.
        static readonly HashSet<string> NpcTags = new HashSet<string>
        {
            "NPC",
            "Villager",
            "Milo",
            "Cassian",
            "Anastasia",
            "Lirael"
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            int attached = 0;
            int skippedExisting = 0;
            int skippedUntagged = 0;

            Animator[] animators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator anim = animators[i];
                if (anim == null) continue;

                GameObject go = anim.gameObject;

                // Untagged GameObjects in Unity carry the literal tag "Untagged".
                // Guard against TagManager rejecting unknown tag lookups by
                // checking the string directly rather than calling CompareTag.
                string tag = go.tag;
                if (string.IsNullOrEmpty(tag) || !NpcTags.Contains(tag))
                {
                    skippedUntagged++;
                    continue;
                }

                if (go.GetComponent<NPCWalkAnimator>() != null)
                {
                    skippedExisting++;
                    continue;
                }

                go.AddComponent<NPCWalkAnimator>();
                attached++;
            }

            Debug.Log(
                $"[NPCWalkAnimatorAutoAttach] Attached NPCWalkAnimator to {attached} GameObject(s). " +
                $"Skipped {skippedExisting} (already had it), {skippedUntagged} (non-NPC tag). " +
                $"Scanned {animators.Length} Animator(s) total.");
        }
    }
}
