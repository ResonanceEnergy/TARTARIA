using UnityEngine;
using System.Collections;

namespace Tartaria.Testing
{
    /// <summary>
    /// StabilityAgent - Agent 4: Long session stability (24-hour soak test).
    /// </summary>
    public class StabilityAgent : MonoBehaviour
    {
        [SerializeField] private bool isRunning = false;
        [SerializeField] private float sessionTime = 0f;

        public void StartSoakTest()
        {
            if (!isRunning)
                StartCoroutine(SoakTestSequence());
        }

        IEnumerator SoakTestSequence()
        {
            isRunning = true;
            sessionTime = 0f;
            float duration = 86400f; // 24 hours

            Debug.Log("[StabilityAgent] Starting 24-hour soak test...");

            while (sessionTime < duration)
            {
                sessionTime += Time.deltaTime;
                
                if ((int)sessionTime % 3600 == 0) // Every hour
                {
                    Debug.Log($"[StabilityAgent] {sessionTime / 3600f:F1} hours elapsed");
                }

                yield return null;
            }

            isRunning = false;
            Debug.Log("[StabilityAgent] ✅ 24-hour soak test COMPLETE!");
        }
    }
}
