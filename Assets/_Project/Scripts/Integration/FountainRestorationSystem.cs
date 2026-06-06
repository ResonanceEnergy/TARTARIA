using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// FountainRestorationSystem - Harmonic Fountain restoration.
    /// </summary>
    public class FountainRestorationSystem : MonoBehaviour
    {
        [SerializeField] private string buildingId = "fountain";
        [SerializeField] private bool isRestored = false;
        [SerializeField] private int totalNodes = 3;
        private int _nodesCompleted = 0;

        public void TuneNode(int nodeIndex)
        {
            _nodesCompleted++;
            if (_nodesCompleted >= totalNodes && !isRestored)
            {
                isRestored = true;
                GameEvents.FireBuildingRestored(buildingId);
                GameLoopController.Instance?.AwardRS(40f, "Fountain Restoration");
                Debug.Log("[FountainRestoration] ✅ COMPLETE!");
            }
        }

        public bool IsRestored() => isRestored;
    }
}
