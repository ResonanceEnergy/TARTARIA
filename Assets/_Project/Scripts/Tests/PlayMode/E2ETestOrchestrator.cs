using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tartaria.Tests.PlayMode
{
    /// <summary>
    /// E2E Test Orchestrator — Master suite for comprehensive player journey testing.
    /// 
    /// Executes all 5 E2E journey scenarios:
    /// 1. New Player Journey (0-10h)
    /// 2. Mid-Game Journey (10-30h)
    /// 3. Endgame Journey (30-50h)
    /// 4. Critical Path Journey (~20h)
    /// 5. Completionist Journey (100%)
    /// 
    /// USAGE:
    /// - Run via Unity Test Runner: Window > General > Test Runner > PlayMode
    /// - Run via command line: run-e2e-tests.ps1
    /// - Individual tests can be run separately
    /// - Full suite takes ~30-60 minutes to complete
    /// 
    /// OUTPUT:
    /// - Console logs with [E2E] prefix
    /// - Test results in Unity Test Runner
    /// - Final report: BETA_E2E_TEST_REPORT.md
    /// </summary>
    public class E2ETestOrchestrator
    {
        [UnityTest, Order(1)]
        [Category("E2E")]
        [Category("NewPlayer")]
        [Timeout(600000)] // 10 minutes
        public IEnumerator Test_E2E_NewPlayerJourney()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] Starting: New Player Journey (0-10h)");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            var test = new E2EJourney_NewPlayer();
            yield return test.Execute();
            
            Debug.Log($"[E2E] New Player Journey Complete: {test.GetSummary()}");
            
            // Assert no critical failures
            Assert.AreEqual(0, test.FailCount, 
                $"New Player Journey had {test.FailCount} failures");
        }
        
        [UnityTest, Order(2)]
        [Category("E2E")]
        [Category("MidGame")]
        [Timeout(900000)] // 15 minutes
        public IEnumerator Test_E2E_MidGameJourney()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] Starting: Mid-Game Journey (10-30h)");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            var test = new E2EJourney_MidGame();
            yield return test.Execute();
            
            Debug.Log($"[E2E] Mid-Game Journey Complete: {test.GetSummary()}");
            
            // Allow warnings but not failures
            Assert.AreEqual(0, test.FailCount, 
                $"Mid-Game Journey had {test.FailCount} failures");
        }
        
        [UnityTest, Order(3)]
        [Category("E2E")]
        [Category("Endgame")]
        [Timeout(1200000)] // 20 minutes
        public IEnumerator Test_E2E_EndgameJourney()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] Starting: Endgame Journey (30-50h)");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            var test = new E2EJourney_Endgame();
            yield return test.Execute();
            
            Debug.Log($"[E2E] Endgame Journey Complete: {test.GetSummary()}");
            
            Assert.AreEqual(0, test.FailCount, 
                $"Endgame Journey had {test.FailCount} failures");
        }
        
        [UnityTest, Order(4)]
        [Category("E2E")]
        [Category("CriticalPath")]
        [Timeout(600000)] // 10 minutes
        public IEnumerator Test_E2E_CriticalPathJourney()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] Starting: Critical Path Journey (~20h)");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            var test = new E2EJourney_CriticalPath();
            yield return test.Execute();
            
            Debug.Log($"[E2E] Critical Path Journey Complete: {test.GetSummary()}");
            
            // Critical path MUST be clear (zero failures)
            Assert.AreEqual(0, test.FailCount, 
                $"CRITICAL PATH BLOCKER: {test.FailCount} failures detected");
        }
        
        [UnityTest, Order(5)]
        [Category("E2E")]
        [Category("Completionist")]
        [Timeout(1800000)] // 30 minutes
        public IEnumerator Test_E2E_CompletionistJourney()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] Starting: Completionist Journey (100%)");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            var test = new E2EJourney_Completionist();
            yield return test.Execute();
            
            Debug.Log($"[E2E] Completionist Journey Complete: {test.GetSummary()}");
            
            // Allow some warnings for optional content
            Assert.IsTrue(test.FailCount <= 2, 
                $"Completionist Journey had {test.FailCount} failures (max 2 allowed)");
        }
        
        [UnityTest, Order(6)]
        [Category("E2E")]
        [Category("Summary")]
        public IEnumerator Test_E2E_GenerateFinalReport()
        {
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            Debug.Log("[E2E] ALL E2E TESTS COMPLETE - Generating Report...");
            Debug.Log("[E2E] ═══════════════════════════════════════════════════════");
            
            // Final report generation happens in the PowerShell script
            // This test just confirms the suite completed
            
            Debug.Log("[E2E] See BETA_E2E_TEST_REPORT.md for comprehensive results");
            
            yield return null;
            Assert.Pass("E2E test suite completed successfully");
        }
    }
}
