using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// NPCConditionalSpawn — keeps an NPC hidden until a specific building is
    /// restored. Watches GameEvents.OnBuildingRestored and reveals its child
    /// "Visual" GameObject when its `_triggerBuildingId` fires.
    ///
    /// Used by Moon1BuildOutNPCs for Anastasia (revealed after Star Dome
    /// restoration per docs/03 campaign beat order) and can be reused for any
    /// progression-gated NPC.
    /// </summary>
    public class NPCConditionalSpawn : MonoBehaviour
    {
        [SerializeField] private string _triggerBuildingId = "echohaven_stardome";
        [SerializeField] private bool _revealed;

        public void SetRestoreTrigger(string buildingId)
        {
            _triggerBuildingId = buildingId;
        }

        void OnEnable()
        {
            GameEvents.OnBuildingRestoredTyped += HandleRestored;
        }

        void OnDisable()
        {
            GameEvents.OnBuildingRestoredTyped -= HandleRestored;
        }

        void HandleRestored(BuildingRestoredEventArgs args)
        {
            if (_revealed) return;
            if (args == null || args.buildingId != _triggerBuildingId) return;

            _revealed = true;
            var visual = transform.Find("Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(true);
                Debug.Log($"[NPCConditionalSpawn] Revealed {gameObject.name} on restoration of {args.buildingId}");
            }

            // Audible / banner cue for the player
            ServiceLocator.HUD?.ShowBanner("A new figure appears", gameObject.name, 4f);
        }
    }
}
