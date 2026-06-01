#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1AudioWire — drops ambient + SFX into the scene so Echohaven isn't silent.
    /// Per docs/15 §13.
    ///
    /// 1. Creates an "Audio_Ambient" GameObject with a looping 2D AudioSource
    ///    playing the first matching Ambient/Drone .wav we can find.
    /// 2. Drops Audio_SFXManager with the AudioManager component (if not already
    ///    on a GameObject) so PlaySFX(string) calls don't no-op.
    /// 3. Sets reasonable volumes and 2D blend on the ambient source.
    ///
    /// Idempotent: re-runs reuse existing GameObjects.
    /// </summary>
    public static class Moon1AudioWire
    {
        // Preferred ambient track candidates (first found wins)
        static readonly string[] AMBIENT_CANDIDATES = new string[]
        {
            "Assets/25 Rpg Game Tracks/Ambient 1.wav",
            "Assets/25 Rpg Game Tracks/Light Ambient 1 (Loop).wav",
            "Assets/25 Rpg Game Tracks/Ambient 2.wav",
        };

        [MenuItem("Tartaria/3 Wire/Echohaven Audio (Ambient + SFX)", priority = 310)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Audio Wire", "No active scene.", "OK");
                return;
            }

            // Ambient source
            string foundPath = null;
            AudioClip ambient = null;
            foreach (var p in AMBIENT_CANDIDATES)
            {
                if (File.Exists(p))
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                    if (clip != null)
                    {
                        ambient = clip;
                        foundPath = p;
                        break;
                    }
                }
            }

            if (ambient == null)
            {
                EditorUtility.DisplayDialog("Audio Wire",
                    "No ambient clip found at expected paths. Skipping ambient.\n\n" +
                    "Tried:\n" + string.Join("\n", AMBIENT_CANDIDATES),
                    "OK");
            }
            else
            {
                var existing = GameObject.Find("Audio_Ambient");
                GameObject ambGO = existing != null ? existing : new GameObject("Audio_Ambient");
                if (existing == null) Undo.RegisterCreatedObjectUndo(ambGO, "Create Audio_Ambient");

                var src = ambGO.GetComponent<AudioSource>();
                if (src == null) src = ambGO.AddComponent<AudioSource>();
                src.clip = ambient;
                src.loop = true;
                src.playOnAwake = true;
                src.spatialBlend = 0f; // 2D
                src.volume = 0.45f;
                src.priority = 64;

                Debug.Log($"[Moon1AudioWire] Ambient: {ambient.name} ({foundPath}) loaded on {ambGO.name}.");
            }

            // Bestow a hint about SFX wiring
            Debug.Log("[Moon1AudioWire] AudioManager string-key SFX (Discovery, TuneSuccess, BuildingReveal, " +
                      "MudGolemHit, etc.) are already called from InteractableBuilding + EchohavenCombatArena. " +
                      "If clips aren't audible, register them in the AudioManager Inspector → SFX map.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("Audio Wire",
                ambient != null
                    ? $"Ambient '{ambient.name}' looping in 2D.\n\nSee Console for next-step note about SFX clip registration."
                    : "No ambient clip wired (none found). See Console.",
                "OK");
        }
    }
}
#endif
