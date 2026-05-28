using UnityEngine;
using System.Collections;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// DomeRestorationSystem - Star Dome (celestial chamber) restoration.
    /// </summary>
    public class DomeRestorationSystem : MonoBehaviour
    {
        [SerializeField] private string buildingId = "dome";
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
                GameLoopController.Instance?.AwardRS(50f, "Dome Restoration");
                Debug.Log("[DomeRestoration] ✅ COMPLETE!");
            }
        }

        public bool IsRestored() => isRestored;
    }
}
