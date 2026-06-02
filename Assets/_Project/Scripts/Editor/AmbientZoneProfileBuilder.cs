// AmbientZoneProfileBuilder.cs
// Sprint 6 Lane 4 — Editor menu to scaffold the 5 Moon 1 ambient zone profile assets.
//
// Menu: Tartaria/Audio/Build Ambient Zone Profiles
//
// Creates one AmbientZoneProfile ScriptableObject per Moon 1 zone if the .asset
// does not already exist on disk. Idempotent. Existing assets are NEVER overwritten —
// the menu logs "exists, skipped" for those and continues. Designers can safely
// re-run after wiring AudioClip references manually.
//
// Each profile is created with the canonical zoneId, the expected resource paths
// (so the runtime warning log points the designer at the right .wav location),
// and the cymatic tie-in flag set correctly for the Grotto profile.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Tartaria.Audio;

namespace Tartaria.Editor.Audio
{
    public static class AmbientZoneProfileBuilder
    {
        const string PROFILE_DIR = "Assets/_Project/Data/Audio/Ambient";

        struct Spec
        {
            public string fileName;
            public string zoneId;
            public string description;
            public string primaryExpectedPath;
            public float  primaryVolume;
            public string secondaryExpectedPath;
            public float  secondaryVolume;
            public bool   secondaryRequired;
            public bool   activateTelluricOnEnter;
        }

        static readonly Spec[] s_specs = new Spec[]
        {
            new Spec
            {
                fileName = "VillageCenter.asset",
                zoneId = "VillageCenter",
                description = "Echohaven village square ambient — low wind bed + distant villager chatter.",
                primaryExpectedPath = "Assets/_Project/Audio/Ambient/wind_low_loop.wav",
                primaryVolume = 0.55f,
                secondaryExpectedPath = "Assets/_Project/Audio/Ambient/village_chatter_distant_loop.wav",
                secondaryVolume = 0.30f,
                secondaryRequired = true,
                activateTelluricOnEnter = false,
            },
            new Spec
            {
                fileName = "MudPools.asset",
                zoneId = "MudPools",
                description = "Stagnant mud pools northwest of Echohaven — gurgling bubbles + buzzing flies.",
                primaryExpectedPath = "Assets/_Project/Audio/Ambient/mud_gurgle_loop.wav",
                primaryVolume = 0.50f,
                secondaryExpectedPath = "Assets/_Project/Audio/Ambient/flies_buzz_loop.wav",
                secondaryVolume = 0.35f,
                secondaryRequired = true,
                activateTelluricOnEnter = false,
            },
            new Spec
            {
                fileName = "CathedralInterior.asset",
                zoneId = "CathedralInterior",
                description = "Reverberant cathedral interior — echoed footstep tail + sustained pipe-organ pedal drone.",
                primaryExpectedPath = "Assets/_Project/Audio/Ambient/cathedral_reverb_tail_loop.wav",
                primaryVolume = 0.45f,
                secondaryExpectedPath = "Assets/_Project/Audio/Ambient/pipe_organ_pedal_drone_loop.wav",
                secondaryVolume = 0.40f,
                secondaryRequired = true,
                activateTelluricOnEnter = false,
            },
            new Spec
            {
                fileName = "ForestEdge.asset",
                zoneId = "ForestEdge",
                description = "Edge-of-forest fringe — songbird ambient + rustling leaves.",
                primaryExpectedPath = "Assets/_Project/Audio/Ambient/forest_birds_loop.wav",
                primaryVolume = 0.50f,
                secondaryExpectedPath = "Assets/_Project/Audio/Ambient/leaves_rustle_loop.wav",
                secondaryVolume = 0.32f,
                secondaryRequired = true,
                activateTelluricOnEnter = false,
            },
            new Spec
            {
                fileName = "Grotto.asset",
                zoneId = "Grotto",
                description = "Hidden grotto behind the Spire — cave water drips + low Telluric 7.83Hz Schumann drone. Activates CymaticMusicEngine Telluric band on enter.",
                primaryExpectedPath = "Assets/_Project/Audio/Ambient/cave_drips_loop.wav",
                primaryVolume = 0.55f,
                secondaryExpectedPath = "Assets/_Project/Audio/Ambient/schumann_7p83_low_drone_loop.wav",
                secondaryVolume = 0.45f,
                secondaryRequired = true,
                activateTelluricOnEnter = true,
            },
        };

        [MenuItem("Tartaria/Audio/Build Ambient Zone Profiles")]
        public static void BuildProfiles()
        {
            EnsureDirectory(PROFILE_DIR);

            var created = new List<string>(s_specs.Length);
            var skipped = new List<string>(s_specs.Length);

            foreach (var spec in s_specs)
            {
                string assetPath = $"{PROFILE_DIR}/{spec.fileName}";
                var existing = AssetDatabase.LoadAssetAtPath<AmbientZoneProfile>(assetPath);
                if (existing != null)
                {
                    skipped.Add(spec.zoneId);
                    Debug.Log($"[AmbientZone/Builder] '{spec.zoneId}' already exists at '{assetPath}' — skipped (existing asset preserved).");
                    continue;
                }

                var profile = ScriptableObject.CreateInstance<AmbientZoneProfile>();
                profile.zoneId = spec.zoneId;
                profile.description = spec.description;
                profile.primaryClip = null; // designer wires this manually
                profile.primaryExpectedPath = spec.primaryExpectedPath;
                profile.primaryVolume = spec.primaryVolume;
                profile.secondaryClip = null;
                profile.secondaryExpectedPath = spec.secondaryExpectedPath;
                profile.secondaryVolume = spec.secondaryVolume;
                profile.secondaryRequired = spec.secondaryRequired;
                profile.activateTelluricOnEnter = spec.activateTelluricOnEnter;
                profile.crossfadeSeconds = 2.0f;

                AssetDatabase.CreateAsset(profile, assetPath);
                created.Add(spec.zoneId);
                Debug.Log($"[AmbientZone/Builder] Created '{spec.zoneId}' at '{assetPath}'. Wire primaryClip='{spec.primaryExpectedPath}' and secondaryClip='{spec.secondaryExpectedPath}' in the inspector.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AmbientZone/Builder] Done. Created={created.Count} ({string.Join(", ", created)}), Skipped={skipped.Count} ({string.Join(", ", skipped)}).");
            EditorUtility.DisplayDialog(
                "Ambient Zone Profiles",
                $"Created: {created.Count}\nSkipped (already existed): {skipped.Count}\n\nPath: {PROFILE_DIR}\n\nDesigners: assign AudioClip refs in the inspector. Expected paths are listed per profile and will be surfaced in runtime warning logs if a clip is left null.",
                "OK");
        }

        static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            // Walk the path segments and create folders as needed.
            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, parts[i]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogError($"[AmbientZone/Builder] Failed to create folder '{next}'. Check filesystem permissions.");
                        return;
                    }
                }
                current = next;
            }
        }
    }
}
