using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Emergency runtime fix: creates missing spawners if they don't exist in scene.
    /// Run via Unity menu: Tartaria > FIX: Add Missing Spawners
    /// </summary>
    public static class EmergencySpawnerFix
    {
        [UnityEditor.MenuItem("Tartaria/FIX: Add Missing Spawners")]
        static void AddMissingSpawners()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[EmergencyFix] Must be in Play mode to add runtime spawners!");
                return;
            }

            int added = 0;

            // Find managers parent
            var managers = GameObject.Find("--- GAME MANAGERS ---");
            if (managers == null)
            {
                managers = new GameObject("--- GAME MANAGERS ---");
                added++;
            }

            // Add BuildingSpawner if missing
            var buildingSpawner = Object.FindFirstObjectByType<BuildingSpawner>();
            if (buildingSpawner == null)
            {
                var go = new GameObject("BuildingSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<BuildingSpawner>();
                Debug.Log("[EmergencyFix] Created BuildingSpawner");
                added++;
            }
            else
            {
                Debug.Log("[EmergencyFix] BuildingSpawner already exists");
            }

            // Add EchohavenContentSpawner if missing
            var contentSpawner = Object.FindFirstObjectByType<EchohavenContentSpawner>();
            if (contentSpawner == null)
            {
                var go = new GameObject("EchohavenContentSpawner");
                go.transform.SetParent(managers.transform);
                go.AddComponent<EchohavenContentSpawner>();
                Debug.Log("[EmergencyFix] Created EchohavenContentSpawner");
                added++;
            }
            else
            {
                Debug.Log("[EmergencyFix] EchohavenContentSpawner already exists");
            }

            if (added > 0)
            {
                Debug.LogWarning($"[EmergencyFix] Added {added} missing components. Spawners will initialize on their next Start() call.");
                Debug.LogWarning("[EmergencyFix] EXIT PLAY MODE and run menu: Tartaria > Populate Echohaven Scene");
            }
            else
            {
                Debug.Log("[EmergencyFix] All spawners present. If content not spawning, check Console for errors in Start() methods.");
            }
        }
    }
}
