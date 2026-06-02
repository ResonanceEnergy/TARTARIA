// AmbientZoneProfile.cs
// Sprint 6 Lane 4 — Per-zone ambient audio profile.
//
// ScriptableObject that defines the looping ambient mix for one named zone.
// Two layered AudioClips (primary + secondary) blend at independent volumes
// while the player is inside the zone. AmbientZoneController consumes these
// profiles and cross-fades between them as the player walks through triggers.
//
// Asset path convention: Assets/_Project/Data/Audio/Ambient/<ZoneId>.asset
//
// Designed to coexist with:
//   * AdaptiveMusicController  (MUSIC bus, RS-reactive, OnBuildingRestored stinger)
//   * CymaticMusicEngine       (3-band cymatic drones, OnBuildingRestored/OnMoonCompleted)
//   * AmbienceZone             (legacy, single-clip per-collider, static singleton state)
//   * AmbientAudioZone         (legacy, AudioManager string-lookup)
//
// This controller runs on the AMBIENT bus. It does NOT subscribe to GameEvents
// and does NOT touch the music sources owned by AdaptiveMusicController /
// CymaticMusicEngine.
using UnityEngine;
using UnityEngine.Audio;

namespace Tartaria.Audio
{
    /// <summary>
    /// Looping ambient mix for a single Moon 1 zone (e.g. VillageCenter, MudPools,
    /// CathedralInterior, ForestEdge, Grotto). Cross-faded by
    /// <see cref="AmbientZoneController"/> on player enter/exit.
    /// </summary>
    [CreateAssetMenu(menuName = "Tartaria/Audio/Ambient Zone Profile",
                     fileName = "AmbientZoneProfile")]
    public class AmbientZoneProfile : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable zone identifier — appears in logs and is used to dedupe overlapping triggers.")]
        public string zoneId = "Unnamed";

        [Tooltip("Human-readable description for designers. Not used at runtime.")]
        [TextArea(1, 3)]
        public string description;

        [Header("Primary Layer (dominant loop — wind, drips, etc.)")]
        [Tooltip("Looping ambient clip for the primary layer. If null, the controller logs a warning and falls back to silence on this layer.")]
        public AudioClip primaryClip;

        [Tooltip("Expected Resources / Addressables path used for the warning log when primaryClip is null. Helps designers find the missing source.")]
        public string primaryExpectedPath = "Assets/_Project/Audio/Ambient/<clip>.wav";

        [Range(0f, 1f)]
        [Tooltip("Target volume for the primary layer once cross-fade completes.")]
        public float primaryVolume = 0.55f;

        [Header("Secondary Layer (sweetener — flies, distant chatter, etc.)")]
        [Tooltip("Optional second looping layer mixed under the primary. Null = no secondary layer for this profile.")]
        public AudioClip secondaryClip;

        [Tooltip("Expected Resources / Addressables path used for the warning log when secondaryClip is null AND a secondary layer was intended.")]
        public string secondaryExpectedPath = "Assets/_Project/Audio/Ambient/<clip>.wav";

        [Range(0f, 1f)]
        [Tooltip("Target volume for the secondary layer once cross-fade completes.")]
        public float secondaryVolume = 0.35f;

        [Tooltip("If true and secondaryClip is null at load time, the controller logs a warning (per CLAUDE.md no-silent-fallback rule). If false, missing secondary is intentional.")]
        public bool secondaryRequired = false;

        [Header("Routing (optional)")]
        [Tooltip("Optional AudioMixerGroup → Ambience bus. Leave null to rely on AudioManager.AmbienceGroup at controller wire-time.")]
        public AudioMixerGroup mixerGroupOverride;

        [Header("Cymatic Tie-in (Grotto / Telluric)")]
        [Tooltip("If true, the controller pings CymaticMusicEngine.DebugActivateTelluric() when the player enters this zone. Used by Grotto to bring the 7.83Hz drone forward without invoking the OnBuildingRestored event path.")]
        public bool activateTelluricOnEnter = false;

        [Header("Cross-fade")]
        [Tooltip("Seconds for ambient layer A→B cross-fade on enter / fade-out on exit. 2.0s matches AdaptiveMusicController.ZONE_CROSSFADE_SECONDS.")]
        [Min(0.05f)]
        public float crossfadeSeconds = 2.0f;

        /// <summary>
        /// True when this profile has no primary clip and no secondary clip — i.e. it would
        /// silently do nothing. Controller logs a loud warning when this is the case.
        /// </summary>
        public bool IsCompletelyEmpty => primaryClip == null && secondaryClip == null;
    }
}
