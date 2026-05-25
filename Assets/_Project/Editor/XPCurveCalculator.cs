using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// XP Curve Calculator — generates full level 1-50 XP progression table.
    /// Menu: Tartaria > Debug > Calculate XP Curve
    /// Use this to validate progression tuning after changing xpExponent.
    /// </summary>
    public static class XPCurveCalculator
    {
        [UnityEditor.MenuItem("Tartaria/Debug/Calculate XP Curve")]
        public static void Calculate()
        {
            const int maxLevel = 50;
            const int baseXP = 100;
            const float exponentCurrent = 1.5f;
            const float exponentProposed = 1.15f;

            Debug.Log("=== TARTARIA XP CURVE ANALYSIS ===\n");
            Debug.Log("CURRENT (Exponential 1.5) vs PROPOSED (Exponential 1.15)\n");
            Debug.Log("Level | XP (Current) | Cumulative | XP (Proposed) | Cumulative | Hours@500/hr (Current) | Hours@500/hr (Proposed)");
            Debug.Log("------|--------------|------------|---------------|------------|----------------------|----------------------");

            float totalXPCurrent = 0;
            float totalXPProposed = 0;

            for (int level = 1; level <= maxLevel; level++)
            {
                // Current formula: 100 * level^1.5
                int xpCurrent = Mathf.RoundToInt(baseXP * Mathf.Pow(level, exponentCurrent));
                totalXPCurrent += xpCurrent;

                // Proposed formula: 100 * level^1.15
                int xpProposed = Mathf.RoundToInt(baseXP * Mathf.Pow(level, exponentProposed));
                totalXPProposed += xpProposed;

                // Calculate hours at 500 XP/hr
                float hoursCurrent = totalXPCurrent / 500f;
                float hoursProposed = totalXPProposed / 500f;

                // Log every 5 levels + milestones
                if (level % 5 == 0 || level == 1 || level == maxLevel)
                {
                    Debug.Log($"{level,5} | {xpCurrent,12:N0} | {totalXPCurrent,10:N0} | {xpProposed,13:N0} | {totalXPProposed,10:N0} | {hoursCurrent,20:F1} | {hoursProposed,20:F1}");
                }
            }

            Debug.Log("\n=== TOTALS ===");
            Debug.Log($"Total XP to Level 50 (Current):  {totalXPCurrent:N0} XP ({totalXPCurrent / 500f:F1} hours @ 500 XP/hr)");
            Debug.Log($"Total XP to Level 50 (Proposed): {totalXPProposed:N0} XP ({totalXPProposed / 500f:F1} hours @ 500 XP/hr)");
            Debug.Log($"\nReduction: {((totalXPCurrent - totalXPProposed) / totalXPCurrent * 100):F1}%");
            Debug.Log($"\nDoc Target: ~80,000 XP total");
            Debug.Log($"Current Gap: {((totalXPCurrent - 80000) / 80000 * 100):F1}% over target");
            Debug.Log($"Proposed Gap: {((totalXPProposed - 80000) / 80000 * 100):F1}% over target");

            // Analyze dead zones
            Debug.Log("\n=== DEAD ZONE ANALYSIS ===");
            Debug.Log("Level ranges where XP/level exceeds 2× baseline (levels 1-10 avg):");
            
            float baselineXP = 0;
            for (int i = 1; i <= 10; i++)
            {
                baselineXP += Mathf.RoundToInt(baseXP * Mathf.Pow(i, exponentCurrent));
            }
            baselineXP /= 10;

            Debug.Log($"Baseline (Levels 1-10 avg): {baselineXP:F0} XP/level");
            Debug.Log("\nCurrent (1.5 exponent):");
            for (int level = 10; level <= maxLevel; level += 5)
            {
                float xp = Mathf.RoundToInt(baseXP * Mathf.Pow(level, exponentCurrent));
                float multiplier = xp / baselineXP;
                string severity = multiplier < 3 ? "⚠️" : multiplier < 6 ? "❌" : "🚫";
                Debug.Log($"  Level {level}: {xp:F0} XP ({multiplier:F1}× baseline) {severity}");
            }

            Debug.Log("\nProposed (1.15 exponent):");
            baselineXP = 0;
            for (int i = 1; i <= 10; i++)
            {
                baselineXP += Mathf.RoundToInt(baseXP * Mathf.Pow(i, exponentProposed));
            }
            baselineXP /= 10;
            Debug.Log($"Baseline (Levels 1-10 avg): {baselineXP:F0} XP/level");
            
            for (int level = 10; level <= maxLevel; level += 5)
            {
                float xp = Mathf.RoundToInt(baseXP * Mathf.Pow(level, exponentProposed));
                float multiplier = xp / baselineXP;
                string severity = multiplier < 2 ? "✅" : multiplier < 3 ? "⚠️" : "❌";
                Debug.Log($"  Level {level}: {xp:F0} XP ({multiplier:F1}× baseline) {severity}");
            }

            Debug.Log("\n=== CALCULATION COMPLETE ===");
        }
    }
}
