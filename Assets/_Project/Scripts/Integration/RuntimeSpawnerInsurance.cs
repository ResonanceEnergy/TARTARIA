using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime spawner insurance - ensures BuildingSpawner and EchohavenContentSpawner
    /// exist in Echohaven scene even if missing from scene file.
    /// Auto-runs on scene load.
    /// </summary>
    public static class RuntimeSpawnerInsurance
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureSpawners()
        {
            // Only run in Echohaven scene
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneName.Contains("Echohaven")) return;

            var managers = GameObject.Find("--- GAME MANAGERS ---");
            if (managers == null)
            {
                managers = new GameObject("--- GAME MANAGERS ---");
                Debug.Log("[RuntimeInsurance] Created GAME MANAGERS parent");
            }

            // 2026-06-02 ROOT-CAUSE FIX: previous session left these blocks decapitated —
            // the "// SUPERSEDED — orphan go ref" comments killed AddComponent so the warnings
            // fired loud but no actual spawner ran. Result: no Player instantiated, no
            // CharacterController in scene, movement code had nothing to drive. Restoring the
            // canonical pipeline per CLAUDE.md no-debt rule 1 + rule 12.

            // BuildingSpawner check
            if (Object.FindFirstObjectByType<BuildingSpawner>() == null)
            {
                var go = new GameObject("BuildingSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<BuildingSpawner>();
                Debug.LogWarning("[RuntimeInsurance] BuildingSpawner was missing from scene — instantiated + attached at runtime.");
            }

            // PlayerSpawner check — THE movement-blocker fix.
            if (Object.FindFirstObjectByType<PlayerSpawner>() == null)
            {
                var go = new GameObject("PlayerSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<PlayerSpawner>();
                Debug.LogWarning("[RuntimeInsurance] PlayerSpawner was missing from scene — instantiated + attached at runtime. Player prefab will spawn next.");
            }
        }
    }
}
