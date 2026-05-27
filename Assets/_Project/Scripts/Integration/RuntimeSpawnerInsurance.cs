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

            // BuildingSpawner check
            if (Object.FindFirstObjectByType<BuildingSpawner>() == null)
            {
                var go = new GameObject("BuildingSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<BuildingSpawner>();
                Debug.LogWarning("[RuntimeInsurance] BuildingSpawner was missing from scene - created at runtime");
            }

            // EchohavenContentSpawner check
            if (Object.FindFirstObjectByType<EchohavenContentSpawner>() == null)
            {
                var go = new GameObject("EchohavenContentSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<EchohavenContentSpawner>();
                Debug.LogWarning("[RuntimeInsurance] EchohavenContentSpawner was missing from scene - created at runtime");
            }

            // PlayerSpawner check
            if (Object.FindFirstObjectByType<PlayerSpawner>() == null)
            {
                var go = new GameObject("PlayerSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<PlayerSpawner>();
                Debug.LogWarning("[RuntimeInsurance] PlayerSpawner was missing from scene - created at runtime");
            }
        }
    }
}
