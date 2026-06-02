using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Pipe organ ambient drone for the Moon 1 cathedral area.
    /// Auto-attaches at runtime to any GameObject tagged "PipeOrgan" and plays a
    /// looping spatial-blended drone. On <see cref="GameEvents.OnBuildingRestored"/>
    /// with a buildingId containing "cathedral", the pitch ramps up to
    /// <see cref="pitchOnRestore"/> over 2 seconds as the audio sting for restoration.
    ///
    /// Per CLAUDE.md no-debt mandate rules 3 and 4: no silent fallbacks, no silent
    /// catches. All missing-asset paths log loud with the clip name and Resources
    /// path that was tried. Subscribers are removed on destroy to avoid leaks.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class Moon1PipeOrganController : MonoBehaviour
    {
        [Tooltip("Looping pipe-organ drone clip. If null at Awake, falls back to Resources/Audio/SFX/pipe_organ_drone (logs warning).")]
        [SerializeField] AudioClip droneClip;

        [Tooltip("Resting drone volume (0..1). Linear-rolloff spatial source.")]
        [SerializeField] float baseVolume = 0.4f;

        [Tooltip("Linear-rolloff inner radius — full volume within this distance (meters).")]
        [SerializeField] float baseMinDistance = 5f;

        [Tooltip("Linear-rolloff outer radius — silence beyond this distance (meters).")]
        [SerializeField] float baseMaxDistance = 50f;

        [Tooltip("Target pitch multiplier after the cathedral is restored.")]
        [SerializeField] float pitchOnRestore = 1.25f;

        const string FallbackResourcePath = "Audio/SFX/pipe_organ_drone";
        const float RestorePitchRampSeconds = 2f;

        AudioSource _src;
        bool _restored;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoAttach()
        {
            // Tag may not be defined in TagManager; fall through cleanly with a loud log
            // rather than throwing UnityException("Tag: PipeOrgan is not defined.").
            GameObject[] tagged;
            try
            {
                tagged = GameObject.FindGameObjectsWithTag("PipeOrgan");
            }
            catch (UnityException ex)
            {
                Debug.LogWarning($"[Moon1PipeOrganController] Tag 'PipeOrgan' is not defined in TagManager — skipping auto-attach. ({ex.Message}) Add the tag via Project Settings → Tags and Layers.");
                return;
            }

            if (tagged == null || tagged.Length == 0)
            {
                // Quiet info — no organs in this scene is a valid state for non-Moon-1 scenes.
                return;
            }

            int attached = 0;
            for (int i = 0; i < tagged.Length; i++)
            {
                var go = tagged[i];
                if (go == null) continue;
                if (go.GetComponent<Moon1PipeOrganController>() == null)
                {
                    go.AddComponent<Moon1PipeOrganController>();
                    attached++;
                    Debug.Log($"[Moon1PipeOrganController] Auto-attached to '{GetHierarchyPath(go)}'");
                }
            }

            if (attached == 0)
            {
                Debug.Log($"[Moon1PipeOrganController] Found {tagged.Length} 'PipeOrgan'-tagged object(s) but all already had the controller — nothing to do.");
            }
        }

        void Awake()
        {
            _src = GetComponent<AudioSource>();
            if (_src == null)
            {
                // RequireComponent should guarantee this, but log loud if Unity ever lies to us.
                Debug.LogError($"[Moon1PipeOrganController] AudioSource missing on '{GetHierarchyPath(gameObject)}' despite [RequireComponent] — drone disabled.");
                enabled = false;
                return;
            }

            _src.loop = true;
            _src.playOnAwake = false;
            _src.spatialBlend = 1f; // fully 3D
            _src.rolloffMode = AudioRolloffMode.Linear;
            _src.minDistance = baseMinDistance;
            _src.maxDistance = baseMaxDistance;
            _src.volume = baseVolume;
            _src.pitch = 1f;

            if (droneClip == null)
            {
                // Rule 4: explicit warning with id and the path tried before silent-default.
                droneClip = Resources.Load<AudioClip>(FallbackResourcePath);
                if (droneClip == null)
                {
                    Debug.LogWarning(
                        $"[Moon1PipeOrganController] No droneClip serialized on '{GetHierarchyPath(gameObject)}' " +
                        $"and Resources fallback at 'Resources/{FallbackResourcePath}' not found — drone will remain silent. " +
                        $"Drop a wav at Assets/_Project/Audio/SFX/pipe_organ_drone.wav (and mark its parent as a Resources folder, or assign it via the inspector). " +
                        $"Run Tartaria → 3 Tier → Tier 3 Procedural Audio to generate placeholders.");
                    // We still subscribe to OnBuildingRestored so that when a clip is later
                    // hot-loaded by a developer (or Resources are re-baked), the pitch ramp
                    // wiring still works. The actual Play() is gated below.
                }
                else
                {
                    Debug.LogWarning(
                        $"[Moon1PipeOrganController] droneClip not serialized on '{GetHierarchyPath(gameObject)}' — " +
                        $"loaded fallback clip '{droneClip.name}' from 'Resources/{FallbackResourcePath}'.");
                }
            }

            if (droneClip != null)
            {
                _src.clip = droneClip;
                _src.Play();
            }

            GameEvents.OnBuildingRestored += HandleBuildingRestored;
        }

        void OnDestroy()
        {
            // Always unsubscribe; subscribing+unsubscribing are symmetric even if Awake exited early
            // because we register OnBuildingRestored unconditionally above (drone is silent but the
            // pitch state still tracks restoration).
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogWarning($"[Moon1PipeOrganController] OnBuildingRestored fired with empty/null buildingId — ignored on '{GetHierarchyPath(gameObject)}'.");
                return;
            }

            if (!buildingId.ToLowerInvariant().Contains("cathedral")) return;
            if (_restored)
            {
                Debug.Log($"[Moon1PipeOrganController] Cathedral restore event '{buildingId}' received again on '{gameObject.name}' — already at restored pitch, skipping.");
                return;
            }

            _restored = true;

            if (_src == null || _src.clip == null)
            {
                Debug.LogWarning($"[Moon1PipeOrganController] Cathedral restored ('{buildingId}') but no clip loaded on '{GetHierarchyPath(gameObject)}' — pitch ramp skipped, drone silent.");
                return;
            }

            StartCoroutine(PitchShift());
            Debug.Log($"[Moon1PipeOrganController] Cathedral restored ('{buildingId}') — pitch ramping from 1.0 → {pitchOnRestore} over {RestorePitchRampSeconds}s on '{gameObject.name}'.");
        }

        IEnumerator PitchShift()
        {
            float t = 0f;
            float startPitch = _src.pitch;
            while (t < RestorePitchRampSeconds)
            {
                t += Time.deltaTime;
                if (_src == null) yield break;
                _src.pitch = Mathf.Lerp(startPitch, pitchOnRestore, t / RestorePitchRampSeconds);
                yield return null;
            }
            if (_src != null) _src.pitch = pitchOnRestore;
        }

        static string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "<null>";
            var t = go.transform;
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
