using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// SpireRestorationSystem - Crystal Spire (Aether conduit) restoration.
    /// </summary>
    public class SpireRestorationSystem : MonoBehaviour
    {
        [SerializeField] private string buildingId = "spire";
        [SerializeField] private bool isRestored = false;
        [SerializeField] private int totalNodes = 4; // Harder building
        private int _nodesCompleted = 0;

        public void TuneNode(int nodeIndex)
        {
            _nodesCompleted++;
            if (_nodesCompleted >= totalNodes && !isRestored)
            {
                isRestored = true;
                GameEvents.FireBuildingRestored(buildingId);
                GameLoopController.Instance?.AwardRS(75f, "Spire Restoration");
                Debug.Log("[SpireRestoration] ✅ COMPLETE!");
            }
        }

        public bool IsRestored() => isRestored;
    }
}
